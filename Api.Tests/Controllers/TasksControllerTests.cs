using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using TaskPomodoro.Api.Controllers;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Data;
using Models = TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Tests.Helpers;
using TaskPomodoro.Api.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace TaskPomodoro.Api.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<IRepository<Models.Task>> _mockTaskRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockTaskRepository = new Mock<IRepository<Models.Task>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new TasksController(_mockTaskRepository.Object, _mockUnitOfWork.Object);
    }

    #region GetTasks Tests

    // 注意: GetTasks_正常系_* テストは統合テスト（TasksControllerIntegrationTests.cs）に移行済み
    // 理由: IQueryable + ToListAsync()を使用するため

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("INVALID")]
    public async Task GetTasks_異常系_無効なstatusパラメータ(string invalidStatus)
    {
        // Given: 無効なstatusパラメータが渡される
        _mockTaskRepository.Setup(x => x.Query()).Returns(new List<Models.Task>().AsQueryable());

        // When: 無効なstatusでGetTasksを呼び出す
        var result = await _controller.GetTasks(invalidStatus);

        // Then: 400 BadRequestが返却される
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("Invalid status parameter. Use 'active' or 'archived'.");
    }

    #endregion

    #region GetTask Tests

    [Fact]
    public async Task GetTask_正常系_存在するIDで取得()
    {
        // Given: 存在するタスクID
        var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);

        // When: 存在するIDでGetTaskを呼び出す
        var result = await _controller.GetTask(1);

        // Then: 200 OKで該当タスクが返却される
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseTask = okResult?.Value as TaskResponseDto;
        responseTask.Should().NotBeNull();
        responseTask!.Id.Should().Be(1);
        responseTask.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTask_異常系_存在しないID()
    {
        // Given: 存在しないタスクID
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Models.Task?)null);

        // When: 存在しないIDでGetTaskを呼び出す
        var result = await _controller.GetTask(999);

        // Then: 404 NotFoundが返却される
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetTask_異常系_無効なID(int invalidId)
    {
        // Given: 無効なID（負の値またはゼロ）
        _mockTaskRepository.Setup(x => x.GetByIdAsync(invalidId)).ReturnsAsync((Models.Task?)null);

        // When: 無効なIDでGetTaskを呼び出す
        var result = await _controller.GetTask(invalidId);

        // Then: 404 NotFoundが返却される（IDが存在しないため）
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region CreateTask Tests

    [Fact]
    public async Task CreateTask_正常系_最小データで作成()
    {
        // Given: 最小限の有効なデータ
        var createDto = new TaskCreateDto
        {
            Title = "A", // 最小文字数
            Note = null,
            EstimatedPomos = null
        };
        var createdTask = TestDataBuilder.CreateTask(1, "A", null, null, false, DateTime.UtcNow);
        _mockTaskRepository.Setup(x => x.AddAsync(It.IsAny<Models.Task>())).ReturnsAsync(createdTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: 最小データでCreateTaskを呼び出す
        var result = await _controller.CreateTask(createDto);

        // Then: 201 Createdでタスクが作成される
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var responseTask = createdResult?.Value as TaskResponseDto;
        responseTask.Should().NotBeNull();
        responseTask!.Title.Should().Be("A");
    }

    [Fact]
    public async Task CreateTask_正常系_最大データで作成()
    {
        // Given: 最大値の有効なデータ
        var createDto = new TaskCreateDto
        {
            Title = new string('A', 100), // 最大文字数
            Note = new string('B', 1000), // 最大文字数
            EstimatedPomos = 100 // 最大値
        };
        var createdTask = TestDataBuilder.CreateTask(1, createDto.Title, createDto.Note, createDto.EstimatedPomos, false, DateTime.UtcNow);
        _mockTaskRepository.Setup(x => x.AddAsync(It.IsAny<Models.Task>())).ReturnsAsync(createdTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: 最大データでCreateTaskを呼び出す
        var result = await _controller.CreateTask(createDto);

        // Then: 201 Createdでタスクが作成される
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var responseTask = createdResult?.Value as TaskResponseDto;
        responseTask.Should().NotBeNull();
        responseTask!.Title.Should().Be(createDto.Title);
        responseTask.Note.Should().Be(createDto.Note);
        responseTask.EstimatedPomos.Should().Be(createDto.EstimatedPomos);
    }

    // 注意: CreateTask_異常系_* バリデーションテストは削除
    // 理由: ModelStateの自動検証は[ApiController]によって自動処理されるため、
    //      ユニットテストでは検証できない（統合テストで検証すべき）

    #endregion

    #region UpdateTask Tests

    [Fact]
    public async Task UpdateTask_正常系_全項目更新()
    {
        // Given: 存在するタスクと有効な更新データ
        var existingTask = TestDataBuilder.CreateTask(1, "Original Title", "Original Note", 3, false, DateTime.UtcNow);
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Title",
            Note = "Updated Note",
            EstimatedPomos = 5,
            IsArchived = true
        };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTask);
        _mockTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<Models.Task>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: 全項目を更新する
        var result = await _controller.UpdateTask(1, updateDto);

        // Then: 200 OKでタスクが更新される
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseTask = okResult?.Value as TaskResponseDto;
        responseTask.Should().NotBeNull();
        responseTask!.Title.Should().Be("Updated Title");
        responseTask.Note.Should().Be("Updated Note");
        responseTask.EstimatedPomos.Should().Be(5);
        responseTask.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTask_正常系_部分更新()
    {
        // Given: 存在するタスクと部分的な更新データ
        var existingTask = TestDataBuilder.CreateTask(1, "Original Title", "Original Note", 3, false, DateTime.UtcNow);
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Title",
            Note = "Original Note", // 変更なし
            EstimatedPomos = 3, // 変更なし
            IsArchived = false // 変更なし
        };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTask);
        _mockTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<Models.Task>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: タイトルのみを更新する
        var result = await _controller.UpdateTask(1, updateDto);

        // Then: 200 OKでタスクが更新される
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseTask = okResult?.Value as TaskResponseDto;
        responseTask.Should().NotBeNull();
        responseTask!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateTask_異常系_存在しないID()
    {
        // Given: 存在しないタスクID
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Title",
            Note = "Updated Note",
            EstimatedPomos = 5,
            IsArchived = false
        };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Models.Task?)null);

        // When: 存在しないIDでUpdateTaskを呼び出す
        var result = await _controller.UpdateTask(999, updateDto);

        // Then: 404 NotFoundが返却される
        result.Result.Should().BeOfType<NotFoundResult>();
    }


    #endregion

    #region DeleteTask Tests

    [Fact]
    public async Task DeleteTask_正常系_存在するIDで削除()
    {
        // Given: 存在するタスクID
        var task = TestDataBuilder.CreateTask(1, "Test Task", "Test Note", 3, false, DateTime.UtcNow);
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);
        _mockTaskRepository.Setup(x => x.DeleteAsync(It.IsAny<Models.Task>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // When: 存在するIDでDeleteTaskを呼び出す
        var result = await _controller.DeleteTask(1);

        // Then: 204 NoContentが返却される
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTask_異常系_存在しないID()
    {
        // Given: 存在しないタスクID
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Models.Task?)null);

        // When: 存在しないIDでDeleteTaskを呼び出す
        var result = await _controller.DeleteTask(999);

        // Then: 404 NotFoundが返却される
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}