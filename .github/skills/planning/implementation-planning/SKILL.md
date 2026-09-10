---
name: nopcommerce-implementation-planning
description: Convert a nopCommerce technical design into sequenced, dependency-aware backend and frontend tasks split by placement
---

# nopCommerce Implementation Planning Skill

## Purpose
Defines workflow, responsibilities, and outputs for converting the SA design into actionable, placement-tagged backend and frontend tasks with clear dependencies.

---

## Workflow

### Step 1: Read All Input Documents
- Read design document (including the placement decision).
- Read user stories and acceptance criteria.
- Zero assumptions - work only from the provided documents.

### Step 2: Extract Tasks by Placement
Tag each task `new-plugin` or `modify-existing` per the design's placement decision. Never merge a plugin task and a core task into one work item.

### Step 3: Sequence by Dependency
Migrations/entities -> services + DI -> admin UI -> storefront UI -> install/uninstall wiring last.

### Step 4: Version & Compatibility Check
Confirm every task references the same target nopCommerce version; flag any task whose approach differs across versions.

### Step 5: Define Verification Hooks
For each task, state what QA can check as "done" - install/configure/uninstall for plugins, regression scope for core changes, specific event/widget-zone behavior for extension-point tasks.

## Outputs
`.github/doc/{feature-name}/plan/implementation-plan.md` - task list grouped by placement, build order, core-modification tasks flagged separately.

## Definition of Done
- Every task correctly tagged for handoff.
- Dependencies respect nopCommerce's entity -> service -> UI -> install-wiring chain.
- Core-modification tasks flagged with upgrade-risk justification carried forward.
