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
    
    public string MakePath(string? parent, string? child)
    {
        if (string.IsNullOrEmpty(child))
            return EnsureRootSeparator(parent);

        if (string.IsNullOrEmpty(parent) || child.StartsWith(_root.TrimEnd(_separator)))
            return EnsureRootSeparator(child);

        return $"{parent.TrimEnd(_separator)}{_separator}{child.TrimStart(_separator)}";
    }

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

    public string FixRoot(string? path)
    {
        if (path == null)
            return string.Empty;

        var cleanRoot = _root.TrimEnd(_separator);
        var brokenRoot = $@"{cleanRoot}\";
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