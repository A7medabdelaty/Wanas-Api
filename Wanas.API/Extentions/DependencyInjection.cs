using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wanas.API.RealTime;
using Wanas.Application.Handlers.Admin;
using Wanas.Application.Interfaces;
using Wanas.Application.Mappings;
using Wanas.Application.Services;
using Wanas.Application.Validators;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;
using Wanas.Infrastructure.Repositories;

namespace Wanas.API.Extentions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Identity Configuration
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDBContext>()
            .AddDefaultTokenProviders();

            // Repositories - Register all repositories needed by UnitOfWork
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IAppealRepository, AppealRepository>();

            // Unit of Work
            services.AddScoped<AppDbContext, UnitOfWork>();

            // Services (Application Layer)
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();
            services.AddScoped<IListingSearchService, ListingSearchService>();

            #region RAG DEPENDENCIES

            // 1. HTTP Clients
            services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddHttpClient<IChromaService, ChromaService>();

            // 2. Configuration
            services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));

            // 3. Service Registrations
            services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddScoped<IChromaService, ChromaService>();

            // 4. Keep original matching service as concrete implementation
            services.AddScoped<MatchingService>();

            #endregion

            #region MATCHING SERVICE SELECTION

            // Choose one of the following IMatchingService implementations:
            
            // Option A: Use StaticTestMatchingService (current default)
            services.AddScoped<IMatchingService, StaticTestMatchingService>();

            // Option B: Use basic MatchingService
            // services.AddScoped<IMatchingService, MatchingService>();

            // Option C: Use HybridMatchingService (RAG + Traditional)
            // services.AddScoped<IMatchingService>(provider =>
            // {
            //     var traditional = provider.GetRequiredService<MatchingService>();
            //     var chroma = provider.GetRequiredService<IChromaService>();
            //     var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
            //     return new HybridMatchingService(traditional, chroma, unitOfWork);
            // });

            #endregion



            // FluentValidation registration if used
            services.AddValidatorsFromAssemblyContaining<SuspendUserCommandValidator>();


            // MediatR handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SuspendUserCommandHandler>());

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ListingProfile>();
            }, typeof(ListingProfile).Assembly);

            return services;
        }
    }
}
