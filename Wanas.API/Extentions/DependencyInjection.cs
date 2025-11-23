using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wanas.API.Authorization;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using Wanas.API.RealTime;
using Wanas.Application.Handlers.Admin;
using Wanas.Application.Interfaces;
using Wanas.Application.Interfaces.AI;
using Wanas.Application.Interfaces.Authentication;
using Wanas.Application.Mappings;
using Wanas.Application.Services;
using Wanas.Application.Validators;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.AI;
using Wanas.Infrastructure.Authentication;
using Wanas.Infrastructure.Persistence;
using Wanas.Infrastructure.Repositories;
using Wanas.Infrastructure.Services;
using Wanas.Infrastructure.Settings;

namespace Wanas.API.Extentions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Identity Configuration (single registration to avoid duplicate schemes)
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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

            // Additional Identity options
            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            });

            // JWT Options binding & authentication setup
            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience
                };
            });

            services.AddSingleton<IJwtProvider, JwtProvider>();

            // Repositories - Register all repositories needed by UnitOfWork
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportPhotoRepository, ReportPhotoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IAppealRepository, AppealRepository>();
            services.AddScoped<ITrafficLogRepository, TrafficLogRepository>();
            services.AddScoped<IDailyMetricsRepository, DailyMetricsRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ICommissionRepository, CommissionRepository>();
            services.AddScoped<IPayoutRepository, PayoutRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services (Application Layer)
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IListingSearchService, ListingSearchService>();
            services.AddScoped<IListingDescriptionService, ListingDescriptionService>();
            //services.AddScoped<IGenerateListingService, GenerateListingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IListingModerationService, ListingModerationService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IRevenueService, RevenueService>();

            // Real-time notifier (singleton)
            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();

            #region RAG DEPENDENCIES
            services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddHttpClient<IChromaService, ChromaService>();
            services.AddHttpClient<IAIProvider, OpenAIProvider>();
            //services.AddHttpClient<IAIProvider, GroqProvider>();
            services.AddScoped<IChatbotService, ChatbotService>();

            services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));
            services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddScoped<IChromaService, ChromaService>();
            services.AddScoped<MatchingService>();
            #endregion

            #region MATCHING SERVICE SELECTION
            services.AddScoped<IMatchingService, StaticTestMatchingService>();
            #endregion

            #region Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("VerifiedUser", policy =>
                policy.Requirements.Add(new VerifiedUserRequirement()));
            });
            services.AddScoped<IAuthorizationHandler, VerifiedUserHandler>();
            #endregion

            #region Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            #endregion

            services.AddValidatorsFromAssemblyContaining<SuspendUserCommandValidator>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SuspendUserCommandHandler>());

            // AutoMapper
            services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfile>(); }, typeof(MappingProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<ReportProfile>(); }, typeof(ReportProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<ListingProfile>(); }, typeof(ListingProfile).Assembly);

            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
            services.AddProblemDetails();
            services.AddHttpContextAccessor();

            services.AddMapsterConfig();
            services.AddFluentValidationConfig();

            return services;
        }

        private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
        {
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(mappingConfig));
            return services;
        }

        private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            return services;
        }
    }
}
