﻿namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

// note: I don't know any good way to associate kiter to orb; markers and orb actor creation can happen in arbitrary order
// so currently I do the following hack:
// - assume any created orb will eventually explode; whenever explosion counter matches kiter count, reset both
// - any existing orb that hasn't exploded yet is assumed to target kiter with smallest angular distance
class Aetheroplasm : BossComponent
{
    private BitMask _kiters;
    private HashSet<ulong> _explodedOrbs = new();

    private static readonly float _explosionRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_kiters[slot])
            hints.Add("Kite the orb!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var orb in module.Enemies(OID.Aetheroplasm).Where(a => !_explodedOrbs.Contains(a.InstanceID)))
        {
            // TODO: + line to kiter
            hints.AddForbiddenZone(ShapeDistance.Circle(orb.Position, _explosionRadius + 1));
            var kiter = MostLikelyKiter(module, orb);
            if (kiter != null && kiter != actor)
                hints.AddForbiddenZone(ShapeDistance.Rect(orb.Position, kiter.Position, 2));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _kiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in module.Enemies(OID.Aetheroplasm).Where(a => !_explodedOrbs.Contains(a.InstanceID)))
        {
            arena.Actor(orb, ArenaColor.Object, true);
            arena.AddCircle(orb.Position, _explosionRadius, ArenaColor.Danger);
            var kiter = MostLikelyKiter(module, orb);
            if (kiter != null)
                arena.AddLine(orb.Position, kiter.Position, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OrbFixate:
                _kiters.Set(Raid.FindSlot(spell.MainTargetID));
                break;
            case AID.AetheroplasmFixated:
                _explodedOrbs.Add(caster.InstanceID);
                if (_explodedOrbs.Count == _kiters.NumSetBits())
                {
                    _kiters.Reset();
                    _explodedOrbs.Clear();
                }
                break;
        }
    }

    private Actor? MostLikelyKiter(BossModule module, Actor orb)
    {
        if (_kiters.None())
            return null;
        var orbDir = orb.Rotation.ToDirection();
        return Raid.WithSlot().IncludedInMask(_kiters).MaxBy(a => (a.Item2.Position - orb.Position).Normalized().Dot(orbDir)).Item2;
    }
}
