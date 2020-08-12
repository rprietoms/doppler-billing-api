using System.Collections.Generic;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceListItem>> GetInvoices(string clientId);

        Task TestSapConnection();
    }
}
