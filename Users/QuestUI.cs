using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Users;

[User]
public sealed partial class QuestUI : Control, IDependenciesResolved
{
    [Inject]
    private IQuestService _questService = null!;

    [Inject]
    private IScoreService _scoreService = null!;

    [Inject]
    private GameManager _gameManager = null!;

    public bool IsDependenciesReady { get; private set; }

    void IDependenciesResolved.OnDependenciesResolved(bool isAllDependenciesReady)
    {
        IsDependenciesReady = isAllDependenciesReady;
        if (isAllDependenciesReady)
        {
            GD.Print("[QuestUI] Dependencies ready!");
            DisplayQuests();
        }
        else
        {
            GD.Print("[QuestUI] Dependencies failed!");
        }
    }

    public void DisplayQuests()
    {
        GD.Print($"[QuestUI] Active Quests: {_questService.ActiveQuestCount}");
        GD.Print($"[QuestUI] Current Score: {_scoreService.CurrentScore}");
    }

    public IQuestService GetQuestService() => _questService;

    public IScoreService GetScoreService() => _scoreService;

    // Required for DI framework
    public override partial void _Notification(int what);
}
