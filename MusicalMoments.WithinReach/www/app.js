const PAGE_SIZE = 50;
const REFRESH_INTERVAL_MS = 700;
const THEME_STORAGE_KEY = "withinreach_theme";

const state = {
  mode: "set_selected",
  connected: false,
  isPlaying: false,
  selectedAudioPath: "",
  currentPlaybackAudioPath: "",
  sameAudioBehavior: "",
  differentAudioBehavior: ""
};

let allItems = [];
let audioRevision = 0;
let keyword = "";
let currentPage = 1;
let refreshingState = false;
let pollingTimer = null;
let renderKey = "";
let modeSyncing = false;

const connectCard = document.getElementById("connectCard");
const connectPill = document.getElementById("connectPill");
const modePill = document.getElementById("modePill");
const playPill = document.getElementById("playPill");
const behaviorPill = document.getElementById("behaviorPill");
const modeSelect = document.getElementById("modeSelect");
const searchInput = document.getElementById("searchInput");
const reloadButton = document.getElementById("reloadButton");
const stopButton = document.getElementById("stopButton");
const themeButton = document.getElementById("themeButton");
const pagerInfo = document.getElementById("pagerInfo");
const prevPageButton = document.getElementById("prevPageButton");
const nextPageButton = document.getElementById("nextPageButton");
const pageButtons = document.getElementById("pageButtons");
const audioGrid = document.getElementById("audioGrid");

function bindEvents() {
  searchInput.addEventListener("input", () => {
    keyword = (searchInput.value || "").trim().toLowerCase();
    currentPage = 1;
    renderAudio(true);
  });

  modeSelect.addEventListener("change", async () => {
    if (modeSyncing) {
      return;
    }

    const nextMode = modeSelect.value === "direct_play" ? "direct_play" : "set_selected";
    modeSelect.disabled = true;
    try {
      const result = await fetchJson("/api/mode", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ mode: nextMode })
      });

      if (result?.ok && result.data?.mode) {
        state.mode = normalizeMode(result.data.mode);
      } else {
        state.mode = normalizeMode(nextMode);
      }
    } finally {
      modeSelect.disabled = false;
    }

    updateStateUi();
    renderAudio(true);
  });

  reloadButton.addEventListener("click", async () => {
    await loadAudioList(true);
    await refreshState();
  });

  stopButton.addEventListener("click", async () => {
    if (!state.connected || !state.isPlaying) {
      return;
    }

    stopButton.disabled = true;
    try {
      const result = await fetchJson("/api/stop", { method: "POST" });
      if (result?.ok && result.data) {
        applyStatePatch(result.data);
      }
    } finally {
      stopButton.disabled = false;
    }

    updateStateUi();
    renderAudio(true);
  });

  themeButton.addEventListener("click", () => {
    const isDark = document.documentElement.classList.contains("dark");
    applyTheme(isDark ? "light" : "dark");
  });

  prevPageButton.addEventListener("click", () => {
    if (currentPage <= 1) {
      return;
    }

    currentPage -= 1;
    renderAudio(true);
  });

  nextPageButton.addEventListener("click", () => {
    const totalPages = getTotalPages(getFilteredItems());
    if (currentPage >= totalPages) {
      return;
    }

    currentPage += 1;
    renderAudio(true);
  });

  pageButtons.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-page]");
    if (!button) {
      return;
    }

    const targetPage = Number.parseInt(button.dataset.page || "1", 10);
    if (Number.isNaN(targetPage) || targetPage < 1) {
      return;
    }

    currentPage = targetPage;
    renderAudio(true);
  });

  audioGrid.addEventListener("click", async (event) => {
    const button = event.target.closest("button[data-action='audio-click']");
    if (!button) {
      return;
    }

    const card = button.closest(".audio-card");
    const path = card?.dataset?.path || "";
    if (!path || !state.connected) {
      return;
    }

    button.disabled = true;
    card.classList.add("is-busy");
    try {
      const result = await fetchJson("/api/click", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ path })
      });

      if (result?.ok && result.data) {
        applyStatePatch(result.data);
      }
    } finally {
      card.classList.remove("is-busy");
      button.disabled = !state.connected;
    }

    updateStateUi();
    renderAudio(true);
  });

  document.addEventListener("visibilitychange", () => {
    if (document.hidden) {
      stopPolling();
      return;
    }

    startPolling();
    void refreshState();
  });
}

async function init() {
  bindEvents();
  applyTheme(localStorage.getItem(THEME_STORAGE_KEY) || "light");

  await Promise.all([loadAudioList(false), refreshState()]);
  startPolling();
}

function startPolling() {
  if (pollingTimer) {
    return;
  }

  pollingTimer = setInterval(() => {
    void refreshState();
  }, REFRESH_INTERVAL_MS);
}

