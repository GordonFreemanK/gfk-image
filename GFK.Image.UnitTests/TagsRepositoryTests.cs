using System.Collections.Generic;
using GFK.Image.Provider;
using NUnit.Framework;

namespace GFK.Image.UnitTests;

public class TagsRepositoryTests
{
    private TagsRepository _tagsRepository = null!;

    [SetUp]
    public void Setup()
    {
        _tagsRepository = new TagsRepository("Tags:", '/');

        _tagsRepository.AddTag("Tags:/Author/Gordon Freeman");
        _tagsRepository.AddTag("Tags:/Author/Adrian Shephard");
        _tagsRepository.AddTag("Tags:/Author/Adrian Shephard/Other");
        _tagsRepository.AddTag("Tags:/People/The G-Man");
    }

    [Test]
    public void Gets_existing_tag()
    {
        // Act
        var result = _tagsRepository.GetTag("Tags:/Author/Gordon Freeman");

        // Assert
        Assert.That(
            result,
            Is.EqualTo(new Tag("Tags:/Author/Gordon Freeman", "Gordon Freeman")).Using<Tag>(AreTagsEqual));
    }

    [Test]
    public void Does_not_get_parent_tag()
    {
        // Act
        var result = _tagsRepository.GetTag("Tags:/Author");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Does_not_get_non_existent_tag()
    {
        // Act
        var result = _tagsRepository.GetTag("Tags:/People/Gordon Freeman");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Does_not_get_partial_tag()
    {
        // Act
        var result = _tagsRepository.GetTag("Tags:/Author/Gordon");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Tag_path_exists()
    {
        // Act
        var result = _tagsRepository.ItemExists("Tags:/Author/Gordon Freeman");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Tag_parent_path_exists()
    {
        // Act
        var result = _tagsRepository.ItemExists("Tags:/Author");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Root_path_exists()
    {
        // Act
        var result = _tagsRepository.ItemExists("Tags:");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Root_path_with_path_separator_exists()
    {
        // Act
        var result = _tagsRepository.ItemExists("Tags:/");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Invalid_path_does_not_exist()
    {
        // Act
        var result = _tagsRepository.ItemExists("Tags:/Author/The G-Man");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Partial_tag_path_does_not_exist()
    {
        // Act
        var result = _tagsRepository.ItemExists("Tags:/Author/Gordon");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Finds_all_tags_recursively()
    {
        // Act
        var result = _tagsRepository.GetChildTags("Tags:", null);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                    new[]
                    {
                        new Tag("Tags:/Author/Gordon Freeman", "Gordon Freeman"),
                        new Tag("Tags:/Author/Adrian Shephard", "Adrian Shephard"),
                        new Tag("Tags:/Author/Adrian Shephard/Other", "Other"),
                        new Tag("Tags:/People/The G-Man", "The G-Man")
                    })
                .Using<Tag>(AreTagsEqual));
    }

    [Test]
    public void Finds_all_tags_non_recursively()
    {
        // Act
        var result = _tagsRepository.GetChildTags("Tags:", 0);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                    new[]
                    {
                        new Tag("Tags:/Author", "Author"),
                        new Tag("Tags:/People", "People")
                    })
                .Using<Tag>(AreTagsEqual));
    }

    [Test]
    public void Finds_tags_with_limited_recursion()
    {
        // Act
        var result = _tagsRepository.GetChildTags("Tags:", 1);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                    new[]
                    {
                        new Tag("Tags:/Author/Gordon Freeman", "Gordon Freeman"),
                        new Tag("Tags:/Author/Adrian Shephard", "Adrian Shephard"),
                        new Tag("Tags:/People/The G-Man", "The G-Man")
                    })
                .Using<Tag>(AreTagsEqual));
    }

    [Test]
    public void Finds_tags_in_subfolder()
    {
        // Act
        var result = _tagsRepository.GetChildTags("Tags:/Author", 0);

        // Assert
        Assert.That(
            result,
            Is.EqualTo(
                    new[]
                    {
                        new Tag("Tags:/Author/Gordon Freeman", "Gordon Freeman"),
                        new Tag("Tags:/Author/Adrian Shephard", "Adrian Shephard")
                    })
                .Using<Tag>(AreTagsEqual));
    }

    private static readonly IEnumerable<TestCaseData> MakePathCases = new[]
    {
        new TestCaseData("", "") { ExpectedResult = "" },
        new TestCaseData("", "/") { ExpectedResult = "/" },
        new TestCaseData("", "Tag1") { ExpectedResult = "Tag1" },
        new TestCaseData("", "Tag1/") { ExpectedResult = "Tag1/" },
        new TestCaseData("", "/Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData("", "/Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/", "") { ExpectedResult = "/" },
        new TestCaseData("/", "/") { ExpectedResult = "/" },
        new TestCaseData("/", "Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData("/", "Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/", "/Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData("/", "/Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1", "") { ExpectedResult = "/Tag1" },
        new TestCaseData("/Tag1", "/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1", "Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1", "Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("/Tag1", "/Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1", "/Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("/Tag1/", "") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1/", "/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1/", "Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1/", "Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("/Tag1/", "/Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1/", "/Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("Tag1", "") { ExpectedResult = "Tag1" },
        new TestCaseData("Tag1", "/") { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1", "Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1", "Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tag1", "/Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1", "/Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tag1/", "") { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1/", "/") { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1/", "Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1/", "Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tag1/", "/Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1/", "/Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tags:", "") { ExpectedResult = "Tags:" },
        new TestCaseData("Tags:", "/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:", "Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:", "Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:", "/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:", "/Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/", "") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/", "Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/", "/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/", "/Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1", "") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/Tag1", "/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1", "Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1", "Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
        new TestCaseData("Tags:/Tag1", "/Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1", "/Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
        new TestCaseData("Tags:/Tag1/", "") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1/", "/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1/", "Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1/", "/Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1/", "Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
        new TestCaseData("Tags:/Tag1/", "/Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
    };

    [TestCaseSource(nameof(MakePathCases))]
    public string Make_path(string? parent, string? child)
    {
        return _tagsRepository.MakePath(parent, child);
    }

    private static readonly IEnumerable<TestCaseData> MakePathRootedChildCases = new[]
    {
        new TestCaseData("Tags:/Tag1", "Tags:") { ExpectedResult = "Tags:" },
        new TestCaseData("Tags:/Tag1", "Tags:/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("", "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("/", "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("/Tag1", "Tags:/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tag1", "Tags:/") { ExpectedResult = "Tags:/" },
    };

    [TestCaseSource(nameof(MakePathRootedChildCases))]
    public string Make_path_if_child_is_rooted(string? parent, string? child)
    {
        return _tagsRepository.MakePath(parent, child);
    }

    private static readonly IEnumerable<TestCaseData> GetParentPathCases = new[]
    {
        new TestCaseData("") {ExpectedResult = ""},
        new TestCaseData("/") {ExpectedResult = "/"},
        new TestCaseData("/Tag1") {ExpectedResult = "/"},
        new TestCaseData("/Tag1/") {ExpectedResult = "/"},
        new TestCaseData("Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tag1/") {ExpectedResult = "Tag1/"},
        new TestCaseData("Tags:") {ExpectedResult = "Tags:"},
        new TestCaseData("Tags:/") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/Tag1") {ExpectedResult = "Tags:"},
        new TestCaseData("Tags:/Tag1/") {ExpectedResult = "Tags:"},
        new TestCaseData("Tags:/Tag1/Tag2") {ExpectedResult = "Tags:/Tag1"},
        new TestCaseData("Tags:/Tag1/Tag2/") {ExpectedResult = "Tags:/Tag1"},
    };
    
    [TestCaseSource(nameof(GetParentPathCases))]
    public string Get_parent_path(string? path)
    {
        return _tagsRepository.GetParentPath(path);
    }

    private static readonly IEnumerable<TestCaseData> GetChildNameCases = new[]
    {
        new TestCaseData("") {ExpectedResult = ""},
        new TestCaseData("/") {ExpectedResult = "/"},
        new TestCaseData("/Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("/Tag1/") {ExpectedResult = "Tag1"},
        new TestCaseData("Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tag1/") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:") {ExpectedResult = "Tags:"},
        new TestCaseData("Tags:/") {ExpectedResult = "Tags:"},
        new TestCaseData("Tags:/Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:/Tag1/") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:/Tag1/Tag2") {ExpectedResult = "Tag2"},
        new TestCaseData("Tags:/Tag1/Tag2/") {ExpectedResult = "Tag2"}
    };
    
    [TestCaseSource(nameof(GetChildNameCases))]
    public string Get_child_name(string? path)
    {
        return _tagsRepository.GetChildName(path);
    }

    private static bool AreTagsEqual(Tag left, Tag right) => left.Path == right.Path && left.Value == right.Value;
}