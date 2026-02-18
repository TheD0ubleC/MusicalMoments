package webapi

import (
	"net/http"
	"strings"
	"time"

	"musicalmoments/web/internal/store"
)

type metricsResponseWriter struct {
	http.ResponseWriter
	statusCode int
	bytes      int
}

func (w *metricsResponseWriter) WriteHeader(statusCode int) {
	w.statusCode = statusCode
	w.ResponseWriter.WriteHeader(statusCode)
}

func (w *metricsResponseWriter) Write(data []byte) (int, error) {
	if w.statusCode == 0 {
		w.statusCode = http.StatusOK
	}
	n, err := w.ResponseWriter.Write(data)
	w.bytes += n
	return n, err
}

func (s *Server) withRecovery(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		defer func() {
			if recoverValue := recover(); recoverValue != nil {
				s.logger.Printf("panic recovered: %v", recoverValue)
				writeJSON(w, http.StatusInternalServerError, "服务器内部错误", nil)
			}
		}()
		next.ServeHTTP(w, r)
	})
}

func (s *Server) withSecurityHeaders(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("X-Frame-Options", "DENY")
		w.Header().Set("X-Content-Type-Options", "nosniff")
		w.Header().Set("Referrer-Policy", "strict-origin-when-cross-origin")
		w.Header().Set("Permissions-Policy", "geolocation=(), microphone=(), camera=()")
		w.Header().Set("Cross-Origin-Opener-Policy", "same-origin")
		w.Header().Set("Cross-Origin-Resource-Policy", "same-origin")
		w.Header().Set("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'")
		next.ServeHTTP(w, r)
	})
}

func (s *Server) withMetrics(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		writer := &metricsResponseWriter{ResponseWriter: w}
		next.ServeHTTP(writer, r)

		path := r.URL.Path
		isAPI := strings.HasPrefix(path, "/api/")
		isAPIDownload := strings.HasPrefix(path, "/api/public/download/")
		isDirectDownload := strings.HasPrefix(path, "/downloads/")
		isAsset := strings.HasPrefix(path, "/assets/")
		isPageView := r.Method == http.MethodGet && !isAPI && !isDirectDownload && !isAsset
		if writer.statusCode == 0 {
			writer.statusCode = http.StatusOK
		}

		s.store.RecordRequest(store.RequestMetrics{
			Route:         normalizeRouteForMetrics(path),
			BytesServed:   uint64(max(0, writer.bytes)),
			IsAPI:         isAPI,
			IsPageView:    isPageView && writer.statusCode < http.StatusBadRequest,
			IsDownload:    isDirectDownload || isAPIDownload,
			IsAPIDownload: isAPIDownload,
		})
	})
}

func (s *Server) withLogging(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		next.ServeHTTP(w, r)
		s.logger.Printf("%s %s (%s)", r.Method, r.URL.Path, time.Since(start).Truncate(time.Millisecond))
	})
}

func max(a int, b int) int {
	if a >= b {
		return a
	}
	return b
}
