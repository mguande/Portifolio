using System.ComponentModel;

namespace PortfolioEditor;

internal sealed class MainForm : Form
{
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly Label _pathLabel = new();
    private string _filePath = "";
    private Portfolio _data = new();
    private Person? _currentPerson;
    private Project? _currentProject;
    private bool _suppress;

    private readonly TextBox _studioName = NewBox();
    private readonly TextBox _studioTagline = NewBox();
    private readonly TextBox _studioLocation = NewBox();
    private readonly TextBox _studioEmail = NewBox();
    private readonly TextBox _studioPhone = NewBox();
    private readonly TextBox _studioIntro = NewMultiline(5);

    private readonly TextBox _copyEyebrow = NewBox();
    private readonly TextBox _copyNav = NewBox();
    private readonly TextBox _copyVenn = NewBox();
    private readonly TextBox _copyMission = NewMultiline(4);
    private readonly TextBox _copyTrajectoryTitle = NewBox();
    private readonly TextBox _copyTrajectoryLead = NewMultiline(3);
    private readonly DataGridView _statsGrid = NewGrid();
    private readonly DataGridView _milestonesGrid = NewGrid();
    private readonly TextBox _copyProfilesTitle = NewBox();
    private readonly TextBox _copyProfilesLead = NewMultiline(3);
    private readonly TextBox _copyProjectsTitle = NewBox();
    private readonly TextBox _copyProjectsLead = NewMultiline(3);
    private readonly TextBox _copyStackTitle = NewBox();
    private readonly TextBox _copyStack = NewMultiline(4);
    private readonly TextBox _copyCtaEyebrow = NewBox();
    private readonly TextBox _copyCtaTitle = NewMultiline(3);
    private readonly TextBox _copyCtaLead = NewMultiline(3);
    private readonly TextBox _copyFooterBlurb = NewMultiline(3);

    private readonly ListBox _peopleList = new() { Dock = DockStyle.Fill };
    private readonly TextBox _personId = NewBox();
    private readonly TextBox _personName = NewBox();
    private readonly TextBox _personShort = NewBox();
    private readonly TextBox _personPhoto = NewBox();
    private readonly TextBox _personRole = NewBox();
    private readonly TextBox _personBio = NewMultiline(3);
    private readonly TextBox _personLocation = NewBox();
    private readonly TextBox _personEmail = NewBox();
    private readonly DataGridView _socialsGrid = NewGrid();
    private readonly TextBox _personSummary = NewMultiline(4);
    private readonly TextBox _personSkills = NewMultiline(5);
    private readonly DataGridView _experienceGrid = NewGrid();
    private readonly DataGridView _educationGrid = NewGrid();

    private readonly ListBox _projectList = new() { Dock = DockStyle.Fill };
    private readonly TextBox _projectId = NewBox();
    private readonly TextBox _projectYear = NewBox();
    private readonly TextBox _projectTitle = NewBox();
    private readonly TextBox _projectSector = NewBox();
    private readonly ComboBox _projectAuthorship = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly TextBox _projectSummary = NewMultiline(4);
    private readonly TextBox _projectOutcome = NewMultiline(3);
    private readonly TextBox _projectStack = NewMultiline(3);

