# QA Report - expense-category (SCRUM-38)

Date: 2026-10-04 | nopCommerce 4.90.8 | Plugin `Nop.Plugin.Misc.Expenses` (`src/Plugins`) | Tester: nopcommerce-qa-tester

## 1. Summary

- Release recommendation: **Go-with-caveats for merge to the dev branch; No-Go for release** until the runtime checks in section 6 are executed on an installed site (TT-029 stays Blocked, T-002 stays Blocked) and the two Medium bugs (BUG-001, BUG-002) are fixed or consciously accepted.
- Nothing in this report was verified at runtime. The site is not installed in the QA environment (no `dataSettings.json`, no database). Razor views and `expensecategory-grid.js` are not compiled or executed by `dotnet build`; they were reviewed statically only.
- Defects: 10 filed (0 Critical, 0 High, 2 Medium, 8 Low), all `nop-plugin`, none core-safety. Tickets: `docs/expense-category/tickets/tickets.md` (BUG-001 to BUG-010, status Backlog; every ticket also carries a QA Result note).
- Ticket status mismatch reported: T-002 and TT-029 are `Blocked`, not `Done`. Their code was reviewed and tested anyway; their status was not changed.

## 2. Build and tests run

| Step | Command (SolutionDir passed as required) | Result |
|---|---|---|
| Build | `dotnet build NopCommerce.sln -p:SolutionDir="C:\\Tharindu\\Project\\OfficeManagement\\src\\"` | 0 errors, 4 warnings (2 x CS0108, 2 x CS0618 and CS1998 in other projects; none in Expenses). The developer recorded 3 warnings; the count is 4 here, none from the plugin. |
| Expenses tests | `dotnet test Plugins/Nop.Plugin.Misc.Expenses.Tests` | Passed 94, Failed 0, Skipped 0 |
| TimeLog tests | `dotnet test Plugins/Nop.Plugin.Misc.TimeLog.Tests` | Passed 80, Failed 0, Skipped 0 (no regression from the new plugin) |

Limits of the 94 tests: controllers are tested with mocked services and a mocked `ModelState`, the service with an in-memory table and a mocked `IStoreMappingService`, plugin lifecycle with mocked repositories and localization. They prove the plugin's own logic, not framework behaviour (model binder culture, store-mapping SQL, permission filter, localized-property fallback, migration, Razor rendering).

## 3. Acceptance criteria coverage

Levels: **Test** = verified by an automated test (mock-based, see limits above); **Review** = verified by code review only; every row also lists what remains **UNVERIFIED pending runtime**.

