package webapi

import (
	"errors"
	"fmt"
	"net/http"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strings"

	"musicalmoments/web/internal/models"
)

type routeHit struct {
	Route string `json:"route"`
	Hits  uint64 `json:"hits"`
}

type clientUsageInput struct {
	ClientID           string `json:"clientId"`
	SessionID          string `json:"sessionId"`
	Event              string `json:"event"`
	AppVersion         string `json:"appVersion"`
	PlayedCount        uint64 `json:"playedCount"`
	CloseCount         uint64 `json:"closeCount"`
	StreamChangedCount uint64 `json:"streamChangedCount"`
}

type publicDocItem struct {
	Path      string `json:"path"`
	File      string `json:"file"`
	Title     string `json:"title"`
	UpdatedAt string `json:"updatedAt"`
}

var (
	safeDocFilePattern = regexp.MustCompile(`^[A-Za-z0-9][A-Za-z0-9._-]{0,120}\.md$`)
	collapseDashRegex  = regexp.MustCompile(`-+`)
)

func (s *Server) handleHomePage(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}
	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}
	s.servePage(w, "index.html")
}

func (s *Server) handleDownloadPage(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}
	if r.URL.Path != "/download" {
		http.NotFound(w, r)
		return
	}
	s.servePage(w, "download.html")
}

func (s *Server) handleTutorialPage(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}
	if r.URL.Path != "/tutorial" {
		http.NotFound(w, r)
		return
	}
	s.servePage(w, "tutorial.html")
}

func (s *Server) handleStatusPage(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}
	if r.URL.Path != "/statu" && r.URL.Path != "/status" {
		http.NotFound(w, r)
		return
	}
	s.servePage(w, "statu.html")
}

func (s *Server) handleAdminPage(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}
	if r.URL.Path != "/admin" {
		http.NotFound(w, r)
		return
	}
	s.servePage(w, "admin.html")
}

func (s *Server) handleDownloadFile(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	rawPath := pathValueOrRemainder(r, "filepath", "/downloads/")
	if rawPath == "" {
		writeJSON(w, http.StatusBadRequest, "invalid download path", nil)
		return
	}

	cleaned := path.Clean("/" + rawPath)
	relative := strings.TrimPrefix(cleaned, "/")
	if relative == "" || strings.Contains(relative, "..") {
		writeJSON(w, http.StatusBadRequest, "invalid download path", nil)
		return
	}

	fullPath := filepath.Join(s.uploadRoot, filepath.FromSlash(relative))
	absPath, err := filepath.Abs(fullPath)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "failed to resolve path", nil)
		return
	}

	allowedPrefix := s.uploadRoot + string(filepath.Separator)
	if absPath != s.uploadRoot && !strings.HasPrefix(absPath, allowedPrefix) {
		writeJSON(w, http.StatusForbidden, "forbidden download path", nil)
		return
	}

	stat, err := os.Stat(absPath)
	if err != nil {
		if errors.Is(err, os.ErrNotExist) {
			writeJSON(w, http.StatusNotFound, "file not found", nil)
			return
		}
		writeJSON(w, http.StatusInternalServerError, "failed to read file", nil)
		return
	}
	if stat.IsDir() {
		writeJSON(w, http.StatusBadRequest, "invalid file path", nil)
		return
	}

	fileName := filepath.Base(absPath)
	w.Header().Set("Content-Type", contentTypeForFile(fileName))
	w.Header().Set("Content-Disposition", fmt.Sprintf("attachment; filename=%q", fileName))
	http.ServeFile(w, r, absPath)
}

func (s *Server) handlePublicVersions(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	trackFilter := models.VersionTrack(strings.ToLower(strings.TrimSpace(r.URL.Query().Get("track"))))
	versions := s.store.ListVersions()
	if trackFilter != "" {
		if !models.IsValidTrack(trackFilter) {
			writeJSON(w, http.StatusBadRequest, "track must be stable or preview", nil)
			return
		}

		filtered := make([]models.Version, 0, len(versions))
		for _, version := range versions {
			if version.Track == trackFilter {
				filtered = append(filtered, version)
			}
		}
		versions = filtered
	}

	stableID, previewID := s.store.GetLatestPointers()
	writeJSON(w, http.StatusOK, "", map[string]any{
		"versions":        versions,
		"latestStableId":  stableID,
		"latestPreviewId": previewID,
	})
}

