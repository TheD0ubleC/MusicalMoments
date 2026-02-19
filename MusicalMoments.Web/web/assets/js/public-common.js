window.MMWeb = (() => {
  const THEME_KEY = "mm_theme_v3";
  const ADMIN_KNOCK_KEY = "mm_admin_knock_v1";
  const ADMIN_KNOCK_REQUIRED = 10;
  const ADMIN_KNOCK_TIMEOUT_MS = 8000;
  const THEME_FX_FINISH_MS = 780;
  const FOOTER_REPO_URL = "https://github.com/TheD0ubleC/MusicalMoments";
  const FOOTER_REPO_OWNER = "TheD0ubleC";
  const FOOTER_VERSION_FALLBACK = "v--";

  const state = {
    revealObserver: null,
    navOpen: false,
    theme: "light",
    themeFxTimers: [],
    themeFxRunning: false,
    themeFxRunId: 0,
  };

  const q = (id) => document.getElementById(id);

  function formatDate(value) {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return date.toLocaleString("zh-CN", { hour12: false });
  }

  function formatNumber(value) {
    return Number(value || 0).toLocaleString("zh-CN");
  }

  function formatBytes(bytes) {
    const value = Number(bytes || 0);
    if (value <= 0) return "-";

    if (value < 1024) return `${value} B`;
    if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KB`;
    if (value < 1024 * 1024 * 1024) return `${(value / 1024 / 1024).toFixed(2)} MB`;
    return `${(value / 1024 / 1024 / 1024).toFixed(2)} GB`;
  }

  async function fetchJSON(url) {
    const response = await fetch(url, { cache: "no-store" });
    const raw = await response.text();
    let payload = null;

    if (raw) {
      try {
        payload = JSON.parse(raw);
      } catch {
        payload = null;
      }
    }

    if (!payload || typeof payload !== "object") {
      if (!response.ok) {
        const brief = raw.trim();
        throw new Error(brief ? `请求失败: ${brief.slice(0, 120)}` : `请求失败: ${response.status}`);
      }
      throw new Error("接口返回格式错误");
    }

    if (!response.ok || payload?.ok === false) {
      throw new Error(payload?.message || `请求失败: ${response.status}`);
    }

    return payload.data;
  }

  function enableReveal() {
    const nodes = document.querySelectorAll(".mm-animated");
    if (!nodes.length) return;

    if (!("IntersectionObserver" in window)) {
      nodes.forEach((node) => node.classList.add("is-visible"));
      return;
    }

    state.revealObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            state.revealObserver.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.16 }
    );

    nodes.forEach((node) => state.revealObserver.observe(node));
  }

  function observeDynamic(root) {
    if (!root) return;

    const nodes = root.querySelectorAll(".mm-animated");
    if (!nodes.length) return;

    if (!state.revealObserver) {
      nodes.forEach((node) => node.classList.add("is-visible"));
      return;
    }

    nodes.forEach((node) => state.revealObserver.observe(node));
  }

  function setDownloadLink(id, href) {
    const node = q(id);
    if (!node) return;

    if (!href) {
      node.removeAttribute("href");
      node.setAttribute("aria-disabled", "true");
      return;
    }

    node.href = href;
    node.removeAttribute("aria-disabled");
  }

  function setupNavMenu() {
    const toggle = q("navToggle");
    const menu = q("siteNav");
    if (!toggle || !menu) return;

    const setOpen = (open) => {
      state.navOpen = open;
      menu.classList.toggle("show", open);
      toggle.setAttribute("aria-expanded", String(open));
    };

    toggle.addEventListener("click", () => setOpen(!state.navOpen));

    menu.querySelectorAll("a").forEach((link) => {
      link.addEventListener("click", () => {
        if (window.matchMedia("(max-width: 980px)").matches) {
          setOpen(false);
        }
      });
    });

    document.addEventListener("click", (event) => {
      if (!state.navOpen || window.matchMedia("(min-width: 981px)").matches) return;
      if (menu.contains(event.target) || toggle.contains(event.target)) return;
      setOpen(false);
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && state.navOpen) {
        setOpen(false);
      }
    });

    window.addEventListener("resize", () => {
      if (window.matchMedia("(min-width: 981px)").matches) {
        setOpen(false);
      }
    });
  }

  function readKnockState() {
    try {
      const raw = sessionStorage.getItem(ADMIN_KNOCK_KEY);
      if (!raw) return { count: 0, lastAt: 0 };
      const parsed = JSON.parse(raw);
      return {
        count: Number(parsed?.count || 0),
        lastAt: Number(parsed?.lastAt || 0),
      };
    } catch {
      return { count: 0, lastAt: 0 };
    }
  }

  function writeKnockState(nextState) {
    try {
      sessionStorage.setItem(ADMIN_KNOCK_KEY, JSON.stringify(nextState));
    } catch {
      // ignore storage errors
    }
  }

  function clearKnockState() {
    try {
      sessionStorage.removeItem(ADMIN_KNOCK_KEY);
    } catch {
      // ignore storage errors
    }
  }

  function setupHiddenAdminEntry() {
    const brand = document.querySelector(".mm-brand");
    if (!(brand instanceof HTMLAnchorElement)) return;

    brand.setAttribute("title", "连续点击 10 次进入后台");

    brand.addEventListener("click", (event) => {
      if (window.location.pathname === "/admin") return;

      event.preventDefault();

      const now = Date.now();
      const previous = readKnockState();
      const baseline = now - previous.lastAt > ADMIN_KNOCK_TIMEOUT_MS ? 0 : previous.count;
      const current = baseline + 1;

      writeKnockState({ count: current, lastAt: now });

      if (current >= ADMIN_KNOCK_REQUIRED) {
        clearKnockState();
        window.location.assign("/admin");
      }
    });
  }

  function formatFooterVersion(value) {
    const text = String(value || "").trim();
    if (!text) return FOOTER_VERSION_FALLBACK;
    return /^v/i.test(text) ? text : `v${text}`;
  }

  function renderFooterMeta(versionText) {
    const node = q("footerMeta");
    if (!node) return;
    node.innerHTML = `<a class="mm-inline-link" href="${FOOTER_REPO_URL}" target="_blank" rel="noopener noreferrer">${FOOTER_REPO_OWNER}</a> · ${versionText}`;
  }

  function setFooterMeta() {
    if (!q("footerMeta")) return;
    renderFooterMeta(FOOTER_VERSION_FALLBACK);

    fetchJSON("/api/public/versions/latest?channel=stable")
      .then((version) => {
        renderFooterMeta(formatFooterVersion(version?.version));
      })
      .catch(() => void 0);
  }

  function writeThemeToStorage(theme) {
    try {
      localStorage.setItem(THEME_KEY, theme);
    } catch {
      // ignore storage errors
    }
  }

  function readThemeFromStorage() {
    try {
      const stored = localStorage.getItem(THEME_KEY);
      if (stored === "dark" || stored === "light") return stored;
      return "light";
    } catch {
      return "light";
    }
  }

  function clearThemeFxTimers() {
    state.themeFxTimers.forEach((timer) => clearTimeout(timer));
    state.themeFxTimers = [];
  }

  function syncThemeToggleButton() {
    const btn = q("mmThemeToggle");
    if (btn) {
      btn.setAttribute("aria-pressed", String(state.theme === "dark"));
      btn.textContent = state.theme === "dark" ? "切换浅色" : "切换深色";
      btn.setAttribute("aria-label", state.theme === "dark" ? "切换为浅色模式" : "切换为深色模式");
    }
  }

  function applyThemeInstant(theme) {
    state.theme = theme === "dark" ? "dark" : "light";
    document.body.classList.toggle("mm-dark", state.theme === "dark");
    writeThemeToStorage(state.theme);
    syncThemeToggleButton();
  }

  function beginThemeFxRun() {
    state.themeFxRunId += 1;
    state.themeFxRunning = true;
    clearThemeFxTimers();
    document.body.classList.add("is-theme-switching");
    return state.themeFxRunId;
  }

  function finishThemeFx(runId) {
    if (typeof runId === "number" && runId !== state.themeFxRunId) return;
    const layer = q("mmThemeFx");
    if (layer) layer.remove();
    document.body.classList.remove("is-theme-switching");
    document.body.classList.remove("is-theme-view-switching");
    state.themeFxRunning = false;
    clearThemeFxTimers();
  }

  function createThemeFxLayer(nextTheme) {
    const existing = q("mmThemeFx");
    if (existing) existing.remove();

    const layer = document.createElement("div");
    layer.id = "mmThemeFx";
    layer.className = `mm-theme-fx ${nextTheme === "dark" ? "to-dark" : "to-light"}`;
    document.body.appendChild(layer);

    window.requestAnimationFrame(() => {
      layer.classList.add("is-enter");
      window.requestAnimationFrame(() => {
        layer.classList.add("is-active");
      });
    });

    return layer;
  }

  function supportsThemeViewTransition() {
    return typeof document.startViewTransition === "function";
  }

  function applyThemeWithFx(theme) {
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      applyThemeInstant(theme);
      return;
    }

    const nextTheme = theme === "dark" ? "dark" : "light";
    if (nextTheme === state.theme && !state.themeFxRunning) return;
    if (state.themeFxRunning) {
      state.themeFxRunId += 1;
      finishThemeFx();
    }

    const runId = beginThemeFxRun();

    if (supportsThemeViewTransition()) {
      document.body.classList.add("is-theme-view-switching");

      try {
        const transition = document.startViewTransition(() => {
          applyThemeInstant(nextTheme);
        });
        transition.finished.finally(() => {
          finishThemeFx(runId);
        });
        return;
      } catch {
        document.body.classList.remove("is-theme-view-switching");
      }
    }

    const layer = createThemeFxLayer(nextTheme);
    applyThemeInstant(nextTheme);

    const exitTimer = window.setTimeout(() => {
      if (runId !== state.themeFxRunId) return;
      if (!(layer instanceof HTMLElement) || !layer.isConnected) return;
      layer.classList.remove("is-enter");
      layer.classList.remove("is-active");
      layer.classList.add("is-exit");
    }, Math.round(THEME_FX_FINISH_MS * 0.56));

    const finishTimer = window.setTimeout(() => {
      finishThemeFx(runId);
    }, THEME_FX_FINISH_MS);

    state.themeFxTimers.push(exitTimer, finishTimer);
  }

  function applyTheme(theme, options = {}) {
    const nextTheme = theme === "dark" ? "dark" : "light";
    if (options?.animated) {
      applyThemeWithFx(nextTheme);
      return;
    }

    if (state.themeFxRunning) {
      state.themeFxRunId += 1;
      finishThemeFx();
    } else {
      clearThemeFxTimers();
      document.body.classList.remove("is-theme-switching");
      document.body.classList.remove("is-theme-view-switching");
    }

    applyThemeInstant(nextTheme);
  }

  function setupThemeToggle() {
    let btn = q("mmThemeToggle");
    if (!btn) {
      btn = document.createElement("button");
      btn.id = "mmThemeToggle";
      btn.type = "button";
      btn.className = "mm-theme-toggle";
      btn.setAttribute("aria-label", "切换为深色模式");
      document.body.appendChild(btn);
    }

    if (!btn.dataset.bound) {
      btn.addEventListener("click", () => {
        applyTheme(state.theme === "dark" ? "light" : "dark", { animated: true });
      });
      btn.dataset.bound = "1";
    }

    applyTheme(readThemeFromStorage());
  }

  function bootCommon() {
    enableReveal();
    setupNavMenu();
    setupHiddenAdminEntry();
    setFooterMeta();
    setupThemeToggle();
  }

  return {
    q,
    formatDate,
    formatNumber,
    formatBytes,
    fetchJSON,
    enableReveal,
    observeDynamic,
    setDownloadLink,
    setFooterMeta,
    setupThemeToggle,
    applyTheme,
    bootCommon,
  };
})();

document.addEventListener("DOMContentLoaded", () => {
  window.MMWeb.bootCommon();
});
