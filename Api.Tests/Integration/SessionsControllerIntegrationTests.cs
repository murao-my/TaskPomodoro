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
using System.Linq;
using Session = TaskPomodoro.Api.Models.Session;
using SessionKind = TaskPomodoro.Api.Models.SessionKind;

namespace TaskPomodoro.Api.Tests.Integration;

public class SessionsControllerIntegrationTests
{
    #region StartSession Tests

    [Fact]
    public async Task StartSession_正常系_Focusセッション開始()
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

            // タスクを作成
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            // 実リポジトリとコントローラーを作成
            var sessionRepository = new EfRepository<Session>(context);
            var taskRepository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new SessionsController(sessionRepository, taskRepository, unitOfWork);

            // セッション作成DTO
            var createDto = new SessionCreateDto
            {
                TaskId = 1,
                Kind = SessionKind.Focus,
                PlannedMinutes = 25,
                StartedAt = DateTime.UtcNow
            };

            // When: Focusセッションを開始する
            var result = await controller.StartSession(createDto);

            // Then: 201 Createdでセッションが作成される
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            var responseSession = createdResult?.Value as SessionResponseDto;
            responseSession.Should().NotBeNull();
            responseSession!.TaskId.Should().Be(1);
            responseSession.Kind.Should().Be(SessionKind.Focus);
            responseSession.PlannedMinutes.Should().Be(25);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Theory]
    [InlineData(1)]  // 最小値
    [InlineData(120)] // 最大値
    public async Task StartSession_正常系_有効な時間範囲(int plannedMinutes)
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

            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var taskRepository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new SessionsController(sessionRepository, taskRepository, unitOfWork);

            var createDto = new SessionCreateDto
            {
                TaskId = 1,
                Kind = SessionKind.Focus,
                PlannedMinutes = plannedMinutes,
                StartedAt = DateTime.UtcNow
            };

            // When: 有効な時間でセッションを開始する
            var result = await controller.StartSession(createDto);

            // Then: 201 Createdでセッションが作成される
            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task StartSession_異常系_重複するアクティブセッション()
    {
        // Given: テスト用SQLiteデータベースと既存のアクティブセッション
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            // タスクとアクティブセッションを作成
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);

            var existingSession = TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, null, DateTime.UtcNow, null);
            context.Sessions.Add(existingSession);
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var taskRepository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new SessionsController(sessionRepository, taskRepository, unitOfWork);

            var createDto = new SessionCreateDto
            {
                TaskId = 1,
                Kind = SessionKind.Focus,
                PlannedMinutes = 25,
                StartedAt = DateTime.UtcNow
            };

            // When: 重複するセッションを開始する
            var result = await controller.StartSession(createDto);

            // Then: 409 Conflictが返却される
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion

    #region GetSessions Tests

    [Fact]
    public async Task GetSessions_正常系_指定日のセッション取得()
    {
        // Given: テスト用SQLiteデータベースと指定日のセッション
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var targetDate = new DateTime(2024, 1, 1);

            // まずTaskを作成
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            context.Sessions.AddRange(
                TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, 25, targetDate.AddHours(9), targetDate.AddHours(9).AddMinutes(25)),
                TestDataBuilder.CreateSession(2, 1, SessionKind.Break, 5, 5, targetDate.AddHours(9).AddMinutes(25), targetDate.AddHours(9).AddMinutes(30))
            );
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var taskRepository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new SessionsController(sessionRepository, taskRepository, unitOfWork);

            // When: 指定日でセッションを取得する
            var result = await controller.GetSessions("2024-01-01");

            // Then: 200 OKで該当日のセッションが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var responseSessions = okResult?.Value as IEnumerable<SessionResponseDto>;
            responseSessions.Should().HaveCount(2);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task GetSessions_正常系_全期間のセッション取得()
    {
        // Given: テスト用SQLiteデータベースと複数日のセッション
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

            context.Sessions.AddRange(
                TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, 25, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1).AddMinutes(25)),
                TestDataBuilder.CreateSession(2, 1, SessionKind.Focus, 25, 25, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(25))
            );
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var taskRepository = new EfRepository<Models.Task>(context);
            var unitOfWork = new UnitOfWork(context);
            var controller = new SessionsController(sessionRepository, taskRepository, unitOfWork);

            // When: 日付指定なしでセッションを取得する
            var result = await controller.GetSessions(null);

            // Then: 200 OKで全セッションが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var responseSessions = okResult?.Value as IEnumerable<SessionResponseDto>;
            responseSessions.Should().HaveCount(2);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    #endregion
}

