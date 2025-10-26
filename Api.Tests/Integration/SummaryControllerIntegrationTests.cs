using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TaskPomodoro.Api.Controllers;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Data;
using TaskPomodoro.Api.Tests.Helpers;
using System.Threading.Tasks;
using Xunit;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Models = TaskPomodoro.Api.Models;
using Session = TaskPomodoro.Api.Models.Session;
using SessionKind = TaskPomodoro.Api.Models.SessionKind;

namespace TaskPomodoro.Api.Tests.Integration;

public class SummaryControllerIntegrationTests
{
    [Fact]
    public async Task Get_正常系_有効な期間で集計取得()
    {
        // Given: テスト用SQLiteデータベースと期間内のセッションデータ
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 3);

            // まずTaskを作成
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            // テストデータを作成
            context.Sessions.AddRange(
                // 2024-01-01のセッション
                TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, 25, startDate.AddHours(9), startDate.AddHours(9).AddMinutes(25)),
                TestDataBuilder.CreateSession(2, 1, SessionKind.Break, 5, 5, startDate.AddHours(9).AddMinutes(25), startDate.AddHours(9).AddMinutes(30)),
                TestDataBuilder.CreateSession(3, 1, SessionKind.Focus, 25, 25, startDate.AddHours(10), startDate.AddHours(10).AddMinutes(25)),

                // 2024-01-02のセッション
                TestDataBuilder.CreateSession(4, 1, SessionKind.Focus, 25, 25, startDate.AddDays(1).AddHours(9), startDate.AddDays(1).AddHours(9).AddMinutes(25)),
                TestDataBuilder.CreateSession(5, 1, SessionKind.Break, 5, 5, startDate.AddDays(1).AddHours(9).AddMinutes(25), startDate.AddDays(1).AddHours(9).AddMinutes(30)),

                // 2024-01-03のセッション
                TestDataBuilder.CreateSession(6, 1, SessionKind.Focus, 25, 25, startDate.AddDays(2).AddHours(9), startDate.AddDays(2).AddHours(9).AddMinutes(25))
            );
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var controller = new SummaryController(sessionRepository);

            // When: 有効な期間で集計を取得する
            var result = await controller.Get("2024-01-01", "2024-01-03");

            // Then: 200 OKで集計データが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as SummaryResponseDto;
            response.Should().NotBeNull();

            // 3日分のデータが返却される
            response!.Days.Should().HaveCount(3);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task Get_正常系_単一日の集計取得()
    {
        // Given: テスト用SQLiteデータベースと単一日のセッションデータ
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
                TestDataBuilder.CreateSession(2, 1, SessionKind.Break, 5, 5, targetDate.AddHours(9).AddMinutes(25), targetDate.AddHours(9).AddMinutes(30)),
                TestDataBuilder.CreateSession(3, 1, SessionKind.Focus, 25, 25, targetDate.AddHours(10), targetDate.AddHours(10).AddMinutes(25))
            );
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var controller = new SummaryController(sessionRepository);

            // When: 単一日で集計を取得する
            var result = await controller.Get("2024-01-01", "2024-01-01");

            // Then: 200 OKで集計データが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as SummaryResponseDto;
            response.Should().NotBeNull();

            response!.Days.Should().HaveCount(1);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task Get_正常系_セッションなしの期間()
    {
        // Given: テスト用SQLiteデータベース（セッションなし）
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var controller = new SummaryController(sessionRepository);

            // When: セッションなしの期間で集計を取得する
            var result = await controller.Get("2024-01-01", "2024-01-03");

            // Then: 200 OKで空の集計データが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as SummaryResponseDto;
            response.Should().NotBeNull();

            // 3日分のデータが返却される（欠損日補完）
            response!.Days.Should().HaveCount(3);
            response.Days.Should().OnlyContain(d =>
                d.FocusMinutes == 0 && d.BreakMinutes == 0 && d.TotalSessions == 0);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task Get_正常系_未完了セッションは集計対象外()
    {
        // Given: テスト用SQLiteデータベースと完了済み/未完了セッション
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

            // 完了済みセッション（集計対象）
            context.Sessions.Add(TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, 25, targetDate.AddHours(9), targetDate.AddHours(9).AddMinutes(25)));
            context.Sessions.Add(TestDataBuilder.CreateSession(2, 1, SessionKind.Break, 5, 5, targetDate.AddHours(9).AddMinutes(25), targetDate.AddHours(9).AddMinutes(30)));

            // 未完了セッション（集計対象外）
            context.Sessions.Add(TestDataBuilder.CreateSession(3, 1, SessionKind.Focus, 25, null, targetDate.AddHours(10), null));
            context.Sessions.Add(TestDataBuilder.CreateSession(4, 1, SessionKind.Break, 5, null, targetDate.AddHours(10).AddMinutes(25), null));

            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var controller = new SummaryController(sessionRepository);

            // When: 混在データで集計を取得する
            var result = await controller.Get("2024-01-01", "2024-01-01");

            // Then: 完了済みセッションのみが集計される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as SummaryResponseDto;
            response.Should().NotBeNull();

            var daySummary = response!.Days.First();
            daySummary.FocusMinutes.Should().Be(25); // 完了済みのみ
            daySummary.BreakMinutes.Should().Be(5); // 完了済みのみ
            daySummary.TotalSessions.Should().BeGreaterOrEqualTo(2); // 完了済み＋未完了
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }

    [Fact]
    public async Task Get_正常系_長期間の集計取得()
    {
        // Given: テスト用SQLiteデータベースと長期間のセッションデータ
        var testDbPath = $"test_{Guid.NewGuid()}.db";

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 7); // 1週間

            // まずTaskを作成
            var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            // 各日に1つずつセッションを作成
            var sessions = new List<Session>();
            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                sessions.Add(TestDataBuilder.CreateSession(
                    i + 1, 1, SessionKind.Focus, 25, 25,
                    currentDate.AddHours(9), currentDate.AddHours(9).AddMinutes(25)));
            }

            context.Sessions.AddRange(sessions);
            await context.SaveChangesAsync();

            var sessionRepository = new EfRepository<Session>(context);
            var controller = new SummaryController(sessionRepository);

            // When: 長期間で集計を取得する
            var result = await controller.Get("2024-01-01", "2024-01-07");

            // Then: 200 OKで7日分の集計データが返却される
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as SummaryResponseDto;
            response.Should().NotBeNull();

            response!.Days.Should().HaveCount(7);
        }
        finally
        {
            if (File.Exists(testDbPath))
                File.Delete(testDbPath);
        }
    }
}