func (s *Server) handlePublicDocs(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	docsRoot := filepath.Join(s.staticRoot, "assets", "docs")
	absDocsRoot, err := filepath.Abs(docsRoot)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "failed to resolve docs path", nil)
		return
	}

	entries, err := os.ReadDir(absDocsRoot)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "failed to read docs directory", nil)
		return
	}

	candidates := make([]publicDocItem, 0, len(entries))
	for _, entry := range entries {
		if entry.IsDir() {
			continue
		}

		fileName := strings.TrimSpace(entry.Name())
		if !safeDocFilePattern.MatchString(fileName) {
			continue
		}

		filePath := filepath.Join(absDocsRoot, fileName)
		absPath, err := filepath.Abs(filePath)
		if err != nil {
			continue
		}
		allowedPrefix := absDocsRoot + string(filepath.Separator)
		if absPath != absDocsRoot && !strings.HasPrefix(absPath, allowedPrefix) {
			continue
		}

		fileInfo, err := entry.Info()
		if err != nil {
			continue
		}

		content, err := os.ReadFile(absPath)
		if err != nil {
			continue
		}

		baseName := strings.TrimSuffix(fileName, filepath.Ext(fileName))
		docPath := docRoutePathFromBase(baseName)
		title := markdownTitleOrFallback(content, baseName)

		candidates = append(candidates, publicDocItem{
			Path:      docPath,
			File:      fileName,
			Title:     title,
			UpdatedAt: fileInfo.ModTime().UTC().Format(http.TimeFormat),
		})
	}

	if len(candidates) == 0 {
		writeJSON(w, http.StatusOK, "", map[string]any{"docs": []publicDocItem{}})
		return
	}

	sort.Slice(candidates, func(i, j int) bool {
		return strings.ToLower(candidates[i].File) < strings.ToLower(candidates[j].File)
	})

	homeIndex := 0
	for i := range candidates {
		base := strings.TrimSuffix(strings.ToLower(candidates[i].File), ".md")
		if base == "tutorial" {
			homeIndex = i
			break
		}
		if base == "readme" {
			homeIndex = i
		}
	}

	ordered := make([]publicDocItem, 0, len(candidates))
	usedPaths := map[string]struct{}{"": {}}
	home := candidates[homeIndex]
	home.Path = ""
	ordered = append(ordered, home)

	for i := range candidates {
		if i == homeIndex {
			continue
		}

		item := candidates[i]
		pathValue := item.Path
		if pathValue == "" {
			base := strings.TrimSuffix(strings.ToLower(item.File), ".md")
			pathValue = docRoutePathFromBase(base)
		}
		if pathValue == "" {
			continue
		}

		uniquePath := pathValue
		suffix := 2
		for {
			if _, exists := usedPaths[uniquePath]; !exists {
				break
			}
			uniquePath = fmt.Sprintf("%s-%d", pathValue, suffix)
			suffix += 1
		}

		item.Path = uniquePath
		usedPaths[uniquePath] = struct{}{}
		ordered = append(ordered, item)
	}

	writeJSON(w, http.StatusOK, "", map[string]any{
		"docs": ordered,
	})
}

func (s *Server) handlePublicLatestVersion(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	track := models.VersionTrack(strings.ToLower(strings.TrimSpace(r.URL.Query().Get("channel"))))
	if track == "" {
		track = models.VersionTrackStable
	}
	if !models.IsValidTrack(track) {
		writeJSON(w, http.StatusBadRequest, "channel must be stable or preview", nil)
		return
	}

	version, ok := s.store.GetLatestVersion(track)
	if !ok {
		writeJSON(w, http.StatusNotFound, "latest version not found", nil)
		return
	}

	writeJSON(w, http.StatusOK, "", version)
}

func docRoutePathFromBase(base string) string {
	value := strings.ToLower(strings.TrimSpace(base))
	value = strings.TrimPrefix(value, "tutorial-")
	value = strings.TrimPrefix(value, "doc-")
	value = strings.Map(func(r rune) rune {
		if (r >= 'a' && r <= 'z') || (r >= '0' && r <= '9') {
			return r
		}
		if r == '-' || r == '_' || r == '.' {
			return '-'
		}
		return -1
	}, value)
	value = collapseDashRegex.ReplaceAllString(value, "-")
	value = strings.Trim(value, "-")
	if value == "" || value == "readme" || value == "tutorial" {
		return ""
	}
	return value
}

func markdownTitleOrFallback(content []byte, fallback string) string {
	text := strings.ReplaceAll(string(content), "\r\n", "\n")
	lines := strings.Split(text, "\n")
	for _, line := range lines {
		trimmed := strings.TrimSpace(strings.TrimPrefix(line, "\uFEFF"))
		if strings.HasPrefix(trimmed, "#") {
			title := strings.TrimSpace(strings.TrimLeft(trimmed, "#"))
			if title != "" {
				return title
			}
		}
	}

	safe := strings.TrimSpace(fallback)
	safe = strings.ReplaceAll(safe, "_", " ")
	safe = strings.ReplaceAll(safe, "-", " ")
	if safe == "" {
		return "Untitled"
	}
	return safe
}

