using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CabriThonAPI.WebAPI.Middleware;

/// <summary>
/// Middleware to extract clientId from JWT token
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            AttachClientToContext(context, token);
        }

        await _next(context);
    }

    private void AttachClientToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Extract clientId from token claims
            var clientId = jwtToken.Claims.FirstOrDefault(x => x.Type == "clientId")?.Value
                          ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value
                          ?? jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(clientId))
            {
                context.Items["ClientId"] = clientId;
            }
        }
        catch
        {
            // Token is invalid or malformed, will be handled by authentication middleware
        }
    }
}

public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}

