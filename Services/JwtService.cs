using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LUTE_Server.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LUTE_Server.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
{
    var claims = new[]
    {
        new Claim("username", user.Username),  // Custom claim for username
        new Claim("userId", user.Id.ToString()),  // Custom claim for user ID
        new Claim("role", user.Role.ToString()),  // Custom claim for user role
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())  // Token ID
    };

    var jwtKey = _configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("JWT Key is not configured.");
    }
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        _configuration["Jwt:Issuer"],
        _configuration["Jwt:Issuer"],
        claims,
        expires: DateTime.Now.AddHours(5),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}


 // Generic method to extract a specific claim from the JWT token
        public string? GetClaimFromToken(string claimType, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract the specified claim
                var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);

                return claim?.Value ?? string.Empty;  // Return claim value if found, otherwise an empty string
            }
            catch (Exception)
            {
                // Token validation failed
                return string.Empty;
            }
        }

    }
}
