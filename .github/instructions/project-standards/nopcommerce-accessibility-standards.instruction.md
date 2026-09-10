# Accessibility Standards (WCAG 2.1 AA) - nopCommerce

## Purpose
All nopCommerce storefront implementations MUST comply with WCAG 2.1 Level AA standards. Admin area should follow the same bar where practical, since store staff may also rely on assistive technology.

**Referenced by:** frontend instruction file, nopcommerce-frontend-developer agent, nopcommerce-ui-ux-designer agent

---

## 1. Overview

### WCAG 2.1 Level AA Compliance (MANDATORY for storefront)
- **Perceivable** - product images have meaningful `alt` text (product name at minimum, not "image1.jpg"); content isn't conveyed by color alone (e.g., stock status shown with text/icon, not just a colored dot).
- **Operable** - all interactive elements (add-to-cart, filters, widget controls) are keyboard-navigable and have visible focus states; no interaction is mouse-only (hover-only menus need a keyboard-accessible equivalent).
- **Understandable** - form errors (checkout, account, admin forms) are announced in a way assistive tech can pick up, not just a color change on the field.

## 2. nopCommerce-Specific Application
- **Widget zones**: new widget content must not break the surrounding page's heading hierarchy (don't drop an `h1` into a sidebar widget) or tab order.
- **Product grids/lists**: maintain the existing theme's accessible markup pattern (proper `img alt`, `label`/`for` associations on filters) rather than introducing new markup that skips it.
- **Admin grids** (Kendo UI): use the grid's built-in accessible column headers/sorting rather than custom unlabeled controls.
- **Checkout flow**: any new step/field added to checkout must be reachable and operable via keyboard alone, consistent with the existing checkout's accessibility behavior.

## 3. Contrast & Visual
- Minimum 4.5:1 text contrast ratio, matching the active theme's existing palette - don't introduce a new color that fails this for a specific new element.
- Don't rely on placeholder text as the only label for a form field.

## 4. Testing Expectation
`nopcommerce-qa-tester` checks new storefront UI against these points as part of its standard test pass - accessibility is not a separate, optional review step.
