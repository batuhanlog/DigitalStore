using System.Security.Claims;

namespace DigitalStore.Service.Infrastructure
{
    public class JwtReader
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return 0;
            }

            var claim = identity.FindFirst(ClaimTypes.NameIdentifier) ?? identity.FindFirst("id");
            if (claim == null)
            {
                return 0;
            }

            if (int.TryParse(claim.Value, out int id))
            {
                return id;
            }

            return 0;
        }

        public static string GetUserRole(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return string.Empty;
            }

            var claim = identity.FindFirst(ClaimTypes.Role);
            return claim?.Value ?? string.Empty;
        }

        public static Dictionary<string, string> GetUserClaims(ClaimsPrincipal user)
        {
            var claimsDictionary = new Dictionary<string, string>();

            var identity = user.Identity as ClaimsIdentity;
            if (identity != null)
            {
                foreach (var claim in identity.Claims)
                {
                    claimsDictionary[claim.Type] = claim.Value;
                }
            }

            return claimsDictionary;
        }
    }
}
