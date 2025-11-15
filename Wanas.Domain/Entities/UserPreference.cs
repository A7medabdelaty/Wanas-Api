using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class UserPreference
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int MinimumAge { get; set; }
        public int MaximumAge { get; set; }
        public Gender Gender { get; set; }
        public int MinimumBudget { get; set; }
        public int MaximumBudget { get; set; }
        public AllowOrNot Children { get; set; }
        public AllowOrNot Visits { get; set; }
        public AllowOrNot OvernightGuests { get; set; }
        public string? Job { get; set; }
        public bool? IsStudent { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public AllowOrNot Smoking { get; set; }
        public AllowOrNot Pets { get; set; }
        public SleepSchedule SleepSchedule { get; set; }
        public SocialLevel SocialLevel { get; set; }
        public NoiseToleranceLevel NoiseToleranceLevel { get; set; }

        // public int CleanlinessLevel { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
