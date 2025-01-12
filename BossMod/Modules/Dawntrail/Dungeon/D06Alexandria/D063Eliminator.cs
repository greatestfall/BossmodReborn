﻿namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D063Eliminator;

public enum OID : uint
{
    Boss = 0x41CE, // R6.001
    Elimbit = 0x41D0, // R2.0
    EliminationClaw = 0x41CF, // R2.0
    LightningGenerator = 0x41D1, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    Teleport = 36763, // Boss->location, no cast, single-target
    AutoAttack = 36764, // Boss->player, no cast, single-target

    Disruption = 36765, // Boss->self, 5.0s cast, range 60 circle, raidwide

    PartitionVisual1 = 39599, // Boss->self, 6.2+0.6s cast, single-target
    PartitionVisual2 = 39600, // Boss->self, 6.2+0.6s cast, single-target
    PartitionVisual3 = 36768, // Boss->self, 4.3+0.7s cast, single-target
    PartitionVisual4 = 36766, // Boss->self, no cast, single-target
    PartitionVisual5 = 36767, // Boss->self, no cast, single-target
    Partition1 = 39007, // Helper->self, 5.0s cast, range 40 180-degree cone
    Partition2 = 39238, // Helper->self, 7.0s cast, range 40 180-degree cone
    Partition3 = 39249, // Helper->self, 7.0s cast, range 40 180-degree cone

    Subroutine1 = 36781, // Boss->self, 3.0s cast, single-target, summons adds
    Subroutine2 = 36775, // Boss->self, 3.0s cast, single-target
    Subroutine3 = 36772, // Boss->self, 3.0s cast, single-target
    SpawnClaw = 36774, // Boss->self, no cast, single-target
    SpawnElimbit = 36777, // Boss->self, no cast, single-target
    SpawnClawAndElimbit = 36788, // Boss->self, no cast, single-target

    ReconfiguredPartition1 = 39248, // Boss->self, 1.2+5.6s cast, single-target
    ReconfiguredPartition2 = 39247, // Boss->self, 1.2+5.6s cast, single-target

    TerminateVisual = 36773, // EliminationClaw->self, 6.2+0.6s cast, single-target
    Terminate = 39615, // Helper->self, 7.0s cast, range 40 width 10 rect

    HaloOfDestructionVisual = 36776, // Elimbit->self, 6.4+0.4s cast, single-target
    HaloOfDestruction = 39616, // Helper->self, 7.0s cast, range 6-40 donut

    OverexposureMarker = 36778, // Helper->player, no cast, single-target
    OverexposureVisual = 36779, // Boss->self, 4.3+0.7s cast, single-target
    Overexposure = 36780, // Helper->self, no cast, range 40 width 6 rect, line stack

    Electray = 39243, // Helper->player, 5.0s cast, range 6 circle

    HoloArk1 = 36789, // Boss->self, no cast, single-target
    HoloArk2 = 36790, // Helper->self, no cast, range 60 circle
    ChargeLimitBreakBar = 36791, // LightningGenerator->Boss, no cast, single-target

    CompressionVisual = 36792, // EliminationClaw->location, 5.3s cast, single-target
    Compression = 36793, // Helper->self, 6.0s cast, range 6 circle

    Impact = 36794, // Helper->self, 6.0s cast, range 60 circle, knockback 15, away from source

    LightOfSalvationVisual = 36782, // Elimbit->self, 6.0s cast, single-target
    LightOfSalvationMarker = 36783, // Helper->player, 5.9s cast, single-target
    LightOfSalvation = 36784, // Helper->self, no cast, range 40 width 6 rect

    LightOfDevotionVisual = 36785, // EliminationClaw->self, 5.0s cast, single-target
    LightOfDevotionMarker = 36786, // Helper->player, no cast, single-target
    LightOfDevotion = 36787, // Helper->self, no cast, range 40 width 6 rect

