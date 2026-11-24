namespace Starlight.Utils;

/// <summary>
/// Moves colors from one color to another, keeping the shading
/// </summary>
public sealed class ColorShift
{
    private const float MinSaturation = 0.12f;

    private readonly float fromHue;
    private readonly float fromSaturation;
    private readonly float fromValue;
    private readonly float toHue;
    private readonly float toSaturation;
    private readonly float toValue;

    /// <summary>
    /// Shifts <paramref name="from"/> and the colors around it to <paramref name="to"/>
    /// </summary>
    public ColorShift(Color from, Color to)
    {
        From = from;
        To   = to;

        Color.RGBToHSV(from, out fromHue, out fromSaturation, out fromValue);
        Color.RGBToHSV(to, out toHue, out toSaturation, out toValue);
    }

    /// <summary>
    /// The color being replaced
    /// </summary>
    public Color From { get; }

    /// <summary>
    /// The color it becomes
    /// </summary>
    public Color To { get; }

    /// <summary>
    /// How far a hue may be from <see cref="From"/>'s to be shifted
    /// </summary>
    /// <remarks> 0.5 shifts every hue </remarks>
    public float HueRange { get; init; } = 0.5f;

    /// <summary>
    /// Whether saturation and brightness follow <see cref="To"/> too, which dark, pale and grey colors need
    /// </summary>
    public bool MatchTone { get; init; }

    /// <summary>
    /// How far past <see cref="HueRange"/> hues still blend part of the way to <see cref="To"/>
    /// </summary>
    /// <remarks>
    /// Anti-aliased edges between a shifted and a kept color have hues in between;
    /// without a feather they keep their old color and show as a "rim".
    /// With one, hues in the outer half of <see cref="HueRange"/> keep less of their offset,
    /// so edges towards the kept color don't overshoot past <see cref="To"/>
    /// </remarks>
    public float Feather { get; init; }

    /// <summary>
    /// Whether anything would change
    /// </summary>
    public bool Changes =>
        Mathf.Abs(fromHue - toHue) >= 0.01f
        || Mathf.Abs(fromSaturation - toSaturation) >= 0.05f
        || (MatchTone && Mathf.Abs(fromValue - toValue) >= 0.02f);

    private float SaturationScale => fromSaturation > 0.05f
        ? Mathf.Clamp(toSaturation / fromSaturation, MatchTone ? 0f : 0.2f, 2.5f)
        : 1f;

    private float ValueScale => MatchTone && fromValue > 0.05f ? toValue / fromValue : 1f;

    /// <summary>
    /// The shifted color, or <paramref name="color"/> if it is out of range
    /// </summary>
    public Color Apply(Color color)
    {
        Color.RGBToHSV(color, out var hue, out var saturation, out var value);

        if (saturation < MinSaturation) return color;

        var offset   = Mathf.Repeat(hue - fromHue + 0.5f, 1f) - 0.5f;
        var distance = Mathf.Abs(offset);

        if (distance > HueRange + Feather) return color;

        var inside = distance <= HueRange;

        // The feather blends towards the plain new hue; carrying its offset over would tint the edge,
        // so with a feather the offset also fades out over the outer half of the range to meet it there
        var carried = !inside ? 0f
            : Feather > 0f ? offset * Mathf.Clamp01((HueRange - distance) / (HueRange * 0.5f))
            : offset;

        var shifted = Color.HSVToRGB(
            Mathf.Repeat(toHue + carried, 1f),
            Mathf.Clamp01(saturation * SaturationScale),
            Mathf.Clamp01(value * ValueScale));

        shifted.a = color.a;
        if (inside) return shifted;

        return Color.Lerp(color, shifted, 1f - (distance - HueRange) / Feather);
    }
}
