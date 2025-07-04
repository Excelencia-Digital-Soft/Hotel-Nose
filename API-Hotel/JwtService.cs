using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace hotel.Auth
{
    public class JwtService
    {
        private readonly string _key;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _key = _configuration["Jwt:Key"]!; // Retrieve the JWT key from configuration
        }

        public string GenerateToken(string userId, string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Name, username),
                    ]
                ),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["Jwt:Issuer"], // Ensure issuer matches appsettings.json
                Audience = _configuration["Jwt:Audience"], // Ensure audience matches appsettings.json
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new("unique_name", user.UserName ?? user.Email!),
                new("email", user.Email!),
                new("FirstName", user.FirstName ?? ""),
                new("LastName", user.LastName ?? ""),
            };

            // Add institution claim if available
            if (user.InstitucionId.HasValue)
            {
                claims.Add(new Claim("InstitucionId", user.InstitucionId.Value.ToString()));
            }

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1), // Token expiration
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                var principal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out SecurityToken validatedToken
                );
                return principal;
            }
            catch
            {
                return null!;
            }
        }
    }
}

