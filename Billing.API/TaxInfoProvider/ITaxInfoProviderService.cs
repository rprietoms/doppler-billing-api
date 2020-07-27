using System.Threading.Tasks;

namespace Billing.API.TaxInfoProvider
{
    public interface ITaxInfoProviderService
    {
        Task<TaxInfo> GetTaxInfoByCuit(CuitNumber cuit);
    }
}
