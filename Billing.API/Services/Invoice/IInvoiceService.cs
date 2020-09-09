using System.Collections.Generic;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId);

        Task<byte[]?> GetInvoiceFile(string clientPrefix, int clientId, int fileId);

        Task<string> TestSapConnection();
    }
}
