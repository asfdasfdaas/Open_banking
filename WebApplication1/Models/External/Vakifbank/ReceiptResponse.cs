using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class ReceiptResponse
    {
        [JsonPropertyName("Documents")]
        public ReceiptDocuments Documents { get; set; } = new ReceiptDocuments();
    }

    public class ReceiptDocuments
    {
        [JsonPropertyName("PdfReceipt")]
        public string PdfReceipt { get; set; } = string.Empty;
    }
}
