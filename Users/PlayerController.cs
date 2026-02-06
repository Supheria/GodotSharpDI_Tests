using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Users;

[User]
public sealed partial class PlayerController : Node2D, IServicesReady
{
    [Inject]
    private IPlayerStats _playerStats = null!;

    [Inject]
    private IInputManager _inputManager = null!;

    [Inject]
    private IAudioManager _audioManager = null!;

    [Inject]
    private GameManager _gameManager = null!;

    public bool IsServicesReady { get; private set; }
    public Vector2 Velocity { get; private set; }

    void IServicesReady.OnServicesReady()
    {
        IsServicesReady = true;
        GD.Print("[PlayerController] Services ready!");
    }

    public override void _Process(double delta)
    {
        if (!IsServicesReady || _gameManager.CurrentState.IsPaused)
            return;

        // Get input and move
        var input = _inputManager.GetMovementInput();
        Velocity = input * 200.0f;

        if (_inputManager.IsActionPressed("ui_accept"))
        {
            _audioManager.PlaySound("jump");
        }
    }

    public void TakeDamage(int amount)
    {
        _playerStats.TakeDamage(amount);
        _audioManager.PlaySound("hurt");
        GD.Print($"[PlayerController] Took {amount} damage! Health: {_playerStats.Health}");
    }

    // Helper methods for testing
    public IPlayerStats GetPlayerStats() => _playerStats;

    public IInputManager GetInputManager() => _inputManager;

    public IAudioManager GetAudioManager() => _audioManager;

    // Required for DI framework
    public override partial void _Notification(int what);
}