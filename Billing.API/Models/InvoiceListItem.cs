using System;

namespace Billing.API.Models
{
    public class InvoiceListItem
    {
        public string Product { get; }
        public string AccountId { get; }
        public DateTimeOffset Date { get; }
        public string Currency { get; }
        public double Amount { get; }
        public string Link { get; }

        public InvoiceListItem(string product, string accountId, DateTimeOffset date, string currency, double amount, string link)
        {
            Product = product.EqualsIgnoreCase("CD") ? "Doppler"
                : product.EqualsIgnoreCase("CR") ? "Relay"
                : product.EqualsIgnoreCase("CM") ? "Client Manager"
                : product;
            AccountId = accountId;
            Date = date;
            Currency = currency;
            Amount = amount;
            Link = link;
        }
    }
}
