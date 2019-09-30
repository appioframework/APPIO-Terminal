﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Appio.Resources.text.output;
using Appio.Resources.text.logging;

namespace Appio.ObjectModel.CommandStrategies.GenerateCommands
{
    public class GenerateInformationModelStrategy : ICommand<GenerateStrategy>
	{
		private readonly IFileSystem _fileSystem;
		private readonly INodesetGenerator _nodesetGenerator;

		private enum ParamId {AppName, ModelFullName, TypesFullName, RequiredModelFullName}

        private readonly ParameterResolver<ParamId> _resolver;

        public GenerateInformationModelStrategy(string commandName, IFileSystem fileSystem, IModelValidator modelValidator, INodesetGenerator nodesetGenerator)
        {
            Name = commandName;
			_fileSystem = fileSystem;
			_nodesetGenerator = nodesetGenerator;

			_resolver = new ParameterResolver<ParamId>(Constants.CommandName.Generate + " " + Name, new []
            {

                new StringParameterSpecification<ParamId>
                {
                    Identifier = ParamId.AppName,
                    Short = Constants.GenerateCommandOptions.Name,
                    Verbose = Constants.GenerateCommandOptions.VerboseName
                }
            });
        }

        public string Name { get; private set; }

        public CommandResult Execute(IEnumerable<string> inputParams)
        {
            var (error, stringParams, _) = _resolver.ResolveParams(inputParams);
            
            if (error != null)
                return new CommandResult(false, new MessageLines{{error, string.Empty}});

            var projectName = stringParams[ParamId.AppName];

            var outputMessages = new MessageLines();

			// deserialize appioproj file
			var appioprojFilePath = _fileSystem.CombinePaths(projectName, projectName + Constants.FileExtension.Appioproject);
			var opcuaappData = Deserialize.Opcuaapp(appioprojFilePath, _fileSystem);
			if (opcuaappData == null)
			{
				AppioLogger.Warn(LoggingText.GenerateInformationModelFailureCouldntDeserliazeOpcuaapp);
				outputMessages.Add(string.Format(OutputText.GenerateInformationModelFailureCouldntDeserliazeOpcuaapp, projectName, appioprojFilePath), string.Empty);
				return new CommandResult(false, outputMessages);
			}
			if((opcuaappData as IOpcuaClientApp)?.Type == Constants.ApplicationType.Client)
			{
				AppioLogger.Warn(LoggingText.GenerateInformationModelFailuteOpcuaappIsAClient);
				outputMessages.Add(string.Format(OutputText.GenerateInformationModelFailuteOpcuaappIsAClient, projectName), string.Empty);
				return new CommandResult(false, outputMessages);
			}

			var opcuaappModels = (opcuaappData as IOpcuaServerApp)?.Models;

			// check if models are valid
			if(!ValidateModels(opcuaappModels))
			{
				AppioLogger.Warn(LoggingText.GenerateInformationModelInvalidModelsList);
				outputMessages.Add(string.Format(OutputText.GenerateInformationModelInvalidModelsList, projectName), string.Empty);
				return new CommandResult(false, outputMessages);
			}

			// check if there is any circular dependency between models
			if(SearchForCircularDependencies(opcuaappModels))
			{
				AppioLogger.Warn(LoggingText.GenerateInformationModelCircularDependency);
				outputMessages.Add(string.Format(OutputText.GenerateInformationModelCircularDependency, projectName), string.Empty);
				return new CommandResult(false, outputMessages);
			}

			// sort models
			SortModels(opcuaappModels);

			// generate models
			foreach (var model in opcuaappModels)
			{
				var requiredModelData = GetListOfRequiredModels(opcuaappModels, model);

				if (!_nodesetGenerator.GenerateTypesSourceCodeFiles(projectName, model) || !_nodesetGenerator.GenerateNodesetSourceCodeFiles(projectName, model, requiredModelData))
				{
					outputMessages.Add(_nodesetGenerator.GetOutputMessage(), string.Empty);
					return new CommandResult(false, outputMessages);
				}
			}

			// add noodeset variables
			CreateNamespaceVariables(projectName, opcuaappModels);

			// exit method with positive result
			AppioLogger.Info(LoggingText.GenerateInformationModelSuccess);
			outputMessages.Add(string.Format(OutputText.GenerateInformationModelSuccess, projectName), string.Empty);
            return new CommandResult(true, outputMessages);           
        }

