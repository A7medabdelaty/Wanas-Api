using dotenv.net;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Wanas.API.Extentions;
using Wanas.API.Hubs;
using Wanas.API.Middlewares;
using Wanas.Application.Interfaces;
using Wanas.Infrastructure.Persistence.Seed;


// Configure Serilog (basic console + file rolling)
var logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logsPath);


Log.Logger = new LoggerConfiguration()
 .Enrich.FromLogContext()
 .WriteTo.Console()
.WriteTo.File(
    Path.Combine(AppContext.BaseDirectory, "Logs", "requests-.log"),
    rollingInterval: RollingInterval.Day,
    shared: true
)
.CreateLogger();



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Load .env file only in Development
if (builder.Environment.IsDevelopment())
{
    DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
    Log.Information("Loaded .env file for Development environment");
}

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



builder.Services.AddApplicationServices(builder.Configuration);


Log.Information("ContentRootPath: {Path}", builder.Environment.ContentRootPath);
Log.Information("WebRootPath: {Path}", builder.Environment.WebRootPath);



// ======== BUILD & INITIALIZE ========
var app = builder.Build();


var uploadsPath = Path.Combine(builder.Environment.WebRootPath!, "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});





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
    var indexingService = scope.ServiceProvider.GetRequiredService<IChromaIndexingService>();

    try
    {
        await chromaService.InitializeCollectionAsync();
        Console.WriteLine("ChromaDB collection initialized successfully");

        // Check if we need to do initial indexing
        var hasDocuments = await indexingService.HasAnyDocumentsAsync();

        if (!hasDocuments && app.Environment.IsDevelopment())
        {
            Console.WriteLine("No documents found in ChromaDB. Starting initial bulk indexing...");
            var result = await indexingService.IndexAllListingsAsync();
            Console.WriteLine($"Initial indexing complete. Success: {result.SuccessCount}, Failed: {result.FailedCount}");

            if (result.FailedCount > 0)
            {
                Console.WriteLine($"Indexing errors: {string.Join("; ", result.Errors.Take(5))}");
            }
        }
        else if (hasDocuments)
        {
            Console.WriteLine("ChromaDB already has indexed listings");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ChromaDB init/indexing failed: {ex.Message}");
        Console.WriteLine("Application will continue with traditional matching only");
    }
}





app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseMiddleware<SignalRTokenMiddleware>();

app.UseAuthentication();
app.UseMiddleware<TrafficLoggingMiddleware>();
// Add User Status Check Middleware (must be after Authentication)
app.UseMiddleware<UserStatusMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets
    | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
    options.ApplicationMaxBufferSize = 32 * 1024;
    options.TransportMaxBufferSize = 32 * 1024;
});
//using (var scope = app.Services.CreateScope()) { try { await DataSeeder.SeedAsync(scope.ServiceProvider); } catch (Exception ex) { Console.WriteLine($"Seeding failed: {ex.Message}"); } }
app.Run();
