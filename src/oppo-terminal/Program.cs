﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Oppo.ObjectModel;
using Oppo.ObjectModel.CommandStrategies.BuildCommands;
using Oppo.ObjectModel.CommandStrategies.HelloCommands;
using Oppo.ObjectModel.CommandStrategies.HelpCommands;
using Oppo.ObjectModel.CommandStrategies.NewCommands;
using Oppo.ObjectModel.CommandStrategies.PublishCommands;
using Oppo.ObjectModel.CommandStrategies.VersionCommands;

namespace Oppo.Terminal
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        internal static int Main(string[] args)
        {
            var commandFactory = CreateCommandFactory();
            var objectModel = new ObjectModel.ObjectModel(commandFactory);
            var result = objectModel.ExecuteCommand(args);
            return result == Constants.CommandResults.Success ? 0 : 1;
        }

        private static ICommandFactory<ObjectModel.ObjectModel> CreateCommandFactory()
        {
            var reflection = new ReflectionWrapper();
            var writer = new ConsoleWriter();
            var fileSystem = new FileSystemWrapper();

            var commands = new List<ICommand<ObjectModel.ObjectModel>>();

            var buildStrategies = new ICommand<BuildStrategy>[] { new BuildNameStrategy(fileSystem), new BuildVerboseNameStrategy(fileSystem), new BuildHelpStrategy(writer), new BuildVerboseHelpStrategy(writer), };
            var buildStrategyCommandFactory = new CommandFactory<BuildStrategy>(buildStrategies, Constants.BuildCommandArguments.Help);
            commands.Add(new BuildStrategy(buildStrategyCommandFactory));

            commands.Add(new HelloStrategy(writer));

            var helpStrategy = new HelpStrategy(writer);
            commands.Add(helpStrategy);

            var newStrategies = new ICommand<NewStrategy>[] { new NewSlnCommandStrategy(fileSystem), new NewOpcuaAppCommandStrategy(fileSystem), new NewHelpCommandStrategy(writer), new NewVerboseHelpCommandStrategy(writer), };
            var newStrategyCommandFactory = new CommandFactory<NewStrategy>(newStrategies, Constants.NewCommandName.Help);
            commands.Add(new NewStrategy(newStrategyCommandFactory));

            var publishStrategies = new ICommand<PublishStrategy>[] { new PublishNameStrategy(fileSystem), new PublishVerboseNameStrategy(fileSystem), new PublishHelpStrategy(writer), new PublishVerboseHelpStrategy(writer), };
            var publishStrategyCommandFactory = new CommandFactory<PublishStrategy>(publishStrategies, Constants.PublishCommandArguments.Help);
            commands.Add(new PublishStrategy(publishStrategyCommandFactory));

            commands.Add(new VersionStrategy(reflection, writer));

            var shortHelpStrategy = new ShortHelpStrategy(writer);
            commands.Add(shortHelpStrategy);

            var factory = new CommandFactory<ObjectModel.ObjectModel>(commands, Constants.CommandName.Help);

            helpStrategy.CommandFactory = factory;
            shortHelpStrategy.CommandFactory = factory;

            return factory;
        }
    }
}