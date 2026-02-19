package webapi

import (
	"crypto/rand"
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"time"

	"musicalmoments/web/internal/models"
	"musicalmoments/web/internal/security"
)

var sha256Pattern = regexp.MustCompile(`^[a-fA-F0-9]{64}$`)

type versionInput struct {
	ID          string `json:"id"`
	Version     string `json:"version"`
	Title       string `json:"title"`
	Description string `json:"description"`
	Changelog   string `json:"changelog"`
	Track       string `json:"track"`
	DownloadURL string `json:"downloadURL"`
	SHA256      string `json:"sha256"`
	FileSize    int64  `json:"fileSize"`
	PublishedAt string `json:"publishedAt"`
	SetAsLatest bool   `json:"setAsLatest"`
}

type pluginBaseInput struct {
	ID          string `json:"id"`
	Name        string `json:"name"`
	Description string `json:"description"`
}

type pluginVersionInput struct {
	ID          string `json:"id"`
	Version     string `json:"version"`
	Changelog   string `json:"changelog"`
	DownloadURL string `json:"downloadURL"`
	SHA256      string `json:"sha256"`
	FileSize    int64  `json:"fileSize"`
	PublishedAt string `json:"publishedAt"`
}

func (s *Server) requireAdmin(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		token := bearerTokenFromRequest(r)
		if token == "" {
			writeJSON(w, http.StatusUnauthorized, "unauthorized", nil)
			return
		}

		if _, ok := s.tokenMgr.Validate(token); !ok {
			writeJSON(w, http.StatusUnauthorized, "session expired or invalid", nil)
			return
		}

		next.ServeHTTP(w, r)
	}
}

func (s *Server) handleAdminLogin(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodPost) {
		return
	}

	clientIP := security.ClientIP(r)
	fail := func(status int, message string, reason string) {
		s.recordAdminLogin(r, clientIP, false, reason)
		writeJSON(w, status, message, nil)
	}

	if !s.limiter.Allow(clientIP) {
		fail(http.StatusTooManyRequests, "too many login attempts", "rate_limited")
		return
	}

	r.Body = http.MaxBytesReader(w, r.Body, 1024*1024)
	if err := r.ParseMultipartForm(1024 * 1024); err != nil {
		fail(http.StatusBadRequest, "invalid multipart form", "invalid_multipart_form")
		return
	}

	keyFile, _, err := r.FormFile("keyFile")
	if err != nil {
		fail(http.StatusBadRequest, "missing keyFile", "missing_key_file")
		return
	}
	defer keyFile.Close()

	keyBytes, err := io.ReadAll(io.LimitReader(keyFile, 16*1024))
	if err != nil {
		fail(http.StatusBadRequest, "failed to read key file", "read_key_file_failed")
		return
	}

	if !security.VerifyUploadedKey(string(keyBytes), s.cfg.AdminKeyHash) {
		fail(http.StatusUnauthorized, "invalid admin key", "invalid_key")
		return
	}
	s.limiter.Reset(clientIP)

	token, claims, err := s.tokenMgr.Issue()
	if err != nil {
		fail(http.StatusInternalServerError, "failed to issue token", "issue_token_failed")
		return
	}
	s.recordAdminLogin(r, clientIP, true, "")

	writeJSON(w, http.StatusOK, "login success", map[string]any{
		"token":     token,
		"expiresAt": claims.ExpiresAt.Format(time.RFC3339),
		"issuedAt":  claims.IssuedAt.Format(time.RFC3339),
	})
}

func (s *Server) handleAdminMe(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	token := bearerTokenFromRequest(r)
	claims, ok := s.tokenMgr.Validate(token)
	if !ok {
		writeJSON(w, http.StatusUnauthorized, "session expired or invalid", nil)
		return
	}

	writeJSON(w, http.StatusOK, "", map[string]any{
		"expiresAt": claims.ExpiresAt.Format(time.RFC3339),
		"issuedAt":  claims.IssuedAt.Format(time.RFC3339),
	})
}

func (s *Server) handleAdminLogout(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodPost) {
		return
	}
	writeJSON(w, http.StatusOK, "logged out", nil)
}

func (s *Server) handleAdminDashboard(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	versions := s.store.ListVersions()
	plugins := s.store.ListPlugins()
	stats := s.store.GetAnalytics()
	stableID, previewID := s.store.GetLatestPointers()
	usage := s.store.GetUsageDashboard("")

	writeJSON(w, http.StatusOK, "", map[string]any{
		"versionCount":    len(versions),
		"pluginCount":     len(plugins),
		"latestStableId":  stableID,
		"latestPreviewId": previewID,
		"traffic":         stats,
		"usage":           usage,
	})
}

