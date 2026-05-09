namespace GamingMode.Services;

public sealed class AppPaths
{
    private AppPaths(string configDirectory)
    {
        ConfigDirectory = configDirectory;
        ConfigPath = Path.Combine(configDirectory, "config.json");
        StatePath = Path.Combine(configDirectory, "state.json");
        LogPath = Path.Combine(configDirectory, "agent.log");
    }

    public string ConfigDirectory { get; }

    public string ConfigPath { get; }

    public string StatePath { get; }

    public string LogPath { get; }

    public static AppPaths Create()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new AppPaths(Path.Combine(appData, "GamingMode"));
    }
}

