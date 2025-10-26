using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using TaskPomodoro.Api.Controllers;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Data;
using Models = TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Tests.Helpers;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Session = TaskPomodoro.Api.Models.Session;
using SessionKind = TaskPomodoro.Api.Models.SessionKind;

namespace TaskPomodoro.Api.Tests.Controllers;

public class SessionsControllerTests
{
    private readonly Mock<IRepository<Session>> _mockSessionRepository;
    private readonly Mock<IRepository<Models.Task>> _mockTaskRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SessionsController _controller;

    public SessionsControllerTests()
    {
        _mockSessionRepository = new Mock<IRepository<Session>>();
        _mockTaskRepository = new Mock<IRepository<Models.Task>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new SessionsController(_mockSessionRepository.Object, _mockTaskRepository.Object, _mockUnitOfWork.Object);
    }

    #region StartSession Tests

    // 注意: StartSession_正常系_* テストは統合テスト（SessionsControllerIntegrationTests.cs）に移行済み
    // 理由: FirstOrDefaultAsync()を使用するため

    [Fact]
    public async Task StartSession_異常系_存在しないTaskId()
    {
        // Given: 存在しないタスクID
        var createDto = new SessionCreateDto
        {
            TaskId = 999,
            Kind = SessionKind.Focus,
            PlannedMinutes = 25,
            StartedAt = DateTime.UtcNow
        };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Models.Task?)null);

        // When: 存在しないTaskIdでセッションを開始する
        var result = await _controller.StartSession(createDto);

        // Then: 404 NotFoundが返却される
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult?.Value.Should().Be("Task with ID 999 not found.");
    }



    #endregion

    #region CompleteSession Tests

    [Fact]
    public async Task CompleteSession_正常系_自動時間計算で完了()
    {
        // Given: 実行中のセッション
        var existingSession = TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, null, DateTime.UtcNow.AddMinutes(-10), null);
        var updateDto = new SessionUpdateDto
        {
            ActualMinutes = null,
            EndedAt = null // 自動計算
        };

        _mockSessionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingSession);
        _mockSessionRepository.Setup(x => x.UpdateAsync(It.IsAny<Session>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: セッションを完了する（自動時間計算）
        var result = await _controller.CompleteSession(1, updateDto);

        // Then: 200 OKでセッションが更新される
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseSession = okResult?.Value as SessionResponseDto;
        responseSession.Should().NotBeNull();
        responseSession!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task CompleteSession_正常系_手動時間指定で完了()
    {
        // Given: 実行中のセッションと手動時間指定
        var existingSession = TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, null, DateTime.UtcNow.AddMinutes(-10), null);
        var updateDto = new SessionUpdateDto
        {
            ActualMinutes = 30,
            EndedAt = DateTime.UtcNow
        };

        _mockSessionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingSession);
        _mockSessionRepository.Setup(x => x.UpdateAsync(It.IsAny<Session>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: 手動時間指定でセッションを完了する
        var result = await _controller.CompleteSession(1, updateDto);

        // Then: 200 OKでセッションが更新される
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseSession = okResult?.Value as SessionResponseDto;
        responseSession.Should().NotBeNull();
        responseSession!.ActualMinutes.Should().Be(30);
        responseSession.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task CompleteSession_異常系_存在しないID()
    {
        // Given: 存在しないセッションID
        var updateDto = new SessionUpdateDto
        {
            ActualMinutes = 30,
            EndedAt = DateTime.UtcNow
        };
        _mockSessionRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Session?)null);

        // When: 存在しないIDでセッションを完了する
        var result = await _controller.CompleteSession(999, updateDto);

        // Then: 404 NotFoundが返却される
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CompleteSession_異常系_既に完了済み()
    {
        // Given: 既に完了済みのセッション
        var completedSession = TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, 25, DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow);
        var updateDto = new SessionUpdateDto
        {
            ActualMinutes = 30,
            EndedAt = DateTime.UtcNow
        };

        _mockSessionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(completedSession);

        // When: 既に完了済みのセッションを完了する
        var result = await _controller.CompleteSession(1, updateDto);

        // Then: 409 Conflictが返却される
        result.Result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result.Result as ConflictObjectResult;
        conflictResult?.Value.Should().Be("Session with ID 1 is already completed.");
    }


    #endregion

    #region GetSession Tests

    [Fact]
    public async Task GetSession_正常系_存在するIDで取得()
    {
        // Given: 存在するセッションID
        var session = TestDataBuilder.CreateSession(1, 1, SessionKind.Focus, 25, 25, DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow);
        _mockSessionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(session);

        // When: 存在するIDでGetSessionを呼び出す
        var result = await _controller.GetSession(1);

        // Then: 200 OKでセッションが返却される
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseSession = okResult?.Value as SessionResponseDto;
        responseSession.Should().NotBeNull();
        responseSession!.Id.Should().Be(1);
        responseSession.TaskId.Should().Be(1);
        responseSession.Kind.Should().Be(SessionKind.Focus);
    }

    [Fact]
    public async Task GetSession_異常系_存在しないID()
    {
        // Given: 存在しないセッションID
        _mockSessionRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Session?)null);

        // When: 存在しないIDでGetSessionを呼び出す
        var result = await _controller.GetSession(999);

        // Then: 404 NotFoundが返却される
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetSessions Tests

    // 注意: GetSessions_正常系_* テストは統合テスト（SessionsControllerIntegrationTests.cs）に移行済み
    // 理由: IQueryable + ToListAsync()を使用するため

    [Theory]
    [InlineData("invalid-date")]
    [InlineData("2024-13-01")]
    [InlineData("2024-01-32")]
    [InlineData("not-a-date")]
    public async Task GetSessions_異常系_無効な日付形式(string invalidDate)
    {
        // Given: 無効な日付形式
        _mockSessionRepository.Setup(x => x.Query()).Returns(new List<Session>().AsQueryable());

        // When: 無効な日付でセッションを取得する
        var result = await _controller.GetSessions(invalidDate);

        // Then: 400 BadRequestが返却される
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("Invalid date format. Use YYYY-MM-DD format.");
    }

    #endregion
}
