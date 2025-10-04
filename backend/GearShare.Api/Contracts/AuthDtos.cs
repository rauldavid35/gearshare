namespace GearShare.Api.Contracts
{
    public record RegisterRequest(
        string Email,
        string Password,
        string DisplayName,
        string Role
    );

    public record LoginRequest(
        string Email,
        string Password
    );

    public record UserDto(
        string Id,
        string Email,
        string? DisplayName,
        string[] Roles
    );

    public record AuthResponse(
        string Token,
        DateTime ExpiresAt,
        UserDto User
    );
}
