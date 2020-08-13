using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public class DummyInvoiceProviderService : IInvoiceService
    {
        public async Task<IEnumerable<InvoiceListItem>> GetInvoices(string clientId)
        {
            if (clientId.In("000000000000000", "999999999999999"))
                return await Task.Run(() => new List<InvoiceListItem>());

            return await Task.Run(() => new List<InvoiceListItem>()
            {
                new InvoiceListItem(
                    "Prod A",
                    clientId,
                    DateTime.Today,
                    "ARS",
                    100,
                    "valid_path"),
                new InvoiceListItem(
                    "Prod B",
                    clientId,
                    DateTime.Today.AddDays(1),
                    "ARS",
                    200,
                    "valid_path_b")
            });
        }

        public Task TestSapConnection()
        {
            throw new System.NotImplementedException();
        }
    }
}
