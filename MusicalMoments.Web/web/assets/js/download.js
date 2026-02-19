(function () {
  const { q, fetchJSON, formatDate, formatBytes, setDownloadLink, observeDynamic } = window.MMWeb;

  function safeText(value, fallback = "-") {
    const text = String(value ?? "").trim();
    return text || fallback;
  }

  function clipText(value, max = 180) {
    const text = safeText(value, "");
    if (!text) return "暂无说明";
    if (text.length <= max) return text;
    return `${text.slice(0, max - 1)}...`;
  }

  function sortByDateDesc(list, ...keys) {
    return [...list].sort((a, b) => {
      const left = keys.map((key) => a?.[key]).find(Boolean);
      const right = keys.map((key) => b?.[key]).find(Boolean);
      return new Date(right || 0).getTime() - new Date(left || 0).getTime();
    });
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

    if (titleNode) {
      titleNode.textContent = `${safeText(version.version, "-")} · ${safeText(version.title, "未命名版本")}`;
    }
    if (metaNode) {
      const desc = safeText(version.description, "推荐用于正式环境。");
      metaNode.textContent = `${desc}`;
    }

    const published = version.publishedAt || version.updatedAt;
    const updated = version.updatedAt || version.publishedAt;
    if (publishedNode) {
      const publishedText = published ? formatDate(published) : "未知";
      const updatedText = updated ? formatDate(updated) : "未知";
      publishedNode.textContent = `发布时间：${publishedText} · 更新时间：${updatedText}`;
    }

    if (changelogNode) {
      changelogNode.textContent = safeText(version.changelog, "暂无更新日志。");
    }

    setDownloadLink("latestStableDownload", `/api/public/download/version/${encodeURIComponent(version.id)}`);
  }

  function createHistoryChangelog(changelogText) {
    const details = document.createElement("details");
    details.className = "mm-history-log";

    const summary = document.createElement("summary");
    summary.textContent = "查看更新日志";

    const pre = document.createElement("pre");
    pre.textContent = changelogText;

    details.append(summary, pre);
    return details;
  }

  function createVersionHistoryItem(item, latestId, index) {
    const card = document.createElement("article");
    card.className = "mm-history-item mm-animated";
    if (index > 0 && index < 4) card.classList.add(`mm-stagger-${index}`);
    if (item.id === latestId) card.classList.add("is-latest");

    const head = document.createElement("div");
    head.className = "mm-history-head";

    const title = document.createElement("h3");
    title.className = "mm-history-title";
    title.textContent = `${safeText(item.version)} · ${safeText(item.title, "未命名版本")}`;

    const badges = document.createElement("div");
    badges.className = "mm-history-badges";

    const stableBadge = document.createElement("span");
    stableBadge.className = "mm-history-badge";
    stableBadge.textContent = safeText(item.track || "stable", "stable");
    badges.appendChild(stableBadge);

    if (item.id === latestId) {
      const latestBadge = document.createElement("span");
      latestBadge.className = "mm-history-badge latest";
      latestBadge.textContent = "最新推荐";
      badges.appendChild(latestBadge);
    }

    head.append(title, badges);

    const desc = document.createElement("p");
    desc.className = "mm-history-desc";
    desc.textContent = clipText(item.description || item.changelog, 220);

    const meta = document.createElement("div");
    meta.className = "mm-history-meta";

    const published = document.createElement("span");
    published.textContent = `发布时间：${formatDate(item.publishedAt || item.updatedAt)}`;

    const size = document.createElement("span");
    size.textContent = `文件大小：${formatBytes(item.fileSize)}`;

    const hash = document.createElement("span");
    hash.textContent = item.sha256 ? `SHA256：${item.sha256.slice(0, 12)}...` : "SHA256：-";

    meta.append(published, size, hash);

    const actions = document.createElement("div");
    actions.className = "mm-history-actions";

    const download = document.createElement("a");
    download.className = "mm-link-ghost";
    download.href = `/api/public/download/version/${encodeURIComponent(item.id)}`;
    download.textContent = "下载此版本";

    actions.appendChild(download);

    card.append(head, desc, meta, actions);

    const changelogText = safeText(item.changelog, "");
    if (changelogText) {
      card.appendChild(createHistoryChangelog(changelogText.slice(0, 12000)));
    }
    return card;
  }

  function createEmptyCard(titleText, descText) {
    const card = document.createElement("article");
    card.className = "mm-empty-card mm-animated";

    const title = document.createElement("h3");
    title.textContent = titleText;

    const desc = document.createElement("p");
    desc.textContent = descText;

    card.append(title, desc);
    return card;
  }

  function renderVersionHistory(versions, latestId) {
    const list = q("versionHistoryList");
    if (!list) return;

    list.innerHTML = "";

    if (!versions.length) {
      list.appendChild(createEmptyCard("暂无稳定版记录", "当前还没有可展示的稳定版历史。"));
    } else {
      versions.forEach((item, index) => {
        list.appendChild(createVersionHistoryItem(item, latestId, index));
      });
    }

    const totalNode = q("versionHistoryTotal");
    if (totalNode) totalNode.textContent = `共 ${versions.length} 条`;

    observeDynamic(list);
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

  function renderPluginList(plugins) {
    const list = q("pluginList");
    if (!list) return;

    list.innerHTML = "";
    const sorted = sortByDateDesc(plugins, "updatedAt", "createdAt");

    if (!sorted.length) {
      list.appendChild(createEmptyCard("暂无插件", "当前暂无可下载插件。"));
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
    const [latestRes, versionsRes, pluginsRes] = await Promise.allSettled([
      fetchJSON("/api/public/versions/latest?channel=stable"),
      fetchJSON("/api/public/versions"),
      fetchJSON("/api/public/plugins"),
    ]);

    const latest = latestRes.status === "fulfilled" ? latestRes.value : null;
    const versionsPayload = versionsRes.status === "fulfilled" ? versionsRes.value : { versions: [], latestStableId: "" };
    const plugins = pluginsRes.status === "fulfilled" && Array.isArray(pluginsRes.value) ? pluginsRes.value : [];

    const versions = Array.isArray(versionsPayload?.versions) ? versionsPayload.versions : [];
    const stableHistory = sortByDateDesc(
      versions.filter((item) => String(item.track || "").toLowerCase() === "stable"),
      "publishedAt",
      "updatedAt"
    );

    let latestStableId = String(versionsPayload?.latestStableId || "").trim();
    let resolvedLatest = latest && latest.id ? latest : null;

    if (!resolvedLatest && latestStableId) {
      resolvedLatest = stableHistory.find((item) => item.id === latestStableId) || null;
    }

    if (!resolvedLatest && stableHistory.length) {
      resolvedLatest = stableHistory[0];
      latestStableId = stableHistory[0].id;
    }

    if (!latestStableId && resolvedLatest?.id) {
      latestStableId = resolvedLatest.id;
    }

    setLatestStable(resolvedLatest);
    renderVersionHistory(stableHistory, latestStableId);
    renderPluginList(plugins);
  }

  document.addEventListener("DOMContentLoaded", () => {
    loadData().catch((error) => console.error(error));
  });
})();
