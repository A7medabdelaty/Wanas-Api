using CloudinaryDotNet;
using dotenv.net;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using Wanas.API.Authorization;
using Wanas.API.RealTime;
using Wanas.Application.Handlers.Admin;
using Wanas.Application.Interfaces;
using Wanas.Application.Interfaces.AI;
using Wanas.Application.Interfaces.Authentication;
using Wanas.Application.Mappings;
using Wanas.Application.Services;
using Wanas.Application.Validators;
using Wanas.Application.Validators.Authentication;
using Wanas.Application.Validators.Listing;
using Wanas.Application.Validators.Review;
using Wanas.Application.Validators.User;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.AI;
using Wanas.Infrastructure.Authentication;
using Wanas.Infrastructure.Persistence;
using Wanas.Infrastructure.Repositories;
using Wanas.Infrastructure.Repositories.Listings;
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
                // Allow SignalR to read token from query string
                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        
                        // Check for token in query string for both WebSocket upgrade and regular requests
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton<IJwtProvider, JwtProvider>();

            // Repositories - Register all repositories needed by UnitOfWork
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();
            services.AddScoped<IListingPhotoRepository, ListingPhotoRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportPhotoRepository, ReportPhotoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IAppealRepository, AppealRepository>();
            services.AddScoped<ITrafficLogRepository, TrafficLogRepository>();
            services.AddScoped<IDailyMetricsRepository, DailyMetricsRepository>();
            services.AddScoped<ICommissionRepository, CommissionRepository>();
            services.AddScoped<IPayoutRepository, PayoutRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IBedRepository, BedRepository>();
            services.AddScoped<IBookingApprovalRepository, BookingApprovalRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
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
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IListingService, ListingService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IListingModerationService, ListingModerationService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IRevenueService, RevenueService>();
            services.AddScoped<IBookingApprovalService, BookingApprovalService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddHostedService<ReservationExpirationService>();

            // Real-time notifier (singleton)
            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });
            
            services.AddSingleton<IUserIdProvider, ClaimNameUserIdProvider>();


            services.AddScoped<IRoommateMatchingService, RoommateMatchingService>();
            //services.AddScoped<IMatchingService,MatchingService>(); // real traditional matcher
            
            // RAG dependencies
            services.AddHttpClient<IAIProvider, OpenAIProvider>();
            services.AddScoped<IChatbotService, ChatbotService>();
            services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));

            // matching service - Using Google Gemini for embeddings
            services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>();
            services.AddHttpClient<IChromaService, ChromaService>();
            services.AddScoped<IChromaIndexingService, ChromaIndexingService>();
            services.AddScoped<MatchingService>();
            services.AddScoped<IMatchingService>(sp =>
                new HybridMatchingService(
                    sp.GetRequiredService<MatchingService>(),
                    sp.GetRequiredService<IChromaService>(),
                    sp.GetRequiredService<IUnitOfWork>(),
                    sp.GetRequiredService<ILogger<HybridMatchingService>>()));


            // Background service for ChromaDB indexing
            services.AddHostedService<ChromaIndexingBackgroundService>();


            // auth service
            services.AddAuthorization(options =>
            {
                options.AddPolicy("VerifiedUser", policy =>
                policy.Requirements.Add(new VerifiedUserRequirement()));
            });
            services.AddScoped<IAuthorizationHandler, VerifiedUserHandler>();
            // swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Validators
            services.AddValidatorsFromAssemblyContaining<CreateListingDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateListingDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateCommentDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateListingDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateRoomDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateRoomDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<BedDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<ConfirmEmailRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<ForgetPasswordRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<ResendConfirmationEmailRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<ResetPasswordRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateReviewDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateReviewDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<CompletePreferencesRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<CompleteProfileRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdatePreferencesRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateProfileRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<BanUserCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<ReviewAppealCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<SubmitAppealCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<SuspendUserCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<UnbanUserCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<UnsuspendUserCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<UnverifyUserCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<VerifyUserCommandValidator>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SuspendUserCommandHandler>());

            // AutoMapper
            services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfile>(); }, typeof(MappingProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<ReportProfile>(); }, typeof(ReportProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<ListingProfile>(); }, typeof(ListingProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<ReviewProfile>(); }, typeof(ReviewProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<CommentProfile>(); }, typeof(CommentProfile).Assembly);
            services.AddAutoMapper(cfg => { cfg.AddProfile<AIListingMappingProfile>(); }, typeof(AIListingMappingProfile).Assembly);

            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
            services.AddProblemDetails();
            services.AddHttpContextAccessor();

            services.AddMapsterConfig();
            services.AddFluentValidationConfig();

            // .env configurations (Development only - loaded in Program.cs)
            // DotEnv.Load is now called conditionally in Program.cs based on environment

            // Cloudinary Init
            services.AddSingleton(sp =>
            {
                var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");

                var cloudinary = new Cloudinary(cloudinaryUrl);
                cloudinary.Api.Secure = true;

                return cloudinary;
            });

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
