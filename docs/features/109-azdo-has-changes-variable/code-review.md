# Code Review: Azure DevOps Has-Changes Pipeline Variable

## Summary

This review covers the implementation of feature 109: emitting an Azure DevOps logging command
(`##vso[task.setvariable variable=tfplan2md_haschanges]true|false`) to stdout when the render
target is `AzureDevOps`. The change spans two files: `ProgramEntry.cs` (core logic, ~7 lines)
and `ProgramMainTests.cs` (three new CLI integration tests).

The core logic is **correct** and the tests that do exist all pass (all 1140 tests pass). However,
the review finds a **Blocker** in Work Protocol completeness, two **Major** issues in test coverage
and missing documentation, and several **Minor** issues.

**Decision: Changes Requested.**

---

## Verification Results

- **Tests**: ✅ Pass — 1140 passed, 0 failed (full suite with `--timeout-seconds 300`)
- **Build**: ✅ Success (no build errors required; ran against pre-built binary)
- **Docker**: Not checked (no Docker available in this environment; Docker build not required
  for these non-rendering changes)
- **Markdownlint (comprehensive demo)**: ✅ 0 errors — `artifacts/comprehensive-demo.md`
  regenerated cleanly
- **Errors**: None at runtime

---

## Specification Compliance

The feature was delivered without a formal specification document (the work item folder contains
only `analysis.md` and `work-protocol.md`). Acceptance criteria are inferred from `analysis.md`.

| Acceptance Criterion | Implemented | Tested | Notes |
|----------------------|:-----------:|:------:|-------|
| Emit `##vso[task.setvariable variable=tfplan2md_haschanges]true` when plan has real changes | ✅ | ✅ | `WithChanges` test uses `azapi-create-plan.json` |
| Emit `##vso[task.setvariable variable=tfplan2md_haschanges]false` when plan is no-op | ✅ | ✅ | `WithNoChanges` test uses `no-op-plan.json` |
| Do NOT emit the variable when `--render-target github` | ✅ | ✅ | `WithGitHubRenderTarget` test |
| Default render target (AzureDevOps, no flag) emits the variable | ✅ | ⚠️ | Implicitly tested by `WithChanges` (no `--render-target` flag), but no dedicated test |
| Emit `false` when ALL changes are filtered by `--ignore-azure-id-case-changes` | ✅ (formula supports it) | ❌ | **No test for this critical edge case** |
| Emit to stdout (not stderr) | ✅ | ✅ | `result.StdOut` assertions confirm stdout |
| Values are lowercase `true`/`false` | ✅ | ✅ | Assertion strings use lowercase |
| Variable emitted after markdown output is written | ✅ | ✅ | Insertion point in code is correct |

**Spec Deviations Found:** None — the implemented behaviour matches the analysis document.

---

## Adversarial Testing

| Test Case | Result | Notes |
|-----------|--------|-------|
| No-op plan → false | ✅ Pass | `no-op-plan.json` has one `['no-op']` resource; `Summary.Total = 0` |
| Plan with additions → true | ✅ Pass | `azapi-create-plan.json` has create actions |
| GitHub render target → no output | ✅ Pass | Confirmed by test and manual code inspection |
| All-filtered casing-only plan → false | ❌ Not Tested | `azurerm-case-only-ids-plan.json` + `--ignore-azure-id-case-changes` is the exact scenario; test data exists but no test covers it |
| Negative value of `Total - FilteredResourceCount` | ✅ Not a concern | Would require a bug in `ReportModelBuilder` to produce; defensive is unnecessary here |
| stdout vs stderr separation | ✅ Pass | `Console.WriteLine` writes to stdout; confirmed by test assertions on `result.StdOut` |

---

## Review Decision

**Status: Changes Requested**

---

## Snapshot Changes

- Snapshot files changed: No
- N/A

---

## Issues Found

### Blockers

#### B1 — Work Protocol incomplete: five required agents have no log entries

**File:** `docs/features/109-azdo-has-changes-variable/work-protocol.md`

This is a **Feature** workflow. Per `docs/agents.md § Required Agents by Workflow Type`, the
following agents are **required** and must have logged entries before a Code Review can be
approved:

