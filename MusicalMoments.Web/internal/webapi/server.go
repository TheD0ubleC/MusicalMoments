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
	mux.Handle("/assets/", http.StripPrefix("/assets/", assetsFS))

	mux.HandleFunc("/", s.handleHomePage)
	mux.HandleFunc("/download", s.handleDownloadPage)
	mux.HandleFunc("/tutorial", s.handleTutorialPage)
	mux.HandleFunc("/statu", s.handleStatusPage)
	mux.HandleFunc("/status", s.handleStatusPage)
	mux.HandleFunc("/admin", s.handleAdminPage)
	mux.HandleFunc("/downloads/", s.handleDownloadFile)

	mux.HandleFunc("/api/public/versions/latest", s.handlePublicLatestVersion)
	mux.HandleFunc("/api/public/versions", s.handlePublicVersions)
	mux.HandleFunc("/api/public/docs", s.handlePublicDocs)
	mux.HandleFunc("/api/public/plugins", s.handlePublicPlugins)
	mux.HandleFunc("/api/public/stats", s.handlePublicStats)
	mux.HandleFunc("/api/public/download/version/", s.handlePublicVersionDownload)
	mux.HandleFunc("/api/public/download/plugin/", s.handlePublicPluginDownload)
	mux.HandleFunc("/api/client/usage", s.handleClientUsageIngest)

	mux.HandleFunc("/api/admin/login", s.handleAdminLogin)
	mux.HandleFunc("/api/admin/me", s.requireAdmin(s.handleAdminMe))
	mux.HandleFunc("/api/admin/logout", s.requireAdmin(s.handleAdminLogout))
	mux.HandleFunc("/api/admin/dashboard", s.requireAdmin(s.handleAdminDashboard))
	mux.HandleFunc("/api/admin/usage", s.requireAdmin(s.handleAdminUsage))
	mux.HandleFunc("/api/admin/login-logs", s.requireAdmin(s.handleAdminLoginLogs))

	mux.HandleFunc("/api/admin/versions", s.requireAdmin(s.handleAdminVersionsRoot))
	mux.HandleFunc("/api/admin/versions/", s.requireAdmin(s.handleAdminVersionsSubpath))

	mux.HandleFunc("/api/admin/plugins", s.requireAdmin(s.handleAdminPluginsRoot))
	mux.HandleFunc("/api/admin/plugins/", s.requireAdmin(s.handleAdminPluginsSubpath))

	mux.HandleFunc("/api/admin/upload", s.requireAdmin(s.handleAdminUpload))

	return s.withRecovery(s.withSecurityHeaders(s.withMetrics(s.withLogging(mux))))
}
