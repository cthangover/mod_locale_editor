namespace Cthangover.Tools.LocaleEditor
{
    /// <summary>
    /// One text the translations window can type: a key that a project language lacks in a mod, or a key a
    /// mod uses but declares nowhere. Gaps come from the locale atlas result (<see cref="LocaleGapList"/>),
    /// so the atlas stays the only checker of what is missing.
    /// </summary>
    public sealed class LocaleGap
    {
        /// <summary>The translation key, as the game asks for it.</summary>
        public string Key { get; set; }

        /// <summary>The mod that owns the key; the text is written into this mod's folder.</summary>
        public string ModId { get; set; }

        /// <summary>The language that lacks the text.</summary>
        public string Language { get; set; }

        /// <summary>Mod-relative file the line goes into, for example <c>locale/ko.properties</c>.</summary>
        public string TargetFile { get; set; }

        /// <summary>Atlas finding code (<c>key_missing_language</c>, <c>key_partial_override</c>, <c>key_undeclared</c>, <c>key_undeclared_optional</c>).</summary>
        public string Code { get; set; }

        /// <summary>Language of the text shown as a hint next to the field; null when the key has no text yet.</summary>
        public string SourceLanguage { get; set; }

        /// <summary>The hint text: the same key in another language, or the literal a scenario uses.</summary>
        public string SourceText { get; set; }

        /// <summary>Unique within a scan: one row per mod, key and language.</summary>
        public string Id => ModId + "|" + Key + "|" + Language;
    }
}
