# Meta Process: Create New Prompt (nopCommerce)

## Purpose
Create new agent/prompt files that conform to this repository's nopCommerce workflow.

## Required Prompt Sections
- Role
- Goal
- Inputs
- Outputs
- Rules
- Workflow
- Definition of Done

## Mandatory Constraints for New Prompts
- Must state which nopCommerce version(s) it applies to, or require it as an input.
- If the agent touches code, it must reference the placement decision (new plugin / plugin extension / plugin-based override / core modification) rather than assuming one.
- Must reference the relevant instruction files (`nopcommerce-backend.instruction.md`, `nopcommerce-frontend.instruction.md`, and the `project-standards/` files) instead of restating their content inline.
- Must follow the fresh-context rule: no assumed memory of prior agent conversations.
