using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.ItemResolvers;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.SecurityModel;
using Sitecore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Support.Pipelines.HttpRequest
{
    public class ItemResolver : HttpRequestProcessor
    {
        /// <summary>
        /// Item path resolver.
        /// </summary>
        private ItemPathResolver pathResolver;

        /// <summary>
        /// Gets or sets item path resolver.
        /// </summary>
        /// <value>Item path resolver.</value>
        protected ItemPathResolver PathResolver
        {
            get
            {
                if (this.pathResolver == null)
                {
                    ItemPathResolver itemPathResolver = new ContentItemPathResolver();
                    this.pathResolver = (((Settings.ItemResolving.FindBestMatch & MixedItemNameResolvingMode.Enabled) == MixedItemNameResolvingMode.Enabled) ? new Sitecore.Support.Data.ItemResolvers.MixedItemNameResolver(itemPathResolver) : itemPathResolver);
                }
                return this.pathResolver;
            }
            set
            {
                this.pathResolver = value;
            }
        }

        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (Context.Item != null || Context.Database == null || args.Url.ItemPath.Length == 0)
            {
                return;
            }
            Profiler.StartOperation("Resolve current item.");
            string text = MainUtil.DecodeName(args.Url.ItemPath);
            Item item = args.GetItem(text);
            if (item == null && !args.PermissionDenied)
            {
                text = args.Url.ItemPath;
                item = args.GetItem(text);
            }
            SiteContext site = Context.Site;
            string text2 = (site != null) ? site.RootPath : string.Empty;
            if (item == null && !args.PermissionDenied)
            {
                text = args.LocalPath;
                item = args.GetItem(text);
            }
            if (item == null && !args.PermissionDenied)
            {
                text = MainUtil.DecodeName(args.LocalPath);
                item = args.GetItem(text);
            }
            if (item == null && !args.PermissionDenied)
            {
                text = FileUtil.MakePath(text2, args.LocalPath, '/');
                item = args.GetItem(text);
            }
            if (item == null && !args.PermissionDenied)
            {
                text = MainUtil.DecodeName(FileUtil.MakePath(text2, args.LocalPath, '/'));
                item = args.GetItem(text);
            }
            if (item == null && !args.PermissionDenied)
            {
                string text3 = (site != null) ? site.StartItem : string.Empty;
                string[] source = new string[]
                {
                    text2,
                    MainUtil.DecodeName(text2)
                };
                string[] startItemArray = new string[]
                {
                    text3,
                    MainUtil.DecodeName(text3)
                };
                string[] localPathArray = new string[]
                {
                    args.LocalPath,
                    MainUtil.DecodeName(args.LocalPath)
                };
                IEnumerable<string> source2 = from s1 in source
                                              from s2 in startItemArray
                                              from s3 in localPathArray
                                              select FileUtil.MakePath(FileUtil.MakePath(s1, s2, '/'), s3, '/');
                foreach (string current in source2.Distinct<string>())
                {
                    item = args.GetItem(current);
                    if (item != null)
                    {
                        break;
                    }
                }
            }
            if ((item == null || item.Name.Equals("*")) && !args.PermissionDenied)
            {
                Item item2 = this.ResolveUsingDisplayName(args);
                if (item2 != null)
                {
                    item = item2;
                }
            }
            if (item == null && args.UseSiteStartPath && site != null && !args.PermissionDenied)
            {
                item = args.GetItem(site.StartPath);
            }
            if (item != null)
            {
                Tracer.Info("Current item is \"" + text + "\".");
            }
            Context.Item = item;
            Profiler.EndOperation();
        }

        /// <summary>
        /// Resolves the full path.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        private Item ResolveFullPath(HttpRequestArgs args)
        {
            return this.ResolvePath(args.Url.ItemPath) ?? this.ResolvePath(args.LocalPath);
        }

        /// <summary>
        /// Resolves the path based on the url eg: args.Url.ItemPath or args.LocalPath.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private Item ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path[0] != '/')
            {
                return null;
            }
            int num = path.IndexOf('/', 1);
            if (num < 0)
            {
                return null;
            }
            Item item = ItemManager.GetItem(path.Substring(0, num), Language.Current, Sitecore.Data.Version.Latest, Context.Database, SecurityCheck.Disable);
            if (item == null)
            {
                return null;
            }
            string path2 = path.Substring(num);
            return this.PathResolver.ResolveItem(path2, item);
        }

        /// <summary>
        /// Resolves the local path.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        private Item ResolveLocalPath(HttpRequestArgs args)
        {
            SiteContext site = Context.Site;
            if (site == null)
            {
                return null;
            }
            Item item = ItemManager.GetItem(site.RootPath, Language.Current, Sitecore.Data.Version.Latest, Context.Database, SecurityCheck.Disable);
            if (item == null)
            {
                return null;
            }
            string localPath = args.LocalPath;
            return this.PathResolver.ResolveItem(localPath, item);
        }

        /// <summary>
        /// Resolves the display name of the using.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        private Item ResolveUsingDisplayName(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item;
            using (new SecurityDisabler())
            {
                item = this.ResolveLocalPath(args);
                if (item == null)
                {
                    item = this.ResolveFullPath(args);
                }
                if (item == null)
                {
                    return null;
                }
            }
            return args.ApplySecurity(item);
        }
    }
}