using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using RestfulAPI_FarmTimeManagement.Models;
using System.Text;




namespace RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom
{
    public static class TokenService
    {
        // =================================================================
        // Bug Fix: Logout User Role Change Security Vulnerability
        // Developer: Tim
        // Date: 2025-09-21
        // Description: Add token invalidation when user role is changed
        // Issue: Users retain admin privileges after role downgrade
        // =================================================================

        // Có thể đọc các giá trị này từ IConfiguration
        private const string Issuer = "FarmTimeManagement";
        private const string Audience = "FarmTimeManagement.Clients";
        private static readonly string Key =
            Environment.GetEnvironmentVariable("FTM_JWT_KEY")
            ?? "REPLACE_WITH_LONG_RANDOM_SECRET_32+CHARS";

        // =================================================================
        // START: Token Invalidation Mechanism
        // Purpose: Store invalidated tokens when user roles are changed
        // =================================================================
        private static readonly HashSet<string> _invalidatedTokens = new HashSet<string>();

        public static string Generate(Staff staff, TimeSpan? lifetime = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, staff.Email ?? $"{staff.StaffId}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("staff_id", staff.StaffId.ToString()),
                new Claim(ClaimTypes.Name, $"{staff.FirstName} {staff.LastName}"),
                new Claim(ClaimTypes.Email, staff.Email ?? string.Empty),
                // Role để dùng [Authorize(Roles="Admin")]
                new Claim(ClaimTypes.Role, staff.Role ?? "Staff")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(8)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // =================================================================
        // Token Invalidation Methods
        // Purpose: Provide mechanism to invalidate tokens when role changes
        // =================================================================

        /// <summary>
        /// Invalidate a specific token (makes it unusable)
        /// Used when user role is changed
        /// </summary>
        /// <param name="token">JWT token to invalidate</param>
        public static void InvalidateToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _invalidatedTokens.Add(token);
            }
        }

        /// <summary>
        /// Check if a token is still valid (not invalidated)
        /// </summary>
        /// <param name="token">JWT token to check</param>
        /// <returns>True if token is valid, false if invalidated</returns>
        public static bool IsTokenValid(string token)
        {
            return !_invalidatedTokens.Contains(token);
        }

        /// <summary>
        /// Extract staff ID from JWT token for identification
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Staff ID if found, null otherwise</returns>
        public static int? GetStaffIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var staffIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "staff_id")?.Value;
                return int.TryParse(staffIdClaim, out int id) ? id : null;
            }
            catch
            {
                return null;
            }
        }

        // =================================================================
        // END: Token Invalidation Methods
        // =================================================================

    }
}
