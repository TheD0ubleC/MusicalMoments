
const state = {
  token: sessionStorage.getItem("mm_admin_token") || "",
  versions: [],
  plugins: [],
  latestStableId: "",
  revealObserver: null,
  refreshTimer: null,
  selectedUsageDay: "",
  todayUsageDay: "",
  versionSearch: "",
  pluginSearch: "",
  pluginVersionSearch: "",
  selectedPluginId: "",
  loginLogs: [],
  pagination: {
    versions: { page: 1, size: 8 },
    plugins: { page: 1, size: 8 },
    pluginVersions: { page: 1, size: 8 },
    loginLogs: { page: 1, size: 10 },
  },
  modal: {
    open: false,
    resolve: null,
    closeTimer: null,
  },
  actionModal: {
    open: false,
    closeTimer: null,
  },
};

const MODAL_HIDE_DELAY_MS = 260;

const q = (selector) => document.querySelector(selector);
const qs = (selector) => Array.from(document.querySelectorAll(selector));

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString("zh-CN", { hour12: false });
}

function formatNumber(value) {
  return Number(value || 0).toLocaleString("zh-CN");
}

function safeText(value, fallback = "-") {
  const text = String(value ?? "").trim();
  return text || fallback;
}

function clipText(value, max = 54) {
  const text = safeText(value, "");
  if (!text) return "-";
  if (text.length <= max) return text;
  return `${text.slice(0, max - 1)}…`;
}

function setToken(token) {
  state.token = token || "";
  if (state.token) {
    sessionStorage.setItem("mm_admin_token", state.token);
  } else {
    sessionStorage.removeItem("mm_admin_token");
  }
}

function setButtonBusy(button, busy, busyText = "处理中...") {
  if (!(button instanceof HTMLButtonElement)) return;
  if (busy) {
    if (!button.dataset.defaultLabel) button.dataset.defaultLabel = button.textContent || "";
    button.disabled = true;
    button.textContent = busyText;
    return;
  }
  button.disabled = false;
  if (button.dataset.defaultLabel) button.textContent = button.dataset.defaultLabel;
}

function showMessage(message, isError = false) {
  const loginHint = q("#loginHint");
  if (loginHint && !q("#loginPanel")?.classList.contains("is-hidden")) {
    loginHint.textContent = message;
    loginHint.style.color = isError ? "#f3849e" : "#1f9368";
  }

  const toast = q("#globalToast");
  if (!toast) return;
  toast.textContent = message;
  toast.classList.toggle("error", isError);
  toast.classList.add("show");

  clearTimeout(showMessage._timer);
  showMessage._timer = setTimeout(() => toast.classList.remove("show"), 2500);
}

function setupReveal() {
  const nodes = qs(".mm-animated");
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
    { threshold: 0.15 }
  );

  nodes.forEach((node) => state.revealObserver.observe(node));
}

function observeAnimated(root) {
  if (!root || !state.revealObserver) return;
  root.querySelectorAll(".mm-animated").forEach((node) => state.revealObserver.observe(node));
}

function animateCount(id, targetValue) {
  const el = q(`#${id}`);
  if (!el) return;

  const target = Number(targetValue || 0);
  const from = Number(el.dataset.current || 0);
  const start = performance.now();
  const duration = 650;

  const step = (now) => {
    const progress = Math.min(1, (now - start) / duration);
    const eased = 1 - Math.pow(1 - progress, 3);
    const current = Math.round(from + (target - from) * eased);
    el.textContent = formatNumber(current);
    if (progress < 1) {
      requestAnimationFrame(step);
    } else {
      el.dataset.current = String(target);
    }
  };
  requestAnimationFrame(step);
}

