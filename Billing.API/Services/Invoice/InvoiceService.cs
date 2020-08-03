using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Billing.API.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(ILogger<InvoiceService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetInvoices(string clientId)
        {
            if (clientId.In("000000000000000", "999999999999999"))
                return await Task.Run(() => new List<string>());

            return await Task.Run(() => new List<string>
            {
                "Invoice 1.pdf",
                "Invoice 2.pdf"
            });
        }

        public async Task TestSapConnection()
        {
            await Task.Run(() => throw new NotImplementedException());
        }
    }
}
