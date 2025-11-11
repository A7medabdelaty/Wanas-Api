using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Preference
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public int MinimunBudget { get; set; }
        public int MaximunBudget { get; set; }
        public AllowOrNot Children { get; set; }
        public AllowOrNot AllowVisits { get; set; }
        public string? Job { get; set; }
        public bool? IsStudent { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public AllowOrNot SmokingPreference { get; set; }
        public AllowOrNot PetsPreference { get; set; }
        public SleepSchedule SleepSchedule { get; set; }
        public SocialLevel SocialLevel { get; set; }
        public NoiseToleranceLevel NoiseToleranceLevel { get; set; }

        // public int CleanlinessLevel { get; set; }

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
