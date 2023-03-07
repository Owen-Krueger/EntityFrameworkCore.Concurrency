using EntityFrameworkCore.Concurrency.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Moq;
using Moq.AutoMock;

namespace EntityFrameworkCore.Concurrency.Tests;

public class DbContextExtensionsTests
{
    private DbContextOptions<ContextTestEntities> options;
    private const string value1 = "value1";

    [SetUp]
    public void Init()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        options = new DbContextOptionsBuilder<ContextTestEntities>()
            .UseSqlite(connection)
            .Options;
    }

    [Test]
    public async Task SaveChangesWithConflictResolutionAsync_RecordModifiedForceUpdate_PreviousValueOverwritten()
    {
        string value2 = "value2";
        var database1 = new ContextTestEntities(options);
        await SetUpDatabase1Async(database1);
        IContextTestEntities database2 = new ContextTestEntities(options);
        var model2 = database2.TestModels.First();
        model2.TestModelString = value2;
        model2.Version = Guid.NewGuid();
        await database2.SaveChangesAsync();

        int result = await database1.SaveChangesAsync(ConcurrencyConflictApproach.ForceUpdate);

        Assert.That(result, Is.EqualTo(1));
        var record = database1.TestModels.First();
        Assert.That(record.TestModelString, Is.EqualTo(value1));
    }

    [Test]
    public async Task SaveChangesWithConflictResolutionAsync_RecordModifiedSkipConflicts_PreviousValueRemains()
    {
        string value2 = "value2";
        var database1 = new ContextTestEntities(options);
        await SetUpDatabase1Async(database1);
        IContextTestEntities database2 = new ContextTestEntities(options);
        var model2 = database2.TestModels.First();
        model2.TestModelString = value2;
        model2.Version = Guid.NewGuid();
        await database2.SaveChangesAsync();

        int result = await database1.SaveChangesAsync(ConcurrencyConflictApproach.SkipConflicts);

        Assert.That(result, Is.EqualTo(1));
        var record = database1.TestModels.First();
        Assert.That(record.TestModelString, Is.EqualTo(value2));
    }

    [Test]
    public async Task SaveChangesWithConflictResolutionAsync_RecordRemovedForceUpdate_RecordReadded()
    {
        var database1 = new ContextTestEntities(options);
        await SetUpDatabase1Async(database1);
        IContextTestEntities database2 = new ContextTestEntities(options);
        var model2 = database2.TestModels.First();
        database2.TestModels.Remove(model2);
        await database2.SaveChangesAsync();

        int result = await database1.SaveChangesAsync(ConcurrencyConflictApproach.ForceUpdate);

        Assert.That(result, Is.EqualTo(1));
        var record = database1.TestModels.First();
        Assert.That(record.TestModelString, Is.EqualTo(value1));
    }

    [Test]
    public async Task SaveChangesWithConflictResolutionAsync_RecordRemovedSkipConflicts_RecordRemainsRemoved()
    {
        var database1 = new ContextTestEntities(options);
        await SetUpDatabase1Async(database1);
        IContextTestEntities database2 = new ContextTestEntities(options);
        var model2 = database2.TestModels.First();
        database2.TestModels.Remove(model2);
        await database2.SaveChangesAsync();

        int result = await database1.SaveChangesAsync(ConcurrencyConflictApproach.SkipConflicts);

        Assert.That(result, Is.Zero);
        Assert.That(database1.TestModels.Count(), Is.Zero);
    }

    private static async Task SetUpDatabase1Async(ContextTestEntities testEntities)
    {
        await testEntities.Database.EnsureCreatedAsync();
        testEntities.TestModels.Add(new TestModel());
        await testEntities.SaveChangesAsync();
        var model1 = testEntities.TestModels.First();
        model1.TestModelString = value1;
        model1.Version = Guid.NewGuid();
    }
}

public class ContextTestEntities : DbContext, IContextTestEntities
{
    public DbSet<TestModel> TestModels { get; set; }

    public ContextTestEntities(DbContextOptions<ContextTestEntities> dbContextOptions) : base(dbContextOptions) { }
}

public interface IContextTestEntities
{
    DbSet<TestModel> TestModels { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}