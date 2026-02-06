using System.Threading.Tasks;
using FluentAssertions;
using Godot;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Scopes;
using GodotSharpDI.Tests.Services;
using GodotSharpDI.Tests.Users;
using NUnit.Framework;

namespace GodotSharpDI.Tests.Integration;

/// <summary>
/// Advanced integration tests for complex DI scenarios
/// </summary>
[TestFixture]
public class AdvancedDIScenarioTests : GodotIntegrationTestBase
{
    [Test]
    public async Task ComplexSceneHierarchy_ShouldWorkCorrectly()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - build a complex scene hierarchy
        var rootScope = new GameScope { Name = "RootScope" };
        var gameManager = new GameManager { Name = "GameManager" };
        var audioManager = new AudioManager { Name = "AudioManager" };

        var subScope1 = new QuestScope { Name = "SubScope1" };
        var subScope2 = new QuestScope { Name = "SubScope2" };

        var ui1 = new PlayerUI { Name = "UI1" };
        var ui2 = new QuestUI { Name = "UI2" };
        var controller = new PlayerController { Name = "Controller" };
        var spawner = new EnemySpawner { Name = "Spawner" };

        // Build hierarchy
        rootScope.AddChild(gameManager);
        rootScope.AddChild(audioManager);
        rootScope.AddChild(ui1);
        rootScope.AddChild(subScope1);
        rootScope.AddChild(subScope2);

        subScope1.AddChild(ui2);
        subScope1.AddChild(controller);
        subScope2.AddChild(spawner);

        AddNode(rootScope);

        // Act
        await WaitForFrames(3);

        // Assert - all components should be initialized
        ui1.IsServicesReady.Should().BeTrue();
        ui2.IsServicesReady.Should().BeTrue();
        controller.IsServicesReady.Should().BeTrue();
        spawner.IsServicesReady.Should().BeTrue();

