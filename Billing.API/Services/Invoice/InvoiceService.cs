using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Billing.API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.API.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IOptions<InvoiceProviderOptions> _options;

        public InvoiceService(ILogger<InvoiceService> logger, IOptions<InvoiceProviderOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        public Task<IEnumerable<InvoiceListItem>> GetInvoices(string clientId)
        {
            throw new NotImplementedException();
        }

        public async Task TestSapConnection()
        {
            await Task.Run(() => throw new NotImplementedException());
        }
    }
}
