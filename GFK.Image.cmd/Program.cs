using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GFK.Image.cmd;

var applicationDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "GFK",
    "GFK.Image.cmd");
Directory.CreateDirectory(applicationDirectory);
var logPath = Path.Combine(applicationDirectory, "cmd.log");
var logger = new Logger(logPath);

try
{
    await logger.WriteAsync(
        $"Invocation\n" +
        $"Wrapper: {Environment.ProcessPath}\n" +
        $"Working directory: {Environment.CurrentDirectory}\n" +
        $"Arguments: {JsonSerializer.Serialize(args)}");

    var script = ScriptBuilder.GetScript(args);
    await logger.WriteAsync($"Reconstructed PowerShell script:\n{script}");

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

    var standardOutputTask = process.StandardOutput.ReadToEndAsync();
    var standardErrorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    await Task.WhenAll(standardOutputTask, standardErrorTask);
    await logger.WriteAsync($"pwsh stdout:\n{standardOutputTask.Result}");
    await logger.WriteAsync($"pwsh stderr:\n{standardErrorTask.Result}");
    await logger.WriteAsync($"pwsh exit code: {process.ExitCode}");

    // By default PowerShell returns 0 for success and 1 for failure (can be overriden in code)
    // digiKam interprets -1 as a failure and anything else a success
    // This ensures that digiKam knows when the script fails
    return process.ExitCode == 0 ? 0 : -1;
}
catch (Exception e)
{
    await logger.WriteAsync($"Uncaught exception:\n{e}");
    return -1;
}