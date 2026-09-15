# Acceptance Criteria — Time Log Module, Phase 2 (Project Management)

**Traces to:** `docs/time-log/stories/phase2-project-management.md` and `docs/time-log/requirements/phase2-project-management.md`
**Format:** Given-When-Then, testable

---

## AC-P2-1 — Project Manager role & permission gating (Story P2-1, P2-2)

1. **Given** the "Project Manager" role and `ManageProjects` permission do not yet exist
   **When** the plugin is installed (or upgraded from a Phase 1-only install)
   **Then** both are created, and `ManageProjects` is granted to the Project Manager role.

2. **Given** a customer holds `ManageProjects` but not `ManageTimeLog`
   **When** they log into admin
   **Then** they see the "Project" menu item but not the Phase 1 "Log Time" menu item, and a direct request to the time log grid endpoint is rejected.

3. **Given** a customer holds `ManageTimeLog` but not `ManageProjects`
   **When** they log into admin
   **Then** they see "Log Time" but not "Project", and a direct request to any Project controller action is rejected.

4. **Given** a customer holds both `ManageProjects` and `ManageTimeLog` (e.g., a Project Manager who is also Staff)
   **When** they log into admin
   **Then** both menu items are visible and both areas are usable independently.

5. **Given** a customer without `ManageProjects`
   **When** they view the admin menu
   **Then** the "Project" child item under "Time Log" is not rendered.

---

## AC-P2-2 — Project List (Story P2-3)

1. **Given** 3 existing projects with different Statuses
   **When** a Project Manager opens the Project List
   **Then** all 3 rows render with Name, Start Date, End Date, and Status, regardless of paging/sorting.

2. **Given** a project with a null End Date
   **When** it renders in the list
   **Then** the End Date column shows an empty/"ongoing" indicator rather than an error or a default date.

