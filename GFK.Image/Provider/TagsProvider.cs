using System.Management.Automation;
using System.Management.Automation.Provider;

namespace GFK.Image.Provider;

[CmdletProvider("Tags", ProviderCapabilities.None)]
public class TagsProvider : NavigationCmdletProvider
{
    private const char Separator = '/';

    public override char ItemSeparator => Separator;

    public override char AltItemSeparator => Separator;

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        WriteVerbose($"Root: {drive.Root}");
        return new TagsDrive(drive, Separator);
    }

    protected override bool IsValidPath(string path) => true;

    protected override bool ItemExists(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return TagsDrive.Repository.IsPathValid(cleanPath);
    }

    protected override bool IsItemContainer(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return TagsDrive.Repository.IsPathValid(cleanPath);
    }

    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        var tag = TagsDrive.Repository.AddTag(cleanPath);
        WriteItemObject(tag, tag.Path, true);
    }

    protected override void GetItem(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        var tag = TagsDrive.Repository.GetTag(cleanPath);
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
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        var childTags = TagsDrive.Repository.GetChildTags(cleanPath, depth);
        foreach (var tag in childTags)
        {
            WriteItemObject(tag, tag.Path, true);
        }
    }

    protected override string MakePath(string? parent, string? child)
    {
        var cleanParent = TagsDrive.PathMaker.FixRoot(parent);
        var cleanChild = TagsDrive.PathMaker.FixRoot(child);
        return TagsDrive.PathMaker.MakePath(cleanParent, cleanChild);
    }

    protected override string NormalizeRelativePath(string path, string basePath)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        var cleanBasePath = TagsDrive.PathMaker.FixRoot(basePath);
        return base.NormalizeRelativePath(cleanPath, cleanBasePath);
    }

    protected override string GetParentPath(string? path, string? root)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return TagsDrive.PathMaker.GetParentPath(cleanPath);
    }

    protected override string GetChildName(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return TagsDrive.PathMaker.GetChildName(cleanPath);
    }

    private TagsDrive TagsDrive => (TagsDrive)PSDriveInfo;
}