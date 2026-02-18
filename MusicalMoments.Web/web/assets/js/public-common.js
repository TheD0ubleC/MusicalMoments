window.MMWeb = (() => {
  const THEME_KEY = "mm_theme_v2";
  const ADMIN_KNOCK_KEY = "mm_admin_knock_v1";
  const ADMIN_KNOCK_REQUIRED = 10;
  const ADMIN_KNOCK_TIMEOUT_MS = 8000;

  const state = {
    revealObserver: null,
    navOpen: false,
    theme: "light",
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

  async function fetchJSON(url) {
    const response = await fetch(url, { cache: "no-store" });
    const payload = await response.json().catch(() => ({ ok: false, message: "响应解析失败" }));
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
    if (!root || !state.revealObserver) return;
    root.querySelectorAll(".mm-animated").forEach((node) => state.revealObserver.observe(node));
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
      event.preventDefault();

      const now = Date.now();
      const previous = readKnockState();
      const baseline = now-previous.lastAt > ADMIN_KNOCK_TIMEOUT_MS ? 0 : previous.count;
      const current = baseline + 1;

      writeKnockState({ count: current, lastAt: now });

      if (current >= ADMIN_KNOCK_REQUIRED) {
        clearKnockState();
        window.location.assign("/admin");
      }
    });
  }

  function setFooterMeta() {
    const node = q("footerMeta");
    if (!node) return;
    node.textContent = `MM Web Backend · ${new Date().toLocaleString("zh-CN", { hour12: false })}`;
  }

  function applyTheme(theme) {
    state.theme = theme === "dark" ? "dark" : "light";
    document.body.classList.toggle("mm-dark", state.theme === "dark");
    localStorage.setItem(THEME_KEY, state.theme);

    const btn = q("mmThemeToggle");
    if (btn) {
      btn.setAttribute("aria-pressed", String(state.theme === "dark"));
      btn.textContent = state.theme === "dark" ? "切换浅色" : "切换深色";
      btn.setAttribute("aria-label", state.theme === "dark" ? "切换为浅色模式" : "切换为深色模式");
    }
  }

  function resolveInitialTheme() {
    const stored = localStorage.getItem(THEME_KEY);
    if (stored === "dark" || stored === "light") {
      return stored;
    }
    return "light";
  }

  function setupThemeToggle() {
    if (q("mmThemeToggle")) return;

    const btn = document.createElement("button");
    btn.id = "mmThemeToggle";
    btn.type = "button";
    btn.className = "mm-theme-toggle";
    btn.setAttribute("aria-label", "切换为深色模式");

    btn.addEventListener("click", () => {
      applyTheme(state.theme === "dark" ? "light" : "dark");
    });

    document.body.appendChild(btn);
    applyTheme(resolveInitialTheme());
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
