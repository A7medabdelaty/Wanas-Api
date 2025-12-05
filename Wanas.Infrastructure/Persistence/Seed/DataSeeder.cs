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

            // If listings already exist, ensure they have a first photo seeded
            if (await context.Listings.AnyAsync(ct))
            {
                var imagePaths = new[]
                {
                    "images/listings/1.jpg",
                    "images/listings/2.jpg",
                    "images/listings/3.jpg",
                    "images/listings/4.jpg"
                };

                var listings = await context.Listings
                    .Include(l => l.ListingPhotos)
                    .OrderBy(l => l.Id)
                    .Take(imagePaths.Length)
                    .ToListAsync(ct);

                for (int i =0; i < listings.Count; i++)
                {
                    var listing = listings[i];
                    // Only add a first photo if none exists
                    if (listing.ListingPhotos == null || listing.ListingPhotos.Count ==0)
                    {
                        listing.ListingPhotos = listing.ListingPhotos ?? new HashSet<ListingPhoto>();
                        listing.ListingPhotos.Add(new ListingPhoto
                        {
                            URL = imagePaths[i],
                            ListingId = listing.Id
                        });
                    }
                }

                await context.SaveChangesAsync(ct);

                // Ensure some basic reports exist even when listings already existed
                if (!await context.Reports.AnyAsync(ct))
                {
                    var users = await context.Users.OrderBy(u => u.CreatedAt).Take(6).ToListAsync(ct);
                    var anyListing = listings.FirstOrDefault();
                    if (users.Count >0)
                    {
                        var seededReports = new List<Report>();
                        var categories = new[] { ReportCategory.Harassment, ReportCategory.Spam, ReportCategory.SensitiveContent, ReportCategory.Other };
                        var severities = new[] { ReportSeverity.Low, ReportSeverity.Medium, ReportSeverity.High };

                        // Create10 reports alternating target types
                        for (int i =0; i <10; i++)
                        {
                            var reporter = users[i % users.Count];
                            var isUserTarget = i %2 ==0;
                            var category = categories[i % categories.Length];
                            var severity = severities[i % severities.Length];

                            var report = new Report
                            {
                                TargetType = isUserTarget ? ReportTarget.User : ReportTarget.Listing,
                                TargetId = isUserTarget
                                ? users[(i +1) % users.Count].Id // pick a different user as target
                                : (anyListing != null ? anyListing.Id.ToString() : users[(i +1) % users.Count].Id),
                                Category = category,
                                Reason = isUserTarget ? "Reported user behavior." : "Reported listing content.",
                                Status = ReportStatus.Pending,
                                CreatedAt = DateTime.UtcNow.AddMinutes(-60 + i *3),
                                ReporterId = reporter.Id,
                                IsEscalated = false,
                                Severity = severity
                            };
                            seededReports.Add(report);
                        }

                        await context.Reports.AddRangeAsync(seededReports, ct);
                        await context.SaveChangesAsync(ct);

                        // Optionally add photos to first few reports
                        var createdReports = await context.Reports
                            .OrderBy(r => r.ReportId)
                            .Take(3)
                            .ToListAsync(ct);

                        var photoSeed = new List<ReportPhoto>();
                        for (int i =0; i < createdReports.Count; i++)
                        {
                            photoSeed.Add(new ReportPhoto
                            {
                                ReportId = createdReports[i].ReportId,
                                URL = $"images/reports/evidence{i +1}.jpg"
                            });
                        }
                        if (photoSeed.Count >0)
                        {
                            await context.ReportPhotos.AddRangeAsync(photoSeed, ct);
                            await context.SaveChangesAsync(ct);
                        }
                    }
                }

                return; // Skip full seed to avoid duplicating users/listings
            }

            var cities = new[] { "Cairo", "Giza", "Alexandria" };
            var jobs = new[] { "Software Engineer", "Designer", "Teacher", "Student" };

            var owners = new List<ApplicationUser>();
            var renters = new List<ApplicationUser>();

            // Owners + listings
            for (int i =0; i <4; i++)
            {
                var email = $"owner{i +1}@example.com";
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = i %2 ==0 ? $"Owner{i +1} Ahmed" : $"Owner{i +1} Mona",
                    City = cities[i % cities.Length],
                    Age =25 + i,
                    IsVerified = true,
                    IsProfileCompleted = true,
                    IsPreferenceCompleted = true,
                    ProfileType = ProfileType.Owner,
                    PhoneNumber = $"+2010{(i +100).ToString("D8")}"
                };
                await userManager.CreateAsync(user, "P@ssw0rd1");
                await userManager.AddToRoleAsync(user, "User");
                owners.Add(user);

                var ownerPref = new UserPreference
                {
                    UserId = user.Id,
                    City = user.City!,
                    MinimumAge =20,
                    MaximumAge =40,
                    Gender = (i %2 ==0) ? Gender.Male : Gender.Female,
                    MinimumBudget =2500,
                    MaximumBudget =5000,
                    Smoking = (i %2 ==0) ? AllowOrNot.Allow : AllowOrNot.NotAllow,
                    Pets = (i %2 ==0) ? AllowOrNot.NotAllow : AllowOrNot.Allow,
                    SleepSchedule = (i %2 ==0) ? SleepSchedule.Late : SleepSchedule.Normal,
                    SocialLevel = (i %2 ==0) ? SocialLevel.Extrovert : SocialLevel.Introvert,
                    NoiseToleranceLevel = (i %2 ==0) ? NoiseToleranceLevel.Medium : NoiseToleranceLevel.Low,
                    Job = jobs[i % jobs.Length],
                    IsStudent = false
                };
                await context.UserPreferences.AddAsync(ownerPref, ct);

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
                    MonthlyPrice =3000 + (i *300),
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
                    Floor = ((user.Age ??21) %10),
                    AreaInSqMeters =90 + i *10,
                    TotalBathrooms =1,
                    HasKitchen = true,
                    HasInternet = true,
                    HasAirConditioner = true,
                    IsPetFriendly = ownerPref.Pets == AllowOrNot.Allow,
                    IsSmokingAllowed = ownerPref.Smoking == AllowOrNot.Allow,
                    Listing = listing
                };
                await context.ApartmentListings.AddAsync(apt, ct);

                // Seed first photo for the newly created listing
                var initialPhoto = new ListingPhoto
                {
                    Listing = listing,
                    URL = $"images/listings/{i +1}.jpg"
                };
                await context.ListingPhotos.AddAsync(initialPhoto, ct);
            }

            // Renters
            for (int i =0; i <6; i++)
            {
                var email = $"renter{i +1}@example.com";
                var renter = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = i %2 ==0 ? $"Renter{i +1} Ali" : $"Renter{i +1} Sara",
                    City = cities[i % cities.Length],
                    Age =22 + i,
                    IsVerified = true,
                    IsProfileCompleted = true,
                    IsPreferenceCompleted = true,
                    Bio = i %2 ==0 ? "I'm a software engineer who loves coding and needs quiet space for focus" : "Student looking for a peaceful place to study",
                    ProfileType = ProfileType.Renter,
                    PhoneNumber = $"+2010{(i +200).ToString("D8")}"
                };
                await userManager.CreateAsync(renter, "P@ssw0rd1");
                await userManager.AddToRoleAsync(renter, "User");
                renters.Add(renter);

                var pref = new UserPreference
                {
                    UserId = renter.Id,
                    City = renter.City!,
                    MinimumAge = Math.Max(18, (renter.Age ??22) -3),
                    MaximumAge = (renter.Age ??22) +7,
                    Gender = (i %2 ==0) ? Gender.Male : Gender.Female,
                    MinimumBudget =2500,
                    MaximumBudget =4500,
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

            // Seed initial reports linked to renters/owners
            if (!await context.Reports.AnyAsync(ct))
            {
                var users = renters.Concat(owners).ToList();
                var firstListing = await context.Listings.OrderBy(l => l.Id).FirstOrDefaultAsync(ct);

                var reports = new List<Report>();
                var categories = new[] { ReportCategory.Harassment, ReportCategory.Spam, ReportCategory.SensitiveContent, ReportCategory.Other };
                var severities = new[] { ReportSeverity.Low, ReportSeverity.Medium, ReportSeverity.High };

                for (int i =0; i <10; i++)
                {
                    var reporter = users[i % users.Count];
                    var isUserTarget = i %2 ==0;
                    var category = categories[i % categories.Length];
                    var severity = severities[i % severities.Length];

                    var targetId = isUserTarget
                    ? users[(i +1) % users.Count].Id
                    : (firstListing != null ? firstListing.Id.ToString() : users[(i +1) % users.Count].Id);

                    reports.Add(new Report
                    {
                        TargetType = isUserTarget ? ReportTarget.User : ReportTarget.Listing,
                        TargetId = targetId,
                        Category = category,
                        Reason = isUserTarget ? "Owner/Renter behavior reported." : "Listing content reported.",
                        Status = ReportStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-120 + i *5),
                        ReporterId = reporter.Id,
                        IsEscalated = false,
                        Severity = severity
                    });
                }

                await context.Reports.AddRangeAsync(reports, ct);
                await context.SaveChangesAsync(ct);

                // Add photos to first few reports (optional)
                var photoEntries = new List<ReportPhoto>();
                for (int i =0; i < Math.Min(4, reports.Count); i++)
                {
                    photoEntries.Add(new ReportPhoto
                    {
                        ReportId = reports[i].ReportId,
                        URL = $"images/reports/sample-evidence-{i +1}.jpg"
                    });
                }
                if (photoEntries.Count >0)
                {
                    await context.ReportPhotos.AddRangeAsync(photoEntries, ct);
                    await context.SaveChangesAsync(ct);
                }
            }
        }
    }
}
