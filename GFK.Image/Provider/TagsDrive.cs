using System.Collections.Generic;
using System.Management.Automation;

namespace GFK.Image.Provider;

public class TagsDrive : PSDriveInfo, ITagsRepository
{
    private readonly IPathCleaner _pathCleaner;
    private readonly ITagsRepository _tagsRepository;

    public TagsDrive(PSDriveInfo driveInfo) : base(driveInfo)
    {
        var itemSeparator = driveInfo.Provider.ItemSeparator;
        var root = driveInfo.Root.TrimEnd(itemSeparator);
        _pathCleaner = new PathCleaner(root, itemSeparator);
        _tagsRepository = new TagsRepository(root, itemSeparator);
    }

    public Tag AddTag(string? path)
    {
        path = _pathCleaner.CleanInput(path);

        return _tagsRepository.AddTag(path);
    }

    public Tag? GetTag(string? path)
    {
        path = _pathCleaner.CleanInput(path);

        return _tagsRepository.GetTag(path);
    }

    public bool ItemExists(string? path)
    {
        path = _pathCleaner.CleanInput(path);

        return _tagsRepository.ItemExists(path);
    }

    public IReadOnlyCollection<Tag> GetChildTags(string? path, uint? depth)
    {
        path = _pathCleaner.CleanInput(path);

        return _tagsRepository.GetChildTags(path, depth);
    }

    public string MakePath(string? parent, string? child)
    {
        parent = _pathCleaner.CleanInput(parent);
        child = _pathCleaner.CleanInput(child);

        var result = _tagsRepository.MakePath(parent, child);

        return _pathCleaner.CleanOutput(result);
    }

    public string GetParentPath(string? path)
    {
        path = _pathCleaner.CleanInput(path);

        var result = _tagsRepository.GetParentPath(path);

        return _pathCleaner.CleanOutput(result);
    }

    public string GetChildName(string? path)
    {
        path = _pathCleaner.CleanInput(path);

        var result = _tagsRepository.GetChildName(path);

        return _pathCleaner.CleanOutput(result);
    }
}