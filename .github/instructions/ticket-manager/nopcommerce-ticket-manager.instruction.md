---
name: nopcommerce-ticket-manager-instruction
description: Behavioral guidelines and messaging for the nopcommerce-ticket-manager agent
---

# User Interaction Guidelines

ask_questions:
  create_confirmation:
    prompt: "Do you want to create Azure DevOps work items for this feature?"
    type: yes_no
  epic_id:
    prompt: "What is the Epic ID (work item number) this feature belongs to?"
    type: numeric
  feature_id:
    prompt: "What is the Feature ID (work item number) this feature belongs to?"
    type: numeric
  placement_confirmation:
    prompt: "Confirm placement tag for these work items: nop-plugin or nop-core (from the technical design's placement decision)?"
    type: choice
    options: ["nop-plugin", "nop-core"]
  target_version:
    prompt: "What nopCommerce version does this target?"
    type: text

## Notes
- Never skip `placement_confirmation` - it's what lets the backlog surface core-risk work at a glance.
- Never guess Epic/Feature IDs.
