using System.Threading.Tasks;
using Godot;
using GodotSharpDI.Abstractions;
using GodotSharpDI.Tests.Services;

namespace GodotSharpDI.Tests.Hosts;

[Host]
public sealed partial class GameScopeServiceHost : Node
{
    [Provide(ExposedTypes = [typeof(IPlayerStats), typeof(ISecond)])]
    public async Task<PlayerStatsService> PlayerStats()
    {
        // throw new System.NotImplementedException();
        GD.Print("[GameScopeServiceHost] begin loading player stats ...");
        await Task.Delay(3000);
        GD.Print("[GameScopeServiceHost] finish loading player stats!");
        return new PlayerStatsService();
    }

    [Provide(ExposedTypes = [typeof(IInventoryService)])]
    public InventoryService InventoryService { get; } = new();

    [Provide(ExposedTypes = [typeof(IScoreService)])]
    public ScoreService ScoreService { get; } = new();

    public override partial void _Notification(int what);
}
