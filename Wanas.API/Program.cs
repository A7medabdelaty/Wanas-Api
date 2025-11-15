using Microsoft.EntityFrameworkCore;
using Wanas.API.Extentions;
using Wanas.API.Hubs;
using Wanas.Application.Interfaces;
using Wanas.Application.Services;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;
using Wanas.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
.AllowAnyMethod()
         .AllowCredentials()
    .SetIsOriginAllowed(_ => true);
  });
});

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Replace the HybridMatchingService registration with:
builder.Services.AddScoped<IMatchingService, StaticTestMatchingService>();
#region RAG DEPENDENCIES

// 1. HTTP Clients
builder.Services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();
builder.Services.AddHttpClient<IChromaService, ChromaService>();

// 2. Configuration
builder.Services.Configure<OpenAIConfig>(builder.Configuration.GetSection("OpenAI"));

// 3. Service Registrations
builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
builder.Services.AddScoped<IChromaService, ChromaService>();
// In Program.cs - add these registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();

// 4. Keep original matching service
builder.Services.AddScoped<MatchingService>();

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
builder.Services.AddScoped<IMatchingService>(provider =>
{
    var chroma = provider.GetRequiredService<IChromaService>();
    return new StaticTestMatchingService(chroma);
});



#endregion

// ======== BUILD & INITIALIZE ========
var app = builder.Build();

// Configure Swagger (works in all environments)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wanas API v1");
    });
}


//Initialize ChromaDB on startup
using (var scope = app.Services.CreateScope())
{
    var chromaService = scope.ServiceProvider.GetRequiredService<IChromaService>();
    try
    {
        await chromaService.InitializeCollectionAsync();
        Console.WriteLine("ChromaDB collection initialized");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ChromaDB init failed: {ex.Message}");
        // Continue - traditional matching will still work
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
