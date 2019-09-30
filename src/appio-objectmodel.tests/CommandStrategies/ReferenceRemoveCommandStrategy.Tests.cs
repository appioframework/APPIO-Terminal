﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System.Linq;
using Moq;
using NUnit.Framework;
using Appio.ObjectModel.CommandStrategies.ReferenceCommands;
using Appio.Resources.text.output;
using System.Text;
using System.IO;

namespace Appio.ObjectModel.Tests.CommandStrategies
{
	public class ReferenceRemoveComandStrategyTestsShould
	{
		protected static string[][] ValidInputs()
		{
			return new[]
			{
				new [] { "-c", "testClient", "-s", "testServer" },
				new [] { "-c", "testClient", "--server", "testServer" },
				new [] { "--client", "testClient", "-s", "testServer" },
				new [] { "--client", "testClient", "--server", "testServer" },
			};
		}

		protected static string[][] InvalidInputs_UnknownClientParam()
		{
			return new[]
			{
				new [] { "--c", "testClient", "-s", "testServer" },
				new [] { "--Client", "testClient", "-s", "testServer" },
				new [] { "-C", "testClient", "--server", "testServer" },
				new [] { "-client", "testClient", "--server", "testServer" },
			};
		}

		protected static string[][] InvalidInputs_UnknownServerParam()
		{
			return new[]
			{
				new [] { "-c", "testClient", "--s", "testserver" },
				new [] { "-c", "testClient", "-server", "testServer" },
				new [] { "-c", "testClient", "-S", "testServer" },
				new [] { "-c", "testClient", "--Server", "testServer" },
			};
		}

		private Mock<IFileSystem> _fileSystemMock;
		private ReferenceRemoveCommandStrategy _objectUnderTest;

		private readonly string _defaultClientAppioprojContent = "{\"name\":\"testClient\",\"type\":\"Client\",\"references\":[]}";
		private readonly string _sampleClientAppioprojContent = "{\"name\":\"testClient\",\"type\":\"Client\",\"references\":[{\"name\":\"testServer\", \"type\": \"Server\",\"url\":\"127.0.0.1\",\"port\": \"4000\"}]}";
		private readonly string _sampleClientServerAppioprojContent = "{\"name\":\"testClientServer\",\"type\":\"ClientServer\",\"url\":\"127.0.0.1\",\"port\":\"3000\",\"references\":[{\"name\":\"testServer\",\"type\":\"Server\",\"url\":\"127.0.0.1\",\"port\":\"4000\"}]}";

		[SetUp]
		public void SetUp_ObjectUnderTest()
		{
			_fileSystemMock = new Mock<IFileSystem>();
			_objectUnderTest = new ReferenceRemoveCommandStrategy(_fileSystemMock.Object);
		}

		[Test]
		public void ImplementICommandOfReferenceRemoveStrategy()
		{
			// Arrange

			// Act

			// Assert
			Assert.IsInstanceOf<ICommand<ReferenceStrategy>>(_objectUnderTest);
		}
		[Test]
		public void HaveCorrectCommandName()
		{
			// Arrange

			// Act
			var name = _objectUnderTest.Name;

			// Assert
			Assert.AreEqual(Constants.ReferenceCommandArguments.Remove, name);
		}

		[Test]
		public void HaveCorrectHelpText()
		{
			// Arrange

			// Act
			var helpText = _objectUnderTest.GetHelpText();

			// Assert
			Assert.AreEqual(Resources.text.help.HelpTextValues.ReferenceRemoveNameArgumentCommandDescription, helpText);
		}

		[Test]
		public void FailOnUnknownServerParametar([ValueSource(nameof(InvalidInputs_UnknownServerParam))] string[] inputParams)
		{
			// Arrange
			var clientName = inputParams.ElementAt(1);
			var serverNameFlag = inputParams.ElementAtOrDefault(2);

			// Arrange client file
			var appioprojectPath = Path.Combine(clientName, clientName + Constants.FileExtension.Appioproject);
			_fileSystemMock.Setup(x => x.CombinePaths(clientName, clientName+ Constants.FileExtension.Appioproject)).Returns(appioprojectPath);
			_fileSystemMock.Setup(x => x.FileExists(appioprojectPath)).Returns(true);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Act
			var commandResult = _objectUnderTest.Execute(inputParams);

			// Assert
			Assert.IsFalse(commandResult.Success);
			Assert.IsNotNull(commandResult.OutputMessages);
			var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
			AppioLogger.RemoveListener(loggerListenerMock.Object);
			loggerListenerMock.Verify(x => x.Warn(It.IsAny<string>()), Times.Once);
			Assert.AreEqual(string.Empty, firstMessageLine.Value);
		}

