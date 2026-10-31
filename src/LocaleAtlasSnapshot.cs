namespace Cthangover.Tools.LocaleEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cthangover.Core.Mods;
    using Cthangover.LocaleAtlas;

    /// <summary>
    /// The mods the locale atlas scans, taken the way the atlas's own command takes them: loaded mods in load
    /// order, the installed ones after them, then the collection packages. Taken on the main thread (file
    /// providers are read from the scene tree's side); the atlas pass itself runs on a worker thread.
    /// </summary>
    public static class LocaleAtlasSnapshot
    {
        /// <summary>The providers of every mod and collection package that can hold locale files.</summary>
        public static List<(string ModId, IModFileProvider Provider)> Take()
        {
            var registry = ModRegistry.Instance;
            var result = new List<(string ModId, IModFileProvider Provider)>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in registry.GetOrderedModIds())
            {
                if (registry.Mods.TryGetValue(id, out var info) && info?.FileProvider != null && seen.Add(id))
                    result.Add((id, NestedFolders.Visible(info)));
            }

            foreach (var info in registry.AllDiscoveredMods.Values.OrderBy(m => m.Id, StringComparer.Ordinal))
            {
                if (info?.FileProvider != null && seen.Add(info.Id))
                    result.Add((info.Id, NestedFolders.Visible(info)));
            }

            foreach (var collection in ModCollectionStore.LoadAll())
            {
                if (collection.Package != null)
                    result.Add((LocaleAtlasBuilder.CollectionSourcePrefix + collection.Id, collection.Package));
            }

            return result;
        }
    }
}
