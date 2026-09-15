# Requirement

<!-- Describe what you need in plain text. Be as detailed as you like -
     this is read as the requirement input to the orchestrator, so more
     detail here means fewer clarifying questions from the BA agent. -->

# nopCommerce Plugin – Time Log Module
## Requirements Specification (Phase 1)

---

## 1. Overview
A back-office plugin that lets **registered users with the "Staff" role** log time spent against predefined projects. Each staff member can only view and manage their own time entries through an editable grid in the admin panel, and submit completed entries for locking via a bulk action.

---

## 2. Roles & Permissions

| Item | Detail |
|---|---|
| Applicable role | Existing "Staff" customer role (assumed already defined; create if missing) |
| New ACL permission | e.g. `ManageTimeLog` — granted to the Staff role so the menu/page is visible only to them |
| Data visibility | Row-level filter: `TimeLog.CustomerId == CurrentCustomer.Id` enforced server-side (not just UI-hidden) — staff must never be able to see or edit another user's rows, even via direct URL/grid parameter tampering |

---

## 3. Navigation

- New admin menu item, e.g. **"Log Time"**, added under a top-level or existing menu node, visible only to customers with the `ManageTimeLog` permission.
- Clicking it opens the Time Log list page directly (grid), scoped to the logged-in staff user.

---

## 4. Data Model

| Field | Type | Notes |
|---|---|---|
| Id | int | PK |
| CustomerId | int | FK to Customer, set server-side from current logged-in user — never editable/selectable by the user |
| ProjectId | int | FK to a Project entity/table (predefined list, dropdown) |
| Task | nvarchar | Free text |
| Description | nvarchar | Free text |
| Date | datetime/date | The day the time is logged against. **Defaults to the current date** on new/insert rows |
| Time | decimal | Hours logged internally as decimal; **min 0, max 24**. Displayed/entered in time format (e.g., `6.5` → `06:30`) — see 5.5 |
| Status | int/enum | `Draft` (0), `Submitted` (1) |
| CreatedOnUtc | datetime | Audit |
| UpdatedOnUtc | datetime | Audit, updated on every autosave |

**Project source:** a predefined, presumably admin-managed list. Phase 1 assumes Projects already exist (seeded or managed elsewhere) — dropdown simply lists active projects.

---

## 5. Grid Requirements (Kendo UI grid, AJAX-bound — standard nopCommerce admin pattern)

### 5.1 Columns
1. **Checkbox** (first column) — row selection, used for bulk Submit (see 5.6)
2. Date (defaults to current date on new rows)
3. Project (dropdown-bound, shows Project name)
4. Task (text)
5. Description (text)
6. Time (numeric, displayed as `HH:mm`)
7. Status (dropdown/badge)

### 5.2 Behavior
- **Inline insert**: "Add new record" row at top/bottom, same pattern as standard nopCommerce Kendo grids. Date column pre-filled with today's date; Status defaults to `Draft`.
- **Inline update**: click a cell to edit in place.
- **Autosave on blur**: when the user clicks away from an editing row (not a manual "Update" button click), the row must save automatically. This is a deviation from nopCommerce's default Kendo grid behavior (which normally requires an explicit Save action) — needs custom JS to hook the grid's row-leave/blur event and call `.saveRow()` programmatically.
- **Filters** (grid toolbar):
  - Time range (from–to)
  - Status
  - Project

### 5.3 Scope
- Grid data source always filtered server-side to the current staff user's records — the staff user is not a selectable filter/column since there's only ever one user's data in view.

### 5.4 Status-based lock
- **Draft** records: fully editable inline, and **deletable** by the owning staff user.
- **Submitted** records: **read-only** — no inline edit, no delete, checkbox disabled/unselectable. Grid should render Submitted rows non-editable and the update/delete/submit AJAX endpoints must independently re-check status server-side and reject the change even if a request is crafted directly (never rely on the grid UI alone to enforce this).
- Phase 1 has no "un-submit" action — once a record is Submitted it is locked permanently unless a later phase adds one.

### 5.5 Time display/entry format
- Stored as `decimal` (hours).
- Displayed and entered as `HH:mm`-style time (e.g., `6.5` decimal ⇄ `06:30` displayed). Conversion happens at the UI/binding layer (Kendo column template + parse function), not in the stored value — DB and validation (0–24 range) stay decimal-based.
- The decimal fraction maps to minutes as a proportion of an hour (0.5 hour = 30 minutes) — standard decimal-hours-to-time conversion.

### 5.6 Bulk Submit
- Checkbox in the first column of each row (Draft rows only — see 5.4).
- A **"Submit"** button above the grid, enabled once at least one row is checked.
- Clicking it sends the selected record IDs to a dedicated endpoint (e.g., `TimeLogSubmit`) that:
  - Re-validates each record server-side (required fields, 0–24 range) before flipping status — a Draft row must be valid to be submitted, not just checked.
  - Changes `Status` from `Draft` → `Submitted` for each valid selected record owned by the current user only.
  - Refreshes the grid afterward so those rows become read-only immediately.
- This bulk action is the only way a record moves from Draft to Submitted in Phase 1 (no per-row/inline Status dropdown edit that submits directly).

---

## 6. Validation

| Field | Rule |
|---|---|
| Project | Required — must select a valid, existing project |
| Task | Required, non-empty string |
| Time | Required; numeric; **0 ≤ Time ≤ 24** (decimal hours; entered/displayed as `HH:mm`) |
| Date | Required; defaults to current date but editable |
| Status transition | Field edits/delete only permitted while record is `Draft`; submit action only valid on `Draft` rows owned by the current user; once `Submitted`, all changes must be rejected server-side |

Validation should run both **client-side** (Kendo grid validation rules, so autosave-on-blur doesn't silently persist bad data) and **server-side** (model validation in the AJAX insert/update/submit actions — never trust the client, especially with autosave).

## nopCommerce Version

4.90.8

## Related Existing Feature (optional - fill in if this is a CHANGE, not new work)
#1
Search area	
 Date filter should select only date
 error when minimize search area " System.ArgumentNullException: 'Value cannot be null. (Parameter 'name')'"
 
Add Time Entry
 Add Time Entry area should be minimizable like search area
 time field shold math the format HH:mm. 
 validation should point the correct error field and mark text border as red when validation failed.no need alert or validation errors.
 error should show under relevent field.ex: Task missing erros should display under task text box and text box border should be red until it type
 required fileds should marked
 
 
In the grid 
	date should be date only.
	date alignent left
	project also need to editable once edit button clicked
	edit and delete buttons should be in one column called actions
	no need names for edit and delete buttons only icons

  #2

In the grid 
	The record was not found or does not belong to you. when edit and save
	project dropdown not editable by default
	Date column shold show date picker when edit
	add icon before Add Time Entry text
	
Time Logs (All Staff)
Search area
	Date should be only date selection
	
grid
	Date should left align and need to show date without time.

	#3
	Time Logs (All Staff)
	grid only contain submited records