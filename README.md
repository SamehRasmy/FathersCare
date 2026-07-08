# Fathers Care

Fathers Care is a modular monolith for managing a care facility: residents, medication, nutrition, rooms, staff, finance, reports, permissions, audit, and tenant-ready operations.

## Technology

- ASP.NET Core Blazor Web App
- SQL Server
- EF Core
- ASP.NET Core Identity
- Clean Architecture with a modular monolith structure

The solution is pinned to `.NET 10 SDK` via `global.json` and targets `net10.0`.

## Projects

- `FathersCare.Domain`: business entities and rules only.
- `FathersCare.Application`: use cases, DTOs, and interfaces.
- `FathersCare.Infrastructure`: EF Core, SQL Server, Identity, files, notifications, reports, and background jobs.
- `FathersCare.Web`: Blazor UI only.
- `FathersCare.Shared`: shared primitives/contracts that are safe across boundaries.

## Rules

Read `AGENTS.md` and the files under `docs/` before adding features.
