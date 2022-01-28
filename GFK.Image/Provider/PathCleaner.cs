using System.IO;
using System.Management.Automation.Provider;

namespace GFK.Image.Provider;

public interface IPathCleaner
{
    string CleanInput(string? path);
    string CleanOutput(string path);
}

public class PathCleaner : IPathCleaner
{
    private readonly string _root;
    private readonly char _itemSeparator;

    public PathCleaner(string root, char itemSeparator)
    {
        _root = root;
        _itemSeparator = itemSeparator;
    }
    
    /// <summary>
    /// Cleans paths sent by PowerShell to the PSProvider
    /// </summary>
    /// <remarks>
    /// The only task is to fix the root:
    /// On Windows machines, PowerShell (version 7.2) is pretty set on backslash being a separator, but we need the
    /// forward slash to be a separator and backslash to be a valid character in a tag name (as is the case in digiKam).
    /// In the base <see cref="NavigationCmdletProvider"/> implementation, the value used by most of the virtual
    /// methods is <see cref="Path.DirectorySeparatorChar"/> which cannot be modified.
    /// There is an expectation that in the future this would not be the case anymore since PSProvider has the
    /// <see cref="NavigationCmdletProvider.ItemSeparator"/> property but this seems to have little effect in PS 7.2
    /// Meanwhile even though a lot of the <see cref="NavigationCmdletProvider"/> implementation has been overriden
    /// in <see cref="TagsProvider"/> to make use of this property, there are some cases where PS still sends strange
    /// paths to those implementations. For instance if the commands "cd Tags:/Tag1/; cd C:; cd Tags:",
    /// <see cref="TagsProvider.GetChildName"/> receives a path of Tags:\/Tag1. This method aims at removing those rogue
    /// backslashes from the drive name, while making sure that backslash remains a valid tag name character. 
    /// </remarks>
    public string CleanInput(string? path)
    {
        if (path == null)
            return string.Empty;

        var brokenRoot = $@"{_root}\";
        return path.StartsWith(brokenRoot)
            ? $"{_root}{_itemSeparator}{path[brokenRoot.Length..].TrimStart(_itemSeparator)}"
            : path;
    }
    
    /// <summary>
    /// Cleans the paths sent by the PSProvider to PowerShell
    /// </summary>
    /// <remarks>
    /// Only task is to ensure that if the path is the drive root it ends with a separator
    /// </remarks>
    public string CleanOutput(string path)
    {
        return path == _root ? $"{_root}{_itemSeparator}" : path;
    }
}