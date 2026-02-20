using System.Security.Claims;
using BankMore.CurrentAccount.Application.Commands.Login;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BankMore.CurrentAccount.Application.Helpers;

public static class JwtHelper
{
    public static string BuildToken(string accountId, string accountNumber, JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.Secret))
            throw new InvalidOperationException("JWT Secret is not configured. Set Jwt:Secret in configuration.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("accountId", accountId),
            new Claim("accountNumber", accountNumber),
            new Claim(ClaimTypes.NameIdentifier, accountId)
        };
        var expires = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            expires: expires,
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
