using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GFK.Image.cmd;

public partial class Logger(string path)
{
    public Task WriteAsync(string message)
    {
        if (message == string.Empty)
            return Task.CompletedTask;

        using var mutex = new Mutex(false, typeof(Logger).FullName);
        var ownsMutex = false;

        try
        {
            try
            {
                ownsMutex = mutex.WaitOne(TimeSpan.FromMinutes(1));
            }
            catch (AbandonedMutexException)
            {
                ownsMutex = true;
            }

            if (!ownsMutex)
                throw new TimeoutException("Timed out waiting to write to the log.");

            using var log = new StreamWriter(path, true);
            log.WriteLine($"{DateTime.UtcNow:s}");
            log.WriteLine(AnsiEscapeSequence().Replace(message, string.Empty));
        }
        finally
        {
            if (ownsMutex)
                mutex.ReleaseMutex();
        }

        return Task.CompletedTask;
    }

    [GeneratedRegex(@"\x1B\[[0-?]*[ -/]*[@-~]")]
    private static partial Regex AnsiEscapeSequence();
}