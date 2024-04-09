﻿namespace BossMod.RealmReborn.Raid.T05Twintania;

// P3 mechanics
// TODO: preposition for divebombs? it seems that boss spawns in one of the fixed spots that is closest to target...
class P3Divebomb : Components.GenericAOEs
{
    public WPos? Target { get; private set; }
    public DateTime HitAt { get; private set; }

    private static readonly AOEShapeRect _shape = new(35, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Target != null)
        {
            if (Module.PrimaryActor.CastInfo == null)
                yield return new(_shape, Module.PrimaryActor.Position, Angle.FromDirection(Target.Value - Module.PrimaryActor.Position), HitAt);
            else
                yield return new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo.Rotation, Module.PrimaryActor.CastInfo.NPCFinishAt);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.DivebombMarker:
                Target = WorldState.Actors.Find(spell.MainTargetID)?.Position;
                HitAt = WorldState.FutureTime(1.7f);
                break;
            case AID.DivebombAOE:
                Target = null;
                break;
        }
    }
}

class P3Adds : BossComponent
{
    private IReadOnlyList<Actor> _hygieia = ActorEnumeration.EmptyList;
    public IReadOnlyList<Actor> Asclepius { get; private set; } = ActorEnumeration.EmptyList;
    public IEnumerable<Actor> ActiveHygieia => _hygieia.Where(a => !a.IsDead);

    private const float _explosionRadius = 8;

    public override void Init(BossModule module)
    {
        _hygieia = module.Enemies(OID.Hygieia);
        Asclepius = module.Enemies(OID.Asclepius);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var nextHygieia = ActiveHygieia.MinBy(a => a.InstanceID); // select next add to kill by lowest hp
        var asclepiusVuln = Asclepius.FirstOrDefault()?.FindStatus(SID.Disseminate);
        bool killHygieia = asclepiusVuln == null || (asclepiusVuln.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10;
        foreach (var e in hints.PotentialTargets)
        {
            switch ((OID)e.Actor.OID)
            {
                case OID.Hygieia:
                    var predictedHP = e.Actor.HP.Cur + WorldState.PendingEffects.PendingHPDifference(e.Actor.InstanceID);
                    e.Priority = e.Actor.HP.Cur == 1 ? 0
                        : killHygieia && e.Actor == nextHygieia ? 2
                        : predictedHP < 0.3f * e.Actor.HP.Max ? -1
                        : 1;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                    bool gtfo = predictedHP <= (e.ShouldBeTanked ? 1 : 0.1f * e.Actor.HP.Max);
                    if (gtfo)
                        hints.AddForbiddenZone(ShapeDistance.Circle(e.Actor.Position, 9));
                    break;
                case OID.Asclepius:
                    e.Priority = 1;
                    e.AttackStrength = 0.15f;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.MT;
                    break;
            }
        }

        if (!Module.PrimaryActor.IsTargetable && !ActiveHygieia.Any() && !Asclepius.Any(a => !a.IsDead))
        {
            // once all adds are dead, gather where boss will return
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(-6.67f, 5), 5), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var a in ActiveHygieia)
        {
            arena.Actor(a, ArenaColor.Enemy);
            arena.AddCircle(a.Position, _explosionRadius, ArenaColor.Danger);
        }
        foreach (var a in Asclepius)
            arena.Actor(a, ArenaColor.Enemy);
    }
}

class P3AethericProfusion : Components.CastCounter
{
    private DateTime _activation;

    public P3AethericProfusion() : base(ActionID.MakeSpell(AID.AethericProfusion)) { }

    public override void Init(BossModule module)
    {
        _activation = WorldState.FutureTime(6.7f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // select neurolinks to stand at; let everyone except MT stay in one closer to boss
        var neurolinks = module.Enemies(OID.Neurolink);
        var closerNeurolink = neurolinks.Closest(Module.PrimaryActor.Position);
        foreach (var neurolink in neurolinks)
        {
            bool isClosest = neurolink == closerNeurolink;
            bool stayAtClosest = assignment != PartyRolesConfig.Assignment.MT;
            if (isClosest == stayAtClosest)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(neurolink.Position, T05Twintania.NeurolinkRadius), _activation);
        }

        // let MT taunt boss if needed
        var boss = hints.PotentialTargets.Find(e => e.Actor == Module.PrimaryActor);
        if (boss != null)
            boss.PreferProvoking = true;

        // mitigate heavy raidwide
        hints.PredictedDamage.Add((Raid.WithSlot().Mask(), _activation));
        if (actor.Role == Role.Ranged)
            hints.PlannedActions.Add((ActionID.MakeSpell(BLM.AID.Addle), Module.PrimaryActor, (float)(_activation - WorldState.CurrentTime).TotalSeconds, false));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var neurolink in module.Enemies(OID.Neurolink))
            arena.AddCircle(neurolink.Position, T05Twintania.NeurolinkRadius, ArenaColor.Safe);
    }
}
