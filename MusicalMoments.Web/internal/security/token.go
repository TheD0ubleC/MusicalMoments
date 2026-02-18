package security

import (
	"crypto/hmac"
	"crypto/rand"
	"crypto/sha256"
	"crypto/subtle"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"errors"
	"strings"
	"time"
)

type TokenManager struct {
	secret []byte
	ttl    time.Duration
}

type TokenClaims struct {
	IssuedAt  time.Time `json:"iat"`
	ExpiresAt time.Time `json:"exp"`
	Nonce     string    `json:"nonce"`
}

type tokenPayload struct {
	IssuedAtUnix  int64  `json:"iat"`
	ExpiresAtUnix int64  `json:"exp"`
	Nonce         string `json:"nonce"`
}

func NewTokenManager(secret string, ttl time.Duration) (*TokenManager, error) {
	if strings.TrimSpace(secret) == "" {
		return nil, errors.New("secret cannot be empty")
	}

	return &TokenManager{
		secret: []byte(secret),
		ttl:    ttl,
	}, nil
}

func (m *TokenManager) Issue() (token string, claims TokenClaims, err error) {
	nonceBytes := make([]byte, 16)
	if _, err = rand.Read(nonceBytes); err != nil {
		return "", TokenClaims{}, err
	}

	now := time.Now().UTC()
	exp := now.Add(m.ttl)
	payload := tokenPayload{
		IssuedAtUnix:  now.Unix(),
		ExpiresAtUnix: exp.Unix(),
		Nonce:         hex.EncodeToString(nonceBytes),
	}

	rawPayload, err := json.Marshal(payload)
	if err != nil {
		return "", TokenClaims{}, err
	}

	signature := signHMAC(rawPayload, m.secret)
	token = base64.RawURLEncoding.EncodeToString(rawPayload) + "." + base64.RawURLEncoding.EncodeToString(signature)
	claims = TokenClaims{
		IssuedAt:  now,
		ExpiresAt: exp,
		Nonce:     payload.Nonce,
	}
	return token, claims, nil
}

func (m *TokenManager) Validate(token string) (TokenClaims, bool) {
	parts := strings.Split(token, ".")
	if len(parts) != 2 {
		return TokenClaims{}, false
	}

	rawPayload, err := base64.RawURLEncoding.DecodeString(parts[0])
	if err != nil {
		return TokenClaims{}, false
	}

	rawSignature, err := base64.RawURLEncoding.DecodeString(parts[1])
	if err != nil {
		return TokenClaims{}, false
	}

	expectedSignature := signHMAC(rawPayload, m.secret)
	if subtle.ConstantTimeCompare(rawSignature, expectedSignature) != 1 {
		return TokenClaims{}, false
	}

	var payload tokenPayload
	if err := json.Unmarshal(rawPayload, &payload); err != nil {
		return TokenClaims{}, false
	}

	now := time.Now().UTC().Unix()
	if payload.ExpiresAtUnix <= now || payload.IssuedAtUnix > now+60 {
		return TokenClaims{}, false
	}

	return TokenClaims{
		IssuedAt:  time.Unix(payload.IssuedAtUnix, 0).UTC(),
		ExpiresAt: time.Unix(payload.ExpiresAtUnix, 0).UTC(),
		Nonce:     payload.Nonce,
	}, true
}

func signHMAC(payload []byte, secret []byte) []byte {
	mac := hmac.New(sha256.New, secret)
	mac.Write(payload)
	return mac.Sum(nil)
}
