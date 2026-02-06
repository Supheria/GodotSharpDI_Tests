using Godot;
using GodotSharpDI.Abstractions;

namespace GodotSharpDI.Tests.Hosts;

[Host]
public sealed partial class InputManager : Node, IInputManager
{
    [Singleton(typeof(IInputManager))]
    private InputManager Self => this;

    public Vector2 GetMovementInput()
    {
        var input = Vector2.Zero;
        input.X = Input.GetAxis("ui_left", "ui_right");
        input.Y = Input.GetAxis("ui_up", "ui_down");
        return input;
    }

    public bool IsActionPressed(string action)
    {
        return Input.IsActionPressed(action);
    }

    // Required for DI framework
    public override partial void _Notification(int what);
}