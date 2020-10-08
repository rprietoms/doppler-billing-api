using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Billing.API.Models
{
    public class InvoiceListItem
    {
        public string Product { get; }
        public string AccountId { get; }
        public DateTimeOffset Date { get; }
        public string Currency { get; }
        public double Amount { get; }

        [JsonIgnore]
        public int FileId { get; }

        public string Filename { get; }

        [JsonProperty(PropertyName = "_links")]
        public List<Link> Links { get; } = new List<Link>();

        public InvoiceListItem(string product, string accountId, DateTimeOffset date, string currency, double amount, string filename, int fileId)
        {
            Product = product.EqualsIgnoreCase("CD") ? "Doppler"
                : product.EqualsIgnoreCase("CR") ? "Relay"
                : product.EqualsIgnoreCase("CM") ? "Client Manager"
                : product;
            AccountId = accountId;
            Date = date;
            Currency = currency;
            Amount = amount;
            Filename = filename;
            FileId = fileId;
        }
    }
}
