using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Ports;
using System.Net;
using MixerNet.Controller;
using Spectre.Console;

//var client = new OscUdpClient(IPEndPoint.Parse("192.168.52.3:10024"));

namespace MixerNet.Cli;

partial class Cli
{
    private Parser app;
    private OscController mixer;
    private readonly RootCommand root;
    private Validators validators;

    private Cli()
    {
        //var getMutedCommand = new Command("get-muted")
        //    .AddArguments(validators.Input, validators.Output);
        //getMutedCommand.SetHandler(async (i, o) => AnsiConsole.Markup($"{await mixer.IsMutedAsync(i, o)}"), validators.Input, validators.Output);


        root = new RootCommand();

        //root.AddOption(serialOption);

        //var builder = new CommandLineBuilder(root)
        //    .AddMiddleware(async (ctx, next) =>
        //    {
        //        string? serial = ctx.ParseResult.GetValueForOption(serialOption);
        //        if (!string.IsNullOrEmpty(serial) && mixer is null)
        //        {
        //            mixer = new OscController(new SerialPort(serial));
        //            validators = Validators.Create(mixer);

        //            await mixer.Setup();

        //        }

        //        await next(ctx);
        //    })
        //    .UseDefaults();

        //app = builder.Build();
    }

    public async Task<int> StartAsync(string[] args)
    {
        var serialOption = new Option<string>("serial");
        var udpOption = new Option<string>("host", () => "127.0.0.1:10024");
        var serialPort = serialOption.Parse(args).GetValueForOption(serialOption);
        var udpHost = udpOption.Parse(args).GetValueForOption(udpOption);
        if (!string.IsNullOrEmpty(serialPort)) mixer = new OscController(new SerialPort(serialPort));
        else mixer = new OscController(IPEndPoint.Parse(udpHost ?? "127.0.0.1:10024"));

        await mixer.Setup();

        AnsiConsole.Write($"Mixer connected to {mixer.Remote}\n");
        
        validators = Validators.Create(mixer);


        root.AddCommands(
            GetGainCommand, SetGainCommand,
            MatrixCommand,
            GetInputMultipliersCommand, SetInputMultiplierCommand,
            GetOutputMultipliersCommand, SetOutputMultiplierCommand,
            DrawVuCommand
        );


        return await root.InvokeAsync(args);
    }

    private static async Task<int> Main(string[] args)
    {
        var cli = new Cli();

        return await cli.StartAsync(args);
    }
}