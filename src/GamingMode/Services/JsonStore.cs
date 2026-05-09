using System.Text.Json;
using System.Text.Json.Serialization;
using GamingMode.Models;

namespace GamingMode.Services;

public sealed class JsonStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AppPaths _paths;
    private readonly FileLogger _logger;

    public JsonStore(AppPaths paths, FileLogger logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public ModeConfig LoadConfig()
    {
        Directory.CreateDirectory(_paths.ConfigDirectory);

        if (!File.Exists(_paths.ConfigPath))
        {
            var config = new ModeConfig();
            SaveConfig(config);
            return config;
        }

        try
        {
            var json = File.ReadAllText(_paths.ConfigPath);
            var config = JsonSerializer.Deserialize<ModeConfig>(json, Options) ?? new ModeConfig();
            SaveConfig(config);
            return config;
        }
        catch (Exception exception)
        {
            _logger.Error("Config could not be loaded. Using defaults.", exception);
            return new ModeConfig();
        }
    }

    public void SaveConfig(ModeConfig config)
    {
        Directory.CreateDirectory(_paths.ConfigDirectory);
        File.WriteAllText(_paths.ConfigPath, JsonSerializer.Serialize(config, Options));
    }

    public ModeState LoadState()
    {
        if (!File.Exists(_paths.StatePath))
        {
            return new ModeState();
        }

        try
        {
            var json = File.ReadAllText(_paths.StatePath);
            return JsonSerializer.Deserialize<ModeState>(json, Options) ?? new ModeState();
        }
        catch (Exception exception)
        {
            _logger.Error("State could not be loaded. Using defaults.", exception);
            return new ModeState();
        }
    }

    public void SaveState(ModeState state)
    {
        Directory.CreateDirectory(_paths.ConfigDirectory);
        File.WriteAllText(_paths.StatePath, JsonSerializer.Serialize(state, Options));
    }
}
