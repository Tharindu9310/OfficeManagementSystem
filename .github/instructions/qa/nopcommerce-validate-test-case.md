# Validate Test Case (nopCommerce)

Verify test cases are correctly created with all required fields, including nopCommerce-specific tags, after creation.

> MANDATORY: Run this validation after EVERY test case creation. Do NOT proceed if validation fails.

---

## Validation Checklist
- [ ] Test case linked to the correct parent story/task
- [ ] Steps reflect Given-When-Then from the acceptance criteria
- [ ] Target nopCommerce version noted
- [ ] Placement-specific test cases present when applicable (plugin lifecycle for `nop-plugin`, regression for `nop-core`)
- [ ] No missing required Azure DevOps fields (area path, iteration, priority)
