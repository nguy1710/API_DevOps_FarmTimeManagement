using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using RestfulAPI_FarmTimeManagement.Models;
using System.Text;




namespace RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom
{
    public static class TokenService
    {
        // Có thể đọc các giá trị này từ IConfiguration
        private const string Issuer = "FarmTimeManagement";
        private const string Audience = "FarmTimeManagement.Clients";
        private static readonly string Key =
            Environment.GetEnvironmentVariable("FTM_JWT_KEY")
            ?? "REPLACE_WITH_LONG_RANDOM_SECRET_32+CHARS";

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

    }
}
