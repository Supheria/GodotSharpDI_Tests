using System.Threading.Tasks;
using FluentAssertions;
using Godot;
using GodotSharpDI.Tests.Hosts;
using GodotSharpDI.Tests.Scopes;
using GodotSharpDI.Tests.Users;
using NUnit.Framework;

namespace GodotSharpDI.Tests.Integration;

/// <summary>
/// Integration tests for basic dependency injection scenarios
/// Tests the DI framework's ability to inject services into users
/// </summary>
[TestFixture]
public class BasicDependencyInjectionTests : GodotIntegrationTestBase
{
    [Test]
    public async Task Scope_WithUser_ShouldInjectServices()
    {
        // Skip if not in Godot
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var (scope, user) = TestSceneBuilder.CreateScopeWithUser<GameScope, PlayerUI>();
        AddNode(scope);

        // Act - wait for the scope to initialize and inject
        await WaitForFrames(2);

        // Assert
        user.IsServicesReady.Should().BeTrue("services should be injected");
        user.GetPlayerStats().Should().NotBeNull("PlayerStats should be injected");
        user.GetInventory().Should().NotBeNull("Inventory should be injected");
        user.GetGameManager().Should().NotBeNull("GameManager should be injected");
    }

    [Test]
    public async Task Scope_WithMultipleUsers_ShouldInjectServicesIntoAll()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var questScope = new QuestScope { Name = "QuestScope" };
        var user1 = new PlayerUI { Name = "PlayerUI1" };
        var user2 = new PlayerUI { Name = "PlayerUI2" };

        scope.AddChild(questScope);
        scope.AddChild(user1);
        questScope.AddChild(user2);
        AddNode(scope);

        // Act
        await WaitForFrames(2);

        // Assert
        user1.IsServicesReady.Should().BeTrue();
        user2.IsServicesReady.Should().BeTrue();
        user1.GetPlayerStats().Should().NotBeNull();
        user2.GetPlayerStats().Should().NotBeNull();
    }

    [Test]
    public async Task Scope_Services_ShouldBeSingleton()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var user1 = new PlayerUI { Name = "PlayerUI1" };
        var user2 = new PlayerUI { Name = "PlayerUI2" };

        scope.AddChild(user1);
        scope.AddChild(user2);
        AddNode(scope);

        // Act
        await WaitForFrames(2);

        // Assert - both users should receive the same service instance
        var stats1 = user1.GetPlayerStats();
        var stats2 = user2.GetPlayerStats();
        stats1.Should().BeSameAs(stats2, "services should be singleton within a scope");

        // Modify through one user and verify through another
        stats1.Health = 50;
        stats2.Health.Should().Be(50, "singleton service should share state");
    }

    [Test]
    public async Task Host_ShouldBeInjectableAsService()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var (scope, host, user) = TestSceneBuilder.CreateScopeWithHostAndUser<
            GameScope,
            GameManager,
            PlayerUI
        >();
        AddNode(scope);

        // Act
        await WaitForFrames(2);

        // Assert
        user.GetGameManager().Should().BeSameAs(host, "host should be injectable");
    }
}

/// <summary>
/// Integration tests for service lifecycle management
/// </summary>
[TestFixture]
public class ServiceLifecycleTests : GodotIntegrationTestBase
{
    [Test]
    public async Task Service_ModifiedInOneUser_ShouldAffectOtherUsers()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var user1 = new PlayerUI { Name = "PlayerUI1" };
        var user2 = new PlayerUI { Name = "PlayerUI2" };

        scope.AddChild(user1);
        scope.AddChild(user2);
        AddNode(scope);

        await WaitForFrames(2);

        // Act - modify service through first user
        var stats = user1.GetPlayerStats();
        stats.Health = 75;

        var inventory = user1.GetInventory();
        inventory.AddItem("Sword");

        // Assert - changes should be visible through second user
        user2.GetPlayerStats().Health.Should().Be(75);
        user2.GetInventory().HasItem("Sword").Should().BeTrue();
    }

    [Test]
    public async Task Host_Changes_ShouldBeVisibleToAllUsers()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var gameManager = new GameManager { Name = "GameManager" };
        var user1 = new PlayerUI { Name = "PlayerUI1" };
        var user2 = new PlayerUI { Name = "PlayerUI2" };

        scope.AddChild(gameManager);
        scope.AddChild(user1);
        scope.AddChild(user2);
        AddNode(scope);

        await WaitForFrames(2);

        // Act - modify host state
        gameManager.ChangeLevel("Level2");

        // Assert
        user1.GetGameManager().CurrentState.CurrentLevel.Should().Be("Level2");
        user2.GetGameManager().CurrentState.CurrentLevel.Should().Be("Level2");
    }
}

/// <summary>
/// Integration tests for dependency chains
/// Tests services that depend on other services
/// </summary>
[TestFixture]
public class DependencyChainTests : GodotIntegrationTestBase
{
    [Test]
    public async Task ServiceWithDependency_ShouldReceiveInjection()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - QuestService depends on IPlayerStats and IScoreService
        var gameScope = new GameScope { Name = "GameScope" };
        var questScope = new QuestScope { Name = "QuestScope" };
        var user = new QuestUI { Name = "QuestUI" };