async function apiFetch(url, options = {}) {
  const headers = new Headers(options.headers || {});
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  if (!(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json; charset=utf-8");
  }

  const response = await fetch(url, { ...options, headers, cache: "no-store" });
  const payload = await response.json().catch(() => ({ ok: false, message: "响应解析失败" }));
  if (!response.ok || payload?.ok === false) {
    throw new Error(payload?.message || `请求失败: ${response.status}`);
  }
  return payload.data;
}

function setLoggedIn(loggedIn) {
  q("#loginPanel")?.classList.toggle("is-hidden", loggedIn);
  q("#adminApp")?.classList.toggle("is-hidden", !loggedIn);
  const logoutBtn = q("#logoutBtn");
  if (logoutBtn) logoutBtn.disabled = !loggedIn;
  if (loggedIn) observeAnimated(q("#adminApp"));
}

function switchPane(targetId) {
  qs(".admin-nav-btn").forEach((btn) => btn.classList.toggle("active", btn.dataset.target === targetId));
  qs(".admin-pane").forEach((pane) => pane.classList.toggle("active", pane.id === targetId));
}

function setPageInfo(kind, totalCount, totalPages, currentPage) {
  const map = {
    versions: { info: "#versionPageInfo", prev: "#versionPrevBtn", next: "#versionNextBtn" },
    plugins: { info: "#pluginPageInfo", prev: "#pluginPrevBtn", next: "#pluginNextBtn" },
    pluginVersions: { info: "#pluginVersionPageInfo", prev: "#pluginVersionPrevBtn", next: "#pluginVersionNextBtn" },
    loginLogs: { info: "#loginLogsPageInfo", prev: "#loginLogsPrevBtn", next: "#loginLogsNextBtn" },
  };
  const target = map[kind];
  if (!target) return;
  const infoNode = q(target.info);
  if (infoNode) infoNode.textContent = `第 ${currentPage} / ${totalPages} 页 · 共 ${formatNumber(totalCount)} 条`;
  const prevBtn = q(target.prev);
  const nextBtn = q(target.next);
  if (prevBtn) prevBtn.disabled = currentPage <= 1;
  if (nextBtn) nextBtn.disabled = currentPage >= totalPages;
}

function paginateList(kind, fullList) {
  const pager = state.pagination[kind];
  if (!pager) return fullList;
  const total = fullList.length;
  const totalPages = Math.max(1, Math.ceil(total / pager.size));
  pager.page = Math.max(1, Math.min(pager.page, totalPages));
  const start = (pager.page - 1) * pager.size;
  setPageInfo(kind, total, totalPages, pager.page);
  return fullList.slice(start, start + pager.size);
}

function movePage(kind, delta) {
  const pager = state.pagination[kind];
  if (!pager) return;
  pager.page = Math.max(1, pager.page + delta);
}

function createActionButton(action, id, text, variant = "", disabled = false) {
  const btn = document.createElement("button");
  btn.type = "button";
  btn.className = `admin-action-btn ${variant}`.trim();
  btn.dataset.action = action;
  btn.dataset.id = id;
  btn.textContent = text;
  btn.disabled = Boolean(disabled);
  return btn;
}

function createActionMenuTrigger(action, id) {
  return createActionButton(action, id, "操作", "menu");
}
function renderRouteRows(routeMap) {
  const tbody = q("#adminTopRoutes");
  if (!tbody) return;
  tbody.innerHTML = "";

  const rows = Object.entries(routeMap || {})
    .sort((a, b) => Number(b[1]) - Number(a[1]))
    .slice(0, 15);

  if (!rows.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 2;
    td.textContent = "暂无路由数据";
    tr.appendChild(td);
    tbody.appendChild(tr);
    return;
  }

  rows.forEach(([route, hits]) => {
    const tr = document.createElement("tr");
    const routeTd = document.createElement("td");
    const hitsTd = document.createElement("td");
    routeTd.textContent = route;
    hitsTd.textContent = formatNumber(hits);
    tr.append(routeTd, hitsTd);
    tbody.appendChild(tr);
  });
}

function renderLoginLogs(logs) {
  state.loginLogs = Array.isArray(logs) ? logs : [];

  const tbody = q("#adminLoginLogs");
  if (!tbody) return;
  tbody.innerHTML = "";

  const list = paginateList("loginLogs", state.loginLogs);
  if (!list.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 5;
    td.textContent = "暂无登录审计日志";
    tr.appendChild(td);
    tbody.appendChild(tr);
    return;
  }

  list.forEach((item) => {
    const tr = document.createElement("tr");

    const timeTd = document.createElement("td");
    timeTd.textContent = formatDate(item?.time);

    const ipTd = document.createElement("td");
    ipTd.textContent = safeText(item?.ip, "-");

    const locationTd = document.createElement("td");
    locationTd.textContent = safeText(item?.location, "未知");

    const statusTd = document.createElement("td");
    const statusTag = document.createElement("span");
    statusTag.className = `admin-log-tag ${item?.success ? "success" : "fail"}`;
    statusTag.textContent = item?.success ? "成功" : "失败";
    statusTd.appendChild(statusTag);

    const reasonTd = document.createElement("td");
    reasonTd.textContent = item?.success ? "认证通过" : safeText(item?.reason, "未知原因");

    tr.append(timeTd, ipTd, locationTd, statusTd, reasonTd);
    tbody.appendChild(tr);
  });
}

function syncUsageDaySelect(usage) {
  const select = q("#usageDaySelect");
  if (!select) return;

  const daySet = new Set();
  if (usage?.today) daySet.add(usage.today);
  (usage?.recentDays || []).forEach((day) => daySet.add(day));

  const days = Array.from(daySet).sort((a, b) => (a < b ? 1 : -1));
  if (!days.length) days.push(new Date().toISOString().slice(0, 10));

  if (!state.selectedUsageDay || !days.includes(state.selectedUsageDay)) {
    state.selectedUsageDay = usage?.selectedDay || usage?.today || days[0];
  }

  select.innerHTML = "";
  days.forEach((day) => {
    const option = document.createElement("option");
    option.value = day;
    option.textContent = day === usage?.today ? `今天 (${day})` : day;
    select.appendChild(option);
  });
  select.value = state.selectedUsageDay;
}

function renderUsage(usage) {
  if (!usage) return;
  state.todayUsageDay = usage.today || state.todayUsageDay;
  if (!state.selectedUsageDay) state.selectedUsageDay = usage.selectedDay || usage.today || "";

  syncUsageDaySelect(usage);

  animateCount("dashOnlineNow", usage.onlineNow || 0);
  animateCount("dashTodayOnlinePeak", usage.todayUsage?.onlinePeak || 0);
  animateCount("dashSelectedOnlinePeak", usage.dayUsage?.onlinePeak || 0);
  animateCount("dashPlayedTotal", usage.totals?.playedCount || 0);
  animateCount("dashCloseTotal", usage.totals?.closeCount || 0);
  animateCount("dashChangedTotal", usage.totals?.streamChangedCount || 0);
  animateCount("dashPlayedDay", usage.dayUsage?.playedCount || 0);
  animateCount("dashCloseDay", usage.dayUsage?.closeCount || 0);
  animateCount("dashChangedDay", usage.dayUsage?.streamChangedCount || 0);
}

function renderDashboard(data) {
  const traffic = data?.traffic || {};

  animateCount("dashTotalRequests", traffic.totalRequests || 0);
  animateCount("dashPageViews", traffic.pageViews || 0);
  animateCount("dashApiRequests", traffic.apiRequests || 0);
  animateCount("dashDownloads", traffic.downloadRequests || 0);
  animateCount("dashApiDownloads", traffic.apiDownloads || 0);
  animateCount("dashBytesServed", Math.round((traffic.bytesServed || 0) / 1024 / 1024));

  const versionCountNode = q("#dashVersionCount");
  const pluginCountNode = q("#dashPluginCount");
  const pointersNode = q("#dashLatestPointers");
  if (versionCountNode) versionCountNode.textContent = formatNumber(data?.versionCount || 0);
  if (pluginCountNode) pluginCountNode.textContent = formatNumber(data?.pluginCount || 0);
  if (pointersNode) pointersNode.textContent = `stable:${data?.latestStableId || "-"}`;

  renderRouteRows(traffic.routeHits || {});
  renderUsage(data?.usage || null);
}
function filterVersions(list) {
  const term = state.versionSearch.trim().toLowerCase();
  if (!term) return [...list];
  return list.filter((item) =>
    [item.id, item.version, item.title, item.description, item.changelog].some((field) =>
      String(field || "").toLowerCase().includes(term)
    )
  );
}

function fillVersionForm(item) {
  q("#versionId").value = item?.id || "";
  q("#versionTrack").value = item?.track || "stable";
  q("#versionNumber").value = item?.version || "";
  q("#versionTitle").value = item?.title || "";
  q("#versionDownloadURL").value = item?.downloadURL || "";
  q("#versionSha256").value = item?.sha256 || "";
  q("#versionFileSize").value = item?.fileSize || 0;
  q("#versionDescription").value = item?.description || "";
  q("#versionChangelog").value = item?.changelog || "";
  q("#versionSetLatest").checked = false;
}

function renderVersions(payload) {
  state.versions = Array.isArray(payload?.versions) ? payload.versions : state.versions;
  state.latestStableId = payload?.latestStableId || state.latestStableId;

  const stableIdNode = q("#latestStableId");
  if (stableIdNode) stableIdNode.textContent = state.latestStableId || "-";

  const body = q("#versionTableBody");
  if (!body) return;
  body.innerHTML = "";

  const filtered = filterVersions(state.versions);
  const list = paginateList("versions", filtered);

  if (!list.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 6;
    td.textContent = "暂无匹配版本";
    tr.appendChild(td);
    body.appendChild(tr);
    return;
  }

  list.forEach((item) => {
    const tr = document.createElement("tr");

    const idTd = document.createElement("td");
    idTd.textContent = clipText(item.id, 18);
    idTd.title = safeText(item.id, "");

    const versionTd = document.createElement("td");
    versionTd.textContent = safeText(item.version, "-");

    const titleTd = document.createElement("td");
    titleTd.textContent = clipText(item.title, 32);
    titleTd.title = safeText(item.title, "");

    const publishedTd = document.createElement("td");
    publishedTd.textContent = formatDate(item.publishedAt);

    const updatedTd = document.createElement("td");
    updatedTd.textContent = formatDate(item.updatedAt);

    const actionTd = document.createElement("td");
    const wrap = document.createElement("div");
    wrap.className = "admin-action-wrap";
    wrap.append(createActionMenuTrigger("open-version-actions", item.id));
    actionTd.appendChild(wrap);

    tr.append(idTd, versionTd, titleTd, publishedTd, updatedTd, actionTd);
    body.appendChild(tr);
  });
}

function normalizePluginList(data) {
  if (!Array.isArray(data)) return [];
  return data
    .map((item) => {
      const versions = Array.isArray(item?.versions) ? [...item.versions] : [];
      versions.sort((a, b) => {
        const left = new Date(a?.publishedAt || a?.updatedAt || 0).getTime();
        const right = new Date(b?.publishedAt || b?.updatedAt || 0).getTime();
        return right - left;
      });
      return { ...item, versions };
    })
    .sort((a, b) => new Date(b?.updatedAt || 0).getTime() - new Date(a?.updatedAt || 0).getTime());
}

function getPluginLatestVersion(plugin) {
  if (!plugin) return null;
  if (Array.isArray(plugin.versions) && plugin.versions.length) return plugin.versions[0];
  if (plugin.latestVersion && typeof plugin.latestVersion === "object") return plugin.latestVersion;
  return null;
}

function getSelectedPlugin() {
  return state.plugins.find((item) => item.id === state.selectedPluginId) || null;
}

function fillPluginBaseForm(item) {
  q("#pluginBaseId").value = item?.id || "";
  q("#pluginBaseName").value = item?.name || "";
  q("#pluginBaseDescription").value = item?.description || "";
}

function fillPluginVersionForm(item) {
  q("#pluginVersionId").value = item?.id || "";
  q("#pluginVersionNumber").value = item?.version || "";
  q("#pluginVersionPublishedAt").value = item?.publishedAt ? new Date(item.publishedAt).toISOString() : "";
  q("#pluginVersionDownloadURL").value = item?.downloadURL || "";
  q("#pluginVersionSha256").value = item?.sha256 || "";
  q("#pluginVersionFileSize").value = item?.fileSize || 0;
  q("#pluginVersionChangelog").value = item?.changelog || "";
}

function filterPlugins(list) {
  const term = state.pluginSearch.trim().toLowerCase();
  if (!term) return [...list];
  return list.filter((item) => {
    const latest = getPluginLatestVersion(item);
    return [item.id, item.name, item.description, latest?.version].some((field) =>
      String(field || "").toLowerCase().includes(term)
    );
  });
}

function filterPluginVersions(list) {
  const term = state.pluginVersionSearch.trim().toLowerCase();
  if (!term) return [...list];
  return list.filter((item) =>
    [item.id, item.version, item.changelog, item.downloadURL].some((field) =>
      String(field || "").toLowerCase().includes(term)
    )
  );
}
function updatePluginSelectionUI() {
  const selected = getSelectedPlugin();
  const selectedNode = q("#pluginSelectedName");
  const hintNode = q("#pluginVersionHint");
  const selectionCard = q("#pluginSelectionCard");

  if (selectedNode) {
    selectedNode.textContent = selected
      ? `当前插件: ${safeText(selected.name, "未命名")} (${safeText(selected.id, "-")})`
      : "当前插件: 未选择";
  }

  if (hintNode) {
    const latest = getPluginLatestVersion(selected);
    hintNode.textContent = selected
      ? `已选插件：${safeText(selected.name, "-")} · 最新版本：${safeText(latest?.version, "暂无")}`
      : "请先在上方选择插件";
  }

  if (selectionCard) {
    const latest = getPluginLatestVersion(selected);
    selectionCard.textContent = selected
      ? `已选择 ${safeText(selected.name, "-")}（${safeText(selected.id, "-")}）· 版本数 ${formatNumber((selected.versions || []).length)} · 最新版本 ${safeText(latest?.version, "暂无")}`
      : "当前未选中插件。点击插件列表行即可切换。";
    selectionCard.classList.toggle("active", Boolean(selected));
  }

  const versionForm = q("#pluginVersionForm");
  if (versionForm) {
    versionForm.querySelectorAll("input,textarea,button").forEach((node) => {
      if (!(node instanceof HTMLInputElement || node instanceof HTMLTextAreaElement || node instanceof HTMLButtonElement)) {
        return;
      }
      if (node.id === "pluginVersionFormReset") {
        node.disabled = !selected;
        return;
      }
      node.disabled = !selected;
    });
  }

  const createBtn = q("#pluginVersionCreateBtn");
  if (createBtn instanceof HTMLButtonElement) createBtn.disabled = !selected;

  const versionSearch = q("#pluginVersionSearchInput");
  if (versionSearch instanceof HTMLInputElement) versionSearch.disabled = !selected;
}

function setSelectedPlugin(pluginID) {
  const normalized = String(pluginID || "").trim();
  const exists = state.plugins.some((item) => item.id === normalized);
  state.selectedPluginId = exists ? normalized : "";
  document.querySelectorAll("#pluginTableBody tr[data-plugin-id]").forEach((row) => {
    if (!(row instanceof HTMLTableRowElement)) return;
    row.classList.toggle("is-selected", row.dataset.pluginId === state.selectedPluginId);
  });
  state.pagination.pluginVersions.page = 1;
  state.pluginVersionSearch = "";
  const searchInput = q("#pluginVersionSearchInput");
  if (searchInput) searchInput.value = "";
  fillPluginVersionForm(null);
  renderPluginVersions();
}

function renderPlugins(data = state.plugins) {
  state.plugins = normalizePluginList(data);
  if (!state.selectedPluginId && state.plugins.length) state.selectedPluginId = state.plugins[0].id;
  if (state.selectedPluginId && !state.plugins.some((item) => item.id === state.selectedPluginId)) {
    state.selectedPluginId = state.plugins[0]?.id || "";
  }

  const body = q("#pluginTableBody");
  if (!body) return;
  body.innerHTML = "";

  const filtered = filterPlugins(state.plugins);
  const list = paginateList("plugins", filtered);

  if (!list.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 6;
    td.textContent = "暂无匹配插件";
    tr.appendChild(td);
    body.appendChild(tr);
  } else {
    list.forEach((item) => {
      const tr = document.createElement("tr");
      tr.dataset.pluginId = item.id;
      tr.classList.toggle("is-selected", item.id === state.selectedPluginId);

      const idTd = document.createElement("td");
      idTd.textContent = clipText(item.id, 18);
      idTd.title = safeText(item.id, "");

      const nameTd = document.createElement("td");
      nameTd.textContent = safeText(item.name, "-");

      const countTd = document.createElement("td");
      countTd.textContent = formatNumber(Array.isArray(item.versions) ? item.versions.length : 0);

      const latestTd = document.createElement("td");
      latestTd.textContent = safeText(getPluginLatestVersion(item)?.version, "暂无版本");

      const updatedTd = document.createElement("td");
      updatedTd.textContent = formatDate(item.updatedAt || item.createdAt);

      const actionTd = document.createElement("td");
      const wrap = document.createElement("div");
      wrap.className = "admin-action-wrap";
      wrap.append(createActionMenuTrigger("open-plugin-actions", item.id));
      actionTd.appendChild(wrap);

      tr.append(idTd, nameTd, countTd, latestTd, updatedTd, actionTd);
      body.appendChild(tr);
    });
  }

  updatePluginSelectionUI();
  renderPluginVersions();
}

function renderPluginVersions() {
  const selected = getSelectedPlugin();
  const body = q("#pluginVersionTableBody");
  if (!body) return;
  body.innerHTML = "";

  updatePluginSelectionUI();

  if (!selected) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 6;
    td.textContent = "请先在上方插件列表选择一个插件。";
    tr.appendChild(td);
    body.appendChild(tr);
    setPageInfo("pluginVersions", 0, 1, 1);
    return;
  }

  const filtered = filterPluginVersions(Array.isArray(selected.versions) ? selected.versions : []);
  const list = paginateList("pluginVersions", filtered);

  if (!list.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 6;
    td.textContent = "该插件暂无匹配版本。";
    tr.appendChild(td);
    body.appendChild(tr);
    return;
  }

  list.forEach((item) => {
    const tr = document.createElement("tr");

    const idTd = document.createElement("td");
    idTd.textContent = clipText(item.id, 18);
    idTd.title = safeText(item.id, "");

    const versionTd = document.createElement("td");
    versionTd.textContent = safeText(item.version, "-");

    const publishedTd = document.createElement("td");
    publishedTd.textContent = formatDate(item.publishedAt);

    const updatedTd = document.createElement("td");
    updatedTd.textContent = formatDate(item.updatedAt);

    const fileTd = document.createElement("td");
    fileTd.textContent = item.sha256
      ? `${formatNumber(item.fileSize || 0)} B · ${item.sha256.slice(0, 10)}...`
      : `${formatNumber(item.fileSize || 0)} B`;

    const actionTd = document.createElement("td");
    const wrap = document.createElement("div");
    wrap.className = "admin-action-wrap";
    wrap.append(createActionMenuTrigger("open-plugin-version-actions", item.id));
    actionTd.appendChild(wrap);

    tr.append(idTd, versionTd, publishedTd, updatedTd, fileTd, actionTd);
    body.appendChild(tr);
  });
}
async function copyToClipboard(text, okMessage) {
  const value = safeText(text, "");
  if (!value) {
    showMessage("没有可复制的内容。", true);
    return;
  }

  try {
    if (navigator.clipboard?.writeText) {
      await navigator.clipboard.writeText(value);
    } else {
      const input = document.createElement("textarea");
      input.value = value;
      input.style.position = "fixed";
      input.style.opacity = "0";
      document.body.appendChild(input);
      input.focus();
      input.select();
      document.execCommand("copy");
      document.body.removeChild(input);
    }
    showMessage(okMessage || "复制成功。");
  } catch {
    showMessage("复制失败，请手动复制。", true);
  }
}

