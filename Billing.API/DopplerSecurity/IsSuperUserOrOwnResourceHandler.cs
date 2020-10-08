using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Billing.API.DopplerSecurity
{
    public class IsSuperUserOrOwnResourceHandler : AuthorizationHandler<IsSuperUserOrOwnResourceRequirement>
    {
        private readonly ILogger<IsSuperUserOrOwnResourceHandler> _logger;

        public IsSuperUserOrOwnResourceHandler(ILogger<IsSuperUserOrOwnResourceHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsSuperUserOrOwnResourceRequirement requirement)
        {
            if (!IsSuperUser(context))
            {
                var tokenUserId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var resource = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;

                if (resource is null)
                {
                    _logger.LogWarning("Is not possible access to Resource information.");
                    return Task.CompletedTask;
                }

                if (!resource.RouteData.Values.TryGetValue("clientId", out var clientId) || clientId?.ToString() != tokenUserId)
                {
                    _logger.LogWarning("The IdUser into the token is different that in the route. The user hasn't permissions.");
                    return Task.CompletedTask;
                }
                // TODO: check token Issuer information, to validate right origin
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        private bool IsSuperUser(AuthorizationHandlerContext context)
        {
            if (!context.User.HasClaim(c => c.Type.Equals("isSU")))
            {
                _logger.LogDebug("The token hasn't super user permissions.");
                return false;
            }

            var isSuperUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals("isSU")).Value);
            if (isSuperUser)
            {
                return true;
            }

            _logger.LogDebug("The token super user permissions is false.");
            return false;
        }
    }
}
