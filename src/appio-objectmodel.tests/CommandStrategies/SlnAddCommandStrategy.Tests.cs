/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System.Linq;
using Moq;
using NUnit.Framework;
using Appio.ObjectModel.CommandStrategies.SlnCommands;
using Appio.Resources.text.output;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Appio.ObjectModel.Tests.CommandStrategies
{
    public class SlnAddNameStrategyTestsShould
    {
        protected static string[][] ValidInputs()
        {
            return new[]
            {
                new [] { "-s", "testSln", "-p", "testProj" },
                new [] { "-s", "testSln", "--project", "testProj" },
                new [] { "--solution", "testSln", "-p", "testProj" },
                new [] { "--solution", "testSln", "--project", "testProj" },
            };
        }
        
        protected static object[] BadFlagParams =
        {
	        new [] { "-s", "testSln", "--p", "testProj" },
	        new [] { "-s", "testSln", "--Project", "testProj" },
	        new [] { "-solution", "testSln", "-p", "testProj" },
	        new [] { "--Solution", "testSln", "--project", "testProj" },
        };

        private Mock<IFileSystem> _fileSystemMock;
        private SlnAddCommandStrategy _objectUnderTest;

        private readonly string _defaultAppioslnContent = "{\"projects\": []}";
        private readonly string _sampleAppioslnContent = "{\"projects\": [{\"name\":\"clientApp\",\"path\":\"clientApp/clientApp.appioproj\"}]}";

        private readonly string _sampleOpcuaClientAppContent = "{\"name\":\"clientApp\",\"type\":\"Client\",\"references\": []}";
		private readonly string _sampleOpcuaServerAppContent = "{\"name\":\"serverApp\",\"type\":\"Server\"}";

        [SetUp]
        public void SetUp_ObjectUnderTest()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _objectUnderTest = new SlnAddCommandStrategy(_fileSystemMock.Object);
        }

        [Test]
        public void ImplementICommandOfSlnAddStrategy()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsInstanceOf<ICommand<SlnStrategy>>(_objectUnderTest);
        }

        [Test]
        public void HaveCorrectCommandName()
        {
            // Arrange

            // Act
            var name = _objectUnderTest.Name;

            // Assert
            Assert.AreEqual(Constants.SlnCommandArguments.Add, name);
        }

        [Test]
        public void HaveCorrectHelpText()
        {
            // Arrange

            // Act
            var helpText = _objectUnderTest.GetHelpText();

            // Assert
            Assert.AreEqual(Resources.text.help.HelpTextValues.SlnAddNameArgumentCommandDescription, helpText);
        }

        [Test]
        public void FailBecauseOfMissingAppioslnFile([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrange
            var solutionName = inputParams.ElementAtOrDefault(1);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

            // Arrange appiosln file
            var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
            _fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
            _fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(false);


            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsFalse(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.SlnAppioslnFileNotFound), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnAppioslnNotFound, appioslnPath), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);
            _fileSystemMock.Verify(x => x.FileExists(appioslnPath), Times.Once);
        }

        [Test]
        public void FailBecauseOfMissingAppioprojFile([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrange
            var solutionName = inputParams.ElementAtOrDefault(1);
            var opcuaappName = inputParams.ElementAtOrDefault(3);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

            // Arrange appiosln file
            var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
            _fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
            _fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(true);

            // Arrange appioproj file
            var appioprojPath = Path.Combine(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject);
            _fileSystemMock.Setup(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject)).Returns(appioprojPath);
            _fileSystemMock.Setup(x => x.FileExists(appioprojPath)).Returns(false);


            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsFalse(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.SlnAddAppioprojFileNotFound), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnAddOpcuaappNotFound, appioprojPath), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);
            _fileSystemMock.Verify(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject), Times.Once);
            _fileSystemMock.Verify(x => x.FileExists(appioprojPath), Times.Once);
        }

        [Test]
        public void FailOnAppioslnDeserialization([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrage
            var solutionName = inputParams.ElementAtOrDefault(1);
            var opcuaappName = inputParams.ElementAtOrDefault(3);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange appiosln file
			var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			_fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
			_fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(true);

			var solutionFullName = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			Stream slnMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(string.Empty));
            _fileSystemMock.Setup(x => x.ReadFile(solutionFullName)).Returns(slnMemoryStream);

            // Arrange appioproj file
            var appioprojPath = Path.Combine(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject);
            _fileSystemMock.Setup(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject)).Returns(appioprojPath);
            _fileSystemMock.Setup(x => x.FileExists(appioprojPath)).Returns(true);
            

            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsFalse(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.SlnCouldntDeserliazeSln), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnCouldntDeserliazeSln, solutionName), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);
            _fileSystemMock.Verify(x => x.ReadFile(solutionFullName), Times.Once);

			slnMemoryStream.Close();
			slnMemoryStream.Dispose();
        }

        [Test]
        public void FailOnAppioprojDeserialization([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrage
            var solutionName = inputParams.ElementAtOrDefault(1);
            var opcuaappName = inputParams.ElementAtOrDefault(3);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange appiosln file
			var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			_fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
			_fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(true);

			var solutionFullName = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			Stream slnMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_defaultAppioslnContent));
            _fileSystemMock.Setup(x => x.ReadFile(solutionFullName)).Returns(slnMemoryStream);

            // Arrange appioproj file
            var appioprojPath = Path.Combine(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject);
            _fileSystemMock.Setup(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject)).Returns(appioprojPath);
            _fileSystemMock.Setup(x => x.FileExists(appioprojPath)).Returns(true);

            Stream opcuaappMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(string.Empty));
            _fileSystemMock.Setup(x => x.ReadFile(appioprojPath)).Returns(opcuaappMemoryStream);

			
            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsFalse(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.SlnAddCouldntDeserliazeOpcuaapp), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnAddCouldntDeserliazeOpcuaapp, opcuaappName), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);
            _fileSystemMock.Verify(x => x.ReadFile(appioprojPath), Times.Once);

			slnMemoryStream.Close();
			slnMemoryStream.Dispose();
			opcuaappMemoryStream.Close();
			opcuaappMemoryStream.Dispose();
		}

        [Test]
        public void FailOnSlnAlreadyContainsOpcuaapp([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrage
            var solutionName = inputParams.ElementAtOrDefault(1);
            var opcuaappName = inputParams.ElementAtOrDefault(3);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange appiosln file
			var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			_fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
			_fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(true);

			var solutionFullName = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			Stream slnMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleAppioslnContent));
            _fileSystemMock.Setup(x => x.ReadFile(solutionFullName)).Returns(slnMemoryStream);

            // Arrange appioproj file
            var appioprojPath = Path.Combine(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject);
            _fileSystemMock.Setup(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject)).Returns(appioprojPath);
            _fileSystemMock.Setup(x => x.FileExists(appioprojPath)).Returns(true);

            Stream opcuaappMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleOpcuaClientAppContent));
            _fileSystemMock.Setup(x => x.ReadFile(appioprojPath)).Returns(opcuaappMemoryStream);


            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsFalse(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.SlnAddContainsOpcuaapp), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnAddContainsOpcuaapp, solutionName, opcuaappName), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);

			slnMemoryStream.Close();
			slnMemoryStream.Dispose();
			opcuaappMemoryStream.Close();
			opcuaappMemoryStream.Dispose();
		}

        [Test]
        public void AddOpcuaClientAppToDefaultSln([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
			// Arrange
			var solutionName = inputParams.ElementAtOrDefault(1);
            var opcuaappName = inputParams.ElementAtOrDefault(3);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange appiosln file
			var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			_fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
			_fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(true);

			var solutionFullName = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			Stream slnMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_defaultAppioslnContent));
            _fileSystemMock.Setup(x => x.ReadFile(solutionFullName)).Returns(slnMemoryStream);

            // Arrange appioproj file
            var appioprojPath = Path.Combine(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject);
            _fileSystemMock.Setup(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject)).Returns(appioprojPath);
            _fileSystemMock.Setup(x => x.FileExists(appioprojPath)).Returns(true);
            
            Stream opcuaappMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleOpcuaClientAppContent));
            _fileSystemMock.Setup(x => x.ReadFile(appioprojPath)).Returns(opcuaappMemoryStream);


            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsTrue(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.SlnAddSuccess), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnAddSuccess, opcuaappName, solutionName), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);
            _fileSystemMock.Verify(x => x.WriteFile(solutionFullName, It.IsAny<IEnumerable<string>>()), Times.Once);

			slnMemoryStream.Close();
			slnMemoryStream.Dispose();
			opcuaappMemoryStream.Close();
			opcuaappMemoryStream.Dispose();
		}

        [Test]
        public void AddOpcuaappToNotEmptySln([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
			// Arrange
			var solutionName = inputParams.ElementAtOrDefault(1);
            var opcuaappName = inputParams.ElementAtOrDefault(3);

            var loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange appiosln file
			var appioslnPath = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			_fileSystemMock.Setup(x => x.CombinePaths(solutionName + Constants.FileExtension.Appiosln)).Returns(appioslnPath);
			_fileSystemMock.Setup(x => x.FileExists(appioslnPath)).Returns(true);

			var solutionFullName = Path.Combine(solutionName + Constants.FileExtension.Appiosln);
			Stream slnMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleAppioslnContent));
            _fileSystemMock.Setup(x => x.ReadFile(solutionFullName)).Returns(slnMemoryStream);

            // Arrange appioproj file
            var appioprojPath = Path.Combine(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject);
            _fileSystemMock.Setup(x => x.CombinePaths(opcuaappName, opcuaappName + Constants.FileExtension.Appioproject)).Returns(appioprojPath);
            _fileSystemMock.Setup(x => x.FileExists(appioprojPath)).Returns(true);

            Stream opcuaappMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleOpcuaServerAppContent));
            _fileSystemMock.Setup(x => x.ReadFile(appioprojPath)).Returns(opcuaappMemoryStream);


            // Act
            var commandResult = _objectUnderTest.Execute(inputParams);


            // Assert
            Assert.IsTrue(commandResult.Success);
            Assert.IsNotNull(commandResult.OutputMessages);
            var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
            AppioLogger.RemoveListener(loggerListenerMock.Object);
            loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.SlnAddSuccess), Times.Once);
            Assert.AreEqual(string.Format(OutputText.SlnAddSuccess, opcuaappName, solutionName), firstMessageLine.Key);
            Assert.AreEqual(string.Empty, firstMessageLine.Value);
            _fileSystemMock.Verify(x => x.WriteFile(solutionFullName, It.IsAny<IEnumerable<string>>()), Times.Once);

			slnMemoryStream.Close();
			slnMemoryStream.Dispose();
			opcuaappMemoryStream.Close();
			opcuaappMemoryStream.Dispose();
		}
        
        [Test]
        public void FailOnIncorrectFlags([ValueSource(nameof(BadFlagParams))] string[] inputParams)
        {
	        var result = _objectUnderTest.Execute(inputParams);
            
	        Assert.IsFalse(result.Success);
	        var unknownParameterStart = string.Join(" ", OutputText.UnknownParameterProvided.Split().Take(2));
	        Assert.That(() => result.OutputMessages.First().Key.StartsWith(unknownParameterStart));
        }
    }
}
