# Agents Overview (nopCommerce Edition - Streamlined)

This directory defines the minimal, load-bearing set of subagents for an existing nopCommerce store's development workflow. Trimmed from an earlier 10-agent version - see "What Got Cut" below for what was removed and why.

## Workflow Order
1. `nopcommerce-requirement-analyzer` (BA) - clarifies raw asks into testable stories/acceptance criteria
2. `nopcommerce-technical-designer` (SA) - decides plugin vs. core placement, designs entities/services/UI
3. `nopcommerce-ticket-manager` - creates `docs/{feature-name}/tickets/tickets.md`, optionally synced to Azure DevOps
4. `nopcommerce-implementation-planner` - splits the design into sequenced, dependency-aware tasks, extends `tickets.md`
5. `nopcommerce-developer` - implements backend AND UI (Razor views, widget components, theme assets) ticket-by-ticket
6. `nopcommerce-qa-tester` - tests `Done` tickets, logs results, files bugs directly into `tickets.md`, drives the bug-fix loop

## Shared Ticket File
`docs/{feature-name}/tickets/tickets.md` is the pipeline's primary, always-available work-item log. `nopcommerce-ticket-manager` creates it; every agent from the implementation planner onward reads and updates it directly.

## What Got Cut (and why it's safe)
- **`nopcommerce-frontend-developer`** - merged into `nopcommerce-developer`. Its scaffolding (admin controller+views, storefront `ViewComponent`) already substantially overlapped with the developer agent's Path B; the unique parts (theme CSS conventions, client asset bundling, accessibility specifics) were folded in as a dedicated UI section rather than lost.
- **`nopcommerce-documentation-writer`** - removed. Its core output (`docs/{feature-name}/...`) already gets written as a side effect of the BA/SA/planner agents; Confluence sync was its only unique value and is optional anyway.
- **`nopcommerce-handoff-validator`** - removed as a dedicated subagent. It was already invoked only when the user opted in, so it wasn't load-bearing. If you want a quality-gate check between stages, ask Claude directly at that point rather than delegating to a separate agent.
- **`nopcommerce-ui-ux-designer`** - removed. It was already documented as "optional, invoked selectively," and its job (widget zone placement, admin layout decisions) is already covered by the technical designer's UI design step.

If any of these turn out to be missed in practice, they're easy to reintroduce - the pattern for building one (frontmatter + Role/Goal/Inputs/Rules/Workflow/Definition of Done) is established across the remaining 6.
