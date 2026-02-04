using app.API.Data.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace app.API.Service
{
	public class JwtService
	{
		private readonly IConfiguration _config;
		public JwtService(IConfiguration config)
		{
			_config = config;
		}
		public string CreateAccessToken(UserAccount user)
		{
			var Issuer = _config["Jwt:Issuer"] ?? "app";
			var Audience = _config["Jwt:Audience"] ?? "app";
			var SecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"] ?? "__REPLACE__ME__PLEASE__FOR__PRODUCT__AG56R7R5RR65RVFT5R656R__"));
			var Expires = GetAccessTokenMinutes();
			var Claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, user.UserAccountId.ToString()),
				new(JwtRegisteredClaimNames.Name, user.Email.ToString()),
				new(ClaimTypes.Name, user.UserAccountId.ToString()),
				new(ClaimTypes.NameIdentifier, BuildFullName(user)),
				new(ClaimTypes.Role, user.UserRole?.Name ?? string.Empty)
			};
			var Creds = new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha256);
			var Token = new JwtSecurityToken(
				issuer: Issuer,
				audience: Audience,
				claims: Claims,
				signingCredentials: Creds,
				expires: DateTime.UtcNow.AddMinutes(Expires));
			return new JwtSecurityTokenHandler().WriteToken(Token);
		}

		public string BuildFullName(UserAccount user)
		{
			var parts = new[] { user.FirstName, user.LastName, user.Patronymic };
			return string.Join(" ", parts.Where(opt => !string.IsNullOrEmpty(opt)));
		}
		public string CreateRefreshToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		}

		public DateTime GetRefreshTokenExpiry()
		{
			var days = 30;
			if (int.TryParse(_config["Jwt:RefreshTokenDays"], out var parsed))
			{
				days = parsed;
			}

			return DateTime.UtcNow.AddDays(days);
		}

		private int GetAccessTokenMinutes()
		{
			if (int.TryParse(_config["Jwt:AccessTokenMinutes"], out var minutes))
			{
				return minutes;
			}

			return 15;
		}
	}
}
