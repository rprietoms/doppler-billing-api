using Microsoft.AspNetCore.Authorization;

namespace Billing.API.DopplerSecurity
{
    public class IsSuperUserRequirement : IAuthorizationRequirement
    {
    }
}
