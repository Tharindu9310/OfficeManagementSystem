# Clarified Requirement — Time Log Module

**Source document:** `docs/intake/requirement.md`
**Target nopCommerce version:** 4.90.8 (.NET 8, stable line) — confirmed in source doc, section "nopCommerce Version".
**Feature slug:** `time-log`

## 1. Summary

Provide an admin-panel feature that lets customers in the "Staff" role log hours worked against predefined Projects, through an editable Kendo grid, with server-side row-level data isolation (a staff member sees/edits only their own entries), a Draft/Submitted status lifecycle enforced independently of the UI, and a bulk Submit action that locks selected Draft rows after re-validation.

This is a back-office (admin area) feature; there is no storefront-facing component.

## 2. In-Scope Behavior (traced to source doc sections)

### 2.1 Roles & Permissions (source §2)
- Existing "Staff" customer role is the intended audience. If it does not already exist in this store, it must be created.
- A new ACL permission record, `ManageTimeLog`, gates visibility of the menu item and the Time Log pages/actions. It is granted to the Staff role.
- `TimeLog.CustomerId` visibility/editability is restricted server-side to `CustomerId == CurrentCustomer.Id` for every read, write, delete, and submit operation — never enforced only by hiding UI elements or by a client-supplied filter parameter.

### 2.2 Navigation (source §3)
- One new admin menu item (e.g. "Log Time") visible only to customers holding `ManageTimeLog`.
- Opens directly to the Time Log grid, pre-scoped to the current user; no intermediate landing/dashboard page.

### 2.3 Data Model — `TimeLog` entity (source §4)
| Field | Type | Rule |
|---|---|---|
| Id | int | PK |
| CustomerId | int | FK to Customer; set server-side only, never client-editable/selectable |
| ProjectId | int | FK to Project; required, must reference an existing/active project |
| Task | nvarchar | Required, non-empty |
| Description | nvarchar | Optional free text |
| Date | date | Required; defaults to current date on insert; editable |
| Time | decimal | Required; range 0–24 inclusive; stored as decimal hours, displayed/entered as `HH:mm` |
| Status | enum/int | `Draft` (0) / `Submitted` (1) |
| CreatedOnUtc | datetime | Audit, set on insert |
| UpdatedOnUtc | datetime | Audit, updated on every successful save (including autosave) |

- **Project** is treated as an existing, predefined entity/table for Phase 1 — this feature only *consumes* an active-projects list for the dropdown; it does not create Project management screens. (See Open Question Q2 — whether the Project entity already exists in this store.)

### 2.4 Grid Behavior (source §5)
- Kendo UI admin grid, AJAX-bound, columns: selection checkbox, Date, Project (name), Task, Description, Time (`HH:mm` display over decimal storage), Status.
- Inline insert (new row defaults: Date = today, Status = Draft) and inline update (click-to-edit cell).
- **Autosave on blur**: leaving an editing row triggers an automatic save call — no explicit "Update" click required. This is an explicit deviation from nopCommerce's stock Kendo grid pattern and requires custom grid JS bound to the row-leave/blur event.
- Toolbar filters: time range (from–to), Status, Project. No customer/user filter is exposed (grid is always single-user scoped).
- Grid rows always come from a server-side query pre-filtered to the current user; user identity is never a client-supplied grid parameter.

### 2.5 Status Lock (source §5.4)
- Draft: editable inline and deletable by its owner.
- Submitted: read-only in the grid (no inline edit, no delete, checkbox disabled) **and** the insert/update/delete/submit AJAX endpoints independently re-check `Status == Draft` server-side before applying any change, rejecting the request otherwise — this holds even for a directly crafted/tampered request that bypasses the grid UI.
- No un-submit / revert-to-Draft action exists in Phase 1.

### 2.6 Time Format (source §5.5)
- Persisted value is always decimal hours (0–24). UI conversion (decimal ⇄ `HH:mm`) happens only at the Kendo column template/parse layer — never in the stored value or in server-side validation, which stays decimal-based.
- Decimal-to-minutes mapping is the standard proportional one (0.5 hr = 30 min).

### 2.7 Bulk Submit (source §5.6)
- Row checkboxes are selectable only on Draft rows.
- A "Submit" button above the grid is enabled once ≥1 row is checked.
- Submit posts selected IDs to a dedicated endpoint that, per record: verifies current-user ownership, re-runs full field validation (required fields, 0–24 range), and only then flips Draft → Submitted. Records that fail validation or ownership are not submitted.
- Grid refreshes after submit so newly-Submitted rows immediately render read-only.
- Bulk Submit is the only path from Draft to Submitted in Phase 1 — there is no inline/per-row Status dropdown that submits directly.

