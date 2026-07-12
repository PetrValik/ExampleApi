using ExampleApi.Features.Articles.BatchCreateArticles;
using ExampleApi.Features.Articles.CreateArticle;
using ExampleApi.Features.Articles.DeleteArticle;
using ExampleApi.Features.Articles.GetArticle;
using ExampleApi.Features.Articles.GetArticles;
using ExampleApi.Features.Articles.UpdateArticle;

namespace ExampleApi.Features.Articles;

/// <summary>
/// Provides extension methods for registering articles feature services.
/// </summary>
public static class ArticlesRegistration
{
    /// <summary>
    /// Registers all services required for the articles feature.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddArticlesFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICreateArticleHandler, CreateArticleHandler>();
        services.AddScoped<IGetArticleHandler, GetArticleHandler>();
        services.AddScoped<IGetArticlesHandler, GetArticlesHandler>();
        services.AddScoped<IUpdateArticleHandler, UpdateArticleHandler>();
        services.AddScoped<IDeleteArticleHandler, DeleteArticleHandler>();
        services.AddScoped<IBatchCreateArticlesHandler, BatchCreateArticlesHandler>();

        return services;
    }
}