| AC | Level | UNVERIFIED pending runtime / note |
|---|---|---|
| AC1.1 | Review | Menu rendering, ordering, permission hiding (AdminMenuManager code only) |
| AC1.2 | Review | Page renders with search panel, Add panel and grid (Razor never compiled) |
| AC1.3 | Test (keys exist, under uninstall prefixes) + Review (views use `T()`) | Language switch; see BUG-008 (Low) |
| AC2.1 | Test (structural: one config, Administrators) | Framework inserting the record and mapping on restart |
| AC2.2 | Review | Menu hidden for a role without the permission |
| AC2.3 | Test (structural: all six actions carry `[CheckPermission]`, class has `[AuthorizeAdmin]`, antiforgery) | Actual denial for a non-permitted user |
| AC2.4 | Review (single permission used everywhere; stock `SavePreference` needs none) | Walk-through with a role holding only this permission plus admin access |
| AC2.5 | Test (mock lifecycle: permission, locale prefixes, order) + Review | Real removal of menu, permission, locale rows; BUG-003 (Low) breaks full symmetry |
| AC2.6 | Test (predicates match only `ExpenseCategory` rows) | Real row deletion against a database |
| AC3.1 | Review | Four columns rendered |
| AC3.2 | Test (controller and service clamp to 100, zero length safe) | - |
| AC3.3 | Test (empty page from service and controller) | Empty grid UI |
| AC3.4 | Test (null limit gives empty display) | Rendered empty cell |
| AC3.5 | Test (formatter called with the primary currency, resolved once) | Real currency format |
| AC3.6 | Test (trimmed, case-insensitive, empty lists all; no delete action by reflection) | Real SQL translation of `ToLower().Contains` |
| AC3.7 | Test (store 0 does not apply the mapping) | - |
| AC3.8 | Test (mocked `ApplyStoreMapping` only) | Real filtering, with `IgnoreStoreLimitations` OFF; BUG-010 (Low) when ON |
| AC3.9 | Test (mocked) | Real combined filter |
| AC3.10 | Review | Stores cell text |
| AC4.1 | Test (server: persisted, trimmed) | Add panel, no navigation, grid refresh |
| AC4.2 | Test (server update) + Review | Inline edit round trip; BUG-005 (Low) |
| AC4.3 | Test (validator and controller) + Review (dropdown choices) | Dropdown, localized message shown |
| AC4.4 | Review (type column not Editable) | Read-only type in edit mode |
| AC4.5 | Test (different or missing type rejected, same accepted, nothing saved) | JS appends the row type correctly (ajaxPrefilter); if it failed every inline edit would be rejected as TypeLocked |
| AC4.6 | Test | - |
| AC4.7 | Test (0, positive, 4 decimals) | Defects: BUG-001 (Medium), BUG-002 (Medium) |
| AC4.8 | Test (negative, mocked non-numeric) | Real binder; BUG-001 (Medium) |
| AC4.9 | Test (same validator) | BUG-002 (Medium, DB overflow bypasses validation), BUG-009 (Low) |
| AC4.10 | Review (no consumer of the limit exists in the plugin) | - |
| AC4.11 | Test | - |
| AC4.12 | Test | - |
| AC4.13 | Test (`LimitedToStores = false` even if true posted) | - |
| AC4.14 | Review (Add panel has no store or translation inputs) + Test (insert sets false) | - |
| AC5.1 | Review | Modal opens on the same page, AJAX body renders |
| AC5.2 | Review (Bootstrap focus trap and Escape, labels, `aria-labelledby`, focus return by id) | Real keyboard walk; BUG-004, BUG-005, BUG-006, BUG-007 (all Low) |
| AC5.3 | Test (store mapping and update called) + Review (JS reload) | Modal closes, Stores column refreshes; BUG-004 (Low) |
| AC5.4 | Test (service filter, mocked) | Real filter after a save; BUG-010 (Low) |
| AC5.5 | Test (empty list saved) | Discoverability, BUG-006 (Low) |
| AC5.6 | Test (`SaveLocalizedValueAsync` per language on `Name`) | Display wherever the name is shown: no consumer exists in this phase (admin grid always shows the default Name) |
| AC5.7 | Test (empty value passed, which makes the framework delete the row) | Fallback to the default Name at display time |
| AC5.8 | Test (201 rejected, nothing stored, trimmed length) | Field error shown next to the input |
| AC5.9 | Test (Name, limit, type untouched; model carries neither) | - |
| AC5.10 | Test (structural, same as AC2.3) | Actual denial |

Totals (43 criteria): primary level Test 33 (AC1.3, 2.1, 2.3, 2.5, 2.6, 3.2 to 3.9, 4.1 to 4.3, 4.5 to 4.9, 4.11 to 4.14, 5.3 to 5.10; AC4.2 and AC4.14 also rely on review); Review only 10 (AC1.1, 1.2, 2.2, 2.4, 3.1, 3.10, 4.4, 4.10, 5.1, 5.2). None is verified at runtime, so **all 43 have a runtime residual** as listed. "Test" means mock-based server logic or structural reflection checks, not end-to-end proof.

## 4. Plugin lifecycle (nopCommerce-specific)

- Install: locale resources added from one inventory, permission registered through `IPermissionConfigManager` (Test). Plugin listing, migration and permission insert on restart: UNVERIFIED.
- Uninstall order confirmed in framework source: `PluginService` calls `UninstallAsync` (line 578) before `ApplyDownMigrations` (line 582), so the StoreMapping and LocalizedProperty deletes run while the table still exists. `DeletePermissionAsync` removes role mappings, localized permission name and record. Real execution: UNVERIFIED.
- Symmetry gap: BUG-003 (Low), per-customer `GenericAttribute` rows `ExpenseCategoryPage.HideSearchBlock` and `HideAddPanel` survive uninstall.
- Reinstall after uninstall: UNVERIFIED.
- Multi-store: store mapping via `SaveStoreMappingsAsync` (never direct writes), posted ids intersected with real stores. Behaviour with real stores: UNVERIFIED; BUG-010 (Low).
- Core modification: none; no Core Modification Notice is required and none exists. Core files untouched (git status shows only the new plugin folders and the solution file).

