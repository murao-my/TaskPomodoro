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

public class SummaryControllerTests
{
    private readonly Mock<IRepository<Session>> _mockSessionRepository;
    private readonly SummaryController _controller;

    public SummaryControllerTests()
    {
        _mockSessionRepository = new Mock<IRepository<Session>>();
        _controller = new SummaryController(_mockSessionRepository.Object);
    }

    #region Get Tests

    // 注意: Get_正常系_* テストは統合テスト（SummaryControllerIntegrationTests.cs）に移行済み
    // 理由: IQueryable + ToListAsync()を使用するため

    [Theory]
    [InlineData("", "2024-01-01")]
    [InlineData("2024-01-01", "")]
    [InlineData("invalid", "2024-01-01")]
    [InlineData("2024-01-01", "invalid")]
    [InlineData("2024-13-01", "2024-01-01")]
    [InlineData("2024-01-01", "2024-01-32")]
    public async Task Get_異常系_無効な日付形式(string fromDate, string toDate)
    {
        // Given: 無効な日付形式
        _mockSessionRepository.Setup(x => x.Query()).Returns(new List<Session>().AsQueryable());

        // When: 無効な日付で集計を取得する
        var result = await _controller.Get(fromDate, toDate);

        // Then: 400 BadRequestが返却される（実装は"Invalid date format. Use YYYY-MM-DD."を返す）
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("Invalid date format. Use YYYY-MM-DD.");
    }

    [Fact]
    public async Task Get_異常系_from日付がto日付より後()
    {
        // Given: from日付がto日付より後
        _mockSessionRepository.Setup(x => x.Query()).Returns(new List<Session>().AsQueryable());

        // When: 無効な日付順序で集計を取得する
        var result = await _controller.Get("2024-01-03", "2024-01-01");

        // Then: 400 BadRequestが返却される（実装は"'to' must be greater than or equal to 'from'."を返す）
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("'to' must be greater than or equal to 'from'.");
    }


    #endregion
}