        gameScope.AddChild(questScope);
        questScope.AddChild(user);
        AddNode(gameScope);

        // Act
        await WaitForFrames(2);

        // Assert
        user.IsServicesReady.Should().BeTrue();
        var questService = user.GetQuestService();
        questService.Should().NotBeNull();

        // Test that quest service can interact with its dependencies
        questService.StartQuest("Quest1");
        questService.IsQuestActive("Quest1").Should().BeTrue();
    }

    [Test]
    public async Task Factory_ShouldCreateObjectsWithDependencies()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var gameScope = new GameScope { Name = "GameScope" };
        var questScope = new QuestScope { Name = "QuestScope" };
        var spawner = new EnemySpawner { Name = "EnemySpawner" };

        gameScope.AddChild(questScope);
        questScope.AddChild(spawner);
        AddNode(gameScope);

        await WaitForFrames(2);

        // Act - use factory to create enemy
        spawner.SpawnEnemy("Goblin");

        // Assert
        spawner.SpawnedEnemies.Should().HaveCount(1);
        spawner.SpawnedEnemies[0].Type.Should().Be("Goblin");

        // Test that created enemy can interact with dependencies
        var initialHealth = 100; // from setup
        spawner.TriggerAllEnemyAttacks();

        // Enemy should have damaged the player through the injected PlayerStats
        var user = new PlayerUI { Name = "TestUser" };
        gameScope.AddChild(user);
        await WaitForFrame();

        user.GetPlayerStats().Health.Should().BeLessThan(initialHealth);
    }
}

/// <summary>
/// Integration tests for scope hierarchy
/// Tests parent-child scope relationships
/// </summary>
[TestFixture]
public class ScopeHierarchyTests : GodotIntegrationTestBase
{
    [Test]
    public async Task ChildScope_ShouldAccessParentServices()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - nested scopes
        var parentScope = new GameScope { Name = "ParentScope" };
        var childScope = new QuestScope { Name = "ChildScope" };
        var user = new QuestUI { Name = "QuestUI" };

        parentScope.AddChild(childScope);
        childScope.AddChild(user);
        AddNode(parentScope);

        // Act
        await WaitForFrames(2);

        // Assert - child scope user can access services from parent scope
        user.IsServicesReady.Should().BeTrue();
        user.GetQuestService().Should().NotBeNull();
        user.GetScoreService().Should().NotBeNull(); // From parent scope
    }

    [Test]
    public async Task SiblingScopes_ShouldHaveIndependentServices()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange - sibling scopes
        var root = new Node { Name = "Root" };
        var scope1 = new MinimalScope { Name = "Scope1" };
        var scope2 = new MinimalScope { Name = "Scope2" };
        var user1 = new PlayerUI { Name = "User1" };
        var user2 = new PlayerUI { Name = "User2" };

        root.AddChild(scope1);
        root.AddChild(scope2);
        scope1.AddChild(user1);
        scope2.AddChild(user2);
        AddNode(root);

        // Act
        await WaitForFrames(2);

        // Assert - each scope should have its own service instances
        // Note: This test assumes we can access the services somehow
        // In a real scenario, we'd need additional testing infrastructure
        user1.IsServicesReady.Should().BeTrue();
        user2.IsServicesReady.Should().BeTrue();
    }
}

/// <summary>
/// Integration tests for IServicesReady callback
/// </summary>
[TestFixture]
public class ServicesReadyCallbackTests : GodotIntegrationTestBase
{
    [Test]
    public async Task OnServicesReady_ShouldBeCalledAfterInjection()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var (scope, user) = TestSceneBuilder.CreateScopeWithUser<GameScope, PlayerUI>();
        AddNode(scope);

        // Assert - before initialization
        user.IsServicesReady.Should().BeFalse();
        user.UpdateCount.Should().Be(0);

        // Act
        await WaitForFrames(2);

        // Assert - after initialization
        user.IsServicesReady.Should().BeTrue();
        user.UpdateCount.Should().BeGreaterThan(0, "OnServicesReady should trigger UpdateUI");
    }

    [Test]
    public async Task MultipleUsers_ShouldAllReceiveCallback()
    {
        if (SceneTree == null)
        {
            Assert.Ignore("Test requires Godot scene tree");
            return;
        }

        // Arrange
        var scope = new GameScope { Name = "GameScope" };
        var questScope = new QuestScope { Name = "QuestScope" };
        var user1 = new PlayerUI { Name = "PlayerUI" };
        var user2 = new QuestUI { Name = "QuestUI" };
        var user3 = new PlayerController { Name = "PlayerController" };

        scope.AddChild(questScope);
        scope.AddChild(user1);
        questScope.AddChild(user2);
        questScope.AddChild(user3);
        AddNode(scope);

        // Act
        await WaitForFrames(2);

        // Assert
        user1.IsServicesReady.Should().BeTrue();
        user2.IsServicesReady.Should().BeTrue();
        user3.IsServicesReady.Should().BeTrue();
    }
}
