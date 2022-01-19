using GFK.Image.PowerShell.Provider;
using NUnit.Framework;

namespace GFK.Image.PowerShell.UnitTests;

public class TagsRepositoryTests
{
    private TagsRepository _tagsRepository = null!;

    [SetUp]
    public void Setup()
    {
        _tagsRepository = new TagsRepository();
        
        _tagsRepository.AddTag(@"Tags:\Author\Gordon Freeman");
        _tagsRepository.AddTag(@"Tags:\Author\Adrian Shephard");
        _tagsRepository.AddTag(@"Tags:\Author\Adrian Shephard\Other");
        _tagsRepository.AddTag(@"Tags:\People\The G-Man");
    }

    [Test]
    public void Finds_existing_tag()
    {
        // Act
        var result = _tagsRepository.DoesTagExist(@"Tags:\Author\Gordon Freeman");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Finds_containing_tag()
    {
        // Act
        var result = _tagsRepository.DoesTagExist(@"Tags:\Author");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Finds_root()
    {
        // Act
        var result = _tagsRepository.DoesTagExist("Tags:");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Does_not_find_non_existent_tag_that_does_not_exists()
    {
        // Act
        var result = _tagsRepository.DoesTagExist(@"Tags:\Author\The G-Man");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Does_not_find_non_existent_tag_that_starts_with_existing_tag()
    {
        // Act
        var result = _tagsRepository.DoesTagExist(@"Tags:\Author\Gordon");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Finds_all_tags_recursively()
    {
        // Act
        var result = _tagsRepository.GetChildTags(@"Tags:", null);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                new[]
                {
                    @"Tags:\Author\Gordon Freeman",
                    @"Tags:\Author\Adrian Shephard",
                    @"Tags:\Author\Adrian Shephard\Other",
                    @"Tags:\People\The G-Man"
                }));
    }

    [Test]
    public void Finds_all_tags_non_recursively()
    {
        // Act
        var result = _tagsRepository.GetChildTags(@"Tags:", 0);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                new[]
                {
                    @"Tags:\Author",
                    @"Tags:\People"
                }));
    }

    [Test]
    public void Finds_tags_with_limited_recursion()
    {
        // Act
        var result = _tagsRepository.GetChildTags(@"Tags:", 1);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                new[]
                {
                    @"Tags:\Author\Gordon Freeman",
                    @"Tags:\Author\Adrian Shephard",
                    @"Tags:\People\The G-Man"
                }));
    }

    [Test]
    public void Finds_tags_in_subfolder()
    {
        // Act
        var result = _tagsRepository.GetChildTags(@"Tags:\Author", 0);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                new[]
                {
                    @"Tags:\Author\Gordon Freeman",
                    @"Tags:\Author\Adrian Shephard"
                }));
    }

}