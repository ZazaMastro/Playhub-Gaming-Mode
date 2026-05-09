namespace GamingModeSetup.Services;

public sealed class InstallResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = "";

    public string InstallDirectory { get; set; } = "";

    public static InstallResult Ok(string message, string installDirectory)
        => new() { Success = true, Message = message, InstallDirectory = installDirectory };

    public static InstallResult Fail(string message, string installDirectory = "")
        => new() { Success = false, Message = message, InstallDirectory = installDirectory };
}

