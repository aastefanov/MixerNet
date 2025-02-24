using System.CommandLine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MixerNet.Cli;

public partial class Cli
{
    private Command GetGainCommand
    {
        get
        {
            var command = new Command("get-gain").AddArguments(validators.Input, validators.Output);
            command.SetHandler(async (i, o) => AnsiConsole.Markup($"{await mixer.GetGainAsync(i, o):0.00}"),
                validators.Input, validators.Output);
            return command;
        }
    }

    private Command SetGainCommand
    {
        get
        {
            var command =
                new Command("set-gain").AddArguments(validators.Input, validators.Output, validators.Gain);
            command.SetHandler(async (i, o, g) => await mixer.SetGainAsync(i, o, g), validators.Input,
                validators.Output, validators.Gain);
            return command;
        }
    }

    private Command MatrixCommand
    {
        get
        {
            var command = new Command("matrix");
            command.SetHandler(async () =>
            {
                var table = new Table();

                table.AddColumn("");
                foreach (var b in mixer.Buses) table.AddColumn(b);


                foreach (var (ch, i) in mixer.Channels.Enumerate())
                {
                    var columns = new IRenderable[mixer.Buses.Count + 1];

                    columns[0] = new Markup(ch);

                    foreach (var (b, j) in mixer.Buses.Enumerate())
                    {
                        var muted = await mixer.IsMutedAsync(i, j);
                        var gain = await mixer.GetGainAsync(i, j);

                        columns[j + 1] = new Markup($"[{(muted ? "red" : "green")}]{gain:0.00}[/]");
                    }

                    table.AddRow(columns);
                }

                AnsiConsole.Write(table);
            });

            return command;
        }
    }
}