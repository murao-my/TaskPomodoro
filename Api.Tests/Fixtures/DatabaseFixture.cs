using Microsoft.EntityFrameworkCore;
using TaskPomodoro.Api.Data;
using System;

namespace TaskPomodoro.Api.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public AppDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var testDbPath = $"test_{Guid.NewGuid()}.db";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={testDbPath};Cache=Shared;")
            .Options;
        DbContext = new AppDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }
    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        // Note: Test database file cleanup is handled by individual tests
        // since each test creates its own unique database file
    }
}