function openActionModal({ title, subtitle, kind, id, actions }) {
  const modal = q("#adminActionModal");
  const titleNode = q("#adminActionModalTitle");
  const subtitleNode = q("#adminActionModalSubtitle");
  const bodyNode = q("#adminActionModalBody");
  if (!modal || !bodyNode) return;

  if (state.actionModal.closeTimer) {
    clearTimeout(state.actionModal.closeTimer);
    state.actionModal.closeTimer = null;
  }

  if (titleNode) titleNode.textContent = title || "操作菜单";
  if (subtitleNode) subtitleNode.textContent = subtitle || "请选择操作项";

  bodyNode.innerHTML = "";
  (actions || []).forEach((item) => {
    const btn = createActionButton(item.action, id, item.text, item.variant || "", Boolean(item.disabled));
    btn.dataset.actionKind = kind || "";
    bodyNode.appendChild(btn);
  });

  modal.classList.remove("is-hidden");
  modal.classList.remove("is-open");
  modal.setAttribute("aria-hidden", "false");
  requestAnimationFrame(() => {
    modal.classList.add("is-open");
  });
  state.actionModal.open = true;
}

function closeActionModal() {
  const modal = q("#adminActionModal");
  if (!modal || !state.actionModal.open) return;

  state.actionModal.open = false;
  modal.classList.remove("is-open");
  if (state.actionModal.closeTimer) {
    clearTimeout(state.actionModal.closeTimer);
  }
  state.actionModal.closeTimer = window.setTimeout(() => {
    modal.classList.add("is-hidden");
    modal.setAttribute("aria-hidden", "true");
    state.actionModal.closeTimer = null;
  }, MODAL_HIDE_DELAY_MS);
}

