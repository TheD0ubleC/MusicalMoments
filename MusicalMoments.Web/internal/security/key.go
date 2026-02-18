package security

import (
	"crypto/sha256"
	"crypto/subtle"
	"encoding/hex"
	"net"
	"net/http"
	"strings"
	"sync"
	"time"
)

func VerifyUploadedKey(uploadedKey string, expectedHashHex string) bool {
	key := strings.TrimSpace(uploadedKey)
	expectedHex := strings.ToLower(strings.TrimSpace(expectedHashHex))
	if key == "" || len(expectedHex) != 64 {
		return false
	}

	// Generated admin keys are 256-bit+ and base64url encoded, usually >= 43 chars.
	// Keep a minimum length gate to reject weak short keys.
	if len(key) < 43 {
		return false
	}

	expected, err := hex.DecodeString(expectedHex)
	if err != nil || len(expected) != sha256.Size {
		return false
	}

	sum := sha256.Sum256([]byte(key))
	return subtle.ConstantTimeCompare(sum[:], expected) == 1
}

type LoginRateLimiter struct {
	mu       sync.Mutex
	attempts map[string][]time.Time
	limit    int
	window   time.Duration
}

func NewLoginRateLimiter(limit int, window time.Duration) *LoginRateLimiter {
	if limit <= 0 {
		limit = 8
	}

	if window <= 0 {
		window = 10 * time.Minute
	}

	return &LoginRateLimiter{
		attempts: make(map[string][]time.Time),
		limit:    limit,
		window:   window,
	}
}

func (l *LoginRateLimiter) Allow(ip string) bool {
	now := time.Now().UTC()
	normalized := strings.TrimSpace(ip)
	if normalized == "" {
		normalized = "unknown"
	}

	l.mu.Lock()
	defer l.mu.Unlock()

	recent := l.compactLocked(normalized, now)
	if len(recent) >= l.limit {
		l.attempts[normalized] = recent
		return false
	}

	recent = append(recent, now)
	l.attempts[normalized] = recent
	return true
}

func (l *LoginRateLimiter) Reset(ip string) {
	normalized := strings.TrimSpace(ip)
	if normalized == "" {
		normalized = "unknown"
	}

	l.mu.Lock()
	delete(l.attempts, normalized)
	l.mu.Unlock()
}

func (l *LoginRateLimiter) compactLocked(ip string, now time.Time) []time.Time {
	existing := l.attempts[ip]
	if len(existing) == 0 {
		return nil
	}

	cutoff := now.Add(-l.window)
	result := make([]time.Time, 0, len(existing))
	for _, at := range existing {
		if at.After(cutoff) {
			result = append(result, at)
		}
	}

	return result
}

func ClientIP(r *http.Request) string {
	if r == nil {
		return "unknown"
	}

	forwarded := strings.TrimSpace(r.Header.Get("X-Forwarded-For"))
	if forwarded != "" {
		parts := strings.Split(forwarded, ",")
		if len(parts) > 0 {
			ip := strings.TrimSpace(parts[0])
			if ip != "" {
				return ip
			}
		}
	}

	realIP := strings.TrimSpace(r.Header.Get("X-Real-IP"))
	if realIP != "" {
		return realIP
	}

	host, _, err := net.SplitHostPort(r.RemoteAddr)
	if err == nil && host != "" {
		return host
	}

	return r.RemoteAddr
}
