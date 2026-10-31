namespace Cthangover.Tools.LocaleEditor
{
    using Cthangover.Core.UI.Tool;
    using Godot;

    /// <summary>
    /// Registers the translations window opened from the in-game toolbar through
    /// <see cref="LocaleEditorToolBoxButton"/>.
    /// </summary>
    public class LocaleEditorToolProvider : IToolProvider
    {
        /// <summary>Tool id.</summary>
        public string Id => "locale_editor";

        /// <summary>Display name key.</summary>
        public string LocaleKey => "tools/locale_editor/title";

        /// <summary>Creates the window.</summary>
        public Window CreateWindow() => new LocaleEditorWindow();
    }
}
