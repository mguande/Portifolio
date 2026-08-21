(() => {
  const stats = Array.isArray(window.COPY_STATS) ? window.COPY_STATS : [];
  let editingIndex = -1;

  const escapeHtml = (value) =>
    String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;");

  const escapeAttr = (value) => escapeHtml(value).replaceAll('"', "&quot;");

  const render = () => {
    document.getElementById("stats-list").innerHTML =
      stats
        .map(
          (item, i) => `
        <li>
          <div>
            <strong>${escapeHtml(item.value)}</strong>
            <span>${escapeHtml(item.label)}</span>
          </div>
          <div class="item-actions">
            <button type="button" class="btn-icon" data-edit="${i}" title="Editar" aria-label="Editar"><svg class="icon" aria-hidden="true"><use href="#i-edit"></use></svg></button>
            <button type="button" class="btn-icon" data-remove="${i}" title="Excluir" aria-label="Excluir"><svg class="icon" aria-hidden="true"><use href="#i-trash"></use></svg></button>
          </div>
        </li>`
        )
        .join("") || `<li class="empty">Nenhum número cadastrado.</li>`;

    document.getElementById("hidden-stats").innerHTML = stats
      .map(
        (item, i) =>
          `<input type="hidden" name="Stats[${i}].Value" value="${escapeAttr(item.value)}" />` +
          `<input type="hidden" name="Stats[${i}].Label" value="${escapeAttr(item.label)}" />`
      )
      .join("");
  };

  const modal = document.getElementById("stat-modal");
  const valueInput = document.getElementById("stat-value");
  const labelInput = document.getElementById("stat-label");
  const title = document.getElementById("stat-modal-title");

  const openModal = (index) => {
    editingIndex = index;
    const item = index >= 0 ? stats[index] : { value: "", label: "" };
    title.textContent = index >= 0 ? "Editar número" : "Novo número";
    valueInput.value = item.value || "";
    labelInput.value = item.label || "";
    modal.hidden = false;
    valueInput.focus();
  };

  const closeModal = () => {
    modal.hidden = true;
  };

  document.getElementById("add-stat").addEventListener("click", () => openModal(-1));
  document.getElementById("stat-cancel").addEventListener("click", closeModal);
  modal.addEventListener("click", (event) => {
    if (event.target === modal) closeModal();
  });
  document.getElementById("stat-save").addEventListener("click", () => {
    const item = { value: valueInput.value.trim(), label: labelInput.value.trim() };
    if (!item.value && !item.label) return closeModal();
    if (editingIndex >= 0) stats[editingIndex] = item;
    else stats.push(item);
    closeModal();
    render();
  });

  document.getElementById("stats-list").addEventListener("click", (event) => {
    const edit = event.target.closest("[data-edit]");
    if (edit) {
      openModal(Number(edit.dataset.edit));
      return;
    }
    const remove = event.target.closest("[data-remove]");
    if (remove) {
      stats.splice(Number(remove.dataset.remove), 1);
      render();
    }
  });

  render();
})();