		[Test]
		public void FailOnUnknownClientParametar([ValueSource(nameof(InvalidInputs_UnknownClientParam))] string[] inputParams)
		{
			// Arrange
			var clientNameFlag = inputParams.ElementAtOrDefault(0);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Act
			var commandResult = _objectUnderTest.Execute(inputParams);

			// Assert
			Assert.IsFalse(commandResult.Success);
			Assert.IsNotNull(commandResult.OutputMessages);
			var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
			AppioLogger.RemoveListener(loggerListenerMock.Object);
			loggerListenerMock.Verify(x => x.Warn(It.IsAny<string>()), Times.Once);
			Assert.AreEqual(string.Empty, firstMessageLine.Value);
		}

		[Test]
		public void FailBecauseOfMissingClientFile([ValueSource(nameof(ValidInputs))] string[] inputParams)
		{
			// Arrange
			var clientName = inputParams.ElementAtOrDefault(1);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange client file
			var appioprojectPath = Path.Combine(clientName, clientName + Constants.FileExtension.Appioproject);
			_fileSystemMock.Setup(x => x.CombinePaths(clientName, clientName + Constants.FileExtension.Appioproject)).Returns(appioprojectPath);
			_fileSystemMock.Setup(x => x.FileExists(appioprojectPath)).Returns(false);


			// Act
			var commandResult = _objectUnderTest.Execute(inputParams);


			// Assert
			Assert.IsFalse(commandResult.Success);
			Assert.IsNotNull(commandResult.OutputMessages);
			var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
			AppioLogger.RemoveListener(loggerListenerMock.Object);
			loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.ReferenceClientAppioprojFileNotFound), Times.Once);
			Assert.AreEqual(string.Format(OutputText.ReferenceClientAppioprojFileNotFound, appioprojectPath), firstMessageLine.Key);
			Assert.AreEqual(string.Empty, firstMessageLine.Value);
			_fileSystemMock.Verify(x => x.CombinePaths(clientName, clientName + Constants.FileExtension.Appioproject), Times.Once);
			_fileSystemMock.Verify(x => x.FileExists(appioprojectPath), Times.Once);
		}

		[Test]
		public void FailBeacuseClientDeserializationReturnsNull([ValueSource(nameof(ValidInputs))] string[] inputParams)
		{
			// Arrage
			var clientName = inputParams.ElementAtOrDefault(1);
			var serverName = inputParams.ElementAtOrDefault(3);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange client file
			var appioClientPath = Path.Combine(clientName, clientName + Constants.FileExtension.Appioproject);
			_fileSystemMock.Setup(x => x.CombinePaths(clientName, clientName + Constants.FileExtension.Appioproject)).Returns(appioClientPath);
			_fileSystemMock.Setup(x => x.FileExists(appioClientPath)).Returns(true);

			using (var clientMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(string.Empty)))
			{
				_fileSystemMock.Setup(x => x.ReadFile(appioClientPath)).Returns(clientMemoryStream);

				// Act
				var commandResult = _objectUnderTest.Execute(inputParams);

				// Assert
				Assert.IsFalse(commandResult.Success);
				Assert.IsNotNull(commandResult.OutputMessages);
				var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
				AppioLogger.RemoveListener(loggerListenerMock.Object);
				loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.ReferenceCouldntDeserliazeClient), Times.Once);
				Assert.AreEqual(string.Format(OutputText.ReferenceCouldntDeserliazeClient, appioClientPath), firstMessageLine.Key);
				Assert.AreEqual(string.Empty, firstMessageLine.Value);
			}
		}

		[Test]
		public void FailBecauseTryingToRemoveServerWhichIsNotPartOfClientReferenceList([ValueSource(nameof(ValidInputs))] string[] inputParams)
		{
			// Arrage
			var clientName = inputParams.ElementAtOrDefault(1);
			var serverName = inputParams.ElementAtOrDefault(3);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange client file
			var appioClientPath = Path.Combine(clientName, clientName + Constants.FileExtension.Appioproject);
			_fileSystemMock.Setup(x => x.CombinePaths(clientName, clientName + Constants.FileExtension.Appioproject)).Returns(appioClientPath);
			_fileSystemMock.Setup(x => x.FileExists(appioClientPath)).Returns(true);

			using (var clientMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_defaultClientAppioprojContent)))
			{
				_fileSystemMock.Setup(x => x.ReadFile(appioClientPath)).Returns(clientMemoryStream);

				// Act
				var commandResult = _objectUnderTest.Execute(inputParams);

				// Assert
				Assert.IsFalse(commandResult.Success);
				Assert.IsNotNull(commandResult.OutputMessages);
				var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
				AppioLogger.RemoveListener(loggerListenerMock.Object);
				loggerListenerMock.Verify(x => x.Warn(Resources.text.logging.LoggingText.ReferenceRemoveServerIsNotInClient), Times.Once);
				Assert.AreEqual(string.Format(OutputText.ReferenceRemoveServerIsNotInClient, serverName, clientName), firstMessageLine.Key);
				Assert.AreEqual(string.Empty, firstMessageLine.Value);
			}
		}

		[Test]
		public void RemoveServerFormClient([ValueSource(nameof(ValidInputs))] string[] inputParams)
		{
			// Arrage
			var clientName = inputParams.ElementAtOrDefault(1);
			var serverName = inputParams.ElementAtOrDefault(3);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange client file
			var appioClientPath = Path.Combine(clientName, clientName + Constants.FileExtension.Appioproject);
			_fileSystemMock.Setup(x => x.CombinePaths(clientName, clientName + Constants.FileExtension.Appioproject)).Returns(appioClientPath);
			_fileSystemMock.Setup(x => x.FileExists(appioClientPath)).Returns(true);

			using (var clientMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleClientAppioprojContent)))
			{
				_fileSystemMock.Setup(x => x.ReadFile(appioClientPath)).Returns(clientMemoryStream);

				// Act
				var commandResult = _objectUnderTest.Execute(inputParams);

				// Assert
				Assert.IsTrue(commandResult.Success);
				Assert.IsNotNull(commandResult.OutputMessages);
				var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
				AppioLogger.RemoveListener(loggerListenerMock.Object);
				loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.ReferenceRemoveSuccess), Times.Once);
				Assert.AreEqual(string.Format(OutputText.ReferenceRemoveSuccess, clientName, serverName), firstMessageLine.Key);
				Assert.AreEqual(string.Empty, firstMessageLine.Value);
			}
		}

		[Test]
		public void RemoveServerFormClientServer([ValueSource(nameof(ValidInputs))] string[] inputParams)
		{
			// Arrage
			var clientName = inputParams.ElementAtOrDefault(1);
			var serverName = inputParams.ElementAtOrDefault(3);

			var loggerListenerMock = new Mock<ILoggerListener>();
			AppioLogger.RegisterListener(loggerListenerMock.Object);

			// Arrange client file
			var appioClientPath = Path.Combine(clientName, clientName + Constants.FileExtension.Appioproject);
			_fileSystemMock.Setup(x => x.CombinePaths(clientName, clientName + Constants.FileExtension.Appioproject)).Returns(appioClientPath);
			_fileSystemMock.Setup(x => x.FileExists(appioClientPath)).Returns(true);

			using (var clientMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleClientServerAppioprojContent)))
			using (var clientServerMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_sampleClientServerAppioprojContent)))
			{
				_fileSystemMock.SetupSequence(x => x.ReadFile(appioClientPath)).Returns(clientMemoryStream).Returns(clientServerMemoryStream);

				// Act
				var commandResult = _objectUnderTest.Execute(inputParams);

				// Assert
				Assert.IsTrue(commandResult.Success);
				Assert.IsNotNull(commandResult.OutputMessages);
				var firstMessageLine = commandResult.OutputMessages.FirstOrDefault();
				AppioLogger.RemoveListener(loggerListenerMock.Object);
				loggerListenerMock.Verify(x => x.Info(Resources.text.logging.LoggingText.ReferenceRemoveSuccess), Times.Once);
				Assert.AreEqual(string.Format(OutputText.ReferenceRemoveSuccess, clientName, serverName), firstMessageLine.Key);
				Assert.AreEqual(string.Empty, firstMessageLine.Value);
			}
		}
	}
}

