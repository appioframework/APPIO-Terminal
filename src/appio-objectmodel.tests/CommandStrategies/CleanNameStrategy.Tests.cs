﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System.Linq;
using Moq;
using NUnit.Framework;
using Appio.ObjectModel.CommandStrategies.CleanCommands;
using Appio.Resources.text.output;

namespace Appio.ObjectModel.Tests.CommandStrategies
{
    public class CleanNameStrategyTests
    {
        private static string[][] ValidInputs()
        {
            return new[]
            {
                new[]
                {
                    "any-name",
                },
                new[]
                {
                    "any-other-name",
                },
            };
        }

        private static string[][] InvalidInputs()
        {
            return new[]
            {
                new[]
                {
                    "",
                },
                new string[0], 
            };
        }

        private const string AnyCommandName = "any-name";

        private Mock<IFileSystem> _fileSystemMock;
        private CleanNameStrategy _objectUnderTest;

        [SetUp]
        public void SetUp_ObjectUnderTest()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _objectUnderTest = new CleanNameStrategy(AnyCommandName, _fileSystemMock.Object);
        }

        [Test]
        public void CleanNameStrategy_Should_ImplementICommandOfCleanStrategy()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsInstanceOf<ICommand<CleanStrategy>>(_objectUnderTest);
        }

        [Test]
        public void CleanNameStrategy_Should_HaveCorrectCommandName()
        {
            // Arrange

            // Act
            var name = _objectUnderTest.Name;

            // Assert
            Assert.AreEqual(AnyCommandName, name);
        }

        [Test]
        public void CleanNameStrategy_Should_HaveCorrectHelpText()
        {
            // Arrange

            // Act
            var helpText = _objectUnderTest.GetHelpText();

            // Assert
            Assert.AreEqual(Resources.text.help.HelpTextValues.CleanNameArgumentCommandDescription, helpText);
        }

        [Test]
        public void CleanNameStrategy_Should_CleanProjectOnValidProjectName([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrange
            const string projectBuildDirectory = "build-dir";
            var projectName = inputParams.ElementAt(0);
            var resultMessage = string.Format(OutputText.OpcuaappCleanSuccess, projectName);
            
            _fileSystemMock.Setup(x => x.CombinePaths(projectName, Constants.DirectoryName.MesonBuild)).Returns(projectBuildDirectory);
            _fileSystemMock.Setup(x => x.DeleteDirectory(projectBuildDirectory));
            _fileSystemMock.Setup(x => x.DirectoryExists(projectName)).Returns(true);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

            // Act
            var result = _objectUnderTest.Execute(inputParams);

            // Assert
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.CleanSuccess), Times.Once);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(resultMessage, result.OutputMessages.First().Key);
        }

        [Test]
        public void CleanNameStrategy_Should_IgnoreInvalidParameters([ValueSource(nameof(InvalidInputs))] string[] inputParams)
        {
            // Arrange

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

            // Act
            var result = _objectUnderTest.Execute(inputParams);

            // Assert
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.CleanFailure), Times.Once);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(OutputText.OpcuaappCleanFailure, result.OutputMessages.First().Key);
        }

        [Test]
        public void CleanNameStrategy_Should_IgnoreNotExistingDirectory([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrange
            var projectName = inputParams.ElementAt(0);
            _fileSystemMock.Setup(x => x.DirectoryExists(projectName)).Returns(false);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

            // Act
            var result = _objectUnderTest.Execute(inputParams);

            // Assert
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.CleanFailure), Times.Once);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(OutputText.OpcuaappCleanFailure, result.OutputMessages.First().Key);
        }
    }
}
