using System;
using System.Collections.Generic;
using GodotSharpDI.Abstractions;

namespace GodotSharpDI.Tests.Services;

// ========================================
// Service Interfaces
// ========================================

public interface IPlayerStats
{
    int Health { get; set; }
    int MaxHealth { get; }
    int Mana { get; set; }
    int MaxMana { get; }
    void TakeDamage(int damage);
    void Heal(int amount);
    void ConsumeMana(int amount);
    void RestoreMana(int amount);
}

public interface IInventoryService
{
    int ItemCount { get; }
    void AddItem(string itemName);
    void RemoveItem(string itemName);
    bool HasItem(string itemName);
    List<string> GetAllItems();
}

public interface IQuestService
{
    int ActiveQuestCount { get; }
    void StartQuest(string questId);
    void CompleteQuest(string questId);
    bool IsQuestActive(string questId);
    List<string> GetActiveQuests();
}

public interface IScoreService
{
    int CurrentScore { get; }
    void AddScore(int points);
    void ResetScore();
}

// ========================================
// Service Implementations
// ========================================

[Singleton(typeof(IPlayerStats))]
public partial class PlayerStatsService : IPlayerStats
{
    public int Health { get; set; } = 100;
    public int MaxHealth => 100;
    public int Mana { get; set; } = 50;
    public int MaxMana => 50;

    public void TakeDamage(int damage)
    {
        Health = Math.Max(0, Health - damage);
    }

    public void Heal(int amount)
    {
        Health = Math.Min(MaxHealth, Health + amount);
    }

    public void ConsumeMana(int amount)
    {
        Mana = Math.Max(0, Mana - amount);
    }

    public void RestoreMana(int amount)
    {
        Mana = Math.Min(MaxMana, Mana + amount);
    }
}

[Singleton(typeof(IInventoryService))]
public partial class InventoryService : IInventoryService
{
    private readonly List<string> _items = new();

    public int ItemCount => _items.Count;

    public void AddItem(string itemName)
    {
        _items.Add(itemName);
    }

    public void RemoveItem(string itemName)
    {
        _items.Remove(itemName);
    }

    public bool HasItem(string itemName)
    {
        return _items.Contains(itemName);
    }

    public List<string> GetAllItems()
    {
        return new List<string>(_items);
    }
}

[Singleton(typeof(IQuestService))]
public partial class QuestService : IQuestService
{
    private readonly IPlayerStats _playerStats;
    private readonly HashSet<string> _activeQuests = new();

    public int ActiveQuestCount => _activeQuests.Count;

    // Constructor injection with dependency
    [InjectConstructor]
    public QuestService(IPlayerStats playerStats)
    {
        _playerStats = playerStats;
    }

    public void StartQuest(string questId)
    {
        _activeQuests.Add(questId);
    }

    public void CompleteQuest(string questId)
    {
        if (_activeQuests.Remove(questId))
        {
            // Quest completed successfully
            // In a real game, you might give rewards here
        }
    }

    public bool IsQuestActive(string questId)
    {
        return _activeQuests.Contains(questId);
    }

    public List<string> GetActiveQuests()
    {
        return new List<string>(_activeQuests);
    }
}

[Singleton(typeof(IScoreService))]
public partial class ScoreService : IScoreService
{
    public int CurrentScore { get; private set; }

    public void AddScore(int points)
    {
        CurrentScore += points;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
    }
}

// Factory service example
public interface IEnemyFactory
{
    Enemy CreateEnemy(string enemyType);
}

[Singleton(typeof(IEnemyFactory))]
public partial class EnemyFactory : IEnemyFactory
{
    private readonly IPlayerStats _playerStats;

    [InjectConstructor]
    public EnemyFactory(IPlayerStats playerStats)
    {
        _playerStats = playerStats;
    }

    public Enemy CreateEnemy(string enemyType)
    {
        return new Enemy(enemyType, _playerStats);
    }
}

// Non-DI class that receives dependencies
public class Enemy
{
    public string Type { get; }
    private readonly IPlayerStats _playerStats;

    public Enemy(string type, IPlayerStats playerStats)
    {
        Type = type;
        _playerStats = playerStats;
    }

    public void Attack()
    {
        _playerStats.TakeDamage(10);
    }
}
