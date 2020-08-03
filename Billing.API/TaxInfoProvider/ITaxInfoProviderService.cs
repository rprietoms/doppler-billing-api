using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.TaxInfoProvider
{
    public interface ITaxInfoProviderService
    {
        Task<TaxInfo> GetTaxInfoByCuit(CuitNumber cuit);
    }
}
