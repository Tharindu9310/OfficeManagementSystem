# Clarified Requirement — Time Log Module, Phase 2 (Project Management)

**Source document:** `docs/intake/requirement.md`
**Builds on:** `docs/time-log/requirements/clarified-requirement.md` (Phase 1)
**Target nopCommerce version:** 4.90.8 (.NET 8, stable line) — confirmed in source doc, section "nopCommerce Version".
**Feature slug:** `time-log` (Phase 2 adds Project management + staff assignment to the existing plugin)

## 1. Summary

Adds a Project management admin screen (list, create/edit, staff assignment) gated by a new "Project Manager" role/`ManageProjects` permission, and changes one Phase 1 behavior: the staff member's Project dropdown/filter in the time log grid is now restricted to only the projects that staff member has been assigned to (via a new Project–Staff many-to-many mapping), enforced server-side.

This remains an admin-area-only feature; no storefront-facing component.

## 2. In-Scope Behavior (traced to source doc sections)

### 2.1 Roles & Permissions (source §2)
- New "Project Manager" `CustomerRole` — created on install if it doesn't already exist, same pattern as Phase 1's Staff-role check.
- New `PermissionRecord`, `ManageProjects`, granted to the Project Manager role. Controls visibility of the "Project" menu item and access to all Project screens/actions.
- `ManageTimeLog` (Phase 1) is unaffected and stays scoped to Staff. A Project Manager does **not** get time-log grid access unless separately given the Staff role/permission. A single user may legitimately hold both roles (confirmed in source §5.2/§6).
- Data visibility: a Project Manager can see/manage **all** projects — no per-manager ownership restriction is described or should be assumed.

### 2.2 Navigation (source §3)
- New "Project" child menu item under the existing "Time Log" parent menu, visible only to customers holding `ManageProjects`.
- Leads to the Project List screen (create/edit access from there).

### 2.3 Data Model (source §4)

**`Project` entity**
| Field | Type | Rule |
|---|---|---|
| Id | int | PK |
| Name | nvarchar | Required, non-empty |
| StartDate | date | Required |
| EndDate | date, nullable | Optional — null means open-ended/ongoing |
| Description | nvarchar | Optional |
| Status | int (enum) | Required. Values: `Not Started`, `Inprogress`, `On Hold`, `Completed`, `Cancelled`, `Retired`. Only `Not Started` and `Inprogress` allow staff to log time against the project. |
| CreatedOnUtc | datetime | Audit, set on insert |
| UpdatedOnUtc | datetime | Audit, updated on every save |

**`ProjectStaffMapping` entity (many-to-many)**
| Field | Type | Rule |
|---|---|---|
| Id | int | PK |
| ProjectId | int | FK to Project |
| CustomerId | int | FK to Customer (a Staff-role user) |

- A staff member may be assigned to multiple projects; a project may have multiple staff members — standard nopCommerce mapping-table pattern (cf. Customer↔CustomerRole, Product↔Category).
- Mapping rows are synced (inserted/removed) on every Project save to match the submitted staff selection — not append-only.

### 2.4 Screens & Behavior (source §5)

