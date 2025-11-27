namespace Wanas.Application.DTOs.Matching
{
    public class MatchBreakdown
    {
        public bool CityMatch { get; set; }
        public bool AgeCompatible { get; set; }
        public bool BudgetCompatible { get; set; }
        public bool GenderMatch { get; set; }
        public bool SmokingCompatible { get; set; }
        public bool PetsCompatible { get; set; }
        public bool SleepScheduleMatch { get; set; }
        public bool SocialLevelMatch { get; set; }
        public bool NoiseToleranceMatch { get; set; }
    }

    public class RoommateMatchDto
    {
        public string TargetUserId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? City { get; set; }
        public int? Age { get; set; }
        public string? Photo { get; set; }
        public int Score { get; set; }
        public int Percentage { get; set; }
        public string Gender { get; set; } = string.Empty;
        public MatchBreakdown Breakdown { get; set; } = new MatchBreakdown();
    }
}
