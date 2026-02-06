using System.Collections.Generic;
using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Users;

[User]
public sealed partial class EnemySpawner : Node2D, IServicesReady
{
    [Inject]
    private IEnemyFactory _enemyFactory = null!;

    [Inject]
    private IPlayerStats _playerStats = null!;

    public bool IsServicesReady { get; private set; }
    public List<Enemy> SpawnedEnemies { get; } = new();

    void IServicesReady.OnServicesReady()
    {
        IsServicesReady = true;
        GD.Print("[EnemySpawner] Services ready!");
    }

    public void SpawnEnemy(string enemyType)
    {
        if (!IsServicesReady)
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