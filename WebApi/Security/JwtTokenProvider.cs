using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.Security
{
    public class JwtTokenProvider
    {
        public static string CreateToken(string secureKey, int expirationMinutes, string? subject = null, string? role = null)
        {
            var tokenKey = Encoding.UTF8.GetBytes(secureKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            if (!string.IsNullOrEmpty(subject))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, subject),
                    new Claim(JwtRegisteredClaimNames.Sub, subject)
                };

                if (!string.IsNullOrWhiteSpace(role))
                    claims.Add(new Claim(ClaimTypes.Role, role));

                tokenDescriptor.Subject = new ClaimsIdentity(claims);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
