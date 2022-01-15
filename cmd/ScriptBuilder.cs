using System.Text;

namespace cmd;

public static class ScriptBuilder
{
    public static string GetScript(IEnumerable<string> commandArguments)
    {
        var script = new StringBuilder();

        var newLine = true;
        foreach (var commandArgument in commandArguments)
        {
            if (string.Equals(commandArgument, "/C", StringComparison.OrdinalIgnoreCase) && script.Length == 0)
                continue;

            if (string.Equals(commandArgument, "&", StringComparison.OrdinalIgnoreCase))
            {
                script.AppendLine();
                newLine = true;
            }
            else
            {
                if (!newLine) script.Append(' ');
                script.Append(commandArgument);
                newLine = false;
            }
        }

        return script.ToString();
    }
}