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

type pluginInput struct {
	ID          string `json:"id"`
	Name        string `json:"name"`
	Version     string `json:"version"`
	Description string `json:"description"`
	DownloadURL string `json:"downloadURL"`
	SHA256      string `json:"sha256"`
}

func (s *Server) requireAdmin(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		token := bearerTokenFromRequest(r)
		if token == "" {
			writeJSON(w, http.StatusUnauthorized, "未认证，请先登录后台", nil)
			return
		}

		if _, ok := s.tokenMgr.Validate(token); !ok {
			writeJSON(w, http.StatusUnauthorized, "登录状态无效或已过期，请重新登录", nil)
			return
		}

		next.ServeHTTP(w, r)
	}
}

func (s *Server) handleAdminLogin(w http.ResponseWriter, r *http.Request) {
	clientIP := security.ClientIP(r)
	fail := func(status int, message string, reason string) {
		s.recordAdminLogin(r, clientIP, false, reason)
		writeJSON(w, status, message, nil)
	}

	if !s.limiter.Allow(clientIP) {
		fail(http.StatusTooManyRequests, "登录尝试过于频繁，请稍后重试", "rate_limited")
		return
	}

	r.Body = http.MaxBytesReader(w, r.Body, 1024*1024)
	if err := r.ParseMultipartForm(1024 * 1024); err != nil {
		fail(http.StatusBadRequest, "请上传管理员 key 文件", "invalid_multipart_form")
		return
	}

	keyFile, _, err := r.FormFile("keyFile")
	if err != nil {
		fail(http.StatusBadRequest, "缺少 keyFile 文件字段", "missing_key_file")
		return
	}
	defer keyFile.Close()

	keyBytes, err := io.ReadAll(io.LimitReader(keyFile, 16*1024))
	if err != nil {
		fail(http.StatusBadRequest, "读取 key 文件失败", "read_key_file_failed")
		return
	}

	if !security.VerifyUploadedKey(string(keyBytes), s.cfg.AdminKeyHash) {
		fail(http.StatusUnauthorized, "key 校验失败", "invalid_key")
		return
	}
	s.limiter.Reset(clientIP)

	token, claims, err := s.tokenMgr.Issue()
	if err != nil {
		fail(http.StatusInternalServerError, "生成登录令牌失败", "issue_token_failed")
		return
	}
	s.recordAdminLogin(r, clientIP, true, "")

	writeJSON(w, http.StatusOK, "登录成功", map[string]any{
		"token":     token,
		"expiresAt": claims.ExpiresAt.Format(time.RFC3339),
		"issuedAt":  claims.IssuedAt.Format(time.RFC3339),
	})
}
func (s *Server) handleAdminMe(w http.ResponseWriter, r *http.Request) {
	token := bearerTokenFromRequest(r)
	claims, ok := s.tokenMgr.Validate(token)
	if !ok {
		writeJSON(w, http.StatusUnauthorized, "登录状态无效或已过期", nil)
		return
	}

	writeJSON(w, http.StatusOK, "", map[string]any{
		"expiresAt": claims.ExpiresAt.Format(time.RFC3339),
		"issuedAt":  claims.IssuedAt.Format(time.RFC3339),
	})
}

func (s *Server) handleAdminLogout(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, "已退出登录，请在前端清理本地 token", nil)
}

func (s *Server) handleAdminDashboard(w http.ResponseWriter, r *http.Request) {
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
	day := strings.TrimSpace(r.URL.Query().Get("day"))
	writeJSON(w, http.StatusOK, "", s.store.GetUsageDashboard(day))
}

func (s *Server) handleAdminLoginLogs(w http.ResponseWriter, r *http.Request) {
	limit := 50
	if value := strings.TrimSpace(r.URL.Query().Get("limit")); value != "" {
		parsed, err := strconv.Atoi(value)
		if err != nil {
			writeJSON(w, http.StatusBadRequest, "limit 必须为整数", nil)
			return
		}
		limit = parsed
	}

	logs := s.store.ListAdminLoginLogs(limit)
	writeJSON(w, http.StatusOK, "", map[string]any{
		"logs": logs,
	})
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
		writeJSON(w, http.StatusBadRequest, "请求体格式错误", nil)
		return
	}

	if r.Method == http.MethodPut {
		input.ID = strings.TrimSpace(r.PathValue("id"))
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

	writeJSON(w, http.StatusOK, "版本保存成功", saved)
}

