(() => {
  const options = {
    license_key: "gpl",
    base_url: "https://cdn.jsdelivr.net/npm/tinymce@7.7.2",
    suffix: ".min",
    menubar: false,
    branding: false,
    promotion: false,
    plugins: "lists link autolink",
    toolbar: "bold italic underline | bullist numlist | link | removeformat | undo redo",
    height: 260,
    convert_urls: false,
    valid_elements: "p,br,strong/b,em/i,u,ul,ol,li,a[href|target|rel],span",
    content_style: "body { font-family: Inter, sans-serif; font-size: 14px; line-height: 1.5; }",
  };

  window.htmlEditor = {
    options,
    init(selector, extra = {}) {
      if (!window.tinymce) return Promise.resolve();
      return tinymce.init({ ...options, selector, ...extra });
    },
    remove(id) {
      window.tinymce?.get(id)?.remove();
    },
    content(id) {
      window.tinymce?.get(id)?.save();
      return window.tinymce?.get(id)?.getContent() ?? document.getElementById(id)?.value ?? "";
    },
  };

  if (!window.tinymce || !document.querySelector("textarea.html-editor")) return;

  tinymce.init({
    ...options,
    selector: "textarea.html-editor",
  });

  document.addEventListener("submit", () => {
    window.tinymce?.triggerSave();
  }, true);
})();
