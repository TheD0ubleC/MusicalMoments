package main

import (
	"context"
	"fmt"
	"log"
	"net"
	"net/http"
	"os"
	"os/signal"
	"strconv"
	"strings"
	"syscall"
	"time"

	"musicalmoments/web/internal/config"
	"musicalmoments/web/internal/store"
	"musicalmoments/web/internal/webapi"
)

func main() {
	logger := log.New(os.Stdout, "[mm-web] ", log.LstdFlags|log.Lmicroseconds)

	cfg, err := config.LoadFromEnv()
	if err != nil {
		logger.Fatalf("配置加载失败: %v", err)
	}

	dataStore, err := store.New(cfg.DataFile)
	if err != nil {
		logger.Fatalf("数据层初始化失败: %v", err)
	}

	server, err := webapi.New(cfg, dataStore, logger)
	if err != nil {
		logger.Fatalf("Web 服务初始化失败: %v", err)
	}

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	dataStore.StartAutoSave(ctx, cfg.AutoSaveInterval)

	httpServer := &http.Server{
		Handler:      server.Handler(),
		ReadTimeout:  cfg.ReadTimeout,
		WriteTimeout: cfg.WriteTimeout,
		IdleTimeout:  cfg.IdleTimeout,
	}

	listener, bindAddr, err := listenWithFallback(cfg.Addr, 20)
	if err != nil {
		logger.Fatalf("服务运行失败: %v", err)
	}
	httpServer.Addr = bindAddr

	go func() {
		logger.Printf("服务已启动: http://%s", bindAddr)
		if err := httpServer.Serve(listener); err != nil && err != http.ErrServerClosed {
			logger.Fatalf("服务运行失败: %v", err)
		}
	}()

	stopSignal := make(chan os.Signal, 1)
	signal.Notify(stopSignal, syscall.SIGINT, syscall.SIGTERM)
	<-stopSignal

	logger.Println("收到退出信号，正在关闭服务...")
	cancel()
	_ = dataStore.SaveIfDirty()

	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer shutdownCancel()
	_ = httpServer.Shutdown(shutdownCtx)
	logger.Println("服务已安全退出。")
}

func listenWithFallback(addr string, attempts int) (net.Listener, string, error) {
	if attempts < 1 {
		attempts = 1
	}

	listener, err := net.Listen("tcp", addr)
	if err == nil {
		return listener, normalizeAddrForLog(addr), nil
	}

	host, port, parseErr := splitHostPort(addr)
	if parseErr != nil || !isAddressInUse(err) {
		return nil, "", err
	}

	for i := 1; i <= attempts; i++ {
		nextPort := port + i
		nextAddr := net.JoinHostPort(host, strconv.Itoa(nextPort))
		if host == "" {
			nextAddr = ":" + strconv.Itoa(nextPort)
		}

		nextListener, nextErr := net.Listen("tcp", nextAddr)
		if nextErr == nil {
			return nextListener, normalizeAddrForLog(nextAddr), nil
		}
		if !isAddressInUse(nextErr) {
			return nil, "", nextErr
		}
	}

	return nil, "", fmt.Errorf("端口 %d 已被占用，且后续 %d 个端口也不可用", port, attempts)
}

func splitHostPort(addr string) (string, int, error) {
	host, portText, err := net.SplitHostPort(addr)
	if err != nil {
		return "", 0, err
	}

	port, err := strconv.Atoi(portText)
	if err != nil {
		return "", 0, err
	}

	return host, port, nil
}

func normalizeAddrForLog(addr string) string {
	if strings.HasPrefix(addr, ":") {
		return "127.0.0.1" + addr
	}
	return addr
}

func isAddressInUse(err error) bool {
	if err == nil {
		return false
	}

	text := strings.ToLower(err.Error())
	return strings.Contains(text, "address already in use") ||
		strings.Contains(text, "only one usage of each socket address")
}
