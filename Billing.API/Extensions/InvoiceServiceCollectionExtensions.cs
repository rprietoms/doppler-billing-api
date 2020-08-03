using Billing.API.Services.Invoice;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Billing.API.Extensions
{
    public static class InvoiceServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoice(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureInvoiceProviderOptions>();

            services.AddSingleton<DummyInvoiceProviderService>();
            services.AddTransient<InvoiceService>();

            services.AddTransient(serviceProvider =>
            {
                var invoiceProviderOptions = serviceProvider.GetRequiredService<IOptions<InvoiceProviderOptions>>();

                return invoiceProviderOptions.Value.UseDummyData
                    ? (IInvoiceService)serviceProvider.GetRequiredService<DummyInvoiceProviderService>()
                    : serviceProvider.GetRequiredService<InvoiceService>();
            });

            return services;
        }
    }
}
