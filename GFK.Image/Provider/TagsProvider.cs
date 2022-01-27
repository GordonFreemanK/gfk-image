using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Runtime.CompilerServices;

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

    protected override bool IsValidPath(string path)
    {
        return LogAndExecute(() => true, new object?[] { path });
    }

    protected override bool ItemExists(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return LogAndExecute(() => TagsDrive.Repository.IsPathValid(cleanPath),
            new object?[] { path });
    }

    protected override bool IsItemContainer(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return LogAndExecute(() => TagsDrive.Repository.IsPathValid(cleanPath), new object?[] { path });
    }

    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        LogAndExecute(
            () =>
            {
                var tag = TagsDrive.Repository.AddTag(cleanPath);
                WriteItemObject(tag, tag.Path, true);
            },
            new[] { path, itemTypeName, newItemValue });
    }

    protected override void GetItem(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        LogAndExecute(
            () =>
            {
                var tag = TagsDrive.Repository.GetTag(cleanPath);
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

    protected override string MakePath(string? parent, string? child)
    {
        var cleanParent = TagsDrive.PathMaker.FixRoot(parent);
        var cleanChild = TagsDrive.PathMaker.FixRoot(child);
        return LogAndExecute(
            () => TagsDrive.PathMaker.MakePath(cleanParent, cleanChild),
            new object?[] { parent, child });
    }

    protected override string NormalizeRelativePath(string path, string basePath)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        var cleanBasePath = TagsDrive.PathMaker.FixRoot(basePath);
        return LogAndExecute(
            () => base.NormalizeRelativePath(cleanPath, cleanBasePath),
            new object?[] { path, basePath });
    }

    protected override string GetParentPath(string? path, string? root)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return LogAndExecute(() => TagsDrive.PathMaker.GetParentPath(cleanPath),
            new object?[] { path, root });
    }

    protected override string GetChildName(string path)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return LogAndExecute(
            () => TagsDrive.PathMaker.GetChildName(cleanPath),
            new object?[] { path });
    }

    private int GetChildItems(string path, uint? depth)
    {
        var cleanPath = TagsDrive.PathMaker.FixRoot(path);
        return LogAndExecute(
            () =>
            {
                var childTags = TagsDrive.Repository.GetChildTags(cleanPath, depth);
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
    
    private TagsDrive TagsDrive => (TagsDrive)PSDriveInfo;
}