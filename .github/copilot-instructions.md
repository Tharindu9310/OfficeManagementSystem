# Copilot Instructions (nopCommerce Edition)

## General Rules
- Do NOT assume missing requirements.
- Ask clarifying questions if requirements are unclear, including the target nopCommerce version.
- Do NOT modify unrelated code.
- Only work within the given scope.
- Follow existing project patterns and conventions — nopCommerce's own architecture, not generic ASP.NET Core assumptions.

## Coding Expectations
- Write clean, maintainable, production-ready code.
- Avoid hardcoding values — including user-facing strings (use `ILocalizationService`/locale resources).
- Ensure proper validation and error handling.
- Prefer plugin-based extension over core modification — see Placement Decision below.

## Tech Stack
- Platform: nopCommerce (source-available ASP.NET Core e-commerce platform)
- Stable line: 4.90.x on .NET 8 | Develop line: 5.00 on .NET 9 — confirm target version before writing code, plugin APIs differ between them
- Backend: ASP.NET Core MVC, EF Core / Linq2DB-backed data access (`INopDataProvider`), nopCommerce's own DI (`INopStartup`/`IDependencyRegistrar`)
- Frontend: Server-rendered Razor views + nopCommerce theme engine — no SPA framework unless the store has deliberately added one
- Admin: ASP.NET Core MVC Areas/Admin, Kendo UI grids

## Placement Decision (apply before writing any code)
Every task is classified as one of:
1. **New plugin** — self-contained feature (`Nop.Plugin.{Group}.{Name}`)
2. **Extension of an existing plugin**
3. **Plugin-based override of core behavior** — via `IConsumer<T>` event, DI override, scheduled task, or view override — no core files touched
4. **Genuine core modification** — only when no plugin-based alternative exists; requires an explicit Core Modification Notice (file, reason, version, upgrade-check note) since core is overwritten on every upstream upgrade

Default assumption is (1)–(3). Never jump to (4) without first ruling out the others.

## Guardrails (Mandatory Rules)

### Security
- NEVER expose sensitive data in responses or logs.
- ALWAYS validate and sanitize user input; validate `ModelState` first in POST/PUT actions.
- MUST use `IRepository<T>` / parameterized EF Core queries — never string-concatenated SQL.
- Admin actions MUST use `[AuthorizeAdmin]` + CSRF token attribute; gate features with `IPermissionService` against a dedicated `PermissionRecord`.
- NEVER commit secrets, API keys, or credentials — payment/shipping/tax credentials go through an `ISettings`-derived settings class, never a bundled config file.

### Architecture Compliance
- MUST follow nopCommerce's layering: Controller → Service (`I{X}Service`) → Repository (`IRepository<T>`).
- MUST use async/await for all I/O operations.
- MUST respect layer boundaries — no direct DB access from controllers, no business logic in Razor views.
- Storefront UI renders through `ViewComponent`s registered against named widget zones — no ad hoc routes or core theme edits.
- No new ORM, DI container, or MVC convention outside what nopCommerce already uses.

### Code Quality Gates
- MUST include error handling for all external calls (payment gateways, shipping carriers, third-party services).
- MUST add XML documentation for public APIs.
- MUST write xUnit tests for business logic.
- NEVER suppress warnings without justification.

### Plugin Lifecycle
- `Install()`/`Uninstall()` MUST be symmetrical — anything registered (settings, permissions, locale resources, scheduled tasks) MUST be cleaned up on uninstall.
- `plugin.json` MUST be fully populated (`Group`, `FriendlyName`, `SystemName`, `Version`, `SupportedVersions`, `Author`, `DisplayOrder`, `FileName`, `Description`).

### Database
- MUST use versioned migrations (`Migration`/`AutoReversingMigration` + migration attribute) for all schema changes — never hand-edit the database or ship loose SQL scripts.

### Performance
- MUST use pagination for admin/storefront list endpoints (max 100 items).
- MUST respect existing `IStaticCacheManager` caching and invalidate correct cache keys on data changes.
- MUST avoid N+1 queries.

### Breaking Changes Prevention
- NEVER remove or rename an existing public service interface method without a migration plan.
- NEVER modify database schema without a migration.
- NEVER change an existing plugin's public settings/permission contract without documenting the impact for stores already running it.

### Accessibility (Frontend)
- MUST comply with WCAG 2.1 Level AA on storefront UI (and admin where practical).
- MUST ensure keyboard navigation for all interactive elements, including widget-zone content.
- MUST provide proper ARIA labels / `alt` text (real product names, not filenames).
- MUST maintain minimum 4.5:1 text contrast ratio, consistent with the active theme.

## Detailed Standards Reference
For comprehensive framework-specific standards, see:

**Core Standards (Mandatory):**
- **Security**: `.github/instructions/project-standards/nopcommerce-security-standards.instruction.md` (CRITICAL)
- **Error Handling**: `.github/instructions/project-standards/nopcommerce-error-handling-standards.instruction.md` (ALL code)
- **Database**: `.github/instructions/project-standards/nopcommerce-database-standards.instruction.md` (Backend)
- **Accessibility**: `.github/instructions/project-standards/nopcommerce-accessibility-standards.instruction.md` (Frontend)

**Framework-Specific:**
- **Backend/Plugin**: `.github/instructions/backend/nopcommerce-backend.instruction.md`
- **Frontend/Theme**: `.github/instructions/frontend/nopcommerce-frontend.instruction.md`
- **Story Format**: `.github/instructions/nopcommerce-story-format.md`
- **Ticket Manager Behavior**: `.github/instructions/ticket-manager/nopcommerce-ticket-manager.instruction.md`

**Workflow & Agents:**
- **Master Orchestrator**: `.github/workflows/nopcommerce-workflow-orchestrator.md`
- **Agents**: `.github/agents/` (BA, SA, Docs, Ticket Manager, Planner, Dev, Frontend Dev, QA, Validator, UX — see `.github/agents/README.md`)
- **Skills**: `.github/skills/` (deeper how-to behind each agent)

## Required Input Format
All feature work through the orchestrator requires both a requirement and the target version:
```
requirement={your requirement text} nopcommerce-version={e.g. 4.90.8}
```
