namespace ERP.Application.DTOs.Authentication;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FullName);
public record AuthResponse(string Token, string Email, string FullName);
