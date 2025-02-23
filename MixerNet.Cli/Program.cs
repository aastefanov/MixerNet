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

var getVuAnimationCommand = new Command("draw-vu");
getVuAnimationCommand.SetHandler(async () =>
{

    var tableIn = new Table();
    tableIn.Border(TableBorder.None);
    tableIn.AddColumn("");

    var tableOut = new Table();
    tableOut.Border(TableBorder.None);
    tableOut.AddColumn("");

    var grid = new Grid().AddColumn().AddColumn();
    grid.AddRow(tableIn, tableOut);

    var generateColor = (float val) => val switch
        {
            < -35 => Color.Yellow,
            < -13 => Color.Green,
            _ => Color.Red
        };

    var generateChartIn = async () =>
    {
        var barChartIn = new BarChart().Width(60).WithMaxValue(60f).Label("Input Levels").CenterLabel();
        barChartIn.ValueFormatter = (x, ci) => $"{x - 60.0f:0.00} dB";
        var inputLevels = await Enumerable.Range(0, mixer.Channels.Count).SelectAsync(async x => await mixer.GetInputLevels(x)).ToListAsync();

        foreach (var (ch, i) in mixer.Channels.Enumerate())
            barChartIn.AddItem(ch, inputLevels[i].Item1 + 60f, generateColor(inputLevels[i].Item1));

        return barChartIn;
    };
    var generateChartOut = async () =>
    {
        var barChartOut = new BarChart().Width(60).WithMaxValue(60f).Label("Output Levels").CenterLabel();
        barChartOut.ValueFormatter = (x, ci) => $"{x - 60.0f:0.00} dB";
        var outputLevels = await Enumerable.Range(0, mixer.Buses.Count).SelectAsync(async x => await mixer.GetOutputLevels(x)).ToListAsync();

        foreach (var (b, j) in mixer.Buses.Enumerate())
            barChartOut.AddItem(b, outputLevels[j].Item1 + 60f, generateColor(outputLevels[j].Item1));

        return barChartOut;

        //await Console.Live(barChart).StartAsync(async context => {

        //context.Refresh();
        //await Task.Delay(500);
        //});

    };

    await Console.Live(grid).StartAsync(async ctx =>
    {
        while (true)
        {
            tableIn.AddRow(await generateChartIn());
            tableOut.AddRow(await generateChartOut());
            ctx.Refresh();
            tableIn.RemoveRow(0);
            tableOut.RemoveRow(0);
            await Task.Delay(20);
        }
    });

});

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
    getOutputMultiplierCommand,
    getVuAnimationCommand
    );

await root.InvokeAsync(args);