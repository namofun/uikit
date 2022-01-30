using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SatelliteSite.Tests
{
    public class StaticPropertyOrFieldTests
    {
        private readonly ITestOutputHelper output;

        public StaticPropertyOrFieldTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ReviewStaticMembers()
        {
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
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Builder.ConnectionEndpointBuilderExtensions]::[_httpConnectionDispatcherType]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Builder.SignalREndpointBuilderExtensions]::[_signalRMarkerServiceType]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Builder.BlazorEndpointBuilderExtensions]::[_componentHubType]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Builder.BlazorEndpointBuilderExtensions]::[_circuitDisconnectMiddlewareType]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Builder.BlazorEndpointBuilderExtensions]::[_circuitJavaScriptInitializationMiddlewareType]"] = "Fast Reflect",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Builder.BlazorEndpointBuilderExtensions]::[_mapComponentHubDelegate]"] = "Fast Reflect",

                ["[SatelliteSite.Host]::[SatelliteSite.Program]::[Current]"] = "Global Entry Point",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.Rendering.HtmlHelperExtensions]::[EnglishCulture]"] = "Global Entry Point",

                ["[SatelliteSite.Abstraction]::[System.Linq.Expressions.ExpressionExtensions+ParameterExpressionHolder`1]::[Param]"] = "Type Related Singleton",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.TagHelpers.DataTablesTagHelper]::[FactoryCache]"] = "Type Related Singleton",
                ["[SatelliteSite.HostBuilder]::[Microsoft.AspNetCore.Mvc.Menus.ConcreteComponentBuilder]::[param]"] = "Type Related Singleton",
                ["[SatelliteSite.DataAccess]::[Microsoft.EntityFrameworkCore.Diagnostics.DiagnosticDbInterceptor]::[_diagnosticListener]"] = "Type Related Singleton",
                ["[SatelliteSite.DataAccess]::[Microsoft.EntityFrameworkCore.Diagnostics.DiagnosticDbInterceptor]::[Instance]"] = "Type Related Singleton",

                ["[SatelliteSite.Abstraction]::[System.SequentialGuidGenerator]::[_rng]"] = "Singleton",
                ["[SatelliteSite.Abstraction]::[System.SequentialGuidGenerator]::[DatabaseMapping]"] = "Constant Array",
                ["[SatelliteSite.AzureCloud]::[Microsoft.Extensions.FileProviders.AzureBlob.AzureBlobProvider+StrongPath]::[_unusablePathChars]"] = "Constant Array",
                ["[SatelliteSite.SampleModule]::[SatelliteSite.SampleModule.Services.ForecastService]::[Summaries]"] = "Constant Array",
                ["[SatelliteSite.Substrate]::[System.ComponentModel.DataAnnotations.UserNameAttribute]::[AllowedCharacters]"] = "Constant Array",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.TagHelpers.GravatarTagHelper]::[_chars]"] = "Constant Array",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Mvc.TagHelpers.UserInformationTagHelper]::[_emptyInfo]"] = "Constant Object",
                ["[SatelliteSite.IdentityModule]::[Microsoft.AspNetCore.Identity.UserInformationProviderBase`1]::[_emptyDict]"] = "Constant Object",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.TrackAvailabilityMetadata]::[Default]"] = "Constant Object",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.TrackAvailabilityMetadata]::[ErrorHandler]"] = "Constant Object",
                ["[SatelliteSite.Substrate]::[Microsoft.AspNetCore.Routing.TrackAvailabilityMetadata]::[Fallback]"] = "Constant Object",
                ["[SatelliteSite.TelemetryModule]::[SatelliteSite.TelemetryModule.Services.TelemetryDataClient]::[_batchRequestFailed]"] = "Logger Definition",
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
                        if (type.IsAssignableTo(typeof(IRazorPage)) && member.Name.StartsWith("__tagHelperAttribute_")) continue;
                        var memberName = $"[{assemblyName}]::[{typeName}]::[{member.Name}]";

                        if (justification.TryGetValue(memberName, out var say))
                        {
                            if (say == "Fast Reflect") continue;
                            output.WriteLine("[INFO] Justificated {0} as {1}", memberName, say);
                        }
                        else
                        {
                            output.WriteLine("[CRIT] Item {0} not justificated.", memberName);
                            errorCount++;
                        }
                    }
                }
            }

            Assert.Equal(0, errorCount);
        }

        [Fact]
        public void BlazorMapWorksRight()
        {
            var app = new BlazorApplication();
            app.CreateClient();
        }

        private class BlazorModule : AbstractModule
        {
            public override string Area => "Blazor";

            public override void Initialize()
            {
            }

            public override void RegisterServices(IServiceCollection services)
            {
                services.AddServerSideBlazor();
            }

            public override void RegisterEndpoints(IEndpointBuilder endpoints)
            {
                endpoints.MapBlazorHub();
            }
        }

        private class BlazorApplication : SubstrateApplicationBase
        {
            protected override Assembly EntryPointAssembly => typeof(DefaultContext).Assembly;

            protected override IHostBuilder CreateHostBuilder() =>
                Host.CreateDefaultBuilder()
                    .MarkTest(this)
                    .AddModule<BlazorModule>()
                    .ConfigureSubstrateDefaultsCore();
        }
    }
}
