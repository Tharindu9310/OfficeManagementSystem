# Link Work Items in Azure DevOps (nopCommerce)

Establish relationships between work items (Test Cases, Bugs, PBIs) for a nopCommerce feature.

---

## Required Inputs
- Source and target work item IDs
- Relationship type (Tests/Tested By, Parent/Child, Related)

## nopCommerce-Specific Note
When linking a Bug to its source Test Case/Story, carry forward the `nop-plugin`/`nop-core` tag onto the Bug so triage can immediately see whether the fix touches core (higher review scrutiny) or plugin code.
