const q = (id) => document.getElementById(id);

const state = {
  revealObserver: null,
  navOpen: false,
  versions: [],
  plugins: [],
  latestStable: null,
  latestPreview: null,
};

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString("zh-CN", { hour12: false });
}

function formatNumber(value) {
  return Number(value || 0).toLocaleString("zh-CN");
}

function formatSizeMB(bytes) {
  const size = Number(bytes || 0);
  if (size <= 0) return "-";
  return `${(size / 1024 / 1024).toFixed(2)} MB`;
}

function safeText(value, fallback = "-") {
  const text = String(value ?? "").trim();
  return text || fallback;
}

async function fetchJSON(url) {
  const response = await fetch(url, { cache: "no-store" });
  const payload = await response.json().catch(() => ({ ok: false, message: "响应解析失败" }));
  if (!response.ok || payload?.ok === false) {
    throw new Error(payload?.message || `请求失败: ${response.status}`);
  }
  return payload.data;
}

function setupReveal() {
  const targets = document.querySelectorAll(".mm-animated");
  if (!targets.length) return;

  if (!("IntersectionObserver" in window)) {
    targets.forEach((node) => node.classList.add("is-visible"));
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
    { threshold: 0.18 }
  );

  targets.forEach((node) => state.revealObserver.observe(node));
}

function observeDynamicAnimated(root) {
  if (!root || !state.revealObserver) return;
  root.querySelectorAll(".mm-animated").forEach((node) => state.revealObserver.observe(node));
}

function animateCounter(id, targetValue) {
  const el = q(id);
  if (!el) return;

  const target = Number(targetValue || 0);
  const from = Number(el.dataset.current || 0);
  const start = performance.now();
  const duration = 620;

  const frame = (now) => {
    const progress = Math.min(1, (now - start) / duration);
    const eased = 1 - Math.pow(1 - progress, 3);
    const value = Math.round(from + (target - from) * eased);
    el.textContent = formatNumber(value);

    if (progress < 1) {
      requestAnimationFrame(frame);
    } else {
      el.dataset.current = String(target);
    }
  };

  requestAnimationFrame(frame);
}

function setDownloadState(id, enabled, href = "#") {
  const link = q(id);
  if (!link) return;

  if (!enabled) {
    link.removeAttribute("href");
    link.setAttribute("aria-disabled", "true");
    return;
  }

  link.href = href;
  link.removeAttribute("aria-disabled");
}

function setLatestCard(prefix, version) {
  const versionEl = q(`${prefix}Version`);
  if (!version || !version.id) {
    if (versionEl) versionEl.textContent = "暂无";
    setDownloadState(`${prefix}Download`, false);
    return;
  }

  if (versionEl) versionEl.textContent = `${safeText(version.version)} (${safeText(version.track)})`;
  setDownloadState(`${prefix}Download`, true, `/api/public/download/version/${encodeURIComponent(version.id)}`);
}

function syncCtaDownloads() {
  if (state.latestStable?.id) {
    setDownloadState("ctaStableDownload", true, `/api/public/download/version/${encodeURIComponent(state.latestStable.id)}`);
  } else {
    setDownloadState("ctaStableDownload", false);
  }

  if (state.latestPreview?.id) {
    setDownloadState("ctaPreviewDownload", true, `/api/public/download/version/${encodeURIComponent(state.latestPreview.id)}`);
  } else {
    setDownloadState("ctaPreviewDownload", false);
  }
}

function makeMetaTag(text, cls = "") {
  const tag = document.createElement("span");
  tag.className = `mm-card-meta ${cls}`.trim();
  tag.textContent = text;
  return tag;
}

