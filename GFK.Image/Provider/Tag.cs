namespace GFK.Image.Provider;

public record Tag(string Path, string Value)
{
    public string Path { get; } = Path;
    public string Value { get; } = Value;
}