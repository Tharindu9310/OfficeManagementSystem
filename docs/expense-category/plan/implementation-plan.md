# Implementation Plan - Expense Category (SCRUM-38)

**Traces to:** `docs/expense-category/design/technical-design.md`, `docs/expense-category/tickets/tickets.md` (T-001..T-005, tasks TT-001..TT-029), `docs/expense-category/acceptance-criteria/acceptance-criteria.md`
**Target nopCommerce version:** 4.90.8 for every task. Repo builds `net9.0` (`src/Directory.Build.props`); see problem P3.
**Placement:** 100% new plugin `Nop.Plugin.Misc.Expenses`. Every task is tagged `new-plugin`. There are no `modify-existing` tasks and **no core-modification tasks**, so no Core Modification Notice is required and nothing is flagged CORE.

## Problems found while checking tickets.md against the design and acceptance criteria

All five tickets exist, the Index matches the sections, every ticket has all required fields, Status is `Backlog` everywhere, and every AC in `acceptance-criteria.md` (AC1.1-AC5.10) is carried by exactly one ticket. Tickets and the AC file agree with each other. Issues:

- **P1 Ticket dependencies are circular on paper.** T-001 depends on T-002 and owns the plugin scaffold, but T-002 says "None" (and needs the scaffold). T-003 depends on T-001 (page reachable from menu), but T-001's AC1.2 needs the List page that T-003 builds. Resolved at task level: scaffold (TT-001) first, menu (TT-011) after permission (TT-009), List action/view live in T-003 (TT-013/TT-014), lifecycle wiring (T-002) last. The ticket-level text was not edited.
- **P2 The technical design is stale in its traceability.** Its header and section 8 cover only Stories 1-4 and AC1.x-AC4.x. Section 8.1 does not trace AC2.6, AC3.7-AC3.10 or AC5.1-AC5.10, and section 8.2 numbers the modal criteria as AC4.13/AC4.14 whereas the final AC file uses AC4.13 (default all stores), AC4.14 (no stores in the Add panel) and Story 5 for the modal. The AC file and tickets are authoritative; the design body (sections 3-5) already covers the modal, so no task is affected. Recommend the designer refresh section 8.
- **P3 Framework version mismatch.** Design section 1 and CLAUDE.md say .NET 8 for 4.90.x; `src/Directory.Build.props` sets `net9.0` and the time-log tests project is `net9.0`. Plan follows the repo (inherit, do not hard-code a TFM).
- **P4 Stores column wording.** Design 5.2 says the column shows "Limited"; design 5.6 and AC3.10 use "Limited to selected stores". Plan uses the locale key `Admin.Expenses...List.Stores.Limited`.
- **P5 Store filter caveat.** In 4.90.8 `ApplyStoreMapping<T>` is async and returns the query unfiltered when `CatalogSettings.IgnoreStoreLimitations` is true or when no StoreMapping rows exist for the entity type. AC3.8/AC5.4 could only be verified with that setting off; recorded in TT-006 and TT-014. Superseded by BUG-010: the service now uses its own StoreMapping query and the filter works with the setting on or off.
- **P6 AC2.1 timing.** The permission record and Administrators mapping are only created on the next application start (design 5.4), so QA must restart before checking AC2.1. Recorded in TT-009.
- **P7 Minor.** The existing section bodies in tickets.md have no `---` separators (time-log's do); left unchanged per instruction. Also all five tickets share Jira SCRUM-38 (already noted in the file header).

## Build order

```
Stage 0  TT-001 scaffold -> TT-002 test project
Stage 1  TT-003 entity/enum -> TT-004 builder -> TT-005 migration
Stage 2  TT-006 service -> TT-007 DI -> TT-008 service tests
         TT-009 permission, TT-010 locale (parallel with service work; both need only TT-001)
Stage 3  TT-011 menu (TT-009, TT-010) | TT-012 models | TT-013 controller read path | TT-016 validator -> TT-019 validator tests
Stage 4  TT-014 List view + read-only grid | TT-015 list tests | TT-021 modal models
Stage 5  TT-017 insert/update -> TT-018 Add panel + inline edit js | TT-020 edit tests
         TT-022 modal actions -> TT-024 modal tests
Stage 6  TT-023 modal partial + js
Stage 7  TT-025 install/uninstall -> TT-026 data cleanup | TT-027 permission tests | TT-028 lifecycle tests
Stage 8  TT-029 build and smoke gate
```

Rule respected: entities/migration -> services + DI -> permission/menu/admin UI -> install/uninstall wiring last. Controller actions are added to one class, so TT-013, TT-017 and TT-022 must be done in that order by a single developer or merged carefully. Storefront work: none (admin only).

## Task list by stage

### Stage 0 - Scaffold

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-001 Plugin project scaffold | `new-plugin` | T-001 | None |
| TT-002 xUnit test project scaffold | `new-plugin` | T-001 | TT-001 |

### Stage 1 - Entity, mapping and migration

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-003 ExpenseCategory entity, type enum and defaults | `new-plugin` | T-003 | TT-001 |
| TT-004 ExpenseCategoryBuilder and table naming | `new-plugin` | T-003 | TT-003 |
| TT-005 SchemaMigration (installation) | `new-plugin` | T-003 | TT-004 |

### Stage 2 - Services, DI, permission, locale

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-006 IExpenseCategoryService and ExpenseCategoryService | `new-plugin` | T-003 | TT-003, TT-004, TT-005 (runtime) |
| TT-007 NopStartup DI registration | `new-plugin` | T-003 | TT-006 |
| TT-008 xUnit: ExpenseCategoryService tests | `new-plugin` | T-003 | TT-006, TT-002 |
| TT-009 PermissionProvider (IPermissionConfigManager) | `new-plugin` | T-002 | TT-001 |
| TT-010 Locale resource inventory | `new-plugin` | T-001 | TT-001 |

### Stage 3 - Menu, models, controller read path, validator

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-011 AdminMenuManager (Expenses > Expense Category) | `new-plugin` | T-001 | TT-009, TT-010 |
| TT-012 Search, list and row models | `new-plugin` | T-003 | TT-003, TT-006 |
| TT-013 ExpenseCategoryController: List and ExpenseCategoryList | `new-plugin` | T-003 | TT-006, TT-007, TT-009, TT-010, TT-012 |
| TT-016 ExpenseCategoryValidator | `new-plugin` | T-004 | TT-003, TT-010 |
| TT-019 xUnit: ExpenseCategoryValidator tests | `new-plugin` | T-004 | TT-016, TT-002 |

### Stage 4 - Read-only grid UI, controller read tests, modal models

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-014 List view: search panel and read-only grid | `new-plugin` | T-003 | TT-013 |
| TT-015 xUnit: controller list tests | `new-plugin` | T-003 | TT-013, TT-002 |
| TT-017 Controller: ExpenseCategoryInsert and ExpenseCategoryUpdate | `new-plugin` | T-004 | TT-013, TT-016 |
| TT-021 Modal models (stores and translations) | `new-plugin` | T-005 | TT-003, TT-012 |

### Stage 5 - Insert/update/modal controller actions, Add panel, tests

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-018 Add panel, inline edit and expensecategory-grid.js | `new-plugin` | T-004 | TT-014, TT-017 |
| TT-020 xUnit: controller insert/update tests | `new-plugin` | T-004 | TT-017, TT-002 |
| TT-022 Controller: EditStoresAndTranslations and SaveStoresAndTranslations | `new-plugin` | T-005 | TT-013, TT-016, TT-021 |
| TT-024 xUnit: modal save tests | `new-plugin` | T-005 | TT-022, TT-002 |

### Stage 6 - Modal UI

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-023 Modal partial view and modal script | `new-plugin` | T-005 | TT-022, TT-018 |

### Stage 7 - Install/uninstall wiring and lifecycle/permission tests

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-025 Plugin Install/Uninstall wiring | `new-plugin` | T-002 | TT-009, TT-010, TT-011 |
| TT-026 Uninstall cleanup of StoreMapping and LocalizedProperty rows | `new-plugin` | T-002 | TT-003, TT-025 |
| TT-027 xUnit: permission coverage tests | `new-plugin` | T-002 | TT-009, TT-013, TT-017, TT-022, TT-002 |
| TT-028 xUnit: plugin lifecycle symmetry tests | `new-plugin` | T-002 | TT-025, TT-026, TT-002 |

### Stage 8 - Build and smoke gate

| Task | Type tag | Parent | Depends on |
|---|---|---|---|
| TT-029 Build and install/uninstall smoke verification | `new-plugin` | T-002 | TT-001 to TT-028 |

## Task details

Full description, verification and notes for each task are in `tickets.md` (single source of truth). Summary of verification and version notes:

- **TT-001 Plugin project scaffold** (parent T-001, design Design 1, 7): done when solution builds with the new project; plugin DLL and plugin.json land in `Plugins\Misc.Expenses`; plugin appears in Admin > Plugins as `Expenses`; every plugin.json field populated. Version/risk note: First item to sequence (T-001 owns the scaffold, but T-002 tasks also need it, so it precedes everything). No core files touched.
- **TT-002 xUnit test project scaffold** (parent T-001, design Design 7): done when `dotnet test` on the new project runs (0 tests) and passes; project resolves the plugin reference. Version/risk note: See problem P3 in the plan: design and CLAUDE.md say .NET 8, the repo builds net9.0.
- **TT-003 ExpenseCategory entity, type enum and defaults** (parent T-003, design Design 3.1): done when compiles; enum values exactly 1 and 2 (unset 0 is invalid); `[NotMapped]` property mirrors `Project.Status` pattern; XML docs on public members. Version/risk note: Starts the entity-first chain; nothing else compiles without it.
- **TT-004 ExpenseCategoryBuilder and table naming** (parent T-003, design Design 3.3): done when builder discovered by the framework; resulting table has the specified columns, types and nullability; no unique index on Name. Version/risk note: Developer-confirmation item from design 3.3 (table-name mechanism).
- **TT-005 SchemaMigration (installation)** (parent T-003, design Design 3.3): done when plugin install creates the table with all columns including `LimitedToStores`; plugin uninstall drops it (auto-reversed `Down()`); re-install works. Version/risk note: Requires a SQL Server dev database to verify at runtime.
- **TT-006 IExpenseCategoryService and ExpenseCategoryService** (parent T-003, design Design 4.1, 4.4): done when paging clamps to 100; name and store filters work alone and combined; ordering stable; timestamps set; cache prefix invalidated on insert/update; no delete method exists. Version/risk note: 4.90.8 `ApplyStoreMapping<T>` is async (returns `Task<IQueryable<T>>`) and is a no-op when `CatalogSettings.IgnoreStoreLimitations` is true or no StoreMapping rows exist for the entity; AC3.8 testing must run with that setting false.
- **TT-007 NopStartup DI registration** (parent T-003, design Design 4.1): done when app starts with the plugin installed; service resolves in the controller; validator not double-registered. Version/risk note: `INopStartup` is the 4.90 pattern (same as time-log).
- **TT-008 xUnit: ExpenseCategoryService tests** (parent T-003, design Design 7; AC3.2, AC3.6-3.9): done when all tests pass; every service branch (name, store, both, none, clamp) has at least one test. Version/risk note: Business-logic xUnit requirement from CLAUDE.md.
- **TT-009 PermissionProvider (IPermissionConfigManager)** (parent T-002, design Design 5.4): done when after first application start with the plugin installed, one new permission is listed on the role permission screen and mapped to Administrators. Version/risk note: The framework inserts the record and mapping on the next application start, so AC2.1 can be verified only after a restart.
- **TT-010 Locale resource inventory** (parent T-001, design Design 5.6; AC1.3): done when after install every key resolves via `ILocalizationService`; no hard-coded user-facing text anywhere in plugin code or views; no `NameDuplicate` key exists. Version/risk note: Done early because the validator, controller, menu and views all reference these keys.
- **TT-011 AdminMenuManager (Expenses > Expense Category)** (parent T-001, design Design 5.1): done when permitted user sees Expenses > Expense Category and it opens the List page; user without the permission does not see Expenses; labels come from `Admin.Expenses.Menu.*`; menu disappears when the plugin is uninstalled. Version/risk note: Link target works fully only after TT-013/TT-014 exist. Confirm `AdminMenuCreatedEvent` on first build (design 2.4).
- **TT-012 Search, list and row models** (parent T-003, design Design 4.6 (Models)): done when compile; display names resolve to locale keys.
- **TT-013 ExpenseCategoryController: List and ExpenseCategoryList** (parent T-003, design Design 4.6, 5.2): done when grid read returns paged JSON; page size never exceeds 100; null limit gives an empty display; limit formatted in primary currency; empty table returns an empty page without error. Version/risk note: No delete action exists on this controller.
- **TT-014 List view: search panel and read-only grid** (parent T-003, design Design 5.2 (1, 3)): done when page renders with search panel, grid and the four columns; Name and Store filters narrow the grid alone and combined; Stores cell reads "All stores" or the limited text; no delete UI; page loads from the menu link. Version/risk note: Stores cell text uses the locale key, not the shorter word in design 5.2 (problem P4). Needs demo data: several categories with different names, and at least one second store for filter testing. Add panel placeholder is added by TT-018.
- **TT-015 xUnit: controller list tests** (parent T-003, design Design 7; AC3.2, AC3.4, AC3.5): done when all tests pass. Version/risk note: Mock `ICurrencyService`, `IPriceFormatter`, `IExpenseCategoryService`.
- **TT-016 ExpenseCategoryValidator** (parent T-004, design Design 4.1; AC4.3, AC4.7, AC4.8, AC4.11, AC4.12): done when blank, whitespace, 201-char name, type 0, and negative limit fail with the localized message; empty limit, 0, positive decimal, 200-char name and duplicate name pass. Version/risk note: Auto-registered by assembly scan; do not register in DI.
- **TT-017 Controller: ExpenseCategoryInsert and ExpenseCategoryUpdate** (parent T-004, design Design 4.6; AC4.1-4.9, AC4.13): done when valid add persists with all-stores default; invalid add/update rejected with localized messages and nothing saved; different type rejected, same type accepted; type and stored Name never altered by anything but the update's allowed fields; direct calls validated identically to the UI.
- **TT-018 Add panel, inline edit and expensecategory-grid.js** (parent T-004, design Design 5.2 (2, 3), 5.2 script): done when add saves and the grid refreshes with no navigation; inline edit changes Name and limit only; type read-only in edit mode; errors shown per field; all controls keyboard operable. Version/risk note: Modal open/save script is added to this same file by TT-023. Needs a type-less submit attempt and a negative limit for manual verification.
- **TT-019 xUnit: ExpenseCategoryValidator tests** (parent T-004, design Design 7; AC4.3, AC4.7-4.12): done when all tests pass.
- **TT-020 xUnit: controller insert/update tests** (parent T-004, design Design 7; AC4.5, AC4.8, AC4.9, AC4.13): done when all tests pass. Version/risk note: Mirrors `TimeLogControllerTests` mocking approach.
- **TT-021 Modal models (stores and translations)** (parent T-005, design Design 4.6 (Models)): done when compile; display names resolve to locale keys.
- **TT-022 Controller: EditStoresAndTranslations and SaveStoresAndTranslations** (parent T-005, design Design 4.2, 4.4, 4.6; AC5.1, AC5.3, AC5.5-5.10): done when selecting stores limits the category and clearing all stores returns it to all stores; translations persist per language and empty ones fall back to the default; over-200 translation rejects the whole save with nothing stored; stored Name, limit and type unchanged; direct unauthorized calls denied. Version/risk note: `SaveStoreMappingsAsync` in 4.90.8 also flips `LimitedToStores` itself (verified in `StoreMappingService`), so the controller must not set it separately.
- **TT-023 Modal partial view and modal script** (parent T-005, design Design 5.2 (4), accessibility paragraph; AC5.1-5.4): done when modal opens on the same page, saves, closes and the grid shows the updated Stores indicator; keyboard-only operation, focus return and Escape work; store filter reflects the change (AC5.4). Version/risk note: Needs demo data: at least two stores and two installed languages. WCAG 2.1 AA applies.
- **TT-024 xUnit: modal save tests** (parent T-005, design Design 7; AC5.8, AC5.9): done when all tests pass.
- **TT-025 Plugin Install/Uninstall wiring** (parent T-002, design Design 5.5; AC2.5): done when plugin installs and uninstalls cleanly from Admin > Plugins; after uninstall the menu, permission and all `Admin.Expenses*`, enum and permission locale resources are gone; reinstall works. Version/risk note: Wired last, because it references every registration above.
- **TT-026 Uninstall cleanup of StoreMapping and LocalizedProperty rows** (parent T-002, design Design 3.4; AC2.6): done when with categories that have store mappings and translations, uninstall leaves zero StoreMapping rows for `ExpenseCategory` and zero LocalizedProperty rows with key group `ExpenseCategory`. Version/risk note: Needs data from TT-022 flows (categories with mapped stores and translations) to verify; install/uninstall symmetry rule from CLAUDE.md.
- **TT-027 xUnit: permission coverage tests** (parent T-002, design Design 7; AC2.1, AC2.3, AC5.10): done when all tests pass; adding an unprotected public action later fails the test. Version/risk note: Runs after all six actions exist.
- **TT-028 xUnit: plugin lifecycle symmetry tests** (parent T-002, design Design 7, 5.5; AC2.5, AC2.6): done when all tests pass; adding a locale key outside the deleted prefixes fails the test. Version/risk note: Symmetry rule from CLAUDE.md Plugin Lifecycle. If `BasePlugin` makes `base.UninstallAsync()` hard to unit test, test via extracted cleanup method and record in Notes.
- **TT-029 Build and install/uninstall smoke verification** (parent T-002, design Design 5.5; AC2.1-AC2.6): done when all steps pass; results noted in this ticket's Notes for the QA agent. Version/risk note: Developer-side gate before hand-off to `nopcommerce-qa-tester`; not a QA replacement.

## Version and compatibility flags (4.90.8)

- Use `INopStartup` (not `IDependencyRegistrar`), `IPermissionConfigManager` (not `IPermissionProvider`), `AdminMenuCreatedEvent` consumer, `[CheckPermission]` attribute, DataTables admin grid (no Kendo in this repo's admin). Same patterns as `Nop.Plugin.Misc.TimeLog`.
- Locale resources: `AddOrUpdateLocaleResourceAsync` / `DeleteLocaleResourcesAsync` by prefix (current pattern).
- Developer-confirmation items: table-name mechanism (TT-004), localized-editor tag helper inside AJAX partial (TT-023), `AdminMenuCreatedEvent` availability on first build (TT-011).

## Demo-data / environment needs

- SQL Server dev database; a restart after install for the permission (TT-009).
- At least two stores and two languages (TT-014, TT-023, TT-026).
- A non-Administrator admin user without the permission for AC2.2/AC2.3 checks.
- `CatalogSettings.IgnoreStoreLimitations` = false for store-filter checks.

## Acceptance criteria coverage

| Tickets | Criteria | Implementing tasks | Test tasks |
|---|---|---|---|
| T-001 | AC1.1-1.3 | TT-001, TT-010, TT-011, TT-014 | TT-029 (manual) |
| T-002 | AC2.1-2.6 | TT-009, TT-025, TT-026 | TT-027, TT-028, TT-029 |
| T-003 | AC3.1-3.10 | TT-003..TT-007, TT-012..TT-014 | TT-008, TT-015 |
| T-004 | AC4.1-4.14 | TT-016..TT-018 | TT-019, TT-020 |
| T-005 | AC5.1-5.10 | TT-021..TT-023 | TT-024 |

## Scope statement

No additions beyond the design. TT-029 is a developer verification gate, not new scope. There is no enabled/active field, no delete, no uniqueness rule, no settings class and no storefront component, as the design states.
