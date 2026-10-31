using System;
using System.Collections.Generic;
using System.Linq;
using Cthangover.LocaleAtlas;

namespace Cthangover.Tools.LocaleEditor
{
    /// <summary>
    /// Turns the locale atlas result into the list of gaps the window shows. Pure C# (no Godot), so the
    /// rules are pinned by unit tests. The file a gap is written to is read from the atlas' own fix
    /// (<c>&lt;file&gt;: &lt;key&gt; = &lt;hint&gt;</c>): the atlas already knows the sibling file of a
    /// declaration (<c>ui_en.properties</c> to <c>ui_ko.properties</c>) and the scenario or plain file of an
    /// undeclared key, so the window and the checker never disagree about where a translation goes.
    /// </summary>
    public static class LocaleGapList
    {
        private static readonly string[] Codes =
        {
            "key_missing_language",
            "key_partial_override",
            "key_undeclared",
            "key_undeclared_optional",
        };

        /// <summary>One gap per (mod, key, language), sorted by mod, key and language.</summary>
        public static List<LocaleGap> From(LocaleAtlasResult result)
        {
            var gaps = new List<LocaleGap>();
            if (result == null)
                return gaps;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var finding in result.Findings)
            {
                if (!Codes.Contains(finding.Code) || string.IsNullOrEmpty(finding.Key)
                    || string.IsNullOrEmpty(finding.ModId) || string.IsNullOrEmpty(finding.Language))
                    continue;

                var target = TargetFile(finding);
                if (target == null)
                    continue;

                var gap = new LocaleGap
                {
                    Key = finding.Key,
                    ModId = finding.ModId,
                    Language = finding.Language,
                    TargetFile = target,
                    Code = finding.Code,
                };
                if (!seen.Add(gap.Id))
                    continue;

                FillSource(result, gap);
                gaps.Add(gap);
            }

            return gaps
                .OrderBy(g => g.ModId, StringComparer.Ordinal)
                .ThenBy(g => g.Key, StringComparer.Ordinal)
                .ThenBy(g => g.Language, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>The file part of a finding's fix (<c>locale/ru.properties: key = …</c>); null when the fix has another shape.</summary>
        public static string TargetFile(LocaleFinding finding)
        {
            var marker = ": " + finding.Key + " = ";
            var fix = finding.Fix ?? string.Empty;
            var at = fix.IndexOf(marker, StringComparison.Ordinal);
            return at <= 0 ? null : fix.Substring(0, at);
        }

        private static void FillSource(LocaleAtlasResult result, LocaleGap gap)
        {
            if (result.Keys.TryGetValue(gap.Key, out var row))
            {
                foreach (var language in OrderedLanguages(result))
                {
                    if (language == gap.Language || !row.Declarations.TryGetValue(language, out var entries))
                        continue;

                    var entry = entries.FirstOrDefault(e => e.ModId == gap.ModId && e.Value.Length > 0)
                                ?? entries.FirstOrDefault(e => e.Value.Length > 0);
                    if (entry == null)
                        continue;

                    gap.SourceLanguage = language;
                    gap.SourceText = entry.Value;
                    return;
                }

                // A key used by scenarios has its Russian literal as the fallback text of the reference.
                var fallback = row.References.FirstOrDefault(r => r.ModId == gap.ModId && !string.IsNullOrEmpty(r.Fallback))
                               ?? row.References.FirstOrDefault(r => !string.IsNullOrEmpty(r.Fallback));
                if (fallback != null)
                {
                    gap.SourceLanguage = "ru";
                    gap.SourceText = fallback.Fallback;
                }
            }
        }

        /// <summary>English and Russian first (the hints authors write), then the other project languages.</summary>
        private static IEnumerable<string> OrderedLanguages(LocaleAtlasResult result)
            => result.Languages
                .OrderBy(l => l == "en" ? 0 : l == "ru" ? 1 : 2)
                .ThenBy(l => l, StringComparer.Ordinal);
    }
}
