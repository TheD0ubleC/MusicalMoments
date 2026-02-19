package store

import (
	"context"
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"reflect"
	"sort"
	"strings"
	"sync"
	"time"

	"musicalmoments/web/internal/models"
)

type RequestMetrics struct {
	Route         string
	BytesServed   uint64
	IsAPI         bool
	IsPageView    bool
	IsDownload    bool
	IsAPIDownload bool
}

type Store struct {
	mu    sync.RWMutex
	path  string
	data  models.DataFile
	dirty bool
}

const (
	usageSessionTTL   = 3 * time.Minute
	maxLoginAuditLogs = 500
)

func New(path string) (*Store, error) {
	if strings.TrimSpace(path) == "" {
		return nil, errors.New("store path cannot be empty")
	}

	s := &Store{
		path: path,
		data: defaultData(),
	}

	if err := s.loadOrCreate(); err != nil {
		return nil, err
	}

	return s, nil
}

func (s *Store) StartAutoSave(ctx context.Context, interval time.Duration) {
	if interval < 5*time.Second {
		interval = 5 * time.Second
	}

	ticker := time.NewTicker(interval)
	go func() {
		defer ticker.Stop()
		for {
			select {
			case <-ctx.Done():
				_ = s.SaveIfDirty()
				return
			case <-ticker.C:
				_ = s.SaveIfDirty()
			}
		}
	}()
}

func (s *Store) SaveIfDirty() error {
	s.mu.Lock()
	defer s.mu.Unlock()
	return s.saveLocked(false)
}

func (s *Store) ForceSave() error {
	s.mu.Lock()
	defer s.mu.Unlock()
	return s.saveLocked(true)
}

func (s *Store) ListVersions() []models.Version {
	s.mu.RLock()
	defer s.mu.RUnlock()

	out := make([]models.Version, len(s.data.Versions))
	copy(out, s.data.Versions)
	sortVersionsDesc(out)
	return out
}

func (s *Store) ListPlugins() []models.Plugin {
	s.mu.RLock()
	defer s.mu.RUnlock()

	out := make([]models.Plugin, len(s.data.Plugins))
	for i := range s.data.Plugins {
		out[i] = clonePlugin(s.data.Plugins[i])
	}
	sort.Slice(out, func(i, j int) bool {
		return out[i].UpdatedAt.After(out[j].UpdatedAt)
	})
	return out
}

func (s *Store) GetLatestPointers() (stableID string, previewID string) {
	s.mu.RLock()
	defer s.mu.RUnlock()
	return s.data.LatestStableID, s.data.LatestPreviewID
}

func (s *Store) GetLatestVersion(track models.VersionTrack) (models.Version, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()
	return s.getLatestVersionLocked(track)
}

func (s *Store) FindVersion(id string) (models.Version, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()
	for _, version := range s.data.Versions {
		if version.ID == id {
			return version, true
		}
	}

	return models.Version{}, false
}

func (s *Store) FindPlugin(id string) (models.Plugin, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()
	for _, plugin := range s.data.Plugins {
		if plugin.ID == id {
			return clonePlugin(plugin), true
		}
	}

	return models.Plugin{}, false
}

func (s *Store) FindPluginVersion(id string) (models.Plugin, models.PluginVersion, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	for _, plugin := range s.data.Plugins {
		for _, version := range plugin.Versions {
			if version.ID == id {
				return clonePlugin(plugin), version, true
			}
		}
	}

	return models.Plugin{}, models.PluginVersion{}, false
}

