using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GamingMode.Models;

namespace GamingMode.Services;

public sealed class AgentClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly int _port;

    public AgentClient(int port = 47991)
    {
        _port = port;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://127.0.0.1:{port}"),
            Timeout = TimeSpan.FromSeconds(2)
        };
    }

    public async Task<bool> EnsureAgentRunningAsync()
    {
        if (await IsAgentRunningAsync())
        {
            return true;
        }

        var exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe))
        {
            return false;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            Arguments = "agent",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = AppContext.BaseDirectory
        });

        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(250);
            if (await IsAgentRunningAsync())
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> IsAgentRunningAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ModeStatus?> GetStatusAsync()
    {
        return await _httpClient.GetFromJsonAsync<ModeStatus>("/status", JsonOptions);
    }

    public Task<ApiResult?> ApplyGamingModeAsync() => PostAsync("/mode/gaming");

    public Task<ApiResult?> ApplyDesktopModeAsync() => PostAsync("/mode/desktop");

    public Task<ApiResult?> SwitchToGamingModeAsync() => PostAsync("/mode/gaming/switch");

    public Task<ApiResult?> SwitchToDesktopModeAsync() => PostAsync("/mode/desktop/switch");

    public Task<ApiResult?> RestartInGamingModeAsync() => PostAsync("/mode/gaming/restart");

    public Task<ApiResult?> RestartInDesktopModeAsync() => PostAsync("/mode/desktop/restart");

    public Task<ApiResult?> SetDefaultGamingAsync() => PostAsync("/default/gaming");

    public Task<ApiResult?> SetDefaultDesktopAsync() => PostAsync("/default/desktop");

    public Task<ApiResult?> RestartSteamAsync() => PostAsync("/restart/steam");

    public Task<ApiResult?> RestartDeckyAsync() => PostAsync("/restart/decky");

    public Task<ApiResult?> StartCursorAutoHideAsync() => PostAsync("/cursor/autohide/start");

    public Task<ApiResult?> StopCursorAutoHideAsync() => PostAsync("/cursor/autohide/stop");

    private async Task<ApiResult?> PostAsync(string path)
    {
        using var response = await _httpClient.PostAsync(path, content: null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiResult>(JsonOptions);
    }
}
