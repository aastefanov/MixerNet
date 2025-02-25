using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MixerNet.Avalonia.Models;
using MixerNet.Avalonia.ViewModels;
using MixerNet.Avalonia.Views;
using MixerNet.Controller;

namespace MixerNet.Avalonia;

public class App : Application
{
    private OscController controller;
    private Mixer mixer;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        controller = new OscController(new SerialPort("COM3"));
        // controller = new OscController(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10024));
        controller.Setup().Wait();

        mixer = new Mixer
        {
            Inputs = controller.Channels.SelectEnum((x, i) => new AudioThing { Id = i, Name = x }).ToList(),
            Outputs = controller.Buses.SelectEnum((x, i) => new AudioThing { Id = i, Name = x }).ToList(),
        };

        // mixer.Inputs = new List<AudioThing>()

        Task.Run(async void () =>
        {
            while (true)
            {
                for (int i = 0; i < controller.Channels.Count; i++)
                    mixer.Inputs[i].Gain = (await controller.GetInputLevels(i)).Item1;

                for (int j = 0; j < controller.Buses.Count; j++)
                    mixer.Outputs[j].Gain = (await controller.GetOutputLevels(j)).Item1;

                await Task.Delay(30);
            }
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel { Mixer = mixer }
            };
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel { Mixer = mixer }
            };

        base.OnFrameworkInitializationCompleted();
    }
}