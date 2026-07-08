# Development Rules

- Code identifiers must be English.
- Arabic belongs in resources or UI text only.
- Web components must call Application use cases, not `DbContext`.
- Business rules must not live inside Razor components.
- Infrastructure owns EF Core, SQL Server, Identity, files, notifications, reports, and jobs.
- Every database schema change requires an EF migration.
- Use soft delete for operational data unless physical deletion is explicitly approved.
- New modules must include documentation in `docs/modules`.
- Keep modules independent so future extraction remains possible.
