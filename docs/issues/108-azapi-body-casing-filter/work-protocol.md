# Work Protocol: azapi Body Casing Filter

**Work Item:** `docs/issues/108-azapi-body-casing-filter/`
**Branch:** `copilot/filter-out-casing-changes`
**Workflow Type:** Bug Fix
**Created:** 2026-03-02

## Agent Work Log

<!-- Each agent appends their entry below when they complete their work. -->

### Issue Analyst — 2026-03-02

**Summary:** Investigated the azapi casing-only change bug. Identified two distinct rendering pipelines where the fix must be applied, with the primary pipeline being the AzAPI body comparison helper (`CompareJsonProperties`/`ValuesEqual`). Documented root cause, affected files with line numbers, proposed fix approach, and edge cases.

**Artifacts Produced:**
- `docs/issues/108-azapi-body-casing-filter/analysis.md` — Full root cause analysis with file references and proposed fix

**Problems Encountered:** None. Root cause was clear from the code.
