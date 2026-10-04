# Tickets - expense-category

Source Jira Issue: SCRUM-38 (linked, not created; no parent Epic). Target nopCommerce 4.90.8. Plugin: new `Nop.Plugin.Misc.Expenses`. Placement: nop-plugin, no core modification, so no Core Modification Notice is required.
Sources: `../requirements/clarified-requirement.md`, `../stories/user-stories.md`, `../acceptance-criteria/acceptance-criteria.md`, `../design/technical-design.md`.
Jira note: all five local tickets map to the single existing Jira Story SCRUM-38. On 2026-10-04 SCRUM-38 was moved to In Review, and T-001 to T-005 were created as Jira sub-tasks SCRUM-39 to SCRUM-43 (T-002 / SCRUM-40 In Progress because it is Blocked; the other four In Review). Tasks (TT-*) and bugs (BUG-*) are not mirrored in Jira. Later status changes made here are NOT pushed to Jira automatically and need reconciling there by hand.

## Index
| ID | Title | Type | Placement | Status | Linked Jira ID |
|---|---|---|---|---|---|
| T-001 | Expenses menu with Expense Category sub menu | Story | nop-plugin | Done | SCRUM-38 (sub-task SCRUM-39) |
| T-002 | Permission gating and clean uninstall | Story | nop-plugin | Blocked | SCRUM-38 (sub-task SCRUM-40) |
| T-003 | Expense Category grid with search | Story | nop-plugin | Done | SCRUM-38 (sub-task SCRUM-41) |
| T-004 | Add a category and edit it inline | Story | nop-plugin | Done | SCRUM-38 (sub-task SCRUM-42) |
| T-005 | Per-store availability and per-language names | Story | nop-plugin | Done | SCRUM-38 (sub-task SCRUM-43) |
| TT-001 | Plugin project scaffold | Task | nop-plugin | Done | SCRUM-38 |
| TT-002 | xUnit test project scaffold | Task | nop-plugin | Done | SCRUM-38 |
| TT-003 | ExpenseCategory entity, type enum and defaults | Task | nop-plugin | Done | SCRUM-38 |
| TT-004 | ExpenseCategoryBuilder and table naming | Task | nop-plugin | Done | SCRUM-38 |
| TT-005 | SchemaMigration (installation) | Task | nop-plugin | Done | SCRUM-38 |
| TT-006 | IExpenseCategoryService and ExpenseCategoryService | Task | nop-plugin | Done | SCRUM-38 |
| TT-007 | NopStartup DI registration | Task | nop-plugin | Done | SCRUM-38 |
| TT-008 | xUnit: ExpenseCategoryService tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-009 | PermissionProvider (IPermissionConfigManager) | Task | nop-plugin | Done | SCRUM-38 |
| TT-010 | Locale resource inventory | Task | nop-plugin | Done | SCRUM-38 |
| TT-011 | AdminMenuManager (Expenses > Expense Category) | Task | nop-plugin | Done | SCRUM-38 |
| TT-012 | Search, list and row models | Task | nop-plugin | Done | SCRUM-38 |
| TT-013 | ExpenseCategoryController: List and ExpenseCategoryList | Task | nop-plugin | Done | SCRUM-38 |
| TT-014 | List view: search panel and read-only grid | Task | nop-plugin | Done | SCRUM-38 |
| TT-015 | xUnit: controller list tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-016 | ExpenseCategoryValidator | Task | nop-plugin | Done | SCRUM-38 |
| TT-017 | Controller: ExpenseCategoryInsert and ExpenseCategoryUpdate | Task | nop-plugin | Done | SCRUM-38 |
| TT-018 | Add panel, inline edit and expensecategory-grid.js | Task | nop-plugin | Done | SCRUM-38 |
| TT-019 | xUnit: ExpenseCategoryValidator tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-020 | xUnit: controller insert/update tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-021 | Modal models (stores and translations) | Task | nop-plugin | Done | SCRUM-38 |
| TT-022 | Controller: EditStoresAndTranslations and SaveStoresAndTranslations | Task | nop-plugin | Done | SCRUM-38 |
| TT-023 | Modal partial view and modal script | Task | nop-plugin | Done | SCRUM-38 |
| TT-024 | xUnit: modal save tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-025 | Plugin Install/Uninstall wiring | Task | nop-plugin | Done | SCRUM-38 |
| TT-026 | Uninstall cleanup of StoreMapping and LocalizedProperty rows | Task | nop-plugin | Done | SCRUM-38 |
| TT-027 | xUnit: permission coverage tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-028 | xUnit: plugin lifecycle symmetry tests | Task | nop-plugin | Done | SCRUM-38 |
| TT-029 | Build and install/uninstall smoke verification | Task | nop-plugin | Blocked | SCRUM-38 |
| BUG-001 | Per Month Limit accepts group separators and silently misreads them (1,5 in en = 15; 1.5 in de = 15) | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-002 | Per Month Limit has no upper bound or precision rule: huge values overflow decimal(18,4) (unhandled DB error), over 4 decimals silently rounded | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-003 | Uninstall leaves per-customer GenericAttribute rows (ExpenseCategoryPage.Hide*) behind | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-004 | Add/save success status lives inside the collapsible Add panel and is hidden/not announced when it is collapsed | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-005 | Focus is lost when inline Edit is activated (Edit link is hidden, focus not moved into the row) | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-006 | Store multi-select has no multi-select or clear-to-all instructions (keyboard/screen reader) | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-007 | Status text uses text-success; contrast on white is about 3.1:1 (below 4.5:1) | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-008 | Hardcoded or raw user-facing text: literal 200 in NameTooLong, raw alert(errorThrown), dead Accessibility locale keys | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-009 | Insert/Update/Save actions do not check ModelState.IsValid first (security standard 2) | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-010 | Store filter silently does nothing when CatalogSettings.IgnoreStoreLimitations is ON | Bug | nop-plugin | Done | SCRUM-38 |
| BUG-011 | Inline Edit, Update and Cancel links have the same accessible name in every row | Bug | nop-plugin | Backlog | SCRUM-38 |
| BUG-012 | Focus is lost again after inline Update or Cancel (and the grid redraw) | Bug | nop-plugin | Backlog | SCRUM-38 |
| BUG-013 | Docs still say the store filter needs IgnoreStoreLimitations OFF (plan TT-006 and prerequisites, TT-006 ticket text, TT-029 step f) | Bug | nop-plugin | Backlog | SCRUM-38 |

---

## T-001: Expenses menu with Expense Category sub menu
- **Type**: Story
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Description**: As a store admin user with access to Expenses, I want a new "Expenses" menu item in the admin menu with an "Expense Category" sub menu, so that I can reach expense category management from the admin navigation. (Story 1; Jira line 1.)
- **Acceptance Criteria**:
  - AC1.1 Given a user holding the Expenses permission, when the admin menu renders, then a top-level "Expenses" item is shown containing exactly one sub item "Expense Category".
  - AC1.2 Given the user clicks "Expense Category", when the page loads, then the Expense Category page is displayed with the search panel, the Add new panel and the grid.
  - AC1.3 Given menu text, when the admin language changes, then the labels come from locale resources, not hardcoded strings.
- **Dependencies**: T-002 (the menu is gated by the permission); the plugin scaffold (project, `plugin.json`, `ExpensesPlugin`) is created as part of this work and is the first item the planner should sequence.
- **Verification**: Admin menu shows Expenses > Expense Category for a permitted user and opens the List page; labels come from `Admin.Expenses.Menu.*` locale keys; menu disappears when the plugin is uninstalled. Design ref: technical-design 5.1 (`AdminMenuManager : IConsumer<AdminMenuCreatedEvent>`).
- **Notes**: Plugin-only, no core files touched. Menu items use `PermissionNames = { ManageExpenseCategories }` on parent and child. **Implementation (2026-10-04):** Implemented via TT-001, TT-002, TT-010, TT-011 (TT-009 from T-002 done as prerequisite). Plugin-only, no core files touched, no Core Modification Notice. The menu link targets Admin/ExpenseCategory/List and 404s until TT-013/TT-014 add the controller and view. Runtime verification of AC1.1-AC1.3 (menu visible, labels from locale) needs install and restart on a dev database and is part of TT-029. **QA Result (2026-10-04):** PASS by code review only for AC1.1, AC1.2, AC1.3 (menu code, permission names, locale keys; key presence covered by ExpensesLocaleResourceTests). Runtime behaviour UNVERIFIED pending TT-029 (site not installed in the QA environment). Related: BUG-008 (Low).

## T-002: Permission gating and clean uninstall
- **Type**: Story
- **Placement**: nop-plugin
- **Status**: Blocked
- **Linked Jira ID**: SCRUM-38
- **Description**: As a store administrator, I want one new permission (example name `ManageExpenseCategories`) that controls the Expenses menu, viewing and editing, granted to the Administrators role by default, and I want uninstalling the plugin to leave no leftover data rows behind, so that only authorized users can use expense features and the store stays clean if the plugin is removed. (Story 2; Jira line 2, D4, uninstall cleanup.)
- **Acceptance Criteria**:
  - AC2.1 Given a fresh install, when an admin views the role permission screen, then one new permission is listed and is granted to the Administrators role by default.
  - AC2.2 Given a user without the permission, when the admin menu renders, then "Expenses" is not shown.
  - AC2.3 Given a user without the permission, when they request the Expense Category page or any of its endpoints (grid read, add, update, stores and translations modal load and save) directly, then access is denied server-side.
  - AC2.4 Given a user with the permission, when they use the menu, search, view the grid, add, edit inline, and use the stores and translations modal, then no second permission is required.
  - AC2.5 Given the plugin is uninstalled, when uninstall completes, then the menu entry, the permission and the plugin's locale resources are removed.
  - AC2.6 Given categories with store mappings and translated names exist, when the plugin is uninstalled, then all StoreMapping rows for the ExpenseCategory entity and all LocalizedProperty rows for the ExpenseCategory entity are deleted, and no such rows remain in the database.
- **Dependencies**: None for the permission itself. AC2.3 and AC2.4 are fully verifiable only once the endpoints in T-003, T-004 and T-005 exist; AC2.6 needs data created by T-005.
- **Verification**: Permission `Misc.Expenses.ManageExpenseCategories` listed and mapped to Administrators after first app start; `[CheckPermission]` present on all six controller actions; non-permitted direct calls denied; uninstall removes permission, `Admin.Expenses*` and enum locale resources, StoreMapping and LocalizedProperty rows (EntityName / LocaleKeyGroup = `ExpenseCategory`), and the table; Install/Uninstall symmetry covered by xUnit. Design ref: technical-design 3.4, 5.4, 5.5.
- **Notes**: Uses `IPermissionConfigManager`. Uninstall must delete StoreMapping and LocalizedProperty rows before `base.UninstallAsync()`. **Implementation (2026-10-04):** TT-009, TT-025 to TT-028 are Done; plugin-only, no core files touched, no Core Modification Notice. Status is Blocked ONLY because TT-029 (runtime smoke on a dev database) cannot be done in this environment (no `App_Data/dataSettings.json`, so the site is not installed and has no database); no code work remains. 94 Expenses tests pass, the full solution builds. AC2.3/AC5.10 are proven structurally by PermissionCoverageTests (all six actions carry `[CheckPermission(ManageExpenseCategories)]`); actual denial at runtime and AC2.1, AC2.2, AC2.4, AC2.5, AC2.6 at runtime remain to be verified (see TT-029 for the checklist). Files: src/Plugins/Nop.Plugin.Misc.Expenses/ExpensesPlugin.cs (changed); tests Infrastructure/PermissionCoverageTests.cs and Infrastructure/ExpensesPluginLifecycleTests.cs (new). No migration. **QA Result (2026-10-04):** NOT READY, ticket is Blocked (not Done); code and tests were reviewed anyway. AC2.1, AC2.3, AC2.5, AC2.6 have automated structural or mock-based coverage only; AC2.2, AC2.4 code review only; no AC verified at runtime. Related: BUG-003 (Low, uninstall leaves GenericAttribute rows).

## T-003: Expense Category grid with search
- **Type**: Story
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Description**: As a user with the Expenses permission, I want a paged grid of expense categories showing Name, Per Month Limit, Expense or Income and a Stores indicator, with a search by Name and an optional Store filter, so that I can find any category and see its limit at a glance. (Story 3; Jira line 3, D5, D8, D11, D12, D13. No delete.)
- **Acceptance Criteria**:
  - AC3.1 Given categories exist, when the grid loads, then it shows the columns Name, Per Month Limit, Expense/Income and Stores for each category.
  - AC3.2 Given more rows than one page, when the grid loads, then the data is paged with at most 100 rows per page, even if a larger page size is requested directly.
  - AC3.3 Given no categories exist, when the grid loads, then an empty grid is shown without error.
  - AC3.4 Given a category with no Per Month Limit, when the grid renders, then that cell is shown empty (not as 0).
  - AC3.5 Given a Per Month Limit, when the grid renders, then it is shown in the store's primary currency format.
  - AC3.6 Given categories with different names, when the user enters text in the Name search and searches, then only categories whose default-language Name contains the text (trimmed, case-insensitive) are listed; an empty search lists all. Translated names are not searched. The grid has no delete action, and a direct delete request is not available.
  - AC3.7 Given the Store filter is "All stores" (default), then every category is listed regardless of store mapping.
  - AC3.8 Given the Store filter set to a specific store, when the user searches, then only categories available in that store are listed (unlimited categories plus those mapped to that store).
  - AC3.9 Given a Name search and a Store filter are both set, then only categories matching both are listed.
  - AC3.10 Given the Stores column, when a category is not limited to stores, then it shows "All stores"; when limited, it shows that it is limited to selected stores.
