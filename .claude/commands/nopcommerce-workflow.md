---
description: Run the streamlined nopCommerce BA -> SA -> Tickets -> Plan -> Dev -> QA pipeline with gates
---

You are driving the nopCommerce feature-delivery pipeline end to end, using the streamlined 6-agent roster. Since subagents here don't auto-chain, you carry the sequencing yourself.

## Step 0 - Input Validation (mandatory, first)

There are two supported ways to provide the requirement - check in this order:

**A. Inline text** - the user's request to this command contains:
```
requirement={text} nopcommerce-version={e.g. 4.90.8}
```

**B. Intake file** - if the command is invoked with no inline `requirement=`/`nopcommerce-version=` text (e.g. just `/nopcommerce-workflow` on its own), look for `docs/intake/requirement.md` at the repo root. Expected format:
```markdown
# Requirement

<free-text description of what's needed>

## nopCommerce Version
4.90.8
```
Read the file, extract the description (everything under the `# Requirement` heading) as the requirement text, and the version from the `## nopCommerce Version` section. If the file doesn't exist, or exists but is missing either part, fall through to the invalid-input handling below rather than guessing.

**After successfully reading from either source:**
- Confirm both a requirement and a version were captured.
- If input came from the intake file, once Step 1 (BA) has successfully started and the requirement is confirmed clarified, rename the intake file to `docs/intake/requirement.processed-{feature-name}.md` (Write/Edit tool, not chat text) so it isn't accidentally reprocessed on the next invocation, and note this in your summary to the user.

If neither source yields a valid requirement + version, stop immediately and show this:

```markdown
[INVALID INPUT FORMAT DETECTED]

This command needs a requirement and a target nopCommerce version, from one of two sources.

**What You Provided:**
{paste the user's actual input, or "docs/intake/requirement.md not found / incomplete" if source B was attempted}

**Option A - inline:**
requirement={your requirement text} nopcommerce-version={e.g. 4.90.8}

**Option B - intake file:**
Fill in docs/intake/requirement.md with a `# Requirement` section (free text) and a `## nopCommerce Version` section, then re-run /nopcommerce-workflow with no arguments.

**Example (Correct, inline):**
requirement=As a store admin I need a loyalty points system nopcommerce-version=4.90.8

**Action Required:**
Resubmit with the correct inline format, or fill in docs/intake/requirement.md and try again.
```

Do not guess a version or proceed without one.

**Intake-file reprocessing rule:** only the exact filename `docs/intake/requirement.md` counts as a valid, unprocessed intake source. Never re-read a file already renamed to `requirement.processed-*.md` as if it were new input.

## Canonical Sequence

Work through these steps in order. After each step, briefly summarize the output before continuing. For steps flagged **[hard gate]**, do not proceed without explicit user approval.

1. **Delegate to `nopcommerce-requirement-analyzer`** with the requirement and version. Produces `docs/{feature-name}/requirements/`, `stories/`, `acceptance-criteria/`.
2. **Delegate to `nopcommerce-technical-designer`** with the requirement-analyzer's outputs. Produces `docs/{feature-name}/design/technical-design.md`, including the Placement Decision. **[hard gate]** - if the Placement Decision is "genuine core modification," explicitly flag this to the user and confirm they want to proceed before continuing.
3. **Delegate to `nopcommerce-ticket-manager`**:  Produces `docs/{feature-name}/tickets/tickets.md`.
4. **Delegate to `nopcommerce-implementation-planner`** with the design and `tickets.md`. Produces `docs/{feature-name}/plan/implementation-plan.md` and extends `tickets.md` with Task-level entries.
5. **[hard gate] Plan Approval** - show the user the task list from `tickets.md` and ask Yes/No/Revise before any code is written. Do not proceed on anything but explicit Yes.
6. **Delegate to `nopcommerce-developer`** - one delegation per ticket (backend and UI together, since this agent handles both), passing `ticket-id` so it updates its own ticket's Status in `tickets.md` as it works.
7. **Delegate to `nopcommerce-qa-tester`** with `tickets.md`, stories, and acceptance criteria.
8. **Bug-fix loop**: for any Bug-type ticket QA created, re-delegate to `nopcommerce-developer` referencing that ticket ID, then re-run QA on it. Repeat until no unresolved critical/high defects remain.
9. **Context reset reminder**: once done, tell the user this feature's pipeline is complete and recommend starting a fresh Claude Code session (`/clear` or a new session) before beginning the next requirement, so stale context doesn't bleed into it.

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
