namespace Cthangover.Tools.LocaleEditor
{
    using Cthangover.Tools.Services;

    /// <summary>
    /// Writes one translation into the locale file the gap names. The line is changed through
    /// <see cref="PropertiesText"/>, so the rest of the file stays byte for byte and a save shows in a diff as
    /// one line. Only folder-backed mods can be written (zip mods are read-only).
    /// </summary>
    public static class LocaleGapWriter
    {
        /// <summary>
        /// Sets the gap's key to <paramref name="value"/> in its target file. False when the value is empty,
        /// the mod is read-only, or the file could not be written.
        /// </summary>
        public static bool Write(LocaleGap gap, string value)
        {
            if (gap == null || string.IsNullOrWhiteSpace(value))
                return false;
            if (!ModFileService.IsWritable(gap.ModId))
                return false;

            var current = ModFileService.ReadText(gap.ModId, gap.TargetFile) ?? string.Empty;
            return ModFileService.WriteText(gap.ModId, gap.TargetFile, PropertiesText.Set(current, gap.Key, value.Trim()));
        }
    }
}
