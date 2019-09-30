﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Appio.ObjectModel.Exceptions;
using Appio.Resources.text.logging;

namespace Appio.ObjectModel
{
    public class CommandFactory<TDependance> : ICommandFactory<TDependance>
    {
        private readonly Dictionary<string, ICommand<TDependance>> _commands = new Dictionary<string, ICommand<TDependance>>();
        private readonly string _nameOfDefaultCommand;

        public CommandFactory(IEnumerable<ICommand<TDependance>> commandArray, string nameOfDefaultCommand)
        {
            if (commandArray == null)
            {
                throw new ArgumentNullException(nameof(commandArray));
            }           
            
            if (!commandArray.Any())
            {
                throw new ArgumentNullException(nameof(commandArray));
            }

            if (string.IsNullOrEmpty(nameOfDefaultCommand))
            {
                throw new ArgumentNullException(nameof(nameOfDefaultCommand));
            }

            _nameOfDefaultCommand = nameOfDefaultCommand;

            foreach (var command in commandArray)
            {
                if (_commands.ContainsKey(command.Name))
                {
                    throw new DuplicateNameException(command.Name);
                }

                _commands.Add(command.Name, command);
            }

            if (!_commands.ContainsKey(nameOfDefaultCommand))
            {
                throw new ArgumentOutOfRangeException(nameof(nameOfDefaultCommand));
            }
        }

        public IEnumerable<ICommand<TDependance>> Commands => _commands.Values;

        public ICommand<TDependance> GetCommand(string commandName)
        {            
            if (string.IsNullOrEmpty(commandName))
            {
                return _commands[_nameOfDefaultCommand];
            }

            if (_commands.ContainsKey(commandName))
            {
                return _commands[commandName];
            }                        

            return new FallbackCommand();
        }

        private class FallbackCommand : ICommand<TDependance>
        {
            public string Name => string.Empty;

            public CommandResult Execute(IEnumerable<string> inputParams)
            {
                AppioLogger.Warn(LoggingText.UnknownCommandCalled);
                var outputMessages = new MessageLines();
                outputMessages.Add(Constants.CommandResults.Failure, string.Empty);
                return new CommandResult(false, outputMessages);
            }

            public string GetHelpText()
            {
                return string.Empty;
            }
        }
    }
}
