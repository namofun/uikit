using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Provides several common menu names.
    /// The extension class to configure the menu entries and provide functions to finalize the model.
    /// </summary>
    public static class MenuNameDefaults
    {
        /// <summary>
        /// The menu prefix.
        /// </summary>
        private const string Menu_ = nameof(Menu_);

        /// <summary>
        /// The submenu prefix.
        /// </summary>
        private const string Submenu_ = Menu_ + nameof(Submenu_);

        /// <summary>
        /// The main menu displayed on the dashboard navigation bar.
        /// </summary>
        public const string DashboardNavbar = Menu_ + nameof(DashboardNavbar);

        /// <summary>
        /// The cards displayed on the dashboard content area.
        /// </summary>
        public const string DashboardContent = Menu_ + nameof(DashboardContent);

        /// <summary>
        /// The users card on the dashboard content area.
        /// </summary>
        public const string DashboardUsers = Submenu_ + nameof(DashboardUsers);

        /// <summary>
        /// The configurations card on the dashboard content area.
        /// </summary>
        public const string DashboardConfigurations = Submenu_ + nameof(DashboardConfigurations);

        /// <summary>
        /// The documents card on the dashboard content area.
        /// </summary>
        public const string DashboardDocuments = Submenu_ + nameof(DashboardDocuments);

        /// <summary>
        /// The dropdown submenu on the top right corner.
        /// </summary>
        public const string UserDropdown = Submenu_ + nameof(UserDropdown);

        /// <summary>
        /// Configure the defaults for menus.
        /// </summary>
        /// <param name="menus">The menu contributor.</param>
        internal static void ConfigureDefaults(this IMenuContributor menus)
        {
            menus.Menu(UserDropdown, user =>
            {

            });

            menus.Menu(DashboardNavbar, admin =>
            {

            });

            menus.Menu(DashboardContent, menu =>
            {
                menu.HasSubmenu(DashboardConfigurations, 0, conf =>
                {
                    conf.HasTitle(string.Empty, "Infrastructure")
                        .HasLink("javascript:;");
                });

                menu.HasSubmenu(DashboardUsers, 100, user =>
                {
                    user.HasTitle(string.Empty, "Users")
                        .HasLink("javascript:;");
                });

                menu.HasSubmenu(DashboardDocuments, 200, docs =>
                {
                    docs.HasTitle(string.Empty, "Documentation")
                        .HasLink("javascript:;");
                });
            });
        }

        /// <summary>
        /// The default error string
        /// </summary>
        internal const string EverConfigured = "This configuration has been set before. Configurations cannot be override.";

        /// <summary>
        /// The model finalized.
        /// </summary>
        internal const string ModelFinalized = "Model has been finalized.";

        /// <summary>
        /// Set the title for this menu entry.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder HasTitle<TBuilder>(this TBuilder that, string icon, string title) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (that.Metadata.ContainsKey("Title"))
                throw new InvalidOperationException(EverConfigured);
            that.Metadata.Add("Title", title);
            that.Metadata.Add("Icon", icon);
            return that;
        }

        /// <summary>
        /// Set the external link.
        /// </summary>
        /// <param name="link">The external link.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder HasLink<TBuilder>(this TBuilder that, string link) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (that.Metadata.ContainsKey("Link"))
                throw new InvalidOperationException(EverConfigured);

            var rvd = new RouteValueDictionary
            {
                [nameof(link)] = link,
            };

            that.Metadata.Add("Link", rvd);
            return that;
        }

        /// <summary>
        /// Set the route link.
        /// </summary>
        /// <param name="area">The area name.</param>
        /// <param name="controller">The controller name.</param>
        /// <param name="action">The action name.</param>
        /// <param name="routeValues">The anonymous object to provide route values.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder HasLink<TBuilder>(this TBuilder that, string area, string controller, string action, object? routeValues = null) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (that.Metadata.ContainsKey("Link"))
                throw new InvalidOperationException(EverConfigured);

            var rvd = new RouteValueDictionary
            {
                [nameof(area)] = area,
                [nameof(controller)] = controller,
                [nameof(action)] = action,
            };

            if (routeValues != null)
            {
                var vtype = routeValues.GetType();
                if ((!vtype.FullName?.StartsWith("<>f__AnonymousType")) ?? true)
                    throw new ArgumentException(nameof(routeValues));
                foreach (var p in vtype.GetProperties())
                    rvd[p.Name] = p.GetValue(routeValues);
            }

            that.Metadata.Add("Link", rvd);
            return that;
        }

        /// <summary>
        /// Set the badge icon.
        /// </summary>
        /// <param name="id">The badge name.</param>
        /// <param name="color">The badge color.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder HasBadge<TBuilder>(this TBuilder that, string id, BootstrapColor color) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);

            if (!that.Metadata.TryGetValue("Badges", out var ee))
                ee = that.Metadata["Badges"] = new List<(string, BootstrapColor)>();
            if (!(ee is List<(string, BootstrapColor)> badges))
                throw new InvalidOperationException(EverConfigured);

            badges.Add((id, color));
            return that;
        }

        /// <summary>
        /// Provide a function to detect the active status.
        /// </summary>
        /// <param name="area">The area name.</param>
        /// <param name="controller">The controller name.</param>
        /// <param name="action">The action name.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder ActiveWhen<TBuilder>(this TBuilder that, string area, string? controller = null, string? action = null) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);

            if (!that.Metadata.TryGetValue("ActiveWhen", out var ee))
                ee = that.Metadata["ActiveWhen"] = new List<(string, string?, string?)>();
            if (!(ee is List<(string, string?, string?)> activity))
                throw new InvalidOperationException(EverConfigured);

            activity.Add((area, controller, action));
            return that;
        }

        /// <summary>
        /// Require these roles.
        /// </summary>
        /// <param name="roles">The comma separated role list.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder RequireRoles<TBuilder>(this TBuilder that, string roles) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            that.Requirements.Add(c => c.User.IsInRoles(roles));
            return that;
        }

        /// <summary>
        /// Require this claim.
        /// </summary>
        /// <param name="claimKey">The claim key.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder RequireClaim<TBuilder>(this TBuilder that, string claimKey, string claimValue) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            that.Requirements.Add(c => c.User.HasClaim(claimKey, claimValue));
            return that;
        }
    }
}
