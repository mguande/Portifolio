(function () {
  const data = window.PORTFOLIO;
  if (!data) return;

  const $ = (sel, root = document) => root.querySelector(sel);
  const pathBase = String(window.PATH_BASE || "").replace(/\/$/, "");
  const asset = (path) => {
    if (!path) return "";
    if (/^(https?:|data:|mailto:)/i.test(path)) return path;
    return `${pathBase}/${String(path).replace(/^\/+/, "")}`;
  };

  const socialIcons = {
    linkedin:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M4.98 3.5A2.5 2.5 0 1 1 5 8.5a2.5 2.5 0 0 1-.02-5zM4 9h2v12H4zM9 9h1.92v1.64h.03c.27-.5.93-1.64 2.4-1.64C16.4 9 18 10.7 18 13.6V21h-2v-6.5c0-1.7-.6-2.86-2.12-2.86-1.16 0-1.85.78-2.16 1.53-.11.27-.14.64-.14 1V21H9z"/></svg>',
    github:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 2a10 10 0 0 0-3.16 19.49c.5.09.68-.22.68-.48v-1.7c-2.78.6-3.37-1.34-3.37-1.34-.45-1.16-1.1-1.47-1.1-1.47-.9-.62.07-.6.07-.6 1 .07 1.53 1.03 1.53 1.03.9 1.52 2.34 1.08 2.91.83.09-.65.35-1.08.63-1.33-2.22-.25-4.56-1.11-4.56-4.95 0-1.1.39-1.99 1.03-2.7-.1-.25-.45-1.27.1-2.64 0 0 .84-.27 2.75 1.02A9.56 9.56 0 0 1 12 6.8c.85 0 1.71.11 2.51.32 1.9-1.29 2.74-1.02 2.74-1.02.55 1.37.2 2.39.1 2.64.64.71 1.03 1.6 1.03 2.7 0 3.85-2.34 4.7-4.57 4.95.36.31.68.92.68 1.85v2.74c0 .27.18.58.69.48A10 10 0 0 0 12 2z"/></svg>',
    gitlab:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 21.2 16.7 6.8h-2.2L12 13.1 9.5 6.8H7.3L12 21.2zM2.2 14.1.1 8.2a.54.54 0 0 1 .2-.6L12 21.2 2.2 14.1zm4-7.3L4.8 2.2a.5.5 0 0 0-.95 0L2.2 14.1 6.2 6.8zm15.6 7.3L12 21.2l9.8-7.1 2.1-5.9a.54.54 0 0 0-.2-.6L21.8 14.1zM17.8 6.8l1.55-4.6a.5.5 0 0 1 .95 0l1.5 4.6-4 7.3z"/></svg>',
    instagram:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M7 3h10a4 4 0 0 1 4 4v10a4 4 0 0 1-4 4H7a4 4 0 0 1-4-4V7a4 4 0 0 1 4-4zm10 2H7a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2zm-5 3.2A3.8 3.8 0 1 1 8.2 12 3.8 3.8 0 0 1 12 8.2zm0 2A1.8 1.8 0 1 0 13.8 12 1.8 1.8 0 0 0 12 10.2zM17.4 6.6a1 1 0 1 1-1 1 1 1 0 0 1 1-1z"/></svg>',
    x:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M14.7 10.3 22 2h-2.2l-6.3 7.2L8.4 2H2l7.8 11.1L2 22h2.2l6.9-7.8L15.6 22H22l-7.3-11.7zm-2.4 2.8-.8-1.1L4.9 3.5h2.7l5.1 7.3.8 1.1 8.3 11.9h-2.7l-5.8-8.7z"/></svg>',
    facebook:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M14 9h3V6h-3c-2.2 0-4 1.8-4 4v2H8v3h2v7h3v-7h2.6l.4-3H13v-2c0-.6.4-1 1-1z"/></svg>',
    youtube:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M23 12.2s0-3.2-.4-4.6c-.2-.9-.9-1.6-1.8-1.8C19.2 5.4 12 5.4 12 5.4s-7.2 0-8.8.4c-.9.2-1.6.9-1.8 1.8C1 9 1 12.2 1 12.2s0 3.2.4 4.6c.2.9.9 1.6 1.8 1.8 1.6.4 8.8.4 8.8.4s7.2 0 8.8-.4c.9-.2 1.6-.9 1.8-1.8.4-1.4.4-4.6.4-4.6zM9.8 15.6V8.8l6.2 3.4-6.2 3.4z"/></svg>',
    lattes:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M6 3h12a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2zm2 4v2h8V7H8zm0 4v2h8v-2H8zm0 4v2h5v-2H8z"/></svg>',
    website:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 2a10 10 0 1 0 10 10A10 10 0 0 0 12 2zm7.4 9h-3.2a15 15 0 0 0-1.3-5 8.1 8.1 0 0 1 4.5 5zM12 4c.7 0 2.3 2.3 2.9 7H9.1C9.7 6.3 11.3 4 12 4zM4.6 13h3.2a15 15 0 0 0 1.3 5A8.1 8.1 0 0 1 4.6 13zm3.2-2H4.6A8.1 8.1 0 0 1 9.1 6a15 15 0 0 0-1.3 5zM12 20c-.7 0-2.3-2.3-2.9-7h5.8C14.3 17.7 12.7 20 12 20zm2.9-2a15 15 0 0 0 1.3-5h3.2a8.1 8.1 0 0 1-4.5 5z"/></svg>',
    email:
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M3 6h18v12H3V6zm9 6.5L4.6 8h14.8L12 12.5zM4 17.2V9.3l8 4.6 8-4.6v7.9H4z"/></svg>',
  };

  const socialLabels = {
    linkedin: "LinkedIn",
    github: "GitHub",
    gitlab: "GitLab",
    instagram: "Instagram",
    x: "X",
    facebook: "Facebook",
    youtube: "YouTube",
    lattes: "Lattes",
    website: "Site",
    email: "E-mail",
  };

  const collectSocials = (person) => {
    const items = [];
    const seen = new Set();
    const add = (network, url) => {
      if (!url || seen.has(network + url)) return;
      seen.add(network + url);
      items.push({ network, url });
    };

    (person.socials || []).forEach((item) => add(item.network, item.url));
    add("linkedin", person.linkedin);
    add("github", person.github);
    add("gitlab", person.gitlab);
    if (person.email) add("email", `mailto:${person.email}`);
    return items;
  };

  const renderSocialLink = (item) => {
    const label = socialLabels[item.network] || item.network;
    const icon = socialIcons[item.network] || socialIcons.website;
    const extra = item.network === "email" ? "" : ' target="_blank" rel="noreferrer"';
    return `<a class="social" href="${item.url}"${extra} title="${label}" aria-label="${label}">${icon}<span>${label}</span></a>`;
  };

  const renderProjectLink = (item) => {
    const url = String(item.url || "");
    const kind = item.kind === "tool" ? "tool" : "repository";
    const lower = url.toLowerCase();
    let network = "website";
    let label = "Link";
    if (kind === "repository") {
      label = "Repositório";
      if (lower.includes("github.com")) network = "github";
      else if (lower.includes("gitlab.com")) network = "gitlab";
      else network = "github";
    }
    const icon = socialIcons[network] || socialIcons.website;
    return `<a class="social" href="${url}" target="_blank" rel="noreferrer" title="${label}" aria-label="${label}">${icon}<span>${label}</span></a>`;
  };

  const initials = (person) => {
    const source = person.shortName || person.name || "?";
    return source
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0].toUpperCase())
      .join("");
  };

  const displayName = (person) => {
    const parts = String(person.name || person.shortName || "").split(/\s+/).filter(Boolean);
    if (parts.length <= 2) return parts.join(" ");
    return `${parts[0]} ${parts[parts.length - 1]}`;
  };

  const yearsInField = (person) => {
    const years = (person.experience || [])
      .flatMap((item) => [...String(item.period || "").matchAll(/\d{4}/g)].map((m) => Number(m[0])));
    if (!years.length) return null;
    return new Date().getFullYear() - Math.min(...years);
  };

  const startYear = (period) => {
    const match = String(period || "").match(/(\d{4})/);
    return match ? match[1] : "";
  };

  const latestYear = (value) => {
    const years = [...String(value || "").matchAll(/\d{4}/g)].map((m) => Number(m[0]));
    return years.length ? Math.max(...years) : 0;
  };

  const byNewest = (items) =>
    [...items].sort((a, b) => latestYear(b.year) - latestYear(a.year) || latestYear(b.period) - latestYear(a.period));

  const clip = (text, max = 220) => {
    const value = String(text || "").trim();
    if (value.length <= max) return value;
    const cut = value.slice(0, max);
    const at = cut.lastIndexOf(" ");
    return `${cut.slice(0, at > 80 ? at : max)}…`;
  };

  const personById = (id) => data.people.find((p) => p.id === id);

  const avatar = (person, className = "avatar") =>
    person.photo
      ? `<img${className ? ` class="${className}"` : ""} src="${asset(person.photo)}" alt="${person.shortName || person.name}" />`
      : `<span class="${className || "initials"}" aria-hidden="true">${initials(person)}</span>`;

  const italicizeIntro = (text) => {
    const value = String(text || "");
    return value
      .replace("juntos e em separado", "<i>juntos e em separado</i>")
      .replace("clareza de propósito", "<i>clareza de propósito</i>")
      .replace("visão de negócio", "<i>visão de negócio</i>")
      .replace("rigor técnico", "<i>rigor técnico</i>");
  };

  const renderMeta = () => {
    const people = data.people || [];
    const studio = data.studio || {};
    const copy = data.copy || {};
    const pick = (value, fallback) => (value && String(value).trim()) || fallback;
    const letters = people.map((p) => (p.shortName || p.name || "?").charAt(0).toUpperCase());
    $("#studio-mark").innerHTML = letters.length
      ? letters.join('<span class="dot">·</span>')
      : studio.name || "Portfólio";

    document.title = people.length
      ? `${people.map(displayName).join(" & ")} — ${pick(studio.tagline, "Produto, Arquitetura & Tecnologia")}`
      : `${studio.name} — Portfólio`;
    const navParts = pick(copy.navLine, "Produto / Arquitetura / Tecnologia").split(/\s*\/\s*/);
    $("#hero-nav").innerHTML = navParts
      .map((part) => part.trim())
      .filter(Boolean)
      .join(' <span class="dot">/</span> ');
    $("#hero-eyebrow").textContent = pick(copy.eyebrow, "Portfólio Profissional");
    $("#hero-title").innerHTML = people
      .map((p, i) => (i === 0 ? displayName(p) : `<span class="amp">&</span><br>${displayName(p)}`))
      .join(" ");
    $("#hero-lead").innerHTML = studio.intro || studio.tagline || "";

    const statsSource = (copy.stats || []).filter((item) => item && (item.value || item.label));
    $("#hero-stats").innerHTML = (statsSource.length
      ? statsSource
      : people.slice(0, 2).map((person) => {
          const years = yearsInField(person);
          return { value: years ? `${years}+` : "—", label: `${person.role} · ${person.shortName}` };
        })
    )
      .map((item) => `<div class="stat"><b>${item.value}</b><span>${item.label}</span></div>`)
      .join("");

    $("#duo-photo").innerHTML = `
      <div class="ring"></div>
      ${people
        .slice(0, 2)
        .map((person, i) => `<div class="circle c${i + 1}">${avatar(person, "")}</div>`)
        .join("")}
      <div class="venn-label">${pick(copy.vennLabel, "produto ∩ engenharia")}</div>`;

    $("#mission-text").innerHTML = italicizeIntro(pick(copy.mission, studio.intro || studio.tagline || ""));
    $("#perfis-title").textContent = pick(copy.profilesTitle, "Quem somos");
    $("#perfis-lead").innerHTML = pick(copy.profilesLead, "Formação, competências e canais de cada um.");
    $("#trabalho-title").textContent = pick(copy.projectsTitle, "Projetos & atuações relevantes");
    $("#projetos-lead").innerHTML = pick(
      copy.projectsLead,
      "Filtre por trabalhos feitos em dupla ou pelo histórico de cada profissional."
    );
    $("#stack-title").textContent = pick(copy.stackTitle, "Ferramentas & tecnologias");
    $("#cta-eyebrow").textContent = pick(copy.ctaEyebrow, "Vamos conversar");
    $("#contato-title").innerHTML = pick(copy.ctaTitle, studio.tagline || "Vamos conversar");
    $("#cta-lead").innerHTML = pick(
      copy.ctaLead,
      "Atendemos startups e empresas em transformação digital com um time enxuto, experiente e acostumado a ambientes de alta exigência técnica."
    );
    $("#footer-name").textContent = studio.name;
    $("#footer-location").textContent = studio.location || "";
    $("#year").textContent = String(new Date().getFullYear());

    $("#cta-btns").innerHTML = people
      .map(
        (person, i) =>
          `<a class="btn ${i === 0 ? "primary" : "outline"}" href="mailto:${person.email || studio.email}">Falar com ${person.shortName}</a>`
      )
      .join("");

    const studioCol = `
      <div>
        <h4>${letters.join(" · ") || studio.name}</h4>
        <div class="rich">${pick(copy.footerBlurb, studio.intro || studio.tagline || "")}</div>
      </div>`;
    const peopleCols = people
      .map((person) => {
        const linkedin = collectSocials(person).find((s) => s.network === "linkedin");
        return `
          <div>
            <h4>${displayName(person)}</h4>
            ${person.email ? `<a href="mailto:${person.email}">${person.email}</a>` : ""}
            ${linkedin ? `<a href="${linkedin.url}" target="_blank" rel="noreferrer">LinkedIn</a>` : ""}
            ${person.location ? `<p>${person.location}</p>` : ""}
          </div>`;
      })
      .join("");
    $("#footer-grid").innerHTML = studioCol + peopleCols;
  };

  const experienceTimeline = (person) => {
    const source = (person.experience || []).map((item) => ({
      year: item.period || startYear(item.period),
      title: item.title,
      org: item.org || "",
      detail: item.detail || "",
    }));
    const items = byNewest(source)
      .map(
        (item) => `
        <div class="titem">
          <span class="yr">${item.year || ""}</span>
          <b>${item.title}</b>
          <p>${item.org || ""}</p>
          ${item.detail ? `<div class="rich titem-detail">${item.detail}</div>` : ""}
        </div>`
      )
      .join("");
    return items
      ? `<div class="tline-scroll"><div class="tline">${items}</div></div>`
      : `<p class="muted">Nenhuma experiência cadastrada.</p>`;
  };

  const renderFilters = () => {
    const host = $("#filters");
    const buttons = [
      { id: "all", label: "Todos" },
      { id: "joint", label: "Conjuntos" },
      ...data.people.map((p) => ({ id: p.id, label: p.shortName })),
    ];

    host.innerHTML = buttons
      .map(
        (btn, i) => `
        <button class="filter" type="button" data-filter="${btn.id}" aria-pressed="${i === 0}">
          ${btn.label}
        </button>`
      )
      .join("");
  };

  const isJointProject = (project) => {
    const people = data.people || [];
    const authors = project.authors || [];
    return people.length > 1 && people.every((p) => authors.includes(p.id));
  };

  const ownerMarkup = (project) => {
    const people = data.people || [];
    const authors = project.authors || [];
    const selected = people.filter((p) => authors.includes(p.id));
    const badges = selected
      .map((p, i) => `<div class="badge${i ? " teal" : ""}">${initials(p)}</div>`)
      .join("");
    if (isJointProject(project)) return `${badges} Projeto conjunto`;
    if (selected.length === 1) return `${badges} Individual · ${selected[0].shortName}`;
    if (selected.length > 1) return `${badges} ${selected.map((p) => p.shortName).join(" · ")}`;
    return "Individual";
  };

  const renderProjects = (filter = "all") => {
    const host = $("#projects");
    host.innerHTML = (data.projects || [])
      .map((project) => {
        const authors = project.authors || [];
        const visible =
          filter === "all" ||
          (filter === "joint" && isJointProject(project)) ||
          authors.includes(filter);

        return `
          <article class="proj-card${visible ? " reveal in" : " is-hidden"}" data-authors="${authors.join(" ")}" data-authorship="${project.authorship}">
            <span class="proj-tag">${project.sector} · ${project.year}</span>
            <h3>${project.title}</h3>
            <div class="rich">${project.summary}</div>
            ${
              (project.stack || []).length
                ? `<p class="proj-stack">${(project.stack || []).join(" · ")}</p>`
                : ""
            }
            <div class="proj-owner">${ownerMarkup(project)}</div>
            <div class="proj-links">${(project.links || []).map(renderProjectLink).join("")}</div>
          </article>`;
      })
      .join("");
  };

  const renderPeople = () => {
    $("#people").innerHTML = (data.people || [])
      .map((person) => {
        const skills = (person.skills || []).map((s) => `<li>${s}</li>`).join("");
        const socials = collectSocials(person).map(renderSocialLink).join("");
        const education = (person.education || [])
          .slice(0, 2)
          .map(
            (item) => `
            <li>
              <time>${item.period}</time>
              <strong>${item.title}</strong>
              <span>${item.org}</span>
            </li>`
          )
          .join("");

        return `
          <article class="prof-card" id="perfil-${person.id}">
            <div class="prof-top">
              ${avatar(person)}
              <div>
                <h3>${displayName(person)}</h3>
                <div class="prof-socials">${socials}</div>
                <span class="prof-role">${person.role}</span>
              </div>
            </div>
            <div class="bio rich">${person.bio || person.summary || ""}</div>
            <div class="comp-label">Competências-chave</div>
            <ul class="comp-list">${skills}</ul>
            <div class="comp-label">Histórico recente</div>
            ${experienceTimeline(person)}
            <div class="comp-label">Formação</div>
            <ol class="mini-timeline">${education}</ol>
          </article>`;
      })
      .join("");
  };

  const renderStack = () => {
    const items = (data.copy && data.copy.stack) || [];
    $("#stack-row").innerHTML = items
      .map((item, i) => `<div class="stack-item">${i % 2 ? item : `<b>${item}</b>`}</div>`)
      .join("");
  };

  const reveal = () => {
    const nodes = document.querySelectorAll(".reveal");
    if (!("IntersectionObserver" in window)) {
      nodes.forEach((el) => el.classList.add("in"));
      return;
    }
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) return;
          entry.target.classList.add("in");
          observer.unobserve(entry.target);
        });
      },
      { threshold: 0.12 }
    );
    nodes.forEach((el) => observer.observe(el));
  };

  renderMeta();
  renderFilters();
  renderProjects();
  renderPeople();
  renderStack();
  reveal();

  $("#filters").addEventListener("click", (event) => {
    const button = event.target.closest("[data-filter]");
    if (!button) return;
    $("#filters")
      .querySelectorAll(".filter")
      .forEach((el) => el.setAttribute("aria-pressed", "false"));
    button.setAttribute("aria-pressed", "true");
    renderProjects(button.dataset.filter);
  });
})();
