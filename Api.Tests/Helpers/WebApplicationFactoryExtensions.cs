using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskPomodoro.Api.Data;
using System;

namespace TaskPomodoro.Api.Tests.Helpers;

public static class WebApplicationFactoryExtensions
{
    public static WebApplicationFactory<Program> WithTestDatabase(this WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 本番のDbContext設定を削除
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // テスト用のDbContext設定を追加
                var testDbPath = $"test_{Guid.NewGuid()}.db";
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite($"Data Source={testDbPath};Cache=Shared;"));
            });
        });
    }
}