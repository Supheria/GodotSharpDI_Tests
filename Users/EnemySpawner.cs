using System.Collections.Generic;
using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Users;

[User]
public sealed partial class EnemySpawner : Node2D, IDependenciesResolved
{
    [Inject]
    private IEnemyFactory _enemyFactory = null!;

    [Inject]
    private IPlayerStats _playerStats = null!;

    [Inject]
    private ISecond _playerStats2 = null!;

    public bool IsDependenciesReady { get; private set; }
    public List<Enemy> SpawnedEnemies { get; } = new();

    void IDependenciesResolved.OnDependenciesResolved(bool isAllDependenciesReady)
    {
        IsDependenciesReady = isAllDependenciesReady;
        if (isAllDependenciesReady)
        {
            GD.Print("[EnemySpawner] Dependencies ready!");
        }
        else
        {
            GD.Print("[EnemySpawner] Dependencies failed!");
        }
    }

    public void SpawnEnemy(string enemyType)
    {
        if (!IsDependenciesReady)
            return;

        var enemy = _enemyFactory.CreateEnemy(enemyType);
        SpawnedEnemies.Add(enemy);
        GD.Print($"[EnemySpawner] Spawned {enemyType} enemy");
    }

    public void TriggerAllEnemyAttacks()
    {
        foreach (var enemy in SpawnedEnemies)
        {
            enemy.Attack();
        }
    }

    public IEnemyFactory GetEnemyFactory() => _enemyFactory;

    // Required for DI framework
    public override partial void _Notification(int what);
}
