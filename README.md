# UI components

This repository contains the following components:

- Extended Controller functions
- DataTables renderer
- Basic Razor views
- Tag Helpers
- Menu contribution
- Validation attributes
- ClaimsPrincipal helpers



# Application Modules

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

There are several rules to obey.

- The controllers in `Controllers` should `[Area("ThisModuleName")]`
- The controllers in `Dashboards` should `[Area("Dashboard")]`
- The controllers in `Apis` should `[Area("Api")]`