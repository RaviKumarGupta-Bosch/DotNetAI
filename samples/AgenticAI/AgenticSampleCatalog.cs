using DotNetAI.AgenticAI.Samples.Models;
using DotNetAI.AgenticAI.Samples.Tools;
using DotNetAI.Core.Testing;
using Microsoft.Extensions.AI;

namespace DotNetAI.AgenticAI.Samples;

public static class AgenticSampleCatalog
{
    public static IReadOnlyList<AgenticSample> Create() =>
    [
        new(
            "01", "Support ticket triage", "Gather ticket and knowledge-base evidence, then draft a response and escalation recommendation.",
            "Triage ticket INC-8421. Identify likely cause, urgency, and the next diagnostic question. Draft a reply but do not send it.",
            [AIFunctionFactory.Create(EngineeringTools.GetSupportTicket), AIFunctionFactory.Create(EngineeringTools.SearchSupportKnowledge)],
            nameof(EngineeringTools.GetSupportTicket), Args(("ticketId", "INC-8421")),
            "Ticket INC-8421 is high urgency because checkout is blocked. Check the address-service response and recent releases; ask the customer for the failed address-validation request ID. Draft only; not sent."),
        new(
            "02", "Production incident investigation", "Correlate service telemetry with logs and prepare an evidence-based incident handoff.",
            "Investigate the checkout-api latency and error spike. State evidence, likely trigger, and safe next steps without changing production.",
            [AIFunctionFactory.Create(EngineeringTools.GetServiceMetrics), AIFunctionFactory.Create(EngineeringTools.SearchServiceLogs)],
            nameof(EngineeringTools.GetServiceMetrics), Args(("serviceName", "checkout-api")),
            "Metrics show elevated errors and latency beginning at 14:05 UTC; logs point to AddressValidationTimeout after deployment 2026.09.30.3. Compare the deployment and dependency health, then have the on-call engineer approve any mitigation."),
        new(
            "03", "Pull request review assistant", "Inspect a change and CI evidence, then produce actionable review findings without approving or merging.",
            "Review PR-482 for correctness and test risks. Separate confirmed findings from questions and do not approve or merge it.",
            [AIFunctionFactory.Create(EngineeringTools.GetPullRequestDiff), AIFunctionFactory.Create(EngineeringTools.GetPullRequestChecks)],
            nameof(EngineeringTools.GetPullRequestDiff), Args(("pullRequestId", "PR-482")),
            "The retry change needs scrutiny because address validation may not be idempotent. CI also has a timeout in AddressValidationTests. Ask how request IDs are persisted and add a deterministic integration test; no review was submitted."),
        new(
            "04", "Internal documentation research", "Search engineering guidance and policy, then answer with source-aware caveats.",
            "Find guidance for safely retrying address validation and check whether account recovery exceptions require approval.",
            [AIFunctionFactory.Create(EngineeringTools.SearchEngineeringDocs), AIFunctionFactory.Create(EngineeringTools.GetPolicyVersion)],
            nameof(EngineeringTools.SearchEngineeringDocs), Args(("query", "safe retry address validation")),
            "Use the shared retry policy and persist a request ID before retrying address validation. Policy version 4.2 requires human approval for account-recovery exceptions. Verify these sample records against the live documentation source."),
        new(
            "05", "Invoice intake and matching", "Extract invoice details, reconcile a purchase order, and prepare an exception-aware approval packet.",
            "Review invoice DOC-2048 against its purchase order. Summarize mismatches and recommend routing; do not approve or pay it.",
            [AIFunctionFactory.Create(EngineeringTools.ExtractInvoiceSummary), AIFunctionFactory.Create(EngineeringTools.CheckPurchaseOrder)],
            nameof(EngineeringTools.ExtractInvoiceSummary), Args(("documentId", "DOC-2048")),
            "Invoice INV-2048 is $4,850.00 and its supplier matches PO-7712, with $5,000.00 remaining. The sample data has no mismatch; prepare the approval packet for an authorized reviewer. No payment or approval was made."),
        new(
            "06", "Release readiness review", "Combine build, test, security, and change-summary evidence into a release decision brief.",
            "Assess release/2.4 readiness. List blockers and follow-ups; do not publish, deploy, or modify the release.",
            [AIFunctionFactory.Create(EngineeringTools.GetBuildAndTestStatus), AIFunctionFactory.Create(EngineeringTools.GetReleaseChanges)],
            nameof(EngineeringTools.GetBuildAndTestStatus), Args(("branch", "release/2.4")),
            "Build succeeded and 418 tests passed, but two browser tests are quarantined and one medium package advisory remains. Resolve or explicitly accept those risks, review migration 042, and obtain release-owner approval before deployment."),
        new(
            "07", "Dependency vulnerability triage", "Assess an advisory, identify a compatible update candidate, and outline validation steps.",
            "Triage the Newtonsoft.Json 12.0.1 advisory. Recommend an update path and validation, but do not edit project files or install packages.",
            [AIFunctionFactory.Create(EngineeringTools.GetPackageAdvisory), AIFunctionFactory.Create(EngineeringTools.FindCompatiblePackageVersion)],
            nameof(EngineeringTools.GetPackageAdvisory), Args(("packageName", "Newtonsoft.Json"), ("version", "12.0.1")),
            "The sample advisory rates the issue high and names 13.0.1 as fixed; the sample compatibility lookup reports 13.0.3 supports net8.0 and net9.0. Verify the advisory, inspect transitive constraints, update in a branch, and run tests before merging."),
        new(
            "08", "Database migration planning", "Assess operational migration risk and produce a staged, reversible plan without executing SQL.",
            "Plan migration AddCustomerRegion for a large customer table. Identify rollout, backfill, validation, and rollback considerations. Do not execute SQL.",
            [AIFunctionFactory.Create(EngineeringTools.GetMigrationContext), AIFunctionFactory.Create(EngineeringTools.CheckMigrationRisks)],
            nameof(EngineeringTools.GetMigrationContext), Args(("migrationId", "AddCustomerRegion")),
            "The table has 12 million rows and the new Region field is non-null. Prefer expand, deploy nullable/default-compatible code, backfill in batches, validate, then contract in a later release. Test rollback and lock behavior on representative data; migration not executed."),
        new(
            "09", "Accessibility remediation planning", "Turn an accessibility scan into prioritized, standards-aware remediation work.",
            "Review the checkout page accessibility findings. Prioritize fixes and add manual verification steps; do not change the page.",
            [AIFunctionFactory.Create(EngineeringTools.GetAccessibilityFindings), AIFunctionFactory.Create(EngineeringTools.SearchAccessiblePatterns)],
            nameof(EngineeringTools.GetAccessibilityFindings), Args(("pageName", "checkout")),
            "First label the email input, then fix helper-text contrast and contain modal keyboard focus. Verify keyboard-only operation, focus return, contrast ratios, and screen-reader announcements; the sample scan is not a conformance certification."),
        new(
            "10", "Customer feedback synthesis", "Summarize feedback themes, cross-check known issues, and produce a traceable product follow-up.",
            "Analyze checkout feedback. Distinguish recurring themes from known defects and propose follow-up questions; do not contact customers or create tickets.",
            [AIFunctionFactory.Create(EngineeringTools.SummarizeCustomerFeedback), AIFunctionFactory.Create(EngineeringTools.SearchKnownIssues)],
            nameof(EngineeringTools.SummarizeCustomerFeedback), Args(("productArea", "checkout")),
            "Confusing validation is the dominant sample theme (11 of 18 comments); saved carts appear in 5 comments. Cross-check the delayed address-validation issue before attributing cause. Validate counts against production analytics and let a product owner decide whether to open work." )
    ];

    public static MockChatClient CreateMockClient(AgenticSample sample)
    {
        var client = new MockChatClient();
        client.EnqueueResponse((_, _) => new ChatResponse(new ChatMessage(
            ChatRole.Assistant,
            [new FunctionCallContent("sample-call", sample.MockToolName, sample.MockArguments)])));
        client.EnqueueResponse(sample.MockFinalAnswer);
        return client;
    }

    private static IDictionary<string, object?> Args(params (string Name, object? Value)[] values) =>
        values.ToDictionary(pair => pair.Name, pair => pair.Value);
}