    Elimination1 = 36795, // Boss->self, 4.0s cast, single-target
    Elimination2 = 36796, // Helper->self, no cast, range 60 circle

    Explosion = 39239 // Helper->self, 8.5s cast, range 50 width 8 rect
}

class DisruptionArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D063Eliminator.ArenaCenter, 16)], [new Square(D063Eliminator.ArenaCenter, 15)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Disruption && Module.Arena.Bounds == D063Eliminator.StartingBounds)
            _aoe = new(square, Module.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x28)
        {
            Module.Arena.Bounds = D063Eliminator.DefaultBounds;
            _aoe = null;
        }
    }
}

class Disruption(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Disruption));
class Partition1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Partition1), new AOEShapeCone(40, 90.Degrees()));
class Partition2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Partition2), new AOEShapeCone(40, 90.Degrees()));
class Partition3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Partition3), new AOEShapeCone(40, 90.Degrees()));
class Terminate(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Terminate), new AOEShapeRect(40, 5));
class HaloOfDestruction(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HaloOfDestruction), new AOEShapeDonut(6, 40));

class Electray(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electray), 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<HaloOfDestruction>()!.ActiveAOEs(slot, actor).Any() || Module.FindComponent<Partition2>()!.ActiveAOEs(slot, actor).Any())
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
            if (ActiveSpreads.Any())
                hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center - new WDir(0, 15), 15), ActiveSpreads.First().Activation);
        }
    }
}

class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeRect(50, 4, 50));
class Impact(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Impact), 15)
{
    public (WPos, DateTime) Data;
    private static readonly Angle halfAngle = 45.Degrees();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            Data = (caster.Position, Module.CastFinishAt(spell, 0.4f));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Sources(slot, actor).Any() || Data.Item2 > Module.WorldState.CurrentTime) // 0.4s delay to wait for action effect
        {
            var activation = Data.Item2.AddSeconds(-0.4f);
            if (Data.Item1.Z == -640)
                hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(Data.Item1, 6, 8, 180.Degrees(), halfAngle), activation);
            else if (Data.Item1.Z == -656)
                hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(Data.Item1, 6, 8, default, halfAngle), activation);
        }
    }
}

class Compression(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Compression), new AOEShapeCircle(6));

class Overexposure(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.OverexposureMarker), ActionID.MakeSpell(AID.Overexposure), 5, 40, 3);
class LightOfDevotion(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.LightOfDevotionMarker), ActionID.MakeSpell(AID.LightOfDevotion), 5.5f, 40, 3)
{
    public override void Update()
    {
        // as soon as limit break phase ends the line stack gets cancelled
        if (CurrentBaits.Count > 0 && !Module.Enemies(OID.LightningGenerator).Any(x => !x.IsDead))
            CurrentBaits.Clear();
    }
}

class LightOfSalvation(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(40, 3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LightOfSalvationMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.TargetID)!, rect, Module.CastFinishAt(spell, 0.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightOfSalvation)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<Impact>()!.Sources(slot, actor).Any() || Module.FindComponent<Impact>()!.Data.Item2 > Module.WorldState.CurrentTime) // 0.4s delay to wait for action effect
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D063EliminatorStates : StateMachineBuilder
{
    public D063EliminatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DisruptionArenaChange>()
            .ActivateOnEnter<Disruption>()
            .ActivateOnEnter<Partition1>()
            .ActivateOnEnter<Partition2>()
            .ActivateOnEnter<Partition3>()
            .ActivateOnEnter<Terminate>()
            .ActivateOnEnter<HaloOfDestruction>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<Compression>()
            .ActivateOnEnter<LightOfDevotion>()
            .ActivateOnEnter<LightOfSalvation>()
            .ActivateOnEnter<Overexposure>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12729)]
public class D063Eliminator(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-759, -648);
    public static readonly ArenaBoundsSquare StartingBounds = new(15.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(15);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.LightningGenerator));
    }
}
