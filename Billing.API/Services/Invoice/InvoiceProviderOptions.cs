namespace Billing.API.Services.Invoice
{
    public class InvoiceProviderOptions
    {
        public bool   UseDummyData { get; set; }
        public string Host         { get; set; } = string.Empty;
        public string Username     { get; set; } = string.Empty;
        public string Password     { get; set; } = string.Empty;
    }
}