*(Note: an assigned-staff-count column's inclusion and format are unresolved — see phase2-project-management.md Open Question Q4. This AC group does not assume it.)*

---

## AC-P2-3 — Create a project (Story P2-4, P2-8)

1. **Given** a Project Manager submits Name, Start Date, and Status with End Date and Description left blank
   **When** the form is saved
   **Then** a new Project record is created with `CreatedOnUtc`/`UpdatedOnUtc` set, `EndDate` stored as null, and it appears in the Project List.

2. **Given** Name is left empty
   **When** the form is submitted
   **Then** the save is rejected client-side (if caught) and server-side, with a message indicating Name is required.

3. **Given** Start Date is left empty
   **When** the form is submitted
   **Then** the save is rejected with a message indicating Start Date is required.

4. **Given** an End Date is provided that is earlier than the Start Date
   **When** the form is submitted
   **Then** the save is rejected with a message indicating End Date must be on or after Start Date.

5. **Given** Status is not one of the six defined enum values (e.g., a tampered/out-of-range posted value)
   **When** the form is submitted server-side
   **Then** the save is rejected — server-side validation does not trust an arbitrary posted integer.

---

## AC-P2-4 — Staff assignment via dual-listbox (Story P2-5, P2-6)

1. **Given** a Project Manager is on the Create/Edit screen
   **When** the staff picker loads
   **Then** the "available" list contains every customer currently in the Staff role (including one who is also a Project Manager), and the "assigned" list contains only those already mapped to this project (empty on Create).

2. **Given** a Project Manager moves 2 staff members from "available" to "assigned" and saves
   **When** the save completes
   **Then** 2 new `ProjectStaffMapping` rows exist for this Project's Id and those 2 Customer Ids, and no other mapping rows are created.

3. **Given** a project already has 3 staff assigned and the Project Manager removes 1 in the picker and saves
   **When** the save completes
   **Then** the removed staff member's mapping row no longer exists, the other 2 remain unchanged, and no `TimeLogRecord` rows are altered as a result (see AC-P2-8).

4. **Given** a Project Manager saves a new project with zero staff moved to "assigned"
   **When** the save completes
   **Then** the Project record is created successfully with zero `ProjectStaffMapping` rows, and no validation error is raised for the empty assignment.

---

## AC-P2-5 — Project Details (Story P2-7)

1. **Given** a project with 2 assigned staff members
   **When** a Project Manager opens its details (whether a distinct view or the Edit screen — see Open Question Q1)
   **Then** the screen displays Name, Start Date, End Date, Description, Status, and the names of both assigned staff members.

*(Note: the exact screen shape — separate read-only Details view vs. reused Edit screen — is unresolved per Open Question Q1; this criterion is written to hold under either resolution and must be revisited once confirmed.)*

---

## AC-P2-6 — Staff time-log dropdown restricted to assigned projects (Story P2-9)

1. **Given** staff member A is assigned to Projects 1 and 2 (both `Not Started` or `Inprogress`), and Project 3 exists but A is not assigned to it
   **When** A opens the time log grid and inserts/edits a row
   **Then** the Project dropdown offers only Projects 1 and 2, not Project 3.

2. **Given** staff member A is assigned to Project 4, which is not currently assigned to staff member B
   **When** B opens their time log grid
   **Then** B's Project dropdown does not include Project 4.

---

## AC-P2-7 — Time log grid Project filter restricted to assigned projects (Story P2-10)

1. **Given** staff member A is assigned to Projects 1 and 2 only
   **When** A opens the grid's toolbar Project filter dropdown
   **Then** only Projects 1 and 2 appear as filter options, matching the insert/edit dropdown from AC-P2-6.

---

## AC-P2-8 — Only Not Started / Inprogress projects accept new time entries (Story P2-11, P2-12)

1. **Given** staff member A is assigned to Project 5 with Status `On Hold`
   **When** A opens the time log grid's Project dropdown
   **Then** Project 5 does not appear, even though A is assigned to it.

2. **Given** staff member A is assigned to Project 6 with Status `Completed`, `Cancelled`, or `Retired`
   **When** A opens the time log grid's Project dropdown
   **Then** Project 6 does not appear in any of those three Status states.

3. **Given** staff member A crafts a direct insert/update request against Project 5 (`On Hold`, assigned) or Project 3 (`Not Started`/`Inprogress`, not assigned)
   **When** the request reaches the server
   **Then** it is rejected in both cases — the server independently re-validates both the assignment condition and the Status-eligibility condition, not just the assignment condition alone.

4. **Given** staff member A is assigned to Project 1 (`Inprogress`)
   **When** A submits a valid insert against Project 1
   **Then** the request succeeds, confirming the eligibility check is a filter, not a blanket block.

---

## AC-P2-9 — Unassignment does not affect historical time logs (Story P2-13)

1. **Given** staff member A has 4 existing `TimeLogRecord` rows (mixed Draft/Submitted) against Project 2, and is currently assigned to Project 2
   **When** a Project Manager removes A from Project 2's staff assignment and saves
   **Then** all 4 of A's existing `TimeLogRecord` rows remain unchanged (same ProjectId, Status, Time, etc.) and still visible in A's grid.

2. **Given** the state from step 1 (A no longer assigned to Project 2)
   **When** A opens the time log grid
   **Then** Project 2 no longer appears in A's insert/edit dropdown or toolbar filter (per AC-P2-6/AC-P2-7), but A's 4 existing rows referencing Project 2 still display correctly, and any that are still Draft remain editable/deletable per Phase 1 rules.

---

## AC-P2-10 — Project deletion guard (Story P2-14)

1. **Given** a project has zero `TimeLogRecord` rows referencing it (any status)
   **When** a Project Manager triggers Delete
   **Then** the project is deleted successfully and its `ProjectStaffMapping` rows are removed as part of the same operation.

2. **Given** a project has at least one `TimeLogRecord` row referencing it, in Draft status only
   **When** a Project Manager triggers Delete
   **Then** the deletion is blocked with a validation message indicating time has been logged against this project, and the project record is not removed.

3. **Given** a project has at least one `TimeLogRecord` row in Submitted status
   **When** a Project Manager triggers Delete
   **Then** the deletion is blocked the same as step 2 — the guard applies regardless of the referencing record's Draft/Submitted status.

4. **Given** a delete attempt is blocked per step 2/3
   **When** the mechanism for the Retired-status fallback is confirmed (see Open Question Q2)
   **Then** this criterion will specify either (a) the system automatically sets Status to `Retired` as part of the blocked-delete response, or (b) the Project Manager must separately edit the project and set Status to `Retired` themselves — not yet finalized, do not treat either as implemented until confirmed.

---

## Traceability Summary

| AC group | Story | Requirement source |
|---|---|---|
| AC-P2-1 | P2-1, P2-2 | §2.1, §2.2 |
| AC-P2-2 | P2-3 | §2.4 (Project List) |
| AC-P2-3 | P2-4, P2-8 | §2.3, §2.5 |
| AC-P2-4 | P2-5, P2-6 | §2.3, §2.4 |
| AC-P2-5 | P2-7 | §2.4 (Project Details) |
| AC-P2-6 | P2-9 | §2.6 |
| AC-P2-7 | P2-10 | §2.6 |
| AC-P2-8 | P2-11, P2-12 | §2.6, §2.8 |
| AC-P2-9 | P2-13 | §2.6 (Unassignment behavior) |
| AC-P2-10 | P2-14 | §2.7 |

## Unresolved Dependencies
AC-P2-5 (Details screen shape) and AC-P2-10 item 4 (Retired-fallback mechanism) reference open questions in `docs/time-log/requirements/phase2-project-management.md` §6 that must be confirmed before these criteria are treated as final/frozen for development. AC-P2-2's staff-count column is likewise contingent on Open Question Q4.