function openVersionActionMenu(id) {
  const selected = state.versions.find((item) => item.id === id);
  if (!selected) {
    showMessage("未找到对应版本。", true);
    return;
  }

  const isStable = String(selected.track || "") === "stable";
  const isLatest = selected.id === state.latestStableId;
  const latestText = isLatest ? "已为最新版本" : isStable ? "设为最新稳定版" : "仅稳定版可设置";

  openActionModal({
    kind: "version",
    id,
    title: `版本 ${safeText(selected.version, "-")}`,
    subtitle: clipText(safeText(selected.title, "未命名版本"), 42),
    actions: [
      { action: "edit-version", text: "编辑版本" },
      { action: "copy-version-url", text: "复制下载链接" },
      { action: "duplicate-version", text: "复制为新版本" },
      {
        action: "set-latest-stable",
        text: latestText,
        variant: !isLatest && isStable ? "warn" : "",
        disabled: isLatest || !isStable,
      },
      { action: "delete-version", text: "删除版本", variant: "danger" },
    ],
  });
}

function openPluginActionMenu(id) {
  const selected = state.plugins.find((item) => item.id === id);
  if (!selected) {
    showMessage("未找到对应插件。", true);
    return;
  }

  openActionModal({
    kind: "plugin",
    id,
    title: `插件 ${safeText(selected.name, "-")}`,
    subtitle: clipText(safeText(selected.description, "请选择操作项"), 42),
    actions: [
      { action: "manage-plugin", text: "管理版本" },
      { action: "edit-plugin", text: "编辑插件信息" },
      { action: "delete-plugin", text: "删除插件", variant: "danger" },
    ],
  });
}