| Agent | Entry present? |
|-------|:-------------:|
| Requirements Engineer | ⚠️ Not present (Issue Analyst performed initial analysis at Maintainer request — see note) |
| Issue Analyst | ✅ Present |
| Architect | ❌ Missing |
| Quality Engineer | ❌ Missing |
| Task Planner | ❌ Missing |
| Developer | ❌ Missing — the Developer authored the code change (commit `c991e28`) but did not log their work |
| Technical Writer | ❌ Missing |

Note: the Issue Analyst entry acknowledges that the Requirements Engineer step was skipped at
Maintainer request. That is acceptable for the Requirements Engineer entry alone. The remaining
four missing entries (Architect, Quality Engineer, Task Planner, Developer, Technical Writer)
are **Blockers** — the workflow was not followed.

**Action required:** Invoke all missing agents and have each append their entry to
`work-protocol.md` before re-review. The Developer entry should acknowledge that the initial
coding is complete and note any decisions made during implementation.

---

### Major Issues

#### M1 — Missing test for the all-filtered changes scenario

**File:** `src/tests/Oocx.TfPlan2Md.TUnit/CLI/ProgramMainTests.cs`

The formula `model.Summary.Total - model.FilteredResourceCount > 0` is designed to return
`false` when every resource in the plan has only Azure ID casing-only changes that were
suppressed by `--ignore-azure-id-case-changes`. This is the **only case where the subtraction
matters** — if the formula were simplified to `model.Summary.Total > 0`, the two existing
tests would still pass, but the casing-only-filtered scenario would produce an incorrect
`true` result.

The analysis document (section 7, test table) explicitly recommended:

> `Main_WithAzureDevOpsTarget_AllFilteredChanges_EmitsFalse` — Plan whose only changes are
> Azure ID casing (all filtered) → `StdOut` contains `…]false`

Test data for this scenario already exists:
- `TestData/azurerm-case-only-ids-plan.json` — a plan where `azurerm_role_assignment.casing_only`
  is an update with only casing-difference attribute values

The test can be written as:

```csharp
var result = await RunMainAsync([inputPath, "--output", outputPath, "--ignore-azure-id-case-changes"]);
result.StdOut.Should().Contain("##vso[task.setvariable variable=tfplan2md_haschanges]false");
```

(Note: `--ignore-azure-id-case-changes` is the default but specifying it explicitly clarifies
intent for the reader.)

Without this test the formula correctness is not verified by the test suite.

#### M2 — `docs/features.md` not updated

**File:** `docs/features.md`

Feature 109 is not described in `docs/features.md`. This is an established requirement for all
features per the reviewer's checklist and the Technical Writer's responsibilities. The file
currently ends at feature 107; feature 109 should be documented here with a short description
of the behaviour (what the variable is, when it is emitted, how to consume it in a pipeline).

#### M3 — Help text does not document the `tfplan2md_haschanges` variable

**File:** `src/Oocx.TfPlan2Md/CLI/HelpTextProvider.cs`

The `--render-target` option description currently reads:

```
--render-target <github|azuredevops>    Target platform for rendering (default: azuredevops).
```

Since `azuredevops` is the **default**, every user who runs `tfplan2md plan.json` will have the
`##vso[task.setvariable …]` line emitted to stdout without any indication that this happens.
There is no way for a user to discover this side-effect from `--help`.

The analysis document acknowledges this as "Optional: Help text update" (section 6), but given
that:
1. The side-effect affects the **default** invocation, not an opt-in flag
2. It is invisible until a pipeline variable unexpectedly appears
3. Users may inadvertently corrupt piped output in local dev workflows

This rises to a **Major** issue. At minimum, the `--render-target` description or a dedicated
"Output Variables" section in the help text should mention `tfplan2md_haschanges`.

---

### Minor Issues

#### m1 — Missing dedicated test for the default render target

**File:** `src/tests/Oocx.TfPlan2Md.TUnit/CLI/ProgramMainTests.cs`

The analysis recommended a specific test `Main_WithAzureDevOpsDefaultRenderTarget_EmitsHasChangesVariable`
to **explicitly** document and verify that the variable is emitted when no `--render-target`
flag is given (relying on the AzureDevOps default). The existing `WithChanges` test incidentally
exercises this (its args array has no `--render-target`), but the test name does not make this
property explicit.

A dedicated test — or at minimum renaming `WithChanges` to include "DefaultRenderTarget" in its
name — would make the intent clear and prevent future confusion.

#### m2 — Inline comment lacks feature spec reference

