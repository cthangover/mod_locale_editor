namespace Cthangover.Tools.LocaleEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cthangover.Core.Localization;
    using Cthangover.Core.UI.Theming;
    using Cthangover.Core.Utils;
    using Cthangover.LocaleAtlas;
    using Cthangover.Mods.Interface.Theming;
    using Cthangover.Mods.Interface.Tool;
    using Cthangover.Tools.Common;
    using Cthangover.Tools.Services;
    using Godot;

    /// <summary>
    /// The translations window. It reads the locale atlas (the same pass the atlas command runs) and lists every
    /// text a language lacks: a key declared in other languages of its mod, or a key a mod uses and declares
    /// nowhere. Choose a language and, optionally, a mod, click a row, type the text and save: the line goes into
    /// the locale file of the owning mod (the sibling file of the declaration, as the atlas suggests), the
    /// game's texts reload, and the list is scanned again.
    /// <para/>
    /// The window is a front end of the atlas: it never decides what is missing by itself. Mods that are
    /// read-only (zip packages) are listed, but their rows cannot be saved.
    /// </summary>
    public partial class LocaleEditorWindow : ToolWindow
    {
        private readonly OptionButton _langSelect;
        private readonly OptionButton _modSelect;
        private readonly Label _countLabel;
        private readonly FilteredList _list;
        private readonly Label _keyLabel;
        private readonly Label _modLabel;
        private readonly Label _fileLabel;
        private readonly Label _sourceLabel;
        private readonly Label _readOnlyLabel;
        private readonly LineEdit _text;
        private readonly Button _save;

        private List<LocaleGap> _gaps = new();
        private List<LocaleGap> _shown = new();
        private LocaleGap _current;
        private string _language;
        private string _modFilter;
        private Task<LocaleAtlasResult> _pass;

        /// <summary>Gaps of every language in the last finished scan.</summary>
        public IReadOnlyList<LocaleGap> Gaps => _gaps;

        /// <summary>Gaps shown for the selected language and mod.</summary>
        public IReadOnlyList<LocaleGap> Shown => _shown;

        /// <summary>The selected language, or null before the first scan.</summary>
        public string Language => _language;

        /// <summary>True while an atlas pass runs.</summary>
        public bool Scanning => _pass != null;

        /// <summary>The gap whose form is shown, or null.</summary>
        public LocaleGap Current => _current;

        public LocaleEditorWindow() : base("tools/locale_editor/title")
        {
            Size = new Vector2I(1120, 700);
            CthangoverTheme.Apply(this);

            var outer = CreateFillContainer();
            AddChild(outer);

            var toolbar = CreateToolbar();
            outer.AddChild(toolbar);
            _langSelect = new OptionButton { CustomMinimumSize = new Vector2(150, 0), TooltipText = ToolUi.Tr("tools/locale_editor/language_tip") };
            _langSelect.ItemSelected += OnLanguageSelected;
            toolbar.AddChild(_langSelect);
            _modSelect = new OptionButton { CustomMinimumSize = new Vector2(260, 0), TooltipText = ToolUi.Tr("tools/locale_editor/mod_tip") };
            _modSelect.ItemSelected += OnModSelected;
            toolbar.AddChild(_modSelect);
            _countLabel = new Label { VerticalAlignment = VerticalAlignment.Center, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            _countLabel.AddThemeColorOverride("font_color", UiTokens.Muted);
            toolbar.AddChild(_countLabel);
            toolbar.AddChild(ToolUi.Button("tools/locale_editor/rescan", StartScan, ButtonKind.Secondary, "tools/locale_editor/rescan_tip"));

            var split = new HSplitContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            outer.AddChild(split);

            _list = new FilteredList(200f, "tools/locale_editor/search") { CustomMinimumSize = new Vector2(380, 0) };
            _list.Chosen += OnRowChosen;
            split.AddChild(_list);

            var formBox = new VBoxContainer();
            var side = CreateSidebar(formBox, 420);
            side.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            split.AddChild(side);

            formBox.AddChild(ToolUi.Section("tools/locale_editor/gap_section"));
            _keyLabel = ToolUi.Field(formBox, "tools/locale_editor/key", ValueLabel());
            _modLabel = ToolUi.Field(formBox, "tools/locale_editor/mod", ValueLabel());
            _fileLabel = ToolUi.Field(formBox, "tools/locale_editor/file", ValueLabel());
            formBox.AddChild(ToolUi.Section("tools/locale_editor/source"));
            _sourceLabel = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
            formBox.AddChild(_sourceLabel);
            formBox.AddChild(ToolUi.Section("tools/locale_editor/translation"));
            _text = ToolUi.Text(ToolUi.Tr("tools/locale_editor/text_placeholder"));
            _text.TextSubmitted += _ => Save();
            formBox.AddChild(_text);
            _readOnlyLabel = ToolUi.Hint(ToolUi.Tr("tools/locale_editor/read_only"));
            _readOnlyLabel.AddThemeColorOverride("font_color", UiTokens.Warning);
            formBox.AddChild(_readOnlyLabel);
            _save = ToolUi.Button("tools/locale_editor/save", () => Save(), ButtonKind.Primary, "tools/locale_editor/save_tip");
            formBox.AddChild(_save);

            SetHint(ToolUi.Tr("tools/locale_editor/hint"));
            ShowGap(null);
        }

        /// <summary>Starts the first scan once the window is in the tree.</summary>
        public override void _Ready() => StartScan();

        /// <summary>Takes the finished atlas pass (the scan runs on a worker thread).</summary>
        public override void _Process(double delta)
        {
            if (_pass == null || !_pass.IsCompleted)
                return;

            var pass = _pass;
            _pass = null;
            if (pass.IsFaulted)
            {
                var failure = pass.Exception?.GetBaseException().Message ?? "unknown";
                GameLogger.Log("LOCALE_EDITOR", $"locale atlas pass failed: {failure}", LogLevel.Error);
                SetStatus(ToolUi.Tr("tools/locale_editor/scan_failed"), ToolStatusKind.Error);
                return;
            }

            Apply(pass.Result);
        }

        /// <summary>Ctrl+S saves the typed translation.</summary>
        protected override void OnSaveShortcut() => Save();

        /// <summary>Selects a language (a code such as <c>ko</c>) and shows its gaps.</summary>
        public void SelectLanguage(string language)
        {
            for (var i = 0; i < _langSelect.ItemCount; i++)
            {
                if (_langSelect.GetItemText(i) == language)
                {
                    _langSelect.Select(i);
                    OnLanguageSelected(i);
                    return;
                }
            }
        }

        /// <summary>Shows the form of the row with the gap id (<see cref="LocaleGap.Id"/>); false when no such row is shown.</summary>
        public bool SelectGap(string id)
        {
            var gap = _shown.FirstOrDefault(g => g.Id == id);
            if (gap == null)
                return false;
            _list.Select(id);
            ShowGap(gap);
            return true;
        }

        /// <summary>Puts text into the translation field of the open form.</summary>
        public void TypeText(string value) => _text.Text = value;

        /// <summary>Saves the open form as the Save button and Enter do; false when nothing could be written.</summary>
        public bool SaveNow() => Save();

        /// <summary>Starts an atlas pass over the installed mods; a running pass is superseded by the new one.</summary>
        public void StartScan()
        {
            var mods = LocaleAtlasSnapshot.Take();
            _pass = Task.Run(() => LocaleAtlasBuilder.Build(mods));
            SetStatus(ToolUi.Tr("tools/locale_editor/scanning"), ToolStatusKind.Info);
        }

        private void Apply(LocaleAtlasResult result)
        {
            _gaps = LocaleGapList.From(result);
            var keepMod = _modFilter;
            var keepLanguage = _language;

            _langSelect.Clear();
            var languages = result.Languages.ToList();
            foreach (var language in languages)
                _langSelect.AddItem(language);
            var pick = keepLanguage != null && languages.Contains(keepLanguage) ? keepLanguage : DefaultLanguage(languages);
            if (pick != null)
                _langSelect.Select(languages.IndexOf(pick));
            _language = pick;

            RebuildModChoices(keepMod);
            ShowGap(null);
            Refresh();
            SetStatus(string.Format(ToolUi.Tr("tools/locale_editor/scanned"), _gaps.Count), ToolStatusKind.Info);
        }

        /// <summary>The game's own language when it has gaps, else the first language that has any, else the first.</summary>
        private string DefaultLanguage(List<string> languages)
        {
            var game = LanguageCode.Normalize(TranslationServer.GetLocale());
            if (game != null && languages.Contains(game) && _gaps.Any(g => g.Language == game))
                return game;
            var withGaps = languages.FirstOrDefault(l => _gaps.Any(g => g.Language == l));
            return withGaps ?? languages.FirstOrDefault();
        }

        private void RebuildModChoices(string keep)
        {
            _modSelect.Clear();
            _modSelect.AddItem(ToolUi.Tr("tools/locale_editor/all_mods"));
            _modSelect.SetItemMetadata(0, string.Empty);
            var mods = _gaps.Select(g => g.ModId).Distinct(StringComparer.Ordinal).OrderBy(m => m, StringComparer.Ordinal).ToList();
            var selected = 0;
            for (var i = 0; i < mods.Count; i++)
            {
                _modSelect.AddItem(mods[i]);
                _modSelect.SetItemMetadata(i + 1, mods[i]);
                if (mods[i] == keep)
                    selected = i + 1;
            }
            _modSelect.Select(selected);
            _modFilter = selected == 0 ? null : mods[selected - 1];
        }

        private void OnLanguageSelected(long index)
        {
            _language = _langSelect.GetItemText((int)index);
            ShowGap(null);
            Refresh();
        }

        private void OnModSelected(long index)
        {
            var mod = _modSelect.GetItemMetadata((int)index).AsString();
            _modFilter = string.IsNullOrEmpty(mod) ? null : mod;
            ShowGap(null);
            Refresh();
        }

        private void OnRowChosen(string id) => SelectGap(id);

        private void Refresh()
        {
            _shown = _gaps
                .Where(g => g.Language == _language && (_modFilter == null || g.ModId == _modFilter))
                .ToList();
            _list.SetItems(_shown.Select(g => (g.Id, g.Key + "   ·   " + g.ModId, (Texture2D)null)).ToList());
            _countLabel.Text = string.Format(ToolUi.Tr("tools/locale_editor/count"), _shown.Count, _language ?? "—");
        }

        private void ShowGap(LocaleGap gap)
        {
            _current = gap;
            if (gap == null)
            {
                _keyLabel.Text = "—";
                _modLabel.Text = "—";
                _fileLabel.Text = "—";
                _sourceLabel.Text = ToolUi.Tr("tools/locale_editor/pick_row");
                _text.Text = string.Empty;
                _text.Editable = false;
                _save.Disabled = true;
                _readOnlyLabel.Visible = false;
                return;
            }

            _keyLabel.Text = gap.Key;
            _modLabel.Text = gap.ModId;
            _fileLabel.Text = gap.TargetFile;
            _sourceLabel.Text = gap.SourceText == null
                ? ToolUi.Tr("tools/locale_editor/no_source")
                : $"[{gap.SourceLanguage}] {gap.SourceText}";
            _text.Text = string.Empty;
            var writable = ModFileService.IsWritable(gap.ModId);
            _text.Editable = writable;
            _save.Disabled = !writable;
            _readOnlyLabel.Visible = !writable;
        }

        /// <summary>Writes the open form's translation and scans again; false when nothing was written.</summary>
        private bool Save()
        {
            var gap = _current;
            if (gap == null)
            {
                SetStatus(ToolUi.Tr("tools/locale_editor/pick_row"), ToolStatusKind.Error);
                return false;
            }

            var value = _text.Text.Trim();
            if (value.Length == 0)
            {
                SetStatus(ToolUi.Tr("tools/locale_editor/empty"), ToolStatusKind.Error);
                return false;
            }

            if (!LocaleGapWriter.Write(gap, value))
            {
                SetStatus(ToolUi.Tr("tools/common/readonly"), ToolStatusKind.Error);
                return false;
            }

            GameLogger.Log("LOCALE_EDITOR", $"translation {gap.Key} [{gap.Language}] written to {gap.ModId}/{gap.TargetFile}");
            LocaleFileService.ReloadGame();
            SetStatus(string.Format(ToolUi.Tr("tools/locale_editor/saved"), gap.Key, gap.TargetFile), ToolStatusKind.Ok);
            ShowGap(null);
            StartScan();
            return true;
        }

        private static Label ValueLabel()
        {
            var label = new Label
            {
                ClipText = true,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            return label;
        }
    }
}
