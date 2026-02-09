using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Users;

[User]
public sealed partial class PlayerUI : Control, IDependenciesResolved
{
    // Injected services
    [Inject]
    private IPlayerStats _playerStats = null!;

    [Inject]
    private IInventoryService _inventory = null!;

    [Inject]
    private GameManager _gameManager = null!;

    // State tracking for testing
    public bool IsDependenciesReady { get; private set; }
    public int UpdateCount { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        GD.Print("[PlayerUI] _Ready called (before dependencies resolved)");
    }

    void IDependenciesResolved.OnDependenciesResolved(bool isAllDependenciesReady)
    {
        IsDependenciesReady = isAllDependenciesReady;
        if (isAllDependenciesReady)
        {
            GD.Print("[PlayerUI] Dependencies ready! Initializing UI...");
            UpdateUI();
        }
        else
        {
            GD.Print("[PlayerUI] Dependencies failed! Cannot initializing UI...");
        }
    }

    public void UpdateUI()
    {
        UpdateCount++;
        GD.Print($"[PlayerUI] Update #{UpdateCount}");
        GD.Print($"  Health: {_playerStats.Health}/{_playerStats.MaxHealth}");
        GD.Print($"  Mana: {_playerStats.Mana}/{_playerStats.MaxMana}");
        GD.Print($"  Items: {_inventory.ItemCount}");
        GD.Print($"  Level: {_gameManager.CurrentState.CurrentLevel}");
    }

    // Helper methods for testing
    public IPlayerStats GetPlayerStats() => _playerStats;

    public IInventoryService GetInventory() => _inventory;

    public GameManager GetGameManager() => _gameManager;

    // Required for DI framework
    public override partial void _Notification(int what);
}
