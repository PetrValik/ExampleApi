using ExampleApi.Application.Articles.UseCases;
using ExampleApi.Application.Articles.Validation;
using ExampleApi.Application.Auth.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleApi.Application;

/// <summary>
/// Composition helpers for the Application layer: use-case handlers and validators.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers every use-case handler and FluentValidation validator in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateArticleRequestValidator>();

        services.AddScoped<ICreateArticleHandler, CreateArticleHandler>();
        services.AddScoped<IGetArticleHandler, GetArticleHandler>();
        services.AddScoped<IGetArticlesHandler, GetArticlesHandler>();
        services.AddScoped<IUpdateArticleHandler, UpdateArticleHandler>();
        services.AddScoped<IDeleteArticleHandler, DeleteArticleHandler>();
        services.AddScoped<IBatchCreateArticlesHandler, BatchCreateArticlesHandler>();

        services.AddScoped<IGetTokenHandler, GetTokenHandler>();

        return services;
    }
}
