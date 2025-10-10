using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Mo_Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static long? GetUserId(this ClaimsPrincipal user)
        {
            // Thử các cách lấy user ID từ claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                             user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                             user.FindFirst("userId")?.Value;
            
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            
            return null;
        }

        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? 
                   user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        }

        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? 
                   user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        }

        public static List<string> GetRoles(this ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
    }
}
