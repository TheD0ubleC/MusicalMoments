package models

import "time"

type VersionTrack string

const (
	VersionTrackStable  VersionTrack = "stable"
	VersionTrackPreview VersionTrack = "preview"
)

func IsValidTrack(track VersionTrack) bool {
	return track == VersionTrackStable || track == VersionTrackPreview
}

type Version struct {
	ID          string       `json:"id"`
	Version     string       `json:"version"`
	Title       string       `json:"title"`
	Description string       `json:"description"`
	Changelog   string       `json:"changelog"`
	Track       VersionTrack `json:"track"`
	DownloadURL string       `json:"downloadURL"`
	SHA256      string       `json:"sha256"`
	FileSize    int64        `json:"fileSize"`
	PublishedAt time.Time    `json:"publishedAt"`
	UpdatedAt   time.Time    `json:"updatedAt"`
}

type PluginVersion struct {
	ID          string    `json:"id"`
	Version     string    `json:"version"`
	Changelog   string    `json:"changelog"`
	DownloadURL string    `json:"downloadURL"`
	SHA256      string    `json:"sha256"`
	FileSize    int64     `json:"fileSize"`
	PublishedAt time.Time `json:"publishedAt"`
	UpdatedAt   time.Time `json:"updatedAt"`
}

type Plugin struct {
	ID          string          `json:"id"`
	Name        string          `json:"name"`
	Description string          `json:"description"`
	CreatedAt   time.Time       `json:"createdAt"`
	UpdatedAt   time.Time       `json:"updatedAt"`
	Versions    []PluginVersion `json:"versions"`

	// Legacy fields kept for seamless migration from old flat plugin records.
	Version     string `json:"version,omitempty"`
	DownloadURL string `json:"downloadURL,omitempty"`
	SHA256      string `json:"sha256,omitempty"`
}

type DailyAnalytics struct {
	Requests     uint64 `json:"requests"`
	PageViews    uint64 `json:"pageViews"`
	APIRequests  uint64 `json:"apiRequests"`
	Downloads    uint64 `json:"downloads"`
	APIDownloads uint64 `json:"apiDownloads"`
	BytesServed  uint64 `json:"bytesServed"`
}

type AnalyticsSnapshot struct {
	TotalRequests    uint64                    `json:"totalRequests"`
	PageViews        uint64                    `json:"pageViews"`
	APIRequests      uint64                    `json:"apiRequests"`
	DownloadRequests uint64                    `json:"downloadRequests"`
	APIDownloads     uint64                    `json:"apiDownloads"`
	BytesServed      uint64                    `json:"bytesServed"`
	RouteHits        map[string]uint64         `json:"routeHits"`
	Daily            map[string]DailyAnalytics `json:"daily"`
	LastUpdatedAt    time.Time                 `json:"lastUpdatedAt"`
}

type UsageEventType string

const (
	UsageEventHeartbeat UsageEventType = "heartbeat"
	UsageEventStartup   UsageEventType = "startup"
	UsageEventShutdown  UsageEventType = "shutdown"
)

type UsageTotals struct {
	PlayedCount        uint64 `json:"playedCount"`
	CloseCount         uint64 `json:"closeCount"`
	StreamChangedCount uint64 `json:"streamChangedCount"`
}

type DailyUsage struct {
	PlayedCount        uint64    `json:"playedCount"`
	CloseCount         uint64    `json:"closeCount"`
	StreamChangedCount uint64    `json:"streamChangedCount"`
	OnlinePeak         uint64    `json:"onlinePeak"`
	LastUpdatedAt      time.Time `json:"lastUpdatedAt"`
}

type UsageClientSnapshot struct {
	LastPlayedCount        uint64    `json:"lastPlayedCount"`
	LastCloseCount         uint64    `json:"lastCloseCount"`
	LastStreamChangedCount uint64    `json:"lastStreamChangedCount"`
	LastSeenAt             time.Time `json:"lastSeenAt"`
	LastSessionID          string    `json:"lastSessionId"`
	LastAppVersion         string    `json:"lastAppVersion"`
}

type UsageSession struct {
	ClientID   string    `json:"clientId"`
	AppVersion string    `json:"appVersion"`
	StartedAt  time.Time `json:"startedAt"`
	LastSeenAt time.Time `json:"lastSeenAt"`
}

type UsageSnapshot struct {
	Totals        UsageTotals                    `json:"totals"`
	Daily         map[string]DailyUsage          `json:"daily"`
	Clients       map[string]UsageClientSnapshot `json:"clients"`
	Sessions      map[string]UsageSession        `json:"sessions"`
	LastUpdatedAt time.Time                      `json:"lastUpdatedAt"`
}

type UsageDashboard struct {
	OnlineNow     uint64      `json:"onlineNow"`
	Today         string      `json:"today"`
	SelectedDay   string      `json:"selectedDay"`
	TodayUsage    DailyUsage  `json:"todayUsage"`
	DayUsage      DailyUsage  `json:"dayUsage"`
	Totals        UsageTotals `json:"totals"`
	RecentDays    []string    `json:"recentDays"`
	LastUpdatedAt time.Time   `json:"lastUpdatedAt"`
}

type AdminLoginLog struct {
	Time      time.Time `json:"time"`
	IP        string    `json:"ip"`
	Location  string    `json:"location"`
	Success   bool      `json:"success"`
	Reason    string    `json:"reason,omitempty"`
	UserAgent string    `json:"userAgent,omitempty"`
}

type AdminAuditSnapshot struct {
	LoginLogs []AdminLoginLog `json:"loginLogs"`
}

type ClientUsageEvent struct {
	ClientID           string         `json:"clientId"`
	SessionID          string         `json:"sessionId"`
	Event              UsageEventType `json:"event"`
	AppVersion         string         `json:"appVersion"`
	PlayedCount        uint64         `json:"playedCount"`
	CloseCount         uint64         `json:"closeCount"`
	StreamChangedCount uint64         `json:"streamChangedCount"`
}

type DataFile struct {
	Versions        []Version          `json:"versions"`
	Plugins         []Plugin           `json:"plugins"`
	LatestStableID  string             `json:"latestStableId"`
	LatestPreviewID string             `json:"latestPreviewId"`
	Analytics       AnalyticsSnapshot  `json:"analytics"`
	Usage           UsageSnapshot      `json:"usage"`
	AdminAudit      AdminAuditSnapshot `json:"adminAudit"`
	UpdatedAt       time.Time          `json:"updatedAt"`
}
