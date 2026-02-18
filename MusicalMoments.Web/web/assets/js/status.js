(function () {
  const { q, fetchJSON, formatNumber, formatDate } = window.MMWeb;

  function setStatus(id, text, healthy) {
    const node = q(id);
    if (!node) return;
    node.textContent = text;
    node.style.color = healthy ? "#1f5daa" : "#b35a3b";
  }

  function setMetric(id, value) {
    const node = q(id);
    if (!node) return;
    node.textContent = formatNumber(value || 0);
  }

  function renderTopRoutes(list) {
    const root = q("topRoutesList");
    if (!root) return;

    root.innerHTML = "";
    if (!Array.isArray(list) || !list.length) {
      const empty = document.createElement("li");
      empty.textContent = "暂无访问路径数据。";
      root.appendChild(empty);
      return;
    }

    const maxHits = Math.max(...list.map((item) => Number(item?.hits || 0)), 1);
    list.slice(0, 10).forEach((item) => {
      const li = document.createElement("li");

      const top = document.createElement("div");
      top.className = "mm-route-top";

      const route = document.createElement("span");
      route.textContent = String(item?.route || "-");

      const hits = document.createElement("strong");
      hits.textContent = formatNumber(item?.hits || 0);

      const bar = document.createElement("div");
      bar.className = "mm-route-bar";

      const fill = document.createElement("span");
      fill.style.width = `${Math.max(8, Math.round((Number(item?.hits || 0) / maxHits) * 100))}%`;

      top.append(route, hits);
      bar.appendChild(fill);
      li.append(top, bar);
      root.appendChild(li);
    });
  }

  function setLastUpdated(value) {
    const node = q("statuLastUpdated");
    if (!node) return;
    node.textContent = `最近刷新：${value ? formatDate(value) : new Date().toLocaleString("zh-CN", { hour12: false })}`;
  }

  async function loadStatusData() {
    const [versionsRes, pluginsRes, statsRes] = await Promise.allSettled([
      fetchJSON("/api/public/versions"),
      fetchJSON("/api/public/plugins"),
      fetchJSON("/api/public/stats"),
    ]);

    const versionsPayload = versionsRes.status === "fulfilled" ? versionsRes.value : { versions: [] };
    const pluginsPayload = pluginsRes.status === "fulfilled" ? pluginsRes.value : [];
    const statsPayload = statsRes.status === "fulfilled" ? statsRes.value : { summary: {}, topRoutes: [] };

    const versions = Array.isArray(versionsPayload?.versions) ? versionsPayload.versions : [];
    const plugins = Array.isArray(pluginsPayload) ? pluginsPayload : [];
    const stableVersions = versions.filter((item) => String(item?.track || "").toLowerCase() === "stable");

    setMetric("resourceCount", stableVersions.length + plugins.length);
    setMetric("versionCount", stableVersions.length);
    setMetric("pluginCount", plugins.length);

    const summary = statsPayload?.summary || {};
    setMetric("statTotalRequests", summary.totalRequests || 0);
    setMetric("statPageViews", summary.pageViews || 0);
    setMetric("statApiRequests", summary.apiRequests || 0);
    setMetric("statDownloads", summary.downloadRequests || 0);
    setMetric("statApiDownloads", summary.apiDownloads || 0);
    setMetric("statBytesServed", Math.round((summary.bytesServed || 0) / 1024 / 1024));

    renderTopRoutes(Array.isArray(statsPayload?.topRoutes) ? statsPayload.topRoutes : []);

    const resourceHealthy = versionsRes.status === "fulfilled" && pluginsRes.status === "fulfilled";
    setStatus("syncStatus", resourceHealthy ? "服务正常" : "部分异常", resourceHealthy);

    const statsHealthy = statsRes.status === "fulfilled";
    setStatus("statsStatus", statsHealthy ? "服务正常" : "不可用", statsHealthy);

    setLastUpdated(summary.lastUpdatedAt || null);
  }

  document.addEventListener("DOMContentLoaded", () => {
    loadStatusData().catch((error) => {
      console.error(error);
      setStatus("syncStatus", "不可用", false);
      setStatus("statsStatus", "不可用", false);
      setLastUpdated(null);
    });

    setInterval(() => {
      loadStatusData().catch(() => void 0);
    }, 30000);
  });
})();