- **Dependencies**: T-002 (permission), T-001 (page reachable from the menu). Entity, migration, service and DataTables grid are built here.
- **Verification**: Grid renders the four columns; page size clamped to 100 server-side; empty state works; null limit renders empty; limit formatted in primary currency; name and store filters work alone and combined; no delete endpoint or UI. Design ref: technical-design 3.1, 3.3, 4.1, 4.6 (`List`, `ExpenseCategoryList`), 5.2. xUnit: paging clamp, ordering, case-insensitive trimmed name filter, store filter via mocked `IStoreMappingService`.
- **Notes**: Grid is DataTables (not Kendo) per D1/OQ-B. Includes the `ExpenseCategory` entity, `SchemaMigration` and `ExpenseCategoryBuilder` (table name mechanism to be confirmed by the developer in 4.90.8). **QA Result (2026-10-04):** PASS with caveats. AC3.2, AC3.3, AC3.4, AC3.5, AC3.6, AC3.7, AC3.9 automated (mock-based); AC3.8 automated with a mocked ApplyStoreMapping only; AC3.1, AC3.10 code review only. Runtime behaviour UNVERIFIED pending TT-029 (site not installed in the QA environment). Related: BUG-010 (Low).

## T-004: Add a category and edit it inline
- **Type**: Story
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Description**: As a user with the Expenses permission, I want to add a new category in an "Add new" panel above the grid (Name, optional Per Month Limit, Expense or Income) and edit the Name and Per Month Limit of an existing category inline in the grid, with the type fixed after creation, so that the category list reflects how we classify our expenses and income without leaving the grid page. (Story 4; issue title "Create/Edit Category", Jira line 3, D1, D2, D3, D6, D7.)
- **Acceptance Criteria**:
  - AC4.1 Given valid Name, optional limit and a type in the Add new panel, when saved, then the category is persisted, appears in the grid with the entered values, and the page is not navigated away.
  - AC4.2 Given an existing row, when the user edits Name and/or Per Month Limit inline and confirms, then the grid shows the updated values.
  - AC4.3 Given the Add new panel, the only type choices are Expense and Income and one must be chosen; saving without a type fails with a localized validation message.
  - AC4.4 Given an existing category in inline edit, the type is displayed read-only and cannot be changed.
  - AC4.5 Given an update sent directly with a different type, then it is rejected with a localized message and nothing is saved; an update posting the same type succeeds.
  - AC4.6 Given Per Month Limit left empty, the save succeeds.
  - AC4.7 Given a limit of 0 or any positive decimal, the save succeeds and the value is stored as entered.
  - AC4.8 Given a negative or non-numeric limit, validation fails with a localized message and nothing is saved.
  - AC4.9 Given an invalid add or update sent directly to the endpoint, server-side validation rejects it the same way as the UI.
  - AC4.10 Given any limit value, the limit is informational only; no feature blocks or warns based on it.
  - AC4.11 Given the Name: blank fails with a localized message; longer than 200 characters fails with a localized message; a duplicate of an existing Name succeeds. Nothing is saved on failure.
  - AC4.12 Given a category of type Expense or Income, the same limit rules apply to both (optional, 0 or greater).
  - AC4.13 Given a new category is created, it is available in all stores (not limited to stores) until stores are changed in the modal.
  - AC4.14 Given a store or language change, it is made only in the Edit stores and translations modal (Story 5); the Add panel does not set stores or translations.
- **Dependencies**: T-003 (entity, service, grid), T-002 (permission).
- **Verification**: Add panel creates and refreshes grid without navigation; inline edit changes Name and limit only; type locked in UI and on the server (`TypeLocked` error); `ExpenseCategoryValidator` enforces name, limit and type rules for UI and direct calls; new rows have `LimitedToStores = false`. Design ref: technical-design 4.1, 4.6 (`ExpenseCategoryInsert`, `ExpenseCategoryUpdate`), 5.2. xUnit: validator (including duplicates accepted and Income limit), controller type-lock and `ModelState` limit mapping.
- **Notes**: Add panel / inline split relies on the D1 relaxation (technical-design section 2, DR-1). Per the clarified requirement, D1 is recorded as approved by the user. The `clarified-requirement.md` D1 text already reflects the relaxed model. Wording of AC4.13 and AC4.14 depends on T-005 for the modal. **Implementation (2026-10-04):** Implemented via TT-016 to TT-020; plugin-only, no core files touched, no Core Modification Notice. Files under src/Plugins/Nop.Plugin.Misc.Expenses: Validators/ExpenseCategoryValidator.cs, Controllers/ExpenseCategoryController.cs (ExpenseCategoryInsert, ExpenseCategoryUpdate), Views/ExpenseCategory/List.cshtml (Add panel, inline edit), Content/expensecategory-grid.js, NopExpensesDefaults.NameMaxLength; tests under Nop.Plugin.Misc.Expenses.Tests (59 pass in total, 32 new). No new locale keys were needed. Deviation from design 4.6: the stock inline edit posts only the Editable columns, not "every column", so the type is NOT in the stock post; the grid script appends the row current type (read from the Type cell) to the update request, and the server treats a missing or different type as TypeLocked. Runtime verification of AC4.1-AC4.14 (Add panel, inline edit, keyboard operation, field-error display) needs install on a dev database and is part of TT-029. The Razor view and the script are not compiled or executed by dotnet build. **QA Result (2026-10-04):** FAIL (two Medium defects, no Critical/High). Server rules AC4.1, AC4.3, AC4.5, AC4.6, AC4.11, AC4.12, AC4.13 automated; AC4.7, AC4.8, AC4.9 automated for the happy and negative paths but BUG-001 (culture-lenient parse) and BUG-002 (no upper bound or precision, DB overflow) break them for real input; AC4.2, AC4.4, AC4.14 code review only. Runtime behaviour UNVERIFIED pending TT-029 (site not installed in the QA environment). Related: BUG-001, BUG-002, BUG-004, BUG-005, BUG-007, BUG-009.

## T-005: Per-store availability and per-language names
- **Type**: Story
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Description**: As a user with the Expenses permission, I want to open an "Edit stores and translations" modal on a category row to choose which stores the category is available in and to enter a translated Name for each language, so that each store and language shows the right categories and names. (Story 5; D1, D8, D9. A new category defaults to all stores and has no translations.)
- **Acceptance Criteria**:
  - AC5.1 Given a category row, when the user activates its "Edit stores and translations" button, then a modal opens on the same page (no navigation) showing a store multi-select, a read-only reference of the default Name, and one Name field per language.
  - AC5.2 Given the modal is open and keyboard-only use, every control can be reached and used, focus stays inside the dialog, Escape closes it, and focus returns to the opening button. Fields have labels and the dialog has an accessible title.
  - AC5.3 Given one or more stores selected and saved, the category is limited to those stores, the modal closes, and the grid reloads and shows the updated Stores indicator.
  - AC5.4 Given a category limited to specific stores, when the Store filter is a store not selected, the category is not listed; when a selected store, it is listed.
  - AC5.5 Given all stores cleared and saved, the category is available in all stores again.
  - AC5.6 Given a translated Name entered for a language and saved, the translation is stored for that language and shown wherever a name is shown for that language.
  - AC5.7 Given a language whose translation is left empty, the default Name is used.
  - AC5.8 Given a translation longer than 200 characters, saving is rejected with a localized message and nothing from that save is stored; also enforced on direct requests.
  - AC5.9 Given the modal save, it never changes the stored default-language Name, the Per Month Limit or the type.
  - AC5.10 Given a user without the permission, loading or saving the modal endpoints directly is denied server-side (see AC2.3).
- **Dependencies**: T-003 (grid with Stores column and filter), T-004 (categories to edit), T-002 (permission).
- **Verification**: Modal opens from the row and saves via `IStoreMappingService.SaveStoreMappingsAsync` and `ILocalizedEntityService.SaveLocalizedValueAsync` (never direct repository writes, to keep framework caches valid); translation length validated server-side; type, limit and stored Name untouched; keyboard and focus behavior per WCAG 2.1 AA; pattern cache prefix invalidated on save. Design ref: technical-design 3.2, 4.2, 4.4, 4.6 (`EditStoresAndTranslations`, `SaveStoresAndTranslations`), 5.2. xUnit: modal save calls both services and does not alter type.
- **Notes**: Developer to verify the localized-editor tag helper works in an AJAX-loaded partial; fallback is the admin popup window pattern (`_AdminPopupLayout`). Search matches the default-language Name only (D11). **Implementation (2026-10-04):** Implemented via TT-003 to TT-008 and TT-012 to TT-015; plugin-only, no core files touched, no Core Modification Notice. Files under src/Plugins/Nop.Plugin.Misc.Expenses: Domain/, Data/Mapping/Builders/ExpenseCategoryBuilder.cs, Data/Migrations/SchemaMigration.cs (migration `2026/10/04 00:00:00`), Services/, Infrastructure/NopStartup.cs, Models/Admin/, Controllers/ExpenseCategoryController.cs, Views/ExpenseCategory/List.cshtml; tests under Nop.Plugin.Misc.Expenses.Tests. 27 unit tests pass. Table is named `ExpenseCategory` (see TT-004). Runtime verification of AC3.1-AC3.10 (grid render, store filter with IgnoreStoreLimitations off, currency format) needs install on a dev database and is part of TT-029. **Implementation (2026-10-04, modal):** Implemented via TT-021 to TT-024; plugin-only, no core files touched, no Core Modification Notice. New files in src/Plugins/Nop.Plugin.Misc.Expenses: Models/Admin/ExpenseCategoryStoresAndTranslationsModel.cs, ExpenseCategoryLocalizedModel.cs, Views/ExpenseCategory/_StoresAndTranslationsModal.cshtml, _StoresAndTranslationsBody.cshtml; changed: Controllers/ExpenseCategoryController.cs, Views/ExpenseCategory/List.cshtml, Content/expensecategory-grid.js; tests: Nop.Plugin.Misc.Expenses.Tests/Controllers/ExpenseCategoryControllerModalTests.cs (75 tests pass). No new locale keys and no migration. See TT-023 for the two UI deviations (no localized-editor tag helper, native multi-select). Runtime verification of AC5.1-AC5.5 (modal open, keyboard/focus, grid reload, store filter after save) needs install on a dev database with two stores and two languages (TT-029); AC5.10 is covered once TT-027 adds the permission coverage tests. **QA Result (2026-10-04):** PASS with caveats. AC5.3 (service calls), AC5.5, AC5.6 (storage), AC5.7, AC5.8, AC5.9, AC5.10 (structural) automated; AC5.1, AC5.2, AC5.4 code review only. Display of translations and default fallback has no consumer in this phase (admin grid always shows the default Name). Runtime behaviour UNVERIFIED pending TT-029 (site not installed in the QA environment). Related: BUG-004, BUG-006, BUG-007, BUG-010.
---

# Tasks for T-001

