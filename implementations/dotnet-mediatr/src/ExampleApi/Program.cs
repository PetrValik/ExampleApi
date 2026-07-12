using ExampleApi.Common.Behaviors;
using ExampleApi.Common.Endpoints;
using ExampleApi.Configuration;
using ExampleApi.Infrastructure.Database;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// RFC 7807 problem+json is the error shape for every failure path.
builder.Services.AddProblemDetails();

// Infrastructure + cross-cutting.
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// MediatR: register handlers and the validation pipeline behaviour that
// short-circuits any request whose FluentValidation validator fails into a
// Result failure (mapped to 400 problem+json by the endpoints).
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<Program>();
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Minimal-API endpoint modules (IEndpoint) discovered by reflection.
builder.Services.AddEndpoints();

var app = builder.Build();

// Create the schema (and wait for the DB to come up) before serving traffic.
await app.Services.InitializeDatabaseAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
