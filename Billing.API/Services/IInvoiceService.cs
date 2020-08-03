using System.Collections.Generic;
using System.Threading.Tasks;

namespace Billing.API.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<string>> GetInvoices(string clientId);

        Task TestSapConnection();
    }
}
