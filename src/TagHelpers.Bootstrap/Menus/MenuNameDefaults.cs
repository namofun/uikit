﻿using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        /// The main menu displayed on the navigation bar.
        /// </summary>
        public const string MainNavbar = Menu_ + nameof(MainNavbar);

        /// <summary>
        /// Configure the defaults for menus.
        /// </summary>
        /// <param name="menus">The menu contributor.</param>
        internal static void ConfigureDefaults(this IMenuContributor menus)
        {
            menus.Menu(MainNavbar, menu =>
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
        /// Set the dom id.
        /// </summary>
        /// <param name="id">The dom id.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder HasIdentifier<TBuilder>(this TBuilder that, string id) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (that.Metadata.ContainsKey("Id"))
                throw new InvalidOperationException(EverConfigured);
            that.Metadata.Add("Id", id);
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

        private static MethodInfo? _containsWithComparer;

        /// <summary>
        /// Create an expression for <see cref="ViewContext"/>.
        /// </summary>
        /// <param name="src">The corresponding field.</param>
        /// <param name="dest">The results.</param>
        /// <returns>The compiled view context.</returns>
        private static Expression<Func<ViewContext, bool>> CreateExpression(Expression<Func<ViewContext, string?>> src, string dest)
        {
            if (_containsWithComparer == null)
            {
                Expression<Func<IEnumerable<string?>, bool>> exp = c => c.Contains(null!, null!);
                var body = (MethodCallExpression)exp.Body;
                _containsWithComparer = body.Method;
            }

            var dests = dest.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var call = Expression.Call(_containsWithComparer,
                Expression.Constant(dests, typeof(IEnumerable<string>)),
                src.Body,
                Expression.Constant(StringComparer.OrdinalIgnoreCase, typeof(IEqualityComparer<string>)));
            return Expression.Lambda<Func<ViewContext, bool>>(call, src.Parameters);
        }

        /// <summary>
        /// Provide a function to detect the active status.
        /// </summary>
        /// <param name="area">The area names, separated by comma.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder ActiveWhenArea<TBuilder>(this TBuilder that, string area) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (area == null)
                throw new ArgumentNullException(nameof(area));
            that.Activities.Add(CreateExpression(c => (string?)c.RouteData.Values.GetValueOrDefault("area"), area));
            return that;
        }

        /// <summary>
        /// Provide a function to detect the active status.
        /// </summary>
        /// <param name="controller">The controller names, separated by comma.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder ActiveWhenController<TBuilder>(this TBuilder that, string controller) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            that.Activities.Add(CreateExpression(c => ((ControllerActionDescriptor)c.ActionDescriptor).ControllerName, controller));
            return that;
        }

        /// <summary>
        /// Provide a function to detect the active status.
        /// </summary>
        /// <param name="action">The action names, separated by comma.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder ActiveWhenAction<TBuilder>(this TBuilder that, string action) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            that.Activities.Add(CreateExpression(c => ((ControllerActionDescriptor)c.ActionDescriptor).ActionName, action));
            return that;
        }

        /// <summary>
        /// Provide a function to detect the active status.
        /// </summary>
        /// <param name="value">The active action values, separated by comma.</param>
        /// <returns>The <typeparamref name="TBuilder"/> to chain the configures.</returns>
        public static TBuilder ActiveWhenViewData<TBuilder>(this TBuilder that, string value) where TBuilder : IMenuEntryBuilderBase
        {
            if (that.Finalized)
                throw new InvalidOperationException(ModelFinalized);
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            that.Activities.Add(CreateExpression(c => (string)c.ViewData["ActiveAction"], value));
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