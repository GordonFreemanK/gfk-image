using System.Collections.Generic;
using System.Linq;

namespace GFK.Image.PowerShell.Provider
{
    public interface ITagsRepository
    {
        void AddTag(string path);
        bool DoesTagExist(string path);
        IReadOnlyCollection<string> GetChildTags(string path, uint? depth);
    }

    public class TagsRepository : ITagsRepository
    {
        private const char Separator = '\\';

        private readonly List<string> _tags;

        public TagsRepository()
        {
            _tags = new List<string>();
        }

        public void AddTag(string path)
        {
            path = path.TrimEnd(Separator);
            _tags.Add(path);
        }

        public bool DoesTagExist(string path)
        {
            path = path.TrimEnd(Separator);
            return _tags.Any(tag => tag == path || tag.StartsWith($"{path}{Separator}"));
        }
        
        public IReadOnlyCollection<string> GetChildTags(string path, uint? depth)
        {
            path = path.TrimEnd(Separator) + Separator;
            return _tags
                .Where(tag => tag.StartsWith(path))
                .Select(tag => depth == null ? tag : GetPartialPath(tag, path.Length, depth.Value))
                .Distinct()
                .ToArray();
        }

        private static string GetPartialPath(string tag, int position, uint depth)
        {
            do position = tag.IndexOf(Separator, position + 1);
            while (position >= 0 && depth-- > 0);
            return position >= 0 ? tag.Substring(0, position) : tag;
        }
    }
}