function stopPolling() {
  if (!pollingTimer) {
    return;
  }

  clearInterval(pollingTimer);
  pollingTimer = null;
}

function applyTheme(theme) {
  const normalizedTheme = theme === "dark" ? "dark" : "light";
  document.documentElement.classList.toggle("dark", normalizedTheme === "dark");
  localStorage.setItem(THEME_STORAGE_KEY, normalizedTheme);
  themeButton.textContent = normalizedTheme === "dark" ? "切换日间模式" : "切换夜间模式";
}

async function loadAudioList(showBusyState) {
  if (showBusyState) {
    reloadButton.disabled = true;
    reloadButton.textContent = "刷新中...";
  }

  try {
    const result = await fetchJson("/api/audio");
    allItems = Array.isArray(result?.data) ? result.data : [];
    audioRevision += 1;
    currentPage = 1;
    renderAudio(true);
  } finally {
    if (showBusyState) {
      reloadButton.disabled = false;
      reloadButton.textContent = "刷新列表";
    }
  }
}

async function refreshState() {
  if (refreshingState) {
    return;
  }

  refreshingState = true;
  try {
    const result = await fetchJson("/api/state");
    if (result?.ok && result.data) {
      applyStatePatch(result.data);
    } else {
      state.connected = false;
    }

    updateStateUi();
    renderAudio(false);
  } finally {
    refreshingState = false;
  }
}

function applyStatePatch(patch) {
  state.mode = normalizeMode(patch.mode);
  state.connected = !!patch.connected;
  state.isPlaying = !!patch.isPlaying;
  state.selectedAudioPath = patch.selectedAudioPath || "";
  state.currentPlaybackAudioPath = patch.currentPlaybackAudioPath || "";
  state.sameAudioBehavior = patch.sameAudioBehavior || "";
  state.differentAudioBehavior = patch.differentAudioBehavior || "";
}

function updateStateUi() {
  const connectedText = state.connected ? "已连接 MM" : "连接失败，等待重试";
  connectPill.textContent = `连接状态：${connectedText}`;
  connectCard.dataset.state = state.connected ? "ok" : "error";

  const modeText = state.mode === "direct_play" ? "按下后直接播放" : "按下后设为播放项";
  modePill.textContent = `点击行为：${modeText}`;

  modeSyncing = true;
  try {
    modeSelect.value = state.mode;
  } finally {
    modeSyncing = false;
  }

  playPill.textContent = `播放状态：${state.isPlaying ? "播放中" : "空闲"}`;

  const sameText = normalizeBehaviorText(state.sameAudioBehavior);
  const differentText = normalizeBehaviorText(state.differentAudioBehavior);
  behaviorPill.textContent = `同音频：${sameText} | 不同音频：${differentText}`;

  stopButton.disabled = !state.connected || !state.isPlaying;
}

function getFilteredItems() {
  if (!keyword) {
    return allItems;
  }

  return allItems.filter((item) => {
    const name = (item.name || "").toLowerCase();
    const track = (item.track || "").toLowerCase();
    const fileType = (item.fileType || "").toLowerCase();
    const key = (item.key || "").toLowerCase();
    return name.includes(keyword) || track.includes(keyword) || fileType.includes(keyword) || key.includes(keyword);
  });
}

function getTotalPages(items) {
  return Math.max(1, Math.ceil(items.length / PAGE_SIZE));
}

