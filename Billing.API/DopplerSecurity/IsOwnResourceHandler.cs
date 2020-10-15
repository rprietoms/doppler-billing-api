using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Billing.API.DopplerSecurity
{
    public class IsOwnResourceHandler<T> : AuthorizationHandler<T>
        where T: IAuthorizationRequirement
    {
        private readonly ILogger<IsOwnResourceHandler<T>> _logger;

        public IsOwnResourceHandler(ILogger<IsOwnResourceHandler<T>> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
        {
            if (IsOwnResource(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsOwnResource(AuthorizationHandlerContext context)
        {
            var tokenUserId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var resource = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;

            if (resource is null)
            {
                _logger.LogWarning("Is not possible access to Resource information.");
                return false;
            }

            if (!resource.RouteData.Values.TryGetValue("clientId", out var clientId) || clientId?.ToString() != tokenUserId)
            {
                _logger.LogWarning("The IdUser into the token is different that in the route. The user hasn't permissions.");
                return false;
            }
            // TODO: check token Issuer information, to validate right origin

            return true;
        }
    }
}
