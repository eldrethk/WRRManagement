using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.Runtime.CompilerServices;
using WRRManagement.Core.Constants;

namespace WRRManagement.Api.StartupConfig
{
    public static class DICommonExtensions
    {
        public static void AddSerilogServices(this WebApplicationBuilder builder)
        {
            var isDevelopment = builder.Environment.IsDevelopment();

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName();

                if (isDevelopment)
                {
                    // DEVELOPMENT: Clean, readable console output
                    configuration
                        .MinimumLevel.Information()
                        // Reduce noise from Microsoft/System namespaces
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
                        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                        .WriteTo.Console(
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
                }
                else
                {
                    // PRODUCTION: Only warnings and errors to console
                    configuration
                        .MinimumLevel.Warning()
                        .WriteTo.Console(
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
                }

                // Always write detailed logs to file
                configuration.WriteTo.File(
                    path: "logs/api-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            });
        }

        public static void AddAuthorizatonServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(opts =>
            {
                //the policy that makes every endpoint required authentication
                opts.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();


                // ============================================================
                // ROLE-BASED POLICIES
                // ============================================================
                // Usage in controllers:
                //   [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
                //   [Authorize(Policy = AuthConstants.Policies.RequireManagerOrAbove)]
                //   [Authorize(Policy = AuthConstants.Policies.RequireStaff)]
                //
                // These check the "role" claim in the JWT token.
                // The role claim is set by TokenService.GenerateAccessToken().
                //
                // HIERARCHY:
                //   Admin > Manager > FrontDesk
                //   - RequireAdmin: Only Admin
                //   - RequireManagerOrAbove: Admin OR Manager
                //   - RequireStaff: Admin OR Manager OR FrontDesk
                // ============================================================

                //admin only 
                opts.AddPolicy(AuthConstants.Policies.RequireAdmin, policy =>
                {
                    policy.RequireRole(AuthConstants.Roles.Admin);
                });

                //Manager or above
                opts.AddPolicy(AuthConstants.Policies.RequireManagerOrAbove, policy =>
                {
                    policy.RequireRole(AuthConstants.Roles.Manager, AuthConstants.Roles.Admin);
                });

                //Any Staff memeber
                opts.AddPolicy(AuthConstants.Policies.RequireStaff, policy =>
                {
                    policy.RequireRole(AuthConstants.Roles.Admin, AuthConstants.Roles.Manager, AuthConstants.Roles.FrontDesk);
                });
                   
                
            });

        }
    }
}
