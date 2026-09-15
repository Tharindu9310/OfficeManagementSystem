# Acceptance Criteria — Time Log Module

**Traces to:** `docs/time-log/stories/user-stories.md` and `docs/time-log/requirements/clarified-requirement.md`
**Format:** Given-When-Then, testable

---

## AC-1 — Permission gating (Story 1, 2)

1. **Given** the `ManageTimeLog` permission does not yet exist in the system
   **When** the feature is installed
   **Then** the permission record is created and granted to the "Staff" customer role.

2. **Given** a customer does not belong to a role with `ManageTimeLog`
   **When** they view the admin menu
   **Then** the "Log Time" menu item is not rendered.

3. **Given** a customer does not have `ManageTimeLog`
   **When** they navigate directly to the Time Log grid URL
   **Then** they receive an authorization failure (not a rendered grid), consistent with `[AuthorizeAdmin]` + permission-check conventions.

4. **Given** a customer has `ManageTimeLog`
   **When** they open the admin menu
   **Then** "Log Time" is visible and clicking it opens the Time Log grid directly (no intermediate page).

---

## AC-2 — Row-level data isolation (Story 3)

1. **Given** staff user A has 3 time log entries and staff user B has 2
   **When** user A loads the Time Log grid
   **Then** only user A's 3 entries are returned, regardless of grid paging/sorting parameters.

2. **Given** staff user A crafts a grid request (e.g., modifying a hidden filter/customerId parameter in the request) attempting to view user B's data
   **When** the request reaches the server
   **Then** the server ignores/rejects any client-supplied customer identifier and filters strictly by the authenticated current user's Id.

3. **Given** staff user A knows the Id of a TimeLog entry owned by user B
   **When** user A calls the update, delete, or submit endpoint directly with that Id
   **Then** the request is rejected (not found / forbidden) and no data is changed.

---

## AC-3 — Inline insert with defaults (Story 4)

1. **Given** a staff user clicks "Add new record" in the grid
   **When** the new row appears
   **Then** the Date field is pre-filled with the current date and Status is set to Draft, both editable/visible before save.

2. **Given** a staff user fills Project, Task, and Time on the new row and moves focus away
   **When** the row is saved
   **Then** a new TimeLog record is created with `CustomerId` set server-side to the current user (not the value of any client-submitted field, since none is exposed for it), `CreatedOnUtc` and `UpdatedOnUtc` set to the current UTC time, and `Status = Draft`.

---

## AC-4 — Inline update with autosave on blur (Story 5)

1. **Given** a Draft row is open for inline editing
   **When** the staff user changes a field value and then clicks/tabs outside the row (blur), without clicking any explicit "Update" button
   **Then** the row is saved automatically via an AJAX call, and `UpdatedOnUtc` is refreshed to the current UTC time.

2. **Given** autosave-on-blur fires
   **When** the field values fail client-side Kendo validation rules
   **Then** the autosave call is not sent and the invalid cell(s) show a validation message, so invalid data is never silently persisted.

3. **Given** autosave-on-blur fires with client-valid data
   **When** the server-side model validation independently re-checks the same rules
   **Then** the server rejects and returns a validation error for any request that fails server-side rules, even if the client-side check was bypassed (e.g., via a direct AJAX call).

---

## AC-5 — Time format conversion (Story 6)

1. **Given** a TimeLog record with `Time = 6.5` (decimal)
   **When** the grid renders the Time column
   **Then** the displayed value is `06:30`.

2. **Given** a staff user enters `08:15` in the Time cell
   **When** the row is saved
   **Then** the persisted decimal value is the correct proportional conversion (`08:15` → `8.25`), and validation runs against that decimal value, not the display string.

3. **Given** a staff user enters a Time value that converts to less than 0 or more than 24 decimal hours
   **When** the row is saved (client-side) or submitted (server-side)
   **Then** the save/submit is rejected with a validation message referencing the 0–24 hour bound.

*(Note: the exact rounding rule for non-terminating conversions, e.g. 20 minutes, is an open question — see clarified-requirement.md Open Question Q6. This criterion must be finalized once that rule is confirmed.)*

---

## AC-6 — Delete a Draft entry (Story 7)

1. **Given** a Draft row owned by the current staff user
   **When** the user triggers delete on that row
   **Then** the record is permanently removed and no longer appears in the grid on next refresh.

2. **Given** a Submitted row
   **When** the user attempts to trigger delete (via UI or a direct request to the delete endpoint)
   **Then** the delete is rejected both at the UI level (delete control not available/disabled) and at the server level (endpoint re-checks `Status == Draft` and rejects otherwise).

---

## AC-7 — Submitted entries are locked (Story 8)

