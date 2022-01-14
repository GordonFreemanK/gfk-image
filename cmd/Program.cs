using System.Diagnostics;
using cmd;

var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cmd.log");
var logger = new Logger(logPath);

try
{
    var scriptBuilder = new ScriptBuilder();
    var script = scriptBuilder.GetScript(args);

    var processStartInfo = new ProcessStartInfo
    {
        FileName = "pwsh.exe",
        ArgumentList = { "-Command", script },
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    var environmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
    foreach (string key in environmentVariables.Keys)
    {
        processStartInfo.EnvironmentVariables[key] = (string?)environmentVariables[key];
    }

    using var process = new Process { StartInfo = processStartInfo };

    process.Start();

    await process.WaitForExitAsync();

    await logger.WriteAsync(await process.StandardError.ReadToEndAsync());
}
catch (Exception e)
{
    await logger.WriteAsync($"Uncaught exception: {e.Message}");
}