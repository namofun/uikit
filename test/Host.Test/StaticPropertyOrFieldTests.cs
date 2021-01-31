using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SatelliteSite.Tests
{
    public class StaticPropertyOrFieldTests
    {
        [Fact]
        public void ReviewStaticMembers()
        {
            using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().AddDebug());
            var logger = loggerFactory.CreateLogger<StaticPropertyOrFieldTests>();

            var asm = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a =>
                {
                    var name = a.GetName().Name;
                    if (name == null) return false;
                    if (!name.StartsWith("SatelliteSite.")) return false;
                    if (name.EndsWith(".Tests")) return false;
                    return true;
                })
                .ToList();

            var justification = new Dictionary<string, string>
            {
                ["[SatelliteSite.TestServer]::[SatelliteSite.Tests.MiscTestsExtensions]::[CreateHandlersDelegate]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAppendText]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAppendHtml]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAppendFormat]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAddCssClass]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAddCssClass2]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAddAttr2]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[DoAddAttr]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[StringFormat]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[FactoryTableCell]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[FactoryTableRow]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[ExpTableCell]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[ExpTableRow]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions]::[FactoryFiller]"] = "Fast Reflect",
                ["[SatelliteSite.DataAccess]::[Microsoft.EntityFrameworkCore.EntityTypeConfigurationSupplier`1]::[GenericType]"] = "Fast Reflect",
                ["[SatelliteSite.DataAccess]::[Microsoft.EntityFrameworkCore.EntityTypeConfigurationSupplier`1]::[ApplyConfiguration]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.ReExecuteEndpointDataSource]::[_locker]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Routing.ModuleEndpointDataSource]::[_locker]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Routing.ModuleEndpointDataSource]::[_cbFactory]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Routing.ModuleEndpointDataSource]::[_actionEndpointFactoryType]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Routing.ModuleEndpointDataSource]::[AddEndpoints]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Routing.ModuleEndpointDataSource]::[_createCoreMethod]"] = "Fast Reflect",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.MenuNameDefaults]::[_containsWithComparer]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Routing.OrderLinkGenerator]::[typeInner]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Mvc.Routing.SubstrateUrlHelperFactory]::[_nRewriteUrlHelper]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Mvc.Routing.SubstrateUrlHelperFactory]::[_nEndpointRoutingUrlHelper]"] = "Fast Reflect",

                ["[SatelliteSite.Host]::[SatelliteSite.Program]::[Current]"] = "Global Entry Point",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.Rendering.HtmlHelperExtensions]::[EnglishCulture]"] = "Global Entry Point",

                ["[SatelliteSite.Abstraction]::[System.Linq.Expressions.ExpressionExtensions+ParameterExpressionHolder`1]::[Param]"] = "Type Related Singleton",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.TagHelpers.DataTablesTagHelper]::[FactoryCache]"] = "Type Related Singleton",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Mvc.Menus.ConcreteComponentBuilder]::[param]"] = "Type Related Singleton",

                ["[SatelliteSite.SampleModule]::[SatelliteSite.SampleModule.Services.ForecastService]::[Summaries]"] = "Constant Array",
                ["[SatelliteSite.Substrate]::[System.ComponentModel.DataAnnotations.UserNameAttribute]::[AllowedCharacters]"] = "Constant Array",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.TagHelpers.GravatarTagHelper]::[_chars]"] = "Constant Array",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.TrackAvailabilityMetadata]::[Default]"] = "Constant Object",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.TrackAvailabilityMetadata]::[ErrorHandler]"] = "Constant Object",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.TrackAvailabilityMetadata]::[Fallback]"] = "Constant Object",
            };

            int errorCount = 0;
            const BindingFlags searchFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var assembly in asm)
            {
                var assemblyName = assembly.GetName().Name;
                var types = assembly.GetTypes().ToList();
                for (int i = 0; i < types.Count; i++)
                    types.AddRange(types[i].GetNestedTypes(searchFlags));

                foreach (var type in types)
                {
                    if (type.IsEnum) continue;
                    var typeName = type.FullName;
                    if (typeName.Contains("+<")) continue;
                    var fields = type.GetFields(searchFlags).Where(f => !f.IsLiteral);
                    var props = type.GetProperties(searchFlags);
                    var checks = Enumerable.Concat<MemberInfo>(fields, props);

                    foreach (var member in checks)
                    {
                        if (member.Name.StartsWith('<')) continue;
                        if (assemblyName.EndsWith(".Views") && member.Name.StartsWith("__tagHelperAttribute_")) continue;
                        var memberName = $"[{assemblyName}]::[{typeName}]::[{member.Name}]";

                        if (justification.TryGetValue(memberName, out var say))
                        {
                            if (say == "Fast Reflect") continue;
                            logger.LogInformation("Justificated {memberName} as {say}", memberName, say);
                        }
                        else
                        {
                            logger.LogError("{memberName} not justificated.", memberName);
                            errorCount++;
                        }
                    }
                }
            }

            Assert.Equal(0, errorCount);
        }
    }
}
