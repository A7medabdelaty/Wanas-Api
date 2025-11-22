using Microsoft.EntityFrameworkCore;
using Wanas.Application.Interfaces;
using Wanas.Application.Mappings;
using Wanas.Application.Services;
using Wanas.Domain.Repositories;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.Persistence;
using Wanas.Infrastructure.Repositories;
using Wanas.Infrastructure.Repositories.Listings;

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
            services.AddScoped<IReviewRepository, ReviewRepository>();
            //services.AddScoped<IListingRepository, ListingRepository>();
            //services.AddScoped<IListingPhotoRepository, ListingPhotoRepository>();
            //services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services (Application Layer)
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IReviewService, ReviewService>();
            //services.AddScoped<IListingService, ListingService>();

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
