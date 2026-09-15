# User Stories — Time Log Module, Phase 2 (Project Management)

**Traces to:** `docs/time-log/requirements/phase2-project-management.md`
**nopCommerce version:** 4.90.8 (admin area only)
**Format:** As a / I want / So that (per `.claude/instructions/nopcommerce-story-format.md`)

---

## Story P2-1 — Project Manager role & permission gating

**As a** store administrator
**I want** a dedicated "Project Manager" customer role with a `ManageProjects` permission (created on install if they don't already exist)
**So that** only designated managers can access project setup screens, independent of who has time-log (Staff) access

### nopCommerce Context
- Admin area only. `ManageProjects` is a distinct permission from Phase 1's `ManageTimeLog` — a user may hold either, both, or neither role.

### Traceability
- Requirement: phase2-project-management.md §2.1
- Design placement note: extension of the existing Time Log plugin's install/upgrade step (adds role + permission alongside Phase 1's Staff role/`ManageTimeLog`)

---

## Story P2-2 — "Project" admin navigation entry

**As a** Project Manager
**I want** a "Project" menu item under the existing "Time Log" admin menu, visible only if I have `ManageProjects`
**So that** I can reach project setup without it being visible to staff who only log time

### Traceability
- Requirement: phase2-project-management.md §2.2
- Depends on: Story P2-1 (permission must exist first)

---

## Story P2-3 — View the Project list

**As a** Project Manager
**I want** a grid listing all projects with Name, Start Date, End Date, and Status
**So that** I can see everything at a glance and open any project to edit it

### Open dependency
Whether an assigned-staff-count column is included, and its display format, is unresolved — see phase2-project-management.md Open Question Q4.

### Traceability
- Requirement: phase2-project-management.md §2.4 (Project List)

---

## Story P2-4 — Create a new project

**As a** Project Manager
**I want** to create a project with Name, Start Date, End Date (optional), Description, and Status
**So that** I can set up a new project before assigning staff or having anyone log time against it

### Traceability
- Requirement: phase2-project-management.md §2.3 (Project entity), §2.5 (Validation)

---

## Story P2-5 — Assign staff to a project via dual-listbox

**As a** Project Manager
**I want** an available/assigned dual-listbox picker listing all Staff-role customers when creating or editing a project
**So that** I can control which staff members are allowed to log time against this project, and change that set later as needed

### nopCommerce Context
- Staff list is filtered by Staff-role membership, not by excluding the current user — a user may hold both Project Manager and Staff roles.
- Save syncs `ProjectStaffMapping` rows (inserts new, removes deselected) to match the submitted selection — not append-only.

### Traceability
- Requirement: phase2-project-management.md §2.3 (ProjectStaffMapping), §2.4 (Create/Edit)

---

## Story P2-6 — Save a project with no staff assigned

**As a** Project Manager
**I want** to save a project even if I haven't assigned any staff to it yet
**So that** I can set up projects ahead of staffing decisions without being blocked

### Traceability
- Requirement: phase2-project-management.md §2.5 (Assigned staff — optional)

---

## Story P2-7 — View project details including assigned staff

**As a** Project Manager
**I want** to see a project's full details together with its currently assigned staff members
**So that** I can confirm who is staffed on a project without cross-referencing separate screens

### Open dependency
Whether this is a distinct read-only Details view or the Edit screen reused for display is unresolved — see phase2-project-management.md Open Question Q1.

### Traceability
- Requirement: phase2-project-management.md §2.4 (Project Details)

---

## Story P2-8 — Validate project fields on save

**As a** Project Manager
**I want** the system to reject a save if Name is empty, Start Date is missing, End Date is before Start Date, or Status is not one of the defined values
**So that** I cannot create inconsistent or incomplete project records

### Traceability
- Requirement: phase2-project-management.md §2.5 (Validation)

---

## Story P2-9 — Staff time-log dropdown restricted to assigned projects

**As a** staff member
**I want** the Project dropdown in my time log grid (insert/edit row) to list only projects I've been assigned to
**So that** I can't accidentally log time against a project I have no involvement in

### nopCommerce Context
- This changes Phase 1's `TimeLogController` project select-list logic from "all active projects" to a join through `ProjectStaffMapping` filtered by the current user's Id.
- Combined with Story P2-11, the dropdown must further exclude assigned projects whose Status is not `Not Started` or `Inprogress`.

### Traceability
- Requirement: phase2-project-management.md §2.6 (Impact on Phase 1), §2.8 (Status eligibility)
- Modifies: Phase 1 Story 4 (`docs/time-log/stories/user-stories.md`)

---

## Story P2-10 — Time log grid Project filter restricted to assigned projects

**As a** staff member
**I want** the Project filter in my time log grid toolbar to offer only projects I've been assigned to
**So that** I'm not shown a filter option for a project I have no time logged against and can't log against

### Traceability
- Requirement: phase2-project-management.md §2.6 (Impact on Phase 1)
- Modifies: Phase 1 Story 9 (`docs/time-log/stories/user-stories.md`)

---

## Story P2-11 — Only Not Started / Inprogress projects accept new time entries

**As a** staff member
**I want** to be prevented from logging new time against a project that is On Hold, Completed, Cancelled, or Retired
**So that** time is only recorded against projects that are actually active

### Acceptance criteria intent
This is a status-based eligibility filter that applies in addition to (not instead of) the assignment filter from Story P2-9 — a project must satisfy both to be selectable.

### Traceability
- Requirement: phase2-project-management.md §2.8 (Status enum and eligibility)

---

## Story P2-12 — Server-side rejection of unassigned/ineligible ProjectId on time log save

**As a** system enforcing data integrity
**I want** the time log insert/update endpoints to independently re-validate that the posted `ProjectId` is both assigned to the current staff member and in an eligible Status
**So that** a directly crafted request cannot log time against a project the UI wouldn't have offered

### Traceability
- Requirement: phase2-project-management.md §2.6 (Server-side enforcement), §2.8

---

## Story P2-13 — Unassigning a staff member does not affect their historical time logs

**As a** staff member
**I want** my previously logged time entries against a project to remain visible, editable (per normal Draft rules), and unchanged even after a Project Manager removes me from that project
**So that** my historical records aren't silently altered or hidden by an administrative action I wasn't part of

### Traceability
- Requirement: phase2-project-management.md §2.6 (Unassignment behavior)

---

## Story P2-14 — Deletion of a project with logged time is blocked

**As a** Project Manager
**I want** to be prevented from deleting a project that has any time log entries against it (Draft or Submitted)
**So that** historical time-tracking data is never orphaned or lost through a project deletion

### Open dependency
Whether a blocked delete automatically transitions the project to `Retired`, or simply shows a validation message leaving the manual Status change to me, is unresolved — see phase2-project-management.md Open Question Q2.

### Traceability
- Requirement: phase2-project-management.md §2.7 (Deletion guard)

---

## Open Questions Affecting Story Completeness
See `docs/time-log/requirements/phase2-project-management.md` §6 for the full list (Project Details screen shape, delete-guard/Retired mechanics, status transition rules, staff-count display, staff-role-removal cascade, multi-store scoping, existing plugin overlap). No story above assumes an answer to these beyond what is explicitly stated in the source requirement doc.
