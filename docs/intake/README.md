# Requirement Intake

Drop a new requirement here as `requirement.md` (see the template already in this folder) instead of typing the whole `requirement=... nopcommerce-version=...` string inline when invoking the orchestrator.

## Format

```markdown
# Requirement

<free-text description of what's needed>

## nopCommerce Version

<e.g. 4.90.8>
```

## Usage

1. Edit `requirement.md` with your actual requirement and target version.
2. Invoke `nopcommerce-workflow-orchestrator` with no inline `requirement=`/`nopcommerce-version=` text.
3. Validation Gate 0 reads this file as Option B, instead of needing inline text.
4. Once the requirement is confirmed and the BA phase starts, the file is renamed to `requirement.processed-{feature-name}.md` so it doesn't get picked up again on your next run.

## Multiple pending requirements

This convention is single-file by design - only `requirement.md` (exact filename) is treated as a valid, unprocessed intake source. If you want to queue several requirements, use the inline `requirement=... nopcommerce-version=...` form for the others, or ask to extend Gate 0 to scan the whole `docs/intake/` folder for multiple pending files.

## After processing

Processed files (`requirement.processed-*.md`) are kept as a record, not deleted - useful later to compare what was originally asked for against how `docs/{feature-name}/requirements/clarified-requirement.md` turned out after the BA phase.