## TT-001: Plugin project scaffold
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-001
- **Linked Jira ID**: SCRUM-38
- **Description**: Create `src/Plugins/Nop.Plugin.Misc.Expenses`: `Nop.Plugin.Misc.Expenses.csproj` (copy of time-log's; `OutputPath` -> `Plugins\Misc.Expenses`; plugin.json, Views and Content as `PreserveNewest`), fully populated `plugin.json` (Group `Misc`, FriendlyName `Expenses`, SystemName `Misc.Expenses`, Version `1.0`, SupportedVersions `["4.90"]`, Author `OfficeManagement Team`, DisplayOrder, FileName `Nop.Plugin.Misc.Expenses.dll`, Description), `ExpensesPlugin : BasePlugin, IMiscPlugin` with empty-bodied Install/Uninstall (filled by TT-025/TT-026), `NopExpensesDefaults.cs` (MaxPageSize = 100, `ExpenseCategoriesPatternCacheKey` prefix `Nop.plugins.misc.expenses.expensecategory.`), `Views/_ViewImports.cshtml`. Add the project to `src/NopCommerce.sln`.
- **Dependencies**: None
- **Verification (Definition of Done)**: Solution builds with the new project; plugin DLL and plugin.json land in `Plugins\Misc.Expenses`; plugin appears in Admin > Plugins as `Expenses`; every plugin.json field populated.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 1, 7. Build stage: Stage 0 - Scaffold. First item to sequence (T-001 owns the scaffold, but T-002 tasks also need it, so it precedes everything). No core files touched. **Implementation (2026-10-04):** Created src/Plugins/Nop.Plugin.Misc.Expenses (csproj, plugin.json, ExpensesPlugin, NopExpensesDefaults, Views/_ViewImports.cshtml, empty Content/) and added it plus the Tests project to src/NopCommerce.sln (Plugins folder). csproj uses globs (Content\**\*.js, Views\**\*.cshtml, PreserveNewest) instead of per-file entries so later view/js tasks need no csproj edits. OutputPath mirrors time-log ($(SolutionDir)\Presentation\Nop.Web\Plugins\Misc.Expenses). Build succeeded; DLL and plugin.json land in Presentation\Nop.Web\Plugins\Misc.Expenses. Appearing in Admin > Plugins not checked at runtime (needs app run). **QA Result (2026-10-04):** plugin.json fully populated, csproj globs correct, DLL and files land in Presentation/Nop.Web/Plugins/Misc.Expenses. Plugin listing in Admin UNVERIFIED.

---

## TT-002: xUnit test project scaffold
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-001
- **Linked Jira ID**: SCRUM-38
- **Description**: Create `src/Plugins/Nop.Plugin.Misc.Expenses.Tests` mirroring `Nop.Plugin.Misc.TimeLog.Tests.csproj` (xunit 2.9.3, Moq 4.20.72, Microsoft.NET.Test.Sdk 17.14.1, xunit.runner.visualstudio 3.1.4, coverlet.collector 6.0.4, `ProjectReference` to the Expenses plugin). Create empty `Services/`, `Validators/`, `Controllers/`, `Infrastructure/` folders. Add to `src/NopCommerce.sln`. Do not hard-code `TargetFramework` (inherit from `src/Directory.Build.props`, currently `net9.0`, as the time-log tests do).
- **Dependencies**: TT-001
- **Verification (Definition of Done)**: `dotnet test` on the new project runs (0 tests) and passes; project resolves the plugin reference.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7. Build stage: Stage 0 - Scaffold. See problem P3 in the plan: design and CLAUDE.md say .NET 8, the repo builds net9.0. **Implementation (2026-10-04):** Created src/Plugins/Nop.Plugin.Misc.Expenses.Tests (TargetFramework inherited from Directory.Build.props, net9.0; empty Services/Validators/Controllers/Infrastructure folders with .gitkeep where empty). Builds and runs. Also contains PermissionProviderTests (TT-009) and ExpensesLocaleResourceTests (TT-010): 5 tests passing. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Test project resolves and runs.

---

## TT-010: Locale resource inventory
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-001
- **Linked Jira ID**: SCRUM-38
- **Description**: Implement `ExpensesPlugin.GetLocaleResources()` with the full key set from design 5.6 (menu, page/grid, search, fields, modal, enum, validation incl. `RecordNotFound`/`TypeLocked`, notifications, accessibility labels, `Security.Permission.Misc.Expenses.ManageExpenseCategories`). Expose it so the lifecycle tests (TT-028) can enumerate keys (internal + `InternalsVisibleTo`, or public static). Wire the `AddOrUpdateLocaleResourceAsync` call into `InstallAsync`.
- **Dependencies**: TT-001
- **Verification (Definition of Done)**: After install every key resolves via `ILocalizationService`; no hard-coded user-facing text anywhere in plugin code or views; no `NameDuplicate` key exists.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.6; AC1.3. Build stage: Stage 2 - Services, DI, permission, locale. Done early because the validator, controller, menu and views all reference these keys. **Implementation (2026-10-04):** ExpensesPlugin.GetLocaleResources() is public static (IReadOnlyDictionary) and InstallAsync calls AddOrUpdateLocaleResourceAsync with it. Key naming decision: design 5.6 abbreviates keys with "..."; resolved to Admin.Expenses.ExpenseCategory.{PageTitle|List.*|Search.*|Fields.*|StoresAndTranslations.*|Validation.*|Added|Updated|Accessibility.*} plus Admin.Expenses.Menu.*, so every key sits under the single Admin.Expenses prefix that uninstall deletes (TT-025). No NameDuplicate key. NameTooLong text hard-codes "200 characters" in the English string value only. UninstallAsync is still empty apart from base (TT-025/TT-026), so uninstall is not yet symmetric. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). All keys under deleted prefixes; no NameDuplicate key. Related: BUG-008 (Low: hardcoded 200 in NameTooLong text, unused Accessibility.* keys).

---

## TT-011: AdminMenuManager (Expenses > Expense Category)
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-001
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Infrastructure/AdminMenuManager.cs` (`IConsumer<AdminMenuCreatedEvent>`): guard `LoadPluginBySystemNameAsync("Misc.Expenses")` null -> return; top-level `Nop.Plugin.Misc.Expenses.ExpensesAdminMenu` with exactly one child `Nop.Plugin.Misc.Expenses.ExpenseCategoryAdminMenu` (`GetMenuItemUrl("ExpenseCategory", "List")`); `PermissionNames = { ManageExpenseCategories }` on both; inserted before "Third party plugins" with `ChildNodes.Add` fallback; titles from locale.
- **Dependencies**: TT-009, TT-010
- **Verification (Definition of Done)**: Permitted user sees Expenses > Expense Category and it opens the List page; user without the permission does not see Expenses; labels come from `Admin.Expenses.Menu.*`; menu disappears when the plugin is uninstalled.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.1. Build stage: Stage 3 - Menu, models, controller read path, validator. Link target works fully only after TT-013/TT-014 exist. Confirm `AdminMenuCreatedEvent` on first build (design 2.4). **Implementation (2026-10-04):** Infrastructure/AdminMenuManager.cs added as designed (plugin-installed guard, Expenses top-level with one child, PermissionNames on both, InsertBefore "Third party plugins" with ChildNodes.Add fallback, titles from Admin.Expenses.Menu.*). AdminMenuCreatedEvent confirmed available in this 4.90.8 codebase (compiles; same event time-log uses). Not exercised at runtime yet. **QA Result (2026-10-04):** Plugin-installed guard, PermissionNames on parent and child, locale titles. Menu rendering UNVERIFIED.

---

# Tasks for T-002

## TT-009: PermissionProvider (IPermissionConfigManager)
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-002
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Infrastructure/PermissionProvider.cs`: public parameterless ctor, `ManageExpenseCategories = "Misc.Expenses.ManageExpenseCategories"`, `Category = "Misc.Expenses"`, `AllConfigs` with one `PermissionConfig` granted to `NopCustomerDefaults.AdministratorsRoleName`. Not registered in DI.
- **Dependencies**: TT-001
- **Verification (Definition of Done)**: After first application start with the plugin installed, one new permission is listed on the role permission screen and mapped to Administrators.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.4. Build stage: Stage 2 - Services, DI, permission, locale. The framework inserts the record and mapping on the next application start, so AC2.1 can be verified only after a restart. **Implementation (2026-10-04):** Infrastructure/PermissionProvider.cs added as designed (one permission Misc.Expenses.ManageExpenseCategories, Category Misc.Expenses, default role Administrators, not registered in DI). Unit test covers AllConfigs. AC2.1 needs a restart after install to verify. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). One permission mapped to Administrators. Insert-on-restart UNVERIFIED.

---

## TT-025: Plugin Install/Uninstall wiring
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-002
- **Linked Jira ID**: SCRUM-38
- **Description**: Complete `ExpensesPlugin.InstallAsync` (locale add, `base.InstallAsync()`) and `UninstallAsync` in the order from design 5.5: `DeletePermissionAsync(ManageExpenseCategories)`; `DeleteLocaleResourcesAsync("Admin.Expenses")`; `DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType")`; `DeleteLocaleResourceAsync($"Security.Permission.{ManageExpenseCategories}")`; `base.UninstallAsync()` last. Inject the services needed by TT-026.
- **Dependencies**: TT-009, TT-010, TT-011
- **Verification (Definition of Done)**: Plugin installs and uninstalls cleanly from Admin > Plugins; after uninstall the menu, permission and all `Admin.Expenses*`, enum and permission locale resources are gone; reinstall works.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.5; AC2.5. Build stage: Stage 7 - Install/uninstall wiring and lifecycle/permission tests. Wired last, because it references every registration above. **Implementation (2026-10-04):** ExpensesPlugin ctor now takes ILocalizationService, IPermissionService, IRepository<LocalizedProperty>, IRepository<StoreMapping>. InstallAsync is unchanged (TT-010: locale upsert, then base). UninstallAsync order: DeletePermissionAsync(ManageExpenseCategories); StoreMapping and LocalizedProperty predicate deletes (TT-026); DeleteLocaleResourcesAsync("Admin.Expenses"); DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType"); DeleteLocaleResourceAsync(Security.Permission.{ManageExpenseCategories}); base.UninstallAsync() last. Small addition: public static `LocaleResourcePrefixesDeletedOnUninstall` lists the three prefixes/keys so the symmetry test can compare them (the delete calls use the literals, and a test asserts both match). Not exercised at runtime (TT-029). Order deviation from design 5.5 text only: the row deletes sit right after the permission delete and before the locale deletes; design 3.4 only requires them before base.UninstallAsync(). **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Uninstall order and prefixes verified by mock tests. Related: BUG-003.

---

## TT-026: Uninstall cleanup of StoreMapping and LocalizedProperty rows
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-002
- **Linked Jira ID**: SCRUM-38
- **Description**: In `UninstallAsync`, before `base.UninstallAsync()`, delete all `StoreMapping` rows with `EntityName == nameof(ExpenseCategory)` and all `LocalizedProperty` rows with `LocaleKeyGroup == nameof(ExpenseCategory)` via `IRepository<StoreMapping>` / `IRepository<LocalizedProperty>` predicate delete. Must work when the plugin DLL is still loaded and the table still exists.
- **Dependencies**: TT-003, TT-025
- **Verification (Definition of Done)**: With categories that have store mappings and translations, uninstall leaves zero StoreMapping rows for `ExpenseCategory` and zero LocalizedProperty rows with key group `ExpenseCategory`.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 3.4; AC2.6. Build stage: Stage 7 - Install/uninstall wiring and lifecycle/permission tests. Needs data from TT-022 flows (categories with mapped stores and translations) to verify; install/uninstall symmetry rule from CLAUDE.md. **Implementation (2026-10-04):** In UninstallAsync, `_storeMappingRepository.DeleteAsync(m => m.EntityName == nameof(ExpenseCategory))` and `_localizedPropertyRepository.DeleteAsync(p => p.LocaleKeyGroup == nameof(ExpenseCategory))` (IRepository predicate delete, one parameterized statement each, run while the table still exists). The predicate delete does not clear the framework's in-memory localized-property cache; uninstalling a plugin restarts the application, so the cache is rebuilt. Runtime check of AC2.6 against real rows is part of TT-029. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Predicate deletes target ExpenseCategory rows only (mock). Real row deletion UNVERIFIED.

---

## TT-027: xUnit: permission coverage tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-002
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Infrastructure/PermissionCoverageTests.cs`: reflection test that all six controller actions (`List`, `ExpenseCategoryList`, `ExpenseCategoryInsert`, `ExpenseCategoryUpdate`, `EditStoresAndTranslations`, `SaveStoresAndTranslations`) carry `[CheckPermission(PermissionProvider.ManageExpenseCategories)]`, the class has `[AuthorizeAdmin]`, `[Area(AreaNames.ADMIN)]`, `[AutoValidateAntiforgeryToken]`, and no public action lacks the attribute; `PermissionProvider.AllConfigs` has exactly one entry mapped to Administrators.
- **Dependencies**: TT-009, TT-013, TT-017, TT-022, TT-002
- **Verification (Definition of Done)**: All tests pass; adding an unprotected public action later fails the test.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7; AC2.1, AC2.3, AC5.10. Build stage: Stage 7 - Install/uninstall wiring and lifecycle/permission tests. Runs after all six actions exist. **Implementation (2026-10-04):** Infrastructure/PermissionCoverageTests.cs, 11 test cases: the controller has exactly the six documented actions (public declared methods); class-level [Area(admin)], [AuthorizeAdmin], [AutoValidateAntiforgeryToken]; each of the six actions has exactly one [CheckPermission] with exactly ManageExpenseCategories; no public action lacks it (catches a future unprotected action); no action name contains Delete; the four POST actions carry [HttpPost]; PermissionProvider has one config mapped to Administrators. This is a structural proof: it shows the filter is declared, not that the framework denies a non-permitted user (that filter is framework code; runtime denial is a TT-029 check). **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Structural proof only (attributes present); actual denial UNVERIFIED.

---

## TT-028: xUnit: plugin lifecycle symmetry tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-002
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Infrastructure/ExpensesPluginLifecycleTests.cs` with mocked `ILocalizationService`, `IPermissionService`, repositories: Install adds every key from `GetLocaleResources()`; Uninstall deletes the permission and the three locale resource groups/keys covering every key prefix installed (assert each installed key falls under a deleted prefix); StoreMapping and LocalizedProperty predicate deletes target `ExpenseCategory` only and run before `base.UninstallAsync()`.
- **Dependencies**: TT-025, TT-026, TT-002
- **Verification (Definition of Done)**: All tests pass; adding a locale key outside the deleted prefixes fails the test.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7, 5.5; AC2.5, AC2.6. Build stage: Stage 7 - Install/uninstall wiring and lifecycle/permission tests. Symmetry rule from CLAUDE.md Plugin Lifecycle. If `BasePlugin` makes `base.UninstallAsync()` hard to unit test, test via extracted cleanup method and record in Notes. **Implementation (2026-10-04):** Infrastructure/ExpensesPluginLifecycleTests.cs, 7 tests with mocked ILocalizationService, IPermissionService and both repositories. Install adds exactly GetLocaleResources(); Uninstall deletes the permission and the permission locale key; every installed key falls under a deleted prefix/key (a new key outside them fails); the deleted set equals LocaleResourcePrefixesDeletedOnUninstall; the captured StoreMapping and LocalizedProperty predicates are compiled and shown to match only ExpenseCategory rows (not Product/Project); the cleanup call order is asserted. BasePlugin.UninstallAsync is a plain completed task, so the plugin is tested directly (no extracted method needed); cleanup preceding base.UninstallAsync() follows from it being the last statement, and the mock sequence proves all cleanup ran inside UninstallAsync. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). 7 lifecycle tests pass (mocks).

---

