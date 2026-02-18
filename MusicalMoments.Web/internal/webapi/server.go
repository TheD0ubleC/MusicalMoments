package webapi

import (
	"errors"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"sync"
	"time"

	"musicalmoments/web/internal/config"
	"musicalmoments/web/internal/security"
	"musicalmoments/web/internal/store"
)

type Server struct {
	cfg        config.Config
	store      *store.Store
	tokenMgr   *security.TokenManager
	limiter    *security.LoginRateLimiter
	logger     *log.Logger
	staticRoot string
	uploadRoot string
	geoClient  *http.Client
	geoCache   map[string]geoLookupResult
	geoMu      sync.Mutex
}

func New(cfg config.Config, dataStore *store.Store, logger *log.Logger) (*Server, error) {
	if dataStore == nil {
		return nil, errors.New("store is required")
	}

	if logger == nil {
		logger = log.New(os.Stdout, "[mm-web] ", log.LstdFlags|log.Lmicroseconds)
	}

	tokenMgr, err := security.NewTokenManager(cfg.WebSecret, cfg.SessionTTL)
	if err != nil {
		return nil, err
	}

	staticRoot, err := filepath.Abs("./web")
	if err != nil {
		return nil, err
	}
	uploadRoot, err := filepath.Abs(cfg.UploadDir)
	if err != nil {
		return nil, err
	}
	if err := os.MkdirAll(uploadRoot, 0o755); err != nil {
		return nil, err
	}

	return &Server{
		cfg:        cfg,
		store:      dataStore,
		tokenMgr:   tokenMgr,
		limiter:    security.NewLoginRateLimiter(10, 10*time.Minute),
		logger:     logger,
		staticRoot: staticRoot,
		uploadRoot: uploadRoot,
		geoClient: &http.Client{
			Timeout: 1500 * time.Millisecond,
		},
		geoCache: make(map[string]geoLookupResult),
	}, nil
}

func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()

	assetsFS := http.FileServer(http.Dir(filepath.Join(s.staticRoot, "assets")))
	mux.Handle("GET /assets/", http.StripPrefix("/assets/", assetsFS))

	mux.HandleFunc("GET /", s.handleHomePage)
	mux.HandleFunc("GET /download", s.handleDownloadPage)
	mux.HandleFunc("GET /tutorial", s.handleTutorialPage)
	mux.HandleFunc("GET /statu", s.handleStatusPage)
	mux.HandleFunc("GET /status", s.handleStatusPage)
	mux.HandleFunc("GET /admin", s.handleAdminPage)
	mux.HandleFunc("GET /downloads/{filepath...}", s.handleDownloadFile)

	mux.HandleFunc("GET /api/public/versions", s.handlePublicVersions)
	mux.HandleFunc("GET /api/public/versions/latest", s.handlePublicLatestVersion)
	mux.HandleFunc("GET /api/public/plugins", s.handlePublicPlugins)
	mux.HandleFunc("GET /api/public/stats", s.handlePublicStats)
	mux.HandleFunc("GET /api/public/download/version/{id}", s.handlePublicVersionDownload)
	mux.HandleFunc("GET /api/public/download/plugin/{id}", s.handlePublicPluginDownload)
	mux.HandleFunc("POST /api/client/usage", s.handleClientUsageIngest)

	mux.HandleFunc("POST /api/admin/login", s.handleAdminLogin)
	mux.HandleFunc("GET /api/admin/me", s.requireAdmin(s.handleAdminMe))
	mux.HandleFunc("POST /api/admin/logout", s.requireAdmin(s.handleAdminLogout))
	mux.HandleFunc("GET /api/admin/dashboard", s.requireAdmin(s.handleAdminDashboard))
	mux.HandleFunc("GET /api/admin/usage", s.requireAdmin(s.handleAdminUsage))
	mux.HandleFunc("GET /api/admin/login-logs", s.requireAdmin(s.handleAdminLoginLogs))

	mux.HandleFunc("GET /api/admin/versions", s.requireAdmin(s.handleAdminVersions))
	mux.HandleFunc("POST /api/admin/versions", s.requireAdmin(s.handleAdminVersionUpsert))
	mux.HandleFunc("PUT /api/admin/versions/{id}", s.requireAdmin(s.handleAdminVersionUpsert))
	mux.HandleFunc("DELETE /api/admin/versions/{id}", s.requireAdmin(s.handleAdminVersionDelete))
	mux.HandleFunc("POST /api/admin/versions/{id}/latest", s.requireAdmin(s.handleAdminSetLatestVersion))

	mux.HandleFunc("GET /api/admin/plugins", s.requireAdmin(s.handleAdminPlugins))
	mux.HandleFunc("POST /api/admin/plugins", s.requireAdmin(s.handleAdminPluginUpsert))
	mux.HandleFunc("PUT /api/admin/plugins/{id}", s.requireAdmin(s.handleAdminPluginUpsert))
	mux.HandleFunc("DELETE /api/admin/plugins/{id}", s.requireAdmin(s.handleAdminPluginDelete))

	mux.HandleFunc("POST /api/admin/upload", s.requireAdmin(s.handleAdminUpload))

	return s.withRecovery(s.withSecurityHeaders(s.withMetrics(s.withLogging(mux))))
}
