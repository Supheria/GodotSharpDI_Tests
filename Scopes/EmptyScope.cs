using Godot;
using GodotSharpDI.Abstractions;

namespace GodotSharpDI.Tests.Scopes;

[Modules(Services = [], Hosts = [])]
public partial class EmptyScope : Node, IScope
{
    public override void _Ready()
    {
        base._Ready();
        GD.Print("[EmptyScope] Empty scope initialized");
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}
