using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using WRRManagement.Api.HealthChecks;
using Microsoft.OpenApi.Models;



namespace WRRManagement.Api.StartupConfig
{
    public static class DIDevExtensions
    {
        public static void AddDevAuthServices(this WebApplicationBuilder builder)
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("Jwt secret key is not configured");

            builder.Services.AddAuthentication(opts =>
            {
                //default scheme for authentication
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                // TOKEN VALIDATION PARAMETERS
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validate the issuer (who created the token)
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],

                    // Validate the audience (who the token is for)
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],

                    // Validate the signing key
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)),

                    // Validate token hasn't expired
                    ValidateLifetime = true,

                    // Clock skew allows for slight time differences between servers
                    // Default is 5 minutes, reduce for stricter security
                    ClockSkew = TimeSpan.FromMinutes(5),

                    // Require expiration claim
                    RequireExpirationTime = true
                };

                //EVENTS - Hook into authentication events
                opts.Events = new JwtBearerEvents
                {
                    // Called when authentication fails
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            // Add header to tell client token is expired
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },

                    // Called when token is validated
                    OnTokenValidated = context =>
                    {
                        Log.Debug("Token validated for {User}",
                            context.Principal?.Identity?.Name ?? "unknown");
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public static void AddDevHealthCheckServices(this WebApplicationBuilder builder)
        {
            // Register StartupHealthCheck as singleton so we can call SetReady() on it later
            var startupHealthCheck = new StartupHealthCheck();
            builder.Services.AddSingleton(startupHealthCheck);

            builder.Services.AddHealthChecks()
             // Database health check
             .AddSqlServer(
                 connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
                 healthQuery: "SELECT 1;", // Simple query to verify connectivity
                 name: "SQL Server",
                 failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                 tags: ["database", "sql"])
             // Custom health checks
             .AddCheck<ApiHealthCheck>("API Health")
             // Use the singleton instance for StartupHealthCheck
             .AddCheck("Startup", startupHealthCheck, tags: ["ready"]);

            // Health Checks UI - Visual dashboard
            builder.Services.AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(30);        // Check every 30 seconds
                options.MaximumHistoryEntriesPerEndpoint(50); // Keep history
                options.AddHealthCheckEndpoint("API", "/health");
            })
            .AddInMemoryStorage();//In-memory storage for development
        }
        
        public static void AddOpenApiDocumentation(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApi(options =>
            {
                // ============================================================
                // DOCUMENT TRANSFORMER
                // ============================================================
                // In .NET 9 OpenAPI, you use "Document Transformers".
                //
                // A Document Transformer gives you direct access to the
                // OpenApiDocument object, allowing you to modify it before
                // it's served to clients.
                //
                // Think of it as a post-processing hook:
                //   1. .NET generates the base OpenAPI document from your controllers
                //   2. Your transformer modifies it (add auth, metadata, etc.)
                //   3. The final document is served at /openapi/v1.json
                // ============================================================
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    // ============================================================
                    // API INFO / METADATA
                    // ============================================================
                 
                    document.Info = new OpenApiInfo
                    {
                        Title = "APITemplate",
                        Version = "v1",
                        Description = "API Template with OpenAPI documentation"
                    };

                    // ============================================================
                    // SECURITY SCHEME DEFINITIONS
                    // ============================================================
                    //
                    // This tells the OpenAPI document that the API supports
                    // authentication. It defines HOW a client should send
                    // credentials (via headers, query params, etc.).
                    //
                    // IMPORTANT: This only DESCRIBES the scheme in the docs.
                    // It does NOT enforce authentication — that's handled by
                    // your middleware (AddAuthentication / AddJwtBearer).
                    // ============================================================
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                    {
                        // --------------------------------------------------------
                        // JWT BEARER TOKEN
                        // --------------------------------------------------------
                        ["Bearer"] = new OpenApiSecurityScheme
                        {
                            // SecuritySchemeType.Http = uses the Authorization header
                            // Other options: ApiKey, OAuth2, OpenIdConnect
                            Type = SecuritySchemeType.Http,

                            // "bearer" tells clients to use the "Bearer" scheme
                            // Authorization header: "Authorization: Bearer <token>"
                            Scheme = "bearer",

                            // Hint to UI tools (like Scalar) that the token is JWT
                            BearerFormat = "JWT",

                            // Description shown in the UI's auth dialog
                            Description = "Enter your JWT token. Example: 'eyJhbGciOiJIUzI1NiIs...'"
                        },

                        // --------------------------------------------------------
                        // API KEY AUTHENTICATION (alternative to JWT)
                        // --------------------------------------------------------
                        ["ApiKey"] = new OpenApiSecurityScheme
                        {
                            // SecuritySchemeType.ApiKey = sent as a named header/query param
                            Type = SecuritySchemeType.ApiKey,

                            // The header name clients must use
                            Name = "X-Api-Key",

                            // Where the key is sent (Header, Query, or Cookie)
                            In = ParameterLocation.Header,

                            Description = "Enter your API Key"
                        }
                    };

                    // ============================================================
                    // GLOBAL SECURITY REQUIREMENT
                    // ============================================================
                    // This applies the "Bearer" security scheme GLOBALLY to all
                    // endpoints in the OpenAPI document. Every endpoint will show
                    // a lock icon in the Scalar UI.
                    //
                    // NOTE: This is purely a documentation/UI concern.
                    // Actual authorization is still handled by [Authorize]
                    // attributes and your middleware pipeline.
                    //
                    // TIP: If you only want certain endpoints to require auth
                    // in the docs, remove this section and use an
                    // "Operation Transformer" instead to selectively apply
                    // security per endpoint.
                    // ============================================================
                    document.SecurityRequirements = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    // Reference the scheme by its key — must match the
                    // key in SecuritySchemes above ("Bearer")
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }] = Array.Empty<string>() // Empty scopes — not used for JWT
                }
            };

                    // Transformer must return a Task since the signature is async-capable.
                    // Use Task.CompletedTask since we don't do any async work here.
                    return Task.CompletedTask;
                });
            });
        }
    }
}