function renderAudio(force) {
  const filteredItems = getFilteredItems();
  const totalPages = getTotalPages(filteredItems);
  currentPage = Math.max(1, Math.min(currentPage, totalPages));

  const nextRenderKey = [
    audioRevision,
    keyword,
    state.mode,
    state.connected,
    state.isPlaying,
    normalize(state.selectedAudioPath),
    normalize(state.currentPlaybackAudioPath),
    currentPage,
    filteredItems.length
  ].join("|");

  if (!force && nextRenderKey === renderKey) {
    return;
  }

  renderKey = nextRenderKey;

  renderPagination(filteredItems.length, totalPages);

  if (filteredItems.length === 0) {
    const emptyText = allItems.length === 0
      ? "当前没有可用音频，请先在 MM 中导入音频。"
      : "没有匹配的音频，请修改搜索关键字。";

    audioGrid.innerHTML = `<div class="empty-state col-span-full rounded-2xl border border-dashed border-slate-300 bg-white/80 p-6 text-center text-sm text-slate-600 dark:border-slate-600 dark:bg-slate-900/70 dark:text-slate-300">${escapeHtml(emptyText)}</div>`;
    return;
  }

  const startIndex = (currentPage - 1) * PAGE_SIZE;
  const pageItems = filteredItems.slice(startIndex, startIndex + PAGE_SIZE);
  const actionText = state.mode === "direct_play" ? "播放此音频" : "使用此音频";

  audioGrid.innerHTML = pageItems
    .map((item) => {
      const path = item.filePath || "";
      const normalizedPath = normalize(path);
      const selected = normalizedPath === normalize(state.selectedAudioPath);
      const active = normalizedPath === normalize(state.currentPlaybackAudioPath) && state.isPlaying;
      const cardStateClass = `${selected ? " is-selected" : ""}${active ? " is-active" : ""}`;
      const badgeText = active ? "播放中" : selected ? "已设为播放项" : "就绪";

      return `
<article class="audio-card${cardStateClass} rounded-2xl border border-slate-200 bg-white/90 p-4 shadow-lg transition dark:border-slate-700 dark:bg-slate-900/80" data-path="${escapeHtml(path)}">
  <header class="mb-2 flex items-start justify-between gap-2">
    <h3 class="line-clamp-2 text-base font-semibold leading-6 text-slate-900 dark:text-slate-100" title="${escapeHtml(item.name || "未命名音频")}">${escapeHtml(item.name || "未命名音频")}</h3>
    <span class="shrink-0 rounded-full border border-slate-300 bg-slate-50 px-2 py-0.5 text-xs text-slate-600 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-300">${escapeHtml(badgeText)}</span>
  </header>
  <p class="text-sm text-slate-600 dark:text-slate-300">曲目：${escapeHtml(item.track || "无曲目")}</p>
  <p class="mt-1 text-sm text-slate-600 dark:text-slate-300">格式：${escapeHtml(item.fileType || "未知格式")} | 按键：${escapeHtml(item.key || "未绑定")}</p>
  <button class="audio-action mt-3 inline-flex h-10 w-full items-center justify-center rounded-lg border border-blue-300 bg-blue-50 px-3 text-sm font-semibold text-blue-700 transition hover:bg-blue-100 disabled:cursor-not-allowed disabled:opacity-60 dark:border-blue-700 dark:bg-blue-900/30 dark:text-blue-200 dark:hover:bg-blue-900/40" type="button" data-action="audio-click" ${state.connected ? "" : "disabled"}>${actionText}</button>
</article>`;
    })
    .join("");
}

function renderPagination(totalCount, totalPages) {
  pagerInfo.textContent = `第 ${currentPage} / ${totalPages} 页 · 每页 ${PAGE_SIZE} 条 · 共 ${totalCount} 条`;

  prevPageButton.disabled = currentPage <= 1;
  nextPageButton.disabled = currentPage >= totalPages;

  const pageParts = buildPageParts(totalPages, currentPage);
  pageButtons.innerHTML = pageParts
    .map((part) => {
      if (part === "...") {
        return '<span class="px-2 text-sm text-slate-500 dark:text-slate-400">...</span>';
      }

      const page = Number(part);
      const active = page === currentPage;
      const cls = active
        ? "border-blue-500 bg-blue-600 text-white dark:border-blue-400 dark:bg-blue-500"
        : "border-slate-300 bg-white text-slate-700 hover:bg-slate-50 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700";

      return `<button type="button" data-page="${page}" class="inline-flex h-9 min-w-9 items-center justify-center rounded-md border px-2 text-sm font-medium transition ${cls}">${page}</button>`;
    })
    .join("");
}

function buildPageParts(totalPages, current) {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const parts = [1];
  let start = Math.max(2, current - 1);
  let end = Math.min(totalPages - 1, current + 1);

  if (start <= 3) {
    start = 2;
    end = 4;
  }

  if (end >= totalPages - 2) {
    start = totalPages - 3;
    end = totalPages - 1;
  }

  if (start > 2) {
    parts.push("...");
  }

  for (let page = start; page <= end; page += 1) {
    parts.push(page);
  }

  if (end < totalPages - 1) {
    parts.push("...");
  }

  parts.push(totalPages);
  return parts;
}

function normalizeBehaviorText(value) {
  const normalized = (value || "").trim().toLowerCase();
  switch (normalized) {
    case "restartfrombeginning":
      return "从头重新播放";
    case "stopplayback":
      return "再次按下停止";
    case "stopandplaynew":
      return "停止后播放新的音频";
    case "stoponly":
      return "只停止当前播放";
    default:
      return value || "未知";
  }
}

function normalizeMode(mode) {
  return mode === "direct_play" ? "direct_play" : "set_selected";
}

async function fetchJson(url, init) {
  try {
    const response = await fetch(url, init);
    if (!response.ok) {
      return null;
    }

    return await response.json();
  } catch {
    return null;
  }
}

function normalize(value) {
  return (value || "").trim().toLowerCase();
}

function escapeHtml(value) {
  return (value || "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/\"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

void init();
