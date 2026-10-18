using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GFK.Image.cmd;

public class Logger
{
    private readonly string _path;

    public Logger(string path)
    {
        _path = path;
    }

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

            using var log = new StreamWriter(_path, true);
            log.WriteLine($"{DateTime.UtcNow:s}");
            log.WriteLine(message);
        }
        finally
        {
            if (ownsMutex)
                mutex.ReleaseMutex();
        }

        return Task.CompletedTask;
    }
}