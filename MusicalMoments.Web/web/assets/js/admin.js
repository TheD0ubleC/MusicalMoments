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
  loginLogs: [],
};

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

function clipText(value, max = 52) {
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

function showMessage(message, isError = false) {
  const loginHint = q("#loginHint");
  if (loginHint && !q("#loginPanel")?.classList.contains("hidden")) {
    loginHint.textContent = message;
    loginHint.style.color = isError ? "#ffd1da" : "#bdf4dc";
  }

  const toast = q("#globalToast");
  if (!toast) return;
  toast.textContent = message;
  toast.classList.toggle("error", isError);
  toast.classList.add("show");

  clearTimeout(showMessage._timer);
  showMessage._timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 2500);
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
  if (!root) return;
  const nodes = root.querySelectorAll(".mm-animated");
  if (!nodes.length) return;

  if (!state.revealObserver) {
    nodes.forEach((node) => node.classList.add("is-visible"));
    return;
  }

  nodes.forEach((node) => state.revealObserver.observe(node));
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
  if (state.token) {
    headers.set("Authorization", `Bearer ${state.token}`);
  }

  if (!(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json; charset=utf-8");
  }

  const response = await fetch(url, {
    ...options,
    headers,
    cache: "no-store",
  });

  const payload = await response.json().catch(() => ({ ok: false, message: "响应解析失败" }));
  if (!response.ok || payload?.ok === false) {
    throw new Error(payload?.message || `请求失败: ${response.status}`);
  }
  return payload.data;
}

function setLoggedIn(loggedIn) {
  const loginPanel = q("#loginPanel");
  const adminApp = q("#adminApp");

  loginPanel?.classList.toggle("hidden", loggedIn);
  adminApp?.classList.toggle("hidden", !loggedIn);

  const logoutBtn = q("#logoutBtn");
  if (logoutBtn) logoutBtn.disabled = !loggedIn;

  if (loggedIn) {
    observeAnimated(adminApp);
  }
}

function switchPane(targetId) {
  qs(".admin-nav-btn").forEach((btn) => {
    btn.classList.toggle("active", btn.dataset.target === targetId);
  });

  qs(".admin-pane").forEach((pane) => {
    pane.classList.toggle("active", pane.id === targetId);
  });
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

  if (!state.loginLogs.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 5;
    td.textContent = "暂无登录审计日志";
    tr.appendChild(td);
    tbody.appendChild(tr);
    return;
  }

  state.loginLogs.forEach((item) => {
    const tr = document.createElement("tr");

    const timeTd = document.createElement("td");
    timeTd.textContent = formatDate(item?.time);

    const ipTd = document.createElement("td");
    ipTd.textContent = safeText(item?.ip, "-");

    const locationTd = document.createElement("td");
    locationTd.textContent = safeText(item?.location, "未知");

    const statusTd = document.createElement("td");
    statusTd.textContent = item?.success ? "成功" : "失败";

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
  if (!days.length) {
    const today = new Date().toISOString().slice(0, 10);
    days.push(today);
  }

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
  if (!state.selectedUsageDay) {
    state.selectedUsageDay = usage.selectedDay || usage.today || "";
  }

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

function createTrackTag(track) {
  const tag = document.createElement("span");
  const normalized = String(track || "stable").toLowerCase();
  tag.className = "admin-track-tag";
  tag.textContent = normalized;
  return tag;
}

function filterVersions(list) {
  const term = state.versionSearch.trim().toLowerCase();
  if (!term) return [...list];

  return list.filter((item) => {
    return [item.id, item.version, item.title, item.description].some((field) =>
      String(field || "").toLowerCase().includes(term)
    );
  });
}

function filterPlugins(list) {
  const term = state.pluginSearch.trim().toLowerCase();
  if (!term) return [...list];

  return list.filter((item) => {
    return [item.id, item.name, item.version, item.description].some((field) =>
      String(field || "").toLowerCase().includes(term)
    );
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

function renderVersions(payload) {
  state.versions = Array.isArray(payload?.versions) ? payload.versions : state.versions;
  state.latestStableId = payload?.latestStableId || state.latestStableId;

  const stableIdNode = q("#latestStableId");
  if (stableIdNode) stableIdNode.textContent = state.latestStableId || "-";

  const body = q("#versionTableBody");
  if (!body) return;
  body.innerHTML = "";

  const list = filterVersions(state.versions);

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
    idTd.textContent = item.id;

    const versionTd = document.createElement("td");
    versionTd.textContent = item.version;

    const trackTd = document.createElement("td");
    trackTd.appendChild(createTrackTag(item.track || "stable"));

    const titleTd = document.createElement("td");
    titleTd.textContent = item.title;

    const updatedTd = document.createElement("td");
    updatedTd.textContent = formatDate(item.updatedAt);

    const actionTd = document.createElement("td");
    const wrap = document.createElement("div");
    wrap.className = "admin-action-wrap";

    wrap.append(
      createActionButton("edit-version", item.id, "编辑"),
      createActionButton("copy-version-url", item.id, "复制链接"),
      createActionButton("duplicate-version", item.id, "复制"),
      createActionButton("set-latest-stable", item.id, "设为最新", "warn", String(item.track || "") !== "stable"),
      createActionButton("delete-version", item.id, "删除", "danger")
    );

    actionTd.appendChild(wrap);
    tr.append(idTd, versionTd, trackTd, titleTd, updatedTd, actionTd);
    body.appendChild(tr);
  });
}

function renderPlugins(data) {
  state.plugins = Array.isArray(data) ? data : state.plugins;

  const body = q("#pluginTableBody");
  if (!body) return;
  body.innerHTML = "";

  const list = filterPlugins(state.plugins);

  if (!list.length) {
    const tr = document.createElement("tr");
    const td = document.createElement("td");
    td.colSpan = 6;
    td.textContent = "暂无匹配插件";
    tr.appendChild(td);
    body.appendChild(tr);
    return;
  }

  list.forEach((item) => {
    const tr = document.createElement("tr");

    const idTd = document.createElement("td");
    idTd.textContent = item.id;

    const nameTd = document.createElement("td");
    nameTd.textContent = item.name;

    const versionTd = document.createElement("td");
    versionTd.textContent = item.version;

    const descTd = document.createElement("td");
    descTd.title = safeText(item.description, "");
    descTd.textContent = clipText(item.description, 54);

    const updatedTd = document.createElement("td");
    updatedTd.textContent = formatDate(item.updatedAt);

    const actionTd = document.createElement("td");
    const wrap = document.createElement("div");
    wrap.className = "admin-action-wrap";

    wrap.append(
      createActionButton("edit-plugin", item.id, "编辑"),
      createActionButton("copy-plugin-url", item.id, "复制链接"),
      createActionButton("duplicate-plugin", item.id, "复制"),
      createActionButton("delete-plugin", item.id, "删除", "danger")
    );

    actionTd.appendChild(wrap);
    tr.append(idTd, nameTd, versionTd, descTd, updatedTd, actionTd);
    body.appendChild(tr);
  });
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

function fillPluginForm(item) {
  q("#pluginId").value = item?.id || "";
  q("#pluginName").value = item?.name || "";
  q("#pluginVersion").value = item?.version || "";
  q("#pluginDownloadURL").value = item?.downloadURL || "";
  q("#pluginSha256").value = item?.sha256 || "";
  q("#pluginDescription").value = item?.description || "";
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
      // ignore periodic refresh errors
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
  const fileInput = q("#adminKeyFile");
  if (!fileInput?.files?.length) {
    showMessage("请先选择 key 文件。", true);
    return;
  }

  const formData = new FormData();
  formData.append("keyFile", fileInput.files[0]);

  try {
    const data = await apiFetch("/api/admin/login", {
      method: "POST",
      body: formData,
      headers: {},
    });

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
  }
}

async function handleVersionSubmit(event) {
  event.preventDefault();
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

  try {
    if (id) {
      await apiFetch(`/api/admin/versions/${encodeURIComponent(id)}`, {
        method: "PUT",
        body: JSON.stringify(payload),
      });
    } else {
      await apiFetch("/api/admin/versions", {
        method: "POST",
        body: JSON.stringify(payload),
      });
    }

    fillVersionForm(null);
    await refreshAll();
    switchPane("versionPane");
    showMessage("版本保存成功。", false);
  } catch (error) {
    showMessage(error.message || "版本保存失败。", true);
  }
}

async function handlePluginSubmit(event) {
  event.preventDefault();
  const id = q("#pluginId").value.trim();

  const payload = {
    id,
    name: q("#pluginName").value.trim(),
    version: q("#pluginVersion").value.trim(),
    downloadURL: q("#pluginDownloadURL").value.trim(),
    sha256: q("#pluginSha256").value.trim(),
    description: q("#pluginDescription").value.trim(),
  };

  try {
    if (id) {
      await apiFetch(`/api/admin/plugins/${encodeURIComponent(id)}`, {
        method: "PUT",
        body: JSON.stringify(payload),
      });
    } else {
      await apiFetch("/api/admin/plugins", {
        method: "POST",
        body: JSON.stringify(payload),
      });
    }

    fillPluginForm(null);
    await refreshAll();
    switchPane("pluginPane");
    showMessage("插件保存成功。", false);
  } catch (error) {
    showMessage(error.message || "插件保存失败。", true);
  }
}

async function handleUploadSubmit(event) {
  event.preventDefault();
  const fileInput = q("#uploadFile");
  if (!fileInput?.files?.length) {
    showMessage("请先选择要上传的文件。", true);
    return;
  }

  const formData = new FormData();
  formData.append("kind", q("#uploadKind").value);
  formData.append("file", fileInput.files[0]);

  try {
    const result = await apiFetch("/api/admin/upload", {
      method: "POST",
      body: formData,
      headers: {},
    });

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
      switchPane("versionPane");
    } else if (result.kind === "plugin") {
      q("#pluginDownloadURL").value = result.downloadURL;
      q("#pluginSha256").value = result.sha256;
      switchPane("pluginPane");
    }

    showMessage("上传成功。", false);
  } catch (error) {
    showMessage(error.message || "上传失败。", true);
  }
}

async function handleVersionTableClick(event) {
  const target = event.target.closest("button[data-action][data-id]");
  if (!(target instanceof HTMLButtonElement)) return;

  const action = target.dataset.action;
  const id = target.dataset.id;
  if (!action || !id) return;

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
      fillVersionForm({
        ...selected,
        id: "",
        title: `${safeText(selected.title, "版本")} (复制)`,
      });
      q("#versionNumber")?.focus();
      switchPane("versionPane");
      showMessage("已复制版本信息，请修改后保存。", false);
      return;
    }

    if (action === "delete-version") {
      if (!window.confirm(`确认删除版本 ${id}？`)) return;
      await apiFetch(`/api/admin/versions/${encodeURIComponent(id)}`, { method: "DELETE" });
      await refreshAll();
      showMessage("版本已删除。", false);
      return;
    }

    if (action === "set-latest-stable") {
      await apiFetch(`/api/admin/versions/${encodeURIComponent(id)}/latest?channel=stable`, { method: "POST" });
      await refreshAll();
      showMessage("最新稳定版指针已更新。", false);
    }
  } catch (error) {
    showMessage(error.message || "操作失败。", true);
  }
}

async function handlePluginTableClick(event) {
  const target = event.target.closest("button[data-action][data-id]");
  if (!(target instanceof HTMLButtonElement)) return;

  const action = target.dataset.action;
  const id = target.dataset.id;
  if (!action || !id) return;

  const selected = state.plugins.find((item) => item.id === id);

  try {
    if (action === "edit-plugin" && selected) {
      fillPluginForm(selected);
      switchPane("pluginPane");
      return;
    }

    if (action === "copy-plugin-url" && selected) {
      await copyToClipboard(selected.downloadURL, "插件下载链接已复制。");
      return;
    }

    if (action === "duplicate-plugin" && selected) {
      fillPluginForm({
        ...selected,
        id: "",
        name: `${safeText(selected.name, "插件")} (复制)`,
      });
      q("#pluginName")?.focus();
      switchPane("pluginPane");
      showMessage("已复制插件信息，请修改后保存。", false);
      return;
    }

    if (action === "delete-plugin") {
      if (!window.confirm(`确认删除插件 ${id}？`)) return;
      await apiFetch(`/api/admin/plugins/${encodeURIComponent(id)}`, { method: "DELETE" });
      await refreshAll();
      showMessage("插件已删除。", false);
    }
  } catch (error) {
    showMessage(error.message || "操作失败。", true);
  }
}

async function handleLogout() {
  try {
    await apiFetch("/api/admin/logout", { method: "POST" });
  } catch {
    // ignore network errors during logout
  }

  setToken("");
  stopAutoRefresh();
  state.selectedUsageDay = "";
  state.todayUsageDay = "";
  state.versionSearch = "";
  state.pluginSearch = "";
  state.loginLogs = [];

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
  q("#pluginForm")?.addEventListener("submit", handlePluginSubmit);
  q("#uploadForm")?.addEventListener("submit", handleUploadSubmit);

  q("#versionTableBody")?.addEventListener("click", handleVersionTableClick);
  q("#pluginTableBody")?.addEventListener("click", handlePluginTableClick);

  q("#versionFormReset")?.addEventListener("click", () => fillVersionForm(null));
  q("#pluginFormReset")?.addEventListener("click", () => fillPluginForm(null));

  q("#versionCreateBtn")?.addEventListener("click", () => {
    fillVersionForm(null);
    switchPane("versionPane");
    q("#versionNumber")?.focus();
  });

  q("#pluginCreateBtn")?.addEventListener("click", () => {
    fillPluginForm(null);
    switchPane("pluginPane");
    q("#pluginName")?.focus();
  });

  q("#versionSearchInput")?.addEventListener("input", (event) => {
    state.versionSearch = String(event.target?.value || "");
    renderVersions({ versions: state.versions, latestStableId: state.latestStableId });
  });

  q("#pluginSearchInput")?.addEventListener("input", (event) => {
    state.pluginSearch = String(event.target?.value || "");
    renderPlugins(state.plugins);
  });

  q("#logoutBtn")?.addEventListener("click", handleLogout);

  q("#usageDaySelect")?.addEventListener("change", async () => {
    const day = q("#usageDaySelect")?.value || "";
    state.selectedUsageDay = day;
    await refreshUsage(day);
  });

  q("#usageDayRefresh")?.addEventListener("click", async () => {
    await refreshUsage(state.selectedUsageDay);
  });

  qs(".admin-nav-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
      if (btn.dataset.target) {
        switchPane(btn.dataset.target);
      }
    });
  });
}

document.addEventListener("DOMContentLoaded", async () => {
  setupReveal();
  bindEvents();
  await verifySession();
});
