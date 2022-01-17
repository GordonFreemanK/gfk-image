using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace GFK.PowerShell;

[CmdletProvider("Tags", ProviderCapabilities.None)]
public class TagsProvider : NavigationCmdletProvider
{
    public override char ItemSeparator => '\\';

    private TagsDrive TagsDrive => (TagsDrive)PSDriveInfo;

    protected override bool IsValidPath(string path)
    {
        var cleanPath = GetCleanPath(path);

        var isValidPath = TagsDrive.IsValidPath(cleanPath);

        WriteWarning($"{nameof(IsValidPath)}(\"{path}\") => {isValidPath}");

        return isValidPath;
    }

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        var tagsDrive = new TagsDrive(drive);

        WriteWarning(
            $"{nameof(NewDrive)}(Root={drive.Root}) => TagDrive(Root={tagsDrive.Root}, CurrentLocation={tagsDrive.CurrentLocation})");

        return tagsDrive;
    }

    protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
    {
        WriteWarning(nameof(RemoveDrive));

        return drive;
    }

    protected override bool ItemExists(string path)
    {
        var cleanPath = GetCleanPath(path);

        var isValidPath = TagsDrive.IsValidPath(cleanPath);

        WriteWarning($"{nameof(ItemExists)}(\"{path}\") => {isValidPath}");

        return isValidPath;
    }

    protected override bool IsItemContainer(string path)
    {
        var cleanPath = GetCleanPath(path);

        var isValidPath = TagsDrive.IsValidPath(cleanPath);

        WriteWarning($"{nameof(IsItemContainer)}(\"{path}\") => {isValidPath}");

        return isValidPath;
    }

    protected override ProviderInfo Start(ProviderInfo providerInfo)
    {
        WriteWarning(nameof(Start));

        return base.Start(providerInfo);
    }

    protected override void Stop()
    {
        WriteWarning(nameof(Stop));

        base.Stop();
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

    protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
    {
        WriteWarning($"{nameof(ConvertPath)}(\"{path}\",\"{filter}\")");

        return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
    }

    protected override string[] ExpandPath(string path)
    {
        WriteWarning(nameof(ExpandPath));

        return base.ExpandPath(path);
    }

    protected override void GetItem(string path)
    {
        WriteWarning(nameof(GetItem));

        base.GetItem(path);
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

        var cleanPath = GetCleanPath(path);
        var tag = TagsDrive.NewTag(cleanPath);
        
        WriteItemObject(tag.Name, path, true);
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
        var tags = TagsDrive.GetChildTags(path, recurse ? depth : 0);
        
        WriteWarning($"{nameof(GetChildItems)}(\"{path}\",{recurse},{depth}) => {tags.Count}");

        foreach (var (tag, tagPath) in tags)
        {
            WriteItemObject(tag.Name, tagPath, true);
        }
    }

    protected override void GetChildItems(string path, bool recurse)
    {
        var tags = TagsDrive.GetChildTags(path, recurse ? default(uint?) : 0);
        
        WriteWarning($"{nameof(GetChildItems)}(\"{path}\",{recurse}) => {tags.Count}");

        base.GetChildItems(path, recurse);

        foreach (var (tag, tagPath) in tags)
        {
            WriteItemObject(tag.Name, tagPath, true);
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

        var result = lastSeparatorIndex == -1 ? string.Empty : cleanPath[..lastSeparatorIndex];

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

    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
        WriteWarning(nameof(InitializeDefaultDrives));

        return base.InitializeDefaultDrives();
    }

    protected override void InvokeDefaultAction(string path)
    {
        WriteWarning(nameof(InvokeDefaultAction));

        base.InvokeDefaultAction(path);
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

        var result = cleanPath[cleanBasePath.Length..];

        WriteWarning($"{nameof(NormalizeRelativePath)}(\"{path}\",\"{basePath}\") => \"{result}\"");

        return result;
    }

    protected override object StartDynamicParameters()
    {
        WriteWarning(nameof(StartDynamicParameters));

        return base.StartDynamicParameters();
    }

    protected override object ClearItemDynamicParameters(string path)
    {
        WriteWarning(nameof(ClearItemDynamicParameters));

        return base.ClearItemDynamicParameters(path);
    }

    protected override object CopyItemDynamicParameters(string path, string destination, bool recurse)
    {
        WriteWarning(nameof(CopyItemDynamicParameters));

        return base.CopyItemDynamicParameters(path, destination, recurse);
    }

    protected override object GetItemDynamicParameters(string path)
    {
        WriteWarning(nameof(GetItemDynamicParameters));

        return base.GetItemDynamicParameters(path);
    }

    protected override object ItemExistsDynamicParameters(string path)
    {
        WriteWarning(nameof(ItemExistsDynamicParameters));

        return base.ItemExistsDynamicParameters(path);
    }

    protected override object MoveItemDynamicParameters(string path, string destination)
    {
        WriteWarning(nameof(MoveItemDynamicParameters));

        return base.MoveItemDynamicParameters(path, destination);
    }

    protected override object NewDriveDynamicParameters()
    {
        WriteWarning(nameof(NewDriveDynamicParameters));

        return base.NewDriveDynamicParameters();
    }

    protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
    {
        WriteWarning($"{nameof(NewItemDynamicParameters)}(\"{path}\",\"{itemTypeName}\",\"{newItemValue}\")");

        return base.NewItemDynamicParameters(path, itemTypeName, newItemValue);
    }

    protected override object RemoveItemDynamicParameters(string path, bool recurse)
    {
        WriteWarning(nameof(RemoveItemDynamicParameters));

        return base.RemoveItemDynamicParameters(path, recurse);
    }

    protected override object RenameItemDynamicParameters(string path, string newName)
    {
        WriteWarning(nameof(RenameItemDynamicParameters));

        return base.RenameItemDynamicParameters(path, newName);
    }

    protected override object SetItemDynamicParameters(string path, object value)
    {
        WriteWarning(nameof(SetItemDynamicParameters));

        return base.SetItemDynamicParameters(path, value);
    }

    protected override object GetChildItemsDynamicParameters(string path, bool recurse)
    {
        WriteWarning($"{nameof(GetChildItemsDynamicParameters)}(\"{path}\",{recurse})");

        return base.GetChildItemsDynamicParameters(path, recurse);
    }

    protected override object GetChildNamesDynamicParameters(string path)
    {
        WriteWarning(nameof(GetChildNamesDynamicParameters));

        return base.GetChildNamesDynamicParameters(path);
    }

    protected override object InvokeDefaultActionDynamicParameters(string path)
    {
        WriteWarning(nameof(InvokeDefaultActionDynamicParameters));

        return base.InvokeDefaultActionDynamicParameters(path);
    }

    private void NewItem(string path)
    {
        var cleanPath = GetCleanPath(path);
        TagsDrive.NewTag(cleanPath);
    }

    private string GetCleanPath(string path)
    {
        var pathChunks = path.Split(ItemSeparator);

        var resultChunks = new List<string>();
        foreach (var chunk in pathChunks)
        {
            switch (chunk)
            {
                case "" or ".":
                    continue;
                case "..":
                    if (resultChunks.Any()) resultChunks.RemoveAt(resultChunks.Count - 1);
                    break;
                default:
                    resultChunks.Add(chunk);
                    break;
            }
        }

        return string.Join(ItemSeparator, resultChunks);
    }
}