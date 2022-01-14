namespace cmd;

public class Logger
{
    private readonly string _path;

    public Logger(string path)
    {
        _path = path;
    }

    public async Task WriteLineAsync(string message)
    {
        await using var log = new StreamWriter(_path, true);

        await log.WriteLineAsync($"{DateTime.UtcNow:s}\t{message}");
    }
}