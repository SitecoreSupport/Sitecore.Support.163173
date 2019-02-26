namespace Sitecore.Support.Pipelines.HttpRequest
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Abstractions;
    using Sitecore.Configuration;
    using Sitecore.Data.ItemResolvers;
    using Sitecore.DependencyInjection;
    using Sitecore.Pipelines.HttpRequest;
    /// <summary>
    /// Resolves the current item from the query string path.
    /// </summary>
    public class ItemResolver : Sitecore.Pipelines.HttpRequest.ItemResolver
    {
        public ItemResolver() : this(ServiceLocator.ServiceProvider.GetRequiredService<BaseItemManager>(), ServiceLocator.ServiceProvider.GetRequiredService<ItemPathResolver>())
        {
        }

        public ItemResolver(BaseItemManager itemManager, ItemPathResolver pathResolver) : this(itemManager, pathResolver, Settings.ItemResolving.FindBestMatch)
        {
        }

        protected ItemResolver(BaseItemManager itemManager, ItemPathResolver pathResolver, MixedItemNameResolvingMode itemNameResolvingMode): base(itemManager, pathResolver, itemNameResolvingMode)
        {
            base.PathResolver = (((base.ItemNameResolvingMode & MixedItemNameResolvingMode.Enabled) == MixedItemNameResolvingMode.Enabled) ? new Sitecore.Support.Data.ItemResolvers.MixedItemNameResolver(base.PathResolver) : base.PathResolver);
        }
        public override void Process(HttpRequestArgs args)
        {
            base.Process(args);
        }
    }
}
