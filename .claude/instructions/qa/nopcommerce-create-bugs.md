# Create Bugs for Test Failures (nopCommerce)

Create bug work items for failed test cases with proper linking, assignment, and nopCommerce placement tagging.

---

## Required Inputs
- Failed test case ID and actual-vs-expected result
- nopCommerce version the failure was observed on
- Whether the failure relates to plugin logic, a core override, or a genuine core change (inherit the tag from the source work item)

## Procedure
1. Create the bug, linked to the failed test case (Tests/Tested By).
2. Tag with `nop-plugin`/`nop-core` inherited from the source story.
3. If the bug is in a plugin's install/uninstall lifecycle, note this explicitly in the title/description - these get priority since they can leave the store in a broken state for other features too.
4. Assign per the team's standard triage process.
