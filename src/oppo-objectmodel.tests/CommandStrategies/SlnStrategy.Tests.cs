﻿using Moq;
using NUnit.Framework;
using Oppo.ObjectModel.CommandStrategies.SlnCommands;
using System.Collections.Generic;

namespace Oppo.ObjectModel.Tests.CommandStrategies
{
    public class SlnStrategyTests
    {
        private Mock<ICommandFactory<SlnStrategy>> _factoryMock;
        private SlnStrategy _objectUnderTest;

        [SetUp]
        public void SetUp_ObjectUnderTest()
        {
            _factoryMock = new Mock<ICommandFactory<SlnStrategy>>();
            _objectUnderTest = new SlnStrategy(_factoryMock.Object);
        }

        [Test]
        public void SlnStrategy_Should_ImplementICommandOfObjectModel()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsInstanceOf<ICommand<ObjectModel>>(_objectUnderTest);
        }

        [Test]
        public void SlnStrategy_Should_HaveCorrectCommandName()
        {
            // Arrange

            // Act
            var name = _objectUnderTest.Name;

            // Assert
            Assert.AreEqual(Constants.CommandName.Sln, name);
        }

        [Test]
        public void SlnStrategy_Should_ProvideSomeHelpText()
        {
            // Arrange

            // Act
            var helpText = _objectUnderTest.GetHelpText();

            // Assert
            Assert.AreEqual(Resources.text.help.HelpTextValues.SlnCommand, helpText);
        }

        [Test]
        public void SlnStrategy_Should_ExecuteCommand()
        {
            // Arrange
            var inputParams = new[] { "--any-param", "any-value" };
            var commandResultMock = new CommandResult(true, new MessageLines() { { "any-message", string.Empty } });

            var commandMock = new Mock<ICommand<SlnStrategy>>();
            commandMock.Setup(x => x.Execute(It.IsAny<string[]>())).Returns(commandResultMock);

            _factoryMock.Setup(x => x.GetCommand("--any-param")).Returns(commandMock.Object);

            // Act
            var result = _objectUnderTest.Execute(inputParams);

            // Assert
            Assert.AreEqual(commandResultMock, result);
        }
    }
}