﻿using Moq;
using NUnit.Framework;
using Oppo.ObjectModel.CommandStrategies.PublishCommands;

namespace Oppo.ObjectModel.Tests.CommandStrategies
{
    public class PublishNameStrategyTests
    {
        [Test]
        public void PublishNameStrategy_Should_ImplementICommandOfPublishStrategy()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();

            // Act
            var objectUnderTest = new PublishNameStrategy(string.Empty, fileSystemMock.Object);

            // Assert
            Assert.IsInstanceOf<ICommand<PublishStrategy>>(objectUnderTest);
        }

        [Test]
        public void PublishNameStrategy_Should_HaveCorrectCommandName()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var objectUnderTest = new PublishNameStrategy(string.Empty, fileSystemMock.Object);

            // Act
            var commandName = objectUnderTest.Name;

            // Assert
            Assert.AreEqual(string.Empty, commandName);
        }

        [Test]
        public void PublishNameStrategy_Should_ProvideEmptyHelpText()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var objectUnderTest = new PublishNameStrategy(string.Empty, fileSystemMock.Object);

            // Act
            var helpText = objectUnderTest.GetHelpText();

            // Assert
            Assert.AreEqual(string.Empty, helpText);
        }

        [Test]
        public void PublishNameStrategy_Should_CreatePublishDirectoryContainingApplicationFiles()
        {
            // Arrange
            const string applicationName = "any-name";
            const string buildDirectory = "build";
            const string publishDirectory = "publish";
            const string applicationSourcePath = "source";
            const string applicationTargetPath = "target";

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(x => x.CombinePaths(applicationName, Constants.DirectoryName.Publish)).Returns(publishDirectory);
            fileSystemMock.Setup(x => x.CombinePaths(applicationName, Constants.DirectoryName.MesonBuild)).Returns(buildDirectory);
            fileSystemMock.Setup(x => x.CombinePaths(buildDirectory, Constants.ExecutableName.App)).Returns(applicationSourcePath);
            fileSystemMock.Setup(x => x.CombinePaths(publishDirectory, Constants.ExecutableName.App)).Returns(applicationTargetPath);
            var objectUnderTest = new PublishNameStrategy(string.Empty, fileSystemMock.Object);

            // Act
            var result = objectUnderTest.Execute(new[] {applicationName});

            // Assert
            Assert.AreEqual(Constants.CommandResults.Success, result);
            fileSystemMock.Verify(x => x.CreateDirectory(publishDirectory), Times.Once);
            fileSystemMock.Verify(x => x.CopyFile(applicationSourcePath, applicationTargetPath), Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        public void PublishNameStrategy_Should_IgnoreInvalidInputParams(string applicationName)
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var objectUnderTest = new PublishNameStrategy(string.Empty, fileSystemMock.Object);

            // Act
            var result = objectUnderTest.Execute(new[] {applicationName});

            // Assert
            Assert.AreEqual(Constants.CommandResults.Failure, result);
        }
    }
}