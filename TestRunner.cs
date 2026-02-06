using System;
using System.Linq;
using System.Reflection;
using Godot;

namespace GodotSharpDI.Tests;

/// <summary>
/// Automated test runner that executes all integration tests in Godot
/// Attach this to a Node in your test scene and run it
/// </summary>
public partial class TestRunner : Node
{
    [Export]
    public bool AutoRunOnReady = true;

    [Export]
    public bool ExitAfterTests = false;

    private int _totalTests = 0;
    private int _passedTests = 0;
    private int _failedTests = 0;
    private int _skippedTests = 0;

    public override void _Ready()
    {
        if (AutoRunOnReady)
        {
            CallDeferred(nameof(RunAllTests));
        }
    }

    public async void RunAllTests()
    {
        GD.Print("=".PadRight(80, '='));
        GD.Print("Starting GodotSharpDI Integration Test Suite");
        GD.Print("=".PadRight(80, '='));
        GD.Print("");

        // Wait a frame for scene tree to stabilize
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Find all test classes
        var assembly = Assembly.GetExecutingAssembly();
        var testClasses = assembly
            .GetTypes()
            .Where(t => t.GetCustomAttribute<NUnit.Framework.TestFixtureAttribute>() != null)
            .Where(t => t.Namespace?.Contains("Integration") == true)
            .ToList();

        GD.Print($"Found {testClasses.Count} test fixtures");
        GD.Print("");

        foreach (var testClass in testClasses)
        {
            await RunTestClass(testClass);
        }

        // Print summary
        GD.Print("");
        GD.Print("=".PadRight(80, '='));
        GD.Print("Test Summary");
        GD.Print("=".PadRight(80, '='));
        GD.Print($"Total Tests:   {_totalTests}");
        GD.Print($"Passed:        {_passedTests} ({GetPercentage(_passedTests, _totalTests):F1}%)");
        GD.Print($"Failed:        {_failedTests} ({GetPercentage(_failedTests, _totalTests):F1}%)");
        GD.Print(
            $"Skipped:       {_skippedTests} ({GetPercentage(_skippedTests, _totalTests):F1}%)"
        );
        GD.Print("=".PadRight(80, '='));

        if (ExitAfterTests)
        {
            GetTree().Quit(_failedTests > 0 ? 1 : 0);
        }
    }

    private async System.Threading.Tasks.Task RunTestClass(Type testClass)
    {
        GD.Print($"[{testClass.Name}]");

        var testMethods = testClass
            .GetMethods()
            .Where(m => m.GetCustomAttribute<NUnit.Framework.TestAttribute>() != null)
            .ToList();

        foreach (var method in testMethods)
        {
            _totalTests++;
            await RunTestMethod(testClass, method);
        }

        GD.Print("");
    }

    private async System.Threading.Tasks.Task RunTestMethod(Type testClass, MethodInfo method)
    {
        var testName = method.Name.Replace("_", " ");

        try
        {
            // Create test instance
            var instance = Activator.CreateInstance(testClass);

            // Run SetUp if exists
            var setupMethod = testClass.GetMethod("SetUp");
            setupMethod?.Invoke(instance, null);

            // Wait a frame
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Run test
            var result = method.Invoke(instance, null);

            // Handle async tests
            if (result is System.Threading.Tasks.Task task)
            {
                await task;
            }

            // Run TearDown if exists
            var tearDownMethod = testClass.GetMethod("TearDown");
            tearDownMethod?.Invoke(instance, null);

            _passedTests++;
            GD.Print($"  ✓ {testName}");
        }
        catch (System.Exception ex)
        {
            // Check if it's an ignore exception
            if (ex.InnerException?.GetType().Name == "IgnoreException")
            {
                _skippedTests++;
                GD.Print($"  ○ {testName} (Skipped: {ex.InnerException.Message})");
            }
            else
            {
                _failedTests++;
                GD.PushError($"  ✗ {testName}");
                GD.PushError($"    {ex.InnerException?.Message ?? ex.Message}");

                if (ex.InnerException?.StackTrace != null)
                {
                    GD.PushError($"    Stack: {ex.InnerException.StackTrace}");
                }
            }
        }
    }

    private static float GetPercentage(int value, int total)
    {
        return total > 0 ? (value * 100.0f / total) : 0.0f;
    }

    public override void _Input(InputEvent @event)
    {
        // Press R to re-run tests
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.R)
        {
            _totalTests = 0;
            _passedTests = 0;
            _failedTests = 0;
            _skippedTests = 0;
            CallDeferred(nameof(RunAllTests));
        }

        // Press Q to quit
        if (@event is InputEventKey quitKey && quitKey.Pressed && quitKey.Keycode == Key.Q)
        {
            GetTree().Quit();
        }
    }
}
