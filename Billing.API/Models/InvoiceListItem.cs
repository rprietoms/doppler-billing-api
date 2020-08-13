using System;

namespace Billing.API.Models
{
    public class InvoiceListItem
    {
        public string Product { get; }
        public string AccountId { get; }
        public DateTime Date { get; }
        public string Currency { get; }
        public double Amount { get; }
        public string Link { get; }

        public InvoiceListItem(string product, string accountId, DateTime date, string currency, double amount, string link)
        {
            Product = product;
            AccountId = accountId;
            Date = date;
            Currency = currency;
            Amount = amount;
            Link = link;
        }
    }
}
