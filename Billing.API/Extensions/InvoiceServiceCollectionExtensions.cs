using Billing.API.Services;
using Billing.API.TaxInfoProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Billing.API.Extensions
{
    public static class InvoiceServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoice(this IServiceCollection services)
        {
            //services.ConfigureOptions<ConfigureTaxInfoProviderOptions>();

            services.AddSingleton<DummyTaxInfoProviderService>();
            services.AddTransient<InvoiceService>();

            //services.AddTransient(serviceProvider =>
            //{
            //    var taxInfoProviderOptions = serviceProvider.GetRequiredService<IOptions<TaxInfoProviderOptions>>();
            //    return taxInfoProviderOptions.Value.UseDummyData
            //        ? (ITaxInfoProviderService)serviceProvider.GetRequiredService<DummyTaxInfoProviderService>()
            //        : serviceProvider.GetRequiredService<TaxInfoProviderService>();
            //});

            return services;
        }
    }
}
