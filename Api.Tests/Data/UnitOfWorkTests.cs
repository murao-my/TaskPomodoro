using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TaskPomodoro.Api.Data;
using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Tests.Helpers;
using System.Threading.Tasks;
using Xunit;
using System;
using System.IO;
using Task = System.Threading.Tasks.Task;
using Session = TaskPomodoro.Api.Models.Session;
using SessionKind = TaskPomodoro.Api.Models.SessionKind;

namespace TaskPomodoro.Api.Tests.Data;

/// <summary>
/// UnitOfWork のユニットテスト
/// </summary>
public class UnitOfWorkTests
{
    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_正常系_変更あり()
    {
        // Given: テスト用SQLiteデータベースと変更されたエンティティ
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);

            var unitOfWork = new UnitOfWork(context);

            // When: SaveChangesAsyncを呼び出す
            var result = await unitOfWork.SaveChangesAsync();

            // Then: 影響行数が1以上
            result.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_正常系_変更なし()
    {
        // Given: テスト用SQLiteデータベース（変更なし）
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var unitOfWork = new UnitOfWork(context);

            // When: SaveChangesAsyncを呼び出す（変更なし）
            var result = await unitOfWork.SaveChangesAsync();

            // Then: 影響行数が0
            result.Should().Be(0);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_正常系_複数エンティティの変更()
    {
        // Given: テスト用SQLiteデータベースと複数のエンティティ
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var task1 = TestDataBuilder.CreateTask(1, "Task 1", "Note 1", 3, false, DateTime.UtcNow);
            var task2 = TestDataBuilder.CreateTask(2, "Task 2", "Note 2", 5, false, DateTime.UtcNow);
            var task3 = TestDataBuilder.CreateTask(3, "Task 3", "Note 3", 2, true, DateTime.UtcNow);

            context.Tasks.AddRange(task1, task2, task3);

            var unitOfWork = new UnitOfWork(context);

            // When: SaveChangesAsyncを呼び出す
            var result = await unitOfWork.SaveChangesAsync();

            // Then: 影響行数が3
            result.Should().Be(3);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion

    #region Repository Pattern の動作確認

    [Fact]
    public async Task Repository_正常系_Taskリポジトリ取得()
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

            var unitOfWork = new UnitOfWork(context);

            // When: Taskリポジトリを取得する
            var taskRepository = new EfRepository<Models.Task>(context);
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            await taskRepository.AddAsync(task);
            await unitOfWork.SaveChangesAsync();

            // Then: エンティティが保存される
            var savedTask = await taskRepository.GetByIdAsync(1);
            savedTask.Should().NotBeNull();
            savedTask!.Title.Should().Be("Test Task");
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task Repository_正常系_Sessionリポジトリ取得()
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

            // まずTaskを作成
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var unitOfWork = new UnitOfWork(context);

            // When: Sessionリポジトリを取得する
            var sessionRepository = new EfRepository<Session>(context);
            var session = TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, null, DateTime.UtcNow, null);
            await sessionRepository.AddAsync(session);
            await unitOfWork.SaveChangesAsync();

            // Then: エンティティが保存される
            var savedSession = await sessionRepository.GetByIdAsync(1);
            savedSession.Should().NotBeNull();
            savedSession!.TaskId.Should().Be(1);
            savedSession.Kind.Should().Be(SessionKind.Focus);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion
}

