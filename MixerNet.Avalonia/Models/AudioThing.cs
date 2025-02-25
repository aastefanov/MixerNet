using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MixerNet.Avalonia.Models;

public class AudioThing : INotifyPropertyChanged
{
    private float gain;
    private float rms;
    public int Id { get; set; }
    public required string Name { get; set; }

    public float Gain
    {
        get => gain;
        set => SetField(ref gain, value);
    }

    public float Rms
    {
        get => rms;
        set => SetField(ref rms, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}