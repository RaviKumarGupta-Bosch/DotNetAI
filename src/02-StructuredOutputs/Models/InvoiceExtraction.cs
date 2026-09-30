using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNetAI.StructuredOutputs.Models;

public record InvoiceLineItem(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("quantity")] int Quantity,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
    [property: JsonPropertyName("total")] decimal Total
);

public record InvoiceDocument(
    [property: JsonPropertyName("invoiceNumber")] string InvoiceNumber,
    [property: JsonPropertyName("vendorName")] string VendorName,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("invoiceDate")] string InvoiceDate,
    [property: JsonPropertyName("dueDate")] string DueDate,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("lineItems")] List<InvoiceLineItem> LineItems,
    [property: JsonPropertyName("subtotal")] decimal Subtotal,
    [property: JsonPropertyName("taxAmount")] decimal TaxAmount,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("paymentTerms")] string? PaymentTerms
);

public record ContractExtraction(
    [property: JsonPropertyName("contractTitle")] string ContractTitle,
    [property: JsonPropertyName("parties")] List<string> Parties,
    [property: JsonPropertyName("effectiveDate")] string EffectiveDate,
    [property: JsonPropertyName("expirationDate")] string ExpirationDate,
    [property: JsonPropertyName("governingLaw")] string GoverningLaw,
    [property: JsonPropertyName("liabilityCap")] string LiabilityCap,
    [property: JsonPropertyName("terminationNoticeDays")] int TerminationNoticeDays,
    [property: JsonPropertyName("keyObligations")] List<string> KeyObligations
);

public class ExtractionResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
    public int AttemptCount { get; set; } = 1;
    public bool RequiredSelfCorrection { get; set; }
}
