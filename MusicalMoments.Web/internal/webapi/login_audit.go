package webapi

import (
	"encoding/json"
	"fmt"
	"io"
	"net"
	"net/http"
	"net/netip"
	"strings"
	"time"

	"musicalmoments/web/internal/models"
)

type geoLookupResult struct {
	Location  string
	ExpiresAt time.Time
}

type ipapiResponse struct {
	CountryName string `json:"country_name"`
	Region      string `json:"region"`
	City        string `json:"city"`
	Error       bool   `json:"error"`
}

func (s *Server) recordAdminLogin(r *http.Request, clientIP string, success bool, reason string) {
	location := s.resolveClientLocation(r, clientIP)
	entry := models.AdminLoginLog{
		Time:      time.Now().UTC(),
		IP:        strings.TrimSpace(clientIP),
		Location:  location,
		Success:   success,
		Reason:    strings.TrimSpace(reason),
		UserAgent: strings.TrimSpace(r.UserAgent()),
	}

	s.store.AppendAdminLoginLog(entry)

	status := "failed"
	if success {
		status = "success"
	}
	loggedReason := entry.Reason
	if loggedReason == "" {
		loggedReason = "-"
	}

	s.logger.Printf("admin login %s ip=%s location=%s reason=%s", status, entry.IP, entry.Location, loggedReason)
}

func (s *Server) resolveClientLocation(r *http.Request, rawIP string) string {
	if fromHeaders := locationFromHeaders(r); fromHeaders != "" {
		return fromHeaders
	}

	normalized := normalizeIP(rawIP)
	if normalized == "" {
		return "未知"
	}

	addr, err := netip.ParseAddr(normalized)
	if err != nil {
		return "未知"
	}

	if addr.IsLoopback() || addr.IsPrivate() || addr.IsLinkLocalUnicast() || addr.IsLinkLocalMulticast() {
		return "本机或内网"
	}

	if cached, ok := s.getCachedLocation(normalized); ok {
		return cached
	}

	resolved := s.lookupPublicIPLocation(normalized)
	s.setCachedLocation(normalized, resolved)
	return resolved
}

func (s *Server) lookupPublicIPLocation(ip string) string {
	if s.geoClient == nil {
		return "公网 IP"
	}

	request, err := http.NewRequest(http.MethodGet, fmt.Sprintf("https://ipapi.co/%s/json/", ip), nil)
	if err != nil {
		return "公网 IP"
	}
	request.Header.Set("User-Agent", "MM-Web/1.0")

	response, err := s.geoClient.Do(request)
	if err != nil {
		return "公网 IP"
	}
	defer response.Body.Close()

	if response.StatusCode < 200 || response.StatusCode >= 300 {
		return "公网 IP"
	}

	var payload ipapiResponse
	if err := json.NewDecoder(io.LimitReader(response.Body, 32*1024)).Decode(&payload); err != nil {
		return "公网 IP"
	}
	if payload.Error {
		return "公网 IP"
	}

	parts := make([]string, 0, 3)
	for _, part := range []string{strings.TrimSpace(payload.City), strings.TrimSpace(payload.Region), strings.TrimSpace(payload.CountryName)} {
		if part != "" {
			parts = append(parts, part)
		}
	}

	if len(parts) == 0 {
		return "公网 IP"
	}
	return strings.Join(parts, " / ")
}

func (s *Server) getCachedLocation(ip string) (string, bool) {
	s.geoMu.Lock()
	defer s.geoMu.Unlock()

	cached, ok := s.geoCache[ip]
	if !ok {
		return "", false
	}

	if time.Now().After(cached.ExpiresAt) {
		delete(s.geoCache, ip)
		return "", false
	}

	return cached.Location, true
}

func (s *Server) setCachedLocation(ip string, location string) {
	s.geoMu.Lock()
	s.geoCache[ip] = geoLookupResult{
		Location:  location,
		ExpiresAt: time.Now().Add(6 * time.Hour),
	}
	s.geoMu.Unlock()
}

func normalizeIP(raw string) string {
	trimmed := strings.TrimSpace(raw)
	if trimmed == "" {
		return ""
	}

	if addrPort, err := netip.ParseAddrPort(trimmed); err == nil {
		return addrPort.Addr().String()
	}

	if host, _, err := net.SplitHostPort(trimmed); err == nil {
		trimmed = host
	}

	if addr, err := netip.ParseAddr(trimmed); err == nil {
		return addr.String()
	}

	return trimmed
}

func locationFromHeaders(r *http.Request) string {
	if r == nil {
		return ""
	}

	country := strings.TrimSpace(r.Header.Get("CF-IPCountry"))
	if country == "" || strings.EqualFold(country, "XX") || strings.EqualFold(country, "ZZ") {
		country = strings.TrimSpace(r.Header.Get("X-Country-Code"))
	}
	if country == "" || strings.EqualFold(country, "XX") || strings.EqualFold(country, "ZZ") {
		country = strings.TrimSpace(r.Header.Get("X-Appengine-Country"))
	}

	region := strings.TrimSpace(r.Header.Get("X-Region"))
	if region == "" {
		region = strings.TrimSpace(r.Header.Get("X-Region-Code"))
	}
	city := strings.TrimSpace(r.Header.Get("X-City"))

	parts := make([]string, 0, 3)
	for _, part := range []string{city, region, country} {
		if part != "" {
			parts = append(parts, part)
		}
	}

	if len(parts) == 0 {
		return ""
	}
	return strings.Join(parts, " / ")
}
