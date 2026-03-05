# Work Protocol: Azure DevOps Has-Changes Variable

**Work Item:** `docs/features/109-azdo-has-changes-variable/`
**Branch:** `copilot/add-azure-devops-logging-commands`
**Workflow Type:** Feature
**Created:** 2026-06-17

## Agent Work Log

<!-- Each agent appends their entry below when they complete their work. -->

### Issue Analyst
- **Date:** 2026-06-17
- **Summary:** Analysed the feature request for emitting an Azure DevOps logging command
  (`##vso[task.setvariable variable=tfplan2md_haschanges]`) when the render target is
  Azure DevOps. Investigated the render-target pipeline, "has changes" semantics in
  `ReportModel`/`SummaryModel`, the output path in `ProgramEntry`, and the existing ADO
  render-target and test infrastructure. Produced a pre-implementation analysis document.
  Note: This is a feature request, not a bug — under normal workflow this would be handled
  by Requirements Engineer. Maintainer requested Issue Analyst perform the initial analysis.
- **Artifacts Produced:**
  - `docs/features/109-azdo-has-changes-variable/work-protocol.md`
  - `docs/features/109-azdo-has-changes-variable/analysis.md`
- **Problems Encountered:** None

### Code Reviewer
- **Date:** 2026-06-17
- **Summary:** Reviewed implementation of `ProgramEntry.cs` (7-line ADO logging command block)
  and three new CLI integration tests. All 1140 tests pass; comprehensive demo passes
  markdownlint. Core logic is correct. Review status: **Changes Requested**.
- **Artifacts Produced:**
  - `docs/features/109-azdo-has-changes-variable/code-review.md`
- **Problems Encountered:**
  - **Blocker B1:** Work Protocol is missing required entries for Architect, Quality Engineer,
    Task Planner, Developer, and Technical Writer.
  - **Major M1:** No test for the all-filtered casing scenario that exercises the
    `Summary.Total - FilteredResourceCount` formula's key distinction.
  - **Major M2:** `docs/features.md` not updated with feature 109.
  - **Major M3:** `HelpTextProvider.cs` does not document the `tfplan2md_haschanges` variable.
  - **Minor m1–m3:** Missing default-render-target test, missing feature spec ref in comment,
    ternary embedded in string interpolation instead of intermediate variable.