func (s *Server) handleAdminVersionDelete(w http.ResponseWriter, r *http.Request) {
	id := strings.TrimSpace(r.PathValue("id"))
	if id == "" {
		writeJSON(w, http.StatusBadRequest, "鐗堟湰 ID 涓嶈兘涓虹┖", nil)
		return
	}

	if !s.store.DeleteVersion(id) {
		writeJSON(w, http.StatusNotFound, "版本不存在", nil)
		return
	}

	writeJSON(w, http.StatusOK, "版本已删除", nil)
}

func (s *Server) handleAdminSetLatestVersion(w http.ResponseWriter, r *http.Request) {
	id := strings.TrimSpace(r.PathValue("id"))
	channel := models.VersionTrack(strings.ToLower(strings.TrimSpace(r.URL.Query().Get("channel"))))
	if id == "" || !models.IsValidTrack(channel) {
		writeJSON(w, http.StatusBadRequest, "请提供有效的版本 ID 与 channel(stable/preview)", nil)
		return
	}

	if err := s.store.SetLatestVersion(channel, id); err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "已设置最新版本", map[string]any{
		"id":      id,
		"channel": channel,
	})
}

func (s *Server) handleAdminPlugins(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, "", s.store.ListPlugins())
}

func (s *Server) handleAdminPluginUpsert(w http.ResponseWriter, r *http.Request) {
	var input pluginInput
	if err := decodeJSONBody(w, r, &input, 512*1024); err != nil {
		writeJSON(w, http.StatusBadRequest, "请求体格式错误", nil)
		return
	}

	if r.Method == http.MethodPut {
		input.ID = strings.TrimSpace(r.PathValue("id"))
	}

	plugin, err := sanitizePluginInput(input)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	saved, err := s.store.UpsertPlugin(plugin)
	if err != nil {
		writeJSON(w, http.StatusBadRequest, err.Error(), nil)
		return
	}

	writeJSON(w, http.StatusOK, "插件保存成功", saved)
}

func (s *Server) handleAdminPluginDelete(w http.ResponseWriter, r *http.Request) {
	id := strings.TrimSpace(r.PathValue("id"))
	if id == "" {
		writeJSON(w, http.StatusBadRequest, "插件 ID 不能为空", nil)
		return
	}

	if !s.store.DeletePlugin(id) {
		writeJSON(w, http.StatusNotFound, "插件不存在", nil)
		return
	}

	writeJSON(w, http.StatusOK, "插件已删除", nil)
}

