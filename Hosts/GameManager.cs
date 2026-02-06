using Godot;
using GodotSharpDI.Abstractions;

namespace GodotSharpDI.Tests.Hosts;

[Host]
public sealed partial class GameManager : Node, IGameStateManager
{
    // Expose itself as a service
    [Singleton(typeof(GameManager))]
    private GameManager Self
    {
        get
        {
            GD.Print("[GameManager] Providing self as service");
            return this;
        }
    }

    [Singleton(typeof(IGameStateManager))]
    private GameManager StateManager => this;

    public GameState CurrentState { get; private set; } = new();

    public void PauseGame()
    {
        CurrentState.IsPaused = true;
        GD.Print("[GameManager] Game paused");
    }

    public void ResumeGame()
    {
        CurrentState.IsPaused = false;
        GD.Print("[GameManager] Game resumed");
    }

    public void ChangeLevel(string levelName)
    {
        CurrentState.CurrentLevel = levelName;
        GD.Print($"[GameManager] Changed to level: {levelName}");
    }

    public override void _Process(double delta)
    {
        if (!CurrentState.IsPaused)
        {
            CurrentState.GameTime += (float)delta;
        }
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}