function createVersionCard(item) {
  const card = document.createElement("article");
  card.className = "mm-release-card mm-animated";

  const title = document.createElement("h3");
  title.textContent = `${safeText(item.version)} - ${safeText(item.title, "未命名版本")}`;

  const meta = document.createElement("div");
  meta.appendChild(makeMetaTag(item.track === "stable" ? "stable" : "preview", item.track === "stable" ? "mm-track-stable" : "mm-track-preview"));
  if (Number(item.fileSize || 0) > 0) {
    meta.appendChild(makeMetaTag(formatSizeMB(item.fileSize)));
  }

  const detail = document.createElement("p");
  detail.textContent = `发布时间：${formatDate(item.publishedAt || item.updatedAt)} | 标题：${safeText(item.title)}`;

  const desc = document.createElement("p");
  desc.textContent = safeText(item.description, "暂无版本说明");

  const actions = document.createElement("div");
  actions.className = "mm-card-actions";

  const download = document.createElement("a");
  download.className = "mm-button-primary";
  download.href = `/api/public/download/version/${encodeURIComponent(item.id)}`;
  download.textContent = "下载此版本";

  actions.appendChild(download);
  card.append(title, meta, detail, desc, actions);

  if (item.changelog) {
    const details = document.createElement("details");
    details.className = "mt-2 rounded-xl border border-[#d7e1f3] bg-[#eef3fc] p-2 text-xs text-[#5f6c86]";

    const summary = document.createElement("summary");
    summary.className = "cursor-pointer font-semibold text-[#3a4a69]";
    summary.textContent = "查看更新日志";

    const pre = document.createElement("pre");
    pre.className = "mt-2 whitespace-pre-wrap font-body text-xs leading-6";
    pre.textContent = String(item.changelog).slice(0, 1800);

    details.append(summary, pre);
    card.appendChild(details);
  }

  return card;
}

function createPluginCard(item) {
  const card = document.createElement("article");
  card.className = "mm-plugin-card mm-animated";

  const title = document.createElement("h3");
  title.textContent = `${safeText(item.name)} - ${safeText(item.version)}`;

  const info = document.createElement("p");
  info.textContent = `更新时间：${formatDate(item.updatedAt)}`;

  const desc = document.createElement("p");
  desc.textContent = safeText(item.description, "暂无插件说明");

  const actions = document.createElement("div");
  actions.className = "mm-card-actions";

  const download = document.createElement("a");
  download.className = "mm-link-ghost";
  download.href = `/api/public/download/plugin/${encodeURIComponent(item.id)}`;
  download.textContent = "下载插件";

  actions.appendChild(download);
  card.append(title, info, desc, actions);
  return card;
}

function createEmptyCard(text) {
  const card = document.createElement("article");
  card.className = "mm-empty-card mm-animated";
  const title = document.createElement("h3");
  title.className = "m-0 text-base font-semibold text-[#344564]";
  title.textContent = "暂无数据";
  const detail = document.createElement("p");
  detail.textContent = text;
  card.append(title, detail);
  return card;
}

function getVersionFilters() {
  const search = safeText(q("resourceSearch")?.value || "", "").toLowerCase();
  const channel = q("versionChannelFilter")?.value || "all";
  return { search, channel };
}

function filterVersions(list) {
  const { search, channel } = getVersionFilters();

  let result = [...list];
  if (channel !== "all") {
    result = result.filter((item) => item.track === channel);
  }

  if (search) {
    result = result.filter((item) => {
      return [item.version, item.title, item.description, item.track].some((field) => String(field || "").toLowerCase().includes(search));
    });
  }

  result.sort((a, b) => {
    const left = new Date(a.publishedAt || a.updatedAt || 0).getTime();
    const right = new Date(b.publishedAt || b.updatedAt || 0).getTime();
    return right - left;
  });

  return result;
}

function renderVersions() {
  const container = q("versionList");
  if (!container) return;
  container.innerHTML = "";

  const versions = filterVersions(state.versions);
  if (!versions.length) {
    container.appendChild(createEmptyCard("当前筛选条件下没有版本。"));
    observeDynamicAnimated(container);
    return;
  }

  versions.slice(0, 12).forEach((item, index) => {
    const card = createVersionCard(item);
    if (index > 0 && index < 5) card.classList.add(`mm-stagger-${Math.min(index, 4)}`);
    container.appendChild(card);
  });

  observeDynamicAnimated(container);
}

function renderPlugins() {
  const container = q("pluginList");
  if (!container) return;
  container.innerHTML = "";

  const sorted = [...state.plugins].sort((a, b) => {
    const left = new Date(a.updatedAt || 0).getTime();
    const right = new Date(b.updatedAt || 0).getTime();
    return right - left;
  });

  if (!sorted.length) {
    container.appendChild(createEmptyCard("当前暂无可下载插件。"));
    observeDynamicAnimated(container);
    return;
  }

  sorted.slice(0, 12).forEach((item, index) => {
    const card = createPluginCard(item);
    if (index > 0 && index < 5) card.classList.add(`mm-stagger-${Math.min(index, 4)}`);
    container.appendChild(card);
  });

  observeDynamicAnimated(container);
}

