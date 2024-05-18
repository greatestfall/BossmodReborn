﻿namespace BossMod.Endwalker.Variant.V02MR.V022Moko;

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 32 'rest' casts
class Upwell(BossModule module) : Components.GenericAOEs(module)
{
    private class LineSequence
    {
        public WPos NextOrigin;
        public WDir Advance;
        public Angle Rotation;
        public DateTime NextActivation;
        public AOEShapeRect? NextShape; // wide for first line, null for first line mirror, narrow for remaining lines
    }

    private readonly List<LineSequence> _lines = [];

    private static readonly AOEShapeRect _shapeWide = new(30, 5, 30);
    private static readonly AOEShapeRect _shapeNarrow = new(30, 2.5f, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: think about imminent/future color/risk, esp for overlapping lines
        var imminentDeadline = WorldState.FutureTime(5);
        foreach (var l in _lines)
            if (l.NextShape != null && l.NextActivation <= imminentDeadline)
                yield return new(l.NextShape, l.NextOrigin, l.Rotation, l.NextActivation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.UpwellFirst)
        {
            var advance = spell.Rotation.ToDirection().OrthoR() * 5;
            _lines.Add(new() { NextOrigin = caster.Position, Advance = advance, Rotation = spell.Rotation, NextActivation = spell.NPCFinishAt, NextShape = _shapeWide });
            _lines.Add(new() { NextOrigin = caster.Position, Advance = -advance, Rotation = (spell.Rotation + 180.Degrees()).Normalized(), NextActivation = spell.NPCFinishAt });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.UpwellFirst)
        {
            ++NumCasts;
            var index = _lines.FindIndex(l => l.NextOrigin.AlmostEqual(caster.Position, 1) && l.NextShape == _shapeWide && l.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index < 0 || index + 1 >= _lines.Count)
            {
                ReportError($"Unexpected exaline end");
            }
            else
            {
                Advance(_lines[index]);
                Advance(_lines[index + 1]);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.UpwellRest)
        {
            ++NumCasts;
            var index = _lines.FindIndex(l => l.NextOrigin.AlmostEqual(caster.Position, 1) && l.NextShape == _shapeNarrow && l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (index < 0)
                ReportError($"Unexpected exaline @ {caster.Position} / {caster.Rotation}");
            else
                Advance(_lines[index]);
        }
    }

    private void Advance(LineSequence line)
    {
        line.NextOrigin += line.Advance;
        line.NextActivation = WorldState.FutureTime(2);
        var offset = (line.NextOrigin - Module.Bounds.Center).Abs();
        line.NextShape = offset.X < 19 && offset.Z < 19 ? _shapeNarrow : null;
    }
}