# CLAUDE.md — nopCommerce Project Instructions

This file is read automatically by Claude Code at the start of every session in this repo.

## General Rules
- Do NOT assume missing requirements.
- Ask clarifying questions if requirements are unclear, including the target nopCommerce version.
- Do NOT modify unrelated code.
- Only work within the given scope.
- Follow existing project patterns and conventions - nopCommerce's own architecture, not generic ASP.NET Core assumptions.
- **File Creation Rule (MANDATORY):** Any file a subagent produces (documents or code) MUST actually be written to disk with the Write/Edit tool at the stated path relative to the repo root. Returning file content as chat text only does not save it to the project. Re-read the file after writing to confirm it landed before reporting a step complete.
- **Permission scoping note:** `.claude/settings.local.json` grants `Bash(mkdir -p *)` and `additionalDirectories` covering the whole `docs/` tree, not just one feature's folder - this was fixed after an earlier bug where only `docs/time-log`'s exact mkdir command was pre-approved, silently blocking phase 2's folder creation since it needed a different path. If you ever see permission approval prompts for something under `docs/`, that's unexpected given this config - flag it rather than assuming it's normal.

## Coding Expectations
- Write clean, maintainable, production-ready code.
- Avoid hardcoding values - including user-facing strings (use `ILocalizationService`/locale resources).
- Ensure proper validation and error handling.
- Prefer plugin-based extension over core modification - see Placement Decision below.

## Tech Stack
- Platform: nopCommerce (source-available ASP.NET Core e-commerce platform)
- Stable line: 4.90.x on .NET 8 | Develop line: 5.00 on .NET 9 - confirm target version before writing code, plugin APIs differ between them
- Backend: ASP.NET Core MVC, EF Core / Linq2DB-backed data access (`INopDataProvider`), nopCommerce's own DI (`INopStartup`/`IDependencyRegistrar`)
- Frontend: Server-rendered Razor views + nopCommerce theme engine - no SPA framework unless the store has deliberately added one
- Admin: ASP.NET Core MVC Areas/Admin, Kendo UI grids

## Placement Decision (apply before writing any code)
Every task is classified as one of:
1. **New plugin** - self-contained feature (`Nop.Plugin.{Group}.{Name}`)
2. **Extension of an existing plugin**
3. **Plugin-based override of core behavior** - via `IConsumer<T>` event, DI override, scheduled task, or view override - no core files touched
4. **Genuine core modification** - only when no plugin-based alternative exists; requires an explicit Core Modification Notice (file, reason, version, upgrade-check note) since core is overwritten on every upstream upgrade

Default assumption is (1)-(3). Never jump to (4) without first ruling out the others.

## Guardrails (Mandatory Rules)

### Security
- NEVER expose sensitive data in responses or logs.
- ALWAYS validate and sanitize user input; validate `ModelState` first in POST/PUT actions.
- MUST use `IRepository<T>` / parameterized EF Core queries - never string-concatenated SQL.
- Admin actions MUST use `[AuthorizeAdmin]` + CSRF token attribute; gate features with `IPermissionService` against a dedicated `PermissionRecord`.
- NEVER commit secrets, API keys, or credentials - payment/shipping/tax credentials go through an `ISettings`-derived settings class, never a bundled config file.

### Architecture Compliance
- MUST follow nopCommerce's layering: Controller -> Service (`I{X}Service`) -> Repository (`IRepository<T>`).
- MUST use async/await for all I/O operations.
- MUST respect layer boundaries - no direct DB access from controllers, no business logic in Razor views.
- Storefront UI renders through `ViewComponent`s registered against named widget zones - no ad hoc routes or core theme edits.
- No new ORM, DI container, or MVC convention outside what nopCommerce already uses.

### Code Quality Gates
- MUST include error handling for all external calls (payment gateways, shipping carriers, third-party services).
- MUST add XML documentation for public APIs.
- MUST write xUnit tests for business logic.
- NEVER suppress warnings without justification.

### Plugin Lifecycle
- `Install()`/`Uninstall()` MUST be symmetrical - anything registered (settings, permissions, locale resources, scheduled tasks) MUST be cleaned up on uninstall.
- `plugin.json` MUST be fully populated (`Group`, `FriendlyName`, `SystemName`, `Version`, `SupportedVersions`, `Author`, `DisplayOrder`, `FileName`, `Description`).

### Database
- MUST use versioned migrations (`Migration`/`AutoReversingMigration` + migration attribute) for all schema changes - never hand-edit the database or ship loose SQL scripts.

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

## How This Project Uses Claude Code

