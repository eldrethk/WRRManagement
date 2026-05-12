using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using System.Reflection;
using WRRManagement.Api.HealthChecks;
using WRRManagement.Api.StartupConfig;
using WRRManagement.Infrastructure.Data;
using WRRManagement.Infrastructure.Repositories;
using Scalar.AspNetCore;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Services;
using WRRManagement.Application.Services;
using WRRManagement.Application.Rooms;
using WRRManagement.Application.Hotels;


//Serilog Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();


    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.AddSerilogServices();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    //Database access
    var connectionstring = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Default connection is not configured");

    builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionstring));

    // ** Register repositories**
    //Auth
    builder.Services.AddScoped<IApiUserRepository, ApiUserRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    //Hotel info
    builder.Services.AddScoped<IHotelRepository, HotelRepository>();
    builder.Services.AddScoped<IDisclaimerRepository, DisclaimerRepository>();
    //Room query dependencies
    builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
    builder.Services.AddScoped<IRoomImageRepository, RoomImageRepository>();
    builder.Services.AddScoped<IRoomFeaturesRepository, RoomFeaturesRepository>();
    builder.Services.AddScoped<IAdultBaseRepository, AdultBaseRepository>();
    builder.Services.AddScoped<IMaxBaseRepository, MaxBaseRepository>();
    builder.Services.AddScoped<IRoomAllocation, RoomAllocationRepository>();
    builder.Services.AddScoped<IRackRateRepository, RackRateRepository>();
    builder.Services.AddScoped<ITierLevelRepository, TierLevelRepository>();
    builder.Services.AddScoped<IMinStayRepository, MinStayRepository>();
    builder.Services.AddScoped<IHotelSystemRepository, HotelSystemRepository>();
    //Infrastructure Services
    builder.Services.AddSingleton<IPasswordService, PasswordService>(); //Stateless BCrypt hashing
    builder.Services.AddSingleton<ITokenService,  TokenService>(); //Reads Config + generates tokens
    //Application Services
    builder.Services.AddScoped<IAuthService, AuthServices>();
    builder.Services.AddScoped<IRoomQueryService, RoomQueryService>();
    builder.Services.AddScoped<IHotelQueryService, HotelQueryService>();

    builder.AddDevAuthServices();
    builder.AddAuthorizatonServices();

    builder.AddDevHealthCheckServices();

    builder.AddOpenApiDocumentation();

    builder.Services.AddResponseCaching();

    try
    {
        var assembly = typeof(HotelSystemRepository).Assembly;
        var types = assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException ex)
    {
        foreach (var loaderException in ex.LoaderExceptions)
        {
            Console.WriteLine(loaderException?.Message);
        }
        throw;
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();

        app.UseCors("AllowAll");
    }

    app.UseHttpsRedirection();
    app.UseResponseCaching();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    //healthcheck endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    //healthcheck-ui dashboard
    app.MapHealthChecksUI(opts =>
    {
        opts.UIPath = "/healthchecks-ui";
    }).AllowAnonymous();

    //mark startup as complete - required for StartupHealthCheck
    var startupHealthCheck = app.Services.GetService<StartupHealthCheck>();
    startupHealthCheck?.SetReady();


    app.Run();

