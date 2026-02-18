(function () {
  const { q, fetchJSON, formatDate, setDownloadLink, observeDynamic } = window.MMWeb;

  function safeText(value, fallback = "-") {
    const text = String(value ?? "").trim();
    return text || fallback;
  }

  function clipText(value, max = 140) {
    const text = safeText(value, "");
    if (!text) return "暂无说明";
    if (text.length <= max) return text;
    return `${text.slice(0, max - 1)}…`;
  }

  function setLatestStable(version) {
    const titleNode = q("latestStableVersion");
    const metaNode = q("latestStableMeta");
    const publishedNode = q("latestPublishedAt");
    const changelogNode = q("latestChangelog");

    if (!version || !version.id) {
      if (titleNode) titleNode.textContent = "暂无可用稳定版";
      if (metaNode) metaNode.textContent = "当前未检测到可下载的稳定版本。";
      if (publishedNode) publishedNode.textContent = "发布时间：--";
      if (changelogNode) changelogNode.textContent = "暂无更新日志。";
      setDownloadLink("latestStableDownload", "");
      return;
    }

    if (titleNode) titleNode.textContent = safeText(version.version, "-");
    if (metaNode) metaNode.textContent = safeText(version.description, "推荐用于正式环境。");

    const published = version.publishedAt || version.updatedAt;
    if (publishedNode) {
      publishedNode.textContent = `发布时间：${published ? formatDate(published) : "未知"}`;
    }

    if (changelogNode) {
      changelogNode.textContent = safeText(version.changelog, "暂无更新日志。");
    }

    setDownloadLink("latestStableDownload", `/api/public/download/version/${encodeURIComponent(version.id)}`);
  }

  function createPluginCard(item, index) {
    const card = document.createElement("article");
    card.className = "mm-plugin-card mm-animated";
    if (index > 0 && index < 4) card.classList.add(`mm-stagger-${index}`);

    const title = document.createElement("h3");
    title.textContent = `${safeText(item.name)} · ${safeText(item.version)}`;

    const desc = document.createElement("p");
    desc.textContent = clipText(item.description, 180);

    const info = document.createElement("p");
    info.textContent = `更新时间：${formatDate(item.updatedAt || item.createdAt)}`;

    const actions = document.createElement("div");
    actions.className = "mm-card-actions";

    const download = document.createElement("a");
    download.className = "mm-link-ghost";
    download.href = `/api/public/download/plugin/${encodeURIComponent(item.id)}`;
    download.textContent = "下载插件";

    actions.appendChild(download);
    card.append(title, desc, info, actions);
    return card;
  }

  function createEmptyCard(text) {
    const card = document.createElement("article");
    card.className = "mm-empty-card mm-animated";

    const title = document.createElement("h3");
    title.textContent = "暂无插件";

    const desc = document.createElement("p");
    desc.textContent = text;

    card.append(title, desc);
    return card;
  }

  function renderPluginList(plugins) {
    const list = q("pluginList");
    if (!list) return;

    list.innerHTML = "";
    const sorted = [...plugins].sort((a, b) => new Date(b.updatedAt || 0).getTime() - new Date(a.updatedAt || 0).getTime());

    if (!sorted.length) {
      list.appendChild(createEmptyCard("当前暂无可下载插件。"));
    } else {
      sorted.forEach((item, index) => {
        list.appendChild(createPluginCard(item, index));
      });
    }

    const totalNode = q("pluginTotalInfo");
    if (totalNode) totalNode.textContent = `共 ${sorted.length} 项`;

    observeDynamic(list);
  }

  async function loadData() {
    const [stableRes, pluginsRes] = await Promise.allSettled([
      fetchJSON("/api/public/versions/latest?channel=stable"),
      fetchJSON("/api/public/plugins"),
    ]);

    const stable = stableRes.status === "fulfilled" ? stableRes.value : null;
    const plugins = pluginsRes.status === "fulfilled" && Array.isArray(pluginsRes.value) ? pluginsRes.value : [];

    setLatestStable(stable);
    renderPluginList(plugins);
  }

  document.addEventListener("DOMContentLoaded", () => {
    loadData().catch((error) => console.error(error));
  });
})();
