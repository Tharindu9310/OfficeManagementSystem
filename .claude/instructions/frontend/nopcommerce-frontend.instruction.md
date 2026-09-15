# Frontend Extension - nopCommerce Theme/Razor Standards

## Purpose
Framework-specific standards for nopCommerce storefront/theme development. nopCommerce's storefront is server-rendered ASP.NET Core MVC/Razor - there is no separate SPA build unless the store has deliberately added one.

**Works with shared standards:**
- project-standards/security-standards.instruction.md (MANDATORY)
- project-standards/error-handling-standards.instruction.md
- project-standards/accessibility-standards.instruction.md

---

## 1. Technology Stack

ASP.NET Core Razor Views | nopCommerce theme engine | Existing theme's CSS/LESS/SCSS pipeline | Vanilla JS + theme's existing script libraries (no framework addition without explicit instruction)

## 2. Rendering Location
- New UI renders through a ViewComponent registered against a named widget zone (IWidgetPlugin.GetWidgetZones()) - never by editing core theme .cshtml files directly.
- Overrides of existing views use the plugin's own Views/ folder with nopCommerce's view-override mechanism, not in-place core edits.
- Confirm the target widget zone against the technical design before writing markup - don't invent placement.

## 3. Views & Models
- Views stay thin - no business logic in .cshtml; the view component/controller supplies a fully-prepared model.
- Models derive from BaseNopModel (or plain records for simple cases) scoped to the view - never bind a core domain entity directly.
- Use nopCommerce's existing partial views/display templates where one already covers the need (e.g. product box, price display) instead of rebuilding them.

## 4. Client Assets
- Match the active theme's existing CSS structure and naming convention - don't introduce a new methodology inconsistent with the theme.
- Add new JS/CSS through the theme's existing bundling configuration (check bundleconfig.json / the theme's asset pipeline) rather than loose unbundled files.
- No new frontend framework or major library without explicit instruction.

## 5. Admin UI (when the "frontend" task is admin-facing)
- Use nopCommerce's admin conventions: Kendo UI grids for lists, standard tab layout (nav-tabs-custom) for settings pages, existing form-group markup.
- Client validation follows the same pattern as existing admin forms (unobtrusive validation, not a custom validation library).

## 6. Localization
- Every user-facing string goes through ILocalizationService / locale resources (the T() helper in Razor) - never hardcoded text.
- New strings are added via the plugin's install-time locale resource registration, keyed consistently with existing naming (Plugins.{Group}.{Name}.{Key}).

## 7. Multi-Store & Responsive
- If the store is multi-store, verify UI reflects store-specific settings/content rather than assuming one storefront.
- Respect the active theme's existing responsive breakpoints - no fixed-width layouts.

## 8. Accessibility
Follow project-standards/accessibility-standards.instruction.md - same WCAG 2.1 AA bar as any other UI work, applied to Razor markup and the theme's existing component patterns.