func (s *Server) handleAdminUpload(w http.ResponseWriter, r *http.Request) {
	maxPayload := s.cfg.MaxUploadBytes + 1024*1024
	r.Body = http.MaxBytesReader(w, r.Body, maxPayload)
	if err := r.ParseMultipartForm(maxPayload); err != nil {
		writeJSON(w, http.StatusBadRequest, "上传请求格式错误或文件过大", nil)
		return
	}

	kind := sanitizeUploadKind(r.FormValue("kind"))
	file, header, err := r.FormFile("file")
	if err != nil {
		writeJSON(w, http.StatusBadRequest, "缺少 file 文件字段", nil)
		return
	}
	defer file.Close()

	fileName := sanitizeFileName(header.Filename)
	if fileName == "" {
		writeJSON(w, http.StatusBadRequest, "文件名无效", nil)
		return
	}

	datePath := time.Now().UTC().Format("2006/01/02")
	relativeDir := filepath.Join(kind, datePath)
	absDir := filepath.Join(s.uploadRoot, relativeDir)
	if err := os.MkdirAll(absDir, 0o755); err != nil {
		writeJSON(w, http.StatusInternalServerError, "创建上传目录失败", nil)
		return
	}

	fileToken := time.Now().UTC().Format("150405") + "_" + strings.TrimPrefix(strings.TrimPrefix(strings.ToLower(newFileToken()), "id_"), "ver_")
	finalName := fileToken + "_" + fileName
	absPath := filepath.Join(absDir, finalName)

	dst, err := os.Create(absPath)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, "创建文件失败", nil)
		return
	}

	hasher := sha256.New()
	written, copyErr := io.Copy(io.MultiWriter(dst, hasher), io.LimitReader(file, s.cfg.MaxUploadBytes+1))
	closeErr := dst.Close()
	if copyErr != nil || closeErr != nil {
		_ = os.Remove(absPath)
		writeJSON(w, http.StatusInternalServerError, "写入文件失败", nil)
		return
	}

	if written > s.cfg.MaxUploadBytes {
		_ = os.Remove(absPath)
		writeJSON(w, http.StatusRequestEntityTooLarge, "文件超过上传大小限制", nil)
		return
	}

	relativePath := filepath.ToSlash(filepath.Join(relativeDir, finalName))
	downloadURL := "/downloads/" + relativePath
	writeJSON(w, http.StatusOK, "上传成功", map[string]any{
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
		return models.Version{}, errors.New("版本号不能为空")
	}
	if len(version.Version) > 64 {
		return models.Version{}, errors.New("版本号过长")
	}
	if version.Title == "" {
		return models.Version{}, errors.New("版本标题不能为空")
	}
	if len(version.Title) > 120 {
		return models.Version{}, errors.New("版本标题过长")
	}
	if len(version.Description) > 1000 {
		return models.Version{}, errors.New("版本描述过长")
	}
	if len(version.Changelog) > 50000 {
		return models.Version{}, errors.New("更新日志过长")
	}
	if !models.IsValidTrack(version.Track) {
		return models.Version{}, errors.New("track 浠呮敮鎸?stable 鎴?preview")
	}
	if !isSafeDownloadURL(version.DownloadURL) {
		return models.Version{}, errors.New("下载链接不安全，请使用 /downloads/ 或 https:// 链接")
	}
	if version.SHA256 != "" && !sha256Pattern.MatchString(version.SHA256) {
		return models.Version{}, errors.New("SHA256 必须是 64 位十六进制")
	}
	if version.FileSize < 0 {
		return models.Version{}, errors.New("文件大小不能为负数")
	}

	if strings.TrimSpace(input.PublishedAt) != "" {
		parsed, err := time.Parse(time.RFC3339, strings.TrimSpace(input.PublishedAt))
		if err != nil {
			return models.Version{}, errors.New("发布时间格式错误，请使用 RFC3339")
		}
		version.PublishedAt = parsed.UTC()
	}

	return version, nil
}

func sanitizePluginInput(input pluginInput) (models.Plugin, error) {
	plugin := models.Plugin{
		ID:          strings.TrimSpace(input.ID),
		Name:        strings.TrimSpace(input.Name),
		Version:     strings.TrimSpace(input.Version),
		Description: strings.TrimSpace(input.Description),
		DownloadURL: strings.TrimSpace(input.DownloadURL),
		SHA256:      strings.ToLower(strings.TrimSpace(input.SHA256)),
	}

	if plugin.Name == "" {
		return models.Plugin{}, errors.New("插件名称不能为空")
	}
	if len(plugin.Name) > 120 {
		return models.Plugin{}, errors.New("插件名称过长")
	}
	if plugin.Version == "" {
		return models.Plugin{}, errors.New("插件版本不能为空")
	}
	if len(plugin.Version) > 64 {
		return models.Plugin{}, errors.New("插件版本过长")
	}
	if len(plugin.Description) > 2000 {
		return models.Plugin{}, errors.New("插件描述过长")
	}
	if !isSafeDownloadURL(plugin.DownloadURL) {
		return models.Plugin{}, errors.New("下载链接不安全，请使用 /downloads/ 或 https:// 链接")
	}
	if plugin.SHA256 != "" && !sha256Pattern.MatchString(plugin.SHA256) {
		return models.Plugin{}, errors.New("SHA256 必须是 64 位十六进制")
	}

	return plugin, nil
}

func newFileToken() string {
	raw := make([]byte, 6)
	if _, err := rand.Read(raw); err != nil {
		sum := sha256.Sum256([]byte(time.Now().UTC().Format(time.RFC3339Nano)))
		return "id_" + hex.EncodeToString(sum[:6])
	}
	return "id_" + hex.EncodeToString(raw)
}