**Subagents** (`.claude/agents/`) - 6 specialized nopCommerce personas, streamlined from an earlier 10-agent set (see `.claude/agents/README.md` for what was cut and why):
- `nopcommerce-requirement-analyzer` (BA)
- `nopcommerce-technical-designer` (SA - placement decision + design)
- `nopcommerce-ticket-manager` (creates/maintains `tickets.md`)
- `nopcommerce-implementation-planner` (task breakdown)
- `nopcommerce-developer` (backend AND UI implementation - merged role)
- `nopcommerce-qa-tester` (testing + bug-fix loop)

Claude delegates to these automatically based on their `description`, or you can invoke one directly: `@nopcommerce-technical-designer design the loyalty points feature`.

**Skills** (`.claude/skills/`) - deeper how-to playbooks behind each subagent's work. Loaded automatically when relevant.

**The shared ticket file** - `docs/{feature-name}/tickets/tickets.md` is the primary work-item log for a feature, created by `nopcommerce-ticket-manager` and read/updated by every subagent from the implementation planner onward. Jira sync (via the `mcp-atlassian` MCP server) is optional and additive, not required.

**Intake files** - three templates in `docs/intake/` (see `docs/intake/README.md`), checked in this order when you run `/nopcommerce-workflow` with no arguments:
- `requirement.md` - new feature, or a new phase of an existing one (full pipeline, creates a new `docs/{feature-name}/` folder)
- `bug.md` - something already built is broken (lightweight: one new Bug ticket added to the existing feature folder's `tickets.md`, no new folder, no new design cycle)
- `change.md` - something already built should behave differently (same lightweight path as a bug, for deliberate scoped changes rather than defects)

**Multi-phase features** - phase folders are named `{base-feature-name}-phaseN` (suffix form, e.g. `time-log-phase2`), matching this project's actual convention. The BA agent detects a phase from context (title, "builds on" language, an existing matching `docs/` folder) - you don't strictly need to fill in the intake file's `## Phase` field, though doing so removes ambiguity. Each phase reads the immediately preceding phase's folder before writing anything, and documents Do NOT restate/contradict what a prior phase already settled.

**Bugs and changes stay inside their target folder** - `bug.md`/`change.md` both require a `## Target Feature Folder` naming an EXISTING `docs/` folder. Neither creates a new phase - the resulting ticket lands in that folder's `tickets.md` (continuing its existing ticket numbering), and `nopcommerce-developer`/`nopcommerce-qa-tester` pick it up like any other ticket via `ticket-id`. This skips technical design and implementation planning entirely, since the plugin/design those would produce already exists.

**Full pipeline** - run `/nopcommerce-workflow` to drive the canonical BA -> SA -> Tickets -> Plan -> Dev -> QA sequence with gates (requirement/phase path), or the shorter Bug/Change sequence (bug/change path). No dedicated validation-gate subagent in this streamlined roster - do a quick self-check against each agent's own Definition of Done instead.

**MCP** - `mcp-atlassian` (Jira, and Confluence if you enable it) is configured in `.mcp.json`, run via `uvx mcp-atlassian`. Requires `uv` installed (`irm https://astral.sh/uv/install.ps1 | iex` on Windows) and three env vars in `.mcp.json`'s `env` block: `JIRA_URL`, `JIRA_USERNAME`, `JIRA_API_TOKEN` (generate the token at https://id.atlassian.com/manage-profile/security/api-tokens). Verify with `claude mcp list` - should show `mcp-atlassian` as `✔ Connected`. If you ever edit `.mcp.json`, you must fully restart the Claude Code session (`/exit` then `claude` again) - MCP servers load once at session start and won't pick up mid-session config changes.

## Detailed Standards Reference

**Core Standards (Mandatory):**
- **Security**: `.claude/instructions/project-standards/nopcommerce-security-standards.instruction.md` (CRITICAL)
- **Error Handling**: `.claude/instructions/project-standards/nopcommerce-error-handling-standards.instruction.md` (ALL code)
- **Database**: `.claude/instructions/project-standards/nopcommerce-database-standards.instruction.md` (Backend)
- **Accessibility**: `.claude/instructions/project-standards/nopcommerce-accessibility-standards.instruction.md` (Frontend)

**Framework-Specific:**
- **Backend/Plugin**: `.claude/instructions/backend/nopcommerce-backend.instruction.md`
- **Frontend/Theme**: `.claude/instructions/frontend/nopcommerce-frontend.instruction.md`
- **Story Format**: `.claude/instructions/nopcommerce-story-format.md`
- **Ticket Manager Behavior**: `.claude/instructions/ticket-manager/nopcommerce-ticket-manager.instruction.md`
- **QA Procedures**: `.claude/instructions/qa/`

## Required Input Format
When running the full pipeline (`/nopcommerce-workflow`), provide both a requirement and the target version either inline:
```
requirement={your requirement text} nopcommerce-version={e.g. 4.90.8}
```
or by editing `docs/intake/requirement.md` and running `/nopcommerce-workflow` with no arguments - see `docs/intake/README.md` for the exact format.
