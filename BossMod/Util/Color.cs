﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

[JsonConverter(typeof(JsonColorConverter))]
public record struct Color(uint ABGR)
{
    public readonly uint R => ABGR & 0xFF;
    public readonly uint G => (ABGR >> 8) & 0xFF;
    public readonly uint B => (ABGR >> 16) & 0xFF;
    public readonly uint A => (ABGR >> 24) & 0xFF;

    private const float ToFloat = 1.0f / 255;

    public static Color FromRGBA(uint rgba)
    {
        var a = rgba & 0xFF;
        var b = (rgba >> 8) & 0xFF;
        var g = (rgba >> 16) & 0xFF;
        var r = (rgba >> 24) & 0xFF;
        return new((a << 24) | (b << 16) | (g << 8) | r);
    }

    public static Color FromFloat4(Vector4 vec)
    {
        var r = Math.Clamp((uint)(vec.X * 255), 0, 255);
        var g = Math.Clamp((uint)(vec.Y * 255), 0, 255);
        var b = Math.Clamp((uint)(vec.Z * 255), 0, 255);
        var a = Math.Clamp((uint)(vec.W * 255), 0, 255);
        return new((a << 24) | (b << 16) | (g << 8) | r);
    }

    public readonly uint ToRGBA() => (R << 24) | (G << 16) | (B << 8) | A;
    public readonly Vector4 ToFloat4() => new Vector4(R, G, B, A) * ToFloat;
}

public class JsonColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.Length > 0 ? Color.FromRGBA(uint.Parse(str[1..], System.Globalization.NumberStyles.HexNumber)) : default;
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{value.ToRGBA():X8}");
    }
}

public static class Colors
{
    private static readonly ColorConfig _config = Service.Config.Get<ColorConfig>();

    public static uint Background => _config.ArenaBackground.ABGR;
    public static uint Border => _config.ArenaBorder.ABGR;
    public static uint AOE => _config.ArenaAOE.ABGR;
    public static uint SafeFromAOE => _config.ArenaSafeFromAOE.ABGR;
    public static uint Danger => _config.ArenaDanger.ABGR;
    public static uint Safe => _config.ArenaSafe.ABGR;
    public static uint Trap => _config.ArenaTrap.ABGR;
    public static uint PC => _config.ArenaPC.ABGR;
    public static uint Enemy => _config.ArenaEnemy.ABGR;
    public static uint Object => _config.ArenaObject.ABGR;
    public static uint PlayerInteresting => _config.ArenaPlayerInteresting.ABGR;
    public static uint PlayerGeneric => _config.ArenaPlayerGeneric.ABGR;
    public static uint Vulnerable => _config.ArenaVulnerable.ABGR;
    public static uint FutureVulnerable => _config.ArenaFutureVulnerable.ABGR;
    public static uint Shadows => _config.Shadows.ABGR;
    public static uint WaymarkA => _config.WaymarkA.ABGR;
    public static uint WaymarkB => _config.WaymarkB.ABGR;
    public static uint WaymarkC => _config.WaymarkC.ABGR;
    public static uint WaymarkD => _config.WaymarkD.ABGR;
    public static uint Waymark1 => _config.Waymark1.ABGR;
    public static uint Waymark2 => _config.Waymark2.ABGR;
    public static uint Waymark3 => _config.Waymark3.ABGR;
    public static uint Waymark4 => _config.Waymark4.ABGR;
}
