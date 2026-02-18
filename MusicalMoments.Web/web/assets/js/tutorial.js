(function () {
  const { q, formatDate } = window.MMWeb;

  async function loadTutorial() {
    const contentNode = q("tutorialContent");
    const updatedNode = q("tutorialUpdatedAt");
    if (!contentNode) return;

    try {
      const response = await fetch("/assets/docs/tutorial.md", { cache: "no-store" });
      if (!response.ok) {
        throw new Error(`教程文档加载失败: ${response.status}`);
      }

      const markdown = await response.text();

      if (!window.marked || typeof window.marked.parse !== "function") {
        throw new Error("Markdown 渲染库未加载");
      }

      window.marked.setOptions({
        gfm: true,
        breaks: true,
      });

      contentNode.innerHTML = window.marked.parse(markdown);
      if (updatedNode) {
        const headerDate = response.headers.get("Last-Modified") || new Date().toISOString();
        updatedNode.textContent = `更新时间：${formatDate(headerDate)}`;
      }
    } catch (error) {
      console.error(error);
      contentNode.innerHTML = `<p>教程内容暂时无法加载，请稍后重试。(${error.message})</p>`;
      if (updatedNode) {
        updatedNode.textContent = "更新时间：加载失败";
      }
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    loadTutorial();
  });
})();
