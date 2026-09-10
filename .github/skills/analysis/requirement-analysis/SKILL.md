---
name: nopcommerce-requirement-analysis
description: Clarify raw requirements and create implementation-ready user stories with acceptance criteria for a nopCommerce store
---

# nopCommerce Requirement Analysis

## When to Use
- When receiving a raw requirement from stakeholders for a nopCommerce store
- When a feature request needs clarification and structuring
- Before technical design begins
- As the first step in the SDLC workflow

## Inputs
- Required: Raw requirement text (user story or feature request)
- Required: Target nopCommerce version

## Procedure

### Step 1: Read the Raw Requirement
Extract the core capability requested. Don't infer implementation approach yet.

### Step 2: Walk the nopCommerce Clarification Checklist
Only ask what's relevant to this specific requirement:
- Storefront, admin, or both?
- Does it touch existing catalog/customer/order data, or is it new?
- Multi-store applicability?
- Localization needs?
- Who should access/configure it (roles, permissions)?
- Does an existing plugin already do something adjacent?
- Any third-party/marketplace dependency (payment, shipping, marketing)?

### Step 3: Draft Stories and Acceptance Criteria
Use the shared story format (As a / I want / So that, Given-When-Then). Keep them implementation-agnostic - no plugin-vs-core language here, that's the SA's decision.

### Step 4: Flag Core-Change Signals
If the requirement sounds like it needs a change to base checkout, cart, or catalog behavior with no obvious extension point, flag this explicitly as a note for the technical designer to evaluate - don't decide it here.

## Outputs
- `clarified-requirement.md`
- `user-stories.md`
- `acceptance-criteria.md`

## Definition of Done
- No open ambiguity left for the SA to guess on.
- Target nopCommerce version confirmed.
- Relevant checklist axes addressed.
