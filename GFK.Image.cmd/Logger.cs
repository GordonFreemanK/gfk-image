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

    public async Task WriteAsync(string message)
    {
        if (message != string.Empty)
        {
            using var mutex = new Mutex(false, typeof(Logger).FullName);

            mutex.WaitOne(TimeSpan.FromMinutes(1));
            
            await using var log = new StreamWriter(_path, true);

            await log.WriteLineAsync($"{DateTime.UtcNow:s}");
            await log.WriteLineAsync(message);
        }
    }
}