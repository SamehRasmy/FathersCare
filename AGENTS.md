# Fathers Care Agent Guide

Before editing any code, read:
1. docs/00-project-brief.md
2. docs/01-architecture.md
3. docs/02-modules-map.md
4. docs/05-development-rules.md

Rules:
- Code names must be English.
- Arabic text must be stored in resource files or UI only.
- Do not access DbContext from Web project.
- Do not put business rules inside Razor components.
- Every feature must go through Application layer.
- Every database change must include EF migration.
- Every new module must include documentation under docs/modules.
- Every important operation must create an AuditLog.
- Do not delete data physically; use soft delete unless explicitly approved.
- Follow existing DesignSystem components.
