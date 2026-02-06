using System.Reflection;
using System.Threading.Tasks;
using Godot;
using NUnit.Framework;

namespace GodotSharpDI.Tests.Integration;

/// <summary>
/// Base class for integration tests that need to run within Godot scene tree
/// Provides utilities for creating and managing test scenes
/// </summary>
public abstract class GodotIntegrationTestBase
{
    protected SceneTree? SceneTree { get; private set; }
    protected Node? TestRoot { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        // Try to get the scene tree
        if (Engine.GetMainLoop() is SceneTree tree)
        {
            SceneTree = tree;
            TestRoot = new Node { Name = "TestRoot" };
            tree.Root.AddChild(TestRoot);
        }
        else
        {
            Assert.Warn("Running outside Godot scene tree - some tests may be skipped");
        }
    }

    [TearDown]
    public virtual void TearDown()
    {
        if (TestRoot != null && GodotObject.IsInstanceValid(TestRoot))
        {
            TestRoot.QueueFree();
            TestRoot = null;
        }
    }

    /// <summary>
    /// Adds a node to the test scene tree
    /// </summary>
    protected T AddNode<T>(T node)
        where T : Node
    {
        if (TestRoot == null)
        {
            Assert.Fail("TestRoot is not available - running outside Godot?");
            return node;
        }

        TestRoot.AddChild(node);
        return node;
    }

    /// <summary>
    /// Waits for the next frame
    /// </summary>
    protected async Task WaitForFrame()
    {
        if (SceneTree != null)
        {
            await SceneTree.ToSignal(SceneTree, SceneTree.SignalName.ProcessFrame);
        }
    }

    /// <summary>
    /// Waits for multiple frames
    /// </summary>
    protected async Task WaitForFrames(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await WaitForFrame();
        }
    }

    /// <summary>
    /// Checks if services are injected into a User node using reflection
    /// </summary>
    protected bool AreServicesInjected(object userNode)
    {
        var type = userNode.GetType();
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<GodotSharpDI.Abstractions.InjectAttribute>() != null)
            {
                var value = field.GetValue(userNode);
                if (value == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the value of an injected field using reflection
    /// </summary>
    protected T? GetInjectedField<T>(object userNode, string fieldName)
    {
        var type = userNode.GetType();
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null)
        {
            return default;
        }

        var value = field.GetValue(userNode);
        return value is T typedValue ? typedValue : default;
    }
}

/// <summary>
/// Helper class for creating test scenes programmatically
/// </summary>
public static class TestSceneBuilder
{
    /// <summary>
    /// Creates a simple test scene with a scope and user
    /// </summary>
    public static (TScope scope, TUser user) CreateScopeWithUser<TScope, TUser>()
        where TScope : Node, new()
        where TUser : Node, new()
    {
        var scope = new TScope { Name = typeof(TScope).Name };
        var user = new TUser { Name = typeof(TUser).Name };

        scope.AddChild(user);

        return (scope, user);
    }

    /// <summary>
    /// Creates a test scene with scope, host, and user
    /// </summary>
    public static (TScope scope, THost host, TUser user) CreateScopeWithHostAndUser<
        TScope,
        THost,
        TUser
    >()
        where TScope : Node, new()
        where THost : Node, new()
        where TUser : Node, new()
    {
        var scope = new TScope { Name = typeof(TScope).Name };
        var host = new THost { Name = typeof(THost).Name };
        var user = new TUser { Name = typeof(TUser).Name };

        scope.AddChild(host);
        scope.AddChild(user);

        return (scope, host, user);
    }

    /// <summary>
    /// Creates a nested scope hierarchy
    /// </summary>
    public static (TParentScope parent, TChildScope child) CreateNestedScopes<
        TParentScope,
        TChildScope
    >()
        where TParentScope : Node, new()
        where TChildScope : Node, new()
    {
        var parent = new TParentScope { Name = typeof(TParentScope).Name };
        var child = new TChildScope { Name = typeof(TChildScope).Name };

        parent.AddChild(child);

        return (parent, child);
    }
}
