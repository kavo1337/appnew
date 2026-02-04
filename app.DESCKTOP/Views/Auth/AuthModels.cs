using System;
using System.Collections.Generic;
using System.Text;

namespace app.DESCKTOP.Views.Auth;

public sealed record LoginRequest(string email, string password);
public sealed record LoginResponse(string AccessToken, string RefreshToken, UserProfile User);

public sealed record UserProfile(int Id, string Email, string FullName, string Role, string? PhotoUrl);

public sealed record Session()
{
	public static string AccessToken { get; set; }
	public static string RefreshToken { get; set; }
	public static UserProfile User { get; set; }
	public static string? PhotoUrl { get; set; }

	public static string? ApiBaseUrl { get; set; }
	public static Uri GetApiBaseUri()
	{
		if (Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out var uri))
		{
			return uri;
		}

		ApiBaseUrl = "https://localhost:7062/";
		return new Uri(ApiBaseUrl);
	}

	public static string GetApiUrl(string relative)
	{
		var baseUri = GetApiBaseUri();
		return new Uri(baseUri, relative).ToString();
	}
}