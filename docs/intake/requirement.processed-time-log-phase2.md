# nopCommerce Plugin – Time Log Module
## Requirements Specification – Phase 2 (Project Management)

*Builds on `TimeLog-Plugin-Requirements-Phase1-v2.md`. This phase adds project setup and staff assignment, and updates one behavior from Phase 1 (the Project dropdown in the time log grid).*

---

## 1. Overview
Adds a **Project** management screen under the existing "Time Log" admin menu, restricted to a new **Project Manager** role. Project Managers create projects and assign staff members to them. Staff members' Project dropdown in the Phase 1 time log grid is now restricted to only the projects they've been assigned to.

---

## 2. Roles & Permissions

| Item | Detail |
|---|---|
| New role | **Project Manager** customer role (create if it doesn't exist) |
| New ACL permission | e.g. `ManageProjects` — granted to Project Manager role; controls visibility of the new menu item and its screens |
| Existing permission | `ManageTimeLog` (Phase 1) stays scoped to Staff — Project Manager does **not** automatically get time-log access unless also given the Staff role/permission |
| Data visibility | Project Manager can see/manage **all** projects (no ownership restriction described) |

---

## 3. Navigation

- New menu item **"Project"** added under the existing **"Time Log"** parent menu, visible only to customers with the `ManageProjects` permission.
- Leads to a project list screen with create/edit access.

---

## 4. Data Model

### 4.1 Project

| Field | Type | Notes |
|---|---|---|
| Id | int | PK |
| Name | nvarchar | Required |
| StartDate | datetime/date | Required |
| EndDate | datetime/date, nullable | Optional — open-ended projects allowed |
| Description | nvarchar | Optional |
| Status | int/enum | Values not yet defined — see Open Questions |
| CreatedOnUtc | datetime | Audit |
| UpdatedOnUtc | datetime | Audit |

### 4.2 Project–Staff mapping (many-to-many)

| Field | Type | Notes |
|---|---|---|
| Id | int | PK |
| ProjectId | int | FK to Project |
| CustomerId | int | FK to Customer (a Staff-role user) |

- A staff member can be assigned to **multiple** projects (confirmed).
- A project can obviously have multiple staff members — standard many-to-many mapping table, same pattern nopCommerce already uses for e.g. Customer↔CustomerRole or Product↔Category mappings.

---

## 5. Screens & Behavior

### 5.1 Project List
- Standard nopCommerce admin list/grid: Name, Start Date, End Date, Status, (optionally assigned staff count).
- "Add new" button → Create screen (standard nopCommerce create/edit form pattern is a better fit here than the Phase 1 inline-grid style, since assigning staff needs more room than a grid cell).

### 5.2 Create / Edit Project
- Fields: Name, Start Date, End Date (nullable — leave blank for ongoing), Description, Status.
- **Assign staff members**: multi-select control (e.g., a dual-listbox "available / assigned" picker, consistent with nopCommerce's existing patterns for assigning customer roles or store mappings) listing all Staff-role customers, letting the Project Manager check/add multiple staff to the project.
- Save persists both the Project record and the Project–Staff mapping rows (insert/remove mappings to match the selected set).

### 5.3 Project Details
- Viewing a project (or the edit screen itself, if no separate read-only view is needed) shows project fields **plus the list of currently assigned staff members**.

---

## 6. Validation

| Field | Rule |
|---|---|
| Name | Required, non-empty |
| Start Date | Required |
| End Date | Optional; if provided, must be **on or after** Start Date |
| Status | Required; values TBD — see Open Questions |
| Assigned staff | Optional — a project **can be saved with no staff assigned** (confirmed) |

---

## 7. Impact on Phase 1 — Time Log Project Dropdown

- The **Project dropdown** in the staff time log grid (Phase 1, section 4/7) must be updated: instead of listing all active projects, it now lists **only projects the current staff member is assigned to** via the Project–Staff mapping.
- This filter applies to:
  - The dropdown itself (insert/edit row).
  - The **Project filter** in the grid toolbar (Phase 1, section 5.2) — should likewise only offer the staff member's own assigned projects, not the full project list.
- Server-side enforcement required as before: even if a ProjectId outside the staff member's assignments is posted directly, the insert/update action must reject it.
- **Unassignment behavior (confirmed)**: if a staff member is later removed from a project, their existing time log entries against that project **remain unchanged** — unassignment only affects the dropdown/filter (what they can log going forward), not historical records. The project simply drops out of their selectable list; past entries are untouched and still visible/editable per their normal Draft/Submitted status rules from Phase 1.

---

## 8. Technical Implementation Notes (nopCommerce-specific)

1. **Entities & migration**: `Project` entity and `ProjectStaffMapping` entity, each with a FluentMigrator migration; extend the plugin's existing migration set from Phase 1.
2. **Permission**: new `PermissionRecord` `ManageProjects`, mapped to the Project Manager role on install (or on a plugin update/upgrade step if the plugin's already installed from Phase 1).
3. **Role**: check for/create the "Project Manager" `CustomerRole` on install, same pattern nopCommerce uses for its built-in roles.
4. **Admin menu**: extend the existing `ManageSiteMapAsync`/menu hook from Phase 1 to add the "Project" child node under "Time Log", gated on `ManageProjects`.
5. **Controller**: `ProjectController` under `Areas/Admin`, `[AuthorizeAdmin]` + permission attribute. Actions: `List`, `ProjectList` (AJAX grid read), `Create`, `Edit` (GET/POST), `Delete`. Edit/Create POST handles syncing the mapping table against the submitted staff selection.
6. **Staff multi-select**: populate from Customers in the Staff role (existing `ICustomerService` filtered by role), rendered via nopCommerce's standard multi-select/listbox admin component. Because a user can now hold both Project Manager and Staff roles (confirmed), this list is filtered by **role membership**, not by excluding the current user.
7. **Phase 1 controller change**: update `TimeLogController`'s project select-list logic (and the `TimeLogSearchModel` project filter) to join through `ProjectStaffMapping` filtered by `CustomerId == workContext.CurrentCustomer.Id`, replacing the Phase 1 "all active projects" logic.
8. **Project deletion guard**: `Delete` action must check for any `TimeLogRecord` referencing the project (regardless of Draft/Submitted status) and **block deletion** with a validation message if any exist — confirmed requirement, not just a suggested pattern. Consider whether a "deactivate via Status" path is the intended alternative for projects that have logged time (see Open Questions on Status values).
9. **Status select list**: use these status `Not Started`, `Inprogress` / `On Hold` / `Completed` / `Cancelled`/ `Retired`, expose via an enum-to-selectlist helper (same pattern as Phase 1's time log Status), used in both the grid column and the create/edit form.  only active and `Not Started`, `Inprogress` available to log time
**Delete vs Status interplay**: If project has time logged it should be sent to `Retired` status.that will allow to keep existing logged time.
---
## nopCommerce Version

4.90.8
