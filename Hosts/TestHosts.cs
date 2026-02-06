using Godot;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Hosts;

// ========================================
// Game State Interface
// ========================================

public class GameState
{
    public string CurrentLevel { get; set; } = "Level1";
    public bool IsPaused { get; set; }
    public float GameTime { get; set; }
}

public interface IGameStateManager
{
    GameState CurrentState { get; }
    void PauseGame();
    void ResumeGame();
    void ChangeLevel(string levelName);
}

// ========================================
// Audio Manager Host
// ========================================

public interface IAudioManager
{
    float MasterVolume { get; set; }
    void PlaySound(string soundName);
    void PlayMusic(string musicName);
    void StopMusic();
}

// ========================================
// Input Manager Host
// ========================================

public interface IInputManager
{
    Vector2 GetMovementInput();
    bool IsActionPressed(string action);
}
