using System.Management.Automation;

namespace GFK.Image.Provider;

public class TagsDrive : PSDriveInfo
{
    public TagsDrive(PSDriveInfo driveInfo) : base(driveInfo)
    {
        PathMaker = new PathMaker(driveInfo.Provider.ItemSeparator, driveInfo.Root);
        Repository = new TagsRepository(driveInfo.Provider.ItemSeparator);
    }

    public IPathMaker PathMaker { get; }
    public ITagsRepository Repository { get; }
}