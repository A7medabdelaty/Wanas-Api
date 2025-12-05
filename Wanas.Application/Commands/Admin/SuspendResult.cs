using MediatR;


namespace Wanas.Application.Commands.Admin
{
    public record SuspendResult(
    bool Success,
    bool AlreadySuspended,
    DateTime? SuspendedUntil
);
}