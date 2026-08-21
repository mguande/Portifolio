using System.ComponentModel;

namespace PortfolioEditor;

internal sealed class LinkedInImportForm : Form
{
    private readonly IReadOnlyList<Person> _existingPeople;
    private readonly TextBox _urlBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _pathBox = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TextBox _name = NewBox();
    private readonly TextBox _shortName = NewBox();
    private readonly TextBox _role = NewBox();
    private readonly TextBox _location = NewBox();
    private readonly TextBox _email = NewBox();
    private readonly TextBox _linkedin = NewBox();
    private readonly TextBox _summary = NewMultiline();
    private readonly TextBox _bio = NewMultiline();
    private readonly TextBox _skills = NewMultiline();
    private readonly DataGridView _experience = NewGrid();
    private readonly DataGridView _education = NewGrid();
    private readonly Label _status = new() { AutoSize = true, ForeColor = Color.FromArgb(90, 88, 82) };

    public Person ImportedPerson { get; private set; } = new();
    public Person? ReplaceTarget { get; private set; }

    public LinkedInImportForm(IReadOnlyList<Person> existingPeople, Person? selected, string? suggestedPdf)
    {
        _existingPeople = existingPeople;
        Text = "Importar currículo do LinkedIn";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(960, 720);
        Size = new Size(1080, 800);
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
        root.Controls.Add(BuildBody(), 0, 1);
        root.Controls.Add(BuildFooter(), 0, 2);
        Controls.Add(root);

        FillTargets(selected);

        if (!string.IsNullOrWhiteSpace(suggestedPdf) && File.Exists(suggestedPdf))
        {
            _pathBox.Text = suggestedPdf;
            TryParse(suggestedPdf);
        }
    }

    private Control BuildHeader()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 4,
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var intro = new Label
        {
            Text = "Importe o PDF gerado pelo LinkedIn (Mais · Salvar em PDF). Funciona em português e em inglês.",
            AutoSize = true,
            UseMnemonic = false,
            Margin = new Padding(0, 0, 0, 12),
        };
        grid.Controls.Add(intro, 0, 0);
        grid.SetColumnSpan(intro, 3);

        grid.Controls.Add(new Label { Text = "Arquivo PDF", AutoSize = true, UseMnemonic = false, Margin = new Padding(0, 8, 8, 8) }, 0, 1);
        grid.Controls.Add(_pathBox, 1, 1);
        var browse = NewButton("Selecionar PDF");
        browse.Click += (_, _) => Browse();
        grid.Controls.Add(browse, 2, 1);

