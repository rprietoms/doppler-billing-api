using System;

namespace Billing.API.Models
{
    public class InvoiceResponse
    {
        public string Product { get; set; }
        public string AccountId { get; set; }
        public DateTime Date { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Link { get; set; }
    }
}