    public MainForm()
    {
        Text = "Editor do portfólio";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 680);
        Size = new Size(1120, 760);
        Font = new Font("Segoe UI", 9.75f);
        BackColor = Color.FromArgb(246, 244, 239);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16),
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeader(), 0, 0);

        _tabs.TabPages.Add(BuildStudioTab());
        _tabs.TabPages.Add(BuildCopyTab());
        _tabs.TabPages.Add(BuildPeopleTab());
        _tabs.TabPages.Add(BuildProjectsTab());
        root.Controls.Add(_tabs, 0, 1);
        root.Controls.Add(BuildFooter(), 0, 2);

        Controls.Add(root);

        _peopleList.SelectedIndexChanged += (_, _) => ShowSelectedPerson();
        _projectList.SelectedIndexChanged += (_, _) => ShowSelectedProject();
        _projectAuthorship.SelectedIndexChanged += (_, _) => ApplyAuthorshipFromCombo();

        Shown += (_, _) =>
        {
            _filePath = ContentFile.FindDefaultPath();
            if (File.Exists(_filePath))
                LoadFrom(_filePath);
            else
                ChooseFile(createIfMissing: true);
        };

        FormClosing += (_, e) =>
        {
            PullUiIntoModel();
        };
    }

    private Control BuildHeader()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(0, 0, 0, 10),
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Preenchimento do content.js",
            AutoSize = true,
            Font = new Font("Segoe UI", 16f, FontStyle.Regular),
            Margin = new Padding(0, 0, 0, 6),
        };

        _pathLabel.AutoSize = true;
        _pathLabel.ForeColor = Color.FromArgb(90, 88, 82);
        _pathLabel.Margin = new Padding(0, 8, 12, 0);

        var browse = NewButton("Localizar arquivo…");
        browse.Click += (_, _) => ChooseFile(createIfMissing: false);

        panel.Controls.Add(title, 0, 0);
        panel.SetColumnSpan(title, 3);
        panel.Controls.Add(_pathLabel, 0, 1);
        panel.Controls.Add(browse, 2, 1);
        return panel;
    }

    private Control BuildFooter()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 0),
        };

        var save = NewButton("Salvar");
        save.BackColor = Color.FromArgb(47, 68, 84);
        save.ForeColor = Color.White;
        save.FlatStyle = FlatStyle.Flat;
        save.Click += (_, _) => Save();

        var reload = NewButton("Recarregar");
        reload.Click += (_, _) =>
        {
            if (File.Exists(_filePath))
                LoadFrom(_filePath);
        };

        panel.Controls.Add(save);
        panel.Controls.Add(reload);
        return panel;
    }

    private TabPage BuildStudioTab()
    {
        var page = new TabPage("Estúdio");
        var grid = NewFormGrid();
        AddRow(grid, "Nome do estúdio", _studioName);
        AddRow(grid, "Frase de capa", _studioTagline);
        AddRow(grid, "Local", _studioLocation);
        AddRow(grid, "E-mail", _studioEmail);
        AddRow(grid, "Telefone", _studioPhone);
        AddRow(grid, "Texto do hero", _studioIntro, 90);
        page.Controls.Add(grid);
        return page;
    }

    private TabPage BuildCopyTab()
    {
        var page = new TabPage("Textos do site");
        var grid = NewFormGrid();
        AddRow(grid, "Selo do hero", _copyEyebrow);
        AddRow(grid, "Linha do menu", _copyNav);
        AddRow(grid, "Rótulo das fotos", _copyVenn);
        AddRow(grid, "Missão", _copyMission, 90);
        AddRow(grid, "Números do hero", _statsGrid, 110);
        AddRow(grid, "Título da trajetória", _copyTrajectoryTitle);
        AddRow(grid, "Texto da trajetória", _copyTrajectoryLead, 70);
        AddRow(grid, "Marcos da trajetória", _milestonesGrid, 200);
        AddRow(grid, "Título de quem somos", _copyProfilesTitle);
        AddRow(grid, "Texto de quem somos", _copyProfilesLead, 70);
        AddRow(grid, "Título dos projetos", _copyProjectsTitle);
        AddRow(grid, "Texto dos projetos", _copyProjectsLead, 70);
        AddRow(grid, "Título da stack", _copyStackTitle);
        AddRow(grid, "Stack (uma por linha)", _copyStack, 90);
        AddRow(grid, "Selo do contato", _copyCtaEyebrow);
        AddRow(grid, "Título do contato", _copyCtaTitle, 70);
        AddRow(grid, "Texto do contato", _copyCtaLead, 70);
        AddRow(grid, "Texto do rodapé", _copyFooterBlurb, 70);
        ConfigureStatsGrid();
        ConfigureMilestonesGrid();
        page.Controls.Add(grid);
        return page;
    }

    private TabPage BuildPeopleTab()
    {
        var page = new TabPage("Perfis");
        var split = NewColumns(220);
        var left = BuildListPanel(_peopleList, BuildPeopleButtons());
        split.Controls.Add(left, 0, 0);

        var right = NewFormGrid();
        AddRow(right, "Id", _personId);
        AddRow(right, "Nome completo", _personName);
        AddRow(right, "Nome curto", _personShort);
        AddRow(right, "Foto do layout", BuildPhotoRow());
        AddRow(right, "Função", _personRole);
        AddRow(right, "Cidade", _personLocation);
        AddRow(right, "E-mail", _personEmail);
        AddRow(right, "Redes sociais", _socialsGrid, 120);
        AddRow(right, "Texto do card", _personBio, 70);
        AddRow(right, "Resumo (LinkedIn)", _personSummary, 80);
        AddRow(right, "Competências (uma por linha)", _personSkills, 90);
        AddRow(right, "Histórico profissional", _experienceGrid, 160);
        AddRow(right, "Formação", _educationGrid, 110);

        ConfigureExperienceGrid();
        ConfigureEducationGrid();
        ConfigureSocialsGrid();

        split.Controls.Add(right, 1, 0);
        page.Controls.Add(split);
        return page;
    }

    private TabPage BuildProjectsTab()
    {
        var page = new TabPage("Projetos");
        var split = NewColumns(280);
        var left = BuildListPanel(_projectList, BuildProjectButtons());
        split.Controls.Add(left, 0, 0);

        var right = NewFormGrid();
        AddRow(right, "Id", _projectId);
        AddRow(right, "Ano", _projectYear);
        AddRow(right, "Título", _projectTitle);
        AddRow(right, "Setor", _projectSector);
        AddRow(right, "Autoria", _projectAuthorship);
        AddRow(right, "Resumo", _projectSummary, 90);
        AddRow(right, "Resultado", _projectOutcome, 70);
        AddRow(right, "Stack (uma por linha)", _projectStack, 70);
        split.Controls.Add(right, 1, 0);
        page.Controls.Add(split);
        return page;
    }

    private void ConfigureStatsGrid()
    {
        _statsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _statsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(StatLine.Value), HeaderText = "Número", FillWeight = 22 });
        _statsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(StatLine.Label), HeaderText = "Texto", FillWeight = 78 });
    }

    private void ConfigureMilestonesGrid()
    {
        _milestonesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _milestonesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Milestone.PersonId), HeaderText = "Id do perfil", FillWeight = 12 });
        _milestonesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Milestone.Year), HeaderText = "Ano", FillWeight = 12 });
        _milestonesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Milestone.Title), HeaderText = "Título", FillWeight = 28 });
        _milestonesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Milestone.Text), HeaderText = "Texto", FillWeight = 48 });
        _milestonesGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _milestonesGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
    }

    private void ConfigureSocialsGrid()
    {
        _socialsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        var network = new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(SocialLink.Network),
            HeaderText = "Rede",
            FillWeight = 32,
            FlatStyle = FlatStyle.Flat,
            DisplayMember = "Label",
            ValueMember = "Id",
            DataSource = SocialNetworks.All,
        };
        _socialsGrid.Columns.Add(network);
        _socialsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(SocialLink.Url),
            HeaderText = "Link",
            FillWeight = 68,
        });
        _socialsGrid.DefaultValuesNeeded += (_, e) =>
        {
            e.Row.Cells[0].Value = "linkedin";
        };
        _socialsGrid.DataError += (_, e) => e.ThrowException = false;
    }

    private void ConfigureExperienceGrid()
    {
        _experienceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _experienceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Period), HeaderText = "Período", FillWeight = 18 });
        _experienceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Title), HeaderText = "Cargo", FillWeight = 24 });
        _experienceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Org), HeaderText = "Empresa", FillWeight = 20 });
        _experienceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Detail), HeaderText = "Detalhe", FillWeight = 38 });
        _experienceGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _experienceGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
    }

    private void ConfigureEducationGrid()
    {
        _educationGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _educationGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Education.Period), HeaderText = "Período", FillWeight = 22 });
        _educationGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Education.Title), HeaderText = "Curso", FillWeight = 48 });
        _educationGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Education.Org), HeaderText = "Instituição", FillWeight = 30 });
    }

    private void LoadFrom(string path)
    {
        try
        {
            _data = ContentFile.Load(path);
            _filePath = path;
            _pathLabel.Text = path;
            PushModelIntoUi();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Não foi possível ler o arquivo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                ChooseFile(createIfMissing: true);
                if (string.IsNullOrWhiteSpace(_filePath))
                    return;
            }

            PullUiIntoModel();
            if (File.Exists(_filePath))
            {
                var existing = ContentFile.Load(_filePath);
                if (string.IsNullOrWhiteSpace(_data.Studio.Name) && !string.IsNullOrWhiteSpace(existing.Studio.Name))
                    _data.Studio = existing.Studio;
                if (_data.Copy.IsBlank() && !existing.Copy.IsBlank())
                    _data.Copy = existing.Copy;
                else
                    _data.Copy.FillEmptyFrom(existing.Copy);
                if (_data.Principles.Count == 0 && existing.Principles.Count > 0)
                    _data.Principles = existing.Principles;
            }

            ContentFile.Save(_filePath, _data);
            PushModelIntoUi();
            MessageBox.Show(this, "content.js atualizado.", "Salvo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Não foi possível salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ChooseFile(bool createIfMissing)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "content.js|content.js|Arquivos JS|*.js|Todos|*.*",
            FileName = "content.js",
            CheckFileExists = !createIfMissing,
        };

        if (File.Exists(_filePath))
            dialog.InitialDirectory = Path.GetDirectoryName(_filePath);

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _filePath = dialog.FileName;
        if (!File.Exists(_filePath) && createIfMissing)
        {
            _data = new Portfolio();
            ContentFile.Save(_filePath, _data);
        }

        if (File.Exists(_filePath))
            LoadFrom(_filePath);
    }

    private void PushModelIntoUi()
    {
        _suppress = true;
        _studioName.Text = _data.Studio.Name;
        _studioTagline.Text = _data.Studio.Tagline;
        _studioLocation.Text = _data.Studio.Location;
        _studioEmail.Text = _data.Studio.Email;
        _studioPhone.Text = _data.Studio.Phone;
        _studioIntro.Text = _data.Studio.Intro;
        PushCopyIntoUi();

        RefreshPeopleList(select: _data.People.FirstOrDefault());
        RefreshProjectList(select: _data.Projects.FirstOrDefault());
        _suppress = false;
        ShowSelectedPerson();
        ShowSelectedProject();
    }

    private void PullUiIntoModel()
    {
        if (_suppress)
            return;

        _data.Studio.Name = _studioName.Text.Trim();
        _data.Studio.Tagline = _studioTagline.Text.Trim();
        _data.Studio.Location = _studioLocation.Text.Trim();
        _data.Studio.Email = _studioEmail.Text.Trim();
        _data.Studio.Phone = _studioPhone.Text.Trim();
        _data.Studio.Intro = _studioIntro.Text.Trim();
        PullCopyFromUi();
        CommitPerson();
        CommitProject();
        EndEdit(_experienceGrid);
        EndEdit(_educationGrid);
    }

    private void RefreshPeopleList(Person? select)
    {
        _peopleList.BeginUpdate();
        _peopleList.Items.Clear();
        foreach (var person in _data.People)
            _peopleList.Items.Add(person);
        _peopleList.EndUpdate();
        if (select is not null)
            _peopleList.SelectedItem = select;
        else if (_peopleList.Items.Count > 0)
            _peopleList.SelectedIndex = 0;
    }

    private void RefreshProjectList(Project? select)
    {
        _projectList.BeginUpdate();
        _projectList.Items.Clear();
        foreach (var project in _data.Projects)
            _projectList.Items.Add(project);
        _projectList.EndUpdate();
        if (select is not null)
            _projectList.SelectedItem = select;
        else if (_projectList.Items.Count > 0)
            _projectList.SelectedIndex = 0;
    }

    private void ShowSelectedPerson()
    {
        if (_suppress)
            return;

        CommitPerson();
        _currentPerson = _peopleList.SelectedItem as Person;
        _suppress = true;
        if (_currentPerson is null)
        {
            ClearPersonFields();
            _suppress = false;
            return;
        }

        _personId.Text = _currentPerson.Id;
        _personName.Text = _currentPerson.Name;
        _personShort.Text = _currentPerson.ShortName;
        _personPhoto.Text = _currentPerson.Photo;
        _personRole.Text = _currentPerson.Role;
        _personLocation.Text = _currentPerson.Location;
        _personEmail.Text = _currentPerson.Email;
        _personBio.Text = _currentPerson.Bio;
        _personSummary.Text = _currentPerson.Summary;
        _personSkills.Text = string.Join(Environment.NewLine, _currentPerson.Skills);
        _currentPerson.NormalizeSocials();
        _socialsGrid.DataSource = new BindingList<SocialLink>(_currentPerson.Socials);
        _experienceGrid.DataSource = new BindingList<Experience>(_currentPerson.Experience);
        _educationGrid.DataSource = new BindingList<Education>(_currentPerson.Education);
        _suppress = false;
        RefreshAuthorshipCombo();
    }

    private void CommitPerson()
    {
        if (_currentPerson is null)
            return;

        EndEdit(_experienceGrid);
        EndEdit(_educationGrid);
        EndEdit(_socialsGrid);
        _currentPerson.Id = _personId.Text.Trim();
        _currentPerson.Name = _personName.Text.Trim();
        _currentPerson.ShortName = _personShort.Text.Trim();
        _currentPerson.Photo = _personPhoto.Text.Trim().Replace('\\', '/');
        _currentPerson.Role = _personRole.Text.Trim();
        _currentPerson.Location = _personLocation.Text.Trim();
        _currentPerson.Email = _personEmail.Text.Trim();
        _currentPerson.Bio = _personBio.Text.Trim();
        _currentPerson.Summary = _personSummary.Text.Trim();
        _currentPerson.Skills = SplitLines(_personSkills.Text);
        if (_socialsGrid.DataSource is BindingList<SocialLink> socials)
            _currentPerson.Socials = [.. socials];
        _currentPerson.NormalizeSocials();
        if (_experienceGrid.DataSource is BindingList<Experience> experience)
            _currentPerson.Experience = [.. experience];
        if (_educationGrid.DataSource is BindingList<Education> education)
            _currentPerson.Education = [.. education];
    }

    private void ClearPersonFields()
    {
        _personId.Clear();
        _personName.Clear();
        _personShort.Clear();
        _personPhoto.Clear();
        _personRole.Clear();
        _personLocation.Clear();
        _personEmail.Clear();
        _personBio.Clear();
        _personSummary.Clear();
        _personSkills.Clear();
        _socialsGrid.DataSource = null;
        _experienceGrid.DataSource = null;
        _educationGrid.DataSource = null;
    }

    private void ImportLinkedIn()
    {
        PullUiIntoModel();

        string? suggested = null;
        var docs = LinkedInImportForm.FindDocsFolder();
        if (docs is not null)
        {
            suggested = Directory.GetFiles(docs, "*CV*.pdf")
                .Concat(Directory.GetFiles(docs, "*.pdf"))
                .FirstOrDefault();
        }

        using var form = new LinkedInImportForm(_data.People, _peopleList.SelectedItem as Person, suggested);
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        var imported = form.ImportedPerson;
        Person target;
        if (form.ReplaceTarget is Person existing)
        {
            var photo = existing.Photo;
            existing.Name = imported.Name;
            existing.ShortName = imported.ShortName;
            existing.Role = imported.Role;
            existing.Location = imported.Location;
            existing.Email = imported.Email;
            existing.Linkedin = imported.Linkedin;
            existing.Github = imported.Github;
            existing.Socials = imported.Socials;
            existing.Summary = imported.Summary;
            existing.Bio = string.IsNullOrWhiteSpace(imported.Bio)
                ? ClipText(imported.Summary, 320)
                : imported.Bio;
            existing.Skills = imported.Skills;
            existing.Experience = imported.Experience;
            existing.Education = imported.Education;
            existing.Photo = photo;
            existing.NormalizeSocials();
            target = existing;
        }
        else
        {
            imported.Id = NextPersonId();
            imported.NormalizeSocials();
            if (string.IsNullOrWhiteSpace(imported.Bio))
                imported.Bio = ClipText(imported.Summary, 320);
            _data.People.Add(imported);
            target = imported;
        }

        _currentPerson = null;
        RefreshPeopleList(target);
        ShowSelectedPerson();
        _tabs.SelectedTab = _tabs.TabPages.Cast<TabPage>().First(t => t.Text == "Perfis");
    }

    private void AddPerson()
    {
        PullUiIntoModel();
        var person = new Person
        {
            Id = NextPersonId(),
            Name = "Novo perfil",
            ShortName = "Novo",
        };
        _data.People.Add(person);
        RefreshPeopleList(person);
        RefreshAuthorshipCombo();
    }

    private void RemovePerson()
    {
        if (_peopleList.SelectedItem is not Person person)
            return;
        if (MessageBox.Show(this, $"Remover {person}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _currentPerson = null;
        _data.People.Remove(person);
        RefreshPeopleList(_data.People.FirstOrDefault());
        RefreshAuthorshipCombo();
    }

    private void ShowSelectedProject()
    {
        if (_suppress)
            return;

        CommitProject();
        _currentProject = _projectList.SelectedItem as Project;
        _suppress = true;
        if (_currentProject is null)
        {
            _projectId.Clear();
            _projectYear.Clear();
            _projectTitle.Clear();
            _projectSector.Clear();
            _projectSummary.Clear();
            _projectOutcome.Clear();
            _projectStack.Clear();
            _suppress = false;
            return;
        }

        _projectId.Text = _currentProject.Id;
        _projectYear.Text = _currentProject.Year;
        _projectTitle.Text = _currentProject.Title;
        _projectSector.Text = _currentProject.Sector;
        _projectSummary.Text = _currentProject.Summary;
        _projectOutcome.Text = _currentProject.Outcome;
        _projectStack.Text = string.Join(Environment.NewLine, _currentProject.Stack);
        RefreshAuthorshipCombo();
        _suppress = false;
    }

    private void CommitProject()
    {
        if (_currentProject is null)
            return;

        _currentProject.Id = _projectId.Text.Trim();
        _currentProject.Year = _projectYear.Text.Trim();
        _currentProject.Title = _projectTitle.Text.Trim();
        _currentProject.Sector = _projectSector.Text.Trim();
        _currentProject.Summary = _projectSummary.Text.Trim();
        _currentProject.Outcome = _projectOutcome.Text.Trim();
        _currentProject.Stack = SplitLines(_projectStack.Text);
        ApplyAuthorshipFromCombo();
    }

    private void AddProject()
    {
        PullUiIntoModel();
        var project = new Project
        {
            Id = NextProjectId(),
            Year = DateTime.Now.Year.ToString(),
            Title = "Novo projeto",
            Authorship = "joint",
            Authors = _data.People.Select(p => p.Id).Where(id => id.Length > 0).ToList(),
        };
        _data.Projects.Add(project);
        RefreshProjectList(project);
    }

    private void RemoveProject()
    {
        if (_projectList.SelectedItem is not Project project)
            return;
        if (MessageBox.Show(this, $"Remover {project}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _currentProject = null;
        _data.Projects.Remove(project);
        RefreshProjectList(_data.Projects.FirstOrDefault());
    }

    private void RefreshAuthorshipCombo()
    {
        var previous = _projectAuthorship.SelectedItem as AuthorshipOption;
        _projectAuthorship.Items.Clear();
        _projectAuthorship.Items.Add(new AuthorshipOption("joint", "Conjunto"));
        foreach (var person in _data.People)
            _projectAuthorship.Items.Add(new AuthorshipOption(person.Id, $"Individual · {person}"));

        if (_currentProject is null)
            return;

        var match = _projectAuthorship.Items.Cast<AuthorshipOption>()
            .FirstOrDefault(o => o.Id == _currentProject.Authorship);
        _projectAuthorship.SelectedItem = match ?? _projectAuthorship.Items[0];
    }

    private void ApplyAuthorshipFromCombo()
    {
        if (_suppress || _currentProject is null)
            return;
        if (_projectAuthorship.SelectedItem is not AuthorshipOption option)
            return;

        _currentProject.Authorship = option.Id;
        _currentProject.Authors = option.Id == "joint"
            ? _data.People.Select(p => p.Id).Where(id => id.Length > 0).ToList()
            : [option.Id];
    }

    private string NextPersonId()
    {
        foreach (var id in Enumerable.Range(0, 26).Select(i => ((char)('a' + i)).ToString()))
        {
            if (_data.People.All(p => p.Id != id))
                return id;
        }

        return $"p{_data.People.Count + 1}";
    }

    private string NextProjectId()
    {
        var n = 1;
        while (_data.Projects.Any(p => p.Id == $"p{n}"))
            n++;
        return $"p{n}";
    }

    private static List<string> SplitLines(string text) =>
        text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private void PushCopyIntoUi()
    {
        var copy = _data.Copy;
        _copyEyebrow.Text = copy.Eyebrow;
        _copyNav.Text = copy.NavLine;
        _copyVenn.Text = copy.VennLabel;
        _copyMission.Text = copy.Mission;
        _copyTrajectoryTitle.Text = copy.TrajectoryTitle;
        _copyTrajectoryLead.Text = copy.TrajectoryLead;
        _statsGrid.DataSource = new BindingList<StatLine>(copy.Stats);
        _milestonesGrid.DataSource = new BindingList<Milestone>(copy.Milestones);
        _copyProfilesTitle.Text = copy.ProfilesTitle;
        _copyProfilesLead.Text = copy.ProfilesLead;
        _copyProjectsTitle.Text = copy.ProjectsTitle;
        _copyProjectsLead.Text = copy.ProjectsLead;
        _copyStackTitle.Text = copy.StackTitle;
        _copyStack.Text = string.Join(Environment.NewLine, copy.Stack);
        _copyCtaEyebrow.Text = copy.CtaEyebrow;
        _copyCtaTitle.Text = copy.CtaTitle;
        _copyCtaLead.Text = copy.CtaLead;
        _copyFooterBlurb.Text = copy.FooterBlurb;
    }

    private void PullCopyFromUi()
    {
        _data.Copy.Eyebrow = _copyEyebrow.Text.Trim();
        _data.Copy.NavLine = _copyNav.Text.Trim();
        _data.Copy.VennLabel = _copyVenn.Text.Trim();
        _data.Copy.Mission = _copyMission.Text.Trim();
        _data.Copy.TrajectoryTitle = _copyTrajectoryTitle.Text.Trim();
        _data.Copy.TrajectoryLead = _copyTrajectoryLead.Text.Trim();
        EndEdit(_statsGrid);
        EndEdit(_milestonesGrid);
        if (_statsGrid.DataSource is BindingList<StatLine> stats)
            _data.Copy.Stats = [.. stats];
        if (_milestonesGrid.DataSource is BindingList<Milestone> milestones)
            _data.Copy.Milestones = [.. milestones];
        _data.Copy.ProfilesTitle = _copyProfilesTitle.Text.Trim();
        _data.Copy.ProfilesLead = _copyProfilesLead.Text.Trim();
        _data.Copy.ProjectsTitle = _copyProjectsTitle.Text.Trim();
        _data.Copy.ProjectsLead = _copyProjectsLead.Text.Trim();
        _data.Copy.StackTitle = _copyStackTitle.Text.Trim();
        _data.Copy.Stack = SplitLines(_copyStack.Text);
        _data.Copy.CtaEyebrow = _copyCtaEyebrow.Text.Trim();
        _data.Copy.CtaTitle = _copyCtaTitle.Text.Trim();
        _data.Copy.CtaLead = _copyCtaLead.Text.Trim();
        _data.Copy.FooterBlurb = _copyFooterBlurb.Text.Trim();
    }

    private Control BuildPhotoRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.Controls.Add(_personPhoto, 0, 0);
        var browse = NewButton("Selecionar foto");
        browse.Margin = new Padding(8, 0, 0, 0);
        browse.Click += (_, _) => ChoosePhoto();
        row.Controls.Add(browse, 1, 0);
        return row;
    }

    private void ChoosePhoto()
    {
        if (_currentPerson is null)
        {
            MessageBox.Show(this, "Selecione um perfil antes de escolher a foto.", "Foto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Imagens|*.png;*.jpg;*.jpeg;*.webp;*.gif|Todos|*.*",
            Title = "Foto do perfil no layout",
        };

        var current = ResolveSitePath(_personPhoto.Text);
        if (current is not null)
            dialog.InitialDirectory = Path.GetDirectoryName(current);

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var root = SiteRoot();
        if (root is null)
        {
            MessageBox.Show(this, "Salve ou localize o content.js antes de copiar a foto.", "Foto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var imgDir = Path.Combine(root, "img");
        Directory.CreateDirectory(imgDir);
        var ext = Path.GetExtension(dialog.FileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".png";
        var id = string.IsNullOrWhiteSpace(_personId.Text) ? _currentPerson.Id : _personId.Text.Trim();
        var destName = $"person-{id}{ext.ToLowerInvariant()}";
        var dest = Path.Combine(imgDir, destName);
        File.Copy(dialog.FileName, dest, overwrite: true);
        _personPhoto.Text = $"img/{destName}";
        _currentPerson.Photo = _personPhoto.Text;
    }

    private string? SiteRoot()
    {
        if (string.IsNullOrWhiteSpace(_filePath))
            return null;
        var jsDir = Path.GetDirectoryName(_filePath);
        return string.IsNullOrWhiteSpace(jsDir) ? null : Directory.GetParent(jsDir)?.FullName;
    }

    private string? ResolveSitePath(string relative)
    {
        var root = SiteRoot();
        if (root is null || string.IsNullOrWhiteSpace(relative))
            return null;
        var full = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(full) ? full : null;
    }

    private static string ClipText(string text, int max)
    {
        var value = (text ?? "").Trim();
        if (value.Length <= max)
            return value;
        var cut = value[..max];
        var at = cut.LastIndexOf(' ');
        return $"{(at > 80 ? cut[..at] : cut)}…";
    }

    private static void EndEdit(DataGridView grid)
    {
        if (grid.DataSource is null)
            return;

        grid.EndEdit();
        if (grid.BindingContext?[grid.DataSource] is CurrencyManager manager)
            manager.EndCurrentEdit();
    }

    private static TextBox NewBox() => new() { Dock = DockStyle.Fill };

    private static TextBox NewMultiline(int lines) => new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        AcceptsReturn = true,
        Height = lines * 22,
    };

    private static DataGridView NewGrid() => new()
    {
        Dock = DockStyle.Fill,
        AllowUserToAddRows = true,
        AllowUserToDeleteRows = true,
        AutoGenerateColumns = false,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.CellSelect,
    };

    private static Button NewButton(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Padding = new Padding(12, 4, 12, 4),
        Margin = new Padding(8, 0, 0, 0),
    };

    private Control BuildPeopleButtons()
    {
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        var add = NewButton("Adicionar");
        add.Click += (_, _) => AddPerson();
        var import = NewButton("Importar LinkedIn");
        import.Click += (_, _) => ImportLinkedIn();
        var remove = NewButton("Remover");
        remove.Click += (_, _) => RemovePerson();
        buttons.Controls.Add(add);
        buttons.Controls.Add(import);
        buttons.Controls.Add(remove);
        return buttons;
    }

    private Control BuildProjectButtons()
    {
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var add = NewButton("Adicionar");
        add.Click += (_, _) => AddProject();
        var remove = NewButton("Remover");
        remove.Click += (_, _) => RemoveProject();
        buttons.Controls.Add(add);
        buttons.Controls.Add(remove);
        return buttons;
    }

    private static TableLayoutPanel NewColumns(int leftWidth)
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0),
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, leftWidth));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return grid;
    }

    private static TableLayoutPanel BuildListPanel(Control list, Control buttons)
    {
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.Controls.Add(list, 0, 0);
        left.Controls.Add(buttons, 0, 1);
        return left;
    }

    private static TableLayoutPanel NewFormGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
            Padding = new Padding(12),
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return grid;
    }

    private static void AddRow(TableLayoutPanel grid, string label, Control control, int? minHeight = null)
    {
        var row = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var caption = new Label
        {
            Text = label,
            AutoSize = true,
            UseMnemonic = false,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 8, 12, 8),
        };
        control.Margin = new Padding(0, 4, 0, 4);
        if (minHeight is int height)
            control.MinimumSize = new Size(0, height);
        grid.Controls.Add(caption, 0, row);
        grid.Controls.Add(control, 1, row);
    }

    private sealed record AuthorshipOption(string Id, string Label)
    {
        public override string ToString() => Label;
    }
}