**Project List**
- Standard nopCommerce admin grid: Name, Start Date, End Date, Status, (optionally assigned-staff count — see Open Question Q4).
- "Add new" opens the Create screen (standard create/edit form, not Phase 1's inline-grid style, because the staff picker needs more room than a grid cell).

**Create / Edit Project**
- Fields: Name, Start Date, End Date (nullable), Description, Status.
- Staff assignment via a dual-listbox ("available" / "assigned") multi-select, populated from Customers in the Staff role (existing `ICustomerService`, filtered by role membership — not by excluding the current user, since a user can hold both roles).
- Save persists the Project record and syncs `ProjectStaffMapping` rows to the submitted selection.

**Project Details**
- **Decision (confirmed by stakeholder):** No separate read-only view. The Edit screen doubles as the details view — it shows all project fields plus the current assigned-staff list, consistent with Phase 1's lighter-weight style.

### 2.5 Validation (source §6)
| Field | Rule |
|---|---|
| Name | Required, non-empty |
| Start Date | Required |
| End Date | Optional; if provided, must be on or after Start Date |
| Status | Required; one of the six defined enum values |
| Assigned staff | Optional — a project can be saved with zero staff assigned (confirmed) |

### 2.6 Impact on Phase 1 — Time Log Project Dropdown/Filter (source §7)
- The Project dropdown in the staff time log grid's insert/edit row, and the Project filter in the grid toolbar, must both be changed from "all active projects" to "only projects the current staff member is assigned to" via `ProjectStaffMapping`.
- Server-side enforcement is required in both the insert and update actions: a `ProjectId` posted directly that is outside the current staff member's assignments must be rejected, even if it bypasses the dropdown UI.
- **Unassignment does not retroact**: removing a staff member from a project only affects what they can select going forward (dropdown/filter). Existing `TimeLogRecord` rows already logged against that project remain unchanged, still visible, and still editable/deletable per their normal Phase 1 Draft/Submitted rules — the mapping removal is not a cascading delete or lock.

### 2.7 Project Deletion Guard (source §8 item 8)
- The `Delete` action must check for any `TimeLogRecord` referencing the project — **regardless of the record's Draft/Submitted status** — and block deletion with a validation message if any exist. This is a confirmed hard requirement, not a suggested pattern.
- **Decision (confirmed by stakeholder):** When Delete is invoked on a project with existing `TimeLogRecord`s, the system automatically sets Status = `Retired` in place of deleting, and informs the user that the project was retired instead of deleted (rather than only blocking with a message and leaving the status change to the Project Manager).

### 2.8 Status Enum and Time-Logging Eligibility (source §8 item 9)
- Enum values, in the order given: `Not Started`, `Inprogress`, `On Hold`, `Completed`, `Cancelled`, `Retired`.
- Only `Not Started` and `Inprogress` projects are eligible for staff to log new time against (this governs which projects populate a staff member's dropdown, in combination with the assignment filter from §2.6 — both conditions apply together: assigned AND status in {Not Started, Inprogress}).
- **Decision (confirmed by stakeholder):** Status is a free-form select — no transition restrictions. Any status can be changed to any other status via the Edit screen (except the automatic Retired transition on a blocked delete, §2.7, which is system-driven, not a user-initiated transition rule).

## 3. Explicitly Out of Scope (Phase 2)
- Any change to Phase 1's Draft/Submitted time-log lifecycle, autosave behavior, bulk submit, or time-format handling — untouched except for the dropdown/filter data source described in §2.6.
- Per-manager project ownership/visibility restrictions — all Project Managers see all projects.
- Automatic removal of a staff member's project mappings when their Staff role is revoked — **decision (confirmed by stakeholder): mapping rows persist untouched; a Project Manager must manually unassign them from a project separately.** No role-removal event hook is required.
- Reporting/export of project or staff-assignment data.
- Any storefront-facing UI.

## 4. Store-Context Axes Addressed

| Axis | Finding |
|---|---|
| Storefront vs admin | Admin-only, same as Phase 1. No storefront/theme changes. |
| Entity/data impact | Introduces two new entities (`Project`, `ProjectStaffMapping`) and modifies the query logic in Phase 1's `TimeLogController` (project select-list and search-filter source). No changes to existing catalog/order schema. |
| Multi-store (`StoreMapping`) | **Decision (confirmed by stakeholder): Project is per-store scoped.** `Project` must support `StoreMapping` (same pattern as other store-mappable entities), and Project list/select-list queries (both admin grid and the staff dropdown/filter in §2.6) must respect current store context. |
| Localization | Not addressed in source doc. Per CLAUDE.md, all new user-facing strings (Status labels, validation messages, menu item, dual-listbox labels) must go through `ILocalizationService`/locale resources regardless of whether multi-language is enabled at runtime — a coding-standard default, not a scope question. Whether staff/PMs actually see multiple languages at runtime is still open (Phase 1 Open Question Q4, unresolved). |
| Permissions | One new permission, `ManageProjects`, granted to a new Project Manager role. Phase 1's `ManageTimeLog`/Staff pairing is unaffected and the two are explicitly independent (confirmed in source). |
| Existing plugin overlap | **Decision (confirmed by stakeholder): no existing project/HR plugin to extend.** Proceed with new `Project`/`ProjectStaffMapping` entities inside the existing Time Log plugin. |
| Third-party dependency | None — fully internal feature. |

## 5. Notes for the Technical Designer (signal only — not a placement decision)
- This phase changes the *data source* for a control that already exists in Phase 1's plugin (`TimeLogController`'s project select-list and `TimeLogSearchModel` filter). Confirm whether this qualifies as "extension of an existing plugin" (most likely) rather than a new plugin, per the Placement Decision framework — this document does not assume the answer.
- The delete-guard-with-Retired-fallback behavior (§2.7) has a real design choice buried in it: automatic status flip vs. manual PM action blocked by a validation message. This materially affects controller/service design and should be resolved before implementation, not assumed.
- Status-driven time-logging eligibility (§2.8) must be enforced in the same server-side layer as the Phase 1 project-dropdown restriction (§2.6) — both are join/filter conditions against the same query, not separate checks layered ad hoc.

## 6. Decisions (confirmed by stakeholder, 2026-09-15 — resolves prior Open Questions Q1–Q3, Q5–Q7)

1. **Project Details screen** — No separate view; the Edit screen doubles as the details view.
2. **Delete-guard / Retired fallback mechanics** — Automatic: on Delete with existing `TimeLogRecord`s, the system sets Status = `Retired` in place of deleting and informs the user.
3. **Status transition rules** — Free-form; no restrictions between the six enum values (other than the system-driven Retired transition in item 2).
4. **Staff role removal vs. project mapping** — Mapping rows persist untouched; unassignment from a project is a manual Project Manager action only.
5. **Multi-store scoping** — Project is per-store scoped via `StoreMapping`; admin grid and the staff dropdown/filter must respect current store context.
6. **Existing plugin overlap** — None; build new `Project`/`ProjectStaffMapping` entities in the existing Time Log plugin.

## 7. Remaining Open Question (not yet resolved)

1. **Assigned-staff count display (former Q4)** — Source doc marks the staff-count column on the Project List as optional ("optionally assigned staff count"). Should it be included, and if so as a plain number, a "3 of 12" style, or a hover/expandable list? **Recommend the technical designer default to a simple numeric count column** (lowest-risk, no new UI pattern) unless the user specifies otherwise before implementation.
