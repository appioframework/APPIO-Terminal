using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Oppo.ObjectModel.CommandStrategies.BuildCommands;

namespace Oppo.ObjectModel.Tests
{
    public abstract class BuildNameStrategyTestsBase
    {
        protected abstract BuildNameStrategy InstantiateObjectUnderTest(IFileSystem fileSystem);
        protected abstract string GetExpectedCommandName();

        protected static string[][] InvalidInputs()
        {
            return new[]
            {
                new []{""},
                new string[0],
            };
        }

        protected static string[][] ValidInputs()
        {
            return new[]
            {
                new []{"hugo"}
            };
        }

        protected static bool[][] FailingExecutableStates()
        {
            return new[]
            {
                new[] {false, false},
                new[] {true, false},
                new[] {false, true},
            };
        }

        [Test]
        public void BuildNameStrategy_Should_ImplementICommandOfBuildStrategy()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();

            // Act
            var objectUnderTest = InstantiateObjectUnderTest(fileSystemMock.Object);

            // Assert
            Assert.IsInstanceOf<ICommand<BuildStrategy>>(objectUnderTest);
        }

        [Test]
        public void BuildNameStrategy_Should_HaveCorrectCommandName()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var objectUnderTest = InstantiateObjectUnderTest(fileSystemMock.Object);

            // Act
            var commandName = objectUnderTest.Name;

            // Assert
            Assert.AreEqual(GetExpectedCommandName(), commandName);
        }

        [Test]
        public void BuildNameStrategy_Should_ProvideEmptyHelpText()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var objectUnderTest = InstantiateObjectUnderTest(fileSystemMock.Object);

            // Act
            var helpText = objectUnderTest.GetHelpText();

            // Assert
            Assert.AreEqual(string.Empty, helpText);
        }

        [Test]
        public void BuildStrategy_Should_SucceedOnBuildableProject([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrange
            var projectDirectoryName = inputParams.ElementAt(0);
            var projectBuildDirectory = Path.Combine(projectDirectoryName, Constants.DirectoryName.MesonBuild);

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(x => x.CombinePaths(It.IsAny<string>(), It.IsAny<string>())).Returns(projectBuildDirectory);
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Meson, projectDirectoryName, Constants.DirectoryName.MesonBuild)).Returns(true);
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Ninja, projectBuildDirectory, string.Empty)).Returns(true);
            var buildStrategy = InstantiateObjectUnderTest(fileSystemMock.Object);

            // Act
            var strategyResult = buildStrategy.Execute(inputParams);

            // Assert
            Assert.AreEqual(strategyResult, Constants.CommandResults.Success);
            fileSystemMock.VerifyAll();
        }

        [Test]
        public void ShouldExecuteStrategy_Fail_MissingParameter([ValueSource(nameof(InvalidInputs))] string[] inputParams)
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();

            var buildStrategy = InstantiateObjectUnderTest(fileSystemMock.Object);

            // Act
            var strategyResult = buildStrategy.Execute(inputParams);

            // Assert
            Assert.AreEqual(strategyResult, Constants.CommandResults.Failure);
        }

        [Test]
        public void BuildStrategy_ShouldFail_DueToFailingExecutableCalls([ValueSource(nameof(FailingExecutableStates))] bool[] executableStates)
        {
            // Arrange
            var mesonState = executableStates.ElementAt(0);
            var ninjaState = executableStates.ElementAt(1);

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Meson, It.IsAny<string>(), It.IsAny<string>())).Returns(mesonState);
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Ninja, It.IsAny<string>(), It.IsAny<string>())).Returns(ninjaState);

            var buildStrategy = InstantiateObjectUnderTest(fileSystemMock.Object);

            // Act
            var strategyResult = buildStrategy.Execute(new[] {"hugo"});

            // Assert
            Assert.AreEqual(Constants.CommandResults.Failure, strategyResult);
        }
    }

    public class BuildNameStrategyTests : BuildNameStrategyTestsBase
    {
        protected override BuildNameStrategy InstantiateObjectUnderTest(IFileSystem fileSystem)
        {
            return new BuildNameStrategy(fileSystem);
        }

        protected override string GetExpectedCommandName()
        {
            return Constants.BuildCommandArguments.Name;
        }
    }

    public class BuildVerboseNameStrategyTests : BuildNameStrategyTestsBase
    {
        protected override BuildNameStrategy InstantiateObjectUnderTest(IFileSystem fileSystem)
        {
            return new BuildVerboseNameStrategy(fileSystem);
        }

        protected override string GetExpectedCommandName()
        {
            return Constants.BuildCommandArguments.VerboseName;
        }
    }
}