using System.IO;
using System.Management.Automation.Provider;

namespace GFK.Image.Provider;

public interface IPathMaker
{
    string MakePath(string? parent, string? child);
    string GetParentPath(string? path);
    string GetChildName(string? path);
    string FixRoot(string? path);
}

public class PathMaker : IPathMaker
{
    private readonly char _separator;
    private readonly string _root;

    public PathMaker(char separator, string root)
    {
        _separator = separator;
        _root = root;
    }
    
    /// <summary>
    /// Creates a path from two paths
    /// Handles relative and absolute paths
    /// If the second path is absolute then the first path is irrelevant
    /// If a root path is returned it always end with a separator 
    /// </summary>
    public string MakePath(string? parent, string? child)
    {
        if (string.IsNullOrEmpty(child))
            return EnsureRootSeparator(parent);

        if (string.IsNullOrEmpty(parent) || child.StartsWith(_root.TrimEnd(_separator)))
            return EnsureRootSeparator(child);

        return $"{parent.TrimEnd(_separator)}{_separator}{child.TrimStart(_separator)}";
    }

    /// <summary>
    /// Gets the parent path
    /// Handles relative and absolute paths
    /// If already root returns the same path 
    /// If a root path is returned it always end with a separator 
    /// </summary>
    public string GetParentPath(string? path)
    {
        if (path == null) return string.Empty;

        var lastSeparator = path.TrimEnd(_separator).LastIndexOf(_separator);
        if (lastSeparator == 0)
            return _separator.ToString();

        var parent = lastSeparator > 0
            ? path[..lastSeparator]
            : path;
        return EnsureRootSeparator(parent);
    }

    /// <summary>
    /// Gets the name of the child item
    /// Handles relative and absolute paths
    /// If root returns the same path
    /// If a root path is returned it always end with a separator 
    /// </summary>
    public string GetChildName(string? path)
    {
        if (path == null) return string.Empty;

        var cleanPath = path.TrimEnd(_separator);
        if (cleanPath == string.Empty)
            return path;
        
        var lastSeparator = cleanPath.LastIndexOf(_separator);
        return lastSeparator >= 0
            ? cleanPath[(lastSeparator + 1)..]
            : EnsureRootSeparator(cleanPath);
    }

    /// <summary>
    /// Fix root path
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
    /// </summary>
    public string FixRoot(string? path)
    {
        if (path == null)
            return string.Empty;

        if (Path.DirectorySeparatorChar == _separator)
            return path;

        var cleanRoot = _root.TrimEnd(_separator);
        var brokenRoot = $@"{cleanRoot}{Path.DirectorySeparatorChar}";
        if (!path.StartsWith(brokenRoot))
            return path;

        var cleanPath = $"{cleanRoot}{_separator}{path[brokenRoot.Length..].TrimStart(_separator)}";
        return EnsureRootSeparator(cleanPath);
    }

    private string EnsureRootSeparator(string? path)
    {
        if (path == null) return string.Empty;

        var cleanPath = path.TrimEnd(_separator);
        var cleanRoot = _root.TrimEnd(_separator);
        return cleanPath == cleanRoot
            ? $"{cleanPath}{_separator}"
            : path;
    }
}