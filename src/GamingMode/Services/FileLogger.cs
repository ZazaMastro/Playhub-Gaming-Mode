namespace GamingMode.Services;

public sealed class FileLogger
{
    private readonly string _path;
    private readonly object _sync = new();

    public FileLogger(string path)
    {
        _path = path;
    }

    public void Info(string message) => Write("INFO", message);

    public void Error(string message, Exception? exception = null)
    {
        var fullMessage = exception is null ? message : $"{message}: {exception}";
        Write("ERROR", fullMessage);
    }

    private void Write(string level, string message)
    {
        lock (_sync)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.AppendAllText(
                _path,
                $"{DateTimeOffset.Now:O} [{level}] {message}{Environment.NewLine}");
        }
    }
}

