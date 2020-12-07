# Project Substrate

![](https://img.shields.io/travis/com/namofun/uikit/master) ![](https://img.shields.io/github/license/namofun/uikit) ![](https://img.shields.io/github/languages/code-size/namofun/uikit)

This is a project for ASP.NET Core module design standard,  designed as Satellite Site first. With this module, you can build your module application faster.

- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.Abstraction%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.Abstraction): Commonly used classes and extension methods
- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.DataAccess%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.DataAccess): Several abstractions for Entity Framework Core
- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.Substrate%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.Substrate): ASP.NET Core Library with modern module design
- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.StaticWebAssets%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.StaticWebAssets): Static web assets to use in /wwwroot/lib
- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.IdentityCore%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.IdentityCore): Identity abstractions based on Microsoft.Extensions.Identity.Store
- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.IdentityModule%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.IdentityModule): Default User Identity Module based on IdentityEFCore
- [![](https://img.shields.io/endpoint?url=https%3A%2F%2Fnuget.xylab.fun%2Fv3%2Fpackage%2FSatelliteSite.HostBuilder%2Fshields-io.json)](https://nuget.xylab.fun/packages/SatelliteSite.HostBuilder): Host Builder Extensions for Host projects

## UI components

This repository contains the following components:

- Extended Controller functions
- DataTables renderer
- Basic Razor views
- Tag Helpers
- Menu contribution
- Validation attributes
- ClaimsPrincipal helpers

## Application Modules

This section provide guides to write Application Modules.

- /Models/SomeModel.cs
- /Services/SomeService.cs
- /Apis/SomeApiController.cs
- /Controllers/SomeActivityController.cs
- /Dashboards/SomePanelController.cs
- /Components/SomeComponent/SomeComponentViewComponent.cs
- /Components/SomeComponent/_ViewImports.cshtml
- /Components/SomeComponent/SomeView.cshtml
- /Views/_ViewImports.cshtml
- /Views/SomeActivity/SomeAction.cshtml
- /Panels/SomePanel/_ViewImports.cshtml
- /Panels/SomePanel/SomeAction.cshtml

There are several rules to follow.

- The controllers in `Controllers` should `[Area("ThisModuleName")]`
- The controllers in `Dashboards` should `[Area("Dashboard")]`
- The controllers in `Apis` should `[Area("Api")]`

## Contributor

- [yang-er](https://github.com/yang-er)