		private bool ValidateModels(List<IModelData> models)
		{
			// validate each and every model
			foreach(var model in models)
			{
				// check for model duplications
				if (models.Count(x => x.Name == model.Name || x.Uri == model.Uri) > 1)
				{
					return false;
				}

				// check if all required models exists
				foreach(var requiredModelUri in model.RequiredModelUris)
				{
					if (!models.Where(x => x.Name != model.Name).Any(x => x.Uri == requiredModelUri))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool SearchForCircularDependencies(List<IModelData> models)
		{
			// check each and every model for circular dependencies
			foreach(var model in models)
			{
				var visitedModelUris = new List<string>();
				if(CheckSingleModelForCircularDependencies(models, model.Uri, ref visitedModelUris))
				{
					return true;
				}
			}
			return false;
		}

		private bool CheckSingleModelForCircularDependencies(List<IModelData> models, string uri, ref List<string> listOfRegistredUris)
		{
			// add currently checked uri to the stack
			listOfRegistredUris.Add(uri);

			// if currently checked uri appears more then once on the list then we have circular dependency
			if(listOfRegistredUris.GroupBy(x => x).Any(x => x.Count() > 1))
			{
				return true;
			}

			// if there are any then repeat for required models of currently check model
			var requiredModel = models.Find(x => x.Uri == uri);
			if(requiredModel.RequiredModelUris.Count > 0)
			{
				foreach(var requiredModelUri in requiredModel.RequiredModelUris)
				{
					// pass true if circular dependency found in one of requried models
					if(CheckSingleModelForCircularDependencies(models, requiredModelUri, ref listOfRegistredUris))
					{
						return true;
					}
					// clean last item on the stack if circular dependency not found
					else
					{
						listOfRegistredUris.RemoveAt(listOfRegistredUris.Count - 1);
					}
				}
			}
			return false;
		}

		private void SortModels(List<IModelData> models)
		{
			// iterate through models from the first to one before the last (last model is skipped since there is no other model to compare with)
			for(int firstModelIndex = 0; firstModelIndex < models.Count - 1; )
			{
				bool swapped = false;
				// iterate through models from the currently selected to the last (currently selected model is skipped since there is no need to compare it to itself)
				for(int secondModelIndex = firstModelIndex + 1; secondModelIndex < models.Count; secondModelIndex++)
				{
					for(int requiredModelIndex = 0; requiredModelIndex < models[firstModelIndex].RequiredModelUris.Count; requiredModelIndex++)
					{
						// swap models if required model of first model is equal to second model (second model has to be higher in hierarchy)
						if(models[firstModelIndex].RequiredModelUris[requiredModelIndex] == models[secondModelIndex].Uri)
						{
							(models[firstModelIndex], models[secondModelIndex]) = (models[secondModelIndex], models[firstModelIndex]);
							swapped = true;
							break;
						}
					}
					if(swapped)
					{
						break;
					}
				}
				if(!swapped)
				{
					firstModelIndex++;
				}
			}
		}

		private List<RequiredModelsData> GetListOfRequiredModels(List<IModelData> models, IModelData model)
		{
			var result = new List<RequiredModelsData>();

			// for each required model extract model name and set boolean flag if extra types are required
			foreach(var requiredModelUri in model.RequiredModelUris)
			{
				var requiredModel = models.SingleOrDefault(x => x.Uri == requiredModelUri);
				result.Add(new RequiredModelsData(requiredModel.Name, requiredModel.Types != string.Empty));
			}

			return result;
		}

		private void CreateNamespaceVariables(string projectName, List<IModelData> models)
		{
			// get content of mainCallbacks.c file
			var mainCallbacksFilePath = _fileSystem.CombinePaths(projectName, Constants.DirectoryName.SourceCode, Constants.DirectoryName.ServerApp, Constants.FileName.SourceCode_mainCallbacks_c);
			
			var mainCallbacksFileContent = new List<string>();
			using (var constantsFileStream = _fileSystem.ReadFile(mainCallbacksFilePath))
			{
				// convert file stream to list of strings
				using (var reader = new StreamReader(constantsFileStream))
				{
					while (!reader.EndOfStream)
					{
						mainCallbacksFileContent.Add(reader.ReadLine());
					}
				}

				// add namespace variables to content of mainCallbacks.c file
				AddNamespaceVariablesToMainCallbacksFileContent(models, ref mainCallbacksFileContent);
			}

			// write mainCallbacks.c content back to the file
			_fileSystem.WriteFile(mainCallbacksFilePath, mainCallbacksFileContent);
		}

		private void AddNamespaceVariablesToMainCallbacksFileContent(List<IModelData> models, ref List<string> mainCallbacksFileContent)
		{
			// model counter for namespace variable value
			// 0 used by OPC UA basic nodeset
			// 1 used by server application for own nodes
			// first generated model starts with value of 2
			uint variableCounter = 2;

			// for each model generate namespace variable and add it to mainCallbacks.c file
			foreach (var model in models)
			{
				var namespaceVariableTypeAndName = new StringBuilder(Constants.ServerConstants.ServerAppNamespaceVariable).Append(model.NamespaceVariable).ToString();
				var namespaceVariableFullDefinition = new StringBuilder(namespaceVariableTypeAndName).Append(" = ").Append(variableCounter).Append(";").ToString();

				// check if namespace variable already exists
				var namespaceVariableLineIndex = mainCallbacksFileContent.FindIndex(x => x.Contains(namespaceVariableTypeAndName));

				// if namespace variable already exists then rewrite it
				if (namespaceVariableLineIndex != Constants.NumericValues.TextNotFound)
				{
					mainCallbacksFileContent.RemoveAt(namespaceVariableLineIndex);
					mainCallbacksFileContent.Insert(namespaceVariableLineIndex, namespaceVariableFullDefinition);
				}
				// if namespace variable did not exist until now add it
				else
				{
					var includeOpenHLineIndex = mainCallbacksFileContent.FindIndex(x => x.Contains(Constants.ServerConstants.ServerAppOpen62541Include));
					mainCallbacksFileContent.Insert(includeOpenHLineIndex + Constants.NumericValues.StartInNewLine, namespaceVariableFullDefinition);
				}
				variableCounter++;
			}
		}

		public string GetHelpText()
        {
            return Resources.text.help.HelpTextValues.GenerateInformationModelCommandDescription;
        }
    }
}