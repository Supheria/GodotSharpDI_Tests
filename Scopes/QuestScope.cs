using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Scopes;

[Modules(Hosts = [typeof(QuestScopeServiceHost)])]
public partial class QuestScope : Node, IScope
{
    public override void _Ready()
    {
        base._Ready();
        GD.Print("[QuestScope] Quest scope initialized");
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}
