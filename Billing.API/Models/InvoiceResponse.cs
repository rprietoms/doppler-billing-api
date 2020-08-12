using System;

namespace Billing.API.Models
{
    public class InvoiceResponse
    {
        public string Product { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Currency { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Link { get; set; } = string.Empty;
    }
}
