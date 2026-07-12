using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for <see cref="IUnitOfWork"/>. Commits the tracked changes and
/// translates a store-level concurrency failure into the Application's
/// <see cref="ConflictException"/>.
/// </summary>
public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The resource was modified by another request. Please retry.");
        }
    }
}
