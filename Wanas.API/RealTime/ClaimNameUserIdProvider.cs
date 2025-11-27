using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Wanas.API.RealTime
{
    // Ensures SignalR's Context.UserIdentifier is set from ClaimTypes.NameIdentifier
    public class ClaimNameUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
