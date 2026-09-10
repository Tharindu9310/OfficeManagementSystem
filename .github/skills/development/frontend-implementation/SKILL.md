---
name: nopcommerce-frontend-implementation
description: Implement nopCommerce storefront/theme changes - Razor views, widget components, client assets - using existing theme conventions
---

# nopCommerce Frontend Implementation

## When to Use
- After backend services/APIs are ready
- When implementing the storefront or admin-UI scope of a user story
- When creating/modifying Razor views, view components, or theme assets
- When implementing UI per the approved design's widget zone/view mapping

## Inputs
- Required: Design document (with named widget zone/view component), implementation plan, user stories, acceptance criteria

## Procedure
1. Confirm the target widget zone or admin section from the design - don't invent placement.
2. Build the `ViewComponent` (storefront) or admin controller/view (admin), following nopCommerce's existing component/grid conventions.
3. Keep views thin; models are purpose-built, never a core entity bound directly.
4. Add client assets through the active theme's existing bundling setup - no new framework without explicit instruction.
5. Localize every string via `ILocalizationService`/locale resources at install time.
6. Check accessibility (keyboard nav, alt text, contrast) and multi-store behavior before marking done.

## Outputs
- New/modified view component(s), views, models, client assets scoped to the plugin or theme override (never core theme files).
- Localization resource additions.

## Definition of Done
- Renders through the widget zone/view component named in the design.
- No hardcoded strings.
- Existing theme conventions followed.
- Accessibility and multi-store considerations addressed.
