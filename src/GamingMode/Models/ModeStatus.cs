namespace GamingMode.Models;

public sealed class ModeStatus
{
    public bool AgentRunning { get; set; }

    public ModeKind CurrentMode { get; set; }

    public ModeKind DefaultMode { get; set; }

    public ModeKind? NextBootMode { get; set; }

    public DateTimeOffset? LastAppliedAt { get; set; }

    public string? LastAction { get; set; }

    public string? LastError { get; set; }

    public ProcessState Steam { get; set; } = new();

    public ProcessState Decky { get; set; } = new();

    public ProcessState Sunshine { get; set; } = new();

    public ProcessState Explorer { get; set; } = new();

    public bool MouseCursorAutoHide { get; set; }

    public bool MouseCursorHidden { get; set; }

    public string ConfigPath { get; set; } = "";

    public string[] Messages { get; set; } = [];
}

public sealed class ProcessState
{
    public bool Running { get; set; }

    public int[] ProcessIds { get; set; } = [];

    public string? Path { get; set; }
}

public sealed class ApiResult
{
    public bool Ok { get; set; } = true;

    public string Message { get; set; } = "";

    public ModeStatus? Status { get; set; }

    public static ApiResult Success(string message, ModeStatus? status = null)
        => new() { Ok = true, Message = message, Status = status };

    public static ApiResult Failure(string message, ModeStatus? status = null)
        => new() { Ok = false, Message = message, Status = status };
}