## 5. Review findings by topic (requested focus areas)

- **Type lock**: server compares the posted type with the stored one before anything else; missing or different type is rejected (`TypeLocked`), the candidate copy keeps the entity unmodified on failure. Sound. Dependency to watch: the inline edit relies on `expensecategory-grid.js` (`ajaxPrefilter`) to add the type; selector and jQuery behaviour checked against framework `Table.cshtml`, but untested at runtime.
- **Input validation / decimal culture**: BUG-001 and BUG-002 (Medium). Group separators are accepted and misread; no upper bound or scale rule against `decimal(18,4)`.
- **Permission and CSRF**: all six actions have `[CheckPermission]`, class has `[AuthorizeAdmin]`, `[Area]`, `[AutoValidateAntiforgeryToken]`; POST actions are `[HttpPost]`; the one GET (modal load) is a read. AJAX posts send the token. No delete action exists. BUG-009 (Low) for the missing `ModelState.IsValid` first-check. Note: the framework also requires "Access admin area" for any admin page, so a user with only this permission still cannot enter admin (framework rule, not a defect).
- **Store mapping and localization**: services used correctly (`SaveStoreMappingsAsync`, `SaveLocalizedValueAsync`, empty value removes translation); translation length checked before any write. BUG-010 (Low) for the core setting. Translated names are stored but nothing displays them in this phase.
- **Uninstall symmetry**: see section 4 and BUG-003.
- **Accessibility**: dialog semantics, labels, `aria-describedby`, `role="alert"` regions and Bootstrap focus trap are present. Defects: BUG-004, BUG-005, BUG-006, BUG-007 (Low). Not filed as bugs (stock admin patterns): collapsible panel headers are click-only `div`s, inline Update and Cancel are `<a href="#">`, and the Add panel is not a form so Enter does not submit.
- **Hardcoded strings**: BUG-008 (Low). All other user-facing text goes through locale resources.
- **N+1 and paging**: no repository access in the controller; primary currency resolved once; per-row `GetLocalizedEnumAsync` and `FormatPriceAsync` hit the localization and currency caches, not the database; paging clamped to 100 in controller and service (Test). Real query count: UNVERIFIED.
- **Razor and JS correctness (not compile-checked)**: no compile or logic error found by static review (namespaces from `_ViewImports`, tag helpers, `DataTablesModel` usage, `data-columnname` and `editState` conventions, `escapeHtml`, `display_nop_error` override order, `data-raw` handling). Residual risk: first render, script load order (the plugin script must load after `admin.common.js` for the `display_nop_error` wrapper to keep the stock alerts), and `Html.PartialAsync` of the modal shell.
- **Security standards (`.claude/instructions/project-standards`)**: parameterized repository access only, no secrets, no sensitive logging, dedicated permission, uninstall removes permission and locale resources (settings: none exist). Deviations: BUG-003, BUG-009.
- **Other observations (not bugs)**: `ExpenseCategoriesPatternCacheKey` is cleared on writes but nothing caches under it yet; the Expenses plugin and test folders are untracked in git (not yet committed).

## 6. Remaining runtime checks (TT-029, dev SQL Server, two stores, two languages)

1. Plugin listed as Expenses; Install succeeds (AC2.5 baseline).
2. Table `ExpenseCategory` created with the specified columns; dropped on uninstall; reinstall works.
3. After restart: permission listed once and mapped to Administrators (AC2.1); a role without it sees no Expenses menu (AC2.2) and is denied on direct URL and on all AJAX endpoints, including modal GET and POST (AC2.3, AC5.10); a permitted role needs no second permission (AC2.4).
4. Menu and labels, then language switch (AC1.1 to AC1.3).
5. First render of the list page and the modal; empty grid (AC1.2, AC3.3); columns, limit format, Stores text (AC3.1, AC3.4, AC3.5, AC3.10).
6. Store filter with `IgnoreStoreLimitations` OFF, and once ON (AC3.7 to AC3.9, AC5.4, BUG-010); name search with `%` and `_` characters.
7. Add and inline edit: field errors, type locked in the UI and on direct calls, limit entry in a comma-decimal and a point-decimal language (AC4.1 to AC4.9, BUG-001, BUG-002), keyboard-only operation (BUG-005).
8. Modal: keyboard-only walk, focus trap, Escape, focus return (also after a successful save), translation save and empty fallback, over-200 rejection, grid reload (AC5.1 to AC5.9, BUG-004, BUG-006); contrast of status text (BUG-007).
9. Create mapped and translated data, uninstall: zero `StoreMapping` rows with `EntityName = 'ExpenseCategory'`, zero `LocalizedProperty` rows with `LocaleKeyGroup = 'ExpenseCategory'`, no `Admin.Expenses*`, enum or permission locale rows, no `ExpenseCategoryPage.*` GenericAttribute rows (AC2.5, AC2.6, BUG-003); then reinstall.
10. Regression: TimeLog menu, grid and permission still work with both plugins installed.

