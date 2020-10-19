using Billing.API.DopplerSecurity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DopplerSecurityServiceCollectionExtensions
    {
        private const string TOKEN_SIGNATURE_POLICY = "TokenSignature";

        public static IServiceCollection AddDopplerSecurity(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, IsOwnResourceHandler<IsSuperUserOrOwnResourceRequirement>>();
            services.AddSingleton<IAuthorizationHandler, IsSuperUserHandler<IsSuperUserOrOwnResourceRequirement>>();

            services.AddSingleton<IAuthorizationHandler, IsOwnResourceHandler<IsSuperUserOrOwnResourceOrValidSignatureRequirement>>();
            services.AddSingleton<IAuthorizationHandler, IsSuperUserHandler<IsSuperUserOrOwnResourceOrValidSignatureRequirement>>();
            services.AddSingleton<IAuthorizationHandler, IsValidSignatureHandler<IsSuperUserOrOwnResourceOrValidSignatureRequirement>>();

            services.ConfigureOptions<ConfigureDopplerSecurityOptions>();

            services
                .AddOptions<AuthorizationOptions>()
                .Configure(o =>
                {
                    o.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .AddRequirements(new IsSuperUserOrOwnResourceRequirement())
                        .RequireAuthenticatedUser()
                        .Build();

                    o.AddPolicy(TOKEN_SIGNATURE_POLICY, new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) //TODO: Check which scheme to use
                        .AddRequirements(new IsSuperUserOrOwnResourceOrValidSignatureRequirement())
                        .Build());
                });

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<DopplerSecurityOptions>>((o, securityOptions) =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKeys = securityOptions.Value.SigningKeys,
                        ValidateIssuer = false,
                        ValidateLifetime = !securityOptions.Value.SkipLifetimeValidation,
                        ValidateAudience = false,
                    };
                });

            services.AddAuthentication().AddJwtBearer();

            services.AddAuthorization();

            return services;
        }
    }
}