## TT-029: Build and install/uninstall smoke verification
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Blocked
- **Parent**: T-002
- **Linked Jira ID**: SCRUM-38
- **Description**: Full solution build, `dotnet test` on the Expenses tests project, then on a dev database: install the plugin, restart, confirm permission and Administrators mapping, walk the menu, add/edit/search/filter/modal flows, create mapped and translated data, uninstall and confirm table, menu, permission, locale resources, StoreMapping and LocalizedProperty rows are all gone; reinstall succeeds.
- **Dependencies**: TT-001 to TT-028
- **Verification (Definition of Done)**: All steps pass; results noted in this ticket's Notes for the QA agent.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.5; AC2.1-AC2.6. Build stage: Stage 8 - Build and smoke gate. Developer-side gate before hand-off to `nopcommerce-qa-tester`; not a QA replacement. **Outcome (2026-10-04): BLOCKED on the runtime part; the verifiable part passed.** Done and verified: (1) `dotnet build NopCommerce.sln -p:SolutionDir=...` succeeded, 0 errors, 3 pre-existing warnings (none in Expenses); (2) `dotnet test` Expenses: 94 passed, 0 failed (75 earlier + 19 new); TimeLog tests 80 passed; (3) plugin files in Presentation/Nop.Web/Plugins/Misc.Expenses: Nop.Plugin.Misc.Expenses.dll, .pdb, .deps.json, plugin.json, Content/expensecategory-grid.js, Views/_ViewImports.cshtml, Views/ExpenseCategory/List.cshtml, _StoresAndTranslationsModal.cshtml, _StoresAndTranslationsBody.cshtml (plus the same extra Nop.Web.*.json files Misc.TimeLog gets); plugin.json fully populated; (4) static review of the three views and the script against Table.cshtml, admin.table.js and admin.common.js: editData_/editRowData_ naming (`expensecategory_grid`) and the jQuery-row argument match the framework; escapeHtml, updateTable, addAntiForgeryToken and display_nop_error exist as globals; the default cell renderer HTML-encodes; the CSRF token is read the way the framework does; every collapsible panel has a hideAttribute name; _ViewImports supplies every injected service and namespace used; all strings go through T(...). No defects found. NOT done: Razor views are not compiled by the build, and the plugin could not be installed or the app started here: Presentation/Nop.Web/App_Data has no dataSettings.json (site not installed, no database). Nothing here claims runtime verification. **Remaining runtime checks (dev SQL Server database, two stores, two languages):** (a) plugin listed as Expenses in Admin > Plugins and Install succeeds; (b) table created (TT-004 notes: the table is named `ExpenseCategory`, not `Expenses_ExpenseCategory`) with columns Id, Name nvarchar(200), PerMonthLimit decimal(18,4) null, ExpenseCategoryTypeId, LimitedToStores bit default 0, CreatedOnUtc, UpdatedOnUtc; (c) after the restart the permission `Misc.Expenses.ManageExpenseCategories` is listed once and mapped to Administrators, and a role without it sees no Expenses menu and is denied on direct URL and AJAX calls (AC2.1-AC2.3, AC5.10); (d) Expenses > Expense Category menu and labels (AC1.1-AC1.3); (e) first render of the list page and of the modal (AJAX body, focus, Escape, focus return); (f) store filter with CatalogSettings.IgnoreStoreLimitations OFF (AC3.8, AC5.4); (g) add and inline edit (raw limit value in the edit input, type locked, field errors); (h) create mapped and translated data, then Uninstall: table dropped, menu gone, permission gone, no `Admin.Expenses*`/enum/permission locale resources, zero StoreMapping rows with EntityName `ExpenseCategory` and zero LocalizedProperty rows with LocaleKeyGroup `ExpenseCategory` (AC2.5, AC2.6); (i) reinstall succeeds. Set TT-029 and T-002 to Done once (a)-(i) pass. **QA Result (2026-10-04):** BLOCKED, unchanged. QA independently re-ran the build (0 errors, 4 warnings, none in Expenses; the developer recorded 3) and both test projects (Expenses 94/94, TimeLog 80/80). Runtime items (a) to (i) remain open and are listed in docs/expense-category/qa/qa-report.md.

---

# Tasks for T-003

## TT-003: ExpenseCategory entity, type enum and defaults
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Domain/ExpenseCategory.cs` (`BaseEntity, ILocalizedEntity, IStoreMappingSupported`; `Name`, `PerMonthLimit` decimal?, `ExpenseCategoryTypeId`, `[NotMapped] ExpenseCategoryType`, `LimitedToStores`, `CreatedOnUtc`, `UpdatedOnUtc`) and `Domain/ExpenseCategoryType.cs` (`Expense = 1`, `Income = 2`). No `Deleted` column.
- **Dependencies**: TT-001
- **Verification (Definition of Done)**: Compiles; enum values exactly 1 and 2 (unset 0 is invalid); `[NotMapped]` property mirrors `Project.Status` pattern; XML docs on public members.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 3.1. Build stage: Stage 1 - Entity, mapping and migration. Starts the entity-first chain; nothing else compiles without it. **Implementation (2026-10-04):** Domain/ExpenseCategory.cs (BaseEntity, ILocalizedEntity, IStoreMappingSupported; [NotMapped] ExpenseCategoryType mirrors Project.Status) and Domain/ExpenseCategoryType.cs (Expense = 1, Income = 2). No Deleted column. XML docs on all public members. **QA Result (2026-10-04):** Entity and enum as specified (Expense = 1, Income = 2, no Deleted column). Covered indirectly by validator tests.
---

## TT-004: ExpenseCategoryBuilder and table naming
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Data/Mapping/Builders/ExpenseCategoryBuilder.cs` (`NopEntityBuilder<ExpenseCategory>`: `Name` `AsString(200).NotNullable()`, `PerMonthLimit` `AsDecimal(18, 4).Nullable()`, `ExpenseCategoryTypeId` `AsInt32().NotNullable()`, `LimitedToStores` `AsBoolean().NotNullable().WithDefaultValue(false)`). Confirm the `INameCompatibility` mechanism for mapping to `Expenses_ExpenseCategory` in 4.90.8; if it does not work, fall back to `ExpenseCategory` and record the outcome in this ticket's Notes.
- **Dependencies**: TT-003
- **Verification (Definition of Done)**: Builder discovered by the framework; resulting table has the specified columns, types and nullability; no unique index on Name.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 3.3. Build stage: Stage 1 - Entity, mapping and migration. Developer-confirmation item from design 3.3 (table-name mechanism). **Implementation (2026-10-04):** Data/Mapping/Builders/ExpenseCategoryBuilder.cs as designed (Name nvarchar(200) NOT NULL, PerMonthLimit decimal(18,4) NULL, ExpenseCategoryTypeId int NOT NULL, LimitedToStores bit NOT NULL default 0); no unique index. **Table-name outcome: fell back to the default table name `ExpenseCategory`, no INameCompatibility.** Reason (static analysis of 4.90.8 source): ApplicationPartManagerExtensions only registers a plugin's INameCompatibility types for plugins already marked Installed, and NameCompatibilityManager initializes lazily and once per process, so while the install migration runs the mapping would not be loaded; the table would be created as `ExpenseCategory` and then mapped as `Expenses_ExpenseCategory` after restart. `ExpenseCategory` collides with no core table. Not exercised against a database yet (TT-029). **QA Result (2026-10-04):** Builder matches the specified columns; table-name fallback to ExpenseCategory is sound. Table creation UNVERIFIED (needs database).
---

## TT-005: SchemaMigration (installation)
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Data/Migrations/SchemaMigration.cs`: `[NopMigration("2026/10/04 00:00:00", "Nop.Plugin.Misc.Expenses schema", MigrationProcessType.Installation)]`, `AutoReversingMigration`, `Up()` = `Create.TableFor<ExpenseCategory>()`. No seed data. Timestamp literal must stay unique and unchanged after release.
- **Dependencies**: TT-004
- **Verification (Definition of Done)**: Plugin install creates the table with all columns including `LimitedToStores`; plugin uninstall drops it (auto-reversed `Down()`); re-install works.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 3.3. Build stage: Stage 1 - Entity, mapping and migration. Requires a SQL Server dev database to verify at runtime. **Implementation (2026-10-04):** Data/Migrations/SchemaMigration.cs, [NopMigration("2026/10/04 00:00:00", ..., Installation)], AutoReversingMigration, Up() = Create.TableFor<ExpenseCategory>(). No seed data. Runtime verification (table created, dropped on uninstall, reinstall) needs a SQL Server dev database and is part of TT-029. **QA Result (2026-10-04):** Migration is Installation type, AutoReversing; uninstall order (UninstallAsync before ApplyDownMigrations) confirmed in PluginService. Table create/drop UNVERIFIED (needs database).
---

## TT-006: IExpenseCategoryService and ExpenseCategoryService
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Services/IExpenseCategoryService.cs` and `ExpenseCategoryService.cs`: `GetAllExpenseCategoriesAsync(name, storeId, pageIndex, pageSize)` (trimmed case-insensitive contains on stored Name; `await _storeMappingService.ApplyStoreMapping(query, storeId)` when `storeId > 0`; order by Name then Id; pageSize clamped to `NopExpensesDefaults.MaxPageSize`), `GetExpenseCategoryByIdAsync`, `InsertExpenseCategoryAsync` (sets both timestamps), `UpdateExpenseCategoryAsync` (sets `UpdatedOnUtc`). Insert, Update call `RemoveByPrefixAsync` on `ExpenseCategoriesPatternCacheKey`. No Delete, no name-in-use check. `IRepository<ExpenseCategory>` only, async, parameterized LINQ.
- **Dependencies**: TT-003, TT-004, TT-005 (runtime)
- **Verification (Definition of Done)**: Paging clamps to 100; name and store filters work alone and combined; ordering stable; timestamps set; cache prefix invalidated on insert/update; no delete method exists.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.1, 4.4. Build stage: Stage 2 - Services, DI, permission, locale. 4.90.8 `ApplyStoreMapping<T>` is async (returns `Task<IQueryable<T>>`) and is a no-op when `CatalogSettings.IgnoreStoreLimitations` is true or no StoreMapping rows exist for the entity; AC3.8 testing must run with that setting false. **Implementation (2026-10-04):** Services/IExpenseCategoryService.cs and ExpenseCategoryService.cs. GetAll: pageSize clamped to [1, 100] and pageIndex to >= 0; name is trimmed and matched case-insensitively with ToLower().Contains (works in-memory and as parameterized SQL); `ApplyStoreMapping` only when storeId > 0 (P5: no-op when IgnoreStoreLimitations is on or no mappings exist, so AC3.8 needs that setting off); order by Name then Id. Insert sets both timestamps, Update sets UpdatedOnUtc, both call RemoveByPrefixAsync(ExpenseCategoriesPatternCacheKey). No Delete, no name-in-use check. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Clamp, ordering, name and store filters covered with mocks. The real LINQ-to-SQL translation of ToLower().Contains and of ApplyStoreMapping is UNVERIFIED. Related: BUG-010.
---