func (s *Store) UpsertVersion(version models.Version) (models.Version, error) {
	if !models.IsValidTrack(version.Track) {
		return models.Version{}, errors.New("invalid version track")
	}

	now := time.Now().UTC()
	if version.PublishedAt.IsZero() {
		version.PublishedAt = now
	}
	version.UpdatedAt = now
	if strings.TrimSpace(version.ID) == "" {
		version.ID = newID("ver")
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	updated := false
	for i := range s.data.Versions {
		if s.data.Versions[i].ID == version.ID {
			s.data.Versions[i] = version
			updated = true
			break
		}
	}

	if !updated {
		s.data.Versions = append(s.data.Versions, version)
	}

	if version.Track == models.VersionTrackStable && s.data.LatestStableID == "" {
		s.data.LatestStableID = version.ID
	}
	if version.Track == models.VersionTrackPreview && s.data.LatestPreviewID == "" {
		s.data.LatestPreviewID = version.ID
	}

	s.touchDirtyLocked()
	return version, nil
}

func (s *Store) DeleteVersion(id string) bool {
	s.mu.Lock()
	defer s.mu.Unlock()

	oldLen := len(s.data.Versions)
	filtered := s.data.Versions[:0]
	for _, version := range s.data.Versions {
		if version.ID != id {
			filtered = append(filtered, version)
		}
	}
	s.data.Versions = filtered

	if len(s.data.Versions) == oldLen {
		return false
	}

	if s.data.LatestStableID == id {
		s.data.LatestStableID = s.pickLatestVersionIDLocked(models.VersionTrackStable)
	}
	if s.data.LatestPreviewID == id {
		s.data.LatestPreviewID = s.pickLatestVersionIDLocked(models.VersionTrackPreview)
	}

	s.touchDirtyLocked()
	return true
}

func (s *Store) SetLatestVersion(track models.VersionTrack, id string) error {
	if !models.IsValidTrack(track) {
		return errors.New("invalid version track")
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	found := false
	for _, version := range s.data.Versions {
		if version.ID == id {
			if version.Track != track {
				return errors.New("version track mismatch")
			}
			found = true
			break
		}
	}
	if !found {
		return errors.New("version not found")
	}

	if track == models.VersionTrackStable {
		s.data.LatestStableID = id
	} else {
		s.data.LatestPreviewID = id
	}

	s.touchDirtyLocked()
	return nil
}

func (s *Store) UpsertPlugin(plugin models.Plugin) (models.Plugin, error) {
	now := time.Now().UTC()
	if strings.TrimSpace(plugin.ID) == "" {
		plugin.ID = newID("plg")
		plugin.CreatedAt = now
	}
	plugin.Name = strings.TrimSpace(plugin.Name)
	plugin.Description = strings.TrimSpace(plugin.Description)
	plugin.Version = ""
	plugin.DownloadURL = ""
	plugin.SHA256 = ""

	s.mu.Lock()
	defer s.mu.Unlock()

	s.ensureMapsLocked()

	updated := false
	for i := range s.data.Plugins {
		if s.data.Plugins[i].ID == plugin.ID {
			existing := s.data.Plugins[i]
			plugin.CreatedAt = existing.CreatedAt
			plugin.UpdatedAt = now
			plugin.Versions = clonePluginVersions(existing.Versions)
			sortPluginVersionsDesc(plugin.Versions)
			s.data.Plugins[i] = plugin
			updated = true
			break
		}
	}

	if !updated {
		if plugin.CreatedAt.IsZero() {
			plugin.CreatedAt = now
		}
		plugin.UpdatedAt = now
		plugin.Versions = normalizePluginVersions(plugin.Versions, now)
		s.data.Plugins = append(s.data.Plugins, plugin)
	}

	s.touchDirtyLocked()
	return clonePlugin(plugin), nil
}

func (s *Store) UpsertPluginVersion(pluginID string, version models.PluginVersion) (models.Plugin, models.PluginVersion, error) {
	pluginID = strings.TrimSpace(pluginID)
	if pluginID == "" {
		return models.Plugin{}, models.PluginVersion{}, errors.New("plugin id is required")
	}

	now := time.Now().UTC()
	version.ID = strings.TrimSpace(version.ID)
	version.Version = strings.TrimSpace(version.Version)
	version.Changelog = strings.TrimSpace(version.Changelog)
	version.DownloadURL = strings.TrimSpace(version.DownloadURL)
	version.SHA256 = strings.ToLower(strings.TrimSpace(version.SHA256))
	if version.PublishedAt.IsZero() {
		version.PublishedAt = now
	}
	version.UpdatedAt = now
	if version.ID == "" {
		version.ID = newID("plgv")
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	s.ensureMapsLocked()

	for i := range s.data.Plugins {
		if s.data.Plugins[i].ID != pluginID {
			continue
		}

		plugin := s.data.Plugins[i]
		if plugin.CreatedAt.IsZero() {
			plugin.CreatedAt = now
		}

		updated := false
		for j := range plugin.Versions {
			if plugin.Versions[j].ID == version.ID {
				plugin.Versions[j] = version
				updated = true
				break
			}
		}
		if !updated {
			plugin.Versions = append(plugin.Versions, version)
		}

		sortPluginVersionsDesc(plugin.Versions)
		plugin.UpdatedAt = now
		plugin.Version = ""
		plugin.DownloadURL = ""
		plugin.SHA256 = ""
		s.data.Plugins[i] = plugin

		s.touchDirtyLocked()
		return clonePlugin(plugin), version, nil
	}

	return models.Plugin{}, models.PluginVersion{}, errors.New("plugin not found")
}

func (s *Store) DeletePluginVersion(pluginID string, versionID string) bool {
	pluginID = strings.TrimSpace(pluginID)
	versionID = strings.TrimSpace(versionID)
	if pluginID == "" || versionID == "" {
		return false
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	for i := range s.data.Plugins {
		if s.data.Plugins[i].ID != pluginID {
			continue
		}

		oldLen := len(s.data.Plugins[i].Versions)
		filtered := s.data.Plugins[i].Versions[:0]
		for _, item := range s.data.Plugins[i].Versions {
			if item.ID != versionID {
				filtered = append(filtered, item)
			}
		}
		s.data.Plugins[i].Versions = filtered
		sortPluginVersionsDesc(s.data.Plugins[i].Versions)
		if len(filtered) > 0 {
			s.data.Plugins[i].UpdatedAt = filtered[0].UpdatedAt
		} else {
			s.data.Plugins[i].UpdatedAt = time.Now().UTC()
		}

		if len(filtered) == oldLen {
			return false
		}

		s.touchDirtyLocked()
		return true
	}

	return false
}

func (s *Store) ResolvePluginDownloadTarget(id string) (models.Plugin, models.PluginVersion, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	id = strings.TrimSpace(id)
	if id == "" {
		return models.Plugin{}, models.PluginVersion{}, false
	}

	for _, plugin := range s.data.Plugins {
		if plugin.ID == id {
			if len(plugin.Versions) == 0 {
				return models.Plugin{}, models.PluginVersion{}, false
			}
			latest := pickLatestPluginVersion(plugin.Versions)
			if latest.ID == "" {
				return models.Plugin{}, models.PluginVersion{}, false
			}
			return clonePlugin(plugin), latest, true
		}
	}

	for _, plugin := range s.data.Plugins {
		for _, version := range plugin.Versions {
			if version.ID == id {
				return clonePlugin(plugin), version, true
			}
		}
	}

	return models.Plugin{}, models.PluginVersion{}, false
}

func (s *Store) DeletePlugin(id string) bool {
	s.mu.Lock()
	defer s.mu.Unlock()

	oldLen := len(s.data.Plugins)
	filtered := s.data.Plugins[:0]
	for _, plugin := range s.data.Plugins {
		if plugin.ID != id {
			filtered = append(filtered, plugin)
		}
	}
	s.data.Plugins = filtered

	if len(s.data.Plugins) == oldLen {
		return false
	}

	s.touchDirtyLocked()
	return true
}

func (s *Store) RecordRequest(event RequestMetrics) {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.ensureMapsLocked()
	s.data.Analytics.TotalRequests++
	s.data.Analytics.BytesServed += event.BytesServed
	if event.IsPageView {
		s.data.Analytics.PageViews++
	}
	if event.IsAPI {
		s.data.Analytics.APIRequests++
	}
	if event.IsDownload {
		s.data.Analytics.DownloadRequests++
	}
	if event.IsAPIDownload {
		s.data.Analytics.APIDownloads++
	}
	if route := strings.TrimSpace(event.Route); route != "" {
		s.data.Analytics.RouteHits[route]++
	}

	day := time.Now().UTC().Format("2006-01-02")
	daily := s.data.Analytics.Daily[day]
	daily.Requests++
	daily.BytesServed += event.BytesServed
	if event.IsPageView {
		daily.PageViews++
	}
	if event.IsAPI {
		daily.APIRequests++
	}
	if event.IsDownload {
		daily.Downloads++
	}
	if event.IsAPIDownload {
		daily.APIDownloads++
	}
	s.data.Analytics.Daily[day] = daily
	s.data.Analytics.LastUpdatedAt = time.Now().UTC()

	s.touchDirtyLocked()
}

func (s *Store) GetAnalytics() models.AnalyticsSnapshot {
	s.mu.RLock()
	defer s.mu.RUnlock()

	out := s.data.Analytics
	out.RouteHits = make(map[string]uint64, len(s.data.Analytics.RouteHits))
	for k, v := range s.data.Analytics.RouteHits {
		out.RouteHits[k] = v
	}
	out.Daily = make(map[string]models.DailyAnalytics, len(s.data.Analytics.Daily))
	for k, v := range s.data.Analytics.Daily {
		out.Daily[k] = v
	}
	return out
}

func (s *Store) AppendAdminLoginLog(entry models.AdminLoginLog) {
	now := time.Now().UTC()

	s.mu.Lock()
	defer s.mu.Unlock()

	s.ensureMapsLocked()

	if entry.Time.IsZero() {
		entry.Time = now
	}
	entry.IP = strings.TrimSpace(entry.IP)
	entry.Location = strings.TrimSpace(entry.Location)
	entry.Reason = strings.TrimSpace(entry.Reason)
	entry.UserAgent = strings.TrimSpace(entry.UserAgent)

	if len(entry.Reason) > 200 {
		entry.Reason = entry.Reason[:200]
	}
	if len(entry.UserAgent) > 400 {
		entry.UserAgent = entry.UserAgent[:400]
	}

	s.data.AdminAudit.LoginLogs = append([]models.AdminLoginLog{entry}, s.data.AdminAudit.LoginLogs...)
	if len(s.data.AdminAudit.LoginLogs) > maxLoginAuditLogs {
		s.data.AdminAudit.LoginLogs = s.data.AdminAudit.LoginLogs[:maxLoginAuditLogs]
	}

	s.touchDirtyLocked()
}

func (s *Store) ListAdminLoginLogs(limit int) []models.AdminLoginLog {
	s.mu.RLock()
	defer s.mu.RUnlock()

	if limit <= 0 {
		limit = 50
	}
	if limit > 200 {
		limit = 200
	}

	size := len(s.data.AdminAudit.LoginLogs)
	if size == 0 {
		return []models.AdminLoginLog{}
	}
	if size > limit {
		size = limit
	}

	result := make([]models.AdminLoginLog, size)
	copy(result, s.data.AdminAudit.LoginLogs[:size])
	return result
}

func (s *Store) IngestClientUsage(event models.ClientUsageEvent) models.UsageDashboard {
	now := time.Now().UTC()
	day := now.Format("2006-01-02")

	s.mu.Lock()
	defer s.mu.Unlock()

	s.ensureMapsLocked()

	clientID := sanitizeUsageID(event.ClientID, 72)
	if clientID == "" {
		clientID = "anonymous"
	}

	sessionID := sanitizeUsageID(event.SessionID, 96)
	appVersion := strings.TrimSpace(event.AppVersion)
	if len(appVersion) > 64 {
		appVersion = appVersion[:64]
	}

	client := s.data.Usage.Clients[clientID]
	playedDelta := calculateUsageDelta(event.PlayedCount, client.LastPlayedCount)
	closeDelta := calculateUsageDelta(event.CloseCount, client.LastCloseCount)
	changedDelta := calculateUsageDelta(event.StreamChangedCount, client.LastStreamChangedCount)

	s.data.Usage.Totals.PlayedCount += playedDelta
	s.data.Usage.Totals.CloseCount += closeDelta
	s.data.Usage.Totals.StreamChangedCount += changedDelta

	daily := s.data.Usage.Daily[day]
	daily.PlayedCount += playedDelta
	daily.CloseCount += closeDelta
	daily.StreamChangedCount += changedDelta
	daily.LastUpdatedAt = now
	s.data.Usage.Daily[day] = daily

	client.LastPlayedCount = event.PlayedCount
	client.LastCloseCount = event.CloseCount
	client.LastStreamChangedCount = event.StreamChangedCount
	client.LastSeenAt = now
	client.LastAppVersion = appVersion
	if sessionID != "" {
		client.LastSessionID = sessionID
	}
	s.data.Usage.Clients[clientID] = client

	if sessionID != "" {
		if event.Event == models.UsageEventShutdown {
			delete(s.data.Usage.Sessions, sessionID)
		} else {
			session := s.data.Usage.Sessions[sessionID]
			if session.StartedAt.IsZero() {
				session.StartedAt = now
			}
			session.ClientID = clientID
			session.AppVersion = appVersion
			session.LastSeenAt = now
			s.data.Usage.Sessions[sessionID] = session
		}
	}

	s.cleanExpiredUsageSessionsLocked(now)

	onlineNow := uint64(len(s.data.Usage.Sessions))
	daily = s.data.Usage.Daily[day]
	if onlineNow > daily.OnlinePeak {
		daily.OnlinePeak = onlineNow
		daily.LastUpdatedAt = now
		s.data.Usage.Daily[day] = daily
	}

	s.data.Usage.LastUpdatedAt = now
	s.touchDirtyLocked()
	return s.buildUsageDashboardLocked(day, now)
}

func (s *Store) GetUsageDashboard(day string) models.UsageDashboard {
	now := time.Now().UTC()

	s.mu.Lock()
	defer s.mu.Unlock()

	s.ensureMapsLocked()
	s.cleanExpiredUsageSessionsLocked(now)
	normalizedDay := normalizeUsageDay(day, now)
	return s.buildUsageDashboardLocked(normalizedDay, now)
}

func (s *Store) getLatestVersionLocked(track models.VersionTrack) (models.Version, bool) {
	var preferredID string
	if track == models.VersionTrackStable {
		preferredID = s.data.LatestStableID
	} else {
		preferredID = s.data.LatestPreviewID
	}

	if preferredID != "" {
		for _, version := range s.data.Versions {
			if version.ID == preferredID {
				return version, true
			}
		}
	}

	bestID := s.pickLatestVersionIDLocked(track)
	if bestID == "" {
		return models.Version{}, false
	}

	for _, version := range s.data.Versions {
		if version.ID == bestID {
			return version, true
		}
	}

	return models.Version{}, false
}

func (s *Store) buildUsageDashboardLocked(day string, now time.Time) models.UsageDashboard {
	today := now.Format("2006-01-02")
	selectedDay := normalizeUsageDay(day, now)

	todayUsage := s.data.Usage.Daily[today]
	dayUsage := s.data.Usage.Daily[selectedDay]

	days := make([]string, 0, len(s.data.Usage.Daily))
	for key := range s.data.Usage.Daily {
		days = append(days, key)
	}
	sort.Sort(sort.Reverse(sort.StringSlice(days)))
	if len(days) > 60 {
		days = days[:60]
	}

	return models.UsageDashboard{
		OnlineNow:     uint64(len(s.data.Usage.Sessions)),
		Today:         today,
		SelectedDay:   selectedDay,
		TodayUsage:    todayUsage,
		DayUsage:      dayUsage,
		Totals:        s.data.Usage.Totals,
		RecentDays:    days,
		LastUpdatedAt: s.data.Usage.LastUpdatedAt,
	}
}

func (s *Store) cleanExpiredUsageSessionsLocked(now time.Time) {
	if len(s.data.Usage.Sessions) == 0 {
		return
	}

	expireBefore := now.Add(-usageSessionTTL)
	for sessionID, session := range s.data.Usage.Sessions {
		if session.LastSeenAt.IsZero() || session.LastSeenAt.Before(expireBefore) {
			delete(s.data.Usage.Sessions, sessionID)
		}
	}
}

func (s *Store) pickLatestVersionIDLocked(track models.VersionTrack) string {
	var best models.Version
	found := false
	for _, version := range s.data.Versions {
		if version.Track != track {
			continue
		}
		if !found || version.PublishedAt.After(best.PublishedAt) {
			best = version
			found = true
		}
	}

	if !found {
		return ""
	}

	return best.ID
}

func (s *Store) loadOrCreate() error {
	s.mu.Lock()
	defer s.mu.Unlock()

	if err := os.MkdirAll(filepath.Dir(s.path), 0o755); err != nil {
		return err
	}

	if _, err := os.Stat(s.path); errors.Is(err, os.ErrNotExist) {
		s.data = defaultData()
		s.dirty = true
		return s.saveLocked(true)
	}

	raw, err := os.ReadFile(s.path)
	if err != nil {
		return err
	}

	var fileData models.DataFile
	if err := json.Unmarshal(raw, &fileData); err != nil {
		return err
	}

	s.data = fileData
	s.ensureMapsLocked()
	s.normalizePluginsLocked(time.Now().UTC())
	s.dirty = false
	return nil
}

func (s *Store) saveLocked(force bool) error {
	if !force && !s.dirty {
		return nil
	}

	s.data.UpdatedAt = time.Now().UTC()
	s.data.Analytics.LastUpdatedAt = s.data.UpdatedAt
	s.ensureMapsLocked()

	payload, err := json.MarshalIndent(s.data, "", "  ")
	if err != nil {
		return err
	}

	tmp := s.path + ".tmp"
	if err := os.WriteFile(tmp, payload, 0o600); err != nil {
		return err
	}
	if err := os.Rename(tmp, s.path); err != nil {
		return err
	}

	s.dirty = false
	return nil
}

func (s *Store) ensureMapsLocked() {
	if s.data.Versions == nil {
		s.data.Versions = make([]models.Version, 0)
	}
	if s.data.Plugins == nil {
		s.data.Plugins = make([]models.Plugin, 0)
	} else {
		for i := range s.data.Plugins {
			if s.data.Plugins[i].Versions == nil {
				s.data.Plugins[i].Versions = make([]models.PluginVersion, 0)
			}
		}
	}
	if s.data.Analytics.RouteHits == nil {
		s.data.Analytics.RouteHits = make(map[string]uint64)
	}
	if s.data.Analytics.Daily == nil {
		s.data.Analytics.Daily = make(map[string]models.DailyAnalytics)
	}
	if s.data.Usage.Daily == nil {
		s.data.Usage.Daily = make(map[string]models.DailyUsage)
	}
	if s.data.Usage.Clients == nil {
		s.data.Usage.Clients = make(map[string]models.UsageClientSnapshot)
	}
	if s.data.Usage.Sessions == nil {
		s.data.Usage.Sessions = make(map[string]models.UsageSession)
	}
	if s.data.AdminAudit.LoginLogs == nil {
		s.data.AdminAudit.LoginLogs = make([]models.AdminLoginLog, 0)
	}
}

func (s *Store) touchDirtyLocked() {
	s.dirty = true
	s.data.UpdatedAt = time.Now().UTC()
}

func defaultData() models.DataFile {
	now := time.Now().UTC()
	return models.DataFile{
		Versions: make([]models.Version, 0),
		Plugins:  make([]models.Plugin, 0),
		Analytics: models.AnalyticsSnapshot{
			RouteHits:     make(map[string]uint64),
			Daily:         make(map[string]models.DailyAnalytics),
			LastUpdatedAt: now,
		},
		Usage: models.UsageSnapshot{
			Daily:         make(map[string]models.DailyUsage),
			Clients:       make(map[string]models.UsageClientSnapshot),
			Sessions:      make(map[string]models.UsageSession),
			LastUpdatedAt: now,
		},
		AdminAudit: models.AdminAuditSnapshot{
			LoginLogs: make([]models.AdminLoginLog, 0),
		},
		UpdatedAt: now,
	}
}

func (s *Store) normalizePluginsLocked(now time.Time) {
	migrated := false

	for i := range s.data.Plugins {
		original := s.data.Plugins[i]
		plugin := original

		if plugin.CreatedAt.IsZero() {
			plugin.CreatedAt = plugin.UpdatedAt
		}
		if plugin.CreatedAt.IsZero() {
			plugin.CreatedAt = now
		}

		// Migrate legacy flat plugin rows to the new base+version model.
		if len(plugin.Versions) == 0 {
			legacyVersion := strings.TrimSpace(plugin.Version)
			legacyURL := strings.TrimSpace(plugin.DownloadURL)
			if legacyVersion != "" && legacyURL != "" {
				legacyUpdated := plugin.UpdatedAt
				if legacyUpdated.IsZero() {
					legacyUpdated = now
				}
				plugin.Versions = []models.PluginVersion{
					{
						ID:          newID("plgv"),
						Version:     legacyVersion,
						DownloadURL: legacyURL,
						SHA256:      strings.ToLower(strings.TrimSpace(plugin.SHA256)),
						PublishedAt: legacyUpdated,
						UpdatedAt:   legacyUpdated,
					},
				}
			}
		}

		plugin.Versions = normalizePluginVersions(plugin.Versions, now)
		sortPluginVersionsDesc(plugin.Versions)

		if plugin.UpdatedAt.IsZero() {
			if len(plugin.Versions) > 0 {
				plugin.UpdatedAt = plugin.Versions[0].UpdatedAt
			} else {
				plugin.UpdatedAt = now
			}
		}

		plugin.Version = ""
		plugin.DownloadURL = ""
		plugin.SHA256 = ""
		s.data.Plugins[i] = plugin

		if !reflect.DeepEqual(original, plugin) {
			migrated = true
		}
	}

	if migrated {
		s.dirty = true
	}
}

func sortVersionsDesc(versions []models.Version) {
	sort.Slice(versions, func(i, j int) bool {
		return versions[i].PublishedAt.After(versions[j].PublishedAt)
	})
}

func sortPluginVersionsDesc(versions []models.PluginVersion) {
	sort.Slice(versions, func(i, j int) bool {
		left := versions[i].PublishedAt
		if left.IsZero() {
			left = versions[i].UpdatedAt
		}
		right := versions[j].PublishedAt
		if right.IsZero() {
			right = versions[j].UpdatedAt
		}
		return left.After(right)
	})
}

func normalizePluginVersions(versions []models.PluginVersion, now time.Time) []models.PluginVersion {
	if len(versions) == 0 {
		return make([]models.PluginVersion, 0)
	}

	out := make([]models.PluginVersion, len(versions))
	for i := range versions {
		item := versions[i]
		if strings.TrimSpace(item.ID) == "" {
			item.ID = newID("plgv")
		}
		item.Version = strings.TrimSpace(item.Version)
		item.Changelog = strings.TrimSpace(item.Changelog)
		item.DownloadURL = strings.TrimSpace(item.DownloadURL)
		item.SHA256 = strings.ToLower(strings.TrimSpace(item.SHA256))
		if item.PublishedAt.IsZero() {
			item.PublishedAt = item.UpdatedAt
		}
		if item.PublishedAt.IsZero() {
			item.PublishedAt = now
		}
		if item.UpdatedAt.IsZero() {
			item.UpdatedAt = item.PublishedAt
		}
		out[i] = item
	}

	return out
}

func pickLatestPluginVersion(versions []models.PluginVersion) models.PluginVersion {
	if len(versions) == 0 {
		return models.PluginVersion{}
	}
	best := versions[0]
	bestDate := best.PublishedAt
	if bestDate.IsZero() {
		bestDate = best.UpdatedAt
	}

	for i := 1; i < len(versions); i++ {
		item := versions[i]
		itemDate := item.PublishedAt
		if itemDate.IsZero() {
			itemDate = item.UpdatedAt
		}
		if itemDate.After(bestDate) {
			best = item
			bestDate = itemDate
		}
	}

	return best
}

func clonePluginVersions(input []models.PluginVersion) []models.PluginVersion {
	if len(input) == 0 {
		return make([]models.PluginVersion, 0)
	}
	out := make([]models.PluginVersion, len(input))
	copy(out, input)
	return out
}

func clonePlugin(input models.Plugin) models.Plugin {
	plugin := input
	plugin.Versions = clonePluginVersions(input.Versions)
	return plugin
}

func newID(prefix string) string {
	raw := make([]byte, 8)
	if _, err := rand.Read(raw); err != nil {
		return prefix + "_" + time.Now().UTC().Format("20060102150405")
	}

	return prefix + "_" + hex.EncodeToString(raw)
}

func sanitizeUsageID(value string, maxLen int) string {
	value = strings.TrimSpace(value)
	if value == "" {
		return ""
	}
	if len(value) > maxLen {
		value = value[:maxLen]
	}
	return value
}

func calculateUsageDelta(current uint64, previous uint64) uint64 {
	if current >= previous {
		return current - previous
	}
	// Client counters may reset after local settings cleanup or reinstall.
	// Treat a decrease as a new baseline and count current as fresh increments.
	return current
}

func normalizeUsageDay(day string, now time.Time) string {
	day = strings.TrimSpace(day)
	if day == "" {
		return now.Format("2006-01-02")
	}

	if _, err := time.Parse("2006-01-02", day); err != nil {
		return now.Format("2006-01-02")
	}
	return day
}
