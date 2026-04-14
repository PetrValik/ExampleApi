using ExampleApi.Common.Endpoints;
using ExampleApi.Configuration;

var builder = WebApplication.CreateSlimBuilder(args);

// Configure services
builder.Services.AddProblemDetails();
builder.Services.AddApiDocumentation();
builder.Services.AddRoutingConfiguration();
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFeatures();

var app = builder.Build();

// Initialize database
await app.InitializeDatabaseAsync();

// Configure middleware pipeline
app.UseGlobalExceptionHandler();
app.UseRouting();
app.UseJwtAuthentication();
app.UseApiDocumentation(app.Environment);

app.MapEndpoints();

app.Run();
