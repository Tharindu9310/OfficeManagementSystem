---
name: nopcommerce-technical-design
description: Create a complete nopCommerce implementation design - placement decision, entity/service/event mapping - from approved requirements
---

# nopCommerce Technical Design Skill

## When to Use
- After requirement analysis is complete
- When user stories and acceptance criteria are defined
- Before implementation planning begins
- When you need to decide plugin vs. core placement and map extension points

## Inputs
- Required: Clarified requirement, user stories, acceptance criteria, target nopCommerce version
- Optional: Existing plugin list / store customizations already in place

## Procedure

### Step 1: Placement Decision (mandatory, first)
Classify as: new plugin / extension of existing plugin / plugin-based core override / genuine core modification. This is the single most consequential decision in the design - everything downstream depends on it.

### Step 2: Entity & Data Design
New/changed entities, migrations, settings classes, store-mapping/ACL needs.

### Step 3: Service & Extension Point Design
Service interfaces, DI registration approach, specific domain events to consume, specific widget zones to render into, specific provider interfaces (`IPaymentMethod`, `IShippingRateComputationMethod`, `ITaxProvider`) to implement.

### Step 4: Admin & Storefront UI Design
Admin menu placement, configuration page needs, permission records, storefront widget zone/view component mapping, localization string inventory.

### Step 5: Architecture Diagram
Component-level diagram showing Controller/ViewComponent -> Service -> Repository/Migration, with the plugin boundary marked relative to core.

## Outputs
`docs/{feature-name}/design/technical-design.md` with the placement decision surfaced at the top.

## Definition of Done
- Placement decision made and justified.
- Extension points named specifically, not generically.
- Migrations, settings, localization strings identified.
- Full traceability to requirement/stories/acceptance criteria.
