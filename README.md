# SaveableEntities and DbContextExtensions
This package adds the ability for applications using Entity Framework to handle concurrency conflicts automatically. These changes can either be done on interfaces/classes that extend the `ISaveableEntities` interface or `DbContext` objects. Using this package requires the project to be on .NET 6.0 or newer. These packages should only be used with projects using EntityFramework version 6.x.x or higher on .NET 6.0 or version 7.x.x on .NET 7.0.


## ISaveableEntities Interface
This adds the `SaveChangesAsync` method to an interface. This will allow changes to the entities to be saved to the database.

## SaveChangesAsync overloads
This will save changes like above, but will also try to resolve any concurrency conflicts it encounters during the operation. Optionally, consumers can specify the conflict approach to take and the number of retries to attempt before failing. This method can be used by either interfaces/classes that extend the `ISaveableEntities` interface or `DbContext` objects.

There are a few different options to approach conflicts:
- `Default`: Follows the default path when concurrent conflicts are encountered, which is to throw a `DbUpdateConcurrencyException` exception
- `ForceUpdate`: Forces the property to overwrite whatever is currently set in the database.
- `SkipConflicts`: Skips conflicting values to leave whatever is currently set in the database.

These approaches will be taken if the record is modified or removed between the time your context found and modified a record.

The following table shows how each approach will resolve concurrency issues. Record 1 is what the application is currently trying to set the record property to. Record 2 shows what operation happened to the record prior to our update, creating a concurrency conflict.

| Record 1 Value | Record 2 Value   | Approach      | Resulting Value    |
|----------------|------------------|---------------|--------------------|
| Apple          | Orange           | Default       | *Exception thrown* |
| Apple          | Orange           | ForceUpdate   | Apple              |
| Apple          | Orange           | SkipConflicts | Orange             |
| Apple          | *Record Removed* | Default       | *Exception thrown* |
| Apple          | *Record Removed* | ForceUpdate   | Apple              |
| Apple          | *Record Removed* | SkipConflicts | *Record Removed*   |

**Please Note:** This method can still throw exceptions if a non-concurrency exception is raised or if the retry limit was exceeded, so consumers should still add error handling around their save operations.
