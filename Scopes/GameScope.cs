using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Scopes;

[Modules(
    Services = [typeof(PlayerStatsService), typeof(InventoryService), typeof(ScoreService)],
    Hosts = [typeof(GameManager), typeof(AudioManager), typeof(InputManager)]
)]
public partial class GameScope : Node, IScope
{
    public override void _Ready()
    {
        base._Ready();
        GD.Print("[GameScope] Scope initialized");
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}
