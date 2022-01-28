using System;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace GFK.Image.Provider;

/// <summary>
/// This class aims at providing a basic implementation for a PowerShell drive that allows adding items as stored in the
/// digiKam domain (where the forward slash is the hierarchical separator and backslash is a valid tag name character)
/// Items can only be added and not renamed, moved or deleted.
/// Note that the expectations on the protected virtual methods are not always clear even though the default
/// implementation can be seen on the
/// <a href="https://github.com/PowerShell/PowerShell/blob/7dc4587014bfa22919c933607bf564f0ba53db2e/src/System.Management.Automation/namespaces/NavigationProviderBase.cs">PowerShell repository</a>.
/// </summary>
[CmdletProvider("Tags", ProviderCapabilities.None)]
public class TagsProvider : NavigationCmdletProvider
{
    private IPathCleaner? _pathCleaner;
    private ITagsRepository? _tagsRepository;
    
    public override char ItemSeparator => '/';

    public override char AltItemSeparator => ItemSeparator;

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        var root = drive.Root.TrimEnd(ItemSeparator);
        _pathCleaner = new PathCleaner(root, ItemSeparator);
        _tagsRepository = new TagsRepository(root, ItemSeparator);
        return new TagsDrive(drive);
    }

    protected override bool IsValidPath(string path) => true;

    protected override bool IsItemContainer(string path) => ItemExists(path);

    protected override bool ItemExists(string path)
    {
        path = PathCleaner.CleanInput(path);

        return TagsRepository.ItemExists(path);
    }

    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        path = PathCleaner.CleanInput(path);

        var tag = TagsRepository.AddTag(path);

        WriteItemObject(tag, tag.Path, true);
    }

    protected override void GetItem(string path)
    {
        path = PathCleaner.CleanInput(path);

        var tag = TagsRepository.GetTag(path);

        if (tag != null)
        {
            WriteItemObject(tag, tag.Path, true);
        }
    }

    protected override void GetChildItems(string path, bool recurse, uint depth)
    {
        GetChildItems(path, recurse ? depth : 0);
    }

    protected override void GetChildItems(string path, bool recurse)
    {
        GetChildItems(path, recurse ? default : (uint)0);
    }

    private void GetChildItems(string path, uint? depth)
    {
        path = PathCleaner.CleanInput(path);

        var childTags = TagsRepository.GetChildTags(path, depth);

        foreach (var tag in childTags)
        {
            WriteItemObject(tag, tag.Path, true);
        }
    }

    protected override string GetChildName(string path)
    {
        path = PathCleaner.CleanInput(path);

        var result = TagsRepository.GetChildName(path);

        return PathCleaner.CleanOutput(result);
    }
    
    protected override string GetParentPath(string? path, string? root)
    {
        path = PathCleaner.CleanInput(path);

        var result = TagsRepository.GetParentPath(path);

        return PathCleaner.CleanOutput(result);
    }

    protected override string MakePath(string? parent, string? child)
    {
        parent = PathCleaner.CleanInput(parent);
        child = PathCleaner.CleanInput(child);

        var result = TagsRepository.MakePath(parent, child);

        return PathCleaner.CleanOutput(result);
    }

    private IPathCleaner PathCleaner => _pathCleaner ?? throw new ArgumentNullException(nameof(PSDriveInfo));
    private ITagsRepository TagsRepository => _tagsRepository ?? throw new ArgumentNullException(nameof(PSDriveInfo));
}