using System.Collections.Generic;

namespace MixerNet.Avalonia.Models;

public class Mixer
{
    public required List<AudioThing> Inputs { get; set; }
    public required List<AudioThing> Outputs { get; set; }
}