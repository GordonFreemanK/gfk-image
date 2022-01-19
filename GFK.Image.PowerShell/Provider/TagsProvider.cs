using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace GFK.Image.PowerShell.Provider
{
    [CmdletProvider("Tags", ProviderCapabilities.None)]
    public class TagsProvider : NavigationCmdletProvider
    {
        public const char ItemSeparator = '\\';

        private TagsDrive TagsDrive => (TagsDrive)PSDriveInfo;

        protected override PSDriveInfo NewDrive(PSDriveInfo drive) => new TagsDrive(drive);

        protected override bool IsValidPath(string path) => true;

        protected override bool ItemExists(string path)
        {
            var cleanPath = GetCleanPath(path);

            var isValidPath = TagsDrive.Repository.Exists(cleanPath);

            WriteWarning($"{nameof(ItemExists)}(\"{path}\") => {isValidPath}");

            return isValidPath;
        }

        protected override bool IsItemContainer(string path)
        {
            var cleanPath = GetCleanPath(path);

            var isValidPath = TagsDrive.Repository.Exists(cleanPath);

            WriteWarning($"{nameof(IsItemContainer)}(\"{path}\") => {isValidPath}");

            return isValidPath;
        }

        protected override void ClearItem(string path)
        {
            WriteWarning(nameof(ClearItem));

            base.ClearItem(path);
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            WriteWarning(nameof(CopyItem));

            base.CopyItem(path, copyPath, recurse);
        }

        protected override bool ConvertPath(
            string path,
            string filter,
            ref string updatedPath,
            ref string updatedFilter)
        {
            WriteWarning($"{nameof(ConvertPath)}(\"{path}\",\"{filter}\")");

            return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        }

        protected override string MakePath(string parent, string child)
        {
            var cleanPath = GetCleanPath($"{parent}{ItemSeparator}{child}");

            WriteWarning($"{nameof(MakePath)}(\"{parent}\",\"{child}\") => \"{cleanPath}\"");

            return cleanPath;
        }

        protected override void MoveItem(string path, string destination)
        {
            WriteWarning(nameof(MoveItem));

            base.MoveItem(path, destination);
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            WriteWarning($"{nameof(NewItem)}(\"{path}\",\"{itemTypeName}\",\"{newItemValue}\")");

            var tag = GetCleanPath(path);
            TagsDrive.Repository.Add(tag);

            WriteItemObject(new PSObject(tag), tag, true);
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            WriteWarning(nameof(RemoveItem));

            base.RemoveItem(path, recurse);
        }

        protected override void RenameItem(string path, string newName)
        {
            WriteWarning(nameof(RenameItem));

            base.RenameItem(path, newName);
        }

        protected override void SetItem(string path, object value)
        {
            WriteWarning(nameof(SetItem));

            base.SetItem(path, value);
        }

        protected override void StopProcessing()
        {
            WriteWarning(nameof(StopProcessing));

            base.StopProcessing();
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

            base.GetChildItems(path, recurse);

            foreach (var tag in tags)
            {
                WriteItemObject(new PSObject(tag), tag, true);
            }
        }

        protected override string GetChildName(string path)
        {
            var cleanPath = GetCleanPath(path);
            var childName = cleanPath.Split(ItemSeparator).Last();

            WriteWarning($"{nameof(GetChildName)}(\"{path}\") => \"{childName}\"");

            return childName;
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            WriteWarning(nameof(GetChildNames));

            base.GetChildNames(path, returnContainers);
        }

        protected override string GetParentPath(string path, string root)
        {
            var cleanPath = GetCleanPath(path);
            var lastSeparatorIndex = cleanPath.LastIndexOf(ItemSeparator);

            var result = lastSeparatorIndex == -1 ? string.Empty : cleanPath.Substring(0, lastSeparatorIndex);

            WriteWarning($"{nameof(GetParentPath)}(\"{path}\",\"{root}\") => \"{result}\"");

            return result;
        }

        public override string GetResourceString(string baseName, string resourceId)
        {
            WriteWarning(nameof(GetResourceString));

            return base.GetResourceString(baseName, resourceId);
        }

        protected override bool HasChildItems(string path)
        {
            WriteWarning(nameof(HasChildItems));

            return base.HasChildItems(path);
        }

        protected override string NormalizeRelativePath(string path, string basePath)
        {
            var cleanPath = GetCleanPath(path);
            if (!IsValidPath(cleanPath))
                throw new ArgumentException($"Path '{path}' does not exist");

            var cleanBasePath = GetCleanPath(basePath);
            if (!IsValidPath(cleanBasePath))
                throw new ArgumentException($"Path '{basePath}' does not exist");

            if (!cleanPath.StartsWith(cleanBasePath))
                throw new ArgumentException($"Path '{path}' is not under '{basePath}'");

            var result = cleanPath.Substring(cleanBasePath.Length);

            WriteWarning($"{nameof(NormalizeRelativePath)}(\"{path}\",\"{basePath}\") => \"{result}\"");

            return result;
        }

        private static string GetCleanPath(string path)
        {
            var pathChunks = path.Split(ItemSeparator);

            var resultChunks = new List<string>();
            foreach (var chunk in pathChunks)
            {
                switch (chunk)
                {
                    case "":
                    case ".":
                        continue;
                    case "..":
                        if (resultChunks.Any()) resultChunks.RemoveAt(resultChunks.Count - 1);
                        break;
                    default:
                        resultChunks.Add(chunk);
                        break;
                }
            }

            return string.Join($"{ItemSeparator}", resultChunks);
        }
    }
}