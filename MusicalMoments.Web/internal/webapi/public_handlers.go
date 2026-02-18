package webapi

import (
	"errors"
	"fmt"
	"net/http"
	"os"
	"path"
	"path/filepath"
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

func (s *Server) handleHomePage(w http.ResponseWriter, r *http.Request) {
	s.servePage(w, "index.html")
}

func (s *Server) handleDownloadPage(w http.ResponseWriter, r *http.Request) {
	s.servePage(w, "download.html")
}

func (s *Server) handleTutorialPage(w http.ResponseWriter, r *http.Request) {
	s.servePage(w, "tutorial.html")
}

func (s *Server) handleStatusPage(w http.ResponseWriter, r *http.Request) {
	s.servePage(w, "statu.html")
}

func (s *Server) handleAdminPage(w http.ResponseWriter, r *http.Request) {
	s.servePage(w, "admin.html")
}

func (s *Server) handleDownloadFile(w http.ResponseWriter, r *http.Request) {
	rawPath := strings.TrimSpace(r.PathValue("filepath"))
	if rawPath == "" {
		writeJSON(w, http.StatusBadRequest, "下载路径无效", nil)
		return
	}

	cleaned := path.Clean("/" + rawPath)
	relative := strings.TrimPrefix(cleaned, "/")
	if relative == "" || strings.Contains(relative, "..") {
		writeJSON(w, http.StatusBadRequest, "下载路径无效", nil)
		return
	}

	fullPath := filepath.Join(s.uploadRoot, filepath.FromSlash(relative))
	absPath, err := filepath.Abs(fullPath)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "路径解析失败", nil)
		return
	}

	allowedPrefix := s.uploadRoot + string(filepath.Separator)
	if absPath != s.uploadRoot && !strings.HasPrefix(absPath, allowedPrefix) {
		writeJSON(w, http.StatusForbidden, "非法下载路径", nil)
		return
	}

	stat, err := os.Stat(absPath)
	if err != nil {
		if errors.Is(err, os.ErrNotExist) {
			writeJSON(w, http.StatusNotFound, "文件不存在", nil)
			return
		}
		writeJSON(w, http.StatusInternalServerError, "读取文件失败", nil)
		return
	}
	if stat.IsDir() {
		writeJSON(w, http.StatusBadRequest, "文件路径无效", nil)
		return
	}

	fileName := filepath.Base(absPath)
	w.Header().Set("Content-Type", contentTypeForFile(fileName))
	w.Header().Set("Content-Disposition", fmt.Sprintf("attachment; filename=%q", fileName))
	http.ServeFile(w, r, absPath)
}

func (s *Server) handlePublicVersions(w http.ResponseWriter, r *http.Request) {
	trackFilter := models.VersionTrack(strings.ToLower(strings.TrimSpace(r.URL.Query().Get("track"))))
	versions := s.store.ListVersions()
	if trackFilter != "" {
		if !models.IsValidTrack(trackFilter) {
			writeJSON(w, http.StatusBadRequest, "track 参数仅支持 stable 或 preview", nil)
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

func (s *Server) handlePublicLatestVersion(w http.ResponseWriter, r *http.Request) {
	track := models.VersionTrack(strings.ToLower(strings.TrimSpace(r.URL.Query().Get("channel"))))
	if track == "" {
		track = models.VersionTrackStable
	}
	if !models.IsValidTrack(track) {
		writeJSON(w, http.StatusBadRequest, "channel 参数仅支持 stable 或 preview", nil)
		return
	}

	version, ok := s.store.GetLatestVersion(track)
	if !ok {
		writeJSON(w, http.StatusNotFound, "未找到最新版本", nil)
		return
	}

	writeJSON(w, http.StatusOK, "", version)
}

func (s *Server) handlePublicPlugins(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, "", s.store.ListPlugins())
}

func (s *Server) handlePublicStats(w http.ResponseWriter, r *http.Request) {
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
	id := strings.TrimSpace(r.PathValue("id"))
	version, ok := s.store.FindVersion(id)
	if !ok || !isSafeDownloadURL(version.DownloadURL) {
		writeJSON(w, http.StatusNotFound, "版本下载链接不存在", nil)
		return
	}

	http.Redirect(w, r, version.DownloadURL, http.StatusFound)
}

func (s *Server) handlePublicPluginDownload(w http.ResponseWriter, r *http.Request) {
	id := strings.TrimSpace(r.PathValue("id"))
	plugin, ok := s.store.FindPlugin(id)
	if !ok || !isSafeDownloadURL(plugin.DownloadURL) {
		writeJSON(w, http.StatusNotFound, "插件下载链接不存在", nil)
		return
	}

	http.Redirect(w, r, plugin.DownloadURL, http.StatusFound)
}

func (s *Server) handleClientUsageIngest(w http.ResponseWriter, r *http.Request) {
	var input clientUsageInput
	if err := decodeJSONBody(w, r, &input, 128*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "请求体格式错误", nil)
		return
	}

	if strings.TrimSpace(input.ClientID) == "" && strings.TrimSpace(input.SessionID) == "" {
		writeJSON(w, http.StatusBadRequest, "clientId 与 sessionId 不能同时为空", nil)
		return
	}

	eventType := models.UsageEventType(strings.ToLower(strings.TrimSpace(input.Event)))
	if eventType == "" {
		eventType = models.UsageEventHeartbeat
	}
	if eventType != models.UsageEventHeartbeat &&
		eventType != models.UsageEventStartup &&
		eventType != models.UsageEventShutdown {
		writeJSON(w, http.StatusBadRequest, "event 仅支持 startup/heartbeat/shutdown", nil)
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
		writeJSON(w, http.StatusInternalServerError, "页面加载失败", nil)
		return
	}

	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write(payload)
}
