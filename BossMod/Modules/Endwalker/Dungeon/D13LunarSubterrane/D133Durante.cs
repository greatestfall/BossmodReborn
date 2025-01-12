﻿namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D133Durante;

public enum OID : uint
{
    Boss = 0x4042, // R=6.0
    AethericCharge = 0x4043, // R=1.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ArcaneEdge = 35010, // Boss->player, 5.0s cast, single-target
    OldMagic = 35011, // Boss->self, 5.0s cast, range 60 circle
    Teleport = 34991, // Boss->location, no cast, single-target
    DuplicitousBatteryTelegraph = 36058, // Helper->location, 3.5s cast, range 5 circle
    DuplicitousBattery = 36057, // Boss->self, 6.0s cast, single-target
    DuplicitousBattery2 = 34994, // Helper->location, no cast, range 5 circle
    ForsakenFount = 35003, // Boss->self, 3.0s cast, single-target
    Explosion = 35006, // 4043->self, 5.0s cast, range 11 circle
    Explosion2 = 35005, // Helper->self, 12.0s cast, range 9 circle
    FallenGrace = 35882, // Helper->player, 5.0s cast, range 6 circle
    Contrapasso = 35905, // Boss->self, 3.0s cast, range 60 circle
    Splinter = 35004, // 4043->self, no cast, single-target
    AntipodalAssaultMarker = 14588, // Helper->player, no cast, single-target
    AntipodalAssault = 35007, // Boss->self, 5.0s cast, single-target, line stack
    AntipodalAssault2 = 35008, // Boss->location, no cast, width 8 rect charge
    HardSlash = 35009, // Boss->self, 5.0s cast, range 50 90-degree cone
    TwilightPhase = 36055, // Boss->self, 6.0s cast, single-target
    TwilightPhaseA = 34997, // Boss->self, no cast, single-target
    TwilightPhaseB = 34998, // Boss->self, no cast, single-target
    TwilightPhase2 = 36056, // Helper->self, 7.3s cast, range 60 width 20 rect
    DarkImpact = 35001, // Boss->location, 7.0s cast, single-target
    DarkImpact2 = 35002, // Helper->self, 8.0s cast, range 25 circle
    DeathsJourney = 34995, // Boss->self, 6.0s cast, range 8 circle
    DeathsJourney2 = 34996, // Helper->self, 6.5s cast, range 30 30-degree cone, this does the damage
    DeathsJourney3 = 35872, // Helper->self, 6.5s cast, range 30 30-degree cone, visual
}

class OldMagicArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20, 22.5f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OldMagic && Module.Arena.Bounds == D133Durante.StartingBounds)
            _aoe = new(donut, Module.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0A)
        {
            Module.Arena.Bounds = D133Durante.DefaultBounds;
            _aoe = null;
        }
    }
}

class OldMagic(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OldMagic));
class ArcaneEdge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ArcaneEdge));
class Contrapasso(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Contrapasso));

class DuplicitousBattery(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(5);
    private const int MaxInitialAOEs = 16;
    private const int MaxAOEs = 32;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var clampedCount = Math.Min(_aoes.Count, MaxAOEs);
        for (var i = 0; i < clampedCount; ++i)
        {
            var isRisky = i < MaxInitialAOEs;
            yield return new AOEInstance(circle, _aoes[i].Origin, default, _aoes[i].Activation, isRisky ? Colors.Danger : Colors.AOE, isRisky);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DuplicitousBatteryTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, WorldState.FutureTime(6.5f)));

    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.DuplicitousBattery2)
            _aoes.RemoveAt(0);
    }
}

class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(11));
class Explosion2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion2), new AOEShapeCircle(9));
class FallenGrace(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FallenGrace), 6);
class AntipodalAssault(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.AntipodalAssaultMarker), ActionID.MakeSpell(AID.AntipodalAssault2), 5.4f);
class HardSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HardSlash), new AOEShapeCone(50, 45.Degrees()));
class TwilightPhase(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TwilightPhase2), new AOEShapeRect(30, 10, 30));
class DarkImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkImpact2), new AOEShapeCircle(25));
class DeathsJourney(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeathsJourney), new AOEShapeCircle(8));
class DeathsJourney2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeathsJourney2), new AOEShapeCone(30, 15.Degrees()));

class D133DuranteStates : StateMachineBuilder
{
    public D133DuranteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Components.StayInBounds>()
            .ActivateOnEnter<OldMagicArenaChange>()
            .ActivateOnEnter<OldMagic>()
            .ActivateOnEnter<ArcaneEdge>()
            .ActivateOnEnter<Contrapasso>()
            .ActivateOnEnter<DuplicitousBattery>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<FallenGrace>()
            .ActivateOnEnter<AntipodalAssault>()
            .ActivateOnEnter<HardSlash>()
            .ActivateOnEnter<TwilightPhase>()
            .ActivateOnEnter<DarkImpact>()
            .ActivateOnEnter<DeathsJourney>()
            .ActivateOnEnter<DeathsJourney2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12584)]
class D133Durante(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -422), StartingBounds)
{
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsCircle(22.5f);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsCircle(20);
}
