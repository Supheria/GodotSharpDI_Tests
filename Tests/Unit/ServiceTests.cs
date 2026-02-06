using NUnit.Framework;
using FluentAssertions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Unit;

/// <summary>
/// Unit tests for PlayerStatsService
/// Tests business logic without DI framework involvement
/// </summary>
[TestFixture]
public class PlayerStatsServiceTests
{
    private PlayerStatsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new PlayerStatsService();
    }

    [Test]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _service.Health.Should().Be(100);
        _service.MaxHealth.Should().Be(100);
        _service.Mana.Should().Be(50);
        _service.MaxMana.Should().Be(50);
    }

    [Test]
    public void TakeDamage_ShouldReduceHealth()
    {
        // Act
        _service.TakeDamage(30);

        // Assert
        _service.Health.Should().Be(70);
    }

    [Test]
    public void TakeDamage_ShouldNotGoBelowZero()
    {
        // Act
        _service.TakeDamage(150);

        // Assert
        _service.Health.Should().Be(0);
    }

    [Test]
    public void Heal_ShouldIncreaseHealth()
    {
        // Arrange
        _service.TakeDamage(50);

        // Act
        _service.Heal(30);

        // Assert
        _service.Health.Should().Be(80);
    }

    [Test]
    public void Heal_ShouldNotExceedMaxHealth()
    {
        // Arrange
        _service.TakeDamage(20);

        // Act
        _service.Heal(50);

        // Assert
        _service.Health.Should().Be(100);
    }

    [Test]
    public void ConsumeMana_ShouldReduceMana()
    {
        // Act
        _service.ConsumeMana(20);

        // Assert
        _service.Mana.Should().Be(30);
    }

    [Test]
    public void ConsumeMana_ShouldNotGoBelowZero()
    {
        // Act
        _service.ConsumeMana(100);

        // Assert
        _service.Mana.Should().Be(0);
    }

    [Test]
    public void RestoreMana_ShouldIncreaseMana()
    {
        // Arrange
        _service.ConsumeMana(30);

        // Act
        _service.RestoreMana(20);

        // Assert
        _service.Mana.Should().Be(40);
    }

    [Test]
    public void RestoreMana_ShouldNotExceedMaxMana()
    {
        // Arrange
        _service.ConsumeMana(10);

        // Act
        _service.RestoreMana(50);

        // Assert
        _service.Mana.Should().Be(50);
    }
}

/// <summary>
/// Unit tests for InventoryService
/// </summary>
[TestFixture]
public class InventoryServiceTests
{
    private InventoryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new InventoryService();
    }

    [Test]
    public void Constructor_ShouldInitializeEmpty()
    {
        // Assert
        _service.ItemCount.Should().Be(0);
        _service.GetAllItems().Should().BeEmpty();
    }

    [Test]
    public void AddItem_ShouldIncreaseItemCount()
    {
        // Act
        _service.AddItem("Sword");

        // Assert
        _service.ItemCount.Should().Be(1);
    }

    [Test]
    public void AddItem_ShouldStoreItem()
    {
        // Act
        _service.AddItem("Potion");

        // Assert
        _service.HasItem("Potion").Should().BeTrue();
        _service.GetAllItems().Should().Contain("Potion");
    }

    [Test]
    public void AddMultipleItems_ShouldStoreAll()
    {
        // Act
        _service.AddItem("Sword");
        _service.AddItem("Shield");
        _service.AddItem("Potion");

        // Assert
        _service.ItemCount.Should().Be(3);
        _service.GetAllItems().Should().BeEquivalentTo(new[] { "Sword", "Shield", "Potion" });
    }

    [Test]
    public void RemoveItem_ShouldDecreaseItemCount()
    {
        // Arrange
        _service.AddItem("Sword");
        _service.AddItem("Shield");

        // Act
        _service.RemoveItem("Sword");

        // Assert
        _service.ItemCount.Should().Be(1);
    }

    [Test]
    public void RemoveItem_ShouldRemoveCorrectItem()
    {
        // Arrange
        _service.AddItem("Sword");
        _service.AddItem("Shield");

        // Act
        _service.RemoveItem("Sword");

        // Assert
        _service.HasItem("Sword").Should().BeFalse();
        _service.HasItem("Shield").Should().BeTrue();
    }

    [Test]
    public void HasItem_ShouldReturnFalseForNonexistentItem()
    {
        // Assert
        _service.HasItem("NonExistent").Should().BeFalse();
    }
}

/// <summary>
/// Unit tests for ScoreService
/// </summary>
[TestFixture]
public class ScoreServiceTests
{
    private ScoreService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ScoreService();
    }

    [Test]
    public void Constructor_ShouldInitializeWithZeroScore()
    {
        // Assert
        _service.CurrentScore.Should().Be(0);
    }

    [Test]
    public void AddScore_ShouldIncreaseScore()
    {
        // Act
        _service.AddScore(100);

        // Assert
        _service.CurrentScore.Should().Be(100);
    }

    [Test]
    public void AddScore_MultipleTimes_ShouldAccumulate()
    {
        // Act
        _service.AddScore(100);
        _service.AddScore(50);
        _service.AddScore(25);

        // Assert
        _service.CurrentScore.Should().Be(175);
    }

    [Test]
    public void ResetScore_ShouldSetScoreToZero()
    {
        // Arrange
        _service.AddScore(500);

        // Act
        _service.ResetScore();

        // Assert
        _service.CurrentScore.Should().Be(0);
    }

    [Test]
    public void AddScore_AfterReset_ShouldStartFromZero()
    {
        // Arrange
        _service.AddScore(500);
        _service.ResetScore();

        // Act
        _service.AddScore(100);

        // Assert
        _service.CurrentScore.Should().Be(100);
    }
}

/// <summary>
/// Unit tests for EnemyFactory
/// Tests factory pattern with dependency injection
/// </summary>
[TestFixture]
public class EnemyFactoryTests
{
    private PlayerStatsService _playerStats = null!;
    private EnemyFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _playerStats = new PlayerStatsService();
        _factory = new EnemyFactory(_playerStats);
    }

    [Test]
    public void CreateEnemy_ShouldReturnEnemyWithCorrectType()
    {
        // Act
        var enemy = _factory.CreateEnemy("Goblin");

        // Assert
        enemy.Should().NotBeNull();
        enemy.Type.Should().Be("Goblin");
    }

    [Test]
    public void CreateEnemy_MultipleTimes_ShouldCreateDifferentInstances()
    {
        // Act
        var enemy1 = _factory.CreateEnemy("Goblin");
        var enemy2 = _factory.CreateEnemy("Orc");

        // Assert
        enemy1.Should().NotBeSameAs(enemy2);
        enemy1.Type.Should().Be("Goblin");
        enemy2.Type.Should().Be("Orc");
    }

    [Test]
    public void CreatedEnemy_Attack_ShouldDamagePlayer()
    {
        // Arrange
        var enemy = _factory.CreateEnemy("Dragon");
        var initialHealth = _playerStats.Health;

        // Act
        enemy.Attack();

        // Assert
        _playerStats.Health.Should().BeLessThan(initialHealth);
    }
}
