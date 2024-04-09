﻿namespace BossMod.RealmReborn.Extreme.Ex3Titan;

// TODO: most of what's here should be handled by KnockbackFromCastTarget component...
class Upheaval : Components.Knockback
{
    private DateTime _remainInPosition;

    public Upheaval() : base(ActionID.MakeSpell(AID.Upheaval)) { }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_remainInPosition > WorldState.CurrentTime)
            yield return new(Module.PrimaryActor.Position, 13);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_remainInPosition > WorldState.CurrentTime)
        {
            // stack just behind boss, this is a good place to bait imminent landslide correctly
            var dirToCenter = (Module.Bounds.Center - Module.PrimaryActor.Position).Normalized();
            var pos = Module.PrimaryActor.Position + 2 * dirToCenter;
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, 1.5f), _remainInPosition);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _remainInPosition = spell.NPCFinishAt.AddSeconds(1); // TODO: just wait for effectresult instead...
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _remainInPosition = WorldState.FutureTime(1); // TODO: just wait for effectresult instead...
    }
}
