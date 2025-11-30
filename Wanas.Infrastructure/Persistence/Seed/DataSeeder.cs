using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Infrastructure.Persistence.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            await context.Database.MigrateAsync(ct);

            string[] roles = ["Admin", "User"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role, NormalizedName = role.ToUpperInvariant() });
                }
            }

            // If we already have some listings with owners and renters, skip
            if (await context.Listings.AnyAsync(ct) && await context.Users.AnyAsync(ct))
                return;

            var cities = new[] { "Cairo", "Giza", "Alexandria" };
            var jobs = new[] { "Software Engineer", "Designer", "Teacher", "Student" };

            // Create4 owners with listings and6 renters
            var owners = new List<ApplicationUser>();
            var renters = new List<ApplicationUser>();

            // Owners + listings
            for (int i = 0; i < 4; i++)
            {
                var email = $"owner{i + 1}@example.com";
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = i % 2 == 0 ? $"Owner{i + 1} Ahmed" : $"Owner{i + 1} Mona",
                    City = cities[i % cities.Length],
                    Age = 25 + i,
                    IsVerified = true,
                    IsProfileCompleted = true,
                    IsPreferenceCompleted = true,
                    ProfileType = ProfileType.Owner,
                    PhoneNumber = $"+2010{(i + 100).ToString("D8")}"
                };
                await userManager.CreateAsync(user, "P@ssw0rd1");
                await userManager.AddToRoleAsync(user, "User");
                owners.Add(user);

                // Owner preferences (used for matching against renter preferences)
                var ownerPref = new UserPreference
                {
                    UserId = user.Id,
                    City = user.City!,
                    MinimumAge = 20,
                    MaximumAge = 40,
                    Gender = (i % 2 == 0) ? Gender.Male : Gender.Female,
                    MinimumBudget = 2500,
                    MaximumBudget = 5000,
                    Smoking = (i % 2 == 0) ? AllowOrNot.Allow : AllowOrNot.NotAllow,
                    Pets = (i % 2 == 0) ? AllowOrNot.NotAllow : AllowOrNot.Allow,
                    SleepSchedule = (i % 2 == 0) ? SleepSchedule.Late : SleepSchedule.Normal,
                    SocialLevel = (i % 2 == 0) ? SocialLevel.Extrovert : SocialLevel.Introvert,
                    NoiseToleranceLevel = (i % 2 == 0) ? NoiseToleranceLevel.Medium : NoiseToleranceLevel.Low,
                    Job = jobs[i % jobs.Length],
                    IsStudent = false
                };
                await context.UserPreferences.AddAsync(ownerPref, ct);

                // Listing + apartment
                var listing = new Listing
                {
                    Title = i switch
                    {
                        0 => "Quiet Apartment for Focused Professionals",
                        1 => "Social Hub in Downtown Cairo",
                        2 => "Peaceful Student Residence",
                        _ => "Tech Enthusiast's Paradise"
                    },
                    Description = i switch
                    {
                        0 => "Perfect for software engineers who need peace and quiet. High-speed internet, quiet environment.",
                        1 => "Vibrant area with nightlife. Great for extroverts and social lifestyle.",
                        2 => "Quiet neighborhood perfect for studying and focus.",
                        _ => "High-speed internet, quiet environment perfect for programmers."
                    },
                    City = user.City!,
                    MonthlyPrice = 3000 + (i * 300),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    UserId = user.Id
                };
                await context.Listings.AddAsync(listing, ct);

                var apt = new ApartmentListing
                {
                    Address = $"{user.City} Central District",
                    MonthlyPrice = listing.MonthlyPrice,
                    HasElevator = true,
                    Floor = ((user.Age ?? 21) % 10).ToString(),
                    AreaInSqMeters = 90 + i * 10,
                    TotalBathrooms = 1,
                    HasKitchen = true,
                    HasInternet = true,
                    HasAirConditioner = true,
                    // HasFans left to default; ensure DB column exists before setting explicitly
                    IsPetFriendly = ownerPref.Pets == AllowOrNot.Allow,
                    IsSmokingAllowed = ownerPref.Smoking == AllowOrNot.Allow,
                    Listing = listing
                };
                await context.ApartmentListings.AddAsync(apt, ct);
            }

            // Renters
            for (int i = 0; i < 6; i++)
            {
                var email = $"renter{i + 1}@example.com";
                var renter = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = i % 2 == 0 ? $"Renter{i + 1} Ali" : $"Renter{i + 1} Sara",
                    City = cities[i % cities.Length],
                    Age = 22 + i,
                    IsVerified = true,
                    IsProfileCompleted = true,
                    IsPreferenceCompleted = true,
                    Bio = i % 2 == 0 ? "I'm a software engineer who loves coding and needs quiet space for focus" : "Student looking for a peaceful place to study",
                    ProfileType = ProfileType.Renter,
                    PhoneNumber = $"+2010{(i + 200).ToString("D8")}"
                };
                await userManager.CreateAsync(renter, "P@ssw0rd1");
                await userManager.AddToRoleAsync(renter, "User");
                renters.Add(renter);

                var pref = new UserPreference
                {
                    UserId = renter.Id,
                    City = renter.City!,
                    MinimumAge = Math.Max(18, (renter.Age ?? 22) - 3),
                    MaximumAge = (renter.Age ?? 22) + 7,
                    Gender = (i % 2 == 0) ? Gender.Male : Gender.Female,
                    MinimumBudget = 2500,
                    MaximumBudget = 4500,
                    Smoking = AllowOrNot.NotAllow,
                    Pets = AllowOrNot.Neutral,
                    SleepSchedule = SleepSchedule.Late,
                    SocialLevel = SocialLevel.Extrovert,
                    NoiseToleranceLevel = NoiseToleranceLevel.Medium,
                    Job = jobs[i % jobs.Length],
                    IsStudent = jobs[i % jobs.Length] == "Student",
                    University = jobs[i % jobs.Length] == "Student" ? "City University" : null,
                    Major = jobs[i % jobs.Length] == "Student" ? "CS" : null
                };
                await context.UserPreferences.AddAsync(pref, ct);
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
