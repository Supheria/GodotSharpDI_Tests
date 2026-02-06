# GodotSharpDI 测试项目 - 快速开始

## 🚀 快速开始（5分钟）

### 方式一：在 Godot 编辑器中运行

1. **打开项目**
   ```bash
   # 在 Godot 4.3+ 中打开项目目录
   godot --editor .
   ```

2. **构建 C# 项目**
   - 点击菜单：Build → Build Project
   - 或按 Ctrl+B (Windows/Linux) / Cmd+B (Mac)

3. **运行测试场景**
   - 打开 `Scenes/TestScene.tscn`
   - 按 F5 或点击播放按钮
   - 查看控制台输出

4. **运行自动化测试**
   - 打开 `Scenes/TestRunner.tscn`
   - 按 F5 运行
   - 测试结果会显示在控制台
   - 按 R 重新运行测试
   - 按 Q 退出

### 方式二：使用命令行

1. **运行单元测试**
   ```bash
   # 在项目根目录
   chmod +x build_and_test.sh
   ./build_and_test.sh
   ```

2. **运行集成测试（需要 Godot 可执行文件）**
   ```bash
   # Headless 模式运行集成测试
   godot --headless --path . Scenes/TestRunner.tscn
   ```

## 📁 项目结构一览

```
GodotSharpDI_Tests/
├── 📂 Services/        ← 业务逻辑服务（不依赖 Godot Node）
├── 📂 Hosts/           ← 场景资源提供者（Godot Nodes）
├── 📂 Users/           ← 依赖消费者（Godot Nodes）
├── 📂 Scopes/          ← DI 容器（管理服务生命周期）
├── 📂 Scenes/          ← 测试场景
├── 📂 Tests/
│   ├── Unit/          ← 纯逻辑单元测试
│   └── Integration/   ← Godot 场景树集成测试
└── TestRunner.cs      ← 自动化测试运行器
```

## 🎯 核心概念（60秒速览）

### 1. 四种角色

| 角色 | 特点 | 示例 |
|-----|-----|-----|
| **Service** | 纯逻辑，不是 Node | `PlayerStatsService` |
| **Host** | Node，提供服务 | `GameManager` |
| **User** | Node，消费服务 | `PlayerUI` |
| **Scope** | Node，管理容器 | `GameScope` |

### 2. 依赖注入流程

```
┌─────────────┐
│  GameScope  │  ← 定义提供哪些服务
│   (Scope)   │
└──────┬──────┘
       │
       ├──► PlayerStatsService (创建)
       ├──► InventoryService (创建)
       │
       ├──► GameManager (Host，自我注册)
       │
       └──► PlayerUI (User，接收注入)
                ↓
           [Inject] IPlayerStats
           [Inject] GameManager
```

### 3. 必须记住的规则

✅ **DO**
- Service: 用 `[Singleton(typeof(IService))]` 标记
- Host: 用 `[Host]` 标记，暴露自己为服务
- User: 用 `[User]` 标记，用 `[Inject]` 注入依赖
- Scope: 用 `[Modules(...)]` 定义服务和主机
- 所有类都必须定义: `public override partial void _Notification(int what);`

❌ **DON'T**
- Service 不能继承 Node
- 不要在构造函数中使用注入的依赖（可能为 null）
- 不要在 `_Ready()` 中使用注入的依赖（使用 `IServicesReady.OnServicesReady()`）

## 🧪 测试示例

### 运行单个测试类

```bash
dotnet test --filter "FullyQualifiedName~PlayerStatsServiceTests"
```

### 运行所有单元测试

```bash
dotnet test --filter "FullyQualifiedName~.Unit."
```

### 在 IDE 中运行

- **Visual Studio**: Test Explorer (Ctrl+E, T)
- **Rider**: Unit Tests 面板
- **VS Code**: .NET Core Test Explorer 扩展

## 📊 测试覆盖

### 单元测试 (可独立运行)

- ✅ `PlayerStatsServiceTests` - 生命值/魔法值逻辑
- ✅ `InventoryServiceTests` - 物品管理
- ✅ `ScoreServiceTests` - 分数系统
- ✅ `EnemyFactoryTests` - 工厂模式

### 集成测试 (需要 Godot)

- ✅ `BasicDependencyInjectionTests` - 基础注入
- ✅ `ServiceLifecycleTests` - 服务生命周期
- ✅ `DependencyChainTests` - 依赖链
- ✅ `ScopeHierarchyTests` - 作用域层级
- ✅ `ServicesReadyCallbackTests` - 回调机制
- ✅ `AdvancedDIScenarioTests` - 高级场景
- ✅ `EdgeCaseTests` - 边界情况

## 🎨 示例代码

### 定义服务

```csharp
public interface IPlayerStats
{
    int Health { get; set; }
}

[Singleton(typeof(IPlayerStats))]
public partial class PlayerStatsService : IPlayerStats
{
    public int Health { get; set; } = 100;
}
```

### 定义 Host（提供 Node 资源）

```csharp
[Host]
public partial class GameManager : Node
{
    [Singleton(typeof(GameManager))]
    private GameManager Self => this;
    
    public override partial void _Notification(int what);
}
```

### 定义 User（消费服务）

```csharp
[User]
public partial class PlayerUI : Control, IServicesReady
{
    [Inject] private IPlayerStats _stats;
    
    void IServicesReady.OnServicesReady()
    {
        // 依赖已就绪，可以安全使用
        GD.Print($"Health: {_stats.Health}");
    }
    
    public override partial void _Notification(int what);
}
```

### 定义 Scope（组装容器）

```csharp
[Modules(
    Services = [typeof(PlayerStatsService)],
    Hosts = [typeof(GameManager)]
)]
public partial class GameScope : Node, IScope
{
    public override partial void _Notification(int what);
}
```

### 场景树结构

```
GameScope
├── GameManager (Host)
└── PlayerUI (User) ← 自动注入 IPlayerStats 和 GameManager
```

## 🔧 故障排除

### 问题：编译错误 "partial method must have implementation"

**解决**: 确保添加了 `_Notification` 声明：
```csharp
public override partial void _Notification(int what);
```

### 问题：服务注入为 null

**原因**: 在 `_Ready()` 中访问注入的服务
**解决**: 实现 `IServicesReady` 接口：
```csharp
void IServicesReady.OnServicesReady()
{
    // 在这里使用注入的服务
}
```

### 问题：集成测试无法运行

**原因**: 测试需要 Godot 场景树
**解决**: 使用 `TestRunner.tscn` 或在 Godot 中运行

### 问题：子 Scope 找不到父 Scope 的服务

**原因**: 服务未在父 Scope 的 `[Modules]` 中声明
**解决**: 在父 Scope 中添加服务定义

## 📚 下一步

1. **修改测试服务** - 在 `Services/TestServices.cs` 中添加你的业务逻辑
2. **创建测试场景** - 在 Godot 中设计你的 DI 场景树
3. **编写单元测试** - 测试纯业务逻辑
4. **编写集成测试** - 测试 DI 容器行为
5. **查看 README.md** - 完整文档和高级用法

## 🆘 获取帮助

- 📖 查看 `README.md` 获取完整文档
- 🔍 检查 `Tests/` 目录中的示例
- 💡 参考原项目文档: `/mnt/project/README.md`

---

**提示**: 按照这个顺序学习效果最好：
1. 运行 `TestScene.tscn` 看看基础功能
2. 运行 `TestRunner.tscn` 看看测试结果
3. 阅读 `ServiceTests.cs` 理解单元测试
4. 阅读 `DependencyInjectionTests.cs` 理解集成测试
5. 开始修改和扩展！