function openPluginVersionActionMenu(id) {
  const selectedPlugin = getSelectedPlugin();
  const selectedVersion = (selectedPlugin?.versions || []).find((item) => item.id === id);
  if (!selectedVersion) {
    showMessage("未找到对应插件版本。", true);
    return;
  }

  openActionModal({
    kind: "pluginVersion",
    id,
    title: `版本 ${safeText(selectedVersion.version, "-")}`,
    subtitle: `插件：${safeText(selectedPlugin?.name, "-")}`,
    actions: [
      { action: "edit-plugin-version", text: "编辑版本" },
      { action: "copy-plugin-version-url", text: "复制下载链接" },
      { action: "delete-plugin-version", text: "删除版本", variant: "danger" },
    ],
  });
}

async function runVersionAction(action, id) {
  const selected = state.versions.find((item) => item.id === id);

  try {
    if (action === "edit-version" && selected) {
      fillVersionForm(selected);
      switchPane("versionPane");
      return;
    }

    if (action === "copy-version-url" && selected) {
      await copyToClipboard(selected.downloadURL, "下载链接已复制。");
      return;
    }

    if (action === "duplicate-version" && selected) {
      fillVersionForm({ ...selected, id: "", title: `${safeText(selected.title, "版本")} (复制)` });
      switchPane("versionPane");
      q("#versionNumber")?.focus();
      showMessage("已复制版本信息，请修改后保存。", false);
      return;
    }

    if (action === "delete-version") {
      const ok = await openConfirmModal({
        title: "删除版本",
        message: `确认删除版本 ${id}？删除后不可恢复。`,
        confirmText: "确认删除",
        danger: true,
      });
      if (!ok) return;

      await apiFetch(`/api/admin/versions/${encodeURIComponent(id)}`, { method: "DELETE" });
      await refreshAll();
      showMessage("版本已删除。", false);
      return;
    }

    if (action === "set-latest-stable") {
      if (!selected) {
        showMessage("未找到对应版本。", true);
        return;
      }
      if (selected.id === state.latestStableId) {
        showMessage("该版本已是最新稳定版。", false);
        return;
      }
      if (String(selected.track || "") !== "stable") {
        showMessage("仅稳定版可设置为最新稳定版。", true);
        return;
      }
      await apiFetch(`/api/admin/versions/${encodeURIComponent(id)}/latest?channel=stable`, { method: "POST" });
      await refreshAll();
      showMessage("最新稳定版指针已更新。", false);
    }
  } catch (error) {
    showMessage(error.message || "操作失败。", true);
  }
}

async function runPluginAction(action, id) {
  const selected = state.plugins.find((item) => item.id === id);

  try {
    if (action === "manage-plugin" && selected) {
      setSelectedPlugin(id);
      q("#pluginVersionNumber")?.focus();
      return;
    }

    if (action === "edit-plugin" && selected) {
      fillPluginBaseForm(selected);
      setSelectedPlugin(id);
      q("#pluginBaseName")?.focus();
      return;
    }

    if (action === "delete-plugin") {
      const ok = await openConfirmModal({
        title: "删除插件",
        message: `确认删除插件 ${safeText(selected?.name, id)} 及其全部版本？删除后不可恢复。`,
        confirmText: "确认删除",
        danger: true,
      });
      if (!ok) return;

      await apiFetch(`/api/admin/plugins/${encodeURIComponent(id)}`, { method: "DELETE" });
      if (state.selectedPluginId === id) state.selectedPluginId = "";
      await refreshPlugins();
      showMessage("插件已删除。", false);
    }
  } catch (error) {
    showMessage(error.message || "操作失败。", true);
  }
}

async function runPluginVersionAction(action, id) {
  const plugin = getSelectedPlugin();
  if (!plugin) return;
  const selectedVersion = (plugin.versions || []).find((item) => item.id === id);

  try {
    if (action === "edit-plugin-version" && selectedVersion) {
      fillPluginVersionForm(selectedVersion);
      q("#pluginVersionNumber")?.focus();
      return;
    }

    if (action === "copy-plugin-version-url" && selectedVersion) {
      await copyToClipboard(selectedVersion.downloadURL, "插件版本下载链接已复制。");
      return;
    }

    if (action === "delete-plugin-version") {
      const ok = await openConfirmModal({
        title: "删除插件版本",
        message: `确认删除插件版本 ${safeText(selectedVersion?.version, id)}？删除后不可恢复。`,
        confirmText: "确认删除",
        danger: true,
      });
      if (!ok) return;

      await apiFetch(`/api/admin/plugins/${encodeURIComponent(plugin.id)}/versions/${encodeURIComponent(id)}`, {
        method: "DELETE",
      });
      fillPluginVersionForm(null);
      await refreshPlugins();
      showMessage("插件版本已删除。", false);
    }
  } catch (error) {
    showMessage(error.message || "操作失败。", true);
  }
}

function openConfirmModal({ title, message, confirmText = "确认", danger = false }) {
  const modal = q("#adminModal");
  if (!modal) {
    return Promise.resolve(window.confirm(message || "请确认是否继续"));
  }

  const titleNode = q("#adminModalTitle");
  const messageNode = q("#adminModalMessage");
  const confirmBtn = q("#adminModalConfirm");

  if (titleNode) titleNode.textContent = title || "确认操作";
  if (messageNode) messageNode.textContent = message || "请确认是否继续";
  if (confirmBtn) {
    confirmBtn.textContent = confirmText;
    confirmBtn.classList.toggle("danger", danger);
  }

  if (state.modal.closeTimer) {
    clearTimeout(state.modal.closeTimer);
    state.modal.closeTimer = null;
  }

  modal.classList.remove("is-hidden");
  modal.classList.remove("is-open");
  modal.setAttribute("aria-hidden", "false");
  requestAnimationFrame(() => {
    modal.classList.add("is-open");
  });
  state.modal.open = true;
  return new Promise((resolve) => {
    state.modal.resolve = resolve;
  });
}

function closeConfirmModal(result) {
  const modal = q("#adminModal");
  if (!modal || !state.modal.open) return;

  modal.classList.remove("is-open");
  state.modal.open = false;
  if (state.modal.closeTimer) {
    clearTimeout(state.modal.closeTimer);
  }
  state.modal.closeTimer = window.setTimeout(() => {
    modal.classList.add("is-hidden");
    modal.setAttribute("aria-hidden", "true");
    state.modal.closeTimer = null;
  }, MODAL_HIDE_DELAY_MS);

  const resolver = state.modal.resolve;
  state.modal.resolve = null;
  if (typeof resolver === "function") resolver(Boolean(result));
}

