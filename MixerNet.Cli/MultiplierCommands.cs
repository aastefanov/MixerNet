using System.CommandLine;
using Spectre.Console;

namespace MixerNet.Cli;

partial class Cli
{
    private Command GetInputMultipliersCommand
    {
        get
        {
            var command = new Command("img").AddArguments(validators.Input);
            command.SetHandler(async i => AnsiConsole.Markup($"{await mixer.GetInputMultiplier(i):0.00}"),
                validators.Input);
            return command;
        }
    }

    private Command GetOutputMultipliersCommand
    {
        get
        {
            var command = new Command("omg").AddArguments(validators.Output);
            command.SetHandler(async o => AnsiConsole.Markup($"{await mixer.GetOutputMultiplier(o):0.00}"),
                validators.Output);
            return command;
        }
    }

    private Command SetInputMultiplierCommand
    {
        get
        {
            var command = new Command("ims").AddArguments(validators.Input, validators.Gain);
            command.SetHandler(async (i, mu) => await mixer.SetInputMultiplier(i, mu), validators.Input,
                validators.Gain);
            return command;
        }
    }

    private Command SetOutputMultiplierCommand
    {
        get
        {
            var command = new Command("oms").AddArguments(validators.Output, validators.Gain);
            command.SetHandler(async (o, mu) => await mixer.SetOutputMultiplier(o, mu), validators.Output,
                validators.Gain);
            return command;
        }
    }
}