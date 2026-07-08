# Architecture

The solution follows Clean Architecture:

- `Domain` contains entities, enums, domain events, and business concepts.
- `Application` contains use cases, DTOs, and interfaces.
- `Infrastructure` implements external concerns such as EF Core, SQL Server, Identity, files, notifications, reports, and background jobs.
- `Web` contains Blazor UI, routing, components, and user interaction.

Dependency direction:

`Web -> Application -> Domain`

`Infrastructure -> Application + Domain`

Web may depend on Infrastructure for dependency injection composition, but Razor components must not use `DbContext` directly.
