# User Stories — Time Log Module

**Traces to:** `docs/time-log/requirements/clarified-requirement.md`
**nopCommerce version:** 4.90.8 (admin area only)
**Format:** As a / I want / So that (per `.claude/instructions/nopcommerce-story-format.md`)

---

## Story 1 — Role & permission gating

**As a** store administrator
**I want** a dedicated `ManageTimeLog` permission granted to the "Staff" customer role
**So that** only staff members can see or use the Time Log feature, and I can control access the same way I control any other admin permission

### nopCommerce Context
- Admin area only.
- Requires confirming whether the "Staff" role already exists (source doc assumes it does, create if missing — see clarified-requirement.md §2.1).

### Traceability
- Requirement: source doc §2 (Roles & Permissions)
- Open question: none blocking — role creation-if-missing is explicit in source doc

---

## Story 2 — Admin navigation entry

**As a** staff member
**I want** a "Log Time" menu item in the admin panel that only appears if I have the `ManageTimeLog` permission
**So that** I can get straight to my time log grid without navigating through unrelated admin screens

### nopCommerce Context
- Admin area only. Menu item visibility gated by `ManageTimeLog`.

### Traceability
- Requirement: source doc §3 (Navigation)
- Depends on: Story 1 (permission must exist first)

---

## Story 3 — View own time log entries only

**As a** staff member
**I want** the Time Log grid to show only the entries I created
**So that** I cannot see or be distracted by other staff members' time entries, and my data stays private

### Acceptance criteria intent
Row-level isolation must be enforced server-side on every read, not just hidden in the UI.

### Traceability
- Requirement: source doc §2 (Data visibility), §5.3 (Scope)

---

## Story 4 — Add a new Draft time entry inline

**As a** staff member
**I want** to add a new time log row directly in the grid with Date defaulted to today and Status defaulted to Draft
**So that** I can quickly record time worked without leaving the grid or filling a separate form

### Traceability
- Requirement: source doc §4 (Data Model — Date default), §5.2 (Inline insert)

---

## Story 5 — Edit a Draft entry inline with autosave on blur

**As a** staff member
**I want** my edits to a Draft row to save automatically when I click away from the row, without needing to click a separate "Update" button
**So that** logging time is fast and I don't lose entries by forgetting to save

### nopCommerce Context
- This is an explicit deviation from nopCommerce's default Kendo grid save pattern (which normally requires an explicit Save/Update click) and requires custom grid JavaScript on the row-leave/blur event.

### Traceability
- Requirement: source doc §5.2 (Autosave on blur)

---

## Story 6 — Time entered/displayed as HH:mm, stored as decimal

**As a** staff member
**I want** to enter and see time in `HH:mm` format (e.g., `06:30`)
**So that** I can log hours the way I naturally think about time, while the system still validates against the 0–24 decimal-hour rule

### Acceptance criteria intent
Conversion happens only in the UI/binding layer; storage and validation stay decimal-based.

### Open dependency
Rounding/precision rule for non-terminating decimal conversions (e.g., 20 minutes) is unresolved — see clarified-requirement.md Open Question Q6. This story's acceptance criteria assume a 2-decimal-place rounding for illustration only; the exact rule must be confirmed before implementation.

### Traceability
- Requirement: source doc §5.5 (Time display/entry format), §6 (Validation — Time)

---

## Story 7 — Delete a Draft entry

**As a** staff member
**I want** to delete a time log entry while it is still in Draft status
**So that** I can remove mistaken or unwanted entries before they are locked

### Open dependency
Whether a delete confirmation prompt is required is unresolved — see clarified-requirement.md Open Question Q7.

### Traceability
- Requirement: source doc §5.4 (Status-based lock — Draft deletable)

---

## Story 8 — Submitted entries are locked

**As a** staff member
**I want** my Submitted time log entries to become fully read-only (no edit, no delete, no checkbox selection)
**So that** once I've submitted my hours, they can't be accidentally changed by me or bypassed through a direct request

### Acceptance criteria intent
Lock must be enforced independently by both the grid UI and the server-side insert/update/delete/submit endpoints, so a crafted request against a Submitted record is rejected regardless of what the UI allowed.

### Traceability
- Requirement: source doc §5.4 (Status-based lock), §6 (Validation — Status transition)

---

## Story 9 — Filter the grid by time range, status, and project

**As a** staff member
**I want** to filter my time log grid by a date/time range, status, and project
**So that** I can quickly find specific entries without scrolling through my whole history

### Traceability
- Requirement: source doc §5.2 (Filters)

---

## Story 10 — Bulk Submit selected Draft entries

**As a** staff member
**I want** to select multiple Draft entries and submit them all at once
**So that** I can lock in a batch of completed time entries (e.g., at the end of a week) in one action instead of one at a time

### Acceptance criteria intent
- Only Draft rows are selectable.
- Server re-validates each selected record (required fields, 0–24 range) and re-checks ownership before flipping status; invalid or not-owned records are not submitted.
- Grid refreshes afterward so newly Submitted rows render read-only immediately.

### Traceability
- Requirement: source doc §5.6 (Bulk Submit), §6 (Validation)

---

## Story 11 — Field-level validation on insert/update

**As a** staff member
**I want** the grid to tell me immediately if I leave Project, Task, Time, or Date invalid or empty
**So that** I don't lose my entry or accidentally save bad data through autosave-on-blur

### nopCommerce Context
- Client-side Kendo grid validation rules AND server-side model validation are both required — client-side validation exists to catch errors before autosave fires, but server-side is the authoritative, non-bypassable check per CLAUDE.md security standards.

### Traceability
- Requirement: source doc §6 (Validation), §5.2 (autosave risk of silently persisting bad data)

---

## Open Questions Affecting Story Completeness
See `docs/time-log/requirements/clarified-requirement.md` §6 for the full list (admin-oversight visibility, Project entity provenance, multi-store scoping, localization scope, existing plugin overlap, time rounding rule, delete confirmation). No story above assumes an answer to these beyond what is explicitly stated in the source requirement doc.
