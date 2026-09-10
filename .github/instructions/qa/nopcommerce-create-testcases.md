# Create Test Cases from Requirements (nopCommerce)

Create comprehensive test cases for a work item and link them in Azure DevOps, covering both standard functional behavior and nopCommerce-specific plugin/core concerns.

> Note: The MCP server automatically normalizes HTML/XML formats. Use sensible HTML/XML structure.

---

## Procedure

### Step 1: Fetch Work Item Details
Retrieve the linked story/task via `devops-get-work-item`, including its `nop-plugin`/`nop-core` tag and target nopCommerce version.

### Step 2: Derive Test Cases by Category
For every acceptance criterion, write a functional test case (Given-When-Then). In addition, based on the work item's placement tag:

- **nop-plugin**: add install / configure / uninstall / re-install test cases.
- **nop-core**: add regression test cases for the surrounding core flow, not just the new behavior.
- **Any widget-zone UI**: add a rendering test case on the actual storefront page the zone appears on.
- **Any event consumer**: add a test case confirming it fires only on the intended domain event.

### Step 3: Create and Link
Create each test case as a linked Azure DevOps work item under the source story, using `create-testcases`-style linking (Tests/Tested By relationship).

### Step 4: Validate
Immediately follow with `nopcommerce-validate-test-case.md` - do not proceed until each created test case is confirmed correctly linked and populated.
