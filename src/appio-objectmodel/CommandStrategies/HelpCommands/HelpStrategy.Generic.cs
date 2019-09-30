﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System.Collections.Generic;
using System.Linq;

namespace Appio.ObjectModel.CommandStrategies.HelpCommands
{
    public class HelpStrategy<TDependance> : ICommand<TDependance>
    {
        private readonly HelpData _helpData;

        public HelpStrategy(HelpData helpData)
        {
            _helpData = helpData.Clone();
        }

        public ICommandFactory<TDependance> CommandFactory { get; set; }

        public string Name => _helpData.CommandName;

        public CommandResult Execute(IEnumerable<string> inputParams)
        {
			if (_helpData.Arguments.Count() != 0)
			{
				_helpData.Arguments.Sort();
				// add arguments header
				_helpData.Arguments = new MessageLines()
				{
					{string.Empty, string.Empty },
					{ Resources.text.help.HelpTextValues.GeneralArguments, string.Empty },
					_helpData.Arguments
				};
			}

			if (_helpData.Options.Count() != 0)
			{
				_helpData.Options.Sort();
				// add options header
				_helpData.Options = new MessageLines()
				{
					{string.Empty, string.Empty },
					{ Resources.text.help.HelpTextValues.GeneralOptions, string.Empty },
					_helpData.Options
				};
			}

			// put all help data into one structure
            var outputMessages = new MessageLines
            {
                _helpData.HelpTextFirstLine,
				_helpData.Arguments,
				_helpData.Options,
                _helpData.HelpTextLastLine
			};

            AppioLogger.Info(_helpData.LogMessage);
            return new CommandResult(true, outputMessages);            
        }

        public string GetHelpText()
        {
            return _helpData.HelpText;
        }
    }
}