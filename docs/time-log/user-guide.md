# Time Log — User Guide

A simple guide to logging and reviewing time entries in the admin area.

## For Staff

### Accessing your time log
Go to **Time Log > Log Time** in the admin menu (visible only if you've been granted the `ManageTimeLog` permission).

### Adding a new entry
1. Open the **Add Time Entry** panel and fill in **Project** (choose from the active projects list), **Task**, **Description**, **Date** (defaults to today), and **Time** (entered as `HH:mm`, e.g. `06:30` for 6.5 hours).
2. Click the **Add Time Entry** button to save it — the new entry is created as **Draft**.

### Editing an entry
Click the pencil (Edit) icon on a **Draft** row to put it into edit mode — the Date becomes a date picker and the Project becomes a dropdown. Make your changes, then click the checkmark (**Update**) icon to save, or the cancel icon to discard your changes. Nothing is saved until you click Update. If a value is invalid (e.g. Time outside 0–24, empty Task, no Project selected), the save is rejected and you'll see an error message under the affected field.

### Deleting an entry
You can delete a **Draft** row at any time. You'll be asked to confirm before it's removed permanently.

### Filtering your entries
Use the grid's filter controls to narrow the list by **date range**, **status**, or **project**. Filters only ever apply to your own entries — you can't see anyone else's time logs here.

### Submitting entries
1. Check the box next to one or more **Draft** rows you're ready to lock in.
2. Click **Submit**.
3. Each selected row is re-validated. Rows that pass become **Submitted** and turn read-only. Rows that fail validation stay **Draft**, and you'll see the reason for each failure.

### Important: Submitted entries are locked
Once an entry is **Submitted**, it can no longer be edited or deleted — by you or anyone else — in Phase 1. There is no "un-submit" action. Double-check an entry before submitting it.

---

## For Managers

### Accessing the oversight view
Go to **Time Log > Time Logs (All Staff)** in the admin menu (visible only with the `ManageTimeLogAll` permission).

### What you can do
This view is **read-only** and shows only **Submitted** entries across the whole team — Draft entries staff haven't submitted yet never appear here. You can filter by date range, project, and staff member. There is no way to edit, delete, or submit entries from this screen; it's for reviewing completed work only.

---

## Notes
- **Projects** are a fixed list managed outside this plugin in Phase 1 (three sample projects — General, Internal, Client Support — are seeded on install). If you need a new project added, contact your administrator.
- Time is always entered and displayed as `HH:mm` (hours:minutes), even though it's stored internally as a decimal number of hours.
