namespace Sitecore.Support.Data.ItemResolvers
{
    using Sitecore.Configuration;
    using Sitecore.Data.ItemResolvers;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;

    public class MixedItemNameResolver: Sitecore.Data.ItemResolvers.MixedItemNameResolver
    {
        public MixedItemNameResolver(ItemPathResolver defaultResolver) : base(defaultResolver)
        {
        }
        protected override Item ResolveRecursive(Item root, List<string> subPaths)
        {
            Assert.ArgumentNotNull(root, "root");
            Assert.ArgumentNotNull(subPaths, "subPaths");
            if (subPaths.Count == 0)
            {
                return root;
            }
            string name = subPaths[0];
            List<string> subPaths2 = subPaths.Skip(1).ToList<string>();
            Item item = null;
            string[] name2 = this.Tokenize(name);
            foreach (Item item2 in root.Children)
            {
                string[] name3 = this.Tokenize(item2.Name.ToLowerInvariant());
                if (this.AreTokenizedNamesEqual(name2, name3))
                {
                    item = this.ResolveRecursive(item2, subPaths2);
                    if (item != null)
                    {
                        break;
                    }
                    if (Settings.ItemResolving.FindBestMatch != MixedItemNameResolvingMode.DeepScan)
                    {
                        break;
                    }
                }
                #region Sitecore.Support.163173
                if (item == null)
                {
                    string[] name4 = this.Tokenize(item2.DisplayName.ToLowerInvariant());
                    if (this.AreTokenizedNamesEqual(name2, name4))
                    {
                        item = this.ResolveRecursive(item2, subPaths2);
                    }
                }
                #endregion
            }
            return item;
        }

    }
}
