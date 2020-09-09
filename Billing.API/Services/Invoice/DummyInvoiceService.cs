using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public class DummyInvoiceService : IInvoiceService
    {
        public async Task<IEnumerable<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId)
        {
            if (clientId <= 0)
                throw new ArgumentException();


            return await Task.FromResult(new List<InvoiceListItem>
            {
                new InvoiceListItem(
                    "Prod A",
                    $"{clientPrefix}{clientId:0000000000000}",
                    DateTime.Today,
                    "ARS",
                    100,
                    "valid_path"),
                new InvoiceListItem(
                    "Prod B",
                    $"{clientPrefix}{clientId:0000000000000}",
                    DateTime.Today.AddDays(1),
                    "ARS",
                    200,
                    "valid_path_b")
            });
        }

        public Task<byte[]?> GetInvoiceFile(string clientPrefix, int clientId, int fileId)
        {
            var bytes = File.ReadAllBytes("sample.pdf");

            return Task.FromResult(bytes)!;
        }

        public async Task<string> TestSapConnection()
        {
            return await Task.FromResult("Successfull");
        }
    }
}
