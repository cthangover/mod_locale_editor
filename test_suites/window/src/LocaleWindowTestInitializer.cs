namespace Cthangover.Tools.LocaleEditor.Tests
{
    using System.Linq;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Utils;
    using Godot;

    /// <summary>Mounts <see cref="LocaleWindowTestDriver"/> when the game runs with <c>--itest=locale_window</c>.</summary>
    public class LocaleWindowTestInitializer : IModInitializer
    {
        private static bool driverQueued;

        /// <inheritdoc />
        public void OnModLoaded(string modId)
        {
        }

        /// <inheritdoc />
        public void OnModResourcesReady()
        {
            if (driverQueued || !OS.GetCmdlineUserArgs().Any(a => a == "--itest=locale_window"))
                return;
            driverQueued = true;
            GameLogger.Log("TEST", "LocaleWindowTestInitializer: activating locale_window");
            ModRoot.AddDeferred(new LocaleWindowTestDriver());
        }
    }
}
