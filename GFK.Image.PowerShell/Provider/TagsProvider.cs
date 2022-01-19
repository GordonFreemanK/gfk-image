using System.Linq;
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

        protected override bool ItemExists(string path)
        {
            var isValidPath = TagsDrive.Repository.Exists(path);

            WriteWarning($"{nameof(ItemExists)}(\"{path}\") => {isValidPath}");

            return isValidPath;
        }

        protected override bool IsItemContainer(string path)
        {
            var isValidPath = TagsDrive.Repository.Exists(path);

            WriteWarning($"{nameof(IsItemContainer)}(\"{path}\") => {isValidPath}");

            return isValidPath;
        }

        protected override void MoveItem(string path, string destination)
        {
            WriteWarning(nameof(MoveItem));

            base.MoveItem(path, destination);
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            WriteWarning($"{nameof(NewItem)}(\"{path}\",\"{itemTypeName}\",\"{newItemValue}\")");

            TagsDrive.Repository.Add(path);

            WriteItemObject(new PSObject(path), path, true);
        }

        protected override void GetChildItems(string path, bool recurse, uint depth)
        {
            var tags = TagsDrive.Repository.Get(path, recurse ? depth : 0);

            WriteWarning(
                $"{nameof(GetChildItems)}(\"{path}\",{recurse},{depth}) => {string.Join(",", tags.Select(tag => $"\"{tag}\""))}");

            foreach (var tag in tags)
            {
                WriteItemObject(new PSObject(tag), tag, true);
            }
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            var tags = TagsDrive.Repository.Get(path, recurse ? default : (uint)0);

            WriteWarning(
                $"{nameof(GetChildItems)}(\"{path}\",{recurse}) => {string.Join(",", tags.Select(tag => $"\"{tag}\""))}");

            foreach (var tag in tags)
            {
                WriteItemObject(new PSObject(tag), tag, true);
            }
        }
    }
}