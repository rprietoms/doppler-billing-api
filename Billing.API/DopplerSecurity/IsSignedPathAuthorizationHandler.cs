using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Billing.API.DopplerSecurity
{
    public class IsSignedPathAuthorizationHandler : AuthorizationHandler<DopplerAuthorizationRequirement>
    {
        private readonly ILogger<IsSignedPathAuthorizationHandler> _logger;
        private readonly CryptoHelper _cryptoHelper;

        public IsSignedPathAuthorizationHandler(ILogger<IsSignedPathAuthorizationHandler> logger, CryptoHelper cryptoHelper)
        {
            _logger = logger;
            _cryptoHelper = cryptoHelper;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DopplerAuthorizationRequirement requirement)
        {
            if (requirement.AllowSignedPaths && IsValidSignature(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsValidSignature(AuthorizationHandlerContext context)
        {
            if (!(context.Resource is AuthorizationFilterContext resource))
            {
                _logger.LogWarning("Is not possible access to Resource information.");
                return false;
            }

            if (!resource.HttpContext.Request.Query.TryGetValue("_s", out var signature))
            {
                _logger.LogWarning("Cannot get the Signature from the query string.");
                return false;
            }

            return _cryptoHelper.GenerateSignature(resource.HttpContext.Request.Path) == signature;
        }
    }
}
