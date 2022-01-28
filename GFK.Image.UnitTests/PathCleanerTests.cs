using System.Collections.Generic;
using GFK.Image.Provider;
using NUnit.Framework;

namespace GFK.Image.UnitTests;

[TestFixture]
public class PathCleanerTests
{
    private PathCleaner _pathCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        _pathCleaner = new PathCleaner("Tags:", '/');
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
    public string Fix_root(string? path)
    {
        return _pathCleaner.FixRoot(path);
    }

    private static readonly IEnumerable<TestCaseData> EnsureRootSeparatorCases = new[]
    {
        new TestCaseData("") {ExpectedResult = ""},
        new TestCaseData("/") {ExpectedResult = "/"},
        new TestCaseData("/Tag1") {ExpectedResult = "/Tag1"},
        new TestCaseData("Tag1") {ExpectedResult = "Tag1"},
        new TestCaseData("Tags:") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/") {ExpectedResult = "Tags:/"},
        new TestCaseData("Tags:/Tag1") {ExpectedResult = "Tags:/Tag1"},
        new TestCaseData("Tags:/Tag1/") {ExpectedResult = "Tags:/Tag1/"}
    };

    [TestCaseSource(nameof(EnsureRootSeparatorCases))]
    public string Ensure_root_separator(string path)
    {
        return _pathCleaner.EnsureRootSeparator(path);
    }
}