After the bugs are fixed and the checks pass, set TT-029 and T-002 to Done and re-run the install, uninstall and reinstall cycle, since BUG-003 touches Uninstall.

---

# Re-verification of BUG-001 to BUG-009 (2026-10-04)

Tester: nopcommerce-qa-tester | nopCommerce 4.90.8 | Plugin `Nop.Plugin.Misc.Expenses` | Site still NOT installed (no `dataSettings.json`, no database): nothing below is verified at runtime. Razor views and `expensecategory-grid.js` are not compiled or executed by the build.

## R1. Build and tests

| Step | Command | Result |
|---|---|---|
| Build | `dotnet build NopCommerce.sln -p:SolutionDir="C:\\Tharindu\\Project\\OfficeManagement\\src\\"` | 0 errors, 0 warnings (incremental build, so warnings from untouched projects are not re-emitted; the earlier full build reported 4 in other projects, none in Expenses) |
| Expenses tests | `dotnet test Plugins/Nop.Plugin.Misc.Expenses.Tests` | Passed 145, Failed 0, Skipped 0 (was 94) |
| TimeLog tests | `dotnet test Plugins/Nop.Plugin.Misc.TimeLog.Tests` | Passed 80, Failed 0, Skipped 0 |

Extra check beyond the developer tests: a throwaway console probe (kept outside the repo, in the QA scratchpad) bound the real `ExpenseCategoryModel` through the real ASP.NET Core MVC pipeline (model binder factory, `ParameterBinder`, `FormValueProvider` with an explicit culture). It confirms the wiring that the mock-based tests cannot: the property-level `[ModelBinder]` is picked up for `PerMonthLimit`.

| Culture | Posted | Result |
|---|---|---|
| en-US | `1.5` | 1.5, valid |
| en-US | `1,5` / `1,000.50` / `abc` | null, ModelState error on `PerMonthLimit` |
| en-US | empty | null, valid |
| de-DE | `1,5` | 1.5, valid |
| de-DE | `1.5` / `1.000,50` | null, ModelState error on `PerMonthLimit` |
| de-DE | `100000000000000` | binds; the validator rejects it (BUG-002) |
| de-DE | empty | null, valid |

Not covered by the probe: the Nop host (request culture from the culture cookie via `UseNopRequestLocalization`; the Nop binder providers, which only handle `CustomProperties` and `string` and so cannot intercept a decimal). The binder error path reads the display name through `NopResourceDisplayNameAttribute` (localization service, sync over async), the usual Nop pattern; the probe needed a stub engine for it.

## R2. Per-bug verdicts

Level: Test = automated (mock based unless stated); Review = code review only. Every row lists the runtime residual.

