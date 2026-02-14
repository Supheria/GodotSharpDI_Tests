using System;
using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Hosts;

[Host]
public sealed partial class QuestScopeServiceHost : Node
{
    [Inject]
    private IPlayerStats _playerStats;

    [Provide(ExposedTypes = [typeof(IQuestService)], WaitFor = [nameof(_playerStats)])]
    public QuestService QuestService
    {
        get
        {
            if (IsPlayerStatsInjectionReady)
            {
                return new QuestService(_playerStats);
            }

            throw new Exception();
        }
    }

    [Provide(ExposedTypes = [typeof(IEnemyFactory)], WaitFor = [nameof(_playerStats)])]
    public EnemyFactory EnemyFactory
    {
        get
        {
            if (IsPlayerStatsInjectionReady)
            {
                return new EnemyFactory(_playerStats);
            }

            throw new Exception();
        }
    }

    public override partial void _Notification(int what);
}
