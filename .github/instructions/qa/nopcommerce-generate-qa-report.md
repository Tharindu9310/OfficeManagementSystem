# Generate QA Summary Report (nopCommerce)

Generate a comprehensive QA summary report with test results, bugs, and a release recommendation for a nopCommerce feature.

---

## Required Inputs
- All executed test case results
- All filed bugs (with `nop-plugin`/`nop-core` tags)
- Target nopCommerce version

## Report Sections
1. **Summary** - pass/fail counts, overall status.
2. **Plugin lifecycle results** (if applicable) - install/configure/uninstall outcome.
3. **Core-safety regression results** (if applicable) - surrounding-flow impact.
4. **Open defects** - grouped by `nop-plugin` vs. `nop-core`, since core defects typically block release harder.
5. **Upgrade-risk carryforward** - restate any Core Modification Notice from the implementation, so the release decision-maker sees it without digging through code.
6. **Release recommendation** - Go / No-Go / Go-with-caveats, with reasoning.
