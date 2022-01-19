using System.Diagnostics;
using cmd;

var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cmd.log");
var logger = new Logger(logPath);

try
{
    var script = ScriptBuilder.GetScript(args);

    var processStartInfo = new ProcessStartInfo
    {
        FileName = "pwsh.exe",
        ArgumentList = { "-Command", script },
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    var environmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
    foreach (string? key in environmentVariables.Keys)
    {
        if (key == null)
            continue;
        processStartInfo.EnvironmentVariables[key] = (string?)environmentVariables[key];
    }

    using var process = new Process { StartInfo = processStartInfo };

    process.Start();

    process.WaitForExit();

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