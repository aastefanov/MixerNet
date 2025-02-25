using System.Collections.Generic;
using Avalonia.Controls;
using MixerNet.Avalonia.Models;
using MixerNet.Avalonia.ViewModels;

namespace MixerNet.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            // This can be before or after InitializeComponent.
            Design.SetDataContext(this, new MainViewModel
            {
                Mixer = new Mixer
                {
                    Inputs =
                    [
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "In1", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "In2", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "In3", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "In4", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "In5", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "In6", Rms = -15.0f }
                    ],
                    Outputs =
                    [
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "Out1", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "Out2", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "Out3", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "Out4", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "Out5", Rms = -15.0f },
                        new AudioThing { Gain = 0.5f, Id = 0, Name = "Out6", Rms = -15.0f },
                    ],
                }
            });
        }
    }
}