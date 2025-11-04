using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CabriThonAPI.WebAPI.Controllers;

[Authorize]
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected string GetClientId()
    {
        // Try to get from HttpContext Items (set by middleware)
        if (HttpContext.Items.TryGetValue("ClientId", out var clientId) && clientId != null)
        {
            return clientId.ToString()!;
        }

        // Fallback: try to get from claims
        var claim = User.FindFirst("clientId") 
                    ?? User.FindFirst("sub") 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (claim != null)
        {
            return claim.Value;
        }

        throw new UnauthorizedAccessException("ClientId not found in token");
    }
}

