(function () {
  const { q, fetchJSON, formatDate, setDownloadLink, observeDynamic } = window.MMWeb;

  function safeText(value, fallback = "-") {
    const text = String(value ?? "").trim();
    return text || fallback;
  }

  function clipText(value, max = 120) {
    const text = safeText(value, "");
    if (!text) return "暂无说明";
    if (text.length <= max) return text;
    return `${text.slice(0, max - 1)}…`;
  }

  function sortByDateDesc(list, ...keys) {
    return [...list].sort((a, b) => {
      const left = keys.map((key) => a?.[key]).find(Boolean);
      const right = keys.map((key) => b?.[key]).find(Boolean);
      return new Date(right || 0).getTime() - new Date(left || 0).getTime();
    });
  }

  function setLatestStable(version) {
    const versionNode = q("latestStableVersion");
    const metaNode = q("latestStableMeta");

    if (!version || !version.id) {
      if (versionNode) versionNode.textContent = "暂无可用稳定版";
      if (metaNode) metaNode.textContent = "当前未检测到可发布的稳定版本。";
      setDownloadLink("latestStableDownload", "");
      setDownloadLink("ctaStableDownload", "");
      return;
    }

    const versionText = safeText(version.version, "-");
    const published = version.publishedAt || version.updatedAt;

    if (versionNode) versionNode.textContent = versionText;
    if (metaNode) {
      metaNode.textContent = `推荐使用 · 最近更新：${published ? formatDate(published) : "未知时间"}`;
    }

    const href = `/api/public/download/version/${encodeURIComponent(version.id)}`;
    setDownloadLink("latestStableDownload", href);
    setDownloadLink("ctaStableDownload", href);
  }

  function createFeedItem(titleText, descText, metaText, href, index) {
    const card = document.createElement("article");
    card.className = "mm-resource-item mm-animated";
    if (index > 0 && index < 4) card.classList.add(`mm-stagger-${index}`);

    const title = document.createElement("h4");
    title.textContent = titleText;

    const desc = document.createElement("p");
    desc.textContent = clipText(descText, 100);

    const meta = document.createElement("div");
    meta.className = "mm-resource-meta";

    const time = document.createElement("span");
    time.textContent = metaText;

    const action = document.createElement("a");
    action.href = href;
    action.textContent = "查看详情";

    meta.append(time, action);
    card.append(title, desc, meta);
    return card;
  }

  function createEmptyFeed(text) {
    const empty = document.createElement("div");
    empty.className = "mm-resource-empty";
    empty.textContent = text;
    return empty;
  }

  function renderVersionLane(list) {
    const container = q("homeVersionLane");
    if (!container) return;

    container.innerHTML = "";
    if (!list.length) {
      container.appendChild(createEmptyFeed("暂无稳定版更新记录。"));
      return;
    }

    list.slice(0, 3).forEach((item, index) => {
      const version = safeText(item.version, "-");
      const title = safeText(item.title, "未命名版本");
      const desc = item.description || "暂无版本说明";
      const meta = `稳定版 · ${formatDate(item.publishedAt || item.updatedAt)}`;
      const href = `/api/public/download/version/${encodeURIComponent(item.id)}`;
      container.appendChild(createFeedItem(`${version} · ${title}`, desc, meta, href, index));
    });

    observeDynamic(container);
  }

  function renderPluginLane(list) {
    const container = q("homePluginLane");
    if (!container) return;

    container.innerHTML = "";
    if (!list.length) {
      container.appendChild(createEmptyFeed("暂无插件发布记录。"));
      return;
    }

    list.slice(0, 6).forEach((item, index) => {
      const title = `${safeText(item.name)} · ${safeText(item.version)}`;
      const desc = item.description || "暂无插件说明";
      const meta = `更新时间：${formatDate(item.updatedAt || item.createdAt)}`;
      const href = `/api/public/download/plugin/${encodeURIComponent(item.id)}`;
      container.appendChild(createFeedItem(title, desc, meta, href, index));
    });

    observeDynamic(container);
  }

  async function loadHomeData() {
    const [stableRes, versionsRes, pluginsRes] = await Promise.allSettled([
      fetchJSON("/api/public/versions/latest?channel=stable"),
      fetchJSON("/api/public/versions"),
      fetchJSON("/api/public/plugins"),
    ]);

    const stable = stableRes.status === "fulfilled" ? stableRes.value : null;
    const versionsPayload = versionsRes.status === "fulfilled" ? versionsRes.value : { versions: [] };
    const pluginsPayload = pluginsRes.status === "fulfilled" ? pluginsRes.value : [];

    const versions = Array.isArray(versionsPayload?.versions) ? versionsPayload.versions : [];
    const plugins = Array.isArray(pluginsPayload) ? pluginsPayload : [];

    const stableHistory = sortByDateDesc(
      versions.filter((item) => String(item.track || "").toLowerCase() === "stable"),
      "publishedAt",
      "updatedAt"
    );

    const pluginHistory = sortByDateDesc(plugins, "updatedAt", "createdAt");

    setLatestStable(stable);
    renderVersionLane(stableHistory);
    renderPluginLane(pluginHistory);
  }

  document.addEventListener("DOMContentLoaded", () => {
    loadHomeData().catch((error) => console.error(error));
  });
})();
