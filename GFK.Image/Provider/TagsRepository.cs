using System.Collections.Generic;
using System.Linq;

namespace GFK.Image.Provider
{
    public interface ITagsRepository
    {
        Tag AddTag(string path);
        Tag? GetTag(string path);
        bool IsPathValid(string path);
        IReadOnlyCollection<Tag> GetChildTags(string path, uint? depth);
    }

    public class TagsRepository : ITagsRepository
    {
        private const char Separator = '\\';

        private readonly List<string> _tags;

        public TagsRepository()
        {
            _tags = new List<string>();
        }

        public Tag AddTag(string path)
        {
            path = path.TrimEnd(Separator);
            _tags.Add(path);
            return BuildTag(path);
        }

        public Tag? GetTag(string path)
        {
            path = path.TrimEnd(Separator);
            return _tags.Contains(path)
                ? BuildTag(path)
                : null;
        }
        
        public bool IsPathValid(string path)
        {
            path = path.TrimEnd(Separator);
            return _tags.Any(tag => tag == path || tag.StartsWith($"{path}{Separator}"));
        }
        
        public IReadOnlyCollection<Tag> GetChildTags(string path, uint? depth)
        {
            path = path.TrimEnd(Separator) + Separator;
            return _tags
                .Where(tag => tag.StartsWith(path))
                .Select(tag => depth == null ? tag : GetPartialPath(tag, path.Length, depth.Value))
                .Distinct()
                .Select(BuildTag)
                .ToArray();
        }
        
        private static Tag BuildTag(string path)
        {
            return new Tag(path, path.Split(Separator).Last());
        }

        private static string GetPartialPath(string tag, int position, uint depth)
        {
            do position = tag.IndexOf(Separator, position + 1);
            while (position >= 0 && depth-- > 0);
            return position >= 0 ? tag.Substring(0, position) : tag;
        }
    }
}