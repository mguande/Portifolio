(() => {
  if (!window.PROFILE_LISTS) return;
  const data = {
    skills: window.PROFILE_LISTS.skills || [],
    socials: window.PROFILE_LISTS.socials || [],
    experience: window.PROFILE_LISTS.experience || [],
    education: window.PROFILE_LISTS.education || [],
    networks: window.PROFILE_LISTS.networks || [],
  };

  const labels = Object.fromEntries((data.networks || []).map((n) => [n.id, n.label]));
  const iconBtn = (action, type, index, label) =>
    `<button type="button" class="btn-icon" data-${action}="${type}" data-index="${index}" title="${label}" aria-label="${label}"><svg class="icon" aria-hidden="true"><use href="#i-${action === "edit" ? "edit" : "trash"}"></use></svg></button>`;
  let editing = { type: "", index: -1 };

  const networkOptions = () =>
    (data.networks || [])
      .map((n) => `<option value="${n.id}">${n.label}</option>`)
      .join("");

  const field = (name, label, value = "", type = "text") =>
    `<label>${label}</label><input data-field="${name}" type="${type}" value="${escapeAttr(value)}" />`;

  const area = (name, label, value = "") =>
    `<label>${label}</label><textarea data-field="${name}">${escapeHtml(value)}</textarea>`;

  const escapeHtml = (value) =>
    String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;");

  const escapeAttr = (value) =>
    escapeHtml(value).replaceAll('"', "&quot;");

  const render = () => {
    document.getElementById("skills-list").innerHTML = (data.skills || [])
      .map(
        (item, i) => `
        <li>
          <div><strong>${escapeHtml(item)}</strong></div>
          <div class="item-actions">
            ${iconBtn("edit", "skills", i, "Editar")}
            ${iconBtn("remove", "skills", i, "Excluir")}
          </div>
        </li>`
      )
      .join("") || `<li class="empty">Nenhuma competência cadastrada.</li>`;

    document.getElementById("socials-list").innerHTML = (data.socials || [])
      .map(
        (item, i) => `
        <li>
          <div>
            <strong>${escapeHtml(labels[item.network] || item.network)}</strong>
            <span>${escapeHtml(item.url)}</span>
          </div>
          <div class="item-actions">
            ${iconBtn("edit", "socials", i, "Editar")}
            ${iconBtn("remove", "socials", i, "Excluir")}
          </div>
        </li>`
      )
      .join("") || `<li class="empty">Nenhuma rede cadastrada.</li>`;

    document.getElementById("experience-list").innerHTML = (data.experience || [])
      .map(
        (item, i) => `
        <li>
          <div>
            <strong>${escapeHtml(item.title)}</strong>
            <span>${escapeHtml(item.org)} · ${escapeHtml(item.period)}</span>
          </div>
          <div class="item-actions">
            ${iconBtn("edit", "experience", i, "Editar")}
            ${iconBtn("remove", "experience", i, "Excluir")}
          </div>
        </li>`
      )
      .join("") || `<li class="empty">Nenhuma experiência cadastrada.</li>`;

    document.getElementById("education-list").innerHTML = (data.education || [])
      .map(
        (item, i) => `
        <li>
          <div>
            <strong>${escapeHtml(item.title)}</strong>
            <span>${escapeHtml(item.org)} · ${escapeHtml(item.period)}</span>
          </div>
          <div class="item-actions">
            ${iconBtn("edit", "education", i, "Editar")}
            ${iconBtn("remove", "education", i, "Excluir")}
          </div>
        </li>`
      )
      .join("") || `<li class="empty">Nenhuma formação cadastrada.</li>`;

    syncHidden();
  };

  const syncHidden = () => {
    const host = document.getElementById("hidden-lists");
    const parts = [];
    (data.skills || []).forEach((item, i) => {
      parts.push(`<input type="hidden" name="Skills[${i}]" value="${escapeAttr(item)}" />`);
    });
    (data.socials || []).forEach((item, i) => {
      parts.push(`<input type="hidden" name="Socials[${i}].Network" value="${escapeAttr(item.network)}" />`);
      parts.push(`<input type="hidden" name="Socials[${i}].Url" value="${escapeAttr(item.url)}" />`);
    });
    (data.experience || []).forEach((item, i) => {
      parts.push(`<input type="hidden" name="Experience[${i}].Period" value="${escapeAttr(item.period)}" />`);
      parts.push(`<input type="hidden" name="Experience[${i}].Title" value="${escapeAttr(item.title)}" />`);
      parts.push(`<input type="hidden" name="Experience[${i}].Org" value="${escapeAttr(item.org)}" />`);
      parts.push(`<input type="hidden" name="Experience[${i}].Detail" value="${escapeAttr(item.detail)}" />`);
    });
    (data.education || []).forEach((item, i) => {
      parts.push(`<input type="hidden" name="Education[${i}].Period" value="${escapeAttr(item.period)}" />`);
      parts.push(`<input type="hidden" name="Education[${i}].Title" value="${escapeAttr(item.title)}" />`);
      parts.push(`<input type="hidden" name="Education[${i}].Org" value="${escapeAttr(item.org)}" />`);
    });
    host.innerHTML = parts.join("");
  };

  const openModal = (type, index) => {
    window.htmlEditor?.remove("experience-detail");
    editing = { type, index };
    const modal = document.getElementById("item-modal");
    const title = document.getElementById("modal-title");
    const fields = document.getElementById("modal-fields");
    const item = index >= 0 ? data[type][index] : null;
    const isNew = index < 0;
    if (type === "skills") {
      title.textContent = isNew ? "Nova competência" : "Editar competência";
      fields.innerHTML = field("value", "Competência", item || "");
    } else if (type === "socials") {
      title.textContent = isNew ? "Nova rede" : "Editar rede";
      fields.innerHTML = `
        <label>Rede</label>
        <select data-field="network">${networkOptions()}</select>
        ${field("url", "URL", item?.url || "")}`;
      fields.querySelector("[data-field=network]").value = item?.network || "linkedin";
    } else if (type === "experience") {
      title.textContent = isNew ? "Nova experiência" : "Editar experiência";
      fields.innerHTML =
        field("period", "Período", item?.period || "") +
        field("title", "Cargo", item?.title || "") +
        field("org", "Empresa", item?.org || "") +
        `<label>Detalhe</label><textarea id="experience-detail" data-field="detail">${escapeHtml(item?.detail || "")}</textarea>`;
    } else {
      title.textContent = isNew ? "Nova formação" : "Editar formação";
      fields.innerHTML =
        field("period", "Período", item?.period || "") +
        field("title", "Curso", item?.title || "") +
        field("org", "Instituição", item?.org || "");
    }
    modal.querySelector(".modal").classList.toggle("modal-wide", type === "experience");
    modal.hidden = false;
    if (type === "experience") {
      window.htmlEditor?.init("#experience-detail", { height: 360 });
    } else {
      fields.querySelector("input, textarea, select")?.focus();
    }
  };

  const closeModal = () => {
    window.htmlEditor?.remove("experience-detail");
    document.getElementById("item-modal").hidden = true;
  };

  const readFields = () => {
    const values = {};
    document.querySelectorAll("#modal-fields [data-field]").forEach((el) => {
      values[el.dataset.field] = el.value.trim();
    });
    if (editing.type === "experience") {
      values.detail = (window.htmlEditor?.content("experience-detail") || values.detail || "").trim();
    }
    return values;
  };

  const saveItem = () => {
    const values = readFields();
    const { type, index } = editing;
    let item;
    if (type === "skills") item = values.value;
    else if (type === "socials") item = { network: values.network, url: values.url };
    else if (type === "experience") item = { period: values.period, title: values.title, org: values.org, detail: values.detail };
    else item = { period: values.period, title: values.title, org: values.org };

    if (type === "skills" && !item) return closeModal();
    if (type === "socials" && !item.url) return closeModal();
    if ((type === "experience" || type === "education") && !item.title && !item.org) return closeModal();

    if (index >= 0) data[type][index] = item;
    else data[type].push(item);
    closeModal();
    render();
  };

  document.addEventListener("click", (event) => {
    const add = event.target.closest("[data-add]");
    if (add) {
      openModal(add.dataset.add, -1);
      return;
    }
    const edit = event.target.closest("[data-edit]");
    if (edit) {
      openModal(edit.dataset.edit, Number(edit.dataset.index));
      return;
    }
    const remove = event.target.closest("[data-remove]");
    if (remove) {
      const type = remove.dataset.remove;
      data[type].splice(Number(remove.dataset.index), 1);
      render();
    }
  });

  document.getElementById("modal-save").addEventListener("click", saveItem);
  document.getElementById("modal-cancel").addEventListener("click", closeModal);
  document.getElementById("item-modal").addEventListener("click", (event) => {
    if (event.target.id === "item-modal") closeModal();
  });

  const photoInput = document.getElementById("photo-input");
  const preview = document.getElementById("photo-preview");
  const hint = document.getElementById("photo-hint");
  photoInput?.addEventListener("change", () => {
    const file = photoInput.files?.[0];
    if (!file) return;
    preview.src = URL.createObjectURL(file);
    preview.classList.remove("is-empty");
    hint.textContent = file.name;
  });

  render();
})();
