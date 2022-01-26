using System.Collections.Generic;
using System.Linq;

namespace GFK.Image.Provider;

public interface ITagsRepository
{
    Tag AddTag(string path);
    Tag? GetTag(string path);
    bool IsPathValid(string path);
    IReadOnlyCollection<Tag> GetChildTags(string path, uint? depth);
}

public class TagsRepository : ITagsRepository
{
    private readonly char _itemSeparator;
    private readonly List<string> _tags;

    public TagsRepository(string root, char itemSeparator)
    {
        _itemSeparator = itemSeparator;
        _tags = new List<string> { root };
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
        
    public bool IsPathValid(string path)
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