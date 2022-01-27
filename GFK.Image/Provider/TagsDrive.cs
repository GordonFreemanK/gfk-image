using System.Management.Automation;

namespace GFK.Image.Provider;

public class TagsDrive : PSDriveInfo
{
    public TagsDrive(PSDriveInfo driveInfo, char separator) : base(driveInfo)
    {
        PathMaker = new PathMaker(separator, driveInfo.Root);
        Repository = new TagsRepository(separator);
    }

    public IPathMaker PathMaker { get; }
    public ITagsRepository Repository { get; }
}