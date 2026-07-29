using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Smart.Blazor;
using WRR.Admin.Components;
using WRR.Admin.Data;
using WRRManagement.Application.Amenities;
using WRRManagement.Application.Hotels;
using WRRManagement.Application.Marketing;
using WRRManagement.Application.Pricing;
using WRRManagement.Application.Reservations;
using WRRManagement.Application.Rooms;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;
using WRRManagement.Infrastructure.Repositories;

//Bootstrap logger (captures startup errors before full Serilog is configured)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //Serilog 
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Connection string
    var connectionString = builder.Configuration.GetConnectionString("WRRDBConnection")
        ?? throw new InvalidOperationException("Connection string 'WRRDBConnection' not found.");

    // ASP.NET Identity (EF Core + SQL Server) 
    builder.Services.AddDbContext<WRRDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = false;
    }).AddEntityFrameworkStores<WRRDbContext>();

    // Dapper connection factory 
    builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));

    // Repositories 
    // Hotel info
    builder.Services.AddScoped<IHotelRepository, HotelRepository>();
    builder.Services.AddScoped<IDisclaimerRepository, DisclaimerRepository>();
    builder.Services.AddScoped<IHotelUserRepository, HotelUserRepository>();
    // Room management
    builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
    builder.Services.AddScoped<IRoomImageRepository, RoomImageRepository>();
    builder.Services.AddScoped<IRoomFeaturesRepository, RoomFeaturesRepository>();
    builder.Services.AddScoped<IAdultBaseRepository, AdultBaseRepository>();
    builder.Services.AddScoped<IMaxBaseRepository, MaxBaseRepository>();
    builder.Services.AddScoped<IRoomAllocationRepository, RoomAllocationRepository>();
    builder.Services.AddScoped<IRackRateRepository, RackRateRepository>();
    builder.Services.AddScoped<ITierLevelRepository, TierLevelRepository>();
    builder.Services.AddScoped<IMinStayRepository, MinStayRepository>();
    builder.Services.AddScoped<IHotelSystemRepository, HotelSystemRepository>();
    // Package Management
    builder.Services.AddScoped<IPackageRepository, PackageRepository>();
    builder.Services.AddScoped<IPackageAmenityRepository, PackageAmenityRepository>();
    builder.Services.AddScoped<IPackageTierLevelRepository, PackageTierLevelRepository>();
    builder.Services.AddScoped<IPackageAllocationRepository, PackageAllocationRepository>();
    builder.Services.AddScoped<IPackageRateRepository, PackageRateRepository>();
    // Also add IHotelUserRepository after Fix Type 4 is done
    // Amenities
    builder.Services.AddScoped<IExtraAmenityRepository, ExtraAmenityRepository>();
    // Reservations
    builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
    builder.Services.AddScoped<IReservationAmenityRepository, ReservationAmenityRepository>();
    builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
    // Marketing
    builder.Services.AddScoped<IOptInEmailsRepository, OptInEmailsRepository>();

    // Application services 
    builder.Services.AddScoped<IHotelQueryService, HotelQueryService>();
    builder.Services.AddScoped<IRoomQueryService, RoomQueryService>();
    builder.Services.AddScoped<IAmenityQueryService, AmenityQueryService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();
    builder.Services.AddScoped<IMarketingService, MarketingService>();
    builder.Services.AddScoped<IQuoteService, QuoteService>();

    // UI frameworks 
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddControllersWithViews();
    builder.Services.AddSmart();
    builder.Services.AddBlazorBootstrap();

    // HttpClient (calls WRRManagement.Api for guest-facing reads if needed) 
    builder.Services.AddHttpClient("ApiClient", client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7149/api/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
    builder.Services.AddScoped(sp =>
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

    // Session 
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    
    var app = builder.Build();
   
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSession();
    app.UseAntiforgery();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.MapRazorPages();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WRR.Admin failed to start");
}
finally
{
    Log.CloseAndFlush();
}
