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
    desc.textContent = clipText(descText, 108);

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

    list.slice(0, 4).forEach((item, index) => {
      const version = safeText(item.version, "-");
      const title = safeText(item.title, "未命名版本");
      const desc = item.description || item.changelog || "暂无版本说明";
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

    list.slice(0, 4).forEach((item, index) => {
      const title = `${safeText(item.name)} · ${safeText(item.version)}`;
      const desc = item.description || "暂无插件说明";
      const meta = `更新时间：${formatDate(item.updatedAt || item.createdAt)}`;
      const href = `/api/public/download/plugin/${encodeURIComponent(item.id)}`;
      container.appendChild(createFeedItem(title, desc, meta, href, index));
    });

    observeDynamic(container);
  }

  function wait(ms) {
    return new Promise((resolve) => window.setTimeout(resolve, ms));
  }

  const MM_MOVE_EASING = "cubic-bezier(0.76, 0, 0.24, 1)";

  function cubicBezierSampleAt(t, p1x, p1y, p2x, p2y) {
    const cx = 3 * p1x;
    const bx = 3 * (p2x - p1x) - cx;
    const ax = 1 - cx - bx;
    const cy = 3 * p1y;
    const by = 3 * (p2y - p1y) - cy;
    const ay = 1 - cy - by;

    const sampleX = (u) => ((ax * u + bx) * u + cx) * u;
    const sampleY = (u) => ((ay * u + by) * u + cy) * u;
    const sampleDX = (u) => (3 * ax * u + 2 * bx) * u + cx;

    const x = Math.min(1, Math.max(0, t));
    let u = x;

    for (let i = 0; i < 6; i += 1) {
      const dx = sampleDX(u);
      if (Math.abs(dx) < 1e-6) break;
      const err = sampleX(u) - x;
      if (Math.abs(err) < 1e-5) break;
      u -= err / dx;
    }

    u = Math.min(1, Math.max(0, u));
    let low = 0;
    let high = 1;
    for (let i = 0; i < 8; i += 1) {
      const mid = (low + high) * 0.5;
      const midX = sampleX(mid);
      if (Math.abs(midX - x) < 1e-5) {
        u = mid;
        break;
      }
      if (midX < x) low = mid;
      else high = mid;
      u = mid;
    }

    return sampleY(u);
  }

  function specialRevealEase(progress) {
    return cubicBezierSampleAt(progress, 0.76, 0, 0.24, 1);
  }

  async function animateCenteredHorizontalClip({
    wrapEl,
    logoEl,
    edgeEl,
    viewportWidth,
    fullWidth,
    startVisibleWidth,
    duration,
  }) {
    const start = window.performance.now();

    await new Promise((resolve) => {
      const step = (now) => {
        const elapsed = now - start;
        const progress = Math.min(1, Math.max(0, elapsed / duration));
        const eased = specialRevealEase(progress);
        const currentVisible = startVisibleWidth + (fullWidth - startVisibleWidth) * eased;
        const rightInset = Math.max(0, fullWidth - currentVisible);
        const left = (viewportWidth - currentVisible) / 2;
        const clip = `inset(0 ${rightInset}px 0 0)`;

        wrapEl.style.left = `${left}px`;
        logoEl.style.clipPath = clip;
        logoEl.style.webkitClipPath = clip;
        if (edgeEl instanceof HTMLElement) {
          const pulse = 0.55 + 0.45 * Math.sin(progress * Math.PI);
          const opacity = progress >= 1 ? 0 : 0.18 + (1 - progress) * 0.62 * pulse;
          edgeEl.style.left = `${currentVisible}px`;
          edgeEl.style.opacity = `${opacity}`;
          edgeEl.style.transform = `translateX(-50%) scaleY(${0.9 + (1 - progress) * 0.12})`;
        }

        if (progress < 1) {
          window.requestAnimationFrame(step);
          return;
        }
        resolve();
      };

      window.requestAnimationFrame(step);
    });
  }

  async function runHomeIntro() {
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

    const brandLogo = document.querySelector(".mm-brand-title-logo");
    if (!(brandLogo instanceof HTMLImageElement)) return;
    if (!brandLogo.complete && typeof brandLogo.decode === "function") {
      try {
        await brandLogo.decode();
      } catch {
        // ignore decode errors
      }
    }

    const logoSrc = brandLogo.getAttribute("src") || "/assets/img/mm-title-logo.png";

    const intro = document.createElement("div");
    intro.id = "mmHomeIntro";
    intro.className = "mm-home-intro";
    intro.innerHTML = `
      <div class="mm-home-intro-bg"></div>
      <div class="mm-home-intro-stage">
        <div class="mm-home-intro-logo-wrap">
          <img src="${logoSrc}" alt="" class="mm-home-intro-logo" aria-hidden="true">
          <span class="mm-home-intro-reveal-edge" aria-hidden="true"></span>
        </div>
      </div>
    `;
    document.body.appendChild(intro);
    document.body.classList.add("mm-home-intro-running");

    const introWrap = intro.querySelector(".mm-home-intro-logo-wrap");
    const introLogo = intro.querySelector(".mm-home-intro-logo");
    const introEdge = intro.querySelector(".mm-home-intro-reveal-edge");
    if (
      !(introLogo instanceof HTMLImageElement)
      || !(introWrap instanceof HTMLDivElement)
      || !(introEdge instanceof HTMLSpanElement)
    ) {
      intro.remove();
      document.body.classList.remove("mm-home-intro-running");
      return;
    }

    const previousBrandTransition = brandLogo.style.transition;
    const previousBrandOpacity = brandLogo.style.opacity;
    brandLogo.style.transition = "opacity 0.24s linear";
    brandLogo.style.opacity = "0";

    await wait(24);
    if (typeof introLogo.decode === "function") {
      try {
        await introLogo.decode();
      } catch {
        // ignore decode errors
      }
    }

    const fullWidth = Math.min(window.innerWidth * 0.76, 1120);
    const clippedWidth = fullWidth / 5 - 30;
    const logoRatio = introLogo.naturalWidth > 0
      ? introLogo.naturalHeight / introLogo.naturalWidth
      : (brandLogo.clientHeight > 0 && brandLogo.clientWidth > 0
          ? brandLogo.clientHeight / brandLogo.clientWidth
          : 0.238);
    const fullHeight = fullWidth * logoRatio;
    const startLeft = (window.innerWidth - clippedWidth) / 2;
    const endLeft = (window.innerWidth - fullWidth) / 2;
    const startTop = (window.innerHeight - fullHeight) / 2;
    const initialClip = `inset(0 ${fullWidth - clippedWidth}px 0 0)`;
    const fullClip = "inset(0 0px 0 0)";

    introWrap.style.left = `${startLeft}px`;
    introWrap.style.top = `${startTop}px`;
    introWrap.style.width = `${fullWidth}px`;
    introWrap.style.height = `${fullHeight}px`;
    introLogo.style.width = `${fullWidth}px`;
    introLogo.style.height = `${fullHeight}px`;
    introLogo.style.clipPath = initialClip;
    introLogo.style.webkitClipPath = initialClip;
    introEdge.style.left = `${clippedWidth}px`;
    introEdge.style.opacity = "0";

    try {
      await wait(260);

      const fadeInAnim = introWrap.animate(
        [{ opacity: 0 }, { opacity: 1 }],
        {
          duration: 320,
          easing: "linear",
          fill: "forwards",
        }
      );
      await fadeInAnim.finished.catch(() => null);
      introWrap.style.opacity = "1";

      await wait(300);

      await animateCenteredHorizontalClip({
        wrapEl: introWrap,
        logoEl: introLogo,
        edgeEl: introEdge,
        viewportWidth: window.innerWidth,
        fullWidth,
        startVisibleWidth: clippedWidth,
        duration: 500,
      });

      introWrap.style.left = `${endLeft}px`;
      introLogo.style.clipPath = fullClip;
      introLogo.style.webkitClipPath = fullClip;
      introEdge.style.opacity = "0";

      const startRect = introWrap.getBoundingClientRect();
      const targetRect = brandLogo.getBoundingClientRect();
      if (!startRect.width || !targetRect.width) return;

      const deltaX = targetRect.left - startRect.left;
      const deltaY = targetRect.top - startRect.top;
      const scale = targetRect.width / startRect.width;
      const moveDuration = 800;

      const moveAnim = introWrap.animate(
        [
          { transform: "translate(0, 0) scale(1)", opacity: 1 },
          { transform: `translate(${deltaX}px, ${deltaY}px) scale(${scale})`, opacity: 1 },
        ],
        {
          duration: moveDuration,
          easing: MM_MOVE_EASING,
          fill: "forwards",
        }
      );
      const showBrandTask = wait(Math.round(moveDuration * 0.76)).then(() => {
        brandLogo.style.opacity = "1";
      });
      await Promise.all([moveAnim.finished.catch(() => null), showBrandTask]);
      introWrap.style.transform = `translate(${deltaX}px, ${deltaY}px) scale(${scale})`;

      intro.classList.add("is-fading");
      await wait(420);

      intro.classList.add("is-exit");
      await wait(240);
    } finally {
      intro.remove();
      document.body.classList.remove("mm-home-intro-running");
      brandLogo.style.transition = previousBrandTransition;
      brandLogo.style.opacity = previousBrandOpacity;
    }
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

    const fallbackStable = stableHistory[0] || null;
    setLatestStable(stable && stable.id ? stable : fallbackStable);
    renderVersionLane(stableHistory);
    renderPluginLane(pluginHistory);
  }

  function getNavigationType() {
    const entries = typeof performance.getEntriesByType === "function"
      ? performance.getEntriesByType("navigation")
      : [];
    if (Array.isArray(entries) && entries.length > 0) {
      const entry = entries[0];
      if (entry && typeof entry.type === "string") {
        return entry.type;
      }
    }

    const legacyType = performance && performance.navigation ? performance.navigation.type : -1;
    if (legacyType === 1) return "reload";
    if (legacyType === 2) return "back_forward";
    return "navigate";
  }

  function isInternalSiteReferrer() {
    const rawReferrer = String(document.referrer || "").trim();
    if (!rawReferrer) return false;

    try {
      const ref = new URL(rawReferrer);
      return ref.origin === window.location.origin;
    } catch {
      return false;
    }
  }

  function shouldPlayHomeIntro() {
    const navType = getNavigationType();
    if (navType === "reload") return true;
    if (navType === "navigate") {
      // First entry (no referrer) and external entries should play.
      return !isInternalSiteReferrer();
    }
    // back/forward and other in-site transitions should not play.
    return false;
  }

  function fixEntryScrollTop() {
    if (window.location.hash) return;

    if ("scrollRestoration" in window.history) {
      window.history.scrollRestoration = "manual";
    }

    const jumpToTop = () => {
      window.scrollTo({ top: 0, left: 0, behavior: "auto" });
    };

    jumpToTop();
    window.requestAnimationFrame(jumpToTop);
    window.setTimeout(jumpToTop, 0);
    window.setTimeout(jumpToTop, 90);

    window.addEventListener("pageshow", () => {
      jumpToTop();
      window.requestAnimationFrame(jumpToTop);
    });
  }

  function setupHomeLeadExperience() {
    const lead = q("homeLead");
    if (!(lead instanceof HTMLElement)) return;

    const scrollCta = q("homeLeadScroll");
    let revealDistance = 1;
    let ticking = false;

    const recalc = () => {
      revealDistance = Math.max(Math.round(lead.offsetHeight * 0.84), 320);
    };

    const applyProgress = () => {
      const scrollTop = Math.max(0, window.scrollY || window.pageYOffset || 0);
      const progress = Math.min(1, scrollTop / revealDistance);
      document.body.style.setProperty("--mm-home-intro-progress", progress.toFixed(4));
      ticking = false;
    };

    const scheduleProgress = () => {
      if (ticking) return;
      ticking = true;
      window.requestAnimationFrame(applyProgress);
    };

    recalc();
    applyProgress();

    window.addEventListener("scroll", scheduleProgress, { passive: true });
    window.addEventListener("resize", () => {
      recalc();
      scheduleProgress();
    }, { passive: true });

    if (scrollCta instanceof HTMLAnchorElement) {
      scrollCta.addEventListener("click", (event) => {
        event.preventDefault();
        const nextFoldTop = Math.max(lead.offsetHeight - 24, 0);
        window.scrollTo({ top: nextFoldTop, behavior: "smooth" });
      });
    }
  }

  function setupScreenshotModal() {
    const modal = q("homeShotModal");
    const modalImg = q("homeShotModalImage");
    const modalCaption = q("homeShotModalCaption");
    const modalClose = q("homeShotModalClose");
    if (
      !(modal instanceof HTMLDivElement)
      || !(modalImg instanceof HTMLImageElement)
      || !(modalCaption instanceof HTMLElement)
      || !(modalClose instanceof HTMLButtonElement)
    ) {
      return;
    }

    const screenshots = Array.from(document.querySelectorAll(".mm-shot-card img"));
    if (!screenshots.length) return;

    let activeTrigger = null;

    const isOpen = () => modal.classList.contains("is-open");

    const close = () => {
      if (!isOpen()) return;
      modal.classList.remove("is-open");
      modal.setAttribute("aria-hidden", "true");
      document.body.classList.remove("mm-shot-modal-open");
      window.setTimeout(() => {
        modalImg.src = "";
      }, 220);
      if (activeTrigger instanceof HTMLElement) {
        activeTrigger.focus({ preventScroll: true });
      }
      activeTrigger = null;
    };

    const open = (target) => {
      if (!(target instanceof HTMLImageElement)) return;
      const figure = target.closest("figure");
      const caption = figure ? figure.querySelector("figcaption") : null;

      activeTrigger = target;
      modalImg.src = target.currentSrc || target.src;
      modalImg.alt = target.alt || "截图预览";
      modalCaption.textContent = caption ? caption.textContent || "" : "";

      modal.setAttribute("aria-hidden", "false");
      document.body.classList.add("mm-shot-modal-open");
      window.requestAnimationFrame(() => {
        modal.classList.add("is-open");
      });
      modalClose.focus({ preventScroll: true });
    };

    screenshots.forEach((img) => {
      if (!(img instanceof HTMLImageElement)) return;
      img.classList.add("mm-zoomable");
      img.tabIndex = 0;
      img.setAttribute("role", "button");
      img.setAttribute("aria-haspopup", "dialog");
      img.setAttribute("aria-label", "点击放大截图");

      img.addEventListener("click", () => open(img));
      img.addEventListener("keydown", (event) => {
        if (event.key !== "Enter" && event.key !== " ") return;
        event.preventDefault();
        open(img);
      });
    });

    modalClose.addEventListener("click", close);
    modal.addEventListener("click", (event) => {
      if (event.target === modal) {
        close();
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && isOpen()) {
        close();
      }
    });
  }

  document.addEventListener("DOMContentLoaded", async () => {
    fixEntryScrollTop();
    setupHomeLeadExperience();
    setupScreenshotModal();
    const dataTask = loadHomeData().catch((error) => console.error(error));
    if (shouldPlayHomeIntro()) {
      await runHomeIntro();
    }
    await dataTask;
  });
})();