function bindModalEvents() {
  q("#adminModalCancel")?.addEventListener("click", () => closeConfirmModal(false));
  q("#adminModalConfirm")?.addEventListener("click", () => closeConfirmModal(true));
  q("#adminModal")?.addEventListener("click", (event) => {
    if (event.target instanceof Element && event.target.matches("[data-modal-close]")) {
      closeConfirmModal(false);
    }
  });
  q("#adminActionModalCancel")?.addEventListener("click", closeActionModal);
  q("#adminActionModal")?.addEventListener("click", (event) => {
    if (event.target instanceof Element && event.target.matches("[data-action-modal-close]")) {
      closeActionModal();
    }
  });
  q("#adminActionModalBody")?.addEventListener("click", async (event) => {
    if (!(event.target instanceof Element)) return;
    const target = event.target.closest("button[data-action][data-id][data-action-kind]");
    if (!(target instanceof HTMLButtonElement)) return;

    const action = target.dataset.action;
    const id = target.dataset.id;
    const kind = target.dataset.actionKind;
    if (!action || !id || !kind || target.disabled) return;

    closeActionModal();
    window.setTimeout(async () => {
      if (kind === "version") {
        await runVersionAction(action, id);
        return;
      }
      if (kind === "plugin") {
        await runPluginAction(action, id);
        return;
      }
      if (kind === "pluginVersion") {
        await runPluginVersionAction(action, id);
      }
    }, Math.max(0, MODAL_HIDE_DELAY_MS - 30));
  });

  document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") return;

    if (state.actionModal.open) {
      event.preventDefault();
      closeActionModal();
      return;
    }

    if (state.modal.open) {
      event.preventDefault();
      closeConfirmModal(false);
    }
  });
}

function setInputSingleFile(input, file) {
  if (!(input instanceof HTMLInputElement) || !file) return;
  try {
    const transfer = new DataTransfer();
    transfer.items.add(file);
    input.files = transfer.files;
  } catch {
    // fallback
  }
}

function syncDropzoneSelection({ inputSelector, hintSelector, selectedSelector, fileLabelSelector }) {
  const input = q(inputSelector);
  const hint = q(hintSelector);
  const selected = q(selectedSelector);
  const fileLabel = q(fileLabelSelector);

  const hasFile = Boolean(input?.files?.length);
  if (hint) hint.classList.toggle("is-hidden", hasFile);
  if (selected) selected.classList.toggle("is-hidden", !hasFile);
  if (fileLabel) fileLabel.textContent = hasFile ? `已选择: ${input.files[0].name}` : "";
}

function clearDropzoneFile({ inputSelector, hintSelector, selectedSelector, fileLabelSelector }) {
  const input = q(inputSelector);
  if (input) input.value = "";
  syncDropzoneSelection({ inputSelector, hintSelector, selectedSelector, fileLabelSelector });
}

function bindDropzone({ zoneSelector, inputSelector, hintSelector, selectedSelector, fileLabelSelector, clearBtnSelector }) {
  const zone = q(zoneSelector);
  const input = q(inputSelector);
  if (!zone || !input) return;

  const sync = () => syncDropzoneSelection({ inputSelector, hintSelector, selectedSelector, fileLabelSelector });

  zone.addEventListener("click", (event) => {
    if (event.target instanceof HTMLElement && clearBtnSelector && event.target.closest(clearBtnSelector)) return;
    if (event.target === input) return;
    input.click();
  });

  zone.addEventListener("keydown", (event) => {
    if (event.key !== "Enter" && event.key !== " ") return;
    event.preventDefault();
    input.click();
  });

  input.addEventListener("change", sync);

  const clearBtn = clearBtnSelector ? q(clearBtnSelector) : null;
  clearBtn?.addEventListener("click", (event) => {
    event.preventDefault();
    event.stopPropagation();
    clearDropzoneFile({ inputSelector, hintSelector, selectedSelector, fileLabelSelector });
  });

  const prevent = (event) => {
    event.preventDefault();
    event.stopPropagation();
  };

  ["dragenter", "dragover"].forEach((name) => {
    zone.addEventListener(name, (event) => {
      prevent(event);
      zone.classList.add("is-dragover");
    });
  });

  ["dragleave", "drop"].forEach((name) => {
    zone.addEventListener(name, (event) => {
      prevent(event);
      zone.classList.remove("is-dragover");
    });
  });

  zone.addEventListener("drop", (event) => {
    const file = event.dataTransfer?.files?.[0];
    if (!file) return;
    setInputSingleFile(input, file);
    sync();
  });

  sync();
}

async function refreshDashboard() {
  const data = await apiFetch("/api/admin/dashboard", { method: "GET" });
  renderDashboard(data);
}

async function refreshUsage(day = state.selectedUsageDay) {
  const query = day ? `?day=${encodeURIComponent(day)}` : "";
  const data = await apiFetch(`/api/admin/usage${query}`, { method: "GET" });
  state.selectedUsageDay = data?.selectedDay || day || state.selectedUsageDay;
  renderUsage(data);
}

async function refreshLoginLogs() {
  const data = await apiFetch("/api/admin/login-logs?limit=50", { method: "GET" });
  renderLoginLogs(data?.logs || []);
}

async function refreshVersions() {
  const data = await apiFetch("/api/admin/versions", { method: "GET" });
  renderVersions(data);
}

async function refreshPlugins() {
  const data = await apiFetch("/api/admin/plugins", { method: "GET" });
  renderPlugins(data);
}

async function refreshAll() {
  await Promise.all([refreshDashboard(), refreshVersions(), refreshPlugins(), refreshLoginLogs()]);
  if (state.selectedUsageDay && state.todayUsageDay && state.selectedUsageDay !== state.todayUsageDay) {
    await refreshUsage(state.selectedUsageDay);
  }
}

function startAutoRefresh() {
  stopAutoRefresh();
  state.refreshTimer = setInterval(async () => {
    if (!state.token) return;
    try {
      await refreshDashboard();
      await refreshLoginLogs();
      if (state.selectedUsageDay && state.todayUsageDay && state.selectedUsageDay !== state.todayUsageDay) {
        await refreshUsage(state.selectedUsageDay);
      }
    } catch {
      // ignore
    }
  }, 20000);
}

function stopAutoRefresh() {
  if (!state.refreshTimer) return;
  clearInterval(state.refreshTimer);
  state.refreshTimer = null;
}
async function handleLoginSubmit(event) {
  event.preventDefault();
  const submitBtn = event.submitter instanceof HTMLButtonElement
    ? event.submitter
    : q("#adminLoginForm button[type='submit']");

  const fileInput = q("#adminKeyFile");
  if (!fileInput?.files?.length) {
    showMessage("请先选择 key 文件。", true);
    return;
  }

  const formData = new FormData();
  formData.append("keyFile", fileInput.files[0]);

  setButtonBusy(submitBtn, true, "登录中...");
  try {
    const data = await apiFetch("/api/admin/login", { method: "POST", body: formData, headers: {} });
    setToken(data.token);
    q("#tokenExpireAt").textContent = `登录有效期至 ${formatDate(data.expiresAt)}`;

    setLoggedIn(true);
    switchPane("dashboardPane");
    await refreshAll();
    startAutoRefresh();
    showMessage("登录成功。", false);
  } catch (error) {
    setLoggedIn(false);
    showMessage(error.message || "登录失败。", true);
  } finally {
    setButtonBusy(submitBtn, false);
  }
}

