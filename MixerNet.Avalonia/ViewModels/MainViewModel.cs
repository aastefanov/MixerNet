using System.Collections.Generic;
using MixerNet.Avalonia.Models;

namespace MixerNet.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
    {
        public required Mixer Mixer { get; set; }
    }