function renderTopRoutes(topRoutes) {
  const list = q("topRoutes");
  if (!list) return;
  list.innerHTML = "";

  if (!topRoutes.length) {
    const li = document.createElement("li");
    li.textContent = "暂无访问路径数据";
    list.appendChild(li);
    return;
  }

  const maxHits = Math.max(...topRoutes.map((item) => Number(item.hits || 0)), 1);
  topRoutes.slice(0, 10).forEach((item) => {
    const li = document.createElement("li");

    const top = document.createElement("div");
    top.className = "mm-route-top";

    const route = document.createElement("span");
    route.textContent = safeText(item.route);

    const hits = document.createElement("strong");
    hits.textContent = formatNumber(item.hits);

    top.append(route, hits);

    const bar = document.createElement("div");
    bar.className = "mm-route-bar";

    const fill = document.createElement("span");
    fill.style.width = `${Math.max(8, Math.round((Number(item.hits || 0) / maxHits) * 100))}%`;

    bar.appendChild(fill);
    li.append(top, bar);
    list.appendChild(li);
  });
}

function updateKpis() {
  q("resourceCount").textContent = formatNumber(state.versions.length + state.plugins.length);
  q("pluginCount").textContent = formatNumber(state.plugins.length);
  q("versionCount").textContent = formatNumber(state.versions.length);
}

function renderStats(summary, topRoutes) {
  animateCounter("statTotalRequests", summary.totalRequests || 0);
  animateCounter("statPageViews", summary.pageViews || 0);
  animateCounter("statApiRequests", summary.apiRequests || 0);
  animateCounter("statDownloads", summary.downloadRequests || 0);
  animateCounter("statApiDownloads", summary.apiDownloads || 0);
  animateCounter("statBytesServed", Math.round((summary.bytesServed || 0) / 1024 / 1024));
  renderTopRoutes(topRoutes);
}

function setupFooterMeta() {
  const footerMeta = q("footerMeta");
  if (!footerMeta) return;
  footerMeta.textContent = `Powered by MM Web Backend · ${new Date().toLocaleString("zh-CN", { hour12: false })}`;
}

function setupNavMenu() {
  const toggle = q("navToggle");
  const nav = q("siteNav");
  if (!toggle || !nav) return;

  const setOpen = (open) => {
    state.navOpen = open;
    nav.classList.toggle("show", open);
    toggle.setAttribute("aria-expanded", String(open));
  };

  toggle.addEventListener("click", () => setOpen(!state.navOpen));

  nav.querySelectorAll("a[href^='#']").forEach((link) => {
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

function bindEvents() {
  q("resourceSearch")?.addEventListener("input", renderVersions);
  q("versionChannelFilter")?.addEventListener("change", renderVersions);

  q("clearSearchBtn")?.addEventListener("click", () => {
    const input = q("resourceSearch");
    const filter = q("versionChannelFilter");
    if (input) input.value = "";
    if (filter) filter.value = "all";
    renderVersions();
  });
}

async function loadAll() {
  const [latestStable, latestPreview, versionPayload, plugins, statsPayload] = await Promise.all([
    fetchJSON("/api/public/versions/latest?channel=stable").catch(() => null),
    fetchJSON("/api/public/versions/latest?channel=preview").catch(() => null),
    fetchJSON("/api/public/versions").catch(() => ({ versions: [] })),
    fetchJSON("/api/public/plugins").catch(() => []),
    fetchJSON("/api/public/stats").catch(() => ({ summary: {}, topRoutes: [] })),
  ]);

  state.latestStable = latestStable;
  state.latestPreview = latestPreview;
  state.versions = Array.isArray(versionPayload?.versions) ? versionPayload.versions : [];
  state.plugins = Array.isArray(plugins) ? plugins : [];

  setLatestCard("latestStable", state.latestStable);
  setLatestCard("latestPreview", state.latestPreview);
  syncCtaDownloads();

  updateKpis();
  renderVersions();
  renderPlugins();

  renderStats(statsPayload?.summary || {}, Array.isArray(statsPayload?.topRoutes) ? statsPayload.topRoutes : []);
  setupFooterMeta();
}

async function refreshStats() {
  const payload = await fetchJSON("/api/public/stats");
  renderStats(payload?.summary || {}, Array.isArray(payload?.topRoutes) ? payload.topRoutes : []);
  setupFooterMeta();
}

async function boot() {
  setupReveal();
  setupNavMenu();
  bindEvents();

  try {
    await loadAll();
  } catch (error) {
    console.error(error);
  }
}

document.addEventListener("DOMContentLoaded", () => {
  boot();
  setInterval(() => {
    refreshStats().catch(() => void 0);
  }, 15000);
});
