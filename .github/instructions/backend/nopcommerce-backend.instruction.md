# Backend Extension - nopCommerce/.NET Standards

## Purpose
Framework-specific standards for nopCommerce backend development (plugins and, exceptionally, core).

**Works with:** `project-standards/security-standards.instruction.md` (MANDATORY), `error-handling-standards.instruction.md`, `database-standards.instruction.md`

---

## 1. Technology Stack

**.NET 8** (stable 4.90.x line) or **.NET 9** (5.00 develop line) | **ASP.NET Core MVC** | **Entity Framework Core (Linq2DB-backed data layer)** | **nopCommerce's own DI (`INopStartup`/`IDependencyRegistrar`)** | **xUnit**

Confirm which line (4.90.x vs. 5.00) before writing code — plugin interfaces differ.

**Solution structure (reference points, not to be edited unless task is a genuine core change):**
- `Nop.Core` - domain entities, caching abstractions, infrastructure
- `Nop.Data` - repository implementation, migrations, `INopDataProvider`
- `Nop.Services` - business logic services, consumed via interfaces
- `Nop.Web` / `Nop.Web.Framework` - MVC infrastructure, admin area, base controllers/models
- `Plugins/Nop.Plugin.{Group}.{Name}` - where all new plugin work lives

## 2. Placement (inherited from technical design)
Every backend task carries a placement tag from `nopcommerce-technical-designer`: new plugin, plugin extension, plugin-based core override, or genuine core modification. Apply the corresponding rule set from the `nopcommerce-developer` agent definition — do not deviate based on convenience.

## 3. Services & Dependency Injection
- Business logic lives in `I{X}Service` interfaces + implementations, never in controllers.
- Register plugin services via `INopStartup.Configure(IServiceCollection)` (current versions) or `IDependencyRegistrar` (legacy) — match whichever the target version uses.
- Overriding a core service: register the plugin's implementation against the same interface; nopCommerce's DI resolves the last registration, so no core file edit is needed.
- All I/O-bound service methods are `async`/`await` — no `.Result` or `.Wait()` blocking calls.

## 4. Data Access
- All access through `IRepository<T>` — no raw ADO.NET, no new `DbContext`/`INopDataProvider` unless the task explicitly requires a new data source.
- Schema changes ship as versioned migrations (`AutoReversingMigration`/`Migration` + `MigrationVersion` attribute) under the plugin's `Migrations/` folder — never hand-edit the database or ship SQL scripts outside the migration system.
- New settings go through an `ISettings`-derived class, persisted via `ISettingService` — not a custom config file.

## 5. Events & Extension Points
- React to core behavior via `IConsumer<T>` against nopCommerce's built-in domain events (`EntityInsertedEvent<T>`, `EntityUpdatedEvent<T>`, `OrderPlacedEvent`, `OrderPaidEvent`, etc.) rather than modifying the core code that would otherwise raise the behavior.
- Background/periodic work uses `IScheduleTask`, registered via a `ScheduleTask` record inserted during plugin install.

## 6. Admin Controllers
- Inherit from `BaseAdminController`; use `[AuthorizeAdmin]` + `[AutoValidateAntiforgeryToken]` (or the version-appropriate equivalents) on every admin action that mutates data.
- Gate access with `IPermissionService` checks against a `PermissionRecord` the plugin registers on install.
- Use nopCommerce's model/view-model conventions (`BaseNopModel`, paged list models) rather than ad hoc DTOs.

## 7. Testing
- xUnit for service-layer unit tests.
- Mock `IRepository<T>` / service dependencies — don't hit a real database in unit tests.
- For plugin lifecycle, verify `Install()`/`Uninstall()` symmetry (everything registered is also cleaned up).

## 8. Core Modification (exception path only)
If Step 1 of the design concluded a genuine core change is unavoidable:
- Smallest possible diff.
- Ship a Core Modification Notice: file(s) touched, reason, nopCommerce version applied against, and what to check on the next upstream upgrade.
