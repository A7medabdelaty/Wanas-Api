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

            // Repositories - Register all repositories needed by UnitOfWork
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services (Application Layer)
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
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