        // Verify service sharing
        var stats1 = ui1.GetPlayerStats();
        var stats2 = controller.GetPlayerStats();
        stats1.Should().BeSameAs(stats2, "services should be shared across hierarchy");
    }

    [Test]
    public async Task DynamicNodeAddition_ShouldReceiveInjection()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - create initial scene
        var scope = new GameScope { Name = "GameScope" };
        AddNode(scope);
        await WaitForFrames(2);

        // Act - add user dynamically after scope is initialized
        var dynamicUser = new PlayerUI { Name = "DynamicUI" };
        scope.AddChild(dynamicUser);
        await WaitForFrames(2);

        // Assert - dynamically added user should receive injection
        dynamicUser.IsServicesReady.Should().BeTrue();
        dynamicUser.GetPlayerStats().Should().NotBeNull();
    }

    [Test]
    public async Task ServiceModification_ShouldPropagateToAllUsers()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var ui1 = new PlayerUI { Name = "UI1" };
        var ui2 = new PlayerUI { Name = "UI2" };
        var ui3 = new PlayerUI { Name = "UI3" };

        scope.AddChild(ui1);
        scope.AddChild(ui2);
        scope.AddChild(ui3);
        AddNode(scope);

        await WaitForFrames(2);

        // Act - modify service through one user
        var stats = ui1.GetPlayerStats();
        stats.Health = 25;
        stats.Mana = 10;

        var inventory = ui1.GetInventory();
        inventory.AddItem("Legendary Sword");
        inventory.AddItem("Health Potion");

        // Assert - changes visible through all users
        ui2.GetPlayerStats().Health.Should().Be(25);
        ui2.GetPlayerStats().Mana.Should().Be(10);
        ui3.GetPlayerStats().Health.Should().Be(25);
        ui3.GetPlayerStats().Mana.Should().Be(10);

        ui2.GetInventory().ItemCount.Should().Be(2);
        ui3.GetInventory().HasItem("Legendary Sword").Should().BeTrue();
    }

    [Test]
    public async Task HostStateChanges_ShouldBeVisibleToAllUsers()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var gameManager = new GameManager { Name = "GameManager" };
        var ui1 = new PlayerUI { Name = "UI1" };
        var ui2 = new PlayerUI { Name = "UI2" };

        scope.AddChild(gameManager);
        scope.AddChild(ui1);
        scope.AddChild(ui2);
        AddNode(scope);

        await WaitForFrames(2);

        // Act - modify host state
        gameManager.ChangeLevel("BossLevel");
        gameManager.PauseGame();

        // Assert - host state changes visible through injected references
        ui1.GetGameManager().CurrentState.CurrentLevel.Should().Be("BossLevel");
        ui1.GetGameManager().CurrentState.IsPaused.Should().BeTrue();

        ui2.GetGameManager().CurrentState.CurrentLevel.Should().Be("BossLevel");
        ui2.GetGameManager().CurrentState.IsPaused.Should().BeTrue();
    }

    [Test]
    public async Task FactoryService_ShouldCreateObjectsWithSharedDependencies()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var gameScope = new GameScope { Name = "GameScope" };
        var questScope = new QuestScope { Name = "QuestScope" };
        var spawner = new EnemySpawner { Name = "Spawner" };
        var ui = new PlayerUI { Name = "UI" };

        gameScope.AddChild(questScope);
        gameScope.AddChild(ui);
        questScope.AddChild(spawner);
        AddNode(gameScope);

        await WaitForFrames(2);

        // Get initial health
        var initialHealth = ui.GetPlayerStats().Health;

        // Act - spawn enemies and make them attack
        spawner.SpawnEnemy("Goblin");
        spawner.SpawnEnemy("Orc");
        spawner.SpawnEnemy("Dragon");

        spawner.TriggerAllEnemyAttacks();

        // Assert - enemies should have damaged the player through shared service
        ui.GetPlayerStats().Health.Should().BeLessThan(initialHealth);
        spawner.SpawnedEnemies.Should().HaveCount(3);
    }

    [Test]
    public async Task MultipleHosts_ShouldAllBeInjectable()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var gameManager = new GameManager { Name = "GameManager" };
        var audioManager = new AudioManager { Name = "AudioManager" };
        var inputManager = new InputManager { Name = "InputManager" };
        var controller = new PlayerController { Name = "Controller" };

        scope.AddChild(gameManager);
        scope.AddChild(audioManager);
        scope.AddChild(inputManager);

        var questScope = new QuestScope { Name = "QuestScope" };
        scope.AddChild(questScope);
        questScope.AddChild(controller);

        AddNode(scope);

        await WaitForFrames(2);

        // Assert - controller should have all hosts injected
        controller.IsServicesReady.Should().BeTrue();
        controller.GetPlayerStats().Should().NotBeNull();
        controller.GetInputManager().Should().BeSameAs(inputManager);
        controller.GetAudioManager().Should().BeSameAs(audioManager);

        // Test interaction between hosts and services
        controller.TakeDamage(30);
        controller.GetPlayerStats().Health.Should().Be(70);
    }

    [Test]
    public async Task ScopeDestruction_ShouldNotAffectOtherScopes()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - create two independent scopes
        var scope1 = new MinimalScope { Name = "Scope1" };
        var scope2 = new MinimalScope { Name = "Scope2" };
        var user1 = new PlayerUI { Name = "User1" };
        var user2 = new PlayerUI { Name = "User2" };

        scope1.AddChild(user1);
        scope2.AddChild(user2);

        AddNode(scope1);
        AddNode(scope2);

        await WaitForFrames(2);

        // Verify both are working
        user1.IsServicesReady.Should().BeTrue();
        user2.IsServicesReady.Should().BeTrue();

        // Act - destroy first scope
        scope1.QueueFree();
        await WaitForFrames(2);

        // Assert - second scope should still work
        user2.IsServicesReady.Should().BeTrue();
        user2.GetPlayerStats().Should().NotBeNull();
    }
}

/// <summary>
/// Tests for edge cases and error scenarios
/// </summary>
[TestFixture]
public class EdgeCaseTests : GodotIntegrationTestBase
{
    [Test]
    public async Task EmptyScope_ShouldNotCauseErrors()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange & Act
        var scope = new EmptyScope { Name = "EmptyScope" };
        AddNode(scope);
        await WaitForFrames(2);

        // Assert - should not crash
        GodotObject.IsInstanceValid(scope).Should().BeTrue();
    }

    [Test]
    public async Task UserWithoutScope_ShouldHandleGracefully()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - create user without parent scope
        var user = new PlayerUI { Name = "OrphanUser" };
        AddNode(user);

        // Act
        await WaitForFrames(2);

        // Assert - user should exist but not be ready
        GodotObject.IsInstanceValid(user).Should().BeTrue();
        user.IsServicesReady.Should().BeFalse();
    }

    [Test]
    public async Task NestedScopes_DeepHierarchy_ShouldWork()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - create 5-level deep scope hierarchy
        var level1 = new GameScope { Name = "Level1" };
        var level2 = new QuestScope { Name = "Level2" };
        var level3 = new QuestScope { Name = "Level3" };
        var level4 = new QuestScope { Name = "Level4" };
        var level5 = new QuestScope { Name = "Level5" };
        var deepUser = new QuestUI { Name = "DeepUser" };

        level1.AddChild(level2);
        level2.AddChild(level3);
        level3.AddChild(level4);
        level4.AddChild(level5);
        level5.AddChild(deepUser);

        AddNode(level1);

        // Act
        await WaitForFrames(3);

        // Assert - deep user should receive services from top level
        deepUser.IsServicesReady.Should().BeTrue();
        deepUser.GetQuestService().Should().NotBeNull();
        deepUser.GetScoreService().Should().NotBeNull();
    }
}
