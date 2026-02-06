using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Scopes;

[Modules(
    Services = [typeof(PlayerStatsService)],
    Hosts = []
)]
public partial class MinimalScope : Node, IScope
{
    public override void _Ready()
    {
        base._Ready();
        GD.Print("[MinimalScope] Minimal scope initialized");
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}