using Microsoft.AspNetCore.Authorization;

namespace Billing.API.DopplerSecurity
{
    public class IsSuperUserOrOwnResourceOrValidSignatureRequirement : IAuthorizationRequirement
    {
    }
}
