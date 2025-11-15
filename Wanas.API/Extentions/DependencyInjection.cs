using Microsoft.EntityFrameworkCore;
using Wanas.API.RealTime;
using Wanas.Application.Interfaces;
using Wanas.Application.Mappings;
using Wanas.Application.Services;
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

            // Repositories
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IMatchingService, MatchingService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();

            // Services (Application Layer)
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();

            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();

            // Replace the HybridMatchingService registration with:
            services.AddScoped<IMatchingService, StaticTestMatchingService>();
            #region RAG DEPENDENCIES

            // 1. HTTP Clients
            services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddHttpClient<IChromaService, ChromaService>();

            // 2. Configuration
            services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));

            // 3. Service Registrations
            services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
            services.AddScoped<IChromaService, ChromaService>();
            // In Program.cs - add these registrations
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();

            // 4. Keep original matching service
            services.AddScoped<MatchingService>();

            // 5. Register Hybrid as the main IMatchingService
            //builder.Services.AddScoped<IMatchingService>(provider =>
            //{
            //    var traditional = provider.GetRequiredService<MatchingService>();
            //    var chroma = provider.GetRequiredService<IChromaService>();
            //    var userRepo = provider.GetRequiredService<IUserRepository>();
            //    var prefRepo = provider.GetRequiredService<IUserPreferenceRepository>();

            //    return new HybridMatchingService(traditional, chroma, userRepo, prefRepo);
            //});

            // 5. Register StaticTest as the main IMatchingService
            services.AddScoped<IMatchingService>(provider =>
            {
                var chroma = provider.GetRequiredService<IChromaService>();
                return new StaticTestMatchingService(chroma);
            });
            #endregion

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
