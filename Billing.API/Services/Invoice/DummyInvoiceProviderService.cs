using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public class DummyInvoiceProviderService : IInvoiceService
    {
        public async Task<IEnumerable<InvoiceResponse>> GetInvoices(string clientId)
        {
            if (clientId.In("000000000000000", "999999999999999"))
                return await Task.Run(() => new List<InvoiceResponse>());

            return await Task.Run(() => new List<InvoiceResponse>()
            {
                new InvoiceResponse
                {
                    Product   = "Prod A",
                    AccountId = clientId,
                    Date      = DateTime.Today,
                    Currency  = "ARS",
                    Amount    = 100,
                    Link      = "valid_path"
                },
                new InvoiceResponse
                {
                    Product   = "Prod B",
                    AccountId = clientId,
                    Date      = DateTime.Today.AddDays(1),
                    Currency  = "ARS",
                    Amount    = 200,
                    Link      = "valid_path_b"
                }
            });
        }

        public Task TestSapConnection()
        {
            throw new System.NotImplementedException();
        }
    }
}