async function handleVersionSubmit(event) {
  event.preventDefault();
  const submitBtn = event.submitter instanceof HTMLButtonElement
    ? event.submitter
    : q("#versionForm button[type='submit']");

  const id = q("#versionId").value.trim();
  const payload = {
    id,
    version: q("#versionNumber").value.trim(),
    title: q("#versionTitle").value.trim(),
    track: q("#versionTrack")?.value || "stable",
    downloadURL: q("#versionDownloadURL").value.trim(),
    sha256: q("#versionSha256").value.trim(),
    fileSize: Number(q("#versionFileSize").value || 0),
    description: q("#versionDescription").value.trim(),
    changelog: q("#versionChangelog").value.trim(),
    setAsLatest: q("#versionSetLatest").checked,
  };

  setButtonBusy(submitBtn, true, "保存中...");
  try {
    if (id) {
      await apiFetch(`/api/admin/versions/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(payload) });
    } else {
      await apiFetch("/api/admin/versions", { method: "POST", body: JSON.stringify(payload) });
    }

    fillVersionForm(null);
    await refreshAll();
    switchPane("versionPane");
    showMessage("版本保存成功。", false);
  } catch (error) {
    showMessage(error.message || "版本保存失败。", true);
  } finally {
    setButtonBusy(submitBtn, false);
  }
}

async function handlePluginBaseSubmit(event) {
  event.preventDefault();
  const submitBtn = event.submitter instanceof HTMLButtonElement
    ? event.submitter
    : q("#pluginBaseForm button[type='submit']");

  const id = q("#pluginBaseId").value.trim();
  const payload = {
    id,
    name: q("#pluginBaseName").value.trim(),
    description: q("#pluginBaseDescription").value.trim(),
  };

  setButtonBusy(submitBtn, true, "保存中...");
  try {
    if (id) {
      await apiFetch(`/api/admin/plugins/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(payload) });
    } else {
      await apiFetch("/api/admin/plugins", { method: "POST", body: JSON.stringify(payload) });
    }

    fillPluginBaseForm(null);
    await refreshPlugins();
    showMessage("插件主体保存成功。", false);
  } catch (error) {
    showMessage(error.message || "插件主体保存失败。", true);
  } finally {
    setButtonBusy(submitBtn, false);
  }
}

async function handlePluginVersionSubmit(event) {
  event.preventDefault();
  const selected = getSelectedPlugin();
  if (!selected) {
    showMessage("请先选择插件主体。", true);
    return;
  }

  const submitBtn = event.submitter instanceof HTMLButtonElement
    ? event.submitter
    : q("#pluginVersionForm button[type='submit']");

  const versionID = q("#pluginVersionId").value.trim();
  const payload = {
    id: versionID,
    version: q("#pluginVersionNumber").value.trim(),
    publishedAt: q("#pluginVersionPublishedAt").value.trim(),
    downloadURL: q("#pluginVersionDownloadURL").value.trim(),
    sha256: q("#pluginVersionSha256").value.trim(),
    fileSize: Number(q("#pluginVersionFileSize").value || 0),
    changelog: q("#pluginVersionChangelog").value.trim(),
  };

  setButtonBusy(submitBtn, true, "保存中...");
  try {
    if (versionID) {
      await apiFetch(
        `/api/admin/plugins/${encodeURIComponent(selected.id)}/versions/${encodeURIComponent(versionID)}`,
        { method: "PUT", body: JSON.stringify(payload) }
      );
    } else {
      await apiFetch(`/api/admin/plugins/${encodeURIComponent(selected.id)}/versions`, {
        method: "POST",
        body: JSON.stringify(payload),
      });
    }

    fillPluginVersionForm(null);
    await refreshPlugins();
    showMessage("插件版本保存成功。", false);
  } catch (error) {
    showMessage(error.message || "插件版本保存失败。", true);
  } finally {
    setButtonBusy(submitBtn, false);
  }
}

async function handleUploadSubmit(event) {
  event.preventDefault();
  const submitBtn = event.submitter instanceof HTMLButtonElement
    ? event.submitter
    : q("#uploadForm button[type='submit']");

  const fileInput = q("#uploadFile");
  if (!fileInput?.files?.length) {
    showMessage("请先选择要上传的文件。", true);
    return;
  }

  const kind = q("#uploadKind")?.value || "other";
  const formData = new FormData();
  formData.append("kind", kind);
  formData.append("file", fileInput.files[0]);

  setButtonBusy(submitBtn, true, "上传中...");
  try {
    const result = await apiFetch("/api/admin/upload", { method: "POST", body: formData, headers: {} });

    q("#uploadResult").textContent =
      `上传成功\n` +
      `类型: ${result.kind}\n` +
      `下载地址: ${result.downloadURL}\n` +
      `SHA256: ${result.sha256}\n` +
      `大小: ${formatNumber(result.sizeBytes || 0)} Bytes`;

    if (result.kind === "release") {
      q("#versionDownloadURL").value = result.downloadURL;
      q("#versionSha256").value = result.sha256;
      q("#versionFileSize").value = result.sizeBytes || 0;
      q("#uploadTargetHint").textContent = "已回填到版本管理表单。";
      switchPane("versionPane");
    } else if (result.kind === "plugin") {
      const selected = getSelectedPlugin();
      if (selected) {
        q("#pluginVersionDownloadURL").value = result.downloadURL;
        q("#pluginVersionSha256").value = result.sha256;
        q("#pluginVersionFileSize").value = result.sizeBytes || 0;
        q("#uploadTargetHint").textContent = `已回填到插件 ${safeText(selected.name, "-")} 的版本表单。`;
        switchPane("pluginPane");
      } else {
        q("#uploadTargetHint").textContent = "插件文件已上传成功，请先在插件管理中选择插件后回填。";
      }
    }

    clearDropzoneFile({
      inputSelector: "#uploadFile",
      hintSelector: "#uploadDropHint",
      selectedSelector: "#uploadSelected",
      fileLabelSelector: "#uploadFileName",
    });

    showMessage("上传成功。", false);
  } catch (error) {
    showMessage(error.message || "上传失败。", true);
  } finally {
    setButtonBusy(submitBtn, false);
  }
}
async function handleVersionTableClick(event) {
  if (!(event.target instanceof Element)) return;
  const target = event.target.closest("button[data-action][data-id]");
  if (!(target instanceof HTMLButtonElement)) return;

  const action = target.dataset.action;
  const id = target.dataset.id;
  if (!action || !id) return;

  if (action === "open-version-actions") {
    openVersionActionMenu(id);
    return;
  }

  await runVersionAction(action, id);
}

async function handlePluginTableClick(event) {
  if (!(event.target instanceof Element)) return;
  const target = event.target.closest("button[data-action][data-id]");
  if (!(target instanceof HTMLButtonElement)) {
    const row = event.target.closest("tr[data-plugin-id]");
    if (!(row instanceof HTMLTableRowElement)) return;
    const pluginID = String(row.dataset.pluginId || "").trim();
    if (!pluginID) return;
    if (pluginID !== state.selectedPluginId) {
      setSelectedPlugin(pluginID);
    }
    return;
  }

  const action = target.dataset.action;
  const id = target.dataset.id;
  if (!action || !id) return;

  if (action === "open-plugin-actions") {
    openPluginActionMenu(id);
    return;
  }

  await runPluginAction(action, id);
}