func (s *Server) handleAdminUsage(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	day := strings.TrimSpace(r.URL.Query().Get("day"))
	writeJSON(w, http.StatusOK, "", s.store.GetUsageDashboard(day))
}

func (s *Server) handleAdminLoginLogs(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodGet, http.MethodHead) {
		return
	}

	limit := 50
	if value := strings.TrimSpace(r.URL.Query().Get("limit")); value != "" {
		parsed, err := strconv.Atoi(value)
		if err != nil {
			writeJSON(w, http.StatusBadRequest, "limit must be an integer", nil)
			return
		}
		limit = parsed
	}

	logs := s.store.ListAdminLoginLogs(limit)
	writeJSON(w, http.StatusOK, "", map[string]any{
		"logs": logs,
	})
}

func (s *Server) handleAdminVersionsRoot(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	case http.MethodGet, http.MethodHead:
		s.handleAdminVersions(w, r)
	case http.MethodPost:
		s.handleAdminVersionUpsert(w, r)
	default:
		allowMethods(w, r, http.MethodGet, http.MethodPost, http.MethodHead)
	}
}

func (s *Server) handleAdminVersionsSubpath(w http.ResponseWriter, r *http.Request) {
	if strings.HasSuffix(strings.TrimRight(r.URL.Path, "/"), "/latest") {
		if !allowMethods(w, r, http.MethodPost) {
			return
		}
		s.handleAdminSetLatestVersion(w, r)
		return
	}

	switch r.Method {
	case http.MethodPut:
		s.handleAdminVersionUpsert(w, r)
	case http.MethodDelete:
		s.handleAdminVersionDelete(w, r)
	default:
		allowMethods(w, r, http.MethodPut, http.MethodDelete)
	}
}

func (s *Server) handleAdminVersions(w http.ResponseWriter, r *http.Request) {
	stableID, previewID := s.store.GetLatestPointers()
	writeJSON(w, http.StatusOK, "", map[string]any{
		"versions":        s.store.ListVersions(),
		"latestStableId":  stableID,
		"latestPreviewId": previewID,
	})
}

func (s *Server) handleAdminVersionUpsert(w http.ResponseWriter, r *http.Request) {
	var input versionInput
	if err := decodeJSONBody(w, r, &input, 1024*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid request body", nil)
		return
	}

	if r.Method == http.MethodPut {
		input.ID = pathValueOrFirstSegment(r, "id", "/api/admin/versions/", "")
	}

	version, err := sanitizeVersionInput(input)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	saved, err := s.store.UpsertVersion(version)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	if input.SetAsLatest {
		if err := s.store.SetLatestVersion(saved.Track, saved.ID); err != nil {
			writeJSON(w, http.StatusBadRequest, err.Error(), nil)
			return
		}
	}

	writeJSON(w, http.StatusOK, "version saved", saved)
}

func (s *Server) handleAdminVersionDelete(w http.ResponseWriter, r *http.Request) {
	id := pathValueOrFirstSegment(r, "id", "/api/admin/versions/", "")
	if id == "" {
		writeJSON(w, http.StatusBadRequest, "version id is required", nil)
		return
	}

	if !s.store.DeleteVersion(id) {
		writeJSON(w, http.StatusNotFound, "version not found", nil)
		return
	}

	writeJSON(w, http.StatusOK, "version deleted", nil)
}

func (s *Server) handleAdminSetLatestVersion(w http.ResponseWriter, r *http.Request) {
	id := pathValueOrFirstSegment(r, "id", "/api/admin/versions/", "/latest")
	channel := models.VersionTrack(strings.ToLower(strings.TrimSpace(r.URL.Query().Get("channel"))))
	if id == "" || !models.IsValidTrack(channel) {
		writeJSON(w, http.StatusBadRequest, "id and valid channel(stable/preview) are required", nil)
		return
	}

	if err := s.store.SetLatestVersion(channel, id); err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "latest pointer updated", map[string]any{
		"id":      id,
		"channel": channel,
	})
}

func (s *Server) handleAdminPluginsRoot(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	case http.MethodGet, http.MethodHead:
		s.handleAdminPlugins(w, r)
	case http.MethodPost:
		s.handleAdminPluginCreate(w, r)
	default:
		allowMethods(w, r, http.MethodGet, http.MethodPost, http.MethodHead)
	}
}

