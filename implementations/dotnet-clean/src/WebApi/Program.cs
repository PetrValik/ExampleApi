using ExampleApi.Application;
using ExampleApi.Infrastructure;
using ExampleApi.Infrastructure.Persistence;
using ExampleApi.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// --- Composition root ------------------------------------------------------
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// --- Create the schema (retrying while PostgreSQL comes up) ----------------
await app.Services.InitializeDatabaseAsync();

// --- Middleware pipeline ---------------------------------------------------
app.UseDomainExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints -------------------------------------------------------------
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapArticleEndpoints();

app.Run();

/// <summary>
/// Exposed so an integration-test host (WebApplicationFactory) can reference the entry point.
/// </summary>
public partial class Program;
