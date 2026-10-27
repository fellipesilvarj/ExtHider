using System.ComponentModel;

namespace ExtHider;

public class MainForm : Form
{
    private readonly ListBox _folderListBox = new() { SelectionMode = SelectionMode.MultiExtended };
    private readonly CheckedListBox _extCheckedListBox = new() { CheckOnClick = true };
    private readonly CheckBox _recursiveCheckBox = new() { Text = "Include subfolders", Checked = true };
    private readonly TextBox _logTextBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 9),
    };
    private readonly ProgressBar _progressBar = new() { Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 0 };
    private readonly List<string> _folders = new();

    public MainForm()
    {
        Text = "Extension Hider";
        Width = 640;
        Height = 520;
        MinimumSize = new Size(560, 420);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(10),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); // folders
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // options / extensions
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // actions
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // log
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // progress

        // --- Folders group ---
        var folderGroup = new GroupBox { Text = "Target folders", Dock = DockStyle.Fill };
        var folderLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(6) };
        folderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        folderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        _folderListBox.Dock = DockStyle.Fill;
        folderLayout.Controls.Add(_folderListBox, 0, 0);

        var folderBtnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var addFolderBtn = new Button { Text = "Add folder...", Width = 130 };
        addFolderBtn.Click += AddFolderBtn_Click;
        var removeFolderBtn = new Button { Text = "Remove selected", Width = 130 };
        removeFolderBtn.Click += RemoveFolderBtn_Click;
        folderBtnPanel.Controls.Add(addFolderBtn);
        folderBtnPanel.Controls.Add(removeFolderBtn);
        folderLayout.Controls.Add(folderBtnPanel, 1, 0);
        folderGroup.Controls.Add(folderLayout);

        // --- Options group ---
        var optsGroup = new GroupBox { Text = "File types found in selected folders", Dock = DockStyle.Fill };
        var optsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(6) };
        optsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        optsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        optsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        optsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        _extCheckedListBox.Dock = DockStyle.Fill;
        optsLayout.Controls.Add(_extCheckedListBox, 0, 0);
        optsLayout.SetRowSpan(_extCheckedListBox, 2);

        var extBtnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var scanBtn = new Button { Text = "Scan for file types", Width = 130 };
        scanBtn.Click += (_, _) => ScanExtensions();
        var selectAllBtn = new Button { Text = "Select all", Width = 130 };
        selectAllBtn.Click += (_, _) => SetAllChecked(true);
        var selectNoneBtn = new Button { Text = "Select none", Width = 130 };
        selectNoneBtn.Click += (_, _) => SetAllChecked(false);
        extBtnPanel.Controls.Add(scanBtn);
        extBtnPanel.Controls.Add(selectAllBtn);
        extBtnPanel.Controls.Add(selectNoneBtn);
        optsLayout.Controls.Add(extBtnPanel, 1, 0);

        _recursiveCheckBox.Dock = DockStyle.Fill;
        _recursiveCheckBox.CheckedChanged += (_, _) => ScanExtensions();
        optsLayout.Controls.Add(_recursiveCheckBox, 1, 1);

        optsGroup.Controls.Add(optsLayout);

        // --- Actions ---
        var actionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var hideBtn = new Button { Text = "Hide matching files", Width = 150, Height = 30 };
        hideBtn.Click += (_, _) => RunAction(hidden: true);
        var unhideBtn = new Button { Text = "Unhide matching files", Width = 150, Height = 30 };
        unhideBtn.Click += (_, _) => RunAction(hidden: false);
        actionPanel.Controls.Add(hideBtn);
        actionPanel.Controls.Add(unhideBtn);

        // --- Log group ---
        var logGroup = new GroupBox { Text = "Log", Dock = DockStyle.Fill };
        logGroup.Controls.Add(_logTextBox);

        _progressBar.Dock = DockStyle.Fill;

        root.Controls.Add(folderGroup, 0, 0);
        root.Controls.Add(optsGroup, 0, 1);
        root.Controls.Add(actionPanel, 0, 2);
        root.Controls.Add(logGroup, 0, 3);
        root.Controls.Add(_progressBar, 0, 4);

        Controls.Add(root);
    }

    private void AddFolderBtn_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK && !_folders.Contains(dialog.SelectedPath))
        {
            _folders.Add(dialog.SelectedPath);
            _folderListBox.Items.Add(dialog.SelectedPath);
            ScanExtensions();
        }
    }

    private void RemoveFolderBtn_Click(object? sender, EventArgs e)
    {
        var selectedIndices = _folderListBox.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        foreach (var index in selectedIndices)
        {
            _folders.RemoveAt(index);
            _folderListBox.Items.RemoveAt(index);
        }
        ScanExtensions();
    }

    private void SetAllChecked(bool value)
    {
        for (var i = 0; i < _extCheckedListBox.Items.Count; i++)
        {
            _extCheckedListBox.SetItemChecked(i, value);
        }
    }

    private void ScanExtensions()
    {
        var previouslyChecked = _extCheckedListBox.CheckedItems.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        _extCheckedListBox.Items.Clear();

        if (_folders.Count == 0) return;

        var searchOption = _recursiveCheckBox.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var found = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var folder in _folders)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder, "*", searchOption))
                {
                    var ext = Path.GetExtension(file);
                    found.Add(ext.Length == 0 ? "(no extension)" : ext.ToLowerInvariant());
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR scanning {folder}: {ex.Message}");
            }
        }

        foreach (var ext in found)
        {
            _extCheckedListBox.Items.Add(ext, previouslyChecked.Contains(ext));
        }
    }

    private HashSet<string> ParseExtensions()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in _extCheckedListBox.CheckedItems)
        {
            var ext = (string)item;
            result.Add(ext == "(no extension)" ? "" : ext);
        }
        return result;
    }

    private void Log(string message)
    {
        if (_logTextBox.InvokeRequired)
        {
            _logTextBox.Invoke(() => Log(message));
            return;
        }
        _logTextBox.AppendText(message + Environment.NewLine);
    }

    private void RunAction(bool hidden)
    {
        if (_folders.Count == 0)
        {
            MessageBox.Show(this, "Add at least one target folder.", "No folders", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var extensions = ParseExtensions();
        if (extensions.Count == 0)
        {
            MessageBox.Show(this, "Check at least one file type to hide/unhide.", "No extensions selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var recursive = _recursiveCheckBox.Checked;
        var folders = _folders.ToList();

        _progressBar.MarqueeAnimationSpeed = 30;
        Log($"--- {(hidden ? "Hiding" : "Unhiding")} files with extensions [{string.Join(", ", extensions)}] ---");

        var worker = new BackgroundWorker();
        worker.DoWork += (_, args) => ProcessFolders(folders, extensions, hidden, recursive);
        worker.RunWorkerCompleted += (_, args) =>
        {
            _progressBar.MarqueeAnimationSpeed = 0;
            if (args.Error is not null)
            {
                Log($"FATAL ERROR: {args.Error.Message}");
            }
        };
        worker.RunWorkerAsync();
    }

    private void ProcessFolders(List<string> folders, HashSet<string> extensions, bool hidden, bool recursive)
    {
        var processed = 0;
        var errors = 0;
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var folder in folders)
        {
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(folder, "*", searchOption);
            }
            catch (Exception ex)
            {
                errors++;
                Log($"ERROR reading {folder}: {ex.Message}");
                continue;
            }

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (!extensions.Contains(ext)) continue;

                try
                {
                    var attrs = File.GetAttributes(file);
                    var newAttrs = hidden ? attrs | FileAttributes.Hidden : attrs & ~FileAttributes.Hidden;
                    if (newAttrs != attrs)
                    {
                        File.SetAttributes(file, newAttrs);
                    }
                    processed++;
                    Log($"{(hidden ? "Hid" : "Unhid")}: {file}");
                }
                catch (Exception ex)
                {
                    errors++;
                    Log($"ERROR: {file} -> {ex.Message}");
                }
            }
        }

        Log($"--- Done. {processed} file(s) processed, {errors} error(s). ---");
    }
}
