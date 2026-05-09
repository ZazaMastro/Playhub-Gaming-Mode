using System.Text.Json;
using System.Text.Json.Nodes;

namespace GamingModeSetup.Services;

public static class ModeConfigWriter
{
    public static void SetDefaultMode(
        string mode,
        bool hideDesktopShellInGamingMode,
        bool ensureInputCompatibility,
        bool autoHideMouseCursor)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GamingMode");
        var path = Path.Combine(directory, "config.json");

        Directory.CreateDirectory(directory);

        JsonObject root;
        if (File.Exists(path))
        {
            try
            {
                root = JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? CreateDefaultConfig();
            }
            catch
            {
                root = CreateDefaultConfig();
            }
        }
        else
        {
            root = CreateDefaultConfig();
        }

        root["defaultMode"] = mode;
        root["nextBootMode"] = null;

        var gaming = root["gaming"] as JsonObject ?? new JsonObject();
        root["gaming"] = gaming;
        gaming["closeExplorerInGamingMode"] = hideDesktopShellInGamingMode;
        gaming["allowExplorerCloseInGamingMode"] = hideDesktopShellInGamingMode;
        gaming["ensureInputCompatibilityInGamingMode"] = ensureInputCompatibility;
        gaming["ensureSunshineCompatibilityInGamingMode"] = true;
        gaming["autoHideMouseCursorInGamingMode"] = autoHideMouseCursor;
        gaming["autoHideMouseCursorAfterMs"] = 2200;
        gaming["borderlessFullscreenWindowsInGamingMode"] = true;
        gaming["restoreStartupAppsOnDesktop"] = false;
        gaming["openSteamDesktopOnInteractiveDesktopMode"] = false;

        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private static JsonObject CreateDefaultConfig()
        => new()
        {
            ["defaultMode"] = "Desktop",
            ["nextBootMode"] = null,
            ["gaming"] = new JsonObject
            {
                ["steamPath"] = null,
                ["steamArguments"] = "-gamepadui",
                ["deckyPath"] = null,
                ["sunshinePath"] = null,
                ["deckyRequired"] = true,
                ["sunshineRequired"] = true,
                ["delaySteamAfterDeckyMs"] = 1500,
                ["closeExplorerInGamingMode"] = true,
                ["allowExplorerCloseInGamingMode"] = true,
                ["restoreExplorerOnDesktop"] = true,
                ["restoreStartupAppsOnDesktop"] = false,
                ["openSteamDesktopOnInteractiveDesktopMode"] = false,
                ["ensureInputCompatibilityInGamingMode"] = true,
                ["ensureSunshineCompatibilityInGamingMode"] = true,
                ["autoHideMouseCursorInGamingMode"] = true,
                ["autoHideMouseCursorAfterMs"] = 2200,
                ["borderlessFullscreenWindowsInGamingMode"] = true,
                ["manageAudio"] = false
            },
            ["safety"] = new JsonObject
            {
                ["apiPort"] = 47991,
                ["allowRemoteApi"] = false,
                ["restartWithoutPrompt"] = true
            }
        };
}
