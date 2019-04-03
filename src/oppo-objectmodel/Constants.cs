namespace Oppo.ObjectModel
{
    public static class Constants
    {
        public const string HelloString = "Hello from OPPO";
        public const string IncludeSnippet = "#include";
        public const string server = "server";
        public const string definitionXmlElement = "<Definition";
		public const string opcuaappConverterSerializationException = "Use default serialization.";

        public static class FileExtension
        {
            public const string OppoSln = ".opposln";
            public const string OppoProject = ".oppoproj";
            public const string DebianInstaller = ".deb";
            public const string InformationModel = ".xml";
            public const string ModelTypes = ".bsd";
            public const string CFile = ".c";
			public const string ZipFile = ".zip";
        }

        public static class CommandName
        {
            public const string New = "new";
            public const string Hello = "hello";
            public const string Build = "build";
            public const string Publish = "publish";
            public const string Help = "help";
            public const string HelpDash = "-h";
            public const string HelpDashVerbose = "--help";
            public const string ShortHelp = "?";
            public const string Version = "version";
            public const string Clean = "clean";
            public const string Deploy = "deploy";
            public const string Import = "import";
            public const string Generate = "generate";
            public const string GenerateInformationModel = "information-model";
            public const string Sln = "sln";
            public const string Reference = "reference";
        }

        public static class CommandResults
        {
            public const string Success = "success";
            public static string Failure { get; set; } = "failure";
        }

        public static class NewCommandName
        {
            public const string Help = "-h";
            public const string VerboseHelp = "--help";
            public const string Sln = "sln";
            public const string OpcuaApp = "opcuaapp";
        }

        public static class SlnCommandName
        {
            public const string Help = "-h";
            public const string VerboseHelp = "--help";
            public const string Add = "add";
            public const string Remove = "remove";
			public const string Build = "build";
			public const string Publish = "publish";
			public const string Deploy = "deploy";
        }
		
		public static class ReferenceCommandName
		{
			public const string Help = "-h";
			public const string VerboseHelp = "--help";
			public const string Add = "add";
			public const string Remove = "remove";
		}

		public static class DirectoryName
        {
            public const string SourceCode = "src";
            public const string ClientApp = "client";
            public const string ServerApp = "server";
            public const string MesonBuild = "build";
            public const string Publish = "publish";
            public const string Deploy = "deploy";
            public const string Temp = "temp";
            public const string OpcuaappInstaller = "oppo-opcuaapp";
            public const string Usr = "usr";
            public const string Bin = "bin";
            public const string Models = "models";
            public const string InformationModels = "information-models";
        }

        public static class ExecutableName
        {
            public const string Meson = "meson";
            public const string Ninja = "ninja";
            public const string AppClient = "client-app";
            public const string AppServer = "server-app";
            public const string CreateDebianInstaller = "dpkg";
            public static readonly string CreateDebianInstallerArguments = "--build " + DirectoryName.OpcuaappInstaller;
            public const string OppoResourcesDll = "oppo-resources.dll";

            public const string PythonScript = @"python3";
            public static readonly string GenerateDatatypesScriptPath = System.IO.Path.Combine(new string[] { " ", "etc", "oppo", "tools", "open62541", "v0.3.0", "python-scripts", "generate_datatypes.py" });
            // 0 bsd types source path
            public const string GenerateDatatypesTypeBsd = @" --type-bsd={0}";
            public static readonly string NodesetCompilerCompilerPath = System.IO.Path.Combine(new string[] { " ", "etc", "oppo", "tools", "open62541", "v0.3.0", "python-scripts", "nodeset_compiler", "nodeset_compiler.py" });
            public const string NodesetCompilerInternalHeaders = @" --internal-headers";
            // 0 ua types
            public const string NodesetCompilerTypesArray = @" --types-array={0}";
            public const string NodesetCompilerBasicTypes = @"UA_TYPES";
            public const string NodesetCompilerExisting = @" --existing {0}";
            // 0 existing model xml source path
            public static readonly string NodesetCompilerBasicNodeset = System.IO.Path.Combine(new string[] { " ", "etc", "oppo", "tools", "open62541", "v0.3.0", "existing-nodes", "Opc.Ua.NodeSet2.xml" });
            // 0 xml model source path
            // 1 output directory with name for generated files and method
            public const string NodesetCompilerXml = @" --xml {0} {1}";
        }

        public static class FileName
        {
            public const string SourceCode_main_c                   = "main.c";
			public const string SourceCode_globalVariables_h		= "globalVariables.h";
            public const string SourceCode_loadInformationModels_c  = "loadInformationModels.c";
			public const string SourceCode_constants_h				= "constants.h";
			public const string SourceCode_mainCallbacks_c			= "mainCallbacks.c";
            public const string SourceCode_meson_build              = "meson.build";
            public const string SampleInformationModelFile          = "DiNodeset.xml";
			public const string SampleInformationModelTypesFile     = "DiTypes.bsd";
		}

        public static class NewSlnCommandArguments
        {
            public const string Name = "-n";
            public const string VerboseName = "--name";
        }
        
        public static class NewOpcuaAppCommandArguments
        {
            public const string Name = "-n";
            public const string VerboseName = "--name";
			public const string Type = "-t";
			public const string VerboseType = "--type";
			public const string Url = "-u";
			public const string VerboseUrl = "--url";
			public const string Port = "-p";
			public const string VerbosePort = "--port";
        }


        public static class BuildCommandArguments
        {
            public const string Name = "-n";
            public const string VerboseName = "--name";
            public const string VerboseHelp = "--help";
            public const string Help = "-h";
        }

        public static class PublishCommandArguments
        {
            public const string Help = "-h";
            public const string VerboseHelp = "--help";
            public const string Name = "-n";
            public const string VerboseName = "--name";
        }

        public static class CleanCommandArguments
        {
            public const string Help = "-h";
            public const string VerboseHelp = "--help";
            public const string Name = "-n";
            public const string VerboseName = "--name";
        }

        public static class DeployCommandArguments
        {
            public const string Help = "-h";
            public const string VerboseHelp = "--help";
            public const string Name = "-n";
            public const string VerboseName = "--name";
        }

        public static class GenerateInformationModeCommandArguments
        {
            public const string Name                    = "-n";
            public const string VerboseName             = "--name";
            public const string Model                   = "-m";
            public const string VerboseModel            = "--model";
            public const string Types                   = "-t";
            public const string VerboseTypes            = "--types";
            public const string RequiredModel           = "-r";
            public const string VerboseRequiredModel    = "--requiredModel";
            public const string VerboseHelp             = "--help";
            public const string Help                    = "-h";
        }

        public static class GenerateCommandArguments
        {
            public const string InformationModel = "information-model";
            public const string VerboseHelp = "--help";
            public const string Help = "-h";
        }

        public static class ImportInformationModelCommandName
        {
            public const string InformationModel = "information-model";
        }

        public static class GenerateInformationModelCommandName
        {
            public const string InformationModel = "information-model";
        }

        public static class ImportInformationModelCommandArguments
        {
            public const string Name = "-n";
            public const string VerboseName = "--name";
            public const string Path = "-p";
            public const string VerbosePath = "--path";
            public const string Help = "-h";
            public const string VerboseHelp = "--help";
            public const string Sample = "-s";
            public const string VerboseSample = "--sample";

        }

        public static class InformationModelsName
        {
            public const string FileSnippet     = "files('information-models/{0}.c'),";
            public const string Types           = "_types";
            public const string TypesGenerated  = "_types_generated";
        }

        public static class LoadInformationModelsContent
        {
            public const string ReturnLine = "return UA_STATUSCODE_GOOD;";
            public const string FunctionSnippetPart1 = "\tif ({0}(server) != UA_STATUSCODE_GOOD)";
            public const string FunctionSnippetPart2 = "\t{";
            public const string FunctionSnippetPart3 = "\t\tUA_LOG_ERROR(UA_Log_Stdout, UA_LOGCATEGORY_SERVER, \"Could not add the {0} nodeset. Check previous output for any error.\");";
            public const string FunctionSnippetPart4 = "\t\treturn UA_STATUSCODE_BADUNEXPECTEDERROR;";
            public const string FunctionSnippetPart5 = "\t}";
        }

        public static class SlnAddCommandArguments
        {
            public const string Solution = "-s";
            public const string VerboseSolution = "--solution";
            public const string Project = "-p";
            public const string VerboseProject = "--project";
            
        }
        public static class SlnRemoveCommandArguments
        {
            public const string Solution = "-s";
            public const string VerboseSolution = "--solution";
            public const string Project = "-p";
            public const string VerboseProject = "--project";

        }
		
		public static class ReferenceAddCommandArguments
		{
			public const string Server = "-s";
			public const string VerboseServer = "--server";
			public const string Client = "-c";
			public const string VerboseClient = "--client";

		}

		public static class ReferenceRemoveCommandArguments
		{
			public const string Server = "-s";
			public const string VerboseServer = "--server";
			public const string Client = "-c";
			public const string VerboseClient = "--client";

		}

		public static class ApplicationType
		{
			public const string Client = "Client";
			public const string Server = "Server";
			public const string ClientServer = "ClientServer";
		}

		public static class ServerConstants
		{
			public const string ServerAppHostname = "const char* SERVER_APP_HOSTNAME";
			public const string ServerAppPort = "const UA_UInt16 SERVER_APP_PORT";
		}

		public static class ClientGlobalVariables
		{
			public const string FirstLines = "#define numberOfReferences {0}\n\nconst char* SERVER_APP_URL[numberOfReferences] = {{";
			public const string Hostname = " \"opc.tcp://{0}:{1}/\",";
			public const string LastLines = " };\nUA_Client* client[numberOfReferences];";
		}
		
		public static class UAMethodCallback
		{
			public const string UAMethod = "UAMethod";
			public const string BrowseName = "BrowseName";
			public const string NodeId = "NodeId";
			public const string AddCallbacks = "addCallbacks";
			public const string FunctionName = "ns{0}_i{1}_Callback";
			public const string FunctionBody = "// callback function of {0} UAMethod\nUA_MethodCallback ns{1}_i{2}_Callback(UA_Server* server, const UA_NodeId* sessionId, void* sessionContext, const UA_NodeId* methodId, void* methodContext, const UA_NodeId* objectId, void* objectContext, size_t inputSize, const UA_Variant* input, size_t outputSize, UA_Variant* output)\n{{\n\t/* TODO: place your code here */\n\treturn UA_STATUSCODE_GOOD;\n}}\n";
			public const string FunctionCall = "\t// {0} UAMethod\n\tif(UA_Server_setMethodNode_callback(server, UA_NODEID_NUMERIC({1}, {2}), ns{1}_i{2}_Callback) != UA_STATUSCODE_GOOD)\n\t{{\n\t\treturn UA_STATUSCODE_BADUNEXPECTEDERROR;\n\t}}\n";
			public const string ReturnLine = "return UA_STATUSCODE_GOOD;";
		}
    }
}