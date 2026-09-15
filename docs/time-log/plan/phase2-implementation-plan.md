# Implementation Plan — Time Log Module, Phase 2 (Project Management)

**Traces to:** `docs/time-log/design/phase2-project-management.md`, `docs/time-log/tickets/tickets.md` (T-014..T-029)
**Target nopCommerce version:** confirmed 4.90 line / .NET 9 runtime, per the version-history correction recorded at the top of `tickets.md` (the design doc's "5.00" references are the same codebase, label corrected).
**Placement:** 100% "extension of an existing plugin" (`Nop.Plugin.Misc.TimeLog`). No `new-plugin` tasks, no core-modification tasks. Every task below is tagged `modify-existing`.

## Flags carried forward from the design (must be resolved before/at the start of coding, not silently assumed)
1. **Critical Finding** (design doc, top): confirm with requirement owner that extending the existing `Project` entity — not creating a second one — is accepted, and that `Active` is operationally superseded by `Status` eligibility. `tickets.md` T-014 records this as stakeholder-confirmed via requirements §6 Decision 6; treat as resolved, but the developer agent should not re-litigate it.
2. Exact wording of `Admin.TimeLog.Validation.ProjectInvalidOrInactive` vs. a new message distinguishing "not assigned" from "inactive" — cosmetic, resolve during TT-023's implementation (leave a code comment, don't block).
3. Confirm `[CheckPermission]` attribute name/signature against `TimeLogAdminController`'s actual usage before TT-011/TT-014 (permission wiring / controller tasks) start.

No genuine core modification exists anywhere in this phase — nothing here requires a Core Modification Notice.

---

## Build Order (high level)

```
Stage 1 — Schema           TT-018, TT-019, TT-020, TT-021
Stage 2 — Service layer    TT-022, TT-023, TT-024, TT-025, TT-026, TT-027, TT-028
Stage 3 — Permission/role  TT-029, TT-030
Stage 4 — Admin nav        TT-031
Stage 5 — Controller/UI    TT-032..TT-041
Stage 6 — Phase 1 changes  TT-042, TT-043, TT-044   (depend on Stage 2 service methods)
Stage 7 — Delete guard     TT-045, TT-046           (depend on Stage 2 + Stage 5 controller both existing)
Stage 8 — Lifecycle/locale TT-047
Stage 9 — Tests            TT-048
```

Rule respected: migrations/entities → services → permission/role → admin UI → storefront/Phase-1 integration (parallel-safe with admin UI once services are stable) → install/uninstall wiring → delete-guard last.

---

## Stage 1 — Schema (Task-type: `modify-existing`) — Ticket T-014

