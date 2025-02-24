using System.CommandLine;
using Spectre.Console;

namespace MixerNet.Cli;

partial class Cli
{
    public Command DrawVuCommand
    {
        get
        {
            var command = new Command("draw-vu");
            command.SetHandler(async () =>
            {
                var tableIn = new Table();
                tableIn.Border(TableBorder.None);
                tableIn.AddColumn("");

                var tableOut = new Table();
                tableOut.Border(TableBorder.None);
                tableOut.AddColumn("");

                var grid = new Grid().AddColumn().AddColumn();
                grid.AddRow(tableIn, tableOut);

                Color ChooseColor(float val)
                {
                    return val switch
                    {
                        < -35 => Color.Yellow,
                        < -13 => Color.Green,
                        _ => Color.Red
                    };
                }

                async Task<BarChart> GenerateChartIn(Func<float, Color> generateColor)
                {
                    var barChartIn = new BarChart().Width(60).WithMaxValue(60f).Label("Input Levels").CenterLabel();
                    barChartIn.ValueFormatter = (x, ci) => $"{x - 60.0f:0.00} dB";
                    var inputLevels = await Enumerable.Range(0, mixer.Channels.Count)
                        .SelectAsync(async x => await mixer.GetInputLevels(x)).ToListAsync();

                    foreach (var (ch, i) in mixer.Channels.Enumerate())
                        barChartIn.AddItem(ch, inputLevels[i].Item1 + 60f, generateColor(inputLevels[i].Item1));

                    return barChartIn;
                }

                async Task<BarChart> GenerateChartOut(Func<float, Color> generateColor)
                {
                    var barChartOut = new BarChart().Width(60).WithMaxValue(60f).Label("Output Levels").CenterLabel();
                    barChartOut.ValueFormatter = (x, ci) => $"{x - 60.0f:0.00} dB";
                    var outputLevels = await Enumerable.Range(0, mixer.Buses.Count)
                        .SelectAsync(async x => await mixer.GetOutputLevels(x)).ToListAsync();

                    foreach (var (b, j) in mixer.Buses.Enumerate())
                        barChartOut.AddItem(b, outputLevels[j].Item1 + 60f, generateColor(outputLevels[j].Item1));

                    return barChartOut;
                }

                await AnsiConsole.Live(grid).StartAsync(async ctx =>
                {
                    while (true)
                    {
                        tableIn.AddRow(await GenerateChartIn(ChooseColor));
                        tableOut.AddRow(await GenerateChartOut(ChooseColor));
                        ctx.Refresh();
                        tableIn.RemoveRow(0);
                        tableOut.RemoveRow(0);
                        await Task.Delay(20);
                    }
                });
            });
            return command;
        }
    }
}