func (s *Server) handleAdminPluginsSubpath(w http.ResponseWriter, r *http.Request) {
	pluginID, versionID, hasVersionEndpoint := parsePluginRoute(r.URL.Path)
	if pluginID == "" {
		writeJSON(w, http.StatusBadRequest, "plugin id is required", nil)
		return
	}

	if hasVersionEndpoint {
		switch r.Method {
		case http.MethodPost:
			if versionID != "" {
				writeJSON(w, http.StatusBadRequest, "do not include version id when creating a plugin version", nil)
				return
			}
			s.handleAdminPluginVersionCreate(w, r, pluginID)
		case http.MethodPut:
			if versionID == "" {
				writeJSON(w, http.StatusBadRequest, "plugin version id is required", nil)
				return
			}
			s.handleAdminPluginVersionUpdate(w, r, pluginID, versionID)
		case http.MethodDelete:
			if versionID == "" {
				writeJSON(w, http.StatusBadRequest, "plugin version id is required", nil)
				return
			}
			s.handleAdminPluginVersionDelete(w, r, pluginID, versionID)
		default:
			allowMethods(w, r, http.MethodPost, http.MethodPut, http.MethodDelete)
		}
		return
	}

	switch r.Method {
	case http.MethodPut:
		s.handleAdminPluginUpdate(w, r, pluginID)
	case http.MethodDelete:
		s.handleAdminPluginDelete(w, r, pluginID)
	default:
		allowMethods(w, r, http.MethodPut, http.MethodDelete)
	}
}

func (s *Server) handleAdminPlugins(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, "", s.store.ListPlugins())
}

func (s *Server) handleAdminPluginCreate(w http.ResponseWriter, r *http.Request) {
	var input pluginBaseInput
	if err := decodeJSONBody(w, r, &input, 512*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid request body", nil)
		return
	}

	plugin, err := sanitizePluginBaseInput(input)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	saved, err := s.store.UpsertPlugin(plugin)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "plugin saved", saved)
}

func (s *Server) handleAdminPluginUpdate(w http.ResponseWriter, r *http.Request, pluginID string) {
	if _, ok := s.store.FindPlugin(pluginID); !ok {
		writeJSON(w, http.StatusNotFound, "plugin not found", nil)
		return
	}

	var input pluginBaseInput
	if err := decodeJSONBody(w, r, &input, 512*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid request body", nil)
		return
	}
	input.ID = pluginID

	plugin, err := sanitizePluginBaseInput(input)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	saved, err := s.store.UpsertPlugin(plugin)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "plugin updated", saved)
}

func (s *Server) handleAdminPluginDelete(w http.ResponseWriter, r *http.Request, pluginID string) {
	if !s.store.DeletePlugin(pluginID) {
		writeJSON(w, http.StatusNotFound, "plugin not found", nil)
		return
	}

	writeJSON(w, http.StatusOK, "plugin deleted", nil)
}

func (s *Server) handleAdminPluginVersionCreate(w http.ResponseWriter, r *http.Request, pluginID string) {
	var input pluginVersionInput
	if err := decodeJSONBody(w, r, &input, 512*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid request body", nil)
		return
	}

	version, err := sanitizePluginVersionInput(input)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	plugin, saved, err := s.store.UpsertPluginVersion(pluginID, version)
	if err != nil {
		if strings.Contains(strings.ToLower(err.Error()), "not found") {
			writeJSON(w, http.StatusNotFound, err.Error(), nil)
			return
		}
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "plugin version created", map[string]any{
		"plugin":  plugin,
		"version": saved,
	})
}

func (s *Server) handleAdminPluginVersionUpdate(w http.ResponseWriter, r *http.Request, pluginID string, versionID string) {
	plugin, ok := s.store.FindPlugin(pluginID)
	if !ok {
		writeJSON(w, http.StatusNotFound, "plugin not found", nil)
		return
	}
	versionExists := false
	for _, item := range plugin.Versions {
		if item.ID == versionID {
			versionExists = true
			break
		}
	}
	if !versionExists {
		writeJSON(w, http.StatusNotFound, "plugin version not found", nil)
		return
	}

	var input pluginVersionInput
	if err := decodeJSONBody(w, r, &input, 512*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid request body", nil)
		return
	}
	input.ID = versionID

	version, err := sanitizePluginVersionInput(input)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	plugin, saved, err := s.store.UpsertPluginVersion(pluginID, version)
	if err != nil {
		if strings.Contains(strings.ToLower(err.Error()), "not found") {
			writeJSON(w, http.StatusNotFound, err.Error(), nil)
			return
		}
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "plugin version updated", map[string]any{
		"plugin":  plugin,
		"version": saved,
	})
}

