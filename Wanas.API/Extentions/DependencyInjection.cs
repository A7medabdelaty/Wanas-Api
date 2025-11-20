using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using Wanas.API.RealTime;
using Wanas.Application.Interfaces;
using Wanas.Application.Interfaces.AI;
using Wanas.Application.Interfaces.Authentication;
using Wanas.Application.Mappings;
using Wanas.Application.Services;
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

            // Repositories - Register all repositories needed by UnitOfWork
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services (Application Layer)
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();
            services.AddScoped<IListingSearchService, ListingSearchService>();

            #region RAG DEPENDENCIES

            // 1. HTTP Clients
            services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddHttpClient<IChromaService, ChromaService>();
            services.AddHttpClient<IAIProvider, OpenAIProvider>();
            services.AddHttpClient<IAIProvider, GroqProvider>();
            services.AddHttpClient<IChatbotService, ChatbotService>();


            // 2. Configuration
            services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));

            // 3. Service Registrations
            services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddScoped<IChromaService, ChromaService>();

            // 4. Keep original matching service as concrete implementation
            services.AddScoped<MatchingService>();
            services.AddScoped<IReportService, ReportService>();

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
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);
            services.AddAutoMapper(cfg => {cfg.AddProfile<ReportProfile>();}, typeof(ReportProfile).Assembly);

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ListingProfile>();
            }, typeof(ListingProfile).Assembly);


            // EmailService Configuration
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
            services.AddProblemDetails();
            services.AddHttpContextAccessor();

            //Mapster Configuration
            services.AddMapsterConfig();

            // FluentValidation Configuration
            services.AddFluentValidationConfig();

            // Authentication Configuration
            services.AddAuthConfig(configuration);

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
            services
                .AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
        private static IServiceCollection AddAuthConfig(this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDBContext>()
            .AddDefaultTokenProviders();

            services.AddSingleton<IJwtProvider, JwtProvider>();
            services.AddScoped<SignInManager<ApplicationUser>>();

            //services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
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

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            });

            return services;
        }
    }
}