## TT-007: NopStartup DI registration
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Infrastructure/NopStartup.cs` (`INopStartup`, `Order => 3000`): `services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>()`. Do not register the validator (assembly scan registers it).
- **Dependencies**: TT-006
- **Verification (Definition of Done)**: App starts with the plugin installed; service resolves in the controller; validator not double-registered.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.1. Build stage: Stage 2 - Services, DI, permission, locale. `INopStartup` is the 4.90 pattern (same as time-log). **Implementation (2026-10-04):** Infrastructure/NopStartup.cs (INopStartup, Order 3000) registers IExpenseCategoryService as scoped; validator not registered. App-start resolution not exercised at runtime yet (TT-029). **QA Result (2026-10-04):** Service registered as scoped; validator not registered. App-start resolution UNVERIFIED.
---

## TT-008: xUnit: ExpenseCategoryService tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Services/ExpenseCategoryServiceTests.cs` (pattern of `ProjectServiceTests`): paging clamp to 100 even when a larger size is requested, ordering by Name then Id, name filter trimmed and case-insensitive, empty name lists all, store filter via mocked `IStoreMappingService`, combined name+store filter, timestamps set on insert and update, cache prefix removal called.
- **Dependencies**: TT-006, TT-002
- **Verification (Definition of Done)**: All tests pass; every service branch (name, store, both, none, clamp) has at least one test.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7; AC3.2, AC3.6-3.9. Build stage: Stage 2 - Services, DI, permission, locale. Business-logic xUnit requirement from CLAUDE.md. **Implementation (2026-10-04):** Tests/Services/ExpenseCategoryServiceTests.cs, 15 test cases: clamp (oversized and default page size), second page, empty table, ordering, trimmed case-insensitive name filter, empty name variants, store filter (not applied for 0, applied and honoured for >0), combined filters, insert/update timestamps and cache prefix, no Delete method. ApplyStoreMapping is mocked (real behaviour is the framework's). **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). 15 service tests pass.
---

## TT-012: Search, list and row models
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Models/Admin/` records: `ExpenseCategorySearchModel` (`SearchName`, `SearchStoreId`, `AvailableStores`), `ExpenseCategoryListModel`, `ExpenseCategoryModel` (`Name`, `PerMonthLimit`, `PerMonthLimitDisplay`, `ExpenseCategoryTypeId`, `ExpenseCategoryTypeName`, `LimitedToStores`) with `[NopResourceDisplayName]` on all fields.
- **Dependencies**: TT-003, TT-006
- **Verification (Definition of Done)**: Compile; display names resolve to locale keys.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.6 (Models). Build stage: Stage 3 - Menu, models, controller read path, validator. **Implementation (2026-10-04):** Models/Admin/ExpenseCategorySearchModel.cs, ExpenseCategoryListModel.cs, ExpenseCategoryModel.cs (records, [NopResourceDisplayName] on all fields using existing Admin.Expenses.ExpenseCategory.Fields.* / Search.* keys). No new locale keys were needed. **QA Result (2026-10-04):** Models carry NopResourceDisplayName on all fields.
---

## TT-013: ExpenseCategoryController: List and ExpenseCategoryList
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Controllers/ExpenseCategoryController : BasePluginController` with `[Area(AreaNames.ADMIN)] [AuthorizeAdmin] [AutoValidateAntiforgeryToken]` and `[CheckPermission(PermissionProvider.ManageExpenseCategories)]` on every action. Implement `List()` (search model with `SetGridPageSize()`, store dropdown with "All stores" default) and `ExpenseCategoryList(searchModel)` (`Math.Min(PageSize, 100)`, `PrepareToGridAsync`, primary currency resolved once per request via `ICurrencyService`/`CurrencySettings`, `IPriceFormatter.FormatPriceAsync`, empty cell when limit is null). No repository access. Later tasks add the other four actions to this class.
- **Dependencies**: TT-006, TT-007, TT-009, TT-010, TT-012
- **Verification (Definition of Done)**: Grid read returns paged JSON; page size never exceeds 100; null limit gives an empty display; limit formatted in primary currency; empty table returns an empty page without error.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.6, 5.2. Build stage: Stage 3 - Menu, models, controller read path, validator. No delete action exists on this controller. **Implementation (2026-10-04):** Controllers/ExpenseCategoryController.cs with [Area(Admin)] [AuthorizeAdmin] [AutoValidateAntiforgeryToken] and [CheckPermission(ManageExpenseCategories)] on List and ExpenseCategoryList. Page size = Math.Clamp(Length, 1, 100) (also guards Length = 0, which would divide by zero in BaseSearchModel.Page); primary currency resolved once via ICurrencyService + CurrencySettings; null limit gives an empty display, otherwise IPriceFormatter.FormatPriceAsync(limit, true, primaryCurrency) (plain N2 fallback only if the primary currency cannot be loaded). Store dropdown via IBaseAdminModelFactory.PrepareStoresAsync with the Search.Store.All text. No repository access, no delete action. TT-017/TT-022 add the remaining four actions to this class. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Clamp, null-limit empty cell and single currency lookup covered. Real currency formatting UNVERIFIED.
---

## TT-014: List view: search panel and read-only grid
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Views/ExpenseCategory/List.cshtml` (`_AdminLayout`, `NopHtml.SetActiveMenuItemSystemName`): collapsible search panel (Name, Store dropdown default "All stores", Search button) and DataTables grid `expensecategory-grid` with `UrlRead` only and no `UrlDelete`, server-side paging, `LengthMenu` capped at 100. Columns: Name, Per Month Limit (`PerMonthLimitDisplay`), Type (localized text), Stores ("All stores" or `Admin.Expenses...List.Stores.Limited`), row button column. Real labels, `aria-label`s from locale.
- **Dependencies**: TT-013
- **Verification (Definition of Done)**: Page renders with search panel, grid and the four columns; Name and Store filters narrow the grid alone and combined; Stores cell reads "All stores" or the limited text; no delete UI; page loads from the menu link.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.2 (1, 3). Build stage: Stage 4 - Read-only grid UI, controller read tests, modal models. Stores cell text uses the locale key, not the shorter word in design 5.2 (problem P4). Needs demo data: several categories with different names, and at least one second store for filter testing. Add panel placeholder is added by TT-018. **Implementation (2026-10-04):** Views/ExpenseCategory/List.cshtml: search panel (Name, Store default All stores, Search button), DataTables grid `expensecategory-grid` with UrlRead only, Filters SearchName and SearchStoreId, Length/LengthMenu from the search model. Columns Name, Per Month Limit (PerMonthLimitDisplay), Type, Stores (RenderCustom: Stores.All or Stores.Limited locale text, JS-encoded; P4 honoured). Deviation: no row button column yet, because the read-only grid has no row actions; TT-018/TT-023 add the edit and stores/translations buttons. The Razor view is compiled at runtime by nopCommerce, so it was not compiled by dotnet build; first render must be checked in TT-029. **QA Result (2026-10-04):** Razor not compiled by the build; static review found no compile problem, first render UNVERIFIED. Related: BUG-004, BUG-007.
---

## TT-015: xUnit: controller list tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-003
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Controllers/ExpenseCategoryControllerListTests.cs`: oversized `PageSize` is clamped to 100 before the service call, null limit yields empty `PerMonthLimitDisplay`, formatted limit comes from `IPriceFormatter` using the primary currency (resolved once), empty result returns an empty page.
- **Dependencies**: TT-013, TT-002
- **Verification (Definition of Done)**: All tests pass.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7; AC3.2, AC3.4, AC3.5. Build stage: Stage 4 - Read-only grid UI, controller read tests, modal models. Mock `ICurrencyService`, `IPriceFormatter`, `IExpenseCategoryService`. **Implementation (2026-10-04):** Tests/Controllers/ExpenseCategoryControllerListTests.cs, 7 tests: oversized and zero page size clamp before the service call, search filters and page index passed through, null limit gives an empty display with no formatter call, zero limit is formatted, primary currency resolved once and used for formatting, empty page. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). 7 list tests pass.
---

# Tasks for T-004

## TT-016: ExpenseCategoryValidator
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-004
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Validators/ExpenseCategoryValidator : BaseNopValidator<ExpenseCategory>`: type must be a defined enum value (`Validation.TypeRequired`); `PerMonthLimit` when not null >= 0 for both types (`Validation.PerMonthLimitInvalid`); Name required (`Validation.NameRequired`) and max 200 (`Validation.NameTooLong`); no uniqueness rule. Expose the 200 limit as a shared constant used by the translation check in TT-022.
- **Dependencies**: TT-003, TT-010
- **Verification (Definition of Done)**: Blank, whitespace, 201-char name, type 0, and negative limit fail with the localized message; empty limit, 0, positive decimal, 200-char name and duplicate name pass.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.1; AC4.3, AC4.7, AC4.8, AC4.11, AC4.12. Build stage: Stage 3 - Menu, models, controller read path, validator. Auto-registered by assembly scan; do not register in DI. **Implementation (2026-10-04):** Validators/ExpenseCategoryValidator.cs (BaseNopValidator, rules via WithMessageAwait on existing Validation.* locale keys). Name NotEmpty (blank and whitespace fail) and MaximumLength(NopExpensesDefaults.NameMaxLength = 200, new public const shared with TT-022); type must be a defined enum value (0 fails); limit >= 0 only when not null; no uniqueness rule. The controller trims the Name before validating. Not registered in DI (assembly scan). **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Rules enforced. Gap: no upper bound or precision rule on PerMonthLimit (BUG-002).

---

## TT-017: Controller: ExpenseCategoryInsert and ExpenseCategoryUpdate
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-004
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `ExpenseCategoryInsert(model)` (ModelState check, build entity with `LimitedToStores = false`, validate, insert; failures return `Json(new { fieldErrors })`) and `ExpenseCategoryUpdate(model)` (not found -> localized `ErrorJson` with `Validation.RecordNotFound`; apply Name and PerMonthLimit only; reject a different posted type with `Validation.TypeLocked`, accept an equal one; validate; update). Non-numeric `PerMonthLimit` binding errors map to the `PerMonthLimitInvalid` field error. `[CheckPermission]` on both.
- **Dependencies**: TT-013, TT-016
- **Verification (Definition of Done)**: Valid add persists with all-stores default; invalid add/update rejected with localized messages and nothing saved; different type rejected, same type accepted; type and stored Name never altered by anything but the update's allowed fields; direct calls validated identically to the UI.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.6; AC4.1-4.9, AC4.13. Build stage: Stage 4 - Read-only grid UI, controller read tests, modal models. **Implementation (2026-10-04):** ExpenseCategoryController gained ExpenseCategoryInsert and ExpenseCategoryUpdate, both [HttpPost] + [CheckPermission(ManageExpenseCategories)] under the class-level [Area]/[AuthorizeAdmin]/[AutoValidateAntiforgeryToken]. Constructor gained an ExpenseCategoryValidator parameter (time-log precedent); the T-003 list test fixture was updated for it. Both take ExpenseCategoryModel. Insert builds the entity with LimitedToStores = false and a trimmed Name. Update loads by id (RecordNotFound via ErrorJson), rejects posted type != stored type with TypeLocked before anything else, validates a candidate copy (so a rejected request never mutates the loaded entity) and then applies Name and PerMonthLimit only. A PerMonthLimit ModelState error (non-numeric binding) maps to the localized PerMonthLimitInvalid field error. Failures return Json(new { fieldErrors }) keyed by property name (Name, PerMonthLimit, ExpenseCategoryTypeId); success returns NullJsonResult. Note: a stale client that omits the type is rejected as TypeLocked by design. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Type lock and field errors covered with mocked ModelState. Related: BUG-001, BUG-002, BUG-009.

---

## TT-018: Add panel, inline edit and expensecategory-grid.js
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-004
- **Linked Jira ID**: SCRUM-38
- **Description**: Extend `List.cshtml` with the "Add new" panel (Name, Per Month Limit, Type dropdown Expense/Income with required marker, Save, per-field error containers with `role="alert"`/`aria-describedby`); add `UrlUpdate` and `Editable` on Name and Per Month Limit only (Type not `Editable`); add `Content/expensecategory-grid.js` (Add submit, `fieldErrors` display, grid reload, antiforgery token). Verify stock Edit/Confirm/Cancel controls are keyboard reachable and add locale `aria-label`s if not.
- **Dependencies**: TT-014, TT-017
- **Verification (Definition of Done)**: Add saves and the grid refreshes with no navigation; inline edit changes Name and limit only; type read-only in edit mode; errors shown per field; all controls keyboard operable.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.2 (2, 3), 5.2 script. Build stage: Stage 5 - Insert/update/modal controller actions, Add panel, tests. Modal open/save script is added to this same file by TT-023. Needs a type-less submit attempt and a negative limit for manual verification. **Implementation (2026-10-04):** List.cshtml: Add new panel (Name with maxlength 200, Per Month Limit as a text input so non-numeric input reaches the server, Type dropdown with a "Select a type" placeholder option 0 and Expense/Income from the enum locale keys, required markers, labels, aria-describedby and role=alert error containers, Save button, polite status region); UrlUpdate set; Name and PerMonthLimit columns Editable (EditType.String), Type column not Editable; stock RenderButtonsInlineEdit column (Edit/Update/Cancel are text-labelled anchors, so keyboard reachable with accessible names; no extra aria-labels needed). The limit column Data is the raw PerMonthLimit with a custom render showing PerMonthLimitDisplay inside a span carrying data-raw (admin-culture decimal separator); expensecategory-grid.js replaces the edit input value with data-raw, because the framework fills the input from the cell markup. Also fixed T-003 gap: the Search panel row had no data-hideAttribute (collapse click would throw in SavePreference); both panels now carry one and persist their collapsed state. Script (Content/expensecategory-grid.js): Add submit with per-field errors and grid reload via updateTable, ajaxPrefilter appending the row type to ExpenseCategoryUpdate, display_nop_error override that shows an inline update fieldErrors response in a role=alert region above the grid (the framework redraws the grid right after, so errors cannot stay on the row). The csproj globs already include the new js. Not verified at runtime. **QA Result (2026-10-04):** Script and view reviewed; not executed. Related: BUG-004, BUG-005, BUG-007, BUG-008.

---

## TT-019: xUnit: ExpenseCategoryValidator tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-004
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Validators/ExpenseCategoryValidatorTests.cs` (pattern of `ProjectValidatorTests`): name required/blank/201 chars/200 chars OK, duplicate names accepted (no uniqueness rule), type 0 and undefined value rejected, null/0/positive limit accepted, negative rejected, same limit rules for Income and Expense.
- **Dependencies**: TT-016, TT-002
- **Verification (Definition of Done)**: All tests pass.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7; AC4.3, AC4.7-4.12. Build stage: Stage 3 - Menu, models, controller read path, validator. **Implementation (2026-10-04):** Tests/Validators/ExpenseCategoryValidatorTests.cs, 17 test cases: valid; null, empty and whitespace name; 201 chars fails and 200 passes; duplicate names accepted; type 0, 3, -1 and 99 rejected; both types accepted; null, 0, small and 4-decimal limits pass for both types; negative limit fails for both types. Localization mocked to return the key. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). 17 validator cases pass; no test covers an over-range or over-precision limit (BUG-002).

---

## TT-020: xUnit: controller insert/update tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-004
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Controllers/ExpenseCategoryControllerEditTests.cs`: insert sets `LimitedToStores = false`; update with a different type returns the `TypeLocked` error and calls no update; update with the same type succeeds; stored type never assigned from the model; non-numeric limit `ModelState` error maps to the `PerMonthLimitInvalid` field error; not-found update returns the localized error.
- **Dependencies**: TT-017, TT-002
- **Verification (Definition of Done)**: All tests pass.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7; AC4.5, AC4.8, AC4.9, AC4.13. Build stage: Stage 5 - Insert/update/modal controller actions, Add panel, tests. Mirrors `TimeLogControllerTests` mocking approach. **Implementation (2026-10-04):** Tests/Controllers/ExpenseCategoryControllerEditTests.cs, 15 test cases: insert persists trimmed name and LimitedToStores = false (ignoring a posted true), empty limit, missing type, blank name, negative limit, non-numeric ModelState error mapped to the localized field error, none saves on failure; update not found, different type and missing type give TypeLocked with no update call and the entity untouched, same type updates Name and limit only and keeps type and LimitedToStores, clearing the limit, over-length name leaves the entity untouched, non-numeric ModelState error and negative limit give the field error and no save. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). 15 controller edit tests pass; ModelState is mocked, so real binder culture behaviour is not exercised (BUG-001).

---

# Tasks for T-005

## TT-021: Modal models (stores and translations)
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-005
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Models/Admin/ExpenseCategoryStoresAndTranslationsModel : BaseNopEntityModel, IStoreMappingSupportedModel, ILocalizedModel<ExpenseCategoryLocalizedModel>` (`Name` read-only reference, `SelectedStoreIds`, `AvailableStores`, `Locales`) and `ExpenseCategoryLocalizedModel : ILocalizedLocaleModel` (`LanguageId`, `Name`).
- **Dependencies**: TT-003, TT-012
- **Verification (Definition of Done)**: Compile; display names resolve to locale keys.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.6 (Models). Build stage: Stage 4 - Read-only grid UI, controller read tests, modal models. **Implementation (2026-10-04):** Models/Admin/ExpenseCategoryStoresAndTranslationsModel.cs (BaseNopEntityModel, IStoreMappingSupportedModel, ILocalizedModel<ExpenseCategoryLocalizedModel>; Name read-only reference, SelectedStoreIds, AvailableStores, Locales; no limit or type, so a modal save cannot change them) and ExpenseCategoryLocalizedModel.cs (ILocalizedLocaleModel: LanguageId, Name, plus display-only LanguageName used for the field label). Records, XML docs, no new locale keys. **QA Result (2026-10-04):** Modal model carries no limit or type.

