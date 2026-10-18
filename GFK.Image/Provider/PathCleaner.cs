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
    /// On Windows, navigation can supply the drive root with a backslash before the provider's forward-slash
    /// separator. For example, changing from Tags:/Tag1/ to C: and back to Tags: has been observed to pass
    /// Tags:\/Tag1 to provider methods. Normalize only this root prefix so backslashes inside tag names remain intact.
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