**File:** `src/Oocx.TfPlan2Md/ProgramEntry.cs`, line 144

The new block uses:
```csharp
// Emit Azure DevOps pipeline variable for downstream steps
```

The project convention (used elsewhere in the same file and throughout the codebase) adds a
"Related feature:" reference to inline comments that implement specific features:

```csharp
// Emit Azure DevOps pipeline variable for downstream steps.
// Related feature: docs/features/109-azdo-has-changes-variable/analysis.md
```

Compare with `ProgramEntry.cs` line 2:
```csharp
// Related feature: docs/features/046-code-quality-metrics-enforcement/.
```

And `ReportModelBuilder.Build.cs` line 42:
```csharp
// Related feature: docs/features/068-parent-child-resource-grouping/specification.md
```

#### m3 — Readability: ternary expression inside string interpolation

**File:** `src/Oocx.TfPlan2Md/ProgramEntry.cs`, line 148

```csharp
Console.WriteLine($"##vso[task.setvariable variable=tfplan2md_haschanges]{(hasChanges ? "true" : "false")}");
```

The analysis explicitly recommended an intermediate variable for clarity:

```csharp
var hasChangesValue = hasChanges ? "true" : "false";
Console.WriteLine($"##vso[task.setvariable variable=tfplan2md_haschanges]{hasChangesValue}");
```

The nested ternary in an interpolated string requires extra syntax (the outer parentheses) and
makes the line harder to scan. The intermediate variable approach is consistent with how other
string-building in the codebase is handled and reduces visual noise.

---

### Suggestions

None beyond the issues documented above.

---

## Critical Questions Answered

- **What could make this code fail?**
  The formula `model.Summary.Total - model.FilteredResourceCount` is safe: both properties are
  non-negative integers set by `ReportModelBuilder.Build` before `ProgramEntry` executes. A
  negative result would require a bug in the model builder. No null-dereference risk exists
  since `model` and `model.Summary` are always initialised before this point.

- **What edge cases might not be handled?**
  The all-filtered casing scenario (M1) is the main unverified edge case. Output-only changes
  (`model.GlobalOutputs`, `model.ModuleChanges[].Outputs`) are intentionally excluded from
  `hasChanges` per the analysis design decision; this is documented and acceptable.

- **Are all error paths tested?**
  Not applicable for this feature — the new code block has no error paths (it cannot throw
  under normal or abnormal input conditions).

---

## Work Protocol & Documentation Verification

| Check | Status |
|-------|--------|
| `work-protocol.md` exists | ✅ |
| All required agents logged (Feature workflow) | ❌ **BLOCKER** — see B1 |
| `docs/features.md` updated | ❌ **MAJOR** — see M2 |
| `docs/architecture.md` update needed? | No — feature is a one-line stdout emission; no architectural change |
| `docs/testing-strategy.md` update needed? | No — no new test patterns introduced |
| `README.md` update needed? | No — no CLI usage changes visible to first-time users |
| `docs/agents.md` update needed? | No |

---

## Checklist Summary

| Category | Status |
|----------|--------|
| Correctness | ✅ Logic is correct for all verified scenarios |
| Spec Compliance | ✅ Matches analysis document intent |
| Code Quality | ⚠️ Minor style issues (m2, m3) |
| Architecture | ✅ Insertion point is correct; no architectural changes |
| Testing | ❌ Missing critical edge case test (M1) and default-target test (m1) |
| Documentation | ❌ `docs/features.md` missing (M2); help text missing (M3) |
| Work Protocol | ❌ Five required agent entries absent (B1) |

---

## Next Steps

This review requests the following actions before re-approval:

1. **Maintainer:** Invoke the missing agents in sequence — Architect, Quality Engineer,
   Task Planner, Developer (to log their work), and Technical Writer — so each appends an
   entry to `work-protocol.md`.

2. **Developer:** Add the missing test for the all-filtered casing scenario (M1) and consider
   adding a dedicated test for the default render target (m1). Apply the minor comment and
   intermediate variable improvements (m2, m3).

3. **Technical Writer:** Update `docs/features.md` with feature 109 (M2). Update
   `HelpTextProvider.cs` to mention `tfplan2md_haschanges` in the `--render-target` description
   or examples (M3).

Once all Blockers and Major issues are resolved, return to **Code Reviewer** for re-approval,
then proceed to **Release Manager**.
