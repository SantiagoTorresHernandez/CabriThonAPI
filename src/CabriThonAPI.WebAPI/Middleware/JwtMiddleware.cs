using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CabriThonAPI.WebAPI.Middleware;

/// <summary>
/// Middleware to extract clientId from JWT token (Supabase Auth)
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

            // Extract user info from Supabase token
            // Supabase tokens have these claims:
            // - "sub": user_id (UUID from auth.users)
            // - "email": user's email
            // - "role": user's role (usually "authenticated")
            
            // Option 1: Use sub (Supabase user ID) directly as clientId
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            
            // Option 2: Check for custom clientId claim (if you add it in Supabase)
            var clientId = jwtToken.Claims.FirstOrDefault(x => x.Type == "clientId")?.Value
                          ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "client_id")?.Value
                          ?? userId;

            var email = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
            var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;

            if (!string.IsNullOrEmpty(clientId))
            {
                context.Items["ClientId"] = clientId;
                context.Items["SupabaseUserId"] = userId;
                context.Items["UserEmail"] = email;
                context.Items["UserRole"] = role;
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