| Task | Description | Depends on |
|---|---|---|
| TT-018 | `Domain/ProjectStatus.cs` new enum (NotStarted/Inprogress/OnHold/Completed/Cancelled/Retired) + locale resource keys `Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.*` | none |
| TT-019 | Extend `Domain/Project.cs`: add `StartDate`, `EndDate`, `Description`, `StatusId` (+ `[NotMapped] Status`), `CreatedOnUtc`, `UpdatedOnUtc`, `LimitedToStores`; implement `IStoreMappingSupported` | TT-018 |
| TT-020 | New `Domain/ProjectStaffMapping.cs` (BaseEntity, `ProjectId`, `CustomerId`) | none |
| TT-021 | New migration `Data/Migrations/AddProjectManagementSchemaMigration.cs` (`AutoReversingMigration`, `MigrationProcessType.Update`, dated after Phase 1's) altering `Project` table + `Create.TableFor<ProjectStaffMapping>()`; new `Data/Mapping/Builders/ProjectStaffMappingBuilder.cs` and `Data/Mapping/Builders/ProjectBuilder.cs`. **Must not edit the existing `SchemaMigration.cs`.** | TT-019, TT-020 |

**Verification:** fresh install and upgrade-from-Phase-1-only both apply cleanly; new columns have correct defaults; `ProjectStaffMapping` FKs to `Project`/`Customer`; `Down()` cleanly reverses.

---

## Stage 2 — Service layer (Task-type: `modify-existing`) — Ticket T-015

All in `Services/IProjectService.cs` / `ProjectService.cs`, extending not replacing. All depend on Stage 1 (TT-021).

| Task | Description | Depends on |
|---|---|---|
| TT-022 | `NopTimeLogDefaults.TimeLoggableProjectStatuses` static `HashSet<ProjectStatus>` = {NotStarted, Inprogress} — single shared eligibility source | TT-018 |
| TT-023 | `DeleteProjectAsync(Project)` — also bulk-deletes `ProjectStaffMapping` rows for the project via `IRepository<ProjectStaffMapping>` in the same operation | TT-021 |
| TT-024 | Extend `GetAllProjectsAsync(name, storeId, pageIndex, pageSize)` signature; apply `IStoreMappingService.ApplyStoreMapping<Project>` | TT-021 |
| TT-025 | `GetProjectsAssignedToCustomerAsync(customerId, eligibleForTimeLoggingOnly, storeId)` — joins `ProjectStaffMapping`, applies `TimeLoggableProjectStatuses` filter when requested | TT-021, TT-022 |
| TT-026 | `GetAssignedCustomerIdsAsync(projectId)` | TT-021 |
| TT-027 | `SaveStaffAssignmentsAsync(projectId, customerIds)` — sync insert/remove diff against current mapping rows (not append-only) | TT-021 |
| TT-028 | `GetAssignedStaffCountAsync(projectId)`; `HasTimeLogRecordsAsync(projectId)` (queries `IRepository<Domain.TimeLog>` by `ProjectId`, any status); mark `GetAllActiveProjectsAsync()` `[Obsolete]` | TT-021 |

**Verification:** unit-test each method in isolation (assignment sync correctness, `HasTimeLogRecordsAsync` catches Draft+Submitted, eligibility set matches design). DI: confirm registration already covered by existing `NopStartup.cs` entry for `ProjectService` — no new DI file.

---

## Stage 3 — Permission & role (Task-type: `modify-existing`) — Ticket T-016

Depends on: Stage 1 only (no service dependency), but sequenced after Stage 2 per design ordering since install-time seeding is easiest to verify once services compile.

| Task | Description | Depends on |
|---|---|---|
| TT-029 | `Infrastructure/PermissionProvider.cs`: add `ManageProjects` const + `AllConfigs` entry; add `NopTimeLogDefaults.ProjectManagerRoleSystemName = "TimeLogProjectManager"`; add `GetOrCreateProjectManagerRoleAsync` mirroring existing role-creation methods | TT-021 |
| TT-030 | `TimeLogPlugin.InstallAsync`/`UninstallAsync` extended symmetrically: find-or-create Project Manager role + permission on install, safe conditional delete on uninstall | TT-029 |

**Verification:** install/upgrade from Phase-1-only store creates role+permission; `ManageProjects`-only user cannot reach time log grid; `ManageTimeLog`-only user cannot reach any Project controller action; dual-role user gets both.

---

## Stage 4 — Admin navigation (Task-type: `modify-existing`) — Ticket T-017

| Task | Description | Depends on |
|---|---|---|
| TT-031 | Extend `AdminMenuManager.HandleEventAsync`'s `timeLogSection.ChildNodes` with new "Project" node (`NopTimeLogDefaults.ProjectAdminMenuSystemName`, icon `far fa-folder-open`, gated on `ManageProjects`) | TT-029 |

**Verification:** node renders only for `ManageProjects` holders, links to Project List.

---

## Stage 5 — Controller / Admin UI (Task-type: `modify-existing`) — Tickets T-018, T-019, T-020, T-021, T-022, T-023

New `ProjectController` (admin area) + views. All depend on Stage 2 services and Stage 3 permission being in place.

| Task | Description | Ticket | Depends on |
|---|---|---|---|
| TT-032 | `ProjectSearchModel`/`ProjectModel` (incl. `SelectedCustomerIds`, `AvailableStaff`, `AvailableStatuses`) + `ProjectValidator` (FluentValidation, mirrors `TimeLogValidator`) | T-018, T-023 | TT-022, TT-028 |
| TT-033 | `ProjectController.List()` GET + POST (Kendo grid data, store-scoped, `MaxPageSize` clamp) | T-018 | TT-024, TT-029 |
| TT-034 | Project List Kendo grid view (`Views/Project/List.cshtml`): Name, StartDate, EndDate (null → "—"), Status (localized), AssignedStaffCount | T-018 | TT-033, TT-028 |
| TT-035 | `ProjectController.Create()` GET/POST — validator, insert, `IStoreMappingService.SaveStoreMappingsAsync` | T-019 | TT-032, TT-023(service delete not needed here, but insert path via TT-024/21) |
| TT-036 | `_ProjectStaffPicker.cshtml` partial — dual `<select multiple>` + jQuery move buttons, `aria-label`s, keyboard-operable real `<button>`s (WCAG 2.1 AA) | T-020 | TT-025, TT-026 |
| TT-037 | Wire staff picker into Create view; `SaveStaffAssignmentsAsync` called on Create POST (empty selection is valid — covers T-021 zero-staff case) | T-020, T-021 | TT-036, TT-027 |
| TT-038 | `ProjectController.Edit(int id)` GET — loads project + `GetAssignedCustomerIdsAsync`, doubles as Details (Decision 1) | T-022 | TT-032, TT-026 |
| TT-039 | `ProjectController.Edit(ProjectModel)` POST — same validation/save path as Create, syncs staff assignments via `SaveStaffAssignmentsAsync` | T-020, T-022 | TT-038, TT-027 |
| TT-040 | Create/Edit Razor views (`Views/Project/Create.cshtml`, `Edit.cshtml`) — Name/StartDate/EndDate/Description/Status fields + staff picker partial + store-mapping partial | T-019, T-022 | TT-035, TT-037, TT-038 |
| TT-041 | Server-side field validation wired into Create/Edit POST actions (empty Name, missing StartDate, EndDate<StartDate, out-of-range Status rejected even if client bypassed) | T-023 | TT-032, TT-035, TT-039 |

**Verification:** minimal valid project creates and appears in list with null EndDate; dual-listbox move+save produces exact diff; empty-staff save succeeds; Edit screen shows all fields + assigned staff names; each invalid-field case rejected with correct localized message including a crafted out-of-range Status.

---

## Stage 6 — Phase 1 `TimeLogController` changes (Task-type: `modify-existing`) — Tickets T-024, T-025, T-026, T-027

Depend on Stage 2 (`GetProjectsAssignedToCustomerAsync`, `TimeLoggableProjectStatuses`) being complete. Can proceed in parallel with Stage 5 once Stage 2 is done.

| Task | Description | Ticket | Depends on |
|---|---|---|---|
| TT-042 | `TimeLogController.PrepareSearchModelAsync`: replace `GetAllActiveProjectsAsync()` call with `GetProjectsAssignedToCustomerAsync(customerId, eligibleForTimeLoggingOnly: true, storeId)`, feeding both the insert/edit dropdown and `TimeLogSearchModel.AvailableProjects` filter from the one call | T-024, T-025, T-026 | TT-025, TT-022 |
| TT-043 | Server-side re-validation in insert/update actions: re-check posted `ProjectId` against the same eligibility call before persisting; reuse `Admin.TimeLog.Validation.ProjectInvalidOrInactive` (flag wording follow-up per Open Item #2) | T-027 | TT-042 |
| TT-044 | Regression check: confirm T-028 (unassign does not touch historical `TimeLog` rows) requires no code change — verify `SaveStaffAssignmentsAsync` (TT-027) never touches `TimeLog` table, add a confirming unit/integration test | T-028 | TT-027, TT-042 |

**Verification:** staff A (assigned 1,2 not 3) sees only 1,2; staff B doesn't see A's unrelated project; toolbar filter options identical to dropdown; ineligible-status assigned project excluded from dropdown, Inprogress one accepted; crafted request against ineligible-status assigned project and against eligible-status-but-unassigned project both rejected server-side; unassign staff leaves their 4 existing mixed Draft/Submitted rows untouched and still editable per normal Draft rules.

---

## Stage 7 — Delete guard (Task-type: `modify-existing`) — Ticket T-029

Last stage — depends on both the service layer (Stage 2: `HasTimeLogRecordsAsync`, `DeleteProjectAsync`) and the controller (Stage 5: `ProjectController` must already exist to host the `Delete` action).

| Task | Description | Depends on |
|---|---|---|
| TT-045 | `ProjectController.Delete(int id)` — calls `HasTimeLogRecordsAsync`; if true, sets `Status = Retired`, `UpdatedOnUtc`, calls `UpdateProjectAsync`, returns informational JSON message; else calls `DeleteProjectAsync` (hard delete + mapping cleanup) | TT-028, TT-023, TT-033 |
| TT-046 | New locale key `Admin.TimeLog.Project.Delete.BlockedRetired`; wire Delete button/confirmation in List view to surface the message | TT-045, TT-034 |

**Verification:** zero-`TimeLog` project deletes cleanly with mapping cleanup; Draft-only referencing row → auto-Retired + message, not deleted; Submitted-only referencing row → identical behavior.

---

## Stage 8 — Plugin lifecycle & localization (Task-type: `modify-existing`) — cross-cutting (T-014 through T-029)

| Task | Description | Depends on |
|---|---|---|
| TT-047 | Add all Phase 2 locale resource keys (`Admin.TimeLog.Menu.Project`, `Admin.TimeLog.Project.List.*`, `Admin.TimeLog.Project.Fields.*`, `Admin.TimeLog.Project.Validation.*`, `Admin.TimeLog.Project.Delete.BlockedRetired`, `Enums...ProjectStatus.*`, `Security.Permission.Misc.TimeLog.ManageProjects`) to `TimeLogPlugin.InstallAsync`, removed symmetrically in `UninstallAsync` (Plugin Lifecycle standard — install/uninstall must stay symmetrical) | TT-018, TT-029, TT-046 |

**Verification:** install/uninstall cycle leaves no orphaned locale resources, permission, or role; re-install is clean.

---

## Stage 9 — Tests (Task-type: `modify-existing`)

| Task | Description | Depends on |
|---|---|---|
| TT-048 | xUnit tests: `ProjectValidator` (all 4 rules incl. out-of-range Status), `ProjectService` new methods (assignment sync, `HasTimeLogRecordsAsync` Draft+Submitted, eligibility filtering), delete-guard branch logic | All prior stages |

---

## Dependency Graph (ticket level)

```
T-014 (schema) ──┬─> T-015 (service) ──┬─> T-016 (role/permission) ──> T-017 (nav)
                 │                     │
                 │                     ├─> T-018 (list) ──> T-019 (create) ──┬─> T-020 (staff picker) ──> T-021 (empty staff)
                 │                     │                                    │
                 │                     │                                    └─> T-022 (edit/details)
                 │                     │
                 │                     ├─> T-023 (field validation)
                 │                     │
                 │                     ├─> T-024 (dropdown) ──> T-025 (filter) ──> T-026 (status eligibility) ──> T-027 (server re-validation)
                 │                     │                                                                              │
                 │                     │                                                                       T-028 (unassign safety)
                 │                     │
                 │                     └─> T-029 (delete guard) — also needs T-016, and Project Controller from T-018/T-019
```

Everything ultimately traces back to T-014 → T-015 as the two foundational Feature tickets; nothing in Phase 2 can start ahead of those.
