# GodotSharpDI 测试项目

这是一个基于 Godot 4 和 GodotSharpDI (v1.0.0-rc.2) 的完整测试项目，包含单元测试和集成测试。

## 项目结构

```
GodotSharpDI_Tests/
├── Services/           # 服务实现（Singleton Services）
│   └── TestServices.cs # 测试用的服务类
├── Hosts/              # 主机节点（Host）
│   └── TestHosts.cs    # GameManager、AudioManager 等
├── Users/              # 使用者节点（User）
│   └── TestUsers.cs    # PlayerUI、QuestUI 等
├── Scopes/             # 作用域（Scope/Container）
│   └── TestScopes.cs   # GameScope、QuestScope 等
├── Scenes/             # Godot 场景
│   └── TestScene.tscn  # 主测试场景
├── Tests/
│   ├── Unit/           # 单元测试
│   │   └── ServiceTests.cs
│   └── Integration/    # 集成测试
│       ├── GodotIntegrationTestBase.cs
│       └── DependencyInjectionTests.cs
├── project.godot       # Godot 项目配置
└── GodotSharpDI_Tests.csproj  # C# 项目文件
```

## 功能特性

### 1. 服务定义（Services）

项目包含多个示例服务：

- **IPlayerStats / PlayerStatsService**: 玩家状态管理（血量、魔法值）
- **IInventoryService / InventoryService**: 物品栏管理
- **IQuestService / QuestService**: 任务系统（带依赖注入）
- **IScoreService / ScoreService**: 分数系统
- **IEnemyFactory / EnemyFactory**: 工厂模式示例

### 2. 主机节点（Hosts）

Hosts 是场景级资源提供者：

- **GameManager**: 游戏状态管理器
- **AudioManager**: 音频管理器
- **InputManager**: 输入管理器

### 3. 使用者节点（Users）

Users 接收依赖注入：

- **PlayerUI**: 玩家界面，注入玩家状态和物品栏
- **QuestUI**: 任务界面，注入任务服务
- **PlayerController**: 玩家控制器，注入多个服务
- **EnemySpawner**: 敌人生成器，使用工厂模式

### 4. 作用域（Scopes）

- **GameScope**: 主游戏作用域
- **QuestScope**: 任务系统作用域
- **MinimalScope**: 最小测试作用域
- **EmptyScope**: 空作用域（用于测试层级）

## 测试覆盖

### 单元测试 (Unit Tests)

测试纯业务逻辑，不依赖 DI 框架：

- `PlayerStatsServiceTests`: 测试生命值、魔法值管理
- `InventoryServiceTests`: 测试物品添加、移除
- `ScoreServiceTests`: 测试分数累加、重置
- `EnemyFactoryTests`: 测试工厂模式和依赖传递

### 集成测试 (Integration Tests)

测试 DI 框架功能：

1. **BasicDependencyInjectionTests**
   - 服务注入到 User
   - 多个 User 同时注入
   - 单例服务共享

2. **ServiceLifecycleTests**
   - 服务状态共享
   - Host 变更传播

3. **DependencyChainTests**
   - 服务依赖链
   - 工厂创建对象

4. **ScopeHierarchyTests**
   - 子作用域访问父作用域服务
   - 兄弟作用域独立性

5. **ServicesReadyCallbackTests**
   - IServicesReady 回调触发
   - 多个 User 回调

## 使用方法

### 前置要求

- Godot 4.3 或更高版本
- .NET 8.0 SDK
- NuGet 包管理器

### 安装依赖

项目依赖以下 NuGet 包（已在 .csproj 中配置）：

```xml
<PackageReference Include="GodotSharpDI" Version="1.0.0-rc.2" />
<PackageReference Include="NUnit" Version="4.1.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
<PackageReference Include="NSubstitute" Version="5.1.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

### 运行单元测试

单元测试可以在任何 .NET 测试运行器中运行：

```bash
# 使用 dotnet test
dotnet test --filter "FullyQualifiedName~GodotSharpDI.Tests.Unit"

