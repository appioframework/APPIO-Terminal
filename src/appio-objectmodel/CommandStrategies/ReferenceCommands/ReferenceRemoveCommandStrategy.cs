﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using Appio.Resources.text.logging;
using Appio.Resources.text.output;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Appio.ObjectModel.CommandStrategies.ReferenceCommands
{
	public class ReferenceRemoveCommandStrategy : ReferenceBase
	{
		public ReferenceRemoveCommandStrategy(IFileSystem fileSystem) : base(fileSystem) { }

		public override string Name => Constants.ReferenceCommandArguments.Remove;

		public override CommandResult Execute(IEnumerable<string> inputParams)
		{
			if (!ExecuteCommon(inputParams))
			{
				return new CommandResult(false, _outputMessages);
			}

			// deserialise client file
			OpcuaClientApp opcuaClient = null;
			OpcuaClientServerApp opcuaClientServer = null;
			RefUtility.DeserializeClient(ref opcuaClient, ref opcuaClientServer, _clientFullName, _fileSystem);
			if (opcuaClient == null && opcuaClientServer == null)
			{
				AppioLogger.Warn(LoggingText.ReferenceCouldntDeserliazeClient);
				_outputMessages.Add(string.Format(OutputText.ReferenceCouldntDeserliazeClient, _clientFullName), string.Empty);
				return new CommandResult(false, _outputMessages);
			}

			// check if server is part of client's reference and remove it
			string clientNewContent = string.Empty;
			IOpcuaServerApp serverReference = null;
			if (opcuaClientServer != null && (serverReference = opcuaClientServer.ServerReferences.SingleOrDefault(x => x.Name == _serverName)) != null)
			{
				opcuaClientServer.ServerReferences.Remove(serverReference);
				clientNewContent = JsonConvert.SerializeObject(opcuaClientServer, Formatting.Indented);
			}
			else if (opcuaClient != null && (serverReference = opcuaClient.ServerReferences.SingleOrDefault(x => x.Name == _serverName)) != null)
			{
				opcuaClient.ServerReferences.Remove(serverReference);
				clientNewContent = JsonConvert.SerializeObject(opcuaClient, Formatting.Indented);
			}
			else
			{
				AppioLogger.Warn(LoggingText.ReferenceRemoveServerIsNotInClient);
				_outputMessages.Add(string.Format(OutputText.ReferenceRemoveServerIsNotInClient, _serverName, _clientName), string.Empty);
				return new CommandResult(false, _outputMessages);
			}
			_fileSystem.WriteFile(_clientFullName, new List<string> { clientNewContent });

			// exit method with success
			AppioLogger.Info(LoggingText.ReferenceRemoveSuccess);
			_outputMessages.Add(string.Format(OutputText.ReferenceRemoveSuccess, _clientName, _serverName), string.Empty);
			return new CommandResult(true, _outputMessages);
		}

		public override string GetHelpText()
		{
			return Resources.text.help.HelpTextValues.ReferenceRemoveNameArgumentCommandDescription;
		}

	}
}