| Bug | Verdict | Level | What was checked | Runtime residual |
|---|---|---|---|---|
| BUG-001 | Fixed, Done | Test + real-MVC probe + Review | Strict binder applied to `PerMonthLimit`; en and de behave as in R1; group separators rejected, the controller maps the error to `PerMonthLimitInvalid`; the inline edit raw value and the binder both use `CultureInfo.CurrentCulture` | Add and inline edit on the installed site in a point-decimal and a comma-decimal language; page render culture equals POST culture |
| BUG-002 | Fixed, Done | Test + Review | Max `99999999999999.9999` equals the decimal(18,4) maximum; scale rule allows trailing zeros; upper rule skipped for negatives; boundary tests; messages filled from constants | Boundary value against SQL Server |
| BUG-003 | Fixed, Done | Test (mock) + Review | Uninstall order: permission, StoreMapping, LocalizedProperty, GenericAttribute (KeyGroup Customer, Key starts with `ExpenseCategoryPage.`), locale deletes, base last, all before the table drop; predicate scoped by test; constants shared with the view | Install, collapse a panel, uninstall, query GenericAttribute, reinstall (TT-029 step 9) |
| BUG-004 | Fixed, Done | Review | Status region moved out of the collapsible Add panel, exists at page load, polite live region | Collapse the panel, save in the modal, confirm visible and announced |
| BUG-005 | Fixed for the Edit step, Done | Review | Wrapper matches the framework signature (jQuery row, id); Edit is an anchor with onclick resolved by global name so the wrapper is used; inputs exist synchronously; focus call correct | Keyboard activation. Residual gap tracked as BUG-012 |
| BUG-006 | Fixed, Done | Review + locale tests | Hint rendered and linked by `aria-describedby`; text correct | Screen reader announcement |
| BUG-007 | Fixed, Done | Review | `text-dark` replaces `text-success`; about 11.9:1 in the stock palette; no dark admin skin in the codebase | Measure on the real theme |
| BUG-008 | Fixed for items 1 to 3 (dead keys), Done | Test + Review | `{0}` filled from the constant in the validator and the modal save; no `alert(` left; generic localized text from a data attribute; the three dead keys removed and unreferenced; all used keys exist in the inventory and sit under deleted prefixes | Language switch. The accessible-name gap that motivated item 3 is not fixed, filed as BUG-011 |
| BUG-009 | Fixed, Done | Test + Review | ModelState checked first in Insert and Update (before lookup, type lock, validation, write); per-key localized errors; generic `InvalidRequest` otherwise; modal save also localized; no implicit-required risk (nullable reference types not enabled) | Confirm the stock inline update posts no key that fails binding (Table.cshtml review says it does not) |
| BUG-010 | Not touched, stays Backlog (awaits a product decision) | - | - | - |

## R3. Review of the fixes for new defects

- **Binder wiring and cultures**: sound (R1). Design note: the localized message `PerMonthLimitInvalid` ("must be a number that is 0 or greater") does not tell a user who typed a group separator what is wrong; acceptable, not filed.
- **Validator bounds**: no off-by-one; only the first message per field is shown, so no double reporting; messages use `CurrentCulture` for the number text.
- **Uninstall order**: correct (see BUG-003). Partial-failure behaviour (permission deleted first) is unchanged from the earlier build.
- **Locale key symmetry**: every inventory key is under `Admin.Expenses`, the enum prefix or the single permission key; the lifecycle test enforces it. Observation: `Admin.Expenses.ExpenseCategory.Updated` is installed but never used (not filed).
- **ModelState handling**: Insert, Update and Save all reject first; the generic path returns a localized message instead of framework text.
- **Accessibility changes**: improvements confirmed by review; two residual gaps filed (BUG-011, BUG-012).

## R4. New defects

| ID | Severity | Summary |
|---|---|---|
| BUG-011 | Low | Inline Edit, Update and Cancel links are identical in every row (no row-specific accessible name); residual of BUG-008 item 3 |
| BUG-012 | Low | Focus is lost after inline Update or Cancel; BUG-005 only covered the Edit step |

Both are `nop-plugin`, functional/accessibility, Backlog in `tickets.md`. Defect totals now: 12 filed, 9 Done (BUG-001 to BUG-009), 3 Backlog (BUG-010 product decision, BUG-011 and BUG-012 Low). No Critical or High. The two Medium defects (BUG-001, BUG-002) are fixed and verified by test and review.

## R5. Remaining runtime checks (additions to section 6 of the original report)

1. Add and inline edit in a comma-decimal and a point-decimal language: `1,5` and `1.5` each accepted only in their own language, group separators rejected with the localized field error, `100000000000000` and `1.23456` rejected, no HTTP 500 (BUG-001, BUG-002); confirm the page-render culture equals the POST culture.
2. Uninstall with panels collapsed: zero `GenericAttribute` rows with `Key LIKE 'ExpenseCategoryPage.%'`, then reinstall (BUG-003).
3. Collapse the Add panel, save in the modal: confirmation shown and announced (BUG-004).
4. Keyboard: Edit moves focus into the Name input (BUG-005); retest Update and Cancel after BUG-012 is fixed.
5. Screen reader: store hint announced (BUG-006); measured contrast of the status text on the real theme (BUG-007).
6. Language switch, with the plugin reinstalled so the changed locale texts are loaded (BUG-008).
7. Inline update posts no key that makes ModelState invalid (BUG-009).
8. All earlier runtime checks (section 6), plus BUG-010 once decided. TT-029 and T-002 stay Blocked until the install, uninstall, reinstall cycle is done.

