using Billing.API.Services.Invoice;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Billing.API.Extensions
{
    public static class InvoiceServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoiceService(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureInvoiceProviderOptions>();

            services.AddSingleton<DummyInvoiceService>();
            services.AddTransient<InvoiceService>();

            services.AddTransient(serviceProvider =>
            {
                var invoiceProviderOptions = serviceProvider.GetRequiredService<IOptions<InvoiceProviderOptions>>();

                return invoiceProviderOptions.Value.UseDummyData
                    ? (IInvoiceService)serviceProvider.GetRequiredService<DummyInvoiceService>()
                    : serviceProvider.GetRequiredService<InvoiceService>();
            });

            return services;
        }
    }
}
