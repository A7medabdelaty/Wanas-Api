using Wanas.Domain.Entities;

namespace Wanas.Domain.Events
{
    internal class MessageSentEvent : IDomainEvent
    {
        public MessageSentEvent(Message message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            OccurredOn = DateTime.UtcNow;
        }

        public Message Message { get; }
        public DateTime OccurredOn { get; }
    }
}
