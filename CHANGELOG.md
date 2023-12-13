# 8.0.0 (2023-11-15)
## Features
- Added .NET 8.0 as a framework to use.
- Change dependent package from `Microsoft.EntityFrameworkCore.Sqlite` to `Microsoft.EntityFrameworkCore`.
## Misc
- Update packages.

# 7.3.0 (2023-08-03)
## Features
- Retry logic now used when utilizing default conflict approach.

# 7.2.0 (2023-07-30)
## Misc
- Broke from Microsoft's versioning.
- Started supporting .NET 6.0 and .NET 7.0 in one package.

# 7.1.0 (2023-03-06)
## Features
- Added extension method for `DbContext` objects to save changes with concurrency conflict logic.
- Renamed `SaveChangesWithConflictResolutionAsync` to `SaveChangesAsync`
- Move overload methods into specific namespaces.

# 6.1.0 (2023-03-06)
## Features
- Added extension method for `DbContext` objects to save changes with concurrency conflict logic.
- Renamed `SaveChangesWithConflictResolutionAsync` to `SaveChangesAsync`
- Move overload methods into specific namespaces.

# 7.0.0 (2023-02-23)
## Misc
- Updated EntityFramework to 7.0.3.

# 6.0.0 (2023-02-23)
## Features
- Initial release of EntityFrameworkCore.Concurrency.