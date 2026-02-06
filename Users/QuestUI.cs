using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Users;

[User]
public sealed partial class QuestUI : Control, IServicesReady
{
    [Inject]
    private IQuestService _questService = null!;

    [Inject]
    private IScoreService _scoreService = null!;

    public bool IsServicesReady { get; private set; }

    void IServicesReady.OnServicesReady()
    {
        IsServicesReady = true;
        GD.Print("[QuestUI] Services ready!");
        DisplayQuests();
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