## R6. BUG-010 re-verification (store filter independent of IgnoreStoreLimitations)

Verdict: **BUG-010 stays Done** (verified by code review and unit test; runtime still outstanding). One Low documentation defect filed: BUG-013.

**How the tests were run.** A plain `dotnet build` of the test project fails here: standalone it needs `-p:SolutionDir=C:\Tharindu\Project\OfficeManagement\src\`, and then the plugin project cannot copy its dll to `Presentation\Nop.Web\Plugins\Misc.Expenses` because the running Nop.Web.exe (PID 39428) locks it (MSB3021/MSB3027). The compile itself succeeded, so `Nop.Plugin.Misc.Expenses\obj\Debug\net9.0\Nop.Plugin.Misc.Expenses.dll` (13:14, newer than `ExpenseCategoryService.cs` at 12:42) is a fresh build of the current source. I built the test project with `-p:OutDir` pointing to a scratch folder outside the repo, copied that fresh plugin dll and the Nop.* dlls from `Nop.Web\bin\Debug\net9.0` there, and ran `dotnet test Nop.Plugin.Misc.Expenses.Tests.dll` from the scratch folder. The stale locked dll was not used. Result: **149 passed, 0 failed** (all Expenses tests, including the unrelated controller, validator, binder and lifecycle tests). TimeLog tests were not re-run (nothing there changed).

**Semantics (review + test).**

| Check | Result | Evidence |
|---|---|---|
| Store chosen: unlimited plus mapped to that store only | OK | `GetAll_StoreChosen_...`, `GetAll_OtherStoreChosen_...` |
| "All stores" (0) lists everything | OK | `GetAll_StoreIdZero_...` (rows with and without mappings) |
| Entity name | OK | `nameof(ExpenseCategory)` = "ExpenseCategory" = `typeof(T).Name` used when mappings are saved; a Product row with the same id is ignored (tested) |
| Stale rows | OK | Unlimited category with a row for another store is still listed; limited with no rows is hidden (tested) |
| No mappings at all | OK | Limited categories hidden (the stock call would have returned everything); tested |
| Name plus store, page-size clamp with a store set | OK | Tested; clamp applied before the query |
| Independent of the setting | OK | No CatalogSettings or IStoreMappingService dependency (constructor test, source) |

**SQL (review only).** One LINQ expression: `Where(!LimitedToStores || mappedIds.Contains(Id))` with `mappedIds` an `IQueryable<int>` from `IRepository<StoreMapping>.Table`, which linq2db translates to `IN (SELECT EntityId ... WHERE EntityName = @p AND StoreId = @p)`. No loop or per-row call, so no N+1; `ToPagedListAsync` adds the usual count. Entity name and store id are parameters. Core uses the same shape (`ForumService.cs` line 480) and a correlated `Any` in `ApplyStoreMapping`. The tests use in-memory LINQ, so the real translation is not proven by them.

**Dependents.** The controller passes `SearchStoreId` straight through; writes still use `SaveStoreMappingsAsync`; the list view has no setting logic; DI resolves `IRepository<StoreMapping>` automatically. AC3.7 to AC3.9 and AC5.4 match the code; the technical design (line 98) and plan P5 are consistent. Implementation-plan lines 122 and 158 and tickets TT-006/TT-029 still carry the old wording (BUG-013).

**Remaining runtime checks.** Two stores, a category limited to store A: with Ignore store limitations ON and again OFF, filter B (not listed), A (listed), All stores (listed); a not-limited category is listed for both; run against SQL Server to confirm the sub-select executes; change stores in the modal, then re-check the filter (AC5.4).

Defect totals: 13 filed, 10 Done (BUG-001 to BUG-010), 3 Backlog (BUG-011, BUG-012, BUG-013, all Low). No Critical or High.
