using GFK.Image.Provider;
using NUnit.Framework;

namespace GFK.Image.UnitTests
{
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
        public void Gets_existing_tag()
        {
            // Act
            var result = _tagsRepository.GetTag(@"Tags:\Author\Gordon Freeman");

            // Assert
            Assert.That(result, Is.EqualTo(@"Tags:\Author\Gordon Freeman"));
        }

        [Test]
        public void Does_not_get_parent_tag()
        {
            // Act
            var result = _tagsRepository.GetTag(@"Tags:\Author");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Does_not_get_non_existent_tag()
        {
            // Act
            var result = _tagsRepository.GetTag(@"Tags:\People\Gordon Freeman");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Does_not_get_partial_tag()
        {
            // Act
            var result = _tagsRepository.GetTag(@"Tags:\Author\Gordon");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Tag_path_is_valid()
        {
            // Act
            var result = _tagsRepository.IsPathValid(@"Tags:\Author\Gordon Freeman");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Tag_parent_path_is_valid()
        {
            // Act
            var result = _tagsRepository.IsPathValid(@"Tags:\Author");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Root_path_is_valid()
        {
            // Act
            var result = _tagsRepository.IsPathValid("Tags:");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Non_existent_path_is_invalid()
        {
            // Act
            var result = _tagsRepository.IsPathValid(@"Tags:\Author\The G-Man");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void Partial_tag_path_is_invalid()
        {
            // Act
            var result = _tagsRepository.IsPathValid(@"Tags:\Author\Gordon");

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
}