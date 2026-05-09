using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GamingMode.Models;

namespace GamingMode.Services;

public static class AgentHost
{
    public static async Task RunAsync(AppPaths paths, FileLogger logger, string[] args)
    {
        using var mutex = new Mutex(initiallyOwned: true, "GamingMode.Agent", out var ownsMutex);
        if (!ownsMutex)
        {
            logger.Info("Agent is already running.");
            return;
        }

        var store = new JsonStore(paths, logger);
        var processTools = new ProcessTools(logger);
        var shellTools = new ShellTools(logger);
        using var cursorAutoHide = new CursorAutoHideService(logger);
        using var windowFocus = new GamingWindowFocusService(logger);
        var manager = new ModeManager(paths, store, processTools, shellTools, cursorAutoHide, windowFocus, logger);
        processTools.CleanupDeckyOrphanedForks();
        var config = store.LoadConfig();
        var bindAddress = config.Safety.AllowRemoteApi ? "0.0.0.0" : "127.0.0.1";
        var url = $"http://{bindAddress}:{config.Safety.ApiPort}";
        var isShellHost = args.Any(arg => arg.Equals("shell", StringComparison.OrdinalIgnoreCase));
        var shouldApplyBootMode = isShellHost ||
            args.Any(arg => arg.Equals("--boot", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("boot", StringComparison.OrdinalIgnoreCase));

        if (shouldApplyBootMode)
        {
            try
            {
                await manager.ApplyBootModeAsync(isShellHost);
            }
            catch (Exception exception)
            {
                logger.Error("Boot mode application failed. Continuing agent startup in Desktop-safe state.", exception);
            }
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.WebHost.UseUrls(url);
        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,OPTIONS";
            context.Response.Headers["Access-Control-Allow-Headers"] = "content-type";

            if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next();
        });

        app.MapGet("/health", () => Results.Text("ok"));
        app.MapGet("/status", () => Results.Json(manager.GetStatus()));
        app.MapPost("/mode/gaming", async () => Results.Json(await manager.ApplyModeAsync(ModeKind.Gaming, "Applied Gaming Mode")));
        app.MapPost("/mode/desktop", async () => Results.Json(await manager.ApplyModeAsync(ModeKind.Desktop, "Applied Desktop Mode")));
        app.MapPost("/mode/gaming/switch", () => Results.Json(manager.SwitchToMode(ModeKind.Gaming)));
        app.MapPost("/mode/desktop/switch", () => Results.Json(manager.SwitchToMode(ModeKind.Desktop)));
        app.MapPost("/mode/gaming/restart", () => Results.Json(manager.RestartInMode(ModeKind.Gaming)));
        app.MapPost("/mode/desktop/restart", () => Results.Json(manager.RestartInMode(ModeKind.Desktop)));
        app.MapPost("/default/gaming", () => Results.Json(manager.SetDefaultMode(ModeKind.Gaming)));
        app.MapPost("/default/desktop", () => Results.Json(manager.SetDefaultMode(ModeKind.Desktop)));
        app.MapPost("/restart/steam", async () => Results.Json(await manager.RestartSteamAsync()));
        app.MapPost("/restart/decky", async () => Results.Json(await manager.RestartDeckyAsync()));
        app.MapPost("/cursor/autohide/start", () =>
        {
            var config = store.LoadConfig();
            cursorAutoHide.Start(config.Gaming.AutoHideMouseCursorAfterMs);
            return Results.Json(ApiResult.Success("Mouse cursor auto-hide enabled.", manager.GetStatus()));
        });
        app.MapPost("/cursor/autohide/stop", () =>
        {
            cursorAutoHide.Stop();
            return Results.Json(ApiResult.Success("Mouse cursor restored.", manager.GetStatus()));
        });

        app.Lifetime.ApplicationStopped.Register(() => logger.Info("Agent stopped."));
        app.Lifetime.ApplicationStopping.Register(cursorAutoHide.Stop);
        app.Lifetime.ApplicationStopping.Register(windowFocus.Stop);
        _ = Task.Run(async () =>
        {
            try
            {
                await manager.RunSafetyWatchdogAsync(app.Lifetime.ApplicationStopping);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                logger.Error("Safety watchdog crashed.", exception);
            }
        });

        try
        {
            logger.Info($"Agent listening on {url}.");
            await app.RunAsync();
        }
        catch (Exception exception)
        {
            logger.Error("Agent host crashed.", exception);
            throw;
        }
    }
}