        grid.Controls.Add(new Label { Text = "Aplicar em", AutoSize = true, UseMnemonic = false, Margin = new Padding(0, 8, 8, 8) }, 0, 2);
        grid.Controls.Add(_target, 1, 2);
        grid.SetColumnSpan(_target, 2);
        return grid;
    }

    private Control BuildBody()
    {
        _tabs.TabPages.Add(BuildProfileTab());
        _tabs.TabPages.Add(BuildExperienceTab());
        _tabs.TabPages.Add(BuildEducationTab());
        return _tabs;
    }

    private TabPage BuildProfileTab()
    {
        var page = new TabPage("Dados do perfil");
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
            Padding = new Padding(8),
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(grid, "Nome completo", _name);
        AddRow(grid, "Nome curto", _shortName);
        AddRow(grid, "Função", _role);
        AddRow(grid, "Local", _location);
        AddRow(grid, "E-mail", _email);
        AddRow(grid, "LinkedIn", _linkedin);
        AddRow(grid, "Resumo", _summary, 140);
        AddRow(grid, "Texto do card", _bio, 90);
        AddRow(grid, "Competências", _skills, 100);
        page.Controls.Add(grid);
        return page;
    }

    private TabPage BuildExperienceTab()
    {
        var page = new TabPage("Experiência");
        ConfigureExperience(_experience);
        page.Controls.Add(_experience);
        return page;
    }

    private TabPage BuildEducationTab()
    {
        var page = new TabPage("Formação");
        ConfigureEducation(_education);
        page.Controls.Add(_education);
        return page;
    }

    private Control BuildFooter()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 0),
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.Controls.Add(_status, 0, 0);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        var apply = NewButton("Importar para o perfil");
        apply.BackColor = Color.FromArgb(47, 68, 84);
        apply.ForeColor = Color.White;
        apply.FlatStyle = FlatStyle.Flat;
        apply.Click += (_, _) => Apply();

        var cancel = NewButton("Cancelar");
        cancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        buttons.Controls.Add(apply);
        buttons.Controls.Add(cancel);
        panel.Controls.Add(buttons, 1, 0);
        return panel;
    }

    private void FillTargets(Person? selected)
    {
        _target.Items.Add("Criar novo perfil");
        foreach (var person in _existingPeople)
            _target.Items.Add(person);

        if (selected is not null)
            _target.SelectedItem = selected;
        else
            _target.SelectedIndex = 0;
    }

    private async Task FetchFromUrlAsync()
    {
        var url = _urlBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(url) && !LinkedInWebParser.IsLinkedInProfile(url))
        {
            MessageBox.Show(this, "Informe o link da home do perfil, no formato https://www.linkedin.com/in/nome-da-pessoa, ou deixe em branco se a aba já estiver aberta.", "Link inválido", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            url = LinkedInWebParser.NormalizeProfileUrl(url);
            _urlBox.Text = url;
            _linkedin.Text = url;
        }

        _status.Text = "Lendo a aba do Chrome…";
        UseWaitCursor = false;

        try
        {
            var dump = await ChromeLinkedInClient.ReadProfileAsync(url, this);
            if (LinkedInWebParser.LooksLikeLogin(dump.Href, dump.Title, dump.Text))
            {
                _status.Text = "O Chrome abriu a tela de login. Entre no LinkedIn e clique em Buscar no Chrome de novo.";
                return;
            }

            var person = LinkedInWebParser.Parse(dump.Href, dump.Name, dump.Text);
            if (string.IsNullOrWhiteSpace(person.Name) && person.Experience.Count == 0)
            {
                _status.Text = "O LinkedIn não liberou o currículo nesta página. Role o perfil no Chrome e busque de novo, ou use o PDF.";
                return;
            }

            ShowPerson(person);
            _tabs.SelectedIndex = 0;
            _status.Text = $"{person.Name}: {person.Experience.Count} experiências, {person.Education.Count} formações.";
        }
        catch (OperationCanceledException)
        {
            _status.Text = "Busca cancelada.";
        }
        catch (Exception ex)
        {
            _status.Text = "Não foi possível ler o perfil pelo Chrome.";
            MessageBox.Show(this, ex.Message, "Chrome", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void Browse()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "PDF do LinkedIn|*.pdf|Todos|*.*",
            Title = "Selecionar currículo exportado do LinkedIn",
        };

        if (!string.IsNullOrWhiteSpace(_pathBox.Text) && File.Exists(_pathBox.Text))
            dialog.InitialDirectory = Path.GetDirectoryName(_pathBox.Text);
        else
        {
            var docs = FindDocsFolder();
            if (docs is not null)
                dialog.InitialDirectory = docs;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _pathBox.Text = dialog.FileName;
        TryParse(dialog.FileName);
    }

    private void TryParse(string path)
    {
        try
        {
            var person = LinkedInCvParser.ParsePdf(path);
            ShowPerson(person);
            _status.Text = $"{person.Experience.Count} experiências e {person.Education.Count} formações encontradas.";
        }
        catch (Exception ex)
        {
            _status.Text = "Não foi possível ler o PDF.";
            MessageBox.Show(this, ex.Message, "Falha na importação", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowPerson(Person person)
    {
        _name.Text = person.Name;
        _shortName.Text = person.ShortName;
        _role.Text = person.Role;
        _location.Text = person.Location;
        _email.Text = person.Email;
        _linkedin.Text = person.Linkedin;
        _summary.Text = person.Summary;
        _bio.Text = string.IsNullOrWhiteSpace(person.Bio) ? Clip(person.Summary, 320) : person.Bio;
        _skills.Text = string.Join(Environment.NewLine, person.Skills);
        _experience.DataSource = new BindingList<Experience>(person.Experience.Select(Copy).ToList());
        _education.DataSource = new BindingList<Education>(person.Education.Select(Copy).ToList());
    }

    private void Apply()
    {
        EndEdit(_experience);
        EndEdit(_education);

        ImportedPerson = new Person
        {
            Name = _name.Text.Trim(),
            ShortName = _shortName.Text.Trim(),
            Role = _role.Text.Trim(),
            Location = _location.Text.Trim(),
            Email = _email.Text.Trim(),
            Linkedin = _linkedin.Text.Trim(),
            Summary = _summary.Text.Trim(),
            Bio = string.IsNullOrWhiteSpace(_bio.Text) ? Clip(_summary.Text, 320) : _bio.Text.Trim(),
            Skills = _skills.Text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Experience = _experience.DataSource is BindingList<Experience> jobs ? [.. jobs] : [],
            Education = _education.DataSource is BindingList<Education> edu ? [.. edu] : [],
        };

        ReplaceTarget = _target.SelectedItem as Person;
        DialogResult = DialogResult.OK;
        Close();
    }

    internal static string? FindDocsFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var docs = Path.Combine(dir.FullName, "_docs");
            if (Directory.Exists(docs))
                return docs;
            dir = dir.Parent;
        }

        return null;
    }

    private static string Clip(string text, int max)
    {
        var value = (text ?? "").Trim();
        if (value.Length <= max)
            return value;
        var cut = value[..max];
        var at = cut.LastIndexOf(' ');
        return $"{(at > 80 ? cut[..at] : cut)}…";
    }

    private static Experience Copy(Experience item) => new()
    {
        Period = item.Period,
        Title = item.Title,
        Org = item.Org,
        Detail = item.Detail,
    };

    private static Education Copy(Education item) => new()
    {
        Period = item.Period,
        Title = item.Title,
        Org = item.Org,
    };

    private static void ConfigureExperience(DataGridView grid)
    {
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Period), HeaderText = "Período", FillWeight = 16 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Title), HeaderText = "Cargo", FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Org), HeaderText = "Empresa", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Experience.Detail), HeaderText = "Detalhe", FillWeight = 46 });
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
    }

    private static void ConfigureEducation(DataGridView grid)
    {
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Education.Period), HeaderText = "Período", FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Education.Title), HeaderText = "Curso", FillWeight = 50 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Education.Org), HeaderText = "Instituição", FillWeight = 30 });
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

    private static TextBox NewMultiline() => new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        AcceptsReturn = true,
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

    private static void AddRow(TableLayoutPanel grid, string label, Control control, int minHeight = 0)
    {
        var row = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 8, 12, 8),
        }, 0, row);
        if (minHeight > 0)
            control.MinimumSize = new Size(0, minHeight);
        control.Margin = new Padding(0, 4, 0, 4);
        grid.Controls.Add(control, 1, row);
    }
}