func (s *Server) handlePublicPlugins(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	plugins := s.store.ListPlugins()
	payload := make([]map[string]any, 0, len(plugins))
	for _, plugin := range plugins {
		var latest models.PluginVersion
		if len(plugin.Versions) > 0 {
			latest = plugin.Versions[0]
		}

		payload = append(payload, map[string]any{
			"id":            plugin.ID,
			"name":          plugin.Name,
			"description":   plugin.Description,
			"createdAt":     plugin.CreatedAt,
			"updatedAt":     plugin.UpdatedAt,
			"versionCount":  len(plugin.Versions),
			"version":       latest.Version,
			"downloadURL":   latest.DownloadURL,
			"sha256":        latest.SHA256,
			"fileSize":      latest.FileSize,
			"publishedAt":   latest.PublishedAt,
			"latestVersion": latest,
			"versions":      plugin.Versions,
		})
	}

	writeJSON(w, http.StatusOK, "", payload)
}

func (s *Server) handlePublicStats(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	stats := s.store.GetAnalytics()
	top := make([]routeHit, 0, len(stats.RouteHits))
	for route, hits := range stats.RouteHits {
		top = append(top, routeHit{Route: route, Hits: hits})
	}
	sort.Slice(top, func(i, j int) bool {
		if top[i].Hits == top[j].Hits {
			return top[i].Route < top[j].Route
		}
		return top[i].Hits > top[j].Hits
	})
	if len(top) > 20 {
		top = top[:20]
	}

	writeJSON(w, http.StatusOK, "", map[string]any{
		"summary":   stats,
		"topRoutes": top,
	})
}

func (s *Server) handlePublicVersionDownload(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	id := pathValueOrFirstSegment(r, "id", "/api/public/download/version/", "")
	version, ok := s.store.FindVersion(id)
	if !ok || !isSafeDownloadURL(version.DownloadURL) {
		writeJSON(w, http.StatusNotFound, "version download URL not found", nil)
		return
	}

	http.Redirect(w, r, version.DownloadURL, http.StatusFound)
}

func (s *Server) handlePublicPluginDownload(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	id := pathValueOrFirstSegment(r, "id", "/api/public/download/plugin/", "")
	_, version, ok := s.store.ResolvePluginDownloadTarget(id)
	if !ok || !isSafeDownloadURL(version.DownloadURL) {
		writeJSON(w, http.StatusNotFound, "plugin download URL not found", nil)
		return
	}

	http.Redirect(w, r, version.DownloadURL, http.StatusFound)
}

func (s *Server) handleClientUsageIngest(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodPost) {
		return
	}

	var input clientUsageInput
	if err := decodeJSONBody(w, r, &input, 128*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid request body", nil)
		return
	}

	if strings.TrimSpace(input.ClientID) == "" && strings.TrimSpace(input.SessionID) == "" {
		writeJSON(w, http.StatusBadRequest, "clientId and sessionId cannot both be empty", nil)
		return
	}

	eventType := models.UsageEventType(strings.ToLower(strings.TrimSpace(input.Event)))
	if eventType == "" {
		eventType = models.UsageEventHeartbeat
	}
	if eventType != models.UsageEventHeartbeat &&
		eventType != models.UsageEventStartup &&
		eventType != models.UsageEventShutdown {
		writeJSON(w, http.StatusBadRequest, "event must be startup/heartbeat/shutdown", nil)
		return
	}

	snapshot := s.store.IngestClientUsage(models.ClientUsageEvent{
		ClientID:           strings.TrimSpace(input.ClientID),
		SessionID:          strings.TrimSpace(input.SessionID),
		Event:              eventType,
		AppVersion:         strings.TrimSpace(input.AppVersion),
		PlayedCount:        input.PlayedCount,
		CloseCount:         input.CloseCount,
		StreamChangedCount: input.StreamChangedCount,
	})

	writeJSON(w, http.StatusOK, "", map[string]any{
		"onlineNow": snapshot.OnlineNow,
		"today":     snapshot.Today,
		"todayPeak": snapshot.TodayUsage.OnlinePeak,
		"updatedAt": snapshot.LastUpdatedAt,
	})
}

func (s *Server) servePage(w http.ResponseWriter, fileName string) {
	pagePath := filepath.Join(s.staticRoot, "pages", fileName)
	payload, err := os.ReadFile(pagePath)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "failed to load page", nil)
		return
	}

	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write(payload)
}
