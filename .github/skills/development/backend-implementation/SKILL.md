---
name: nopcommerce-backend-implementation
description: Implement nopCommerce backend changes - new plugins or existing-code modifications - using Clean/plugin architecture and upgrade-safe patterns
---

# nopCommerce Backend Implementation Skill

## When to Use
- After implementation plan approval
- When implementing backend scope of user stories (Path A modify-existing or Path B new-plugin from the design)
- Before frontend/theme implementation
- When creating or modifying nopCommerce services, repositories, migrations, or plugin scaffolding

---

## Path A - Modifying Existing Code
1. Read the existing implementation fully - trace Controller -> Service -> Repository before changing anything.
2. Apply the decision order: can this be an event consumer / DI override / scheduled task / view override instead of a core edit? Only fall through to a genuine core edit if none of those fit.
3. Any schema change -> proper migration, never hand-edited DB.
4. Any new/changed string -> `ILocalizationService`, never hardcoded.
5. If touching core: produce a Core Modification Notice (file, reason, version, upgrade-check note).

## Path B - Building a New Plugin
1. Scaffold: `Nop.Plugin.{Group}.{Name}`, `plugin.json` fully populated.
2. Plugin class implementing `IPlugin` (or the specific provider interface) with symmetrical `Install()`/`Uninstall()`.
3. `DependencyRegistrar`/`INopStartup` registration.
4. Admin controller + views for configuration, gated by a dedicated `PermissionRecord`.
5. Public controller/`ViewComponent` for storefront rendering, targeting the widget zone(s) named in the design.
6. Models scoped to their view - never a core entity reused as a view model.
7. Migrations for any new tables/settings.
8. Localization resource file(s) for every user-facing string.

## Rules
- No new ORM, DI container, or MVC convention outside what nopCommerce already uses.
- Async/await for all service and repository calls.
- ACL/permission checks respected on every admin screen.
- Settings via `ISettings`-derived classes, not custom config files.

## Outputs
- Modified files (Path A) or full new plugin project (Path B), matching the task's `task-type` tag.
- Core Modification Notice where applicable.

## Definition of Done
- Correct path (A or B) applied per the task's tag.
- Migrations used for all schema changes.
- Plugin lifecycle symmetrical for new plugins.
- No architectural pattern introduced outside nopCommerce convention.
