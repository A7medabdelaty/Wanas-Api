namespace Wanas.Application.Commands.Admin
{
    public record BanResult(
        bool Success,
        bool AlreadyBanned,
        string? Message
    );
}
