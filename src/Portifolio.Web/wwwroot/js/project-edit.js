(() => {
  const authors = Array.isArray(window.PROJECT_AUTHORS) ? [...window.PROJECT_AUTHORS] : [];
  const people = Array.isArray(window.PROJECT_PEOPLE) ? window.PROJECT_PEOPLE : [];

  const escapeHtml = (value) =>
    String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;");

  const personById = (id) => people.find((p) => p.id === id);

  const labelOf = (id) => {
    const person = personById(id);
    if (!person) return id;
    return person.name || person.shortName || id;
  };

  const available = () => people.filter((p) => p.id && !authors.includes(p.id));

  const isJoint = () => people.length > 1 && people.every((p) => p.id && authors.includes(p.id));

  const render = () => {
    document.getElementById("authors-list").innerHTML =
      authors
        .map(
          (id, i) => `
        <li>
          <div>
            <strong>${escapeHtml(labelOf(id))}</strong>
            <span>${escapeHtml(personById(id)?.shortName || id)}</span>
          </div>
          <div class="item-actions">
            <button type="button" class="btn-icon" data-remove="${i}" title="Excluir" aria-label="Excluir">
              <svg class="icon" aria-hidden="true"><use href="#i-trash"></use></svg>
            </button>
          </div>
        </li>`
        )
        .join("") || `<li class="empty">Nenhum autor adicionado.</li>`;

    document.getElementById("hidden-authors").innerHTML = authors
      .map((id, i) => `<input type="hidden" name="Authors[${i}]" value="${escapeHtml(id).replaceAll('"', "&quot;")}" />`)
      .join("");

    const authorship = document.getElementById("authorship");
    if (authorship) {
      authorship.value = isJoint() ? "joint" : authors.length === 1 ? authors[0] : "";
    }
  };

  const modal = document.getElementById("author-modal");
  const select = document.getElementById("author-select");
  const empty = document.getElementById("author-empty");
  const save = document.getElementById("author-save");

  const openModal = () => {
    const options = available();
    select.innerHTML = options
      .map((p) => `<option value="${escapeHtml(p.id).replaceAll('"', "&quot;")}">${escapeHtml(labelOf(p.id))}</option>`)
      .join("");
    const none = options.length === 0;
    empty.hidden = !none;
    select.hidden = none;
    save.disabled = none;
    modal.hidden = false;
    if (!none) select.focus();
  };

  const closeModal = () => {
    modal.hidden = true;
  };

  document.getElementById("add-author").addEventListener("click", openModal);
  document.getElementById("author-cancel").addEventListener("click", closeModal);
  modal.addEventListener("click", (event) => {
    if (event.target === modal) closeModal();
  });
  save.addEventListener("click", () => {
    const id = select.value;
    if (!id || authors.includes(id)) return closeModal();
    authors.push(id);
    closeModal();
    render();
  });
  document.getElementById("authors-list").addEventListener("click", (event) => {
    const remove = event.target.closest("[data-remove]");
    if (!remove) return;
    authors.splice(Number(remove.dataset.remove), 1);
    render();
  });

  render();

  const stackItems = Array.isArray(window.PROJECT_STACK) ? [...window.PROJECT_STACK] : [];
  const knownStacks = Array.isArray(window.KNOWN_STACKS) ? window.KNOWN_STACKS : [];

  const renderStack = () => {
    document.getElementById("stack-list").innerHTML =
      stackItems
        .map(
          (item, i) => `
        <li>
          <div><strong>${escapeHtml(item)}</strong></div>
          <div class="item-actions">
            <button type="button" class="btn-icon" data-stack-remove="${i}" title="Excluir" aria-label="Excluir">
              <svg class="icon" aria-hidden="true"><use href="#i-trash"></use></svg>
            </button>
          </div>
        </li>`
        )
        .join("") || `<li class="empty">Nenhuma stack adicionada.</li>`;

    document.getElementById("hidden-stack").innerHTML = stackItems
      .map((item, i) => `<input type="hidden" name="Stack[${i}]" value="${escapeHtml(item).replaceAll('"', "&quot;")}" />`)
      .join("");
  };

  const stackModal = document.getElementById("stack-modal");
  const stackInput = document.getElementById("stack-input");
  const stackSuggest = document.getElementById("stack-suggest");
  const stackNewHint = document.getElementById("stack-new-hint");

  const matches = (query) => {
    const used = new Set(stackItems.map((s) => s.toLowerCase()));
    const q = query.trim().toLowerCase();
    return knownStacks.filter((item) => {
      if (used.has(item.toLowerCase())) return false;
      return !q || item.toLowerCase().startsWith(q);
    });
  };

  const renderSuggest = () => {
    const query = stackInput.value;
    const options = matches(query);
    stackSuggest.innerHTML = options
      .map((item) => `<li><button type="button" data-pick="${escapeHtml(item).replaceAll('"', "&quot;")}">${escapeHtml(item)}</button></li>`)
      .join("");
    const typed = query.trim();
    const exists = typed && knownStacks.some((item) => item.toLowerCase() === typed.toLowerCase());
    const already = typed && stackItems.some((item) => item.toLowerCase() === typed.toLowerCase());
    stackNewHint.hidden = !typed || exists || already || options.length > 0;
  };

  const addStack = (value) => {
    const item = value.trim();
    if (!item) return;
    if (stackItems.some((s) => s.toLowerCase() === item.toLowerCase())) return;
    stackItems.push(item);
    if (!knownStacks.some((s) => s.toLowerCase() === item.toLowerCase())) knownStacks.push(item);
    renderStack();
  };

  const openStackModal = () => {
    stackInput.value = "";
    renderSuggest();
    stackModal.hidden = false;
    stackInput.focus();
  };

  const closeStackModal = () => {
    stackModal.hidden = true;
  };

  document.getElementById("add-stack").addEventListener("click", openStackModal);
  document.getElementById("stack-cancel").addEventListener("click", closeStackModal);
  stackModal.addEventListener("click", (event) => {
    if (event.target === stackModal) closeStackModal();
  });
  stackInput.addEventListener("input", renderSuggest);
  stackSuggest.addEventListener("click", (event) => {
    const pick = event.target.closest("[data-pick]");
    if (!pick) return;
    addStack(pick.dataset.pick);
    closeStackModal();
  });
  document.getElementById("stack-save").addEventListener("click", () => {
    addStack(stackInput.value);
    closeStackModal();
  });
  stackInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
      event.preventDefault();
      addStack(stackInput.value);
      closeStackModal();
    }
  });
  document.getElementById("stack-list").addEventListener("click", (event) => {
    const remove = event.target.closest("[data-stack-remove]");
    if (!remove) return;
    stackItems.splice(Number(remove.dataset.stackRemove), 1);
    renderStack();
  });

  renderStack();

  const linkItems = Array.isArray(window.PROJECT_LINKS) ? [...window.PROJECT_LINKS] : [];
  const linkLabels = { repository: "Repositório", tool: "Link" };

  const renderLinks = () => {
    document.getElementById("links-list").innerHTML =
      linkItems
        .map(
          (item, i) => `
        <li>
          <div>
            <strong>${escapeHtml(linkLabels[item.kind] || item.kind)}</strong>
            <span>${escapeHtml(item.url)}</span>
          </div>
          <div class="item-actions">
            <button type="button" class="btn-icon" data-link-remove="${i}" title="Excluir" aria-label="Excluir">
              <svg class="icon" aria-hidden="true"><use href="#i-trash"></use></svg>
            </button>
          </div>
        </li>`
        )
        .join("") || `<li class="empty">Nenhum link adicionado.</li>`;

    document.getElementById("hidden-links").innerHTML = linkItems
      .map(
        (item, i) =>
          `<input type="hidden" name="Links[${i}].Kind" value="${escapeHtml(item.kind || "repository").replaceAll('"', "&quot;")}" />` +
          `<input type="hidden" name="Links[${i}].Url" value="${escapeHtml(item.url).replaceAll('"', "&quot;")}" />`
      )
      .join("");
  };

  const linkModal = document.getElementById("link-modal");
  const linkKind = document.getElementById("link-kind");
  const linkUrl = document.getElementById("link-url");

  const openLinkModal = () => {
    linkKind.value = "repository";
    linkUrl.value = "";
    linkModal.hidden = false;
    linkUrl.focus();
  };

  const closeLinkModal = () => {
    linkModal.hidden = true;
  };

  const addLink = () => {
    let url = linkUrl.value.trim();
    if (!url) return;
    if (!/^https?:\/\//i.test(url) && !url.startsWith("mailto:")) url = `https://${url}`;
    linkItems.push({ kind: linkKind.value === "tool" ? "tool" : "repository", url });
    renderLinks();
  };

  document.getElementById("add-link").addEventListener("click", openLinkModal);
  document.getElementById("link-cancel").addEventListener("click", closeLinkModal);
  linkModal.addEventListener("click", (event) => {
    if (event.target === linkModal) closeLinkModal();
  });
  document.getElementById("link-save").addEventListener("click", () => {
    addLink();
    closeLinkModal();
  });
  linkUrl.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
      event.preventDefault();
      addLink();
      closeLinkModal();
    }
  });
  document.getElementById("links-list").addEventListener("click", (event) => {
    const remove = event.target.closest("[data-link-remove]");
    if (!remove) return;
    linkItems.splice(Number(remove.dataset.linkRemove), 1);
    renderLinks();
  });

  renderLinks();
})();
