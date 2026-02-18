package webapi

import (
	"encoding/json"
	"errors"
	"io"
	"mime"
	"net/http"
	"path"
	"path/filepath"
	"regexp"
	"strings"
)

type apiResponse struct {
	OK      bool   `json:"ok"`
	Message string `json:"message,omitempty"`
	Data    any    `json:"data,omitempty"`
}

func writeJSON(w http.ResponseWriter, status int, message string, data any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(apiResponse{
		OK:      status >= 200 && status < 300,
		Message: message,
		Data:    data,
	})
}

func decodeJSONBody(w http.ResponseWriter, r *http.Request, dst any, maxBytes int64) error {
	if maxBytes <= 0 {
		maxBytes = 256 * 1024
	}

	r.Body = http.MaxBytesReader(w, r.Body, maxBytes)
	defer r.Body.Close()

	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(dst); err != nil {
		return err
	}

	if err := decoder.Decode(&struct{}{}); err != io.EOF {
		return errors.New("request body must contain a single JSON object")
	}

	return nil
}

func bearerTokenFromRequest(r *http.Request) string {
	authorization := strings.TrimSpace(r.Header.Get("Authorization"))
	if authorization == "" {
		return ""
	}
	if !strings.HasPrefix(strings.ToLower(authorization), "bearer ") {
		return ""
	}
	return strings.TrimSpace(authorization[7:])
}

var idLikePattern = regexp.MustCompile(`^[a-zA-Z0-9][a-zA-Z0-9_\-]{7,}$`)

func normalizeRouteForMetrics(urlPath string) string {
	if urlPath == "" {
		return "/"
	}

	clean := path.Clean("/" + urlPath)
	segments := strings.Split(strings.Trim(clean, "/"), "/")
	for i, segment := range segments {
		if looksLikeDynamicSegment(segment) {
			segments[i] = ":id"
		}
	}
	if len(segments) == 1 && segments[0] == "" {
		return "/"
	}
	return "/" + strings.Join(segments, "/")
}

func looksLikeDynamicSegment(value string) bool {
	if value == "" {
		return false
	}
	if len(value) >= 24 {
		return true
	}
	if idLikePattern.MatchString(value) {
		return true
	}
	for _, r := range value {
		if (r >= '0' && r <= '9') || r == '-' || r == '_' {
			continue
		}
		return false
	}
	return len(value) >= 8
}

func sanitizeFileName(name string) string {
	base := filepath.Base(strings.TrimSpace(name))
	base = strings.ReplaceAll(base, "\x00", "")
	if base == "" || base == "." || base == ".." {
		return "file.bin"
	}

	builder := strings.Builder{}
	for _, c := range base {
		if (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '.' || c == '-' || c == '_' {
			builder.WriteRune(c)
		} else {
			builder.WriteRune('_')
		}
	}
	cleaned := builder.String()
	if cleaned == "" {
		cleaned = "file.bin"
	}
	if len(cleaned) > 120 {
		cleaned = cleaned[:120]
	}
	return cleaned
}

func sanitizeUploadKind(kind string) string {
	switch strings.ToLower(strings.TrimSpace(kind)) {
	case "release":
		return "release"
	case "plugin":
		return "plugin"
	default:
		return "other"
	}
}

func isSafeDownloadURL(url string) bool {
	trimmed := strings.TrimSpace(url)
	if trimmed == "" {
		return false
	}
	if strings.HasPrefix(trimmed, "/downloads/") {
		return true
	}
	if strings.HasPrefix(trimmed, "https://") {
		return true
	}
	return false
}

func contentTypeForFile(fileName string) string {
	extension := filepath.Ext(fileName)
	contentType := mime.TypeByExtension(extension)
	if contentType == "" {
		return "application/octet-stream"
	}
	return contentType
}