1. **Given** a TimeLog record with `Status = Submitted`
   **When** the grid renders that row
   **Then** the row is displayed as read-only: no inline edit affordance, no delete action, and its selection checkbox is disabled.

2. **Given** a Submitted record
   **When** a request is sent directly to the update endpoint with a modified field value for that record's Id (bypassing the grid UI)
   **Then** the server rejects the update because the record's current `Status` is not `Draft`, and no field is changed.

3. **Given** a Submitted record
   **When** a request is sent directly to the submit endpoint for that record's Id
   **Then** the server rejects it (already Submitted, not eligible) and the status remains unchanged.

4. **Given** Phase 1 has no un-submit action
   **When** any endpoint is called attempting to move a record from Submitted back to Draft
   **Then** no such endpoint exists / any such request is rejected — this transition is unsupported by design.

---

## AC-8 — Grid filters (Story 9)

1. **Given** a staff user has entries across multiple dates, statuses, and projects
   **When** they apply a time-range filter (from/to)
   **Then** only entries whose Date (or Time value, per whichever field the filter targets as defined in design) falls within that range are shown.

2. **Given** the same data set
   **When** the user filters by Status = Draft
   **Then** only Draft rows for that user are shown; Submitted rows are excluded.

3. **Given** the same data set
   **When** the user filters by a specific Project
   **Then** only rows referencing that Project are shown.

4. **Given** multiple filters are applied together (time range + status + project)
   **When** the grid refreshes
   **Then** all filters apply as a combined AND condition, still scoped to the current user only (AC-2 applies regardless of filter state).

---

## AC-9 — Bulk Submit (Story 10)

1. **Given** a staff user has 2 valid Draft rows and 1 Submitted row
   **When** they view the grid
   **Then** only the 2 Draft rows show an enabled/selectable checkbox; the Submitted row's checkbox is disabled.

2. **Given** the staff user selects the 2 Draft rows and clicks "Submit"
   **When** both rows pass server-side re-validation (required fields present, Time within 0–24) and are owned by the current user
   **Then** both rows' `Status` changes to `Submitted`, `UpdatedOnUtc` is refreshed for each, and the grid refreshes to show both as read-only.

3. **Given** the staff user selects 2 Draft rows where one has an invalid state (e.g., Time missing or out of range) at submit time
   **When** they click "Submit"
   **Then** the valid row is submitted and the invalid row is rejected with a validation message identifying it, and the invalid row remains Draft and editable.

4. **Given** a selected row's Id does not belong to the current user (e.g., tampered request)
   **When** the submit endpoint processes it
   **Then** that record is rejected/skipped and no other user's data is changed.

5. **Given** the "Submit" button
   **When** zero rows are selected
   **Then** the button is disabled (or a click with no selection is a no-op) and no request is sent.

---

## AC-10 — Field validation (Story 11)

1. **Given** a row being edited
   **When** Project is left empty or set to a non-existent/inactive project Id
   **Then** save is rejected client-side (if caught before send) and server-side (authoritative), with a message indicating Project is required and must reference a valid project.

2. **Given** a row being edited
   **When** Task is left empty
   **Then** save is rejected with a "Task is required" message, both client- and server-side.

3. **Given** a row being edited
   **When** Time is empty, non-numeric, negative, or greater than 24
   **Then** save is rejected with a message stating the 0–24 hour bound, both client- and server-side.

4. **Given** a row being edited
   **When** Date is empty
   **Then** save is rejected with a "Date is required" message; when Date is provided (including a user-edited non-default date), the save proceeds assuming all other fields are valid.

5. **Given** any of the above validations fail during an autosave-on-blur trigger
   **When** the failure occurs
   **Then** no partial/invalid record is persisted, and the row remains in edit mode with the validation message visible until corrected or the edit is cancelled.

---

## Traceability Summary

| AC group | Story | Requirement source |
|---|---|---|
| AC-1 | Story 1, 2 | §2, §3 |
| AC-2 | Story 3 | §2, §5.3 |
| AC-3 | Story 4 | §4, §5.2 |
| AC-4 | Story 5 | §5.2 |
| AC-5 | Story 6 | §5.5, §6 |
| AC-6 | Story 7 | §5.4 |
| AC-7 | Story 8 | §5.4, §6 |
| AC-8 | Story 9 | §5.2 |
| AC-9 | Story 10 | §5.6, §6 |
| AC-10 | Story 11 | §6, §5.2 |

## Unresolved Dependencies
AC-5 (rounding rule) and AC-6 (delete confirmation UX) reference open questions in `docs/time-log/requirements/clarified-requirement.md` §6 that should be confirmed before these criteria are treated as final/frozen for development.
