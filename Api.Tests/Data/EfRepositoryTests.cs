using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TaskPomodoro.Api.Data;
using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Tests.Helpers;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace TaskPomodoro.Api.Tests.Data;

/// <summary>
/// EfRepository のユニットテスト
/// </summary>
public class EfRepositoryTests
{
    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_正常系_エンティティ追加()
    {
        // Given: テスト用SQLiteデータベースとTaskエンティティ
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new EfRepository<Models.Task>(context);
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);

            // When: AddAsyncを呼び出す
            var result = await repository.AddAsync(task);

            // Then: エンティティが返却される
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Task");
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task AddAsync_異常系_nullエンティティ()
    {
        // Given: テスト用SQLiteデータベース
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: nullエンティティでAddAsyncを呼び出す
            // Then: ArgumentNullExceptionが発生する（AddAsync はnullチェックあり）
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await repository.AddAsync(null!));
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_正常系_存在するID()
    {
        // Given: テスト用SQLiteデータベースとタスク
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var existingTask = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(existingTask);
            await context.SaveChangesAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: 存在するIDでGetByIdAsyncを呼び出す
            var result = await repository.GetByIdAsync(1);

            // Then: エンティティが返却される
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Test Task");
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task GetByIdAsync_正常系_存在しないID()
    {
        // Given: テスト用SQLiteデータベース（データなし）
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: 存在しないIDでGetByIdAsyncを呼び出す
            var result = await repository.GetByIdAsync(999);

            // Then: nullが返却される
            result.Should().BeNull();
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_正常系_境界値ID(int id)
    {
        // Given: テスト用SQLiteデータベース（データなし）
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: 境界値IDでGetByIdAsyncを呼び出す
            var result = await repository.GetByIdAsync(id);

            // Then: nullが返却される
            result.Should().BeNull();
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_正常系_エンティティ更新()
    {
        // Given: テスト用SQLiteデータベースとタスク
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var existingTask = TestDataBuilder.CreateTask(1, "Old Title", "Old Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(existingTask);
            await context.SaveChangesAsync();

            var repository = new EfRepository<Models.Task>(context);

            // エンティティを更新
            existingTask.Title = "New Title";
            existingTask.Note = "New Note";

            // When: UpdateAsyncを呼び出す
            await repository.UpdateAsync(existingTask);

            // Then: Taskが完了する（例外が発生しない）
            await Task.CompletedTask;
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task UpdateAsync_異常系_nullエンティティ()
    {
        // Given: テスト用SQLiteデータベース
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: nullエンティティでUpdateAsyncを呼び出す
            // Then: NullReferenceExceptionが発生する（現在の実装ではnullチェックがない）
            await Assert.ThrowsAsync<NullReferenceException>(async () => await repository.UpdateAsync(null!));
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_正常系_エンティティ削除()
    {
        // Given: テスト用SQLiteデータベースとタスク
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var existingTask = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(existingTask);
            await context.SaveChangesAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: DeleteAsyncを呼び出す
            await repository.DeleteAsync(existingTask);

            // Then: Taskが完了する（例外が発生しない）
            await Task.CompletedTask;
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task DeleteAsync_異常系_nullエンティティ()
    {
        // Given: テスト用SQLiteデータベース
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: nullエンティティでDeleteAsyncを呼び出す
            // Then: ArgumentNullExceptionが発生する（EF CoreのRemoveがnullチェックする）
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await repository.DeleteAsync(null!));
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task Query_正常系_IQueryable返却()
    {
        // Given: テスト用SQLiteデータベースと複数のタスク
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var tasks = new List<Models.Task>
            {
                TestDataBuilder.CreateTask(1, "Task 1", "Note 1", 3, false, DateTime.UtcNow),
                TestDataBuilder.CreateTask(2, "Task 2", "Note 2", 5, true, DateTime.UtcNow),
                TestDataBuilder.CreateTask(3, "Task 3", "Note 3", 2, false, DateTime.UtcNow),
            };
            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync();

            var repository = new EfRepository<Models.Task>(context);

            // When: Queryを呼び出す
            var result = repository.Query();

            // Then: IQueryable<Task>が返却される
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IQueryable<Models.Task>>();

            // IQueryableの操作が可能であることを確認
            var count = result.Count();
            count.Should().Be(3);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion
}

