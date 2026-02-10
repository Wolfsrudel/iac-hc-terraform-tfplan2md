using NetArchTest.Rules;
using TUnit.Assertions.Exceptions;
using TUnit.Core;

namespace Oocx.TfPlan2Md.TUnit.Architecture;

/// <summary>
/// Architecture tests that enforce layer boundaries and dependency rules.
/// See docs/architecture-rules.md for rule documentation.
/// Related ADR: docs/adr-007-architecture-boundary-enforcement.md
/// </summary>
public class ArchitectureBoundaryTests
{
    // === LAYER DEPENDENCY RULES (FORBIDDEN) ===

    /// <summary>
    /// Verifies that the Parsing layer does not depend on MarkdownGeneration.
    /// Parsing is a core domain layer and should remain independent of rendering concerns.
    /// </summary>
    [Test]
    public void Parsing_ShouldNotDependOn_MarkdownGeneration()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("Oocx.TfPlan2Md.Parsing")
            .ShouldNot().HaveDependencyOn("Oocx.TfPlan2Md.MarkdownGeneration")
            .GetResult();

        if (!result.IsSuccessful)
        {
            throw new AssertionException(CreateViolationMessage(
                "Parsing layer must not depend on MarkdownGeneration",
                "Parsing is a core domain layer and should not know about rendering concerns. This prevents circular dependencies and maintains clean separation between parsing and rendering.",
                result.FailingTypes));
        }
    }

    /// <summary>
    /// Verifies that the Parsing layer does not depend on CLI.
    /// Core domain should be independent of user interface concerns.
    /// </summary>
    [Test]
    public void Parsing_ShouldNotDependOn_CLI()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("Oocx.TfPlan2Md.Parsing")
            .ShouldNot().HaveDependencyOn("Oocx.TfPlan2Md.CLI")
            .GetResult();

        if (!result.IsSuccessful)
        {
            throw new AssertionException(CreateViolationMessage(
                "Parsing layer must not depend on CLI",
                "Core domain layer should be independent of user interface concerns, allowing parsing logic to be reused in different contexts (CLI, API, library).",
                result.FailingTypes));
        }
    }

    /// <summary>
    /// Verifies that the Parsing layer does not depend on Providers.
    /// Core parsing logic should be provider-agnostic.
    /// </summary>
    [Test]
    public void Parsing_ShouldNotDependOn_Providers()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("Oocx.TfPlan2Md.Parsing")
            .ShouldNot().HaveDependencyOn("Oocx.TfPlan2Md.Providers")
            .GetResult();

        if (!result.IsSuccessful)
        {
            throw new AssertionException(CreateViolationMessage(
                "Parsing layer must not depend on Providers",
                "Core parsing logic should be provider-agnostic. Provider-specific handling happens in the Providers layer, which depends on Parsing (not the reverse).",
                result.FailingTypes));
        }
    }

    /// <summary>
    /// Creates a formatted violation message with all required components.
    /// </summary>
    /// <param name="rule">The architectural rule that was violated.</param>
    /// <param name="rationale">Why this rule exists (architectural principle).</param>
    /// <param name="failingTypes">List of types that violate the rule.</param>
    /// <returns>Formatted error message with rule, rationale, violations, guidance link, and ADR reference.</returns>
    private static string CreateViolationMessage(string rule, string rationale, IEnumerable<Type>? failingTypes)
    {
        var violations = failingTypes?.Any() == true
            ? string.Join("\n  - ", failingTypes.Select(t => t.FullName))
            : "(none)";

        return $@"
Architecture Violation: {rule}

Rationale: {rationale}

Violations found in:
  - {violations}

See docs/architecture-rules.md for guidance on architectural boundaries.
Related ADR: docs/adr-007-architecture-boundary-enforcement.md
";
    }
}
