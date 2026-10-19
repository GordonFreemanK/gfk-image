using System;
using System.Collections.Generic;
using System.IO;

namespace GFK.Image.cmd;

public static class ScriptBuilder
{
    public static string GetScript(IEnumerable<string> commandArguments)
    {
        using var argumentEnumerator = commandArguments.GetEnumerator();
        if (!argumentEnumerator.MoveNext() ||
            !string.Equals(argumentEnumerator.Current, "/C", StringComparison.OrdinalIgnoreCase) ||
            !argumentEnumerator.MoveNext())
        {
            throw new ArgumentException(
                "Expected /C followed by a digiKam temporary script path.",
                nameof(commandArguments));
        }

        var scriptPath = argumentEnumerator.Current;
        if (argumentEnumerator.MoveNext())
        {
            throw new ArgumentException(
                "Expected exactly one digiKam temporary script path.",
                nameof(commandArguments));
        }

        return File.ReadAllText(scriptPath);
    }
}
