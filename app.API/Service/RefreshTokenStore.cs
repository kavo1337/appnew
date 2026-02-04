using System.Collections.Concurrent;

namespace app.API.Service
{
	public class RefreshTokenStore
	{
		private readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens = new();
		public void Store(string token, RefreshTokenEntry entry)
		{
			_tokens[token] = entry; 
		}
		public bool TryGet(string token, out RefreshTokenEntry entry)
		{
			if (_tokens.TryGetValue(token, out entry))
			{
				if (entry.ExpiresAt > DateTime.UtcNow)
				{
					return true;
				}
				_tokens.TryRemove(token, out _);
			}
			entry = default;
			return false;
		}
		public void Remove(string token)
		{
			_tokens.TryRemove(token, out _);
		}
	}
	public record struct RefreshTokenEntry(int UserId, DateTime ExpiresAt);
}
