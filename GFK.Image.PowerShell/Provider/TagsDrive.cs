using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace GFK.Image.PowerShell.Provider
{
    public class TagsDrive : PSDriveInfo
    {
        private readonly Tag _rootTag;
        private readonly Tag _currentTag;

        public TagsDrive(PSDriveInfo driveInfo) : base(driveInfo)
        {
            _rootTag = new Tag(driveInfo.Root);
            _currentTag = _rootTag;
            CurrentLocation = _rootTag.Name;
        }

        public bool IsValidPath(string path)
        {
            return FindTag(path) != null;
        }

        public IReadOnlyCollection<(Tag tag, string tagPath)> GetChildTags(string path, uint? depth)
        {
            var fullPath = FindTag(path);
            return
                fullPath == null ? Array.Empty<(Tag, string)>()
                : GetChildTags(fullPath.Value.tag, fullPath.Value.tagPath, depth).ToArray();
        }

        public Tag NewTag(string path)
        {
            if (path == string.Empty)
                throw new InvalidOperationException("Create: tag is empty");

            GetNavigation(path, out var tag, out var tagNames);
            foreach (var tagName in tagNames)
            {
                var childTag = tag.ChildTags.FirstOrDefault(t => t.Name == tagName);
                if (childTag == null)
                {
                    childTag = new Tag(tagName);
                    tag.ChildTags.Add(childTag);
                }

                tag = childTag;
            }

            return tag;
        }

        private (Tag tag, string tagPath)? FindTag(string path)
        {
            if (path == string.Empty)
                throw new InvalidOperationException("Find: tag is empty");

            var tagPath = GetNavigation(path, out var tag, out var tagNames);

            foreach (var tagName in tagNames)
            {
                tag = tag.ChildTags.FirstOrDefault(t => t.Name == tagName);
                if (tag == null)
                    return null;
                tagPath += TagsProvider.ItemSeparator;
                tagPath += tag.Name;
            }

            return (tag, tagPath);
        }

        private string GetNavigation(string path, out Tag tag, out IEnumerable<string> tagNames)
        {
            tagNames = path.Split(TagsProvider.ItemSeparator);
            if (tagNames.ElementAt(0) == _rootTag.Name)
            {
                tag = _rootTag;
                tagNames = tagNames.Skip(1);
                return _rootTag.Name;
            }

            tag = _currentTag;
            return CurrentLocation;
        }

        private static IEnumerable<(Tag tag, string tagPath)> GetChildTags(Tag tag, string tagPath, uint? depth)
        {
            var childTags = tag.ChildTags
                .Select(t => (tag: t, tagPath: $"{tagPath}{TagsProvider.ItemSeparator}{t.Name}"))
                .ToArray();
            return depth == 0
                ? childTags
                : childTags.Union(
                    childTags.SelectMany(
                        x => GetChildTags(
                            x.tag,
                            $"{x.tagPath}{TagsProvider.ItemSeparator}{x.tag.Name}",
                            depth - 1)));
        }
    }
}