---
description: Run the streamlined nopCommerce BA -> SA -> Tickets -> Plan -> Dev -> QA pipeline with gates
---

You are driving the nopCommerce feature-delivery pipeline end to end, using the streamlined 6-agent roster. Since subagents here don't auto-chain, you carry the sequencing yourself.

## Step 0 - Input Validation (mandatory, first)

There are three supported ways to provide input - check in this order:

**A. Inline text** - the user's request to this command contains:
```
requirement={text} nopcommerce-version={e.g. 4.90.8}
```

**B. Requirement intake file** - if invoked with no inline text (e.g. just `/nopcommerce-workflow` on its own), check `docs/intake/requirement.md` first. Expected format:
```markdown
# Requirement

<free-text description of what's needed>

## nopCommerce Version
4.90.8
```
If found and complete, this is a new-feature-or-phase run - proceed to the **Canonical Sequence** below.

**C. Bug or change intake file** - if `requirement.md` isn't present/complete, check `docs/intake/bug.md` and `docs/intake/change.md` (in that order - if both are present and unprocessed, handle `bug.md` first and tell the user `change.md` is still pending). Either indicates a **lightweight run** - proceed to the **Bug/Change Sequence** below instead of the full canonical one.

**After successfully reading from any source:**
- For A/B: confirm both a requirement and a version were captured.
- For C: confirm the `## Target Feature Folder` field is filled in AND that `docs/{that-folder}/` actually exists - if either fails, stop and ask rather than guessing.
- Once the relevant agent has successfully started, rename the source intake file so it isn't reprocessed:
  - `requirement.md` -> `requirement.processed-{feature-name}.md`
  - `bug.md` -> `bug.processed-{target-folder}-{short-slug}.md`
  - `change.md` -> `change.processed-{target-folder}-{short-slug}.md`

If none of the three sources yields valid input, stop immediately and show this:

```markdown
[INVALID INPUT FORMAT DETECTED]

This command needs one of: an inline requirement, docs/intake/requirement.md, docs/intake/bug.md, or docs/intake/change.md.

**What You Provided:**
{paste the user's actual input, or note which intake files were checked and why each failed}

**Option A - inline:**
requirement={your requirement text} nopcommerce-version={e.g. 4.90.8}

**Option B - new feature/phase:**
Fill in docs/intake/requirement.md, then re-run /nopcommerce-workflow with no arguments.

**Option C - bug or change to something already built:**
Fill in docs/intake/bug.md or docs/intake/change.md (both require an existing Target Feature Folder), then re-run /nopcommerce-workflow with no arguments.

**Action Required:**
Resubmit with the correct inline format, or fill in the relevant intake file and try again.
```

Do not guess a version, target folder, or intake type - proceed only once the source is genuinely valid.

**Intake-file reprocessing rule:** only the exact filenames `requirement.md`, `bug.md`, `change.md` count as valid, unprocessed intake sources. Never re-read a file already renamed to `*.processed-*.md` as if it were new input.

## Canonical Sequence (new feature or new phase - source A or B)

Work through these steps in order. After each step, briefly summarize the output before continuing. For steps flagged **[hard gate]**, do not proceed without explicit user approval.

1. **Delegate to `nopcommerce-requirement-analyzer`** with the requirement and version. Produces `docs/{feature-name}/requirements/`, `stories/`, `acceptance-criteria/`. If this is a detected phase 2+ (see that agent's Multi-Phase Initiatives section), it reads the prior phase's folder first and names this one `{base-feature-name}-phaseN`.
2. **Delegate to `nopcommerce-technical-designer`** with the requirement-analyzer's outputs. Produces `docs/{feature-name}/design/technical-design.md`, including the Placement Decision. **[hard gate]** - if the Placement Decision is "genuine core modification," explicitly flag this to the user and confirm they want to proceed before continuing.
3. **Delegate to `nopcommerce-ticket-manager`**: Produces `docs/{feature-name}/tickets/tickets.md`.
4. **Delegate to `nopcommerce-implementation-planner`** with the design and `tickets.md`. Produces `docs/{feature-name}/plan/implementation-plan.md` and extends `tickets.md` with Task-level entries.
5. **[hard gate] Plan Approval** - show the user the task list from `tickets.md` and ask Yes/No/Revise before any code is written. Do not proceed on anything but explicit Yes.
6. **Delegate to `nopcommerce-developer`** - one delegation per ticket (backend and UI together, since this agent handles both), passing `ticket-id` so it updates its own ticket's Status in `tickets.md` as it works.
7. **Delegate to `nopcommerce-qa-tester`** with `tickets.md`, stories, and acceptance criteria.
8. **Bug-fix loop**: for any Bug-type ticket QA created, re-delegate to `nopcommerce-developer` referencing that ticket ID, then re-run QA on it. Repeat until no unresolved critical/high defects remain.
9. **Context reset reminder**: once done, tell the user this feature's pipeline is complete and recommend starting a fresh Claude Code session (`/clear` or a new session) before beginning the next requirement, so stale context doesn't bleed into it.

## Bug/Change Sequence (source C - lightweight, no new phase folder)

1. **Delegate to `nopcommerce-requirement-analyzer`** with the bug/change content and target folder. Per its Bug and Change Intake section, this does NOT produce a new requirements/stories/acceptance-criteria doc set - it hands off directly to ticket creation.
2. **Delegate to `nopcommerce-ticket-manager`** (its Path C) to add exactly one new ticket to the existing `docs/{target-folder}/tickets/tickets.md`.
3. Report the new ticket ID to the user. Ask: proceed straight to development now, or stop here and let the user batch it with other pending work first?
4. If proceeding: **delegate to `nopcommerce-developer`** with that specific `ticket-id`, then **`nopcommerce-qa-tester`** to verify it.
5. **Context reset reminder** as in the canonical sequence, once resolved.

This sequence skips technical design and implementation planning entirely - a bug/change ticket doesn't need a new Placement Decision or task breakdown, since it's operating within a plugin/design that already exists.

Between any step, if you're unsure an output is solid enough to proceed on, do a quick self-check against that agent's own Definition of Done section yourself rather than delegating to a separate validator - a dedicated validation agent isn't part of this streamlined roster.

## Definition of Done
- Feature implemented and acceptance criteria satisfied.
- No unresolved critical/high defects - all Bug tickets in `tickets.md` at `Done`.
- `docs/{feature-name}/tickets/tickets.md` fully up to date, with every ticket's Status reflecting its real outcome.
- If sourced from `docs/intake/requirement.md`: file renamed to `requirement.processed-{feature-name}.md` (confirm this in your final summary to the user).
- DevOps work items (if synced) correctly tagged `nop-plugin`/`nop-core` and linked back into `tickets.md`.

## Rules
- Never skip Step 0 or either hard gate (Step 2's core-modification confirmation, Step 5's plan approval).
- Never let a `nop-core` ticket move through development/QA as if it were routine plugin work - keep its flag visible at every step you report to the user.
- Always pass explicit `docs/{feature-name}/...` paths and the `tickets.md` path between delegations - don't rely on subagents remembering earlier steps, since each runs in its own context.
- If a subagent's output is incomplete or ambiguous, don't guess forward - ask the user or re-delegate with a clarification.
