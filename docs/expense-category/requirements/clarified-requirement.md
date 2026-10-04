# Clarified Requirement - Expense Category

Source Jira Issue: SCRUM-38
Parent Epic: none (the issue has no parent Epic and no sub-tasks in Jira)
Jira issue type: Story | Status: In Progress | Priority: Medium | Reporter: TharinduPerera
Feature slug: `expense-category`
Target nopCommerce version: 4.90.8 (.NET 8) - confirmed by the user. The Jira issue itself does not state a version.
Plugin: `Nop.Plugin.Misc.Expenses` (new plugin, SystemName `Misc.Expenses`, per the technical design).

## Phase Detection
This is NOT phase 2+ of `time-log`. Nothing in the Jira issue says "Phase N", "builds on", or references `docs/time-log/`. It introduces a new concept (Expenses and Expense Category) that does not appear in the time-log Phase 1 or Phase 2 docs. It is a new feature with its own folder `docs/expense-category/`. No prior-phase impact.

## Original Requirement (verbatim from SCRUM-38, "Create/Edit Category")
> Create new menu item called Expenses and inside that sub menu called expense Category.
> should have new permission to access expense menu.
> category grid should have name, per month limit, expense or income

## Interpretation
"Category" means an Expense Category, a new concept inside the new Expenses area. It is not the nopCommerce catalog category and not something inside the time-log plugin.

## Summary
An admin-area feature with no storefront component. It adds an "Expenses" top-level menu item with an "Expense Category" sub menu item, gated by one new permission. The sub menu opens one page with a name/store search, an "Add new" panel above the grid, and a paged grid of expense categories (Name, Per Month Limit, Expense or Income, Stores). Name and Per Month Limit are edited inline in the grid. Stores and per-language names are edited in a per-row modal on the same page. There is no delete.

## Decisions
All questions are answered. Nothing below is assumed.

- **D1 (Q2, relaxed and approved by the user) Editing model.**
  - Create: an "Add new" panel above the grid taking Name (default language), Per Month Limit and Type.
  - Inline edit: Name (default language) and Per Month Limit are edited inline in the grid. Type is read-only on edit.
  - Modal: stores and per-language names are edited in a per-row "Edit stores and translations" modal on the same page.
  - There are no separate create or edit pages and no navigation away from the grid page.
  - The grid is DataTables (the framework's admin grid), not Kendo.
- **D2 (Q3) Per Month Limit**: an optional decimal, must be >= 0 when provided, informational only (not enforced anywhere), in the store's primary currency.
- **D3 (Q5) Expense or Income**: required, a single choice, locked after creation.
- **D4 (Q7) Permission**: one permission (example name `ManageExpenseCategories`) covers menu, view and edit. Granted to the Administrators role by default.
- **D5 (Q1) Search and delete**: search/filter by Name is added. There is no delete. Paging stays.
- **D6 (Q4) Limit applicability**: Per Month Limit applies to both Expense and Income categories.
- **D7 (Q6) Name**: required, maximum 200 characters, duplicates are allowed (no uniqueness rule).
- **D8 (Q8) Multi-store**: categories are per store via store mapping. A new category defaults to all stores. Stores are changed afterwards in the modal.
- **D9 (Q9) Localization**: the Name is localizable per language. The inline-edited Name is the default-language value. A translation left empty falls back to the default Name.
- **D10 (Q10) Placement**: a new plugin `Nop.Plugin.Misc.Expenses`.
- **D11 Name search scope**: the name search matches the default-language Name only (not translations).
- **D12 Grid scope**: the grid lists all categories regardless of store, with an optional Store filter (default "All stores").
- **D13 Name search behavior**: trimmed, case-insensitive "contains" on the default-language Name.

## In-Scope Behavior (traced to the Jira text and decisions)
1. **Menu**: new admin menu item "Expenses" containing one sub menu "Expense Category". (Jira line 1)
2. **Permission**: the single permission in D4 gates the menu, grid data, and every create, edit, search and modal endpoint, enforced server-side. (Jira line 2, D4)
3. **Grid**: columns Name, Per Month Limit, Expense-or-Income, plus a Stores indicator, paged with at most 100 rows per page. (Jira line 3, D5, D8)
4. **Search/filter**: by Name and by Store. (D5, D11, D12, D13)
5. **Create** in the Add panel and **inline edit** of Name and Per Month Limit. (Issue title, D1)
6. **Stores and translations modal** per row. (D1, D8, D9)
7. **Field rules**: Per Month Limit per D2 and D6; Type per D3; Name per D7.
8. **Uninstall cleanup**: uninstalling the plugin removes the menu, the permission, the plugin's locale resources, and the plugin's StoreMapping and LocalizedProperty rows (these are core tables that the table drop does not clear).

## nopCommerce Axes Addressed
- Storefront vs admin: admin only. The issue says "menu", which in nopCommerce is the admin menu.
- Entity/data impact: a new concept. It does not touch catalog, customer or order data. The plugin does add its own rows to the core StoreMapping and LocalizedProperty tables.
- Multi-store: per store (D8).
- Localization: Name localizable per language (D9); menu labels and UI text use locale resources.
- Currency: the store's primary currency (D2). No per-category currency.
- Permissions: resolved by D4.
- Existing plugin overlap: none reused; a new plugin (D10).
- Third-party dependency: none.
- Signal for the designer: only public extension points are needed. Nothing suggests a core behavior change.

## Out of Scope (not stated in the issue)
Delete, an enabled/active flag, recording expense or income transactions, reports, enforcing the monthly limit, a storefront view, import/export, setting stores or translations at creation time in the Add panel, searching translated names, translations of the admin UI strings into other languages.
