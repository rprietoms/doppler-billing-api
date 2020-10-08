namespace Billing.API.Services.Invoice
{
    public class InvoiceProviderOptions
    {
        public bool UseDummyData { get; set; }
        public string Schema { get; set; } = string.Empty;
        public string DbConnectionString { get; set; } = string.Empty;
        public string SignatureHashKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
