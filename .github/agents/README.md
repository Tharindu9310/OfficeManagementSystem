# Agents Overview (nopCommerce Edition)

This directory defines specialized agents for an existing nopCommerce store's development workflow.

## Workflow Order
1. `nopcommerce-requirement-analyzer` (BA)
2. `nopcommerce-technical-designer` (SA — decides plugin vs. core placement)
3. `nopcommerce-documentation-writer` (docs + Confluence, including plugin inventory)
4. `nopcommerce-ticket-manager` (Azure DevOps work items, tagged `nop-plugin`/`nop-core`)
5. `nopcommerce-implementation-planner` (task split by placement + dependency mapping)
6. `nopcommerce-developer` and `nopcommerce-frontend-developer` (parallel — backend/plugin logic vs. storefront/theme UI)
7. `nopcommerce-qa-tester` (functional + plugin-lifecycle + core-safety + bug-fix loop)

`nopcommerce-ui-ux-designer` is optional and invoked selectively between steps 2 and 6, only when a story needs UX detail beyond nopCommerce's existing patterns.

`nopcommerce-handoff-validator` runs between every stage above, on user confirmation.

## Global Constraints
- Never skip any phase.
- Never jump directly to development.
- Never assume missing requirements.
- Never introduce new frameworks/libraries/patterns unless explicitly instructed.
- Keep full traceability: Requirement → Story → Design (placement decision) → Plan → Code → Test → Documentation.

## nopCommerce-Specific Principle (applies across all agents)
Every requirement gets classified early as one of: **new plugin**, **extension of existing plugin**, **plugin-based override of core behavior**, or **genuine core modification**. This placement decision, made by `nopcommerce-technical-designer`, determines which rules every downstream agent applies — it is the single most important decision in the pipeline, since nopCommerce core gets overwritten on every upstream upgrade and plugin-based work does not.

## Required Agent Sections
Every agent includes:
- Role
- Goal
- Inputs
- Outputs
- Rules
- Workflow
- Definition of Done

## Target Version
All agents require an explicit target nopCommerce version as input (current stable: 4.90.x on .NET 8; `develop` branch: 5.00 on .NET 9). Plugin interfaces and conventions differ across major versions — agents ask rather than assume.
