using dotenv.net;
using Serilog;
using Wanas.API.Extentions;
using Wanas.API.Hubs;
using Wanas.API.Middlewares;
using Wanas.Application.Interfaces;


// Configure Serilog (basic console + file rolling)
Log.Logger = new LoggerConfiguration()
 .Enrich.FromLogContext()
 .WriteTo.Console()
 .WriteTo.File("Logs/requests-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit:7)
 .CreateLogger();



var builder = WebApplication.CreateBuilder(args);

DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

builder.Host.UseSerilog();

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

app.Run();