﻿namespace BossMod.Dawntrail.Raid.M1NBlackCat;

class BloodyScratch(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BloodyScratch));
class BiscuitMaker(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BiscuitMaker));
class Clawful(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Clawful), 5, 8);
class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Shockwave), 18, stopAfterWall: true);
class GrimalkinGale2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.GrimalkinGale2), 5);
class Overshadow(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.OverShadowMarker), ActionID.MakeSpell(AID.Overshadow), 5.3f, 60, 2.5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 985, NameID = 12686)]
public class M1NBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));