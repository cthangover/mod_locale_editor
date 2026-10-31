namespace Cthangover.Tools.LocaleEditor
{
    using Cthangover.Core.UI.Tool;

    /// <summary>In-game toolbar button of the translations window: a missing text is typed while playing.</summary>
    public class LocaleEditorToolBoxButton : IToolBoxButton
    {
        /// <summary>Matches <see cref="LocaleEditorToolProvider.Id"/>.</summary>
        public string ToolId => "locale_editor";

        /// <summary>No icon: the button shows its label.</summary>
        public string IconPath => "";

        /// <summary>Button label key.</summary>
        public string LocaleKey => "tools/locale_editor/title";

        /// <summary>Always shown.</summary>
        public bool IsVisible() => true;
    }
}
