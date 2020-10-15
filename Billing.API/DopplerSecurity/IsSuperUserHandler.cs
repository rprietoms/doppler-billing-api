using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Billing.API.DopplerSecurity
{
    public class IsSuperUserHandler<T> : AuthorizationHandler<T>
        where T: IAuthorizationRequirement
    {
        private readonly ILogger<IsSuperUserHandler<T>> _logger;

        public IsSuperUserHandler(ILogger<IsSuperUserHandler<T>> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
        {
            if (IsSuperUser(context))
            {
                context.Succeed(requirement);
            }

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