# 使用 Visual Studio Test Explorer
# 或 JetBrains Rider 的测试面板
```

### 运行集成测试

集成测试需要在 Godot 编辑器中运行：

1. 在 Godot 中打开项目
2. 构建 C# 项目（Build → Build Project）
3. 运行场景 `Scenes/TestScene.tscn`
4. 或使用 Godot 的测试运行器插件

### 在 Godot 编辑器中测试

1. 打开 `Scenes/TestScene.tscn`
2. 运行场景（F5 或点击播放按钮）
3. 查看控制台输出，验证 DI 是否正常工作

预期输出示例：

```
[GameScope] Scope initialized
[GameManager] Providing self as service
[PlayerUI] Services ready! Initializing UI...
[PlayerUI] Update #1
  Health: 100/100
  Mana: 50/50
  Items: 0
  Level: Level1
[QuestUI] Services ready!
[QuestUI] Active Quests: 0
[QuestUI] Current Score: 0
```

## 测试场景说明

### TestScene.tscn

场景树结构：

```
TestScene (Node)
└── GameScope (IScope)
    ├── GameManager (Host)
    ├── AudioManager (Host)
    ├── InputManager (Host)
    ├── PlayerUI (User)
    └── QuestScope (IScope)
        ├── QuestUI (User)
        ├── PlayerController (User)
        └── EnemySpawner (User)
```

这个场景演示了：
- 嵌套 Scope 层级
- Host 提供服务
- User 接收注入
- 服务在不同层级间共享

## 关键概念

### 1. 必须定义 _Notification 方法

所有 Host、User 和 Scope 都必须显式定义：

```csharp
public override partial void _Notification(int what);
```

这是 Godot 引擎要求，以便识别生命周期钩子。

### 2. 服务生命周期

- **Singleton Services**: 在 Scope 创建时实例化，Scope 销毁时释放
- **Host**: 与 Node 生命周期绑定
- **依赖注入时机**: Node 进入场景树后触发

### 3. 作用域层级

子 Scope 可以访问父 Scope 的服务，实现服务继承。

### 4. IServicesReady 接口

User 实现此接口后，在所有依赖注入完成后会收到回调：

```csharp
void IServicesReady.OnServicesReady()
{
    // 所有依赖已就绪，可以安全使用
}
```

## 扩展测试

### 添加新服务

1. 在 `Services/` 目录定义接口和实现
2. 用 `[Singleton(typeof(IYourService))]` 标记
3. 在 Scope 的 `[Modules]` 中注册
4. 编写单元测试

### 添加新 Host

1. 在 `Hosts/` 目录创建继承自 `Node` 的类
2. 用 `[Host]` 标记
3. 提供服务：`[Singleton(typeof(...))] private T Self => this;`
4. 在 Scope 的 `[Modules]` 中注册

### 添加新 User

1. 在 `Users/` 目录创建继承自 `Node` 的类
2. 用 `[User]` 标记
3. 用 `[Inject]` 标记需要注入的字段
4. 可选：实现 `IServicesReady`

### 添加集成测试

1. 继承 `GodotIntegrationTestBase`
2. 在 `SetUp` 中构建测试场景
3. 使用 `await WaitForFrames()` 等待初始化
4. 断言注入状态

## 常见问题

### Q: 为什么集成测试需要 await WaitForFrames()？

A: DI 注入发生在节点进入场景树后的下一帧，需要等待 Godot 完成初始化。

### Q: 如何验证服务是单例的？

A: 从多个 User 获取同一服务，用 `Should().BeSameAs()` 验证是同一实例。

### Q: 子 Scope 如何访问父 Scope 的服务？

A: 子 Scope 会自动继承父 Scope 的服务，无需额外配置。

### Q: 可以在运行时动态添加服务吗？

A: 不可以，GodotSharpDI 是编译时 DI，所有服务必须在编译时定义。

## 参考资料

- [GodotSharpDI GitHub](https://github.com/yourusername/GodotSharpDI)
- [GodotSharpDI 文档](https://github.com/yourusername/GodotSharpDI/blob/main/README.md)
- [NUnit 文档](https://docs.nunit.org/)
- [FluentAssertions 文档](https://fluentassertions.com/)

## 许可证

本测试项目遵循 GodotSharpDI 的许可证。
