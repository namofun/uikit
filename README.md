# Project Substrate

Build Status: ![](https://api.travis-ci.org/namofun/uikit.svg?branch=master)

This is a project for ASP.NET Core module design standard,  designed as Satellite Site first. With this module, you can build your module application faster.

- [SatelliteSite.Abstraction](https://nuget.xylab.fun/packages/SatelliteSite.Abstraction): Commonly used classes and extension methods
- [SatelliteSite.DataAccess](https://nuget.xylab.fun/packages/SatelliteSite.DataAccess): Several abstractions for Entity Framework Core
- [SatelliteSite.Substrate](https://nuget.xylab.fun/packages/SatelliteSite.Substrate): ASP.NET Core Library with modern module design
- [SatelliteSite.IdentityModule](https://nuget.xylab.fun/packages/SatelliteSite.IdentityModule): Default User Identity Module based on IdentityEFCore

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
