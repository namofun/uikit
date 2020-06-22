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

- /Apis/SomeApiController.cs
- /Controllers/SomeActivityController.cs
- /Dashboards/SomePanelController.cs
- /Views/SomeActivity/SomeAction.cshtml
- /Panels/SomePanel/SomeAction.cshtml
- /Models/SomeModel.cs
- /Services/SomeService.cs

There are several rules to obey.

- The controllers in `Controllers` should `[Area("ThisModuleName")]`
- The controllers in `Dashboards` should `[Area("Dashboard")]`
- The controllers in `Apis` should `[Area("Api")]`