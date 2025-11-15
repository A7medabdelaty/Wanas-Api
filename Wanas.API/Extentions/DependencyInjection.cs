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
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services (Application Layer)
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IReportService, ReportService>();

            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);
            services.AddAutoMapper(cfg => {cfg.AddProfile<ReportProfile>();}, typeof(ReportProfile).Assembly);

            return services;
        }
    }
}
