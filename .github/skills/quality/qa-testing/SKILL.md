---
name: nopcommerce-qa-testing
description: Execute comprehensive testing - functional, plugin lifecycle, core-safety - and manage bug-fix cycles for a nopCommerce feature until acceptance criteria are met
---

# nopCommerce QA Testing

## When to Use
- After development is complete
- When validating implementation against acceptance criteria
- Before deployment to production
- When managing bug-fix iterations

## Inputs
- Required: User stories, acceptance criteria, implementation scope, target nopCommerce version, placement tag (`nop-plugin`/`nop-core`)

## Test Categories
1. **Functional** - every acceptance criterion, storefront and admin as applicable.
2. **Plugin lifecycle** (`nop-plugin`) - install, configure, uninstall (settings/permissions/locale resources actually removed), re-install.
3. **Core-safety regression** (`nop-core`) - surrounding flow unaffected; Core Modification Notice verified accurate.
4. **Event/extension-point correctness** - consumer fires only on intended events; DI override actually resolved.
5. **Integration** - payment/shipping/tax changes tested through the real checkout flow; widget zones tested on the actual target page.
6. **Security/accessibility/performance** - ACL checks block unauthorized access; WCAG 2.1 AA on new UI; pagination on new list endpoints.

## Bug-Fix Loop
File bugs tagged by category (functional/plugin-lifecycle/core-safety), loop until all acceptance criteria pass and no unresolved critical/high defects remain. Re-run plugin lifecycle tests after any fix touching settings/permissions/migrations.

## Outputs
Test results, filed bugs, and a QA summary report with release recommendation.

## Definition of Done
- All acceptance criteria verified.
- Plugin lifecycle verified for new-plugin work.
- Core Modification Notices verified accurate.
- No unresolved critical/high defects.
