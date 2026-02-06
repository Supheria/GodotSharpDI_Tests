using Godot;
using GodotSharpDI.Abstractions;

namespace GodotSharpDI.Tests.Hosts;

[Host]
public sealed partial class AudioManager : Node, IAudioManager
{
    [Singleton(typeof(IAudioManager))]
    private AudioManager Self => this;

    public float MasterVolume { get; set; } = 1.0f;

    private string _currentMusic = string.Empty;

    public void PlaySound(string soundName)
    {
        GD.Print($"[AudioManager] Playing sound: {soundName} at volume {MasterVolume}");
    }

    public void PlayMusic(string musicName)
    {
        _currentMusic = musicName;
        GD.Print($"[AudioManager] Playing music: {musicName}");
    }

    public void StopMusic()
    {
        _currentMusic = string.Empty;
        GD.Print("[AudioManager] Music stopped");
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}