func (s *Server) handleAdminPluginVersionDelete(w http.ResponseWriter, r *http.Request, pluginID string, versionID string) {
	if !s.store.DeletePluginVersion(pluginID, versionID) {
		writeJSON(w, http.StatusNotFound, "plugin version not found", nil)
		return
	}

	writeJSON(w, http.StatusOK, "plugin version deleted", nil)
}

func (s *Server) handleAdminUpload(w http.ResponseWriter, r *http.Request) {
	if !allowMethods(w, r, http.MethodPost) {
		return
	}

	maxPayload := s.cfg.MaxUploadBytes + 1024*1024
	r.Body = http.MaxBytesReader(w, r.Body, maxPayload)
	if err := r.ParseMultipartForm(maxPayload); err != nil {
		writeJSON(w, http.StatusBadRequest, "invalid upload request or file too large", nil)
		return
	}

	kind := sanitizeUploadKind(r.FormValue("kind"))
	file, header, err := r.FormFile("file")
	if err != nil {
		writeJSON(w, http.StatusBadRequest, "missing file field", nil)
		return
	}
	defer file.Close()

	fileName := sanitizeFileName(header.Filename)
	if fileName == "" {
		writeJSON(w, http.StatusBadRequest, "invalid file name", nil)
		return
	}

	datePath := time.Now().UTC().Format("2006/01/02")
	relativeDir := filepath.Join(kind, datePath)
	absDir := filepath.Join(s.uploadRoot, relativeDir)
	if err := os.MkdirAll(absDir, 0o755); err != nil {
		writeJSON(w, http.StatusInternalServerError, "failed to create upload directory", nil)
		return
	}

	fileToken := time.Now().UTC().Format("150405") + "_" + strings.TrimPrefix(strings.TrimPrefix(strings.ToLower(newFileToken()), "id_"), "ver_")
	finalName := fileToken + "_" + fileName
	absPath := filepath.Join(absDir, finalName)

	dst, err := os.Create(absPath)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "failed to create file", nil)
		return
	}

	hasher := sha256.New()
	written, copyErr := io.Copy(io.MultiWriter(dst, hasher), io.LimitReader(file, s.cfg.MaxUploadBytes+1))
	closeErr := dst.Close()
	if copyErr != nil || closeErr != nil {
		_ = os.Remove(absPath)
		writeJSON(w, http.StatusInternalServerError, "failed to write file", nil)
		return
	}

	if written > s.cfg.MaxUploadBytes {
		_ = os.Remove(absPath)
		writeJSON(w, http.StatusRequestEntityTooLarge, "file exceeds upload limit", nil)
		return
	}

	relativePath := filepath.ToSlash(filepath.Join(relativeDir, finalName))
	downloadURL := "/downloads/" + relativePath
	writeJSON(w, http.StatusOK, "upload success", map[string]any{
		"kind":         kind,
		"fileName":     finalName,
		"sizeBytes":    written,
		"sha256":       hex.EncodeToString(hasher.Sum(nil)),
		"relativePath": relativePath,
		"downloadURL":  downloadURL,
	})
}

