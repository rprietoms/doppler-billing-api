using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Billing.API.TaxInfoProvider
{
    public class ConfigureTaxInfoProviderOptions : IConfigureOptions<TaxInfoProviderOptions>
    {
        private const string DEFAULT_CONFIGURATION_SECTION_NAME = "TaxInfoProvider";

        private readonly IConfiguration _configuration;

        public ConfigureTaxInfoProviderOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(TaxInfoProviderOptions options)
        {
            _configuration.Bind(DEFAULT_CONFIGURATION_SECTION_NAME, options);
        }
    }
}
