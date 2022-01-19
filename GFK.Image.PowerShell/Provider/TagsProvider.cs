using System.Management.Automation;
using System.Management.Automation.Provider;

namespace GFK.Image.PowerShell.Provider
{
    [CmdletProvider("Tags", ProviderCapabilities.None)]
    public class TagsProvider : NavigationCmdletProvider
    {
        private TagsDrive TagsDrive => (TagsDrive)PSDriveInfo;

        protected override PSDriveInfo NewDrive(PSDriveInfo drive) => new TagsDrive(drive);

        protected override bool IsValidPath(string path) => true;

        protected override bool ItemExists(string path) => TagsDrive.Repository.IsPathValid(path);

        protected override bool IsItemContainer(string path) => TagsDrive.Repository.IsPathValid(path);
        
        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            var tag = TagsDrive.Repository.AddTag(path);

            WriteItemObject(tag, tag, true);
        }

        protected override void GetItem(string path)
        {
            var tag = TagsDrive.Repository.GetTag(path);
            if (tag != null)
            {
                WriteItemObject(new PSObject(tag), tag, true);
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
            foreach (var tag in TagsDrive.Repository.GetChildTags(path, depth))
            {
                WriteItemObject(tag, tag, true);
            }
        }
    }
}