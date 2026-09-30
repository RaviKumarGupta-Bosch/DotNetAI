namespace DotNetAI.AgenticAI.Samples.Tools;

public static class EngineeringTools
{
    public static string GetSupportTicket(string ticketId) =>
        $"Ticket {ticketId}: Checkout fails after address entry; severity reported as high; customer is blocked from ordering. This is sample fixture data.";

    public static string SearchSupportKnowledge(string query) =>
        $"Knowledge search for '{query}': validate the address-service response and check recent checkout releases. This is sample fixture data.";

    public static string GetServiceMetrics(string serviceName) =>
        $"{serviceName}: error rate 8.2% (baseline 0.4%), p95 latency 3.8s (baseline 620ms), started 14:05 UTC. Sample telemetry only.";

    public static string SearchServiceLogs(string query) =>
        $"Log search '{query}': repeated AddressValidationTimeout events after deployment 2026.09.30.3. Sample logs only.";

    public static string GetPullRequestDiff(string pullRequestId) =>
        $"Pull request {pullRequestId} changes retry handling and request validation in CheckoutService. Diff fixture; no repository was contacted.";

    public static string GetPullRequestChecks(string pullRequestId) =>
        $"Pull request {pullRequestId}: unit tests passed; integration tests report one timeout in AddressValidationTests. Fixture checks only.";

    public static string SearchEngineeringDocs(string query) =>
        $"Documentation results for '{query}': use the shared retry policy; address validation is not idempotent until the request ID is persisted. Sample docs only.";

    public static string GetPolicyVersion(string policyName) =>
        $"{policyName}: version 4.2, reviewed 2026-08-15; require human approval for account recovery exceptions. Sample policy record.";

    public static string ExtractInvoiceSummary(string documentId) =>
        $"Invoice {documentId}: supplier Northwind Hosting; total $4,850.00; due 2026-10-30; purchase order PO-7712. Extracted from sample fixture.";

    public static string CheckPurchaseOrder(string purchaseOrder) =>
        $"{purchaseOrder}: open balance $5,000.00; supplier matches; amount is within limit. Sample finance data; no approval performed.";

    public static string GetBuildAndTestStatus(string branch) =>
        $"{branch}: build succeeded; 418 tests passed; 2 flaky browser tests quarantined; package scan has 1 medium advisory. Sample CI status.";

    public static string GetReleaseChanges(string version) =>
        $"Release {version}: adds saved searches and fixes checkout retry behavior; migration 042 adds a nullable Region column. Sample changelog.";

    public static string GetPackageAdvisory(string packageName, string version) =>
        $"{packageName} {version}: sample advisory indicates a high-severity denial-of-service issue fixed in 13.0.1. Verify against your actual advisory source.";

    public static string FindCompatiblePackageVersion(string packageName) =>
        $"{packageName}: sample compatibility check found 13.0.3 supports net8.0 and net9.0. Validate transitive dependencies before updating.";

    public static string GetMigrationContext(string migrationId) =>
        $"{migrationId}: adds a non-null Customer.Region field to a 12-million-row table. Sample schema metadata; migration not executed.";

    public static string CheckMigrationRisks(string migrationId) =>
        $"{migrationId}: direct table rewrite may lock writes. Consider expand/backfill/contract and prepare a tested rollback. Sample review only.";

    public static string GetAccessibilityFindings(string pageName) =>
        $"{pageName}: 3 findings: unlabeled email input, low-contrast helper text, and a modal without focus containment. Sample scan output.";

    public static string SearchAccessiblePatterns(string issueType) =>
        $"Pattern for '{issueType}': associate a visible label, preserve keyboard focus, and verify with an automated plus manual screen-reader check. Sample guidance.";

    public static string SummarizeCustomerFeedback(string productArea) =>
        $"{productArea}: 18 sample comments; 11 mention confusing validation, 5 request saved carts, 2 praise the new layout. Not production analytics.";

    public static string SearchKnownIssues(string topic) =>
        $"Known-issue search for '{topic}': one open issue tracks delayed address validation; owner is Checkout Platform. Sample issue tracker result.";
}