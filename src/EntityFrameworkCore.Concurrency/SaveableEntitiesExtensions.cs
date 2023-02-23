using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Concurrency;

/// <summary>
/// Extension methods for <see cref="ISaveableEntities"/> classes.
/// </summary>
public static class SaveableEntitiesExtensions
{
    /// <summary>
    /// Attempts to save changes to the database and resolve any concurrency conflicts the operation encounters.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows modified.</returns>
    /// <exception cref="DbUpdateConcurrencyException">A concurrency conflict could not be resolved.</exception>
    /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<int> SaveChangesWithConflictResolutionAsync(this ISaveableEntities entities, CancellationToken cancellationToken = default)
    {
        return await entities.SaveChangesWithConflictResolutionAsync(ConcurrencyConflictApproach.Default, 0, cancellationToken);
    }

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
    public static async Task<int> SaveChangesWithConflictResolutionAsync(this ISaveableEntities entities, ConcurrencyConflictApproach approach, CancellationToken cancellationToken = default)
    {
        return await entities.SaveChangesWithConflictResolutionAsync(approach, 0, cancellationToken);
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
    public static async Task<int> SaveChangesWithConflictResolutionAsync(this ISaveableEntities entities, ConcurrencyConflictApproach approach, int retryCount, CancellationToken cancellationToken = default)
    {
        int attemptCount = 0;
        do
        {
            try
            {
                return await entities.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (approach == ConcurrencyConflictApproach.Default)
                {
                    throw;
                }

                foreach (var entry in ex.Entries)
                {
                    FixConflictedEntry(entry, approach);
                }
            }
        }
        while (attemptCount++ <= retryCount); // Success will return out of this method and not loop again.

        return 0;
    }

    /// <summary>
    /// Updates the current entry to either keep the value currently in the database, or overwrite
    /// it with the new value, depending on the approach specified.
    /// </summary>
    /// <param name="entry">The conflicted entry to fix.</param>
    /// <param name="approach">The approach to take to fix the conflict.</param>
    private static void FixConflictedEntry(EntityEntry entry, ConcurrencyConflictApproach approach)
    {
        var proposedValues = entry.CurrentValues;
        var databaseValues = entry.GetDatabaseValues();

        if (databaseValues == null) // Record in database was deleted.
        {
            entry.State = approach == ConcurrencyConflictApproach.ForceUpdate ? EntityState.Added : EntityState.Unchanged;
            return;
        }

        foreach (var property in proposedValues.Properties) // Record in database was updated.
        {
            var proposedValue = proposedValues[property];
            var databaseValue = databaseValues[property];

            proposedValues[property] = approach == ConcurrencyConflictApproach.ForceUpdate ? proposedValue : databaseValue;
        }

        // Refresh original values to bypass next concurrency check.
        entry.OriginalValues.SetValues(databaseValues);
    }
}