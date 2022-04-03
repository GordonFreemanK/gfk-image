using System.Management.Automation;
using System.Management.Automation.Provider;

namespace GFK.Image.Provider;

/// <summary>
/// This class aims at providing a basic implementation for a PowerShell drive that allows adding items as stored in the
/// digiKam domain (where the forward slash is the hierarchical separator and backslash is a valid tag name character)
/// Items can only be added and not renamed, moved or deleted.
/// Note that the expectations on the protected virtual methods are not always clear even though the default
/// implementation can be seen on the
/// <a href="https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/namespaces/NavigationProviderBase.cs">PowerShell repository</a>.
/// </summary>
[CmdletProvider("Tags", ProviderCapabilities.None)]
public class TagsProvider : NavigationCmdletProvider
{
    public override char ItemSeparator => '/';

    public override char AltItemSeparator => ItemSeparator;

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        return new TagsDrive(drive, ItemSeparator);
    }

    protected override bool IsValidPath(string path) => true;

    protected override bool IsItemContainer(string path) => ItemExists(path);

    protected override bool ItemExists(string path)
    {
        path = TagsDrive.PathCleaner.CleanInput(path);

        return TagsDrive.TagsRepository.ItemExists(path);
    }

    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        path = TagsDrive.PathCleaner.CleanInput(path);

        var tag = TagsDrive.TagsRepository.AddTag(path);

        WriteItemObject(tag, tag.Path, true);
    }

    protected override void GetItem(string path)
    {
        path = TagsDrive.PathCleaner.CleanInput(path);

        var tag = TagsDrive.TagsRepository.GetTag(path);

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
        path = TagsDrive.PathCleaner.CleanInput(path);

        var childTags = TagsDrive.TagsRepository.GetChildTags(path, depth);

        foreach (var tag in childTags)
        {
            WriteItemObject(tag, tag.Path, true);
        }
    }

    protected override string GetChildName(string path)
    {
        path = TagsDrive.PathCleaner.CleanInput(path);

        var result = TagsDrive.TagsRepository.GetChildName(path);

        return TagsDrive.PathCleaner.CleanOutput(result);
    }
    
    protected override string GetParentPath(string? path, string? root)
    {
        path = TagsDrive.PathCleaner.CleanInput(path);

        var result = TagsDrive.TagsRepository.GetParentPath(path);

        return TagsDrive.PathCleaner.CleanOutput(result);
    }

    protected override string MakePath(string? parent, string? child)
    {
        parent = TagsDrive.PathCleaner.CleanInput(parent);
        child = TagsDrive.PathCleaner.CleanInput(child);

        var result = TagsDrive.TagsRepository.MakePath(parent, child);

        return TagsDrive.PathCleaner.CleanOutput(result);
    }

    private ITagsDrive TagsDrive => (ITagsDrive)PSDriveInfo;
}