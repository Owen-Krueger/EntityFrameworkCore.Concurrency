using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.Concurrency.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Moq;
using Moq.AutoMock;

namespace EntityFrameworkCore.Concurrency.Tests;

public class SaveableEntitiesTests
    {
        private DbContextOptions<InterfaceTestEntities> options;
        private const string value1 = "value1";

        [SetUp]
        public void Init()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            options = new DbContextOptionsBuilder<InterfaceTestEntities>()
                .UseSqlite(connection)
                .Options;
        }

        [Test]
        public void SaveChangesWithConflictResolutionAsync_DefaultApproach_DbUpdateConcurrencyExceptionThrown()
        {
            var mock = new AutoMocker();
            var entry = mock.GetMock<IUpdateEntry>();
            var entries = new List<IUpdateEntry>() { entry.Object };
            var exception = new DbUpdateConcurrencyException(string.Empty, entries);
            var testEntities = mock.GetMock<ISaveableTestEntities>();
            testEntities.Setup(x => x.SaveChangesAsync(CancellationToken.None))
                .ThrowsAsync(exception);
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                testEntities.Object.SaveChangesAsync(ConcurrencyConflictApproach.Default)
            );
        }

        [Test]
        public async Task SaveChangesWithConflictResolutionAsync_DefaultApproachWithRetrySuccess_SaveSuccess()
        {
            var mock = new AutoMocker();
            var entry = mock.GetMock<IUpdateEntry>();
            var entries = new List<IUpdateEntry>() { entry.Object };
            var exception = new DbUpdateConcurrencyException(string.Empty, entries);
            var testEntities = mock.GetMock<ISaveableTestEntities>();
            testEntities.SetupSequence(x => x.SaveChangesAsync(CancellationToken.None))
                .ThrowsAsync(exception)
                .ThrowsAsync(exception)
                .ReturnsAsync(1);
            
            int result = await testEntities.Object.SaveChangesAsync(ConcurrencyConflictApproach.Default, 3);
            
            Assert.That(result, Is.EqualTo(1));
        }
        
        [Test]
        public void SaveChangesWithConflictResolutionAsync_DefaultApproachWithRetry_DbUpdateConcurrencyExceptionThrown()
        {
            var mock = new AutoMocker();
            var entry = mock.GetMock<IUpdateEntry>();
            var entries = new List<IUpdateEntry>() { entry.Object };
            var exception = new DbUpdateConcurrencyException(string.Empty, entries);
            var testEntities = mock.GetMock<ISaveableTestEntities>();
            testEntities.Setup(x => x.SaveChangesAsync(CancellationToken.None))
                .ThrowsAsync(exception);
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                testEntities.Object.SaveChangesAsync(ConcurrencyConflictApproach.Default, 3)
            );
        }

        [Test]
        public async Task SaveChangesWithConflictResolutionAsync_RecordModifiedForceUpdate_PreviousValueOverwritten()
        {
            string value2 = "value2";
            var database1 = new InterfaceTestEntities(options);
            await SetUpDatabase1Async(database1);
            ISaveableTestEntities database2 = new InterfaceTestEntities(options);
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
            var database1 = new InterfaceTestEntities(options);
            await SetUpDatabase1Async(database1);
            ISaveableTestEntities database2 = new InterfaceTestEntities(options);
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
            var database1 = new InterfaceTestEntities(options);
            await SetUpDatabase1Async(database1);
            ISaveableTestEntities database2 = new InterfaceTestEntities(options);
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
            var database1 = new InterfaceTestEntities(options);
            await SetUpDatabase1Async(database1);
            ISaveableTestEntities database2 = new InterfaceTestEntities(options);
            var model2 = database2.TestModels.First();
            database2.TestModels.Remove(model2);
            await database2.SaveChangesAsync();

            int result = await database1.SaveChangesAsync(ConcurrencyConflictApproach.SkipConflicts);

            Assert.That(result, Is.Zero);
            Assert.That(database1.TestModels.Count(), Is.Zero);
        }

        private static async Task SetUpDatabase1Async(InterfaceTestEntities testEntities)
        {
            await testEntities.Database.EnsureCreatedAsync();
            testEntities.TestModels.Add(new TestModel());
            await testEntities.SaveChangesAsync();
            var model1 = testEntities.TestModels.First();
            model1.TestModelString = value1;
            model1.Version = Guid.NewGuid();
        }
    }

    public class InterfaceTestEntities : DbContext, ISaveableTestEntities
    {
        public DbSet<TestModel> TestModels { get; set; }

        public InterfaceTestEntities(DbContextOptions<InterfaceTestEntities> dbContextOptions) : base(dbContextOptions) { }
    }

    public interface ISaveableTestEntities : ISaveableEntities
    {
        DbSet<TestModel> TestModels { get; set; }
    }