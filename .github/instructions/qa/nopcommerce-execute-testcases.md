# Execute Test Cases (nopCommerce)

Execute linked test cases against a nopCommerce QA environment and log results.

---

## Required Inputs
- Linked test case IDs
- Target nopCommerce version and environment (confirm the QA store is on the same version the feature was built against)
- Plugin system name (if testing a plugin) so lifecycle test cases can be run against the actual Admin > Plugins screen

## Procedure
1. Confirm the QA environment matches the target nopCommerce version before executing anything - a version mismatch invalidates plugin-lifecycle results.
2. Execute functional test cases in order; log actual result against expected.
3. Execute plugin-lifecycle test cases (install/configure/uninstall) if the work item is tagged `nop-plugin`.
4. Execute regression test cases against the surrounding core flow if tagged `nop-core`.
5. Log pass/fail per test case with enough detail (screenshots, log excerpts from the `Log` table if relevant) to support bug filing on failure.
