using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Runtime.CompilerServices;

namespace GFK.Image.Provider;

[CmdletProvider("Tags", ProviderCapabilities.None)]
public class TagsProvider : NavigationCmdletProvider
{
    public override char ItemSeparator => '\\';

    public override char AltItemSeparator => '\\';

    private TagsDrive TagsDrive => (TagsDrive)PSDriveInfo;

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        WriteVerbose($"Root: {drive.Root}");
        return new TagsDrive(drive, ItemSeparator);
    }

    protected override bool IsValidPath(string path)
    {
        return LogAndExecute(() => true, new object?[] { path });
    }

    protected override bool ItemExists(string path)
    {
        return LogAndExecute(() => TagsDrive.Repository.IsPathValid(path), new object?[] { path });
    }

    protected override bool IsItemContainer(string path)
    {
        return LogAndExecute(() => TagsDrive.Repository.IsPathValid(path), new object?[] { path });
    }

    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        LogAndExecute(
            () =>
            {
                var tag = TagsDrive.Repository.AddTag(path);
                WriteItemObject(tag, tag.Path, true);
            },
            new[] { path, itemTypeName, newItemValue });
    }

    protected override void GetItem(string path)
    {
        LogAndExecute(
            () =>
            {
                var tag = TagsDrive.Repository.GetTag(path);
                if (tag != null)
                {
                    WriteItemObject(tag, tag.Path, true);
                }
            },
            new object?[] { path });
    }

    protected override void GetChildItems(string path, bool recurse, uint depth)
    {
        LogAndExecute(() => GetChildItems(path, recurse ? depth : 0), new object?[] { path, recurse, depth });
    }

    protected override void GetChildItems(string path, bool recurse)
    {
        LogAndExecute(() => GetChildItems(path, recurse ? default : (uint)0), new object?[] { path, recurse });
    }

    protected override string MakePath(string parent, string child)
    {
        return LogAndExecute(() => base.MakePath(parent, child), new object?[] { parent, child });
    }

    protected override string[] ExpandPath(string path)
    {
        return LogAndExecute(() => base.ExpandPath(path), new object?[] { path });
    }

    protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
    {
        var result = base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        return LogAndExecute(() => result, new object?[] { path, filter, updatedPath, updatedFilter });
    }

    protected override string NormalizeRelativePath(string path, string basePath)
    {
        return LogAndExecute(() => base.NormalizeRelativePath(path, basePath), new object?[] { path, basePath });
    }

    protected override string GetParentPath(string path, string? root)
    {
        return LogAndExecute(() => base.GetParentPath(path, root), new object?[] { path, root });
    }

    protected override string GetChildName(string path)
    {
        return LogAndExecute(() => base.GetChildName(path), new object?[] { path });
    }

    protected override void GetChildNames(string path, ReturnContainers returnContainers)
    {
        LogAndExecute(() => base.GetChildNames(path, returnContainers), new object?[] { path, returnContainers });
    }

    protected override bool HasChildItems(string path)
    {
        return LogAndExecute(() => base.HasChildItems(path), new object?[] { path });
    }

    private int GetChildItems(string path, uint? depth)
    {
        return LogAndExecute(
            () =>
            {
                var childTags = TagsDrive.Repository.GetChildTags(path, depth);
                foreach (var tag in childTags)
                {
                    WriteItemObject(tag, tag.Path, true);
                }

                return childTags.Count;
            },
            new object?[] { path, depth });
    }

    private TResult LogAndExecute<TResult>(
        Func<TResult> func,
        IEnumerable<object?> parameters,
        [CallerMemberName] string? methodName = null)
    {
        var call = $"{methodName}({string.Join(',', parameters)})";
        try
        {
            var result = func();
            WriteVerbose($"{call} => {result}");
            return result;
        }
        catch (Exception exception)
        {
            WriteVerbose($"{call} => {exception.Message}");
            throw;
        }
    }

    private void LogAndExecute(
        Action action,
        IEnumerable<object?> parameters,
        [CallerMemberName] string? methodName = null)
    {
        var call = $"{methodName}({string.Join(',', parameters)})";
        try
        {
            action();
            WriteVerbose(call);
        }
        catch (Exception exception)
        {
            WriteVerbose($"{call} => {exception.Message}");
            throw;
        }
    }
}