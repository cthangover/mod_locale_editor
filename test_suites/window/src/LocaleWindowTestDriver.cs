namespace Cthangover.Tools.LocaleEditor.Tests
{
    using System.Linq;
    using Cthangover.Core.Bootstrap;
    using Cthangover.Core.Localization;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Utils;
    using Cthangover.Mods.Interface.Tool;
    using Cthangover.Tools.Services;
    using Godot;

    /// <summary>
    /// Drives the translations window like a translator: the probe key of this suite is missing in Korean, so the
    /// window lists it; the translator picks Korean and the row, types the text and saves. The suite's Korean file
    /// gets the line, the atlas lists one gap less, and the game shows the text. Every file the test made is removed
    /// and the game's language is restored.
    /// </summary>
    public partial class LocaleWindowTestDriver : Node
    {
        private const float TickSeconds = 0.15f;
        private const float TimeoutSeconds = 90f;
        private const string Mod = "locale_editor__window";
        private const string Probe = "locale_editor/probe";
        private const string KoFile = "locale/ko.properties";
        private const string Typed = "탐침 텍스트";
        private const string Id = Mod + "|" + Probe + "|ko";

        private readonly System.Collections.Generic.List<string> failures = new();
        private int passed;
        private int step;
        private double elapsed;
        private int gapsBefore;
        private string originalLocale;
        private LocaleEditorWindow window;

        public LocaleWindowTestDriver()
        {
            Name = "LocaleWindowTestDriver";
            var timer = new Timer { WaitTime = TickSeconds, Autostart = true };
            AddChild(timer);
            timer.Timeout += Tick;
        }

        private void Tick()
        {
            if (step < 0)
                return;
            elapsed += TickSeconds;
            if (elapsed > TimeoutSeconds)
            {
                Fail($"timeout at step {step}");
                Cleanup();
                Finish();
                return;
            }

            try
            {
                RunStep();
            }
            catch (System.Exception ex)
            {
                Fail($"step {step}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                Cleanup();
                Finish();
            }
        }

        private void RunStep()
        {
            switch (step)
            {
                case 0:
                    if (!Boot.IsReady)
                        return;
                    originalLocale = TranslationServer.GetLocale();
                    Check(ModFileService.IsWritable(Mod), "the suite's own folder is writable");
                    Check(ModFileService.ReadText(Mod, KoFile) == null, "the suite has no Korean file yet");
                    window = ToolWindow.Open<LocaleEditorWindow>();
                    step++;
                    return;
                case 1:
                    if (!window.IsInsideTree() || window.Scanning || window.Gaps.Count == 0)
                    {
                        if (elapsed > TimeoutSeconds - 5)
                            Fail("the window found no gaps");
                        return;
                    }
                    var gap = window.Gaps.FirstOrDefault(g => g.Id == Id);
                    Check(gap != null, "the window lists the probe key as missing in Korean");
                    Check(gap != null && gap.TargetFile == KoFile, $"the Korean line goes to {KoFile} ({gap?.TargetFile})");
                    Check(gap != null && gap.SourceText == "Probe text" && gap.SourceLanguage == "en", "the hint is the English text");
                    gapsBefore = window.Gaps.Count(g => g.Language == "ko");
                    window.SelectLanguage("ko");
                    Check(window.Language == "ko" && window.Shown.Any(g => g.Id == Id), "Korean shows the probe row");
                    Check(window.SelectGap(Id), "the row opens its form");
                    Check(window.Current?.Key == Probe, "the form is for the probe key");
                    window.TypeText(Typed);
                    Check(window.SaveNow(), "Save writes the translation");
                    step++;
                    return;
                case 2:
                    if (window.Scanning)
                        return;
                    var written = ModFileService.ReadText(Mod, KoFile) ?? string.Empty;
                    Check(written.Contains($"{Probe} = {Typed}"), "the Korean file holds the translated line");
                    Check(window.Gaps.Count(g => g.Language == "ko") == gapsBefore - 1, $"the atlas lists one Korean gap less ({gapsBefore} -> {window.Gaps.Count(g => g.Language == "ko")})");
                    Check(window.Gaps.All(g => g.Id != Id), "the probe row is gone from the list");
                    LocaleLoader.LoadLocale("ko");
                    Check(TranslationServer.Translate(Probe) == Typed, "the game shows the typed text in Korean");
                    step++;
                    return;
                case 3:
                    Cleanup();
                    Finish();
                    return;
            }
        }

        private void Check(bool condition, string what)
        {
            if (condition)
            {
                passed++;
                GameLogger.Log("TEST", $"[PASS] {what}");
            }
            else
            {
                Fail(what);
            }
        }

        private void Fail(string what)
        {
            failures.Add(what);
            GameLogger.Log("TEST", $"[FAIL] {what}", LogLevel.Error);
        }

        private void Cleanup()
        {
            if (step < 0)
                return;
            if (window != null && IsInstanceValid(window))
                window.QueueFree();
            if (ModFileService.ReadText(Mod, KoFile) != null)
                ModFileService.DeleteFile(Mod, KoFile);
            if (originalLocale != null)
                LocaleLoader.LoadLocale(originalLocale);
        }

        private void Finish()
        {
            step = -1;
            GameLogger.Log("TEST", $"[TEST_RESULT] name=locale_window passed={passed} failed={failures.Count}");
            ApplicationExit.Quit(GetTree(), failures.Count == 0 ? 0 : 1);
        }
    }
}
