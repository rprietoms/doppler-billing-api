using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Billing.API.TaxInfoProvider
{
    public static class TaxInfoProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddTaxInfoProvider(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureTaxInfoProviderOptions>();

            services.AddSingleton<DummyTaxInfoProviderService>();
            services.AddTransient<TaxInfoProviderService>();

            services.AddTransient(serviceProvider =>
            {
                var taxInfoProviderOptions = serviceProvider.GetRequiredService<IOptions<TaxInfoProviderOptions>>();
                return taxInfoProviderOptions.Value.UseDummyData
                    ? (ITaxInfoProviderService)serviceProvider.GetRequiredService<DummyTaxInfoProviderService>()
                    : serviceProvider.GetRequiredService<TaxInfoProviderService>();
            });

            return services;
        }
    }
}
