using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TaskPomodoro.Api.Controllers;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Data;
using Models = TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Tests.Helpers;
using System.Threading.Tasks;
using Xunit;
using System;
using System.IO;
using System.Collections.Generic;

namespace TaskPomodoro.Api.Tests.Integration;

public class TasksControllerIntegrationTests
{
    [Fact]
    public async Task GetTasks_正常系_全件取得_パラメータなし()
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

            // テストデータを追加
            context.Tasks.AddRange(
                TestDataBuilder.CreateTask(1, "Task 1", "Note 1", 3, false, DateTime.UtcNow),
                TestDataBuilder.CreateTask(2, "Task 2", "Note 2", 5, true, DateTime.UtcNow)
            );
            await context.SaveChangesAsync();

            // 実リポジトリとコントローラーを作成
            var repository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new TasksController(repository, unitOfWork);

            // When: statusパラメータなしでGetTasksを呼び出す
            var result = await controller.GetTasks(null);

            // Then: 200 OKで全タスクが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var responseTasks = okResult?.Value as IEnumerable<TaskResponseDto>;
            responseTasks.Should().HaveCount(2);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task GetTasks_正常系_アクティブタスクのみ取得()
    {
        // Given: テスト用SQLiteデータベースとアクティブ/アーカイブタスク
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            // テストデータを追加
            context.Tasks.AddRange(
                TestDataBuilder.CreateTask(1, "Active Task 1", "Note 1", 3, false, DateTime.UtcNow),
                TestDataBuilder.CreateTask(2, "Active Task 2", "Note 2", 5, false, DateTime.UtcNow),
                TestDataBuilder.CreateTask(3, "Archived Task", "Note 3", 2, true, DateTime.UtcNow)
            );
            await context.SaveChangesAsync();

            // 実リポジトリとコントローラーを作成
            var repository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new TasksController(repository, unitOfWork);

            // When: status="active"でGetTasksを呼び出す
            var result = await controller.GetTasks("active");

            // Then: 200 OKでアクティブタスクのみが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var responseTasks = okResult?.Value as IEnumerable<TaskResponseDto>;
            responseTasks.Should().HaveCount(2);
            responseTasks.Should().OnlyContain(t => !t.IsArchived);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task GetTasks_正常系_アーカイブタスクのみ取得()
    {
        // Given: テスト用SQLiteデータベースとアクティブ/アーカイブタスク
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            // テストデータを追加
            context.Tasks.AddRange(
                TestDataBuilder.CreateTask(1, "Active Task", "Note 1", 3, false, DateTime.UtcNow),
                TestDataBuilder.CreateTask(2, "Archived Task 1", "Note 2", 5, true, DateTime.UtcNow),
                TestDataBuilder.CreateTask(3, "Archived Task 2", "Note 3", 2, true, DateTime.UtcNow)
            );
            await context.SaveChangesAsync();

            // 実リポジトリとコントローラーを作成
            var repository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new TasksController(repository, unitOfWork);

            // When: status="archived"でGetTasksを呼び出す
            var result = await controller.GetTasks("archived");

            // Then: 200 OKでアーカイブタスクのみが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var responseTasks = okResult?.Value as IEnumerable<TaskResponseDto>;
            responseTasks.Should().HaveCount(2);
            responseTasks.Should().OnlyContain(t => t.IsArchived);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }
}

