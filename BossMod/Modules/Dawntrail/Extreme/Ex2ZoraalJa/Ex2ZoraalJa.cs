﻿namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class MultidirectionalDivide(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivide), new AOEShapeCross(30, 2));
class MultidirectionalDivideMain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivideMain), new AOEShapeCross(30, 4));
class MultidirectionalDivideExtra(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivideExtra), new AOEShapeCross(40, 2));
class RegicidalRage(BossModule module) : Components.TankbusterTether(module, ActionID.MakeSpell(AID.RegicidalRageAOE), (uint)TetherID.RegicidalRage, 8);
class BitterWhirlwind(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.BitterWhirlwind), ActionID.MakeSpell(AID.BitterWhirlwindAOEFirst), ActionID.MakeSpell(AID.BitterWhirlwindAOERest), 3.1f, new AOEShapeCircle(5), true);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.BurningChainsAOE));
class HalfCircuitRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuitAOERect), new AOEShapeRect(60, 60));
class HalfCircuitDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuitAOEDonut), new AOEShapeDonut(10, 30));
class HalfCircuitCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuitAOECircle), new AOEShapeCircle(10));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 996, NameID = 12882, PlanLevel = 100)]
public class Ex2ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, NormalCenter, NormalBounds)
{
    public static readonly Angle ArenaRotation = 45.Degrees();
    public static readonly WPos NormalCenter = new(100, 100);
    public static readonly ArenaBoundsSquare NormalBounds = new(20, ArenaRotation);
    public static readonly ArenaBoundsSquare SmallBounds = new(10, ArenaRotation);
    public static readonly ArenaBoundsCustom NWPlatformBounds = BuildTwoPlatformsBounds(135.Degrees());
    public static readonly ArenaBoundsCustom NEPlatformBounds = BuildTwoPlatformsBounds(-135.Degrees());

    private static ArenaBoundsCustom BuildTwoPlatformsBounds(Angle orientation)
    {
        var dir = orientation.ToDirection();
        var main = new PolygonClipper.Operand(CurveApprox.Rect(dir, 10, 10).Select(p => p - 15 * dir));
        var side = new PolygonClipper.Operand(CurveApprox.Rect(dir, 10, 10).Select(p => p + 15 * dir));
        return new(20, NormalBounds.Clipper.Union(main, side));
    }
}