---

## TT-022: Controller: EditStoresAndTranslations and SaveStoresAndTranslations
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-005
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `EditStoresAndTranslations(id)` (GET partial: `IStoreMappingSupportedModelFactory.PrepareModelStoresAsync` + `ILocalizedModelFactory.PrepareLocalizedModelsAsync` using `GetLocalizedAsync`) and `SaveStoresAndTranslations(model)` (load by id, validate each non-empty translation <= 200 with `NameTooLong` before writing anything, `IStoreMappingService.SaveStoreMappingsAsync(category, SelectedStoreIds)`, `ILocalizedEntityService.SaveLocalizedValueAsync(category, c => c.Name, value, languageId)` per locale, set `UpdatedOnUtc`, `RemoveByPrefixAsync` cache prefix, JSON result). Never touches stored Name, limit or type. `[CheckPermission]` on both. No direct repository writes.
- **Dependencies**: TT-013, TT-016, TT-021
- **Verification (Definition of Done)**: Selecting stores limits the category and clearing all stores returns it to all stores; translations persist per language and empty ones fall back to the default; over-200 translation rejects the whole save with nothing stored; stored Name, limit and type unchanged; direct unauthorized calls denied.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 4.2, 4.4, 4.6; AC5.1, AC5.3, AC5.5-5.10. Build stage: Stage 5 - Insert/update/modal controller actions, Add panel, tests. `SaveStoreMappingsAsync` in 4.90.8 also flips `LimitedToStores` itself (verified in `StoreMappingService`), so the controller must not set it separately. **Implementation (2026-10-04):** ExpenseCategoryController gained EditStoresAndTranslations (GET, returns partial _StoresAndTranslationsBody; missing id returns ErrorJson RecordNotFound) and SaveStoresAndTranslations (POST). Save order: ModelState check, load (RecordNotFound), validate every translation first (trimmed, max NopExpensesDefaults.NameMaxLength, error key `Locales[i].Name` with the NameTooLong text, nothing stored on any error), then SaveStoreMappingsAsync (no separate LimitedToStores), SaveLocalizedValueAsync per language (empty value is passed through, which makes the service delete the translation so the default name applies), then UpdateExpenseCategoryAsync (UpdatedOnUtc and the plugin cache prefix). Hardening beyond the design: posted store ids are intersected with existing stores (an unknown id would otherwise set LimitedToStores with no mapping and hide the category everywhere) and unknown language ids are ignored (no orphan LocalizedProperty rows). Stored Name, limit and type are never assigned. Constructor gained 6 dependencies (ILanguageService, ILocalizedEntityService, ILocalizedModelFactory, IStoreMappingService, IStoreMappingSupportedModelFactory, IStoreService); existing controller test fixtures were updated. [CheckPermission] on both, class-level [AuthorizeAdmin] and [AutoValidateAntiforgeryToken] apply (antiforgery validates the POST; the GET is a safe read). **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). Validate-first, store/translation save, hardening for unknown stores and languages confirmed in code and tests.

---

