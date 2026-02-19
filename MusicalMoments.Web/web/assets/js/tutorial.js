(function () {
  const { q, formatDate } = window.MMWeb;

  const DOC_FILE_RE = /^[A-Za-z0-9][A-Za-z0-9._-]{0,120}\.md$/;
  const DOC_PATH_RE = /^[a-z0-9][a-z0-9-]{0,80}$/;

  const state = {
    docs: [],
    docsByPath: new Map(),
  };

  function escapeHtml(value) {
    return String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function sanitizeDocItem(raw) {
    const file = String(raw?.file || "").trim();
    if (!DOC_FILE_RE.test(file)) return null;

    const title = String(raw?.title || "").trim() || file.replace(/\.md$/i, "");
    let path = String(raw?.path || "")
      .trim()
      .toLowerCase()
      .replace(/^\/+|\/+$/g, "");
    if (path && !DOC_PATH_RE.test(path)) return null;

    const updatedAt = String(raw?.updatedAt || "").trim();
    return { file, title, path, updatedAt };
  }

  async function loadDocManifest() {
    const response = await fetch("/api/public/docs", { cache: "no-store" });
    if (!response.ok) {
      throw new Error(`教程清单加载失败: ${response.status}`);
    }

    const payload = await response.json().catch(() => null);
    const docsRaw = Array.isArray(payload?.data?.docs) ? payload.data.docs : [];
    const docs = docsRaw.map(sanitizeDocItem).filter(Boolean);
    if (docs.length === 0) {
      throw new Error("未发现可用 Markdown 文档");
    }

    const unique = [];
    const seen = new Set();
    docs.forEach((doc) => {
      const key = `${doc.path}|${doc.file}`;
      if (seen.has(key)) return;
      seen.add(key);
      unique.push(doc);
    });

    state.docs = unique;
    state.docsByPath = new Map(unique.map((doc) => [doc.path, doc]));
  }

  function getCurrentPath() {
    const hash = String(window.location.hash || "").trim();
    if (!hash.startsWith("#/")) return "";
    return hash
      .slice(2)
      .split("?")[0]
      .replace(/^\/+|\/+$/g, "");
  }

  function getCurrentDoc() {
    const path = getCurrentPath();
    return state.docsByPath.get(path) || state.docs[0];
  }

  function buildDocHash(doc) {
    return doc.path ? `#/${doc.path}` : "#/";
  }

  function renderDocNav() {
    const container = q("tutorialDocNav");
    if (!container) return;

    container.innerHTML = "";
    if (!state.docs.length) return;

    const current = getCurrentDoc();
    state.docs.forEach((doc) => {
      const link = document.createElement("a");
      link.className = "mm-doc-nav-link";
      if (doc.path === current.path) {
        link.classList.add("active");
      }
      link.href = buildDocHash(doc);
      link.textContent = doc.title;
      container.appendChild(link);
    });
  }

  function renderSectionNav() {
    const container = q("tutorialSectionNav");
    if (!container) return;

    container.innerHTML = "";
    const headings = Array.from(
      document.querySelectorAll("#tutorialContent .markdown-section h2, #tutorialContent .markdown-section h3")
    );

    if (headings.length === 0) {
      const empty = document.createElement("span");
      empty.className = "mm-doc-anchor-empty";
      empty.textContent = "当前文档暂无章节锚点";
      container.appendChild(empty);
      return;
    }

    headings.slice(0, 24).forEach((heading) => {
      if (!heading.id) return;
      const link = document.createElement("a");
      const level = heading.tagName === "H3" ? 3 : 2;
      link.className = `mm-doc-anchor-link level-${level}`;
      link.href = "javascript:void(0)";
      link.textContent = String(heading.textContent || "").trim();
      link.addEventListener("click", () => {
        heading.scrollIntoView({ behavior: "smooth", block: "start" });
      });
      container.appendChild(link);
    });
  }


  function refreshTopbar() {
    renderDocNav();
    renderSectionNav();
  }

function buildDocsifyAlias() {
  const alias = {};
  const home = state.docs[0];
  alias["/"] = `/${home.file}`;
  alias["/."] = `/${home.file}`;
  alias["/README"] = `/${home.file}`;
  alias["/README.md"] = `/${home.file}`;

  state.docs.forEach((doc) => {
    if (!doc.path) return;
    alias[`/${doc.path}`] = `/${doc.file}`;
    alias[`/${doc.path}/`] = `/${doc.file}`;
    alias[`/${doc.path}.md`] = `/${doc.file}`;
  });

  return alias;
}

  function bootstrapDocsifyConfig() {
    const previousConfig = window.$docsify || {};
    const previousPlugins = Array.isArray(previousConfig.plugins) ? previousConfig.plugins : [];
    const home = state.docs[0];

    window.$docsify = {
      ...previousConfig,
      name: "",
      repo: "",
      basePath: "/assets/docs/",
      auto2top: false,
      loadSidebar: false,
      loadNavbar: false,
      subMaxLevel: 3,
      homepage: home.file,
      alias: buildDocsifyAlias(),
      plugins: [
        ...previousPlugins,
        function (hook) {
          hook.doneEach(() => {
            refreshTopbar();
          });
        },
      ],
    };
  }

  function ensureDocsifyScriptLoaded() {
    if (window.Docsify) {
      return Promise.resolve();
    }

    if (window.__mmDocsifyLoadPromise) {
      return window.__mmDocsifyLoadPromise;
    }

    window.__mmDocsifyLoadPromise = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = "/assets/js/vendor/docsify.min.js";
      script.defer = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error("Docsify 脚本加载失败"));
      document.body.appendChild(script);
    });

    return window.__mmDocsifyLoadPromise;
  }

  function bindRouteChange() {
    window.addEventListener("hashchange", () => {
      window.setTimeout(() => {
        refreshTopbar();
      }, 0);
    });
  }

  function showLoadError(error) {
    const container = q("app");
    if (!container) return;
    container.innerHTML = `<p>教程加载失败：${escapeHtml(error?.message || "未知错误")}</p>`;
  }

  function showDocsifyLoadErrorIfNeeded() {
    window.setTimeout(() => {
      const hasRendered = document.querySelector("#tutorialContent .markdown-section");
      if (hasRendered) return;

      const container = q("app");
      if (container) {
        container.innerHTML = "<p>Docsify 加载失败，请检查本地脚本与文档路径后刷新重试。</p>";
      }
    }, 5000);
  }

  document.addEventListener("DOMContentLoaded", async () => {
    bindRouteChange();

    try {
      await loadDocManifest();
      bootstrapDocsifyConfig();
      refreshTopbar();
      await ensureDocsifyScriptLoaded();
      showDocsifyLoadErrorIfNeeded();
    } catch (error) {
      console.error(error);
      showLoadError(error);
    }
  });
})();
