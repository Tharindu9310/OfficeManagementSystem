---
name: nopcommerce-implementation
description: Implement nopCommerce changes - new plugins or existing-code modifications, backend and UI together - using plugin architecture and upgrade-safe patterns
---

# nopCommerce Implementation Skill

## When to Use
- After implementation plan approval
- When implementing the scope of a ticket (Path A modify-existing or Path B new-plugin from the design)
- When creating or modifying nopCommerce services, repositories, migrations, plugin scaffolding, Razor views, widget components, or theme assets

---

## Path A - Modifying Existing Code
1. Read the existing implementation fully - trace Controller -> Service -> Repository (and, for view changes, the rendering chain) before changing anything.
2. Apply the decision order: can this be an event consumer / DI override / scheduled task / view override instead of a core edit? Only fall through to a genuine core edit if none of those fit.
3. Any schema change -> proper migration, never hand-edited DB.
4. Any new/changed string -> `ILocalizationService`, never hardcoded.
5. If touching core (backend or theme): produce a Core Modification Notice (file, reason, version, upgrade-check note).

## Path B - Building a New Plugin (backend + UI)
1. Scaffold: `Nop.Plugin.{Group}.{Name}`, `plugin.json` fully populated.
2. Plugin class implementing `IPlugin` (or the specific provider interface) with symmetrical `Install()`/`Uninstall()`.
3. `DependencyRegistrar`/`INopStartup` registration.
4. Migrations for any new tables/settings.
5. Admin controller + views for configuration, gated by a dedicated `PermissionRecord`, following nopCommerce's existing grid/tab conventions.
6. Public controller/`ViewComponent` for storefront rendering, targeting the widget zone(s) named in the design - never editing core theme files directly.
7. Models scoped to their view - never a core entity reused as a view model.
8. Client assets (CSS/JS) added through the active theme's existing bundling setup - no new frontend framework without explicit instruction.
9. Localization resource file(s) for every user-facing string, admin and storefront.

## Rules
- No new ORM, DI container, MVC convention, or frontend framework outside what nopCommerce already uses.
- Async/await for all service and repository calls.
- ACL/permission checks respected on every admin screen.
- Settings via `ISettings`-derived classes, not custom config files.
- Accessibility (keyboard nav, alt text, contrast) and multi-store behavior checked on any new UI before marking done.

## Outputs
- Modified files (Path A) or full new plugin project including UI (Path B), matching the task's `task-type` tag.
- Core Modification Notice where applicable.
- Localization resource additions for every new string.

## Definition of Done
- Correct path (A or B) applied per the task's tag.
- Migrations used for all schema changes.
- Plugin lifecycle symmetrical for new plugins.
- UI renders through the widget zone/view component named in the design - no ad hoc routes or core theme edits.
- No architectural or frontend pattern introduced outside nopCommerce convention.
