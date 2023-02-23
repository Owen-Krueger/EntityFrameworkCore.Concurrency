namespace EntityFrameworkCore.Concurrency;

/// <summary>
/// The approach that should be taken when concurrency conflicts are encountered.
/// </summary>
public enum ConcurrencyConflictApproach
{
    /// <summary>
    /// The default approach (<see cref="DbUpdateConcurrencyException"/> thrown)
    /// </summary>
    Default,

    /// <summary>
    /// Forces the property to overwrite whatever is currently set in the database.
    /// </summary>
    ForceUpdate,

    /// <summary>
    /// Skips conflicting values to leave whatever is currently set in the database.
    /// </summary>
    SkipConflicts
}