func sanitizeVersionInput(input versionInput) (models.Version, error) {
	version := models.Version{
		ID:          strings.TrimSpace(input.ID),
		Version:     strings.TrimSpace(input.Version),
		Title:       strings.TrimSpace(input.Title),
		Description: strings.TrimSpace(input.Description),
		Changelog:   strings.TrimSpace(input.Changelog),
		Track:       models.VersionTrack(strings.ToLower(strings.TrimSpace(input.Track))),
		DownloadURL: strings.TrimSpace(input.DownloadURL),
		SHA256:      strings.ToLower(strings.TrimSpace(input.SHA256)),
		FileSize:    input.FileSize,
	}

	if version.Version == "" {
		return models.Version{}, errors.New("version is required")
	}
	if len(version.Version) > 64 {
		return models.Version{}, errors.New("version is too long")
	}
	if version.Title == "" {
		return models.Version{}, errors.New("title is required")
	}
	if len(version.Title) > 120 {
		return models.Version{}, errors.New("title is too long")
	}
	if len(version.Description) > 1000 {
		return models.Version{}, errors.New("description is too long")
	}
	if len(version.Changelog) > 50000 {
		return models.Version{}, errors.New("changelog is too long")
	}
	if !models.IsValidTrack(version.Track) {
		return models.Version{}, errors.New("track must be stable or preview")
	}
	if !isSafeDownloadURL(version.DownloadURL) {
		return models.Version{}, errors.New("downloadURL must start with /downloads/ or https://")
	}
	if version.SHA256 != "" && !sha256Pattern.MatchString(version.SHA256) {
		return models.Version{}, errors.New("sha256 must be 64-char hex")
	}
	if version.FileSize < 0 {
		return models.Version{}, errors.New("fileSize must be >= 0")
	}

	if strings.TrimSpace(input.PublishedAt) != "" {
		parsed, err := time.Parse(time.RFC3339, strings.TrimSpace(input.PublishedAt))
		if err != nil {
			return models.Version{}, errors.New("publishedAt must be RFC3339")
		}
		version.PublishedAt = parsed.UTC()
	}

	return version, nil
}

func sanitizePluginBaseInput(input pluginBaseInput) (models.Plugin, error) {
	plugin := models.Plugin{
		ID:          strings.TrimSpace(input.ID),
		Name:        strings.TrimSpace(input.Name),
		Description: strings.TrimSpace(input.Description),
	}

	if plugin.Name == "" {
		return models.Plugin{}, errors.New("name is required")
	}
	if len(plugin.Name) > 120 {
		return models.Plugin{}, errors.New("name is too long")
	}
	if len(plugin.Description) > 2000 {
		return models.Plugin{}, errors.New("description is too long")
	}

	return plugin, nil
}

func sanitizePluginVersionInput(input pluginVersionInput) (models.PluginVersion, error) {
	version := models.PluginVersion{
		ID:          strings.TrimSpace(input.ID),
		Version:     strings.TrimSpace(input.Version),
		Changelog:   strings.TrimSpace(input.Changelog),
		DownloadURL: strings.TrimSpace(input.DownloadURL),
		SHA256:      strings.ToLower(strings.TrimSpace(input.SHA256)),
		FileSize:    input.FileSize,
	}

	if version.Version == "" {
		return models.PluginVersion{}, errors.New("version is required")
	}
	if len(version.Version) > 64 {
		return models.PluginVersion{}, errors.New("version is too long")
	}
	if len(version.Changelog) > 50000 {
		return models.PluginVersion{}, errors.New("changelog is too long")
	}
	if !isSafeDownloadURL(version.DownloadURL) {
		return models.PluginVersion{}, errors.New("downloadURL must start with /downloads/ or https://")
	}
	if version.SHA256 != "" && !sha256Pattern.MatchString(version.SHA256) {
		return models.PluginVersion{}, errors.New("sha256 must be 64-char hex")
	}
	if version.FileSize < 0 {
		return models.PluginVersion{}, errors.New("fileSize must be >= 0")
	}

	if strings.TrimSpace(input.PublishedAt) != "" {
		parsed, err := time.Parse(time.RFC3339, strings.TrimSpace(input.PublishedAt))
		if err != nil {
			return models.PluginVersion{}, errors.New("publishedAt must be RFC3339")
		}
		version.PublishedAt = parsed.UTC()
	}

	return version, nil
}

func parsePluginRoute(urlPath string) (pluginID string, versionID string, hasVersionEndpoint bool) {
	remainder := strings.Trim(strings.TrimPrefix(urlPath, "/api/admin/plugins/"), "/")
	if remainder == "" {
		return "", "", false
	}

	parts := strings.Split(remainder, "/")
	pluginID = strings.TrimSpace(parts[0])
	if pluginID == "" {
		return "", "", false
	}

	if len(parts) >= 2 {
		if !strings.EqualFold(parts[1], "versions") {
			return "", "", false
		}
		hasVersionEndpoint = true
		if len(parts) >= 3 {
			versionID = strings.TrimSpace(parts[2])
		}
	}

	return pluginID, versionID, hasVersionEndpoint
}

func newFileToken() string {
	raw := make([]byte, 6)
	if _, err := rand.Read(raw); err != nil {
		sum := sha256.Sum256([]byte(time.Now().UTC().Format(time.RFC3339Nano)))
		return "id_" + hex.EncodeToString(sum[:6])
	}
	return "id_" + hex.EncodeToString(raw)
}
