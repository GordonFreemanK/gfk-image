using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GFK.Image.cmd;

string[] envNames = ["TITLE", "COMMENTS", "COLORLABEL", "PICKLABEL", "RATING", "TAGSPATH"];

var applicationDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "GFK",
    "GFK.Image.cmd");

Directory.CreateDirectory(applicationDirectory);
var logPath = Path.Combine(applicationDirectory, "cmd.log");
var logger = new Logger(logPath);

try
{
    await logger.WriteAsync($"Invocation arguments: {string.Join(" ", args)}");
    await logger.WriteAsync($"digiKam environment: {string.Join(", ", envNames.Select(name => $"{name}={Environment.GetEnvironmentVariable(name)}"))}");

    if (args.Length != 2 || !string.Equals(args[0], "/C", StringComparison.OrdinalIgnoreCase))
    {
        await logger.WriteAsync("Expected /C followed by a digiKam temporary script path, got: " + string.Join(" ", args));
        return -1;
    }

    var script = File.ReadAllText(args[1]);

    var processStartInfo = new ProcessStartInfo
    {
        FileName = "pwsh.exe",
        ArgumentList = { "-Command", script },
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    using var process = new Process { StartInfo = processStartInfo };

    process.Start();

    await process.WaitForExitAsync();

    await logger.WriteAsync(await process.StandardError.ReadToEndAsync());

    // By default PowerShell returns 0 for success and 1 for failure (can be overriden in code)
    // digiKam interprets -1 as a failure and anything else a success
    // This ensures that digiKam knows when the script fails
    return process.ExitCode == 0 ? 0 : -1;
}
catch (Exception e)
{
    await logger.WriteAsync($"Uncaught exception: {e.Message}");
    return -1;
}