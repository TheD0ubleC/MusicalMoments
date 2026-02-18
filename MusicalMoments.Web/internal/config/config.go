package config

import (
	"bufio"
	"errors"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"time"
)

type Config struct {
	Addr             string
	DataFile         string
	UploadDir        string
	WebSecret        string
	AdminKeyHash     string
	SessionTTL       time.Duration
	AutoSaveInterval time.Duration
	MaxUploadBytes   int64
	ReadTimeout      time.Duration
	WriteTimeout     time.Duration
	IdleTimeout      time.Duration
}

func LoadFromEnv() (Config, error) {
	if err := preloadDotEnv(); err != nil {
		return Config{}, err
	}

	cfg := Config{
		Addr:             getOrDefault("MM_WEB_ADDR", ":8090"),
		DataFile:         getOrDefault("MM_WEB_DATA_FILE", "./data/site-data.json"),
		UploadDir:        getOrDefault("MM_WEB_UPLOAD_DIR", "./data/uploads"),
		WebSecret:        os.Getenv("MM_WEB_SECRET"),
		AdminKeyHash:     os.Getenv("MM_ADMIN_KEY_HASH"),
		SessionTTL:       time.Duration(getEnvInt("MM_WEB_SESSION_TTL_HOURS", 12)) * time.Hour,
		AutoSaveInterval: time.Duration(getEnvInt("MM_WEB_AUTOSAVE_SECONDS", 15)) * time.Second,
		MaxUploadBytes:   int64(getEnvInt("MM_WEB_MAX_UPLOAD_MB", 1024)) * 1024 * 1024,
		ReadTimeout:      time.Duration(getEnvInt("MM_WEB_READ_TIMEOUT_SECONDS", 15)) * time.Second,
		WriteTimeout:     time.Duration(getEnvInt("MM_WEB_WRITE_TIMEOUT_SECONDS", 30)) * time.Second,
		IdleTimeout:      time.Duration(getEnvInt("MM_WEB_IDLE_TIMEOUT_SECONDS", 60)) * time.Second,
	}

	if cfg.WebSecret == "" {
		return Config{}, errors.New("missing MM_WEB_SECRET; run: python ../scripts/generate_mm_web_secret.py")
	}

	if cfg.AdminKeyHash == "" {
		return Config{}, errors.New("missing MM_ADMIN_KEY_HASH; run: python ../scripts/generate_mm_web_secret.py")
	}

	if cfg.SessionTTL < time.Hour {
		cfg.SessionTTL = time.Hour
	}

	if cfg.AutoSaveInterval < 5*time.Second {
		cfg.AutoSaveInterval = 5 * time.Second
	}

	if cfg.MaxUploadBytes < 10*1024*1024 {
		cfg.MaxUploadBytes = 10 * 1024 * 1024
	}

	return cfg, nil
}

func preloadDotEnv() error {
	custom := strings.TrimSpace(os.Getenv("MM_WEB_ENV_FILE"))
	if custom != "" {
		return loadDotEnvFile(custom)
	}

	candidates := []string{
		".env",
		filepath.Join("MusicalMoments.Web", ".env"),
	}
	seen := make(map[string]struct{}, len(candidates))
	for _, candidate := range candidates {
		if candidate == "" {
			continue
		}
		if _, ok := seen[candidate]; ok {
			continue
		}
		seen[candidate] = struct{}{}
		if err := loadDotEnvFile(candidate); err != nil {
			return err
		}
	}
	return nil
}

func loadDotEnvFile(path string) error {
	file, err := os.Open(path)
	if err != nil {
		if errors.Is(err, os.ErrNotExist) {
			return nil
		}
		return err
	}
	defer file.Close()

	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}

		eq := strings.IndexRune(line, '=')
		if eq <= 0 {
			continue
		}

		key := strings.TrimSpace(line[:eq])
		if key == "" {
			continue
		}

		if _, exists := os.LookupEnv(key); exists {
			continue
		}

		value := strings.TrimSpace(line[eq+1:])
		if len(value) >= 2 {
			if (value[0] == '"' && value[len(value)-1] == '"') || (value[0] == '\'' && value[len(value)-1] == '\'') {
				value = value[1 : len(value)-1]
			}
		}

		_ = os.Setenv(key, value)
	}

	return scanner.Err()
}

func getOrDefault(key string, fallback string) string {
	value := os.Getenv(key)
	if value == "" {
		return fallback
	}

	return value
}

func getEnvInt(key string, fallback int) int {
	value := os.Getenv(key)
	if value == "" {
		return fallback
	}

	parsed, err := strconv.Atoi(value)
	if err != nil {
		return fallback
	}

	return parsed
}
