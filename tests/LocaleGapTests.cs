using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cthangover.Core.Mods;
using Cthangover.Core.Mods.Testing;
using Cthangover.LocaleAtlas;

namespace Cthangover.Tools.LocaleEditor.Tests
{
    /// <summary>
    /// The window's list is the atlas' own findings read as gaps: which file a text goes into, which hint it
    /// shows, and that a complete key is not offered. Pure tests on in-memory mods (no Godot, no writes).
    /// </summary>
    public class LocaleGapTests
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        [ModTest("gaps/missing_language_goes_to_sibling_file")]
        static void MissingLanguageGoesToSiblingFile(ModTestContext ctx)
        {
            var mod = new MemoryProvider("m")
                .Add("locale/ui_en.properties", "ui/only_en = Only English\nui/both = Both\n")
                .Add("locale/ui_ru.properties", "ui/both = Оба\n");
            var other = new MemoryProvider("k").Add("locale/ko.properties", "other/key = Other\n");
            var result = LocaleAtlasBuilder.Build(new[] { ("m", (IModFileProvider)mod), ("k", other) });

            var gaps = LocaleGapList.From(result);
            var gap = gaps.Single(g => g.Key == "ui/only_en" && g.Language == "ko");
            ModAssert.Equal("m", gap.ModId, "owner is the mod that declares the key");
            ModAssert.Equal("locale/ui_ko.properties", gap.TargetFile, "the sibling of the English declaration");
            ModAssert.Equal("en", gap.SourceLanguage, "the hint is the English text");
            ModAssert.Equal("Only English", gap.SourceText, "hint text");
            ModAssert.Equal("m|ui/only_en|ko", gap.Id, "row id");
        }

        [ModTest("gaps/complete_keys_are_not_offered")]
        static void CompleteKeysAreNotOffered(ModTestContext ctx)
        {
            var mod = new MemoryProvider("m")
                .Add("locale/en.properties", "ok = OK\nmod_m_name = M\nmod_m_desc = M\n")
                .Add("locale/ru.properties", "ok = Ок\nmod_m_name = М\nmod_m_desc = М\n");
            var result = LocaleAtlasBuilder.Build(new[] { ("m", (IModFileProvider)mod) });
            ModAssert.Equal(0, LocaleGapList.From(result).Count, "nothing is missing");
        }

        [ModTest("gaps/undeclared_scenario_key_targets_scenario_file")]
        static void UndeclaredScenarioKeyTargetsScenarioFile(ModTestContext ctx)
        {
            var mod = new MemoryProvider("s")
                .Add("locale/en.properties", "known = Known\n")
                .Add("locale/ru.properties", "known = Известно\n")
                .Add("scenarios/intro.scenario", "text \"Привет\" key=intro/hello\nend\n");
            var result = LocaleAtlasBuilder.Build(new[] { ("s", (IModFileProvider)mod) });

            var gaps = LocaleGapList.From(result).Where(g => g.Key == "intro/hello").ToList();
            ModAssert.Equal(2, gaps.Count, "undeclared in every project language");
            var en = gaps.Single(g => g.Language == "en");
            ModAssert.Equal("locale/scenarios_en.properties", en.TargetFile, "scenario keys go to scenarios_<lang>");
            ModAssert.Equal("ru", en.SourceLanguage, "the Russian literal is the hint");
            ModAssert.Equal("Привет", en.SourceText, "hint text is the literal");
        }

        [ModTest("gaps/target_file_reads_the_fix_prefix")]
        static void TargetFileReadsFixPrefix(ModTestContext ctx)
        {
            var finding = new LocaleFinding { Key = "a/b", Fix = "locale/ru.properties: a/b = текст: с двоеточием" };
            ModAssert.Equal("locale/ru.properties", LocaleGapList.TargetFile(finding), "file part");
            ModAssert.True(LocaleGapList.TargetFile(new LocaleFinding { Key = "a/b", Fix = null }) == null, "no fix, no target");
        }

        /// <summary>In-memory mod: paths are mod-relative with '/' separators.</summary>
        private sealed class MemoryProvider : IModFileProvider
        {
            private readonly Dictionary<string, byte[]> files = new(StringComparer.Ordinal);

            public MemoryProvider(string mod) => Mod = mod;

            public string Mod { get; }

            public string RootPath => null;

            public MemoryProvider Add(string path, string text)
            {
                files[path] = Utf8.GetBytes(text);
                return this;
            }

            public bool FileExists(string path) => files.ContainsKey(path);

            public IEnumerable<string> ListFiles(string directory = "")
            {
                var prefix = string.IsNullOrEmpty(directory) ? string.Empty : directory.TrimEnd('/') + "/";
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var path in files.Keys.Where(p => p.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    var rest = path.Substring(prefix.Length);
                    var slash = rest.IndexOf('/');
                    var entry = slash < 0 ? path : prefix + rest.Substring(0, slash + 1);
                    if (seen.Add(entry))
                        yield return entry;
                }
            }

            public string ReadFileText(string path) => files.TryGetValue(path, out var bytes) ? Utf8.GetString(bytes) : null;

            public byte[] ReadFileBinary(string path) => files.TryGetValue(path, out var bytes) ? bytes : null;

            public Stream OpenStream(string path) => files.TryGetValue(path, out var bytes) ? new MemoryStream(bytes) : null;

            public string GetFileSystemPath(string path) => null;

            public string ResolveIncludePath(string includePath, string currentPath) => includePath;

            public void Dispose() { }
        }
    }
}
