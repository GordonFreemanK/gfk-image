using System.Text;

namespace cmd;

public class ScriptBuilder
{
    public string GetScript(IEnumerable<string> commandArguments)
    {
        var script = new StringBuilder();

        var newLine = true;
        foreach (var commandArgument in commandArguments)
        {
            if (string.Equals(commandArgument, "/C", StringComparison.OrdinalIgnoreCase) && script.Length == 0)
                continue;

            if (commandArgument == "&")
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