### 2.8 Validation (source §6)
| Field | Rule |
|---|---|
| Project | Required, must be a valid existing (active) project |
| Task | Required, non-empty |
| Time | Required, numeric, 0 ≤ Time ≤ 24 |
| Date | Required (defaults to today, editable) |
| Status transition | Edits/delete allowed only while Draft; Submit allowed only on Draft rows owned by the current user; all changes to a Submitted row are rejected server-side |

Validation is required on both tiers: client-side (Kendo grid rules, so autosave-on-blur cannot silently persist invalid data) and server-side (model validation in every insert/update/submit action) — server-side is authoritative and must not trust the client.

## 3. Explicitly Out of Scope (Phase 1)
- Un-submitting / reverting a Submitted record.
- Project management UI (create/edit/deactivate projects) — Phase 1 assumes Projects already exist.
- Any storefront-facing (non-admin) UI.
- Reporting/export/aggregation views over logged time.
- Any admin/manager-level "view all staff time logs" screen — not requested in source doc; flagged as an open question below since it is a common adjacent ask.

## 4. Store-Context Axes Addressed

| Axis | Finding |
|---|---|
| Storefront vs admin | Admin-only. No storefront/theme changes. |
| Entity/data impact | Introduces one new entity (`TimeLog`). Depends on a `Project` entity/table whose existence in this store is unconfirmed (Open Question Q2). Reads `Customer`/`CustomerRole` for the Staff role and current-user context; no changes to existing catalog/order/customer schema. |
| Multi-store (`StoreMapping`) | Not addressed in source doc. Open Question Q3. |
| Localization | Not addressed in source doc. Grid labels, validation messages, and menu text are user-facing strings that per CLAUDE.md must go through `ILocalizationService`/locale resources regardless of whether multi-language is enabled — this is a coding-standard default, not a scope question, so no clarification needed here. Whether staff actually see multiple languages at runtime is Open Question Q4. |
| Permissions | Single new permission `ManageTimeLog`, granted to Staff. No mention of a separate admin-oversight permission (e.g., a manager role viewing all logs) — see Open Question Q1. |
| Existing plugin/feature overlap | Not covered in source doc. Open Question Q5. |
| Third-party dependency | None — fully internal feature, no external API/credentials. |

## 5. Notes for the Technical Designer (signal only — not a placement decision)
- The autosave-on-blur behavior is a deliberate deviation from nopCommerce's default Kendo grid save pattern (explicit Update click) and requires custom JS; this is a UI/JS customization, not a core behavior change, but is called out because it diverges from the standard admin grid pattern developers may expect.
- The server-side re-check of `Status == Draft` on every write path (independent of the UI disabling controls) is a hard requirement, not an optional hardening step — it should be evaluated for where in the layering (service vs. controller) it is enforced consistently across insert/update/delete/submit.
- Whether this is a new self-contained plugin, an extension of an existing plugin, or something else is a call for the technical designer, per the Placement Decision framework — this document does not assume an answer.

## 6. Open Questions (need stakeholder/user answer before design can finalize)

1. **Admin/manager oversight** — Is there any need (now or clearly-planned-next-phase) for a non-Staff role (e.g., manager/admin) to view or export time logs across all staff, or is single-owner-only visibility final for Phase 1? Source doc only describes staff-owned rows.
2. **Project entity provenance** — Does a `Project` entity/table already exist in this store (from another plugin or core customization), or must this feature define and seed it? Source doc says "Phase 1 assumes Projects already exist (seeded or managed elsewhere)" but does not confirm whether that table currently exists in this codebase.
3. **Multi-store** — Does this store run multiple storefronts via `StoreMapping`? If so, should Projects and/or TimeLog entries be scoped per store, or are they global regardless of store count?
4. **Localization** — Does this store currently support multiple languages/currencies for admin users? (Affects whether locale resources need more than a single default-language set authored at build time — the coding standard of using resource keys applies either way.)
5. **Existing plugin overlap** — Is there an existing time-tracking, HR, or project-management plugin/customization already installed in this store that this feature should extend rather than duplicate?
6. **Time value rounding** — Some HH:mm values do not map to a terminating decimal (e.g., 20 minutes = 0.3333... hours). What rounding/precision rule should the decimal column and the HH:mm⇄decimal conversion use (e.g., round to nearest minute, store to 2 or 4 decimal places)? Not specified in source doc §4/§5.5.
7. **Delete confirmation** — Should deleting a Draft row require a confirmation prompt (standard nopCommerce admin grid pattern), or is a bare delete acceptable? Not specified in source doc.

None of the above have been assumed one way or the other in the stories/acceptance criteria that follow; where a story depends on one of these answers, it is flagged inline.
