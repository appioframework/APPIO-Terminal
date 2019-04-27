﻿using Oppo.Resources.text.logging;
using Oppo.Resources.text.output;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;

namespace Oppo.ObjectModel.CommandStrategies.ImportCommands
{
    public class ImportInformationModelCommandStrategy : ICommand<ImportStrategy>
    {
        private readonly IFileSystem _fileSystem;
		private readonly IModelValidator _modelValidator;
		private readonly MessageLines _outputMessages;

		public ImportInformationModelCommandStrategy(IFileSystem fileSystem, IModelValidator modelValidator)
        {
            _fileSystem = fileSystem;
			_modelValidator = modelValidator;
			_outputMessages = new MessageLines();
		}

        public string Name => Constants.ImportInformationModelCommandName.InformationModel;

		public CommandResult Execute(IEnumerable<string> inputParams)
		{
			var inputParamsList = inputParams.ToList();
			var nameFlag = inputParamsList.ElementAtOrDefault(0);
			var opcuaAppName = inputParamsList.ElementAtOrDefault(1);
			var pathFlag = inputParams.ElementAtOrDefault(2);
			var modelPath = inputParamsList.ElementAtOrDefault(3);
			var typesFlag = inputParamsList.ElementAtOrDefault(4);
			var typesPath = inputParamsList.ElementAtOrDefault(5);

			// opcuaapp name validation
			if (!ValidateOpcuaAppName(nameFlag, opcuaAppName))
			{
				return new CommandResult(false, _outputMessages);
			}

			// -s flag (temporary solution for now -> needs bigger design changes)
			if (pathFlag == Constants.ImportInformationModelCommandArguments.Sample || pathFlag == Constants.ImportInformationModelCommandArguments.VerboseSample)
			{
				var modelsDir = _fileSystem.CombinePaths(opcuaAppName, Constants.DirectoryName.Models);

				var nodesetContent = _fileSystem.LoadTemplateFile(Resources.Resources.SampleInformationModelFileName);
				var nodesetFilePath = _fileSystem.CombinePaths(modelsDir, Constants.FileName.SampleInformationModelFile);
				_fileSystem.CreateFile(nodesetFilePath, nodesetContent);

				var typesContent = _fileSystem.LoadTemplateFile(Resources.Resources.SampleInformationModelTypesFileName);
				var typesFilePath = _fileSystem.CombinePaths(modelsDir, Constants.FileName.SampleInformationModelTypesFile);
				_fileSystem.CreateFile(typesFilePath, typesContent);

				_outputMessages.Add(string.Format(OutputText.ImportSampleInformationModelSuccess, Constants.FileName.SampleInformationModelFile), string.Empty);
				OppoLogger.Info(string.Format(LoggingText.ImportInforamtionModelCommandSuccess, Constants.FileName.SampleInformationModelFile));
				return new CommandResult(true, _outputMessages);
			}

			// path flag validation
			if (!ValidateModel(pathFlag, modelPath))
			{
				return new CommandResult(false, _outputMessages);
			}
			var modelFileName = _fileSystem.GetFileName(modelPath);

			// types flag validation
			string typesFileName = string.Empty;
			if(typesFlag != null && !ValidateTypes(ref typesFileName, typesFlag, typesPath))
			{
				return new CommandResult(false, _outputMessages);
			}

			// here I should check if model is part of oppoproj
			// check if opcuaapp alrady has models with this name (uri later)
			var oppoprojFilePath = _fileSystem.CombinePaths(opcuaAppName, opcuaAppName + Constants.FileExtension.OppoProject);
			var opcuaappData = Deserialize.Opcuaapp(oppoprojFilePath, _fileSystem);
			if (opcuaappData == null)
			{
				OppoLogger.Warn(LoggingText.ImportInforamtionModelCommandFailureCannotReadOppoprojFile);
				_outputMessages.Add(OutputText.ImportInforamtionModelCommandFailureCannotReadOppoprojFile, string.Empty);
				return new CommandResult(false, _outputMessages);
			}
			if (opcuaappData.Type == Constants.ApplicationType.Client)
			{
				OppoLogger.Warn(LoggingText.ImportInformationModelCommandOpcuaappIsAClient);
				_outputMessages.Add(OutputText.ImportInformationModelCommandOpcuaappIsAClient, string.Empty);
				return new CommandResult(false, _outputMessages);
			}

			// here I should add the information to oppoproj file

			var modelData = new ModelData();
			modelData.Name = _fileSystem.GetFileName(modelFileName);
			if (!ExtractNodesetUris(ref modelData, modelPath))
			{
				return new CommandResult(false, _outputMessages);
			}
			modelData.Types = typesFileName;
			modelData.NamespaceVariable = "ns_" + _fileSystem.GetFileNameWithoutExtension(modelFileName);


			// check if oppoproj file already contains imported model
			if((opcuaappData as IOpcuaServerApp).Models.Any(x => x.Name == modelData.Name) || (opcuaappData as IOpcuaServerApp).Models.Any(x => x.Uri == modelData.Uri))
			{
				OppoLogger.Warn(LoggingText.ImportInforamtionModelCommandFailureModelDuplication);
				_outputMessages.Add(string.Format(OutputText.ImportInforamtionModelCommandFailureModelDuplication, opcuaAppName, modelFileName), string.Empty);
				return new CommandResult(false, _outputMessages);
			}
			
			(opcuaappData as IOpcuaServerApp).Models.Add(modelData);

			var oppoprojNewContent = JsonConvert.SerializeObject(opcuaappData, Newtonsoft.Json.Formatting.Indented);

			_fileSystem.WriteFile(oppoprojFilePath, new List<string> { oppoprojNewContent });

			// copy model file
			var modelsDirectory = _fileSystem.CombinePaths(opcuaAppName, Constants.DirectoryName.Models);            
            var targetModelFilePath = _fileSystem.CombinePaths(modelsDirectory, modelFileName);
            _fileSystem.CopyFile(modelPath, targetModelFilePath);

			// copy types file
			if (typesFlag != null)
			{
				var targetTypesFilePath = _fileSystem.CombinePaths(modelsDirectory, typesFileName);
				_fileSystem.CopyFile(typesPath, targetTypesFilePath);
			}

            OppoLogger.Info(string.Format(LoggingText.ImportInforamtionModelCommandSuccess, modelPath));
			_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandSuccess, modelPath), string.Empty);
            return new CommandResult(true, _outputMessages);
        }

		private bool ValidateOpcuaAppName(string nameFlag, string opcuaAppName)
		{
			// opcuaapp name flag validation
			if (nameFlag != Constants.ImportInformationModelCommandArguments.Name && nameFlag != Constants.ImportInformationModelCommandArguments.VerboseName)
			{
				OppoLogger.Warn(LoggingText.UnknownImportInfomrationModelCommandParam);
				_outputMessages.Add(OutputText.ImportInformationModelCommandUnknownParamFailure, string.Empty);
				return false;
			}

			// opcuaapp name validation
			if (string.IsNullOrEmpty(opcuaAppName))
			{
				OppoLogger.Warn(LoggingText.EmptyOpcuaappName);
				_outputMessages.Add(OutputText.ImportInformationModelCommandUnknownParamFailure, string.Empty);
				return false;
			}

			if (_fileSystem.GetInvalidFileNameChars().Any(opcuaAppName.Contains) || !_fileSystem.DirectoryExists(opcuaAppName))
			{
				OppoLogger.Warn(LoggingText.InvalidOpcuaappName);
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandInvalidOpcuaappName, opcuaAppName), string.Empty);
				return false;
			}

			return true;
		}

		private bool ValidateModel(string pathFlag, string modelPath)
		{
			// path flag validation
			if (pathFlag != Constants.ImportInformationModelCommandArguments.Path && pathFlag != Constants.ImportInformationModelCommandArguments.VerbosePath)
			{
				OppoLogger.Warn(LoggingText.UnknownImportInfomrationModelCommandParam);
				_outputMessages.Add(OutputText.ImportInformationModelCommandUnknownParamFailure, string.Empty);
				return false;
			}

			// model path validation
			if (string.IsNullOrEmpty(modelPath))
			{
				OppoLogger.Warn(LoggingText.InvalidInformationModelMissingModelFile);
				_outputMessages.Add(OutputText.ImportInformationModelCommandMissingModelPath, string.Empty);
				return false;
			}

			if (_fileSystem.GetInvalidPathChars().Any(modelPath.Contains))
			{
				OppoLogger.Warn(string.Format(LoggingText.InvalidInformationModelPath, modelPath));
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandInvalidModelPath, modelPath), string.Empty);
				return false;
			}

			if (!_fileSystem.FileExists(modelPath))
			{
				OppoLogger.Warn(string.Format(LoggingText.InvalidInformationModelNotExistingPath, modelPath));
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandNotExistingModelPath, modelPath), string.Empty);
				return false;
			}

			// model file name/extension validation
			var modelFileName = _fileSystem.GetFileName(modelPath);
			if (_fileSystem.GetExtension(modelPath) != Constants.FileExtension.InformationModel)
			{
				OppoLogger.Warn(string.Format(LoggingText.InvalidInformationModelExtension, modelFileName));
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandInvalidModelExtension, modelFileName), string.Empty);
				return false;
			}

			// validate model against UANodeSet xsd file
			if (!_modelValidator.Validate(modelPath, Resources.Resources.UANodeSetXsdFileName))
			{
				OppoLogger.Warn(string.Format(LoggingText.NodesetValidationFailure, modelPath));
				_outputMessages.Add(string.Format(OutputText.NodesetValidationFailure, modelPath), string.Empty);
				return false;
			}

			return true;
		}

		private bool ValidateTypes(ref string typesFileName, string typesFlag, string typesPath)
		{
			if(typesFlag != Constants.ImportInformationModelCommandArguments.Types && typesFlag != Constants.ImportInformationModelCommandArguments.VerboseTypes)
			{
				OppoLogger.Warn(LoggingText.ImportInformationModelCommandFailureInvalidTypesFlag);
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandFailureInvalidTypesFlag, typesFlag), string.Empty);
				return false;
			}
			
			if (string.IsNullOrEmpty(typesPath))
			{
				OppoLogger.Warn(LoggingText.ImportInformationModelCommandFailureMissingTypesName);
				_outputMessages.Add(OutputText.ImportInformationModelCommandFailureMissingTypesName, string.Empty);
				return false;
			}

			typesFileName = _fileSystem.GetFileName(typesPath);
			if (_fileSystem.GetExtension(typesPath) != Constants.FileExtension.ModelTypes)
			{
				OppoLogger.Warn(LoggingText.ImportInformationModelCommandFailureTypesHasInvalidExtension);
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandFailureTypesHasInvalidExtension, typesFileName), string.Empty);
				return false;
			}

			if (!_fileSystem.FileExists(typesPath))
			{
				OppoLogger.Warn(LoggingText.ImportInformationModelCommandFailureTypesFileDoesNotExist);
				_outputMessages.Add(string.Format(OutputText.ImportInformationModelCommandFailureTypesFileDoesNotExist, typesPath), string.Empty);
				return false;
			}

			return true;
		}

		private bool ExtractNodesetUris(ref ModelData modelData, string nodesetPath)
		{
			XmlDocument nodesetXml = new XmlDocument();
			using (var nodesetStream = _fileSystem.ReadFile(nodesetPath))
			{
				StreamReader reader = new StreamReader(nodesetStream);
				var xmlFileContent = reader.ReadToEnd();
				nodesetXml.LoadXml(xmlFileContent);
			}
			
			var nsmgr = new XmlNamespaceManager(nodesetXml.NameTable);
			nsmgr.AddNamespace("ns", new UriBuilder("http", "opcfoundation.org", -1, "UA/2011/03/UANodeSet.xsd").ToString());
			var modelNode = nodesetXml.SelectSingleNode("//ns:UANodeSet//ns:Models//ns:Model", nsmgr);

			if(modelNode == null || modelNode.Attributes == null || modelNode.Attributes["ModelUri"] == null)
			{
				OppoLogger.Warn(LoggingText.ImportInforamtionModelCommandFailureModelMissingUri);
				_outputMessages.Add(string.Format(OutputText.ImportInforamtionModelCommandFailureModelMissingUri, nodesetPath), string.Empty);
				return false;
			}

			modelData.Uri = modelNode.Attributes["ModelUri"].Value;

			if(modelNode.ChildNodes.Count > 0)
			{
				for(int index = 0; index < modelNode.ChildNodes.Count; index++)
				{
					modelData.RequiredModelUris.Add(modelNode.ChildNodes[index].Attributes["ModelUri"].Value);
				}
			}

			return true;
		}

		public string GetHelpText()
        {
            return string.Empty;
        }
    }
}