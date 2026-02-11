# Workflow 069: Screenshot Quality Enforcement

## Problem Context
During Feature 068 release, the Release Manager agent bypassed the "Screenshots" section of release notes when encountering a `ScreenshotGenerator` timeout. This pattern has recurred in previous releases, where agents prioritize "completing the workflow" (merging PR) over "quality of artifacts," leading to incomplete releases that violate project quality standards.

**Root Cause:** Screenshot generation is currently treated as "optional" in agent instructions, with conditional language ("if visual changes", "recommended but not required"). When technical friction occurs (timeout, environment issues), agents interpret this as permission to skip screenshots entirely.

**Evidence:**
- Feature 068 retrospective shows Release Manager bypassed repo scripts and potentially skipped screenshots
- Issue description mentions this is a "recurring pattern" across multiple releases
- Current Release Manager instructions use conditional language: "Only include a screenshots section if you have actual screenshots"

## Candidate Workflow Improvements

| ID | Title | Source | Status | Rationale | Impact | Effort | Risk | Notes |
|---:|---|---|---|---|---|---|---|---|
| 1 | Harden Release Manager instructions: Make screenshots MANDATORY for visual features | Issue description | ⬜ Not started | Current instructions treat screenshots as optional/conditional. Need explicit MUST STOP rule when generation fails. | High | Low | Low | Changes `.github/agents/release-manager-coding-agent.agent.md` Boundaries section |
| 2 | Update copilot-instructions.md: Add global screenshot quality standards | Issue description | ⬜ Not started | Project-wide guidance missing. All agents should understand screenshot importance for visual features. | Medium | Low | Low | Adds new "Screenshot Guidelines" section |
| 3 | Add pre-release validation script: Fail if screenshots missing for visual features | Issue description | ⬜ Not started | Automated guardrail prevents incomplete releases from being merged. | High | Medium | Medium | Requires defining "visual feature" heuristic; may need work item folder inspection |
| 4 | Investigate ScreenshotGenerator timeout root causes | Issue description | ⬜ Not started | Address technical friction: CDN lookups, browser init overhead in dev container. | Medium | High | Low | Requires debugging Playwright in resource-constrained environment |
| 5 | Implement local CSS/asset caching for ScreenshotGenerator | Issue description | ⬜ Not started | Eliminate external network dependencies that cause timeouts. | Medium | High | Medium | Changes HTML generation or Playwright setup |
| 6 | Enhance screenshot generation script with retry logic and verbose errors | Issue description | ⬜ Not started | Better error reporting when failures occur; retry transient failures. | Low | Medium | Low | Changes `scripts/generate-release-screenshots.sh` |

## Recommendations

- **Option 1 (Best balance of effort/impact):** **IDs 1, 2, 3** — Instruction hardening (#1, #2) plus validation guardrail (#3) creates defense-in-depth: agents know the rule, and automation enforces it. Medium effort (need validation script), high impact (prevents incomplete releases).
  
- **Option 2 (Quick win):** **IDs 1, 2** — Instruction-only changes (lowest effort). Makes expectations explicit in agent and global instructions. Relies on agent compliance without automated enforcement.
  
- **Option 3 (Root cause fix):** **IDs 4, 5** — Address technical debt causing timeouts. Highest effort (debugging Playwright, implementing caching), uncertain impact (may not eliminate all failures). Best done after implementing guardrails to prevent workarounds.

## Decision
*Waiting for Maintainer selection via PR comment.*
