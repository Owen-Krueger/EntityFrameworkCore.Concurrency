using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Concurrency.Context;

public static class DbContextExtensions
{
    /// <summary>
    /// Attempts to save changes to the database and resolve any concurrency conflicts the operation encounters.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="approach">The approach to take when concurrency conflicts encountered.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows modified.</returns>
    /// <exception cref="DbUpdateConcurrencyException">A concurrency conflict could not be resolved.</exception>
    /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<int> SaveChangesAsync(this DbContext entities, ConcurrencyConflictApproach approach, CancellationToken cancellationToken = default)
    {
        return await entities.SaveChangesAsync(approach, 0, cancellationToken);
    }

    /// <summary>
    /// Attempts to save changes to the database and resolve any concurrency conflicts the operation encounters.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="approach">The approach to take when concurrency conflicts encountered.</param>
    /// <param name="retryCount">The amount of times to retry resolving conflicts.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows modified.</returns>
    /// <exception cref="DbUpdateConcurrencyException">A concurrency conflict could not be resolved.</exception>
    /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<int> SaveChangesAsync(this DbContext entities, ConcurrencyConflictApproach approach, int retryCount, CancellationToken cancellationToken = default)
    {
        return await ConcurrencyService.SaveChangesAsync(() => entities.SaveChangesAsync(cancellationToken),
            approach, retryCount);
    }
}