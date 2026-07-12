using System.Text;
using ExampleApi.Common.Filters;
using ExampleApi.Configuration;
using ExampleApi.Data;
using ExampleApi.Repositories;
using ExampleApi.Services;
using ExampleApi.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// MVC / controllers + cross-cutting filters
// ---------------------------------------------------------------------------
builder.Services
    .AddControllers(options =>
    {
        // Domain exceptions -> problem+json (404/409/500).
        options.Filters.Add<GlobalExceptionFilter>();
        // FluentValidation failures -> normalised 400 problem+json with an "errors" map.
        options.Filters.Add<FluentValidationActionFilter>();
    })
    .AddJsonOptions(options =>
    {
        // camelCase for the pagination wrapper and token response; the article DTOs override this
        // per-property with [JsonPropertyName] to emit snake_case. Nulls are serialised (default).
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddProblemDetails();

// ---------------------------------------------------------------------------
// Persistence (EF Core + Npgsql)
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(dbContextOptions =>
    dbContextOptions.UseNpgsql(builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found in configuration.")));

// ---------------------------------------------------------------------------
// Layered dependencies: repository <- service <- controller
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ---------------------------------------------------------------------------
// JWT authentication
// ---------------------------------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException($"JWT settings section '{JwtSettings.SectionName}' not found in configuration.");

if (jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        $"JWT SecretKey must be at least 32 characters for HMAC-SHA256. Current length: {jwtSettings.SecretKey.Length}.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Create/upgrade the schema (with connect retry) before serving traffic.
await app.Services.InitialiseDatabaseAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
