namespace app.API.Contracts;

public record struct LoginRequest(string email, string password);
public sealed record LoginResponse(string AccessToken, string RefreshToken, UserProfile User);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record UserProfile(int Id, string Email, string FullName, string Role);
