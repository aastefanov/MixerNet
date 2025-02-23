using MixerNet.Cli;
using MixerNet.Controller;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.CommandLine;
using System.Net;

//var client = new OscUdpClient(IPEndPoint.Parse("192.168.52.3:10024"));

var Console = AnsiConsole.Console;

var mixer = new OscController(IPEndPoint.Parse("192.168.52.3:10024"));

await mixer.Setup();



var validators = Validators.Create(mixer);

var getGainCommand = new Command("get-gain")
    .AddArguments(validators.Input, validators.Output);
getGainCommand.SetHandler(async (i, o) => Console.Markup($"{await mixer.GetGainAsync(i, o):0.00}"), validators.Input, validators.Output);

var getMutedCommand = new Command("get-muted")
    .AddArguments(validators.Input, validators.Output);
getMutedCommand.SetHandler(async (i, o) => Console.Markup($"{await mixer.IsMutedAsync(i, o)}"), validators.Input, validators.Output);

var getInputMultiplierCommand = new Command("img")
    .AddArguments(validators.Input);
getInputMultiplierCommand.SetHandler(async (i) => Console.Markup($"{await mixer.GetInputMultiplier(i):0.00}"), validators.Input);

var getOutputMultiplierCommand = new Command("omg")
    .AddArguments(validators.Output);
getOutputMultiplierCommand.SetHandler(async (o) => Console.Markup($"{await mixer.GetOutputMultiplier(o):0.00}"), validators.Output);


var matrixCommand = new Command("matrix");
matrixCommand.SetHandler(async () =>
{
    var table = new Table();

    table.AddColumn("");
    foreach (var b in mixer.Buses) table.AddColumn(b);


    foreach (var (ch, i) in mixer.Channels.Enumerate())
    {
        IRenderable[] columns = new IRenderable[mixer.Buses.Count + 1];

        columns[0] = new Markup(ch);

        foreach (var (b, j) in mixer.Buses.Enumerate())
        {
            bool muted = await mixer.IsMutedAsync(i, j);
            float gain = await mixer.GetGainAsync(i, j);

            columns[j + 1] = new Markup($"[{(muted ? "red" : "green")}]{gain:0.00}[/]");
        }
        table.AddRow(columns);
    }
    Console.Write(table);
});

var root = new RootCommand();

root.AddCommands(
    matrixCommand,
    getGainCommand,
    getMutedCommand,
    getInputMultiplierCommand,
    getOutputMultiplierCommand
    );

await root.InvokeAsync(args);