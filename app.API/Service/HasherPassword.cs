using app.API.Data.Entities;
using System.Security.Cryptography;
using System.Text;

namespace app.API.Service
{
	public class HasherPassword
	{
		public static string CreateHashPassword(string password, string salt)
		{
			var saltBytes = GetSaltBytes(salt);
			var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
				Encoding.UTF8.GetBytes(password),
				saltBytes,
				100000,
				HashAlgorithmName.SHA256,
				32);
			return Convert.ToBase64String(hashBytes);
		}
		public static bool VerifyPassword(string password, string hash, string salt)
		{
			if (string.IsNullOrWhiteSpace(hash))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(salt))
			{
				if (SlowEqual(password, hash))
				{
					return true;
				}
				var simpleHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
				return SlowEqual(simpleHash, hash);
			}
			var computed = CreateHashPassword(password, salt);
			return SlowEqual(computed, hash);
		}
		private static bool SlowEqual(string a, string b)
		{
			var aBytes = Encoding.UTF8.GetBytes(a);
			var bBytes = Encoding.UTF8.GetBytes(b);
			var diff = aBytes.Length ^ bBytes.Length;
			var lenght = Math.Min(aBytes.Length, bBytes.Length);
			for (int i = 0; i < lenght; i++)
			{
				diff |= aBytes[i] ^ bBytes[i];
			}
			return diff == 0;
		}
		private static byte[] GetSaltBytes(string salt)
		{
			try
			{
				return Convert.FromBase64String(salt);
			}
			catch
			{
				return Encoding.UTF8.GetBytes(salt);
			}
		}
	}
}
