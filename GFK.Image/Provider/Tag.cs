namespace GFK.Image.Provider
{
    public class Tag
    {
        public Tag(string path, string value)
        {
            Path = path;
            Value = value;
        }

        public string Path { get; }
        public string Value { get; }
    }
}