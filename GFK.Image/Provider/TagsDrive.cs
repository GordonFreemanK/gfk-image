using System.Management.Automation;

namespace GFK.Image.Provider;

public interface ITagsDrive
{
    IPathCleaner PathCleaner { get; }
    ITagsRepository Repository { get; }
}

public class TagsDrive : PSDriveInfo, ITagsDrive
{
    public TagsDrive(PSDriveInfo driveInfo, char itemSeparator) : base(driveInfo)
    {
        var root = driveInfo.Root.TrimEnd(itemSeparator);
        PathCleaner = new PathCleaner(root, itemSeparator);
        Repository = new TagsRepository(root, itemSeparator);
    }

    public IPathCleaner PathCleaner { get; }
    public ITagsRepository Repository { get; }
}