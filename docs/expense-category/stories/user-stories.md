# User Stories - Expense Category

Source Jira Issue: SCRUM-38 | Target nopCommerce version: 4.90.8 | Area: Admin only | Plugin: new `Nop.Plugin.Misc.Expenses`. Traceability: requirement statements and decisions D1-D13 are in `../requirements/clarified-requirement.md`; design is in `../design/technical-design.md`.

## Story 1 - Expenses menu with Expense Category sub menu
As a store admin user with access to Expenses
I want a new "Expenses" menu item in the admin menu with an "Expense Category" sub menu
So that I can reach expense category management from the admin navigation
Requirement trace: Jira line 1.

## Story 2 - Permission gating and clean uninstall
As a store administrator
I want one new permission (example name `ManageExpenseCategories`) that controls the Expenses menu, viewing and editing, granted to the Administrators role by default, and I want uninstalling the plugin to leave no leftover data rows behind
So that only authorized users can use expense features and the store stays clean if the plugin is removed
Requirement trace: Jira line 2, D4, uninstall cleanup.

## Story 3 - Expense Category grid with search
As a user with the Expenses permission
I want a paged grid of expense categories showing Name, Per Month Limit, Expense or Income and a Stores indicator, with a search by Name and an optional Store filter
So that I can find any category and see its limit at a glance
Requirement trace: Jira line 3, D5, D8, D11, D12, D13. There is no delete.

## Story 4 - Add a category and edit it inline
As a user with the Expenses permission
I want to add a new category in an "Add new" panel above the grid (Name, optional Per Month Limit, Expense or Income) and edit the Name and Per Month Limit of an existing category inline in the grid, with the type fixed after creation
So that the category list reflects how we classify our expenses and income without leaving the grid page
Requirement trace: issue title "Create/Edit Category", Jira line 3, D1, D2, D3, D6, D7.

## Story 5 - Per-store availability and per-language names
As a user with the Expenses permission
I want to open an "Edit stores and translations" modal on a category row to choose which stores the category is available in and to enter a translated Name for each language
So that each store and language shows the right categories and names
Requirement trace: D1, D8, D9. A new category defaults to all stores and has no translations.