async function handlePluginVersionTableClick(event) {
  if (!(event.target instanceof Element)) return;
  const target = event.target.closest("button[data-action][data-id]");
  if (!(target instanceof HTMLButtonElement)) return;

  const action = target.dataset.action;
  const id = target.dataset.id;
  if (!action || !id) return;

  if (action === "open-plugin-version-actions") {
    openPluginVersionActionMenu(id);
    return;
  }

  await runPluginVersionAction(action, id);
}

async function handleLogout() {
  closeActionModal();
  closeConfirmModal(false);
  try {
    await apiFetch("/api/admin/logout", { method: "POST" });
  } catch {
    // ignore logout network error
  }

  setToken("");
  stopAutoRefresh();
  state.selectedUsageDay = "";
  state.todayUsageDay = "";
  state.versionSearch = "";
  state.pluginSearch = "";
  state.pluginVersionSearch = "";
  state.selectedPluginId = "";
  state.loginLogs = [];

  Object.keys(state.pagination).forEach((key) => {
    state.pagination[key].page = 1;
  });

  setLoggedIn(false);
  q("#tokenExpireAt").textContent = "未登录";
  showMessage("已退出登录。", false);
}

async function verifySession() {
  if (!state.token) {
    setLoggedIn(false);
    return;
  }

  try {
    const data = await apiFetch("/api/admin/me", { method: "GET" });
    q("#tokenExpireAt").textContent = `登录有效期至 ${formatDate(data.expiresAt)}`;
    setLoggedIn(true);
    switchPane("dashboardPane");
    await refreshAll();
    startAutoRefresh();
  } catch {
    setToken("");
    stopAutoRefresh();
    setLoggedIn(false);
    showMessage("登录已过期，请重新上传 key 文件。", true);
  }
}

function bindEvents() {
  q("#adminLoginForm")?.addEventListener("submit", handleLoginSubmit);
  q("#versionForm")?.addEventListener("submit", handleVersionSubmit);
  q("#pluginBaseForm")?.addEventListener("submit", handlePluginBaseSubmit);
  q("#pluginVersionForm")?.addEventListener("submit", handlePluginVersionSubmit);
  q("#uploadForm")?.addEventListener("submit", handleUploadSubmit);

  q("#versionTableBody")?.addEventListener("click", handleVersionTableClick);
  q("#pluginTableBody")?.addEventListener("click", handlePluginTableClick);
  q("#pluginVersionTableBody")?.addEventListener("click", handlePluginVersionTableClick);

  q("#versionFormReset")?.addEventListener("click", () => fillVersionForm(null));
  q("#pluginBaseFormReset")?.addEventListener("click", () => fillPluginBaseForm(null));
  q("#pluginVersionFormReset")?.addEventListener("click", () => fillPluginVersionForm(null));

  q("#versionCreateBtn")?.addEventListener("click", () => {
    fillVersionForm(null);
    q("#versionNumber")?.focus();
  });

  q("#pluginCreateBtn")?.addEventListener("click", () => {
    fillPluginBaseForm(null);
    q("#pluginBaseName")?.focus();
  });

  q("#pluginVersionCreateBtn")?.addEventListener("click", () => {
    if (!getSelectedPlugin()) {
      showMessage("请先选择插件。", true);
      return;
    }
    fillPluginVersionForm(null);
    q("#pluginVersionNumber")?.focus();
  });

  q("#versionSearchInput")?.addEventListener("input", (event) => {
    state.versionSearch = String(event.target?.value || "");
    state.pagination.versions.page = 1;
    renderVersions({ versions: state.versions, latestStableId: state.latestStableId });
  });

  q("#pluginSearchInput")?.addEventListener("input", (event) => {
    state.pluginSearch = String(event.target?.value || "");
    state.pagination.plugins.page = 1;
    renderPlugins(state.plugins);
  });

  q("#pluginVersionSearchInput")?.addEventListener("input", (event) => {
    state.pluginVersionSearch = String(event.target?.value || "");
    state.pagination.pluginVersions.page = 1;
    renderPluginVersions();
  });

  q("#logoutBtn")?.addEventListener("click", handleLogout);

  q("#usageDaySelect")?.addEventListener("change", async () => {
    state.selectedUsageDay = q("#usageDaySelect")?.value || "";
    await refreshUsage(state.selectedUsageDay);
  });

  q("#usageDayRefresh")?.addEventListener("click", async () => {
    await refreshUsage(state.selectedUsageDay);
  });

  qs(".admin-nav-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
      if (btn.dataset.target) switchPane(btn.dataset.target);
    });
  });

  q("#versionPrevBtn")?.addEventListener("click", () => {
    movePage("versions", -1);
    renderVersions({ versions: state.versions, latestStableId: state.latestStableId });
  });
  q("#versionNextBtn")?.addEventListener("click", () => {
    movePage("versions", 1);
    renderVersions({ versions: state.versions, latestStableId: state.latestStableId });
  });
  q("#pluginPrevBtn")?.addEventListener("click", () => {
    movePage("plugins", -1);
    renderPlugins(state.plugins);
  });
  q("#pluginNextBtn")?.addEventListener("click", () => {
    movePage("plugins", 1);
    renderPlugins(state.plugins);
  });
  q("#pluginVersionPrevBtn")?.addEventListener("click", () => {
    movePage("pluginVersions", -1);
    renderPluginVersions();
  });
  q("#pluginVersionNextBtn")?.addEventListener("click", () => {
    movePage("pluginVersions", 1);
    renderPluginVersions();
  });
  q("#loginLogsPrevBtn")?.addEventListener("click", () => {
    movePage("loginLogs", -1);
    renderLoginLogs(state.loginLogs);
  });
  q("#loginLogsNextBtn")?.addEventListener("click", () => {
    movePage("loginLogs", 1);
    renderLoginLogs(state.loginLogs);
  });

  bindDropzone({
    zoneSelector: "#adminKeyDropzone",
    inputSelector: "#adminKeyFile",
    hintSelector: "#adminKeyDropHint",
    selectedSelector: "#adminKeySelected",
    fileLabelSelector: "#adminKeyFilename",
    clearBtnSelector: "#adminKeyClearBtn",
  });

  bindDropzone({
    zoneSelector: "#uploadDropzone",
    inputSelector: "#uploadFile",
    hintSelector: "#uploadDropHint",
    selectedSelector: "#uploadSelected",
    fileLabelSelector: "#uploadFileName",
    clearBtnSelector: "#uploadClearBtn",
  });

  bindModalEvents();
}

document.addEventListener("DOMContentLoaded", async () => {
  setupReveal();
  bindEvents();
  await verifySession();
});
