namespace Taskly.Api.Auth;

public record RegisterRequest(string Username, string Password, int? WorkspaceId); // default to 1 if null
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token);
