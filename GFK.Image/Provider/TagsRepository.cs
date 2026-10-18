using System.Collections.Generic;
using System.Linq;

namespace GFK.Image.Provider;

public interface ITagsRepository
{
    Tag AddTag(string path);
    Tag? GetTag(string path);
    bool ItemExists(string path);
    IReadOnlyCollection<Tag> GetChildTags(string path, uint? depth);
    string MakePath(string parent, string child);
    string GetParentPath(string path);
    string GetChildName(string path);
}

public class TagsRepository : ITagsRepository
{
    private readonly string _root;
    private readonly char _itemSeparator;
    private readonly List<string> _tags;

    public TagsRepository(string root, char itemSeparator)
    {
        _root = root;
        _itemSeparator = itemSeparator;
        _tags = new List<string>();
    }

    public Tag AddTag(string path)
    {
        path = path.TrimEnd(_itemSeparator);
        _tags.Add(path);
        return BuildTag(path);
    }

    public Tag? GetTag(string path)
    {
        path = path.TrimEnd(_itemSeparator);
        return _tags.Contains(path)
            ? BuildTag(path)
            : null;
    }

    public bool ItemExists(string path)
    {
        path = path.TrimEnd(_itemSeparator);
        return _tags.Any(tag => tag == path || tag.StartsWith($"{path}{_itemSeparator}"));
    }

    public IReadOnlyCollection<Tag> GetChildTags(string path, uint? depth)
    {
        path = path.TrimEnd(_itemSeparator) + _itemSeparator;
        return _tags
            .Where(tag => tag.StartsWith(path))
            .Select(tag => depth == null ? tag : GetPartialPath(tag, path.Length, depth.Value))
            .Distinct()
            .Select(BuildTag)
            .ToArray();
    }

    /// <summary>
    /// Creates a path from two paths
    /// Handles relative and absolute paths
    /// If the second path is absolute then the first path is ignored
    /// </summary>
    public string MakePath(string parent, string child)
    {
        return
            child == string.Empty ? parent
            : parent == string.Empty || child.StartsWith(_root) ? child
            : $"{parent.TrimEnd(_itemSeparator)}{_itemSeparator}{child.TrimStart(_itemSeparator)}";
    }

    /// <summary>
    /// Gets the parent path
    /// Handles relative and absolute paths
    /// If root returns the same path 
    /// </summary>
    public string GetParentPath(string path)
    {
        var trimEnd = path.TrimEnd(_itemSeparator);
        var lastSeparator = trimEnd.LastIndexOf(_itemSeparator);
        return lastSeparator switch
        {
            0 => _itemSeparator.ToString(),
            > 0 => path[..lastSeparator],
            _ => path
        };
    }

    /// <summary>
    /// Gets the name of the child item
    /// Handles relative and absolute paths
    /// If root returns the same path
    /// </summary>
    public string GetChildName(string path)
    {
        var cleanPath = path.TrimEnd(_itemSeparator);
        if (cleanPath == string.Empty)
            return path;

        var lastSeparator = cleanPath.LastIndexOf(_itemSeparator);
        return lastSeparator >= 0 ? cleanPath[(lastSeparator + 1)..] : cleanPath;
    }

    private Tag BuildTag(string path)
    {
        return new Tag(path, path.Split(_itemSeparator).Last());
    }

    private string GetPartialPath(string tag, int position, uint depth)
    {
        do position = tag.IndexOf(_itemSeparator, position + 1);
        while (position >= 0 && depth-- > 0);
        return position >= 0 ? tag[..position] : tag;
    }
}