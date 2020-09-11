namespace Billing.API.Models
{
    public class InvoiceFile
    {
        public string? ContentType { get; set; }

        public byte[]? Content { get; set; }
    }
}
