using System.Collections.Generic;

namespace GFK.Image.PowerShell.Provider
{
    public class Tag
    {
        public Tag(string name)
        {
            Name = name;
            ChildTags = new List<Tag>();
        }

        public string Name { get; }

        public List<Tag> ChildTags { get; }
    }
}