## TT-023: Modal partial view and modal script
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-005
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `Views/ExpenseCategory/_EditStoresAndTranslations.cshtml` (store multi-select using the same select2 usage as time-log's `Project/_CreateOrUpdate.cshtml`, read-only default Name reference with `DefaultNameHint`, one Name field per language, Save, Cancel) and the Bootstrap modal shell `#expensecategory-stores-translations-modal` in `List.cshtml` plus the per-row "Edit stores and translations" button; extend `Content/expensecategory-grid.js` (AJAX-load partial, post to save, close, reload grid, show errors). Focus trap, Escape closes, focus returns to the opening button, `aria-labelledby` on the title, labels on every field, accessible names on icon-only buttons. Verify the localized-editor tag helper works inside an AJAX-loaded partial; if not, use the admin popup (`_AdminPopupLayout`) fallback and record it in Notes.
- **Dependencies**: TT-022, TT-018
- **Verification (Definition of Done)**: Modal opens on the same page, saves, closes and the grid shows the updated Stores indicator; keyboard-only operation, focus return and Escape work; store filter reflects the change (AC5.4).
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 5.2 (4), accessibility paragraph; AC5.1-5.4. Build stage: Stage 6 - Modal UI. Needs demo data: at least two stores and two installed languages. WCAG 2.1 AA applies. **Implementation (2026-10-04):** Views/ExpenseCategory/_StoresAndTranslationsModal.cshtml (Bootstrap 4 modal shell: role=dialog, aria-modal, aria-labelledby on the title, close button with Accessibility.CloseDialog label, URLs and saved text in data attributes; rendered from List.cshtml), Views/ExpenseCategory/_StoresAndTranslationsBody.cshtml (form returned by the GET: read-only default Name with DefaultNameHint via aria-describedby, store multi-select, one labelled Name field per language with maxlength 200 and an error container, Cancel and Save), List.cshtml (new per-row button column using RenderCustom, with aria-label that appends the category name), Content/expensecategory-grid.js (delegated open, AJAX load, focus to first control on shown, Escape/backdrop/close via Bootstrap, focus returned to the opener (looked up by id because the grid may redraw), save with serialize() plus antiforgery token, per-field errors, modal closes and grid reloads with a status message). Deviations from the design text: (1) the localized-editor tag helper (Html.LocalizedEditorAsync) was NOT used. It renders a tabbed UI driven by page-level script, which is not dependable in an AJAX-loaded fragment, and I did not verify it at runtime; per-language fields are a plain labelled list instead, so the _AdminPopupLayout fallback was not needed. (2) The store picker is a native `<select multiple>` instead of select2, because select2 inside a Bootstrap modal needs dropdownParent handling and is less reliable for keyboard and screen reader use. Neither the Razor views nor the script were run: dotnet build does not compile Razor views (only `node --check` on the script). First render, focus trap, Escape and focus return must be checked in TT-029 / QA, with two stores and two languages. **QA Result (2026-10-04):** Razor and JS reviewed; not executed; Bootstrap provides focus trap and Escape. Related: BUG-004, BUG-006, BUG-007, BUG-008.

---

## TT-024: xUnit: modal save tests
- **Type**: Task
- **Placement**: nop-plugin
- **Status**: Done
- **Parent**: T-005
- **Linked Jira ID**: SCRUM-38
- **Description**: Add `src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Controllers/ExpenseCategoryControllerModalTests.cs`: save calls `SaveStoreMappingsAsync` and one `SaveLocalizedValueAsync` per locale; over-200 translation returns the localized error and calls neither service; empty translation allowed; stored Name, limit and type are unchanged after save; not-found id returns the localized error; load action returns models for every language.
- **Dependencies**: TT-022, TT-002
- **Verification (Definition of Done)**: All tests pass.
- **Notes**: task-type: new-plugin. Target nopCommerce 4.90.8. Design ref: Design 7; AC5.8, AC5.9. Build stage: Stage 5 - Insert/update/modal controller actions, Add panel, tests. **Implementation (2026-10-04):** Tests/Controllers/ExpenseCategoryControllerModalTests.cs, 16 test cases (75 tests pass in total across the plugin test project): save calls SaveStoreMappingsAsync, SaveLocalizedValueAsync per language and UpdateExpenseCategoryAsync and leaves Name, limit and type untouched; translation key is the Name property; no stores selected saves an empty list (AC5.5); unknown store ids dropped; empty/whitespace translation passed as empty (AC5.7); 200 characters accepted, 201 rejected with `Locales[1].Name` and nothing stored even for a valid sibling language (AC5.8); trimmed length is counted; unknown language ignored; not found and invalid ModelState store nothing; GET returns the partial with stored name, per-language values (empty when untranslated) and calls PrepareModelStoresAsync. Existing list and edit test fixtures updated for the new constructor arguments. **QA Result (2026-10-04):** PASS (automated tests; build 0 errors; 94 Expenses and 80 TimeLog tests green). 16 modal tests pass.

---

# Bugs (QA 2026-10-04, nopCommerce 4.90.8)

QA basis: build `NopCommerce.sln` 0 errors; Expenses tests 94/94 passed; TimeLog tests 80/80 passed; static code review of controller, service, validator, plugin class, three Razor views and `expensecategory-grid.js`. The site is not installed in the QA environment, so no bug below was reproduced at runtime; each states how it was established. No Critical or High defect was found. All bugs are `nop-plugin`; none is a core-safety bug. Full detail and AC coverage: `docs/expense-category/qa/qa-report.md`.

## BUG-001: Per Month Limit accepts group separators and silently misreads them
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-004 (AC4.7 value stored as entered, AC4.8 non-numeric rejected); also TT-017, TT-020
- **Severity**: Medium
- **Description**: `ExpenseCategoryModel.PerMonthLimit` is a `decimal?` bound by the stock ASP.NET Core decimal model binder under the request culture (nopCommerce sets it from the admin language via `UseNopRequestLocalization`). That binder parses with `NumberStyles.Float | AllowThousands`, so a group separator is accepted and ignored instead of rejected. The Add panel and the inline edit input are free text (`type="text"`), so any typed value reaches the binder. Established by code review of `ExpenseCategoryController.ExpenseCategoryInsert/Update`, `List.cshtml` and the framework `Table.cshtml` (inline edit posts the raw input string); not reproduced at runtime. The unit tests mock `ModelState`, so the real binder is never exercised.
  - **Reproduction (needs runtime)**: (1) Admin language en-US: Add a category with Per Month Limit `1,5`. (2) Admin language de-DE (decimal separator comma, group separator dot): Add a category with Per Month Limit `1.5`, or inline edit a limit to `1.5`. Expected: `1,5` in en-US is rejected with the localized message (or read as 1.5); `1.5` in de-DE is read as 1.5 or rejected. Actual (by code review): saved as 15 with no error, a 10x error the user cannot see until they read the grid.
  - AC reference: AC4.7 (stored as entered), AC4.8 (non-numeric rejected), AC4.9 (direct requests rejected the same way).
- **Notes**: Functional bug (data integrity). Suggested fix: parse the posted limit explicitly (for example bind to a string and parse with `NumberStyles.Number & ~NumberStyles.AllowThousands` under the admin culture, reject anything else with `PerMonthLimitInvalid`), and add a test that posts real strings. Re-run AC4.6 to AC4.9 after the fix. **Fix (2026-10-04, nopcommerce-developer):** `ExpenseCategoryModel.PerMonthLimit` now binds through a plugin-owned `StrictNullableDecimalModelBinder` (Models/Binding): plain number in the request culture's own format only (optional sign, decimal point; no group separators, exponent, currency symbol). `1,5` in en and `1.5` in de are now a model state error, mapped by the controller to the localized `PerMonthLimitInvalid` field error; empty binds to null. New tests post real strings under en-US and de-DE (StrictNullableDecimalModelBinderTests, 22 cases). Files: Models/Binding/StrictNullableDecimalModelBinder.cs (new), Models/Admin/ExpenseCategoryModel.cs. Values beyond decimal range also get PerMonthLimitInvalid. Not verified at runtime: that MVC picks up the property-level binder and that the request culture supplies the culture (needs the installed site; re-run Reproduction steps 1 and 2). **QA Re-verification (2026-10-04): VERIFIED, Done.** By test: 145 Expenses tests pass, including StrictNullableDecimalModelBinderTests (en-US and de-DE strings). Beyond the developer's tests, QA drove the real ASP.NET Core MVC pipeline (throwaway probe outside the repo: DefaultModelBinderFactory, ParameterBinder, FormValueProvider with the culture, real ExpenseCategoryModel, all four fields posted): the property-level `[ModelBinder]` IS applied to PerMonthLimit (property metadata BinderType = StrictNullableDecimalModelBinder). en-US: `1.5` binds 1.5; `1,5`, `1,000.50`, `abc` give a PerMonthLimit ModelState error; empty gives null. de-DE: `1,5` binds 1.5; `1.5` and `1.000,50` give a PerMonthLimit ModelState error; empty gives null; `100000000000000` binds (then the validator rejects it, BUG-002). The controller maps the PerMonthLimit ModelState error to the localized field error (BUG-009 path). Not covered: the Nop host pipeline (request culture from the culture cookie, the added Nop binder providers; NopModelBinderProvider only handles CustomProperties and string, so it cannot intercept decimal). Runtime still needed: Reproduction steps 1 and 2 on the installed site, and that the page-render culture and POST culture match (the inline edit raw value uses `CultureInfo.CurrentCulture`, the same culture the binder uses).

## BUG-002: Per Month Limit has no upper bound or precision rule (database overflow, silent rounding)
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-004 (AC4.7, AC4.9); also TT-016, TT-019
- **Severity**: Medium
- **Description**: The column is `decimal(18,4)` (`ExpenseCategoryBuilder`) but `ExpenseCategoryValidator` only checks `>= 0`. A value with more than 14 integer digits (for example `100000000000000`) passes validation and the insert/update then fails in the database with an arithmetic overflow, returning an unhandled 500 instead of the localized field error. A value with more than 4 decimals (for example `1.23456`) passes validation and is stored rounded to 4 decimals, so it is not "stored as entered". Established by code review; the overflow depends on SQL Server behaviour and must be confirmed at runtime.
  - **Reproduction (needs runtime)**: POST `ExpenseCategoryInsert` (UI Add panel or direct) with Name `X`, type 1, PerMonthLimit `100000000000000`. Expected: field error `PerMonthLimit` with the localized message and nothing saved (AC4.8, AC4.9). Actual (expected from code): unhandled exception, HTTP 500, generic error. Second case: PerMonthLimit `1.23456` expected stored as entered or rejected; actual (expected): stored as 1.2346.
  - AC reference: AC4.7, AC4.8, AC4.9.
- **Notes**: Functional bug. Suggested fix: add `LessThanOrEqualTo(max)` and a scale rule to the validator (shared constant next to `NameMaxLength`) with a localized message; add validator tests for the boundary values. **Fix (2026-10-04, nopcommerce-developer):** `ExpenseCategoryValidator` now rejects a limit above 99999999999999.9999 (new `NopExpensesDefaults.PerMonthLimitMaxValue`) with `PerMonthLimitTooLarge`, and a limit with more than 4 decimals (`PerMonthLimitMaxScale`; trailing zeros allowed) with `PerMonthLimitTooManyDecimals`, both as localized `PerMonthLimit` field errors, so the DB overflow and silent rounding cannot be reached through Add or inline edit. New locale keys PerMonthLimitTooLarge and PerMonthLimitTooManyDecimals (with {0} placeholders filled from the constants) are in the inventory under the deleted `Admin.Expenses` prefix, so uninstall symmetry holds (lifecycle test still proves every key is removed). Tests: boundary and rounding cases in ExpenseCategoryValidatorTests, insert/update controller tests, locale placeholder tests. Files: NopExpensesDefaults.cs, Validators/ExpenseCategoryValidator.cs, ExpensesPlugin.cs. Real SQL Server behaviour not exercised. **QA Re-verification (2026-10-04): VERIFIED, Done.** By test and review: validator bounds checked against the column: PerMonthLimitMaxValue 99999999999999.9999 is exactly the decimal(18,4) maximum (14 integer digits, 4 decimals); `LessThanOrEqualTo` runs only for non-negative values (negatives get only the range error); scale rule `Math.Round(x, 4) == x` allows trailing zeros (1.50000) and rejects 1.23456; tests cover 100000000000000, 99999999999999.99995, decimal.MaxValue, 1.2345, 1.50000, 0.0001, 1.23456, 0.00001. Values that overflow decimal itself are stopped earlier by the binder (BUG-001). Messages are formatted from the constants (no hardcoded numbers). Runtime still needed: the Add and inline-edit flows with a boundary value on SQL Server.

## BUG-003: Uninstall leaves per-customer GenericAttribute rows behind
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-002 (AC2.5 and the Install/Uninstall symmetry rule); also TT-025, TT-028
- **Severity**: Low
- **Description**: `List.cshtml` declares `data-hideAttribute` names `ExpenseCategoryPage.HideSearchBlock` and `ExpenseCategoryPage.HideAddPanel`. The core `Admin/Preferences/SavePreference` action stores each as a `GenericAttribute` row (KeyGroup `Customer`) whenever an admin collapses or expands a panel. `ExpensesPlugin.UninstallAsync` removes the permission, locale resources, StoreMapping and LocalizedProperty rows, but not these rows, so uninstall is not fully symmetric. Established by code review of `ExpensesPlugin.cs`, `List.cshtml` and `PreferencesController.SavePreference`.
  - **Reproduction (needs runtime)**: Install, open Expense Category, collapse the Add new panel, uninstall, then query `GenericAttribute` for `Key LIKE 'ExpenseCategoryPage.%'`. Expected: zero rows. Actual (expected from code): one row per customer and key that was toggled.
  - AC reference: AC2.5 (and CLAUDE.md Plugin Lifecycle symmetry; security standards section 5).
- **Notes**: Plugin-lifecycle bug (hygiene, low impact). Suggested fix: delete `GenericAttribute` rows whose `Key` starts with `ExpenseCategoryPage.` in `UninstallAsync`, add the key prefix to a constant, extend `ExpensesPluginLifecycleTests`. Re-run the install, uninstall, reinstall cycle afterwards. **Fix (2026-10-04, nopcommerce-developer):** `ExpensesPlugin.UninstallAsync` now also deletes `GenericAttribute` rows with `KeyGroup == Customer` and `Key` starting with `ExpenseCategoryPage.` (via `IRepository<GenericAttribute>` predicate delete, same pattern as the StoreMapping and LocalizedProperty cleanup; the time-log plugin has no equivalent, and the core does not remove plugin page preferences). The two key names and the prefix are now constants in NopExpensesDefaults and List.cshtml uses them, so view and uninstall cannot drift. Lifecycle tests updated (new ctor argument, cleanup order including the new step, predicate matches only the two Customer keys and not ProjectPage.* or another entity, constants share the prefix). Files: ExpensesPlugin.cs, NopExpensesDefaults.cs, Views/ExpenseCategory/List.cshtml. Real row deletion needs the install, uninstall, reinstall cycle on a database (TT-029 step 9). **QA Re-verification (2026-10-04): VERIFIED by test and code review, Done; real deletion needs runtime.** UninstallAsync order: DeletePermissionAsync, StoreMapping delete, LocalizedProperty delete, GenericAttribute delete (KeyGroup == Customer and Key starts with the constant `ExpenseCategoryPage.`), then the three locale deletions, base.UninstallAsync() last, so all row deletes run before the migration-down drops the table. The GenericAttribute predicate is a single parameterized IRepository delete, scoped to Customer rows with that prefix (the lifecycle test proves it does not match ProjectPage.*, OtherExpenseCategoryPage.* or a non-Customer group). The two keys in List.cshtml and the uninstall prefix come from the same constants. Runtime still needed: TT-029 step 9 (collapse a panel, uninstall, query GenericAttribute for `ExpenseCategoryPage.%`, expect zero rows; then reinstall).

## BUG-004: Add/save success message is hidden and not announced when the Add panel is collapsed
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-005 (AC5.3) and T-004 (AC4.1); WCAG 2.1 SC 4.1.3; also TT-018, TT-023
- **Severity**: Low
- **Description**: The polite status region `#expensecategory-status` is rendered inside `#expensecategory-add-panel`. `expensecategory-grid.js` also writes the "stores and translations saved" message into it. The panel's collapsed state persists per customer (`ExpenseCategoryPage.HideAddPanel`), and a collapsed panel is `display:none`, so a user who collapsed it gets no visible or announced confirmation after saving the modal. The collapse header is also a `div` with a click handler (stock admin pattern), which keyboard users cannot activate to expand it. Established by code review.
  - **Reproduction (needs runtime)**: Collapse the Add new panel with the mouse, reload, open a row's "Edit stores and translations", save. Expected: a visible, announced confirmation. Actual (expected): modal closes, grid reloads, no message shown.
  - AC reference: AC5.3 (confirmation of a successful save), accessibility rules in CLAUDE.md.
- **Notes**: Accessibility bug. Suggested fix: move the status region outside the collapsible panel (next to `#expensecategory-grid-errors`). **Fix (2026-10-04, nopcommerce-developer):** the polite status region `#expensecategory-status` moved out of the collapsible Add panel to the grid card, next to `#expensecategory-grid-errors`, so the Add and modal-save confirmations show and are announced with the panel collapsed. The collapse header being a click-only div is the stock admin pattern and was not changed. View-only change (Razor and JS are not compiled or executed by the build); confirm at runtime per the Reproduction steps. Files: Views/ExpenseCategory/List.cshtml. **QA Re-verification (2026-10-04): VERIFIED by code review only, Done; runtime pending.** `#expensecategory-status` (role=status, aria-live=polite) now sits in the grid card next to `#expensecategory-grid-errors`, outside `#expensecategory-add-panel`, so it is not hidden when the Add panel is collapsed; both the Add success and the modal save write to it via setRegion. The region exists at page load (needed for live-region announcement). Runtime still needed: collapse the Add panel, reload, save in the modal, confirm the message shows and a screen reader announces it. The click-only collapse header is the stock admin pattern and was left as is (not filed).

## BUG-005: Focus is lost when inline Edit is activated
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-004 (AC4.2, keyboard use); WCAG 2.1 SC 2.4.3; also TT-018
- **Severity**: Low
- **Description**: The stock inline edit hides the activated Edit link (`$('#buttonEdit_...').hide()`) and shows Update and Cancel; the plugin wraps `editData_expensecategory_grid` but only swaps the limit input value, so focus is never moved. After pressing Enter on Edit, the focused element disappears and focus falls back to the document start; keyboard users must tab back through the whole page to reach the edit inputs. Established by code review of `Table.cshtml` and `expensecategory-grid.js`.
  - **Reproduction (needs runtime)**: Tab to a row's Edit link, press Enter. Expected: focus in the Name input of that row. Actual (expected): focus lost.
  - AC reference: AC4.2, CLAUDE.md "keyboard navigation for all interactive elements".
- **Notes**: Accessibility bug. Suggested fix: in the existing `editData_` wrapper, focus the row's first `input.userinput`; consider Escape to cancel. **Fix (2026-10-04, nopcommerce-developer):** the existing `editData_expensecategory_grid` wrapper now moves focus to the row's first `input.userinput` after the framework swaps Edit for Update/Cancel. JS only (not executed by the build); confirm by keyboard at runtime. Escape-to-cancel was not added. Files: Content/expensecategory-grid.js. **QA Re-verification (2026-10-04): VERIFIED by code review only, Done; runtime pending.** The wrapper matches the framework signature `editData_<table>(dataId /*jQuery row*/, data)` (Table.cshtml line 181; the Edit control is an `<a href="#" onclick="editData_...($(this).parent().parent(), id);return false;">` in _Table.Definition.cshtml, resolved by global name at click time, so reassigning `window.editData_expensecategory_grid` takes effect). The framework swaps the cells to `input.userinput` synchronously inside originalEdit, so the focus call after it finds the Name input. Residual, filed separately: focus is still lost after Update or Cancel (see BUG-012). Runtime still needed: keyboard activation of Edit puts focus in the Name input.

## BUG-006: Store multi-select has no usage or "empty means all stores" instruction
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-005 (AC5.2, AC5.5); WCAG 2.1 SC 3.3.2; also TT-023
- **Severity**: Low
- **Description**: `_StoresAndTranslationsBody.cshtml` uses a native `<select multiple>` labelled "Limited to stores" with no help text. Selecting several stores or clearing the selection needs Ctrl/Shift (mouse) or Ctrl+Space (keyboard), and nothing tells the user that selecting no store makes the category available in all stores (AC5.5). Established by code review.
  - **Reproduction (needs runtime)**: Open the modal for a limited category and try to clear all stores by keyboard only. Expected: instructions available. Actual (expected): none, behaviour discoverable only by trial.
  - AC reference: AC5.2, AC5.5.
- **Notes**: Accessibility/usability bug. Suggested fix: add a localized hint (`aria-describedby`) such as "Select none to make the category available in all stores. Use Ctrl to select several." **Fix (2026-10-04, nopcommerce-developer):** added a localized hint under the store multi-select (`StoresAndTranslations.StoresHint`: select none for all stores; Ctrl/Command click or Ctrl+Arrow and Ctrl+Space for several), linked with `aria-describedby`. View and locale only; confirm at runtime with a screen reader. Files: Views/ExpenseCategory/_StoresAndTranslationsBody.cshtml, ExpensesPlugin.cs. **QA Re-verification (2026-10-04): VERIFIED by code review and locale tests, Done; runtime pending.** The hint `<small id="SelectedStoreIds-hint">` is rendered from the new key `StoresAndTranslations.StoresHint`, linked to the select by `aria-describedby="SelectedStoreIds-hint"`; the text covers both "select none = all stores" and the multi-select gestures; the key is in the inventory under the deleted `Admin.Expenses` prefix. Runtime still needed: screen reader announces the hint on focus.

## BUG-007: Success status text uses text-success (contrast about 3.1:1)
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-004/T-005 accessibility rule (WCAG 2.1 SC 1.4.3, 4.5:1); also TT-014, TT-018, TT-023
- **Severity**: Low
- **Description**: `#expensecategory-status` uses the Bootstrap `text-success` class. In the stock Bootstrap/AdminLTE palette that is `#28a745` on white, about 3.1:1, below the 4.5:1 required for body text. Established by code review of the class and the default palette; the admin theme may override it, so confirm with a contrast tool at runtime.
  - AC reference: CLAUDE.md Accessibility (minimum 4.5:1).
- **Notes**: Accessibility bug (confirm at runtime first). Suggested fix: use a darker green or `text-dark` with a check icon. **Fix (2026-10-04, nopcommerce-developer):** safe change only: the status region no longer uses `text-success` (about 3.1:1) but `text-dark`, which is well above 4.5:1 on a white card in the stock palette. Contrast against the real admin theme was NOT measured: it needs a runtime check with a contrast tool (the theme may override Bootstrap colors). Files: Views/ExpenseCategory/List.cshtml. **QA Re-verification (2026-10-04): VERIFIED by code review only, Done; contrast not measured.** `text-success` is gone from the status region; it now uses `text-dark` (stock Bootstrap 4 #343a40 on a white card is about 11.9:1). The admin has no dark skin in this codebase (no dark-mode styles found), so a light-on-dark inversion is not a risk. Runtime still needed: measure the rendered colour with a contrast tool on the real admin theme.

## BUG-008: Hardcoded or raw user-facing text and dead locale keys
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-001 (AC1.3) and the CLAUDE.md "no hardcoded values"; also TT-010, TT-018, TT-023
- **Severity**: Low
- **Description**: (1) The English text of `Validation.NameTooLong` hard-codes "200 characters", which can drift from `NopExpensesDefaults.NameMaxLength`. (2) `expensecategory-grid.js` calls `alert(errorThrown)` in three AJAX error handlers, showing the raw browser/HTTP status text (not localized). (3) The keys `Accessibility.EditRow`, `SaveRow`, `CancelEdit` are installed but never used (the inline Edit/Update/Cancel links are all named "Edit", "Update", "Cancel" in every row, so screen reader users cannot tell rows apart). Established by code review.
  - AC reference: AC1.3, CLAUDE.md Coding Expectations.
- **Notes**: Minor localization and accessibility hygiene. Suggested fix: pass the limit as a placeholder into the message, use a localized generic error text from a data attribute, and either use or drop the dead keys. **Fix (2026-10-04, nopcommerce-developer):** (1) `NameTooLong` text is now `Name must not exceed {0} characters.`, filled from `NopExpensesDefaults.NameMaxLength` in the validator and in the modal save (tests assert the filled value 200); (2) the three `alert(errorThrown)` handlers now show the localized `Error.Generic` text (rendered as `data-generic-error` on the grid error region) in the alert regions, never the raw HTTP status text; (3) the unused keys Accessibility.EditRow, SaveRow, CancelEdit were dropped from the inventory (they cannot be applied: the Edit/Update/Cancel links are rendered by the framework's inline-edit buttons, so the per-row accessible-name gap for those three links remains open and would need a runtime-tested JS change). Locale tests added. Files: ExpensesPlugin.cs, Validators/ExpenseCategoryValidator.cs, Controllers/ExpenseCategoryController.cs, Content/expensecategory-grid.js, Views/ExpenseCategory/List.cshtml. Note: locale text is written on Install only, so a store that already installed an earlier build must reinstall to get the changed and new texts (the QA environment has no installed site). **QA Re-verification (2026-10-04): VERIFIED for items (1), (2) and the dead-key part of (3), Done; the accessible-name gap is tracked as BUG-011.** (1) NameTooLong text is `Name must not exceed {0} characters.` and both the validator and the modal save fill {0} from NopExpensesDefaults.NameMaxLength (tests assert 200). (2) No `alert(` remains in expensecategory-grid.js or the views; the error handlers write the localized `Error.Generic` text from `data-generic-error` into the alert region (Razor-encoded attribute, inserted as text). (3) Accessibility.EditRow, SaveRow and CancelEdit are no longer in the inventory and nothing references them. Locale symmetry: every `Admin.Expenses.*` key used in the views, controller and script exists in the inventory, and every inventory key sits under a prefix that uninstall deletes (lifecycle test). Observation (not filed): key `Admin.Expenses.ExpenseCategory.Updated` is installed but unused. Install writes locale text only once, so an already-installed store must reinstall to get changed texts (none exists here).

## BUG-009: Insert, Update and Save actions do not check ModelState.IsValid first
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Deviation from `.claude/instructions/project-standards/nopcommerce-security-standards.instruction.md` section 2 and CLAUDE.md Security; relates to T-004 (AC4.9); also TT-017
- **Severity**: Low
- **Description**: `ExpenseCategoryInsert` and `ExpenseCategoryUpdate` inspect only the `PerMonthLimit` ModelState key; other binding failures (for example a non-numeric `ExpenseCategoryTypeId` or `Id`) are coerced to 0 and caught indirectly by the validator or the not-found check. Today this does not produce wrong data, but it relies on coincidence and breaks the project rule "validate ModelState first in POST actions". `SaveStoresAndTranslations` does check it. Established by code review.
  - AC reference: AC4.9.
- **Notes**: Standards deviation. Suggested fix: check `ModelState.IsValid` first and map failures to field errors, keeping the existing limit mapping. **Fix (2026-10-04, nopcommerce-developer):** `ExpenseCategoryInsert` and `ExpenseCategoryUpdate` now check `ModelState` first, before any lookup, validation or write (helper `RejectInvalidModelStateAsync`): a binding failure on PerMonthLimit, ExpenseCategoryTypeId or Name returns the same localized field error the validator gives; a failure on any other key (for example Id) returns the localized `Validation.InvalidRequest` error. `SaveStoresAndTranslations` already checked first; it now returns the same localized InvalidRequest text instead of raw framework messages. New locale key InvalidRequest. Tests: insert and update binding failures on each key, Update with a bad Id is rejected before the lookup, nothing saved. Files: Controllers/ExpenseCategoryController.cs, ExpensesPlugin.cs. **QA Re-verification (2026-10-04): VERIFIED by test and code review, Done.** Insert and Update call RejectInvalidModelStateAsync first, before the lookup, type-lock, validation or any write; a failure on PerMonthLimit, ExpenseCategoryTypeId or Name returns the validator-equivalent localized field error (key = property name, which the Add panel map and the inline display_nop_error handle), any other key (for example Id) returns the localized InvalidRequest text; SaveStoresAndTranslations checks ModelState first and returns the same localized text. No implicit-required risk: nullable reference types are not enabled in this repo, so a missing Name does not become a ModelState error (the validator still gives NameRequired). Tests cover binding failures on each key, a bad Id rejected before the lookup, and nothing saved. Runtime still needed: confirm the stock inline update posts no extra key that fails binding (code review of Table.cshtml says it posts only Id, the Editable columns and the token, plus the type the script appends).

## BUG-010: Store filter does nothing when CatalogSettings.IgnoreStoreLimitations is ON
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Done
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Failure of T-003 (AC3.8) and T-005 (AC5.4); also TT-006
- **Severity**: Low
- **Description**: `ExpenseCategoryService.GetAllExpenseCategoriesAsync` filters through `IStoreMappingService.ApplyStoreMapping`, which returns the query unchanged when `CatalogSettings.IgnoreStoreLimitations` is true (verified in `StoreMappingService` line 95). With that core setting on, the Store filter drop-down is shown but ignored, contradicting AC3.8 and AC5.4. The developer recorded this as P5; it is not mentioned in the acceptance criteria. Established by code review.
  - **Reproduction (needs runtime)**: Enable "Ignore store limitations" in catalog settings, limit a category to store A, filter by store B. Expected: not listed (AC5.4). Actual (expected): listed.
  - AC reference: AC3.8, AC5.4.
- **Notes**: Needs a product decision: either document the dependency, or apply the mapping in the plugin query regardless of the setting. Re-run AC3.8 and AC5.4 under both setting values.

**Re-verification note (2026-10-04, QA):** BUG-010 deliberately left at Backlog (awaits a product decision, not part of this fix round).

**Fix (2026-10-04, nopcommerce-developer):** Product decision: the Store filter filters regardless of `CatalogSettings.IgnoreStoreLimitations`. `ExpenseCategoryService.GetAllExpenseCategoriesAsync` no longer calls `IStoreMappingService.ApplyStoreMapping`; when `storeId > 0` it applies `!LimitedToStores || mappedIds.Contains(Id)`, where `mappedIds` is a parameterized sub-select on `IRepository<StoreMapping>` (`EntityName = "ExpenseCategory"`, `StoreId = storeId`), so it runs as one query (no N+1). `storeId = 0` returns everything; name filter, ordering and the 100 page-size clamp are unchanged. The service constructor now takes `IRepository<StoreMapping>` instead of `IStoreMappingService` (DI auto-resolves; nothing else constructs it) and never reads CatalogSettings. Checked for dependents: the controller still uses `IStoreMappingService.SaveStoreMappingsAsync` for writes (unchanged), the Stores column and modal read `LimitedToStores` and the mapping via the stock factory, not the filter; the list view has no `IgnoreStoreLimitations` logic. Tests (ExpenseCategoryServiceTests): store filter for not limited, limited and mapped to the chosen store, limited and mapped only to another store, limited with no rows, a same-id mapping row of another entity, no mapping rows at all, stale rows on a not-limited category, "All stores", name plus store combined, clamp with a store set, and a constructor check that the service has no CatalogSettings or IStoreMappingService dependency (so the setting on or off cannot change the result). No core change, no migration. Files: src/Plugins/Nop.Plugin.Misc.Expenses/Services/ExpenseCategoryService.cs, src/Plugins/Nop.Plugin.Misc.Expenses.Tests/Services/ExpenseCategoryServiceTests.cs; docs updated: acceptance-criteria.md (AC3.8, AC5.4), technical-design.md (service section), implementation-plan.md (P5). Runtime check still needed: with two stores, limit a category to store A, filter by store B (not listed) and A (listed), with Ignore store limitations ON and OFF; and verify the SQL runs on the real provider (the translation of `Contains` on a sub-select is covered only by in-memory tests). Awaiting QA re-verification.

**QA Re-verification (2026-10-04, nopcommerce-qa-tester):** PASS, status stays Done. Verified by code review plus 149/149 unit tests (the store tests use in-memory LINQ; no SQL provider). Semantics confirmed: a chosen store lists unlimited plus mapped-to-that-store categories and nothing else (stale rows on an unlimited category ignored, rows of another entity ignored, limited with no rows hidden, no mappings at all hides limited ones); storeId 0 lists all; entity name `nameof(ExpenseCategory)` equals the `typeof(T).Name` used by `SaveStoreMappingsAsync`; clamp unchanged. One statement plus the standard count from `ToPagedListAsync`; `IQueryable.Contains(id)` as IN (sub-select) has a core precedent (`ForumService` line 480). Still runtime-only: real SQL Server execution, and the two-store scenario with Ignore store limitations ON and OFF. New doc defect: BUG-013 (Low).

## BUG-011: Inline Edit, Update and Cancel links have the same accessible name in every row
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Backlog
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Residual of BUG-008 item (3) (the developer dropped the unused Accessibility.EditRow/SaveRow/CancelEdit keys and recorded that the gap stays open); WCAG 2.1 SC 2.4.6 and 4.1.2; also TT-018
- **Severity**: Low
- **Description**: The inline edit controls are rendered by the framework's `RenderButtonsInlineEdit` as `<a href="#">` links whose text is just "Edit", "Update" and "Cancel". With one row per category, a screen reader link list shows many identical "Edit" links with no way to tell which category each acts on (the Stores and translations button already appends the category name, so the page is inconsistent). Established by code review of `_Table.Definition.cshtml` line 413 and `List.cshtml`; not reproduced at runtime.
  - **Reproduction (needs runtime)**: With two or more categories, open the screen reader links list or tab through the grid. Expected: each Edit, Update and Cancel link identifies its row (for example "Edit Travel"). Actual (expected from code): identical "Edit" links.
  - AC reference: AC4.2 keyboard and screen reader use; CLAUDE.md Accessibility (ARIA labels).
- **Notes**: Accessibility bug, functional path. Suggested fix: in the plugin script, add a row-specific `aria-label` to the three links after each draw (`drawCallback` or a delegated handler), using a localized label with the category name; test at runtime. Not a core change (the links are rendered by the stock partial; do not edit it).

## BUG-012: Focus is lost again after inline Update or Cancel
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Backlog
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Residual of BUG-005 (focus moved into the row on Edit, but not out of it); WCAG 2.1 SC 2.4.3; also TT-018
- **Severity**: Low
- **Description**: `confirmEditData_<table>` hides the Update and Cancel links (Table.cshtml lines 265 to 267) and the grid is then redrawn on success; Cancel hides the same links. The focused element disappears and focus falls back to the document start; the BUG-005 fix only handles the Edit step. Escape-to-cancel is also absent. Established by code review of Table.cshtml and `expensecategory-grid.js`; not reproduced at runtime.
  - **Reproduction (needs runtime)**: Keyboard only: Edit a row, change the name, activate Update (or Cancel). Expected: focus lands on that row's Edit link (or the grid) and the result is announced. Actual (expected from code): focus is lost and the user restarts from the top of the page.
  - AC reference: AC4.2, CLAUDE.md "keyboard navigation for all interactive elements".
- **Notes**: Accessibility bug. Suggested fix: wrap `confirmEditData_expensecategory_grid` and `cancelEditData_expensecategory_grid` the same way as `editData_` and refocus the row's Edit link by row id after the redraw. Confirm at runtime.

## BUG-013: Docs still say the store filter needs IgnoreStoreLimitations OFF
- **Type**: Bug
- **Placement**: nop-plugin
- **Status**: Backlog
- **Linked Jira ID**: SCRUM-38
- **Dependencies**: Doc leftover of BUG-010 (the decision made the filter independent of the setting)
- **Severity**: Low
- **Description**: The BUG-010 fix updated AC3.8, AC5.4, the technical design and plan note P5, but other places still describe the superseded behaviour: `implementation-plan.md` line 122 (TT-006 version/risk note: ApplyStoreMapping is a no-op, "AC3.8 testing must run with that setting false") and line 158 (prerequisite "IgnoreStoreLimitations = false for store-filter checks"); `tickets.md` TT-006 description (line 336, "ApplyStoreMapping when storeId > 0") and TT-029 step (f) ("store filter with CatalogSettings.IgnoreStoreLimitations OFF"). A tester following TT-029 would not run the ON case that BUG-010 exists to prove.
  - **Expected**: those lines say the service uses its own StoreMapping sub-select, and TT-029 (f) runs the store filter with the setting both ON and OFF (category limited to store A: filter B hides it, filter A shows it).
  - AC reference: AC3.8, AC5.4.
- **Notes**: Documentation only, functional path, no code change. Historical QA and implementation notes on TT-006/TT-014 that mention ApplyStoreMapping may stay as history.
