using System.Collections.Generic;
using GFK.Image.Provider;
using NUnit.Framework;

namespace GFK.Image.UnitTests;

[TestFixture]
public class PathMakerTests
{
    private static readonly IEnumerable<TestCaseData> MakePathCases = new[]
    {
        new TestCaseData(null, null) { ExpectedResult = "" },
        new TestCaseData(null, "") { ExpectedResult = "" },
        new TestCaseData(null, "/") { ExpectedResult = "/" },
        new TestCaseData(null, "Tag1") { ExpectedResult = "Tag1" },
        new TestCaseData(null, "Tag1/") { ExpectedResult = "Tag1/" },
        new TestCaseData(null, "/Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData(null, "/Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("", null) { ExpectedResult = "" },
        new TestCaseData("", "") { ExpectedResult = "" },
        new TestCaseData("", "/") { ExpectedResult = "/" },
        new TestCaseData("", "Tag1") { ExpectedResult = "Tag1" },
        new TestCaseData("", "Tag1/") { ExpectedResult = "Tag1/" },
        new TestCaseData("", "/Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData("", "/Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/", null) { ExpectedResult = "/" },
        new TestCaseData("/", "") { ExpectedResult = "/" },
        new TestCaseData("/", "/") { ExpectedResult = "/" },
        new TestCaseData("/", "Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData("/", "Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/", "/Tag1") { ExpectedResult = "/Tag1" },
        new TestCaseData("/", "/Tag1/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1", null) { ExpectedResult = "/Tag1" },
        new TestCaseData("/Tag1", "") { ExpectedResult = "/Tag1" },
        new TestCaseData("/Tag1", "/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1", "Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1", "Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("/Tag1", "/Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1", "/Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("/Tag1/", null) { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1/", "") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1/", "/") { ExpectedResult = "/Tag1/" },
        new TestCaseData("/Tag1/", "Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1/", "Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("/Tag1/", "/Tag2") { ExpectedResult = "/Tag1/Tag2" },
        new TestCaseData("/Tag1/", "/Tag2/") { ExpectedResult = "/Tag1/Tag2/" },
        new TestCaseData("Tag1", null) { ExpectedResult = "Tag1" },
        new TestCaseData("Tag1", "") { ExpectedResult = "Tag1" },
        new TestCaseData("Tag1", "/") { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1", "Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1", "Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tag1", "/Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1", "/Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tag1/", null) { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1/", "") { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1/", "/") { ExpectedResult = "Tag1/" },
        new TestCaseData("Tag1/", "Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1/", "Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tag1/", "/Tag2") { ExpectedResult = "Tag1/Tag2" },
        new TestCaseData("Tag1/", "/Tag2/") { ExpectedResult = "Tag1/Tag2/" },
        new TestCaseData("Tags:", null) { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:", "") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:", "/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:", "Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:", "Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:", "/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:", "/Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/", null) { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/", "Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/", "/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/", "/Tag1/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1", null) { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/Tag1", "") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("Tags:/Tag1", "/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1", "Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1", "Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
        new TestCaseData("Tags:/Tag1", "/Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1", "/Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
        new TestCaseData("Tags:/Tag1/", null) { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1/", "") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1/", "/") { ExpectedResult = "Tags:/Tag1/" },
        new TestCaseData("Tags:/Tag1/", "Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1/", "/Tag2") { ExpectedResult = "Tags:/Tag1/Tag2" },
        new TestCaseData("Tags:/Tag1/", "Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
        new TestCaseData("Tags:/Tag1/", "/Tag2/") { ExpectedResult = "Tags:/Tag1/Tag2/" },
    };

    [TestCaseSource(nameof(MakePathCases))]
    public string Makes_path_without_separator_in_root(string? parent, string? child)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:");
        
        // Assert
        return pathMaker.MakePath(parent, child);
    }

    [TestCaseSource(nameof(MakePathCases))]
    public string Makes_path_with_separator_in_root(string? parent, string? child)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:/");
        
        // Assert
        return pathMaker.MakePath(parent, child);
    }
    
    
    private static readonly IEnumerable<TestCaseData> MakePathRootedChildCases = new[]
    {
        new TestCaseData("Tags:/Tag1", "Tags:") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/Tag1", "Tags:/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tags:/", "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData(null, "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("", "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("/", "Tags:/Tag1") { ExpectedResult = "Tags:/Tag1" },
        new TestCaseData("/Tag1", "Tags:/") { ExpectedResult = "Tags:/" },
        new TestCaseData("Tag1", "Tags:/") { ExpectedResult = "Tags:/" },
    };

    [TestCaseSource(nameof(MakePathRootedChildCases))]
    public string Makes_path_without_separator_in_root_if_child_is_rooted(string? parent, string? child)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:");
        
        // Assert
        return pathMaker.MakePath(parent, child);
    }

    [TestCaseSource(nameof(MakePathRootedChildCases))]
    public string Makes_path_with_separator_in_root_if_child_is_rooted(string? parent, string? child)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:/");
        
        // Assert
        return pathMaker.MakePath(parent, child);
    }

    private static readonly IEnumerable<TestCaseData> GetParentPathCases = new[]
    {
        new TestCaseData(null) {ExpectedResult = ""},
        new TestCaseData("") {ExpectedResult = ""},
        new TestCaseData("/") {ExpectedResult = "/"},
        new TestCaseData("/Tag1") {ExpectedResult = "/"},
        new TestCaseData("/Tag1/") {ExpectedResult = "/"},
        new TestCaseData("Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tag1/") {ExpectedResult = "Tag1/"},
        new TestCaseData("Tags:") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/Tag1") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/Tag1/") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/Tag1/Tag2") {ExpectedResult = "Tags:/Tag1"},
        new TestCaseData("Tags:/Tag1/Tag2/") {ExpectedResult = "Tags:/Tag1"},
    };
    
    [TestCaseSource(nameof(GetParentPathCases))]
    public string Get_parent_path_without_separator_in_root(string? path)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:");
        
        // Assert
        return pathMaker.GetParentPath(path);
    }

    [TestCaseSource(nameof(GetParentPathCases))]
    public string Get_parent_path_with_separator_in_root(string? path)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:/");
        
        // Assert
        return pathMaker.GetParentPath(path);
    }
    
    private static readonly IEnumerable<TestCaseData> GetChildNameCases = new[]
    {
        new TestCaseData(null) {ExpectedResult = ""},
        new TestCaseData("") {ExpectedResult = ""},
        new TestCaseData("/") {ExpectedResult = "/"},
        new TestCaseData("/Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("/Tag1/") {ExpectedResult = "Tag1"},
        new TestCaseData("Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tag1/") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:/Tag1/") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:/Tag1/Tag2") {ExpectedResult = "Tag2"},
        new TestCaseData("Tags:/Tag1/Tag2/") {ExpectedResult = "Tag2"}
    };
    
    [TestCaseSource(nameof(GetChildNameCases))]
    public string Get_child_name_without_separator_in_root(string? path)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:");
        
        // Assert
        return pathMaker.GetChildName(path);
    }

    [TestCaseSource(nameof(GetChildNameCases))]
    public string Get_child_name_with_separator_in_root(string? path)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:/");
        
        // Assert
        return pathMaker.GetChildName(path);
    }

    private static readonly IEnumerable<TestCaseData> FixRootCases = new[]
    {
        new TestCaseData(null) {ExpectedResult = ""},
        new TestCaseData("") {ExpectedResult = ""},
        new TestCaseData("/") {ExpectedResult = "/"},
        new TestCaseData("/Tag1") {ExpectedResult = "/Tag1"},
        new TestCaseData("Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:") {ExpectedResult = "Tags:"},
        new TestCaseData("Tags:/") {ExpectedResult = "Tags:/"},
        new TestCaseData(@"Tags:\") {ExpectedResult = "Tags:/"},
        new TestCaseData(@"Tags:\/") {ExpectedResult = "Tags:/"},
        new TestCaseData(@"Tags:/\") {ExpectedResult = @"Tags:/\"},
        new TestCaseData("Tags:/Tag1") {ExpectedResult = "Tags:/Tag1"},
        new TestCaseData(@"Tags:\/Tag1") {ExpectedResult = "Tags:/Tag1"},
        new TestCaseData(@"Tags:/Tag1\/") {ExpectedResult = @"Tags:/Tag1\/"},
        new TestCaseData(@"Tags:/Tag1\/Tag2") {ExpectedResult = @"Tags:/Tag1\/Tag2"}
    };
    
    [TestCaseSource(nameof(FixRootCases))]
    public string Fix_root_without_separator_in_root(string? path)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:");
        
        // Assert
        return pathMaker.FixRoot(path);
    }

    [TestCaseSource(nameof(FixRootCases))]
    public string Fix_root_with_separator_in_root(string? path)
    {
        // Arrange
        var pathMaker = new PathMaker('/', "Tags:/");
        
        // Assert
        return pathMaker.FixRoot(path);
    }
}