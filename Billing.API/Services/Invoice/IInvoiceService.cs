using System.Collections.Generic;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public interface IInvoiceService
    {
        Task<PaginatedResult<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId, int page, int pageSize);

        Task<InvoiceFile?> GetInvoiceFile(string clientPrefix, int clientId, int fileId);

        Task<string> TestSapConnection();
    }
}
