using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Billing.API.DopplerSecurity
{
    public class IsValidSignatureHandler<T> : AuthorizationHandler<IsSuperUserOrOwnResourceRequirement>
    {
        private const string INVOICE_FILENAME_REGEX = @"^invoice_\d{4}-\d{2}-\d{2}_(\d+)\.pdf$";

        private readonly ILogger<IsValidSignatureHandler<T>> _logger;
        private readonly CryptoHelper _cryptoHelper;


        public IsValidSignatureHandler(ILogger<IsValidSignatureHandler<T>> logger, CryptoHelper cryptoHelper)
        {
            _logger = logger;
            _cryptoHelper = cryptoHelper;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsSuperUserOrOwnResourceRequirement requirement)
        {
            if (IsValidSignature(context))
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

            if (!resource.ActionDescriptor.RouteValues.TryGetValue("action", out var action))
            {
                _logger.LogWarning("Invalid action");
                return false;
            }

            if (!action.Equals("GetInvoiceFile", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!resource.RouteData.Values.TryGetValue("origin", out var origin))
            {
                _logger.LogWarning("Cannot get the Origin from the route.");
                return false;
            }

            if (!resource.RouteData.Values.TryGetValue("clientId", out var clientId))
            {
                _logger.LogWarning("Cannot get the ClientId from the route.");
                return false;
            }

            if (!resource.RouteData.Values.TryGetValue("filename", out var filename))
            {
                _logger.LogWarning("Cannot get the Filename from the route.");
                return false;
            }

            if (!resource.HttpContext.Request.Query.TryGetValue("s", out var signature))
            {
                _logger.LogWarning("Cannot get the Signature from the query string.");
                return false;
            }

            string clientPrefix;

            switch (origin.ToString().ToLowerInvariant())
            {
                case "doppler":
                    clientPrefix = "CD";

                    break;
                case "relay":
                    clientPrefix = "CR";

                    break;
                case "clientmanager":
                    clientPrefix = "CM";

                    break;
                default:
                    clientPrefix = string.Empty;
                    break;
            }

            if (clientPrefix.IsNullOrEmpty())
                return false;

            var match = Regex.Match(filename.ToString(), INVOICE_FILENAME_REGEX);

            if (!match.Success)
                return false;

            var fileId = match.Groups[1].Value.ToInt32();

            return _cryptoHelper.GenerateInvoiceSign($"{clientPrefix}{clientId}{fileId}") == signature;
        }
    }
}
