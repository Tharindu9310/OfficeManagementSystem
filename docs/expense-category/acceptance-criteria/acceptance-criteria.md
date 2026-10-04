# Acceptance Criteria - Expense Category

Source Jira Issue: SCRUM-38 | nopCommerce 4.90.8 | Admin area | Plugin `Nop.Plugin.Misc.Expenses`. All criteria are final. Decisions D1-D13 are in `../requirements/clarified-requirement.md`.

## Story 1 - Menu
AC1.1 Given a user holding the Expenses permission, when the admin menu renders, then a top-level "Expenses" item is shown containing exactly one sub item "Expense Category".
AC1.2 Given the user clicks "Expense Category", when the page loads, then the Expense Category page is displayed with the search panel, the Add new panel and the grid.
AC1.3 Given menu text, when the admin language changes, then the labels come from locale resources, not hardcoded strings.

## Story 2 - Permission and uninstall (D4)
AC2.1 Given a fresh install, when an admin views the role permission screen, then one new permission (example name `ManageExpenseCategories`) is listed and is granted to the Administrators role by default.
AC2.2 Given a user without the permission, when the admin menu renders, then "Expenses" is not shown.
AC2.3 Given a user without the permission, when they request the Expense Category page or any of its endpoints (grid read, add, update, stores and translations modal load and save) directly, then access is denied server-side.
AC2.4 Given a user with the permission, when they use the menu, search, view the grid, add, edit inline, and use the stores and translations modal, then no second permission is required.
AC2.5 Given the plugin is uninstalled, when uninstall completes, then the menu entry, the permission and the plugin's locale resources are removed.
AC2.6 Given categories with store mappings and translated names exist, when the plugin is uninstalled, then all StoreMapping rows for the ExpenseCategory entity and all LocalizedProperty rows for the ExpenseCategory entity are deleted, and no such rows remain in the database.

## Story 3 - Grid, search and filter
AC3.1 Given categories exist, when the grid loads, then it shows the columns Name, Per Month Limit, Expense/Income and Stores for each category.
AC3.2 Given more rows than one page, when the grid loads, then the data is paged with at most 100 rows per page, even if a larger page size is requested directly.
AC3.3 Given no categories exist, when the grid loads, then an empty grid is shown without error.
AC3.4 Given a category with no Per Month Limit, when the grid renders, then that cell is shown empty (not as 0).
AC3.5 Given a Per Month Limit, when the grid renders, then it is shown in the store's primary currency format.
AC3.6 Given categories with different names, when the user enters text in the Name search and searches, then only categories whose default-language Name contains the text (trimmed, case-insensitive) are listed; an empty search lists all. Translated names are not searched. The grid has no delete action, and a direct delete request is not available.
AC3.7 Given the Store filter, when it is "All stores" (default), then every category is listed regardless of store mapping.
AC3.8 Given the Store filter set to a specific store, when the user searches, then only categories available in that store are listed (categories not limited to stores, plus categories mapped to that store). This holds regardless of the catalog setting "Ignore store limitations" (BUG-010).
AC3.9 Given a Name search and a Store filter are both set, when the user searches, then only categories matching both are listed.
AC3.10 Given the Stores column, when a category is not limited to stores, then it shows "All stores"; when limited, it shows that it is limited to selected stores.

## Story 4 - Add and inline edit (D1, D2, D3, D7)
AC4.1 Given a user with the permission, when they enter a valid Name, optional limit and a type in the Add new panel above the grid and save, then the category is persisted, appears in the grid with the entered values, and the page is not navigated away.
AC4.2 Given an existing category row, when the user edits Name and/or Per Month Limit inline and confirms, then the grid shows the updated values.
AC4.3 Given the Add new panel, when the user chooses the type, then the only choices are Expense and Income, and one must be chosen; saving without a type fails with a localized validation message.
AC4.4 Given an existing category, when the grid row enters inline edit, then the type is displayed read-only and cannot be changed.
AC4.5 Given an update request for an existing category sent directly to the endpoint with a different type, when it is processed, then it is rejected with a localized message and nothing is saved; an update that posts the same type succeeds.
AC4.6 Given Per Month Limit left empty, when the user saves, then the save succeeds.
AC4.7 Given a Per Month Limit of 0 or any positive decimal, when the user saves, then the save succeeds and the value is stored as entered.
AC4.8 Given a Per Month Limit that is negative or non-numeric, when the user saves, then validation fails with a localized message and nothing is saved.
AC4.9 Given an invalid add or update request sent directly to the endpoint, when it is processed, then server-side validation rejects it the same way as the UI would.
AC4.10 Given any limit value, when categories are saved or used, then the limit is informational only and no feature blocks or warns based on it.
AC4.11 Given the Name, when it is blank the save fails with a localized message; when it is longer than 200 characters the save fails with a localized message; when it duplicates an existing category's Name the save succeeds (no uniqueness rule). Nothing is saved on failure.
AC4.12 Given a category of type Expense or of type Income, when a Per Month Limit is entered, then the same rules apply to both types (optional, 0 or greater).
AC4.13 Given a new category is created, when it is saved, then it is available in all stores (not limited to stores) until stores are changed in the modal.
AC4.14 Given a category, when a store or language change is made, then it is made only in the Edit stores and translations modal (see Story 5); the Add panel does not set stores or translations.

## Story 5 - Stores and translations modal (D1, D8, D9)
AC5.1 Given a category row, when the user activates its "Edit stores and translations" button, then a modal dialog opens on the same page (no navigation) showing a store multi-select, a read-only reference of the default Name, and one Name field per language.
AC5.2 Given the modal is open, when the user uses the keyboard only, then every control can be reached and used, focus stays inside the dialog, Escape closes it, and focus returns to the button that opened it. Fields have labels and the dialog has an accessible title.
AC5.3 Given the user selects one or more stores in the modal and saves, when the save succeeds, then the category is limited to those stores, the modal closes, and the grid reloads and shows the updated Stores indicator.
AC5.4 Given a category limited to specific stores, when the Store filter is set to a store that is not selected, then the category is not listed; when set to a selected store, it is listed. This applies whether or not the catalog setting "Ignore store limitations" is on (BUG-010).
AC5.5 Given the user clears all stores in the modal and saves, then the category is available in all stores again.
AC5.6 Given the user enters a translated Name for a language and saves, then the translation is stored for that language, and wherever a name is shown for that language it displays the translation.
AC5.7 Given a language whose translation is left empty, when the Name is shown for that language, then the default Name is used.
AC5.8 Given a translation longer than 200 characters, when the user saves the modal, then it is rejected with a localized message and nothing from that save is stored; this is also enforced when the request is sent directly to the endpoint.
AC5.9 Given the modal save, when it is processed, then it never changes the stored default-language Name, the Per Month Limit or the type.
AC5.10 Given a user without the permission, when they load or save the modal endpoints directly, then access is denied server-side (see AC2.3).
