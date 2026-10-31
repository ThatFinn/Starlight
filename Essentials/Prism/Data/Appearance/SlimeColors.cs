using System;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// The color set that defines how a slime looks
/// </summary>
/// <param name="Top"> Top-body tone </param>
/// <param name="Middle"> Mid-body tone </param>
/// <param name="Bottom"> Lower-body tone </param>
/// <param name="Specular"> Highlight tint; Defaults to Middle if not provided </param>
public readonly record struct SlimePalette(Color Top, Color Middle, Color Bottom, Color Specular)
{
    public SlimePalette(Color top, Color middle, Color bottom) : this(top, middle, bottom, middle) { }
}

/// <summary>
/// Applies a <see cref="SlimePalette"/> to a slime's appearance
/// </summary>
public static class SlimeColors
{
    private const string TopColorProperty      = "_TopColor";
    private const string MiddleColorProperty   = "_MiddleColor";
    private const string BottomColorProperty   = "_BottomColor";
    private const string SpecularColorProperty = "_SpecColor";
    
    private static readonly string[] ColorSetPrefixes = { "", "_Red", "_Green", "_Blue", "_Twin" };

    /// <summary>
    /// The colors halfway between two appearance palettes
    /// </summary>
    public static SlimePalette Blend(SlimeAppearance.Palette first, SlimeAppearance.Palette second) => new(
        Color.Lerp(first.Top, second.Top, 0.5f),
        Color.Lerp(first.Middle, second.Middle, 0.5f),
        Color.Lerp(first.Bottom, second.Bottom, 0.5f));

    /// <summary>
    /// Recolors every structure of the appearance
    /// </summary>
    public static void Apply(SlimeAppearance appearance, SlimePalette palette)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        foreach (var structure in appearance.Structures.AsEnumerable())
            ApplyToMaterials(structure?.DefaultMaterials, palette);

        appearance._colorPalette = new SlimeAppearance.Palette
        {
            Top    = palette.Top,
            Middle = palette.Middle,
            Bottom = palette.Bottom,
            Ammo   = palette.Middle
        };
        appearance._splatColor = palette.Middle;
    }

    /// <summary>
    /// Applies a palette to a single material
    /// </summary>
    /// <returns> How many color properties the shader had </returns>
    public static int Apply(Material material, SlimePalette palette)
    {
        ArgumentNullException.ThrowIfNull(material);

        var written = 0;

        foreach (var prefix in ColorSetPrefixes)
        {
            var top = prefix.Length == 0 ? TopColorProperty : prefix + "TopColor";
            var middle = prefix.Length == 0 ? MiddleColorProperty : prefix + "MiddleColor";
            var bottom = prefix.Length == 0 ? BottomColorProperty : prefix + "BottomColor";

            if (SetColor(material, top, palette.Top)) written++;
            if (SetColor(material, middle, palette.Middle)) written++;
            if (SetColor(material, bottom, palette.Bottom)) written++;
        }

        if (SetColor(material, SpecularColorProperty, palette.Specular)) written++;
        if (SetColor(material, "_TwinSpecColor", palette.Specular)) written++;

        return written;
    }

    /// <summary>
    /// Tints a material that paints with artwork rather than a gradient
    /// </summary>
    /// <returns> How many color properties the shader had </returns>
    public static int Tint(Material material, Color color)
    {
        ArgumentNullException.ThrowIfNull(material);

        var written = 0;

        foreach (var property in TintProperties)
            if (SetColor(material, property, color)) written++;

        return written;
    }

    /// <summary>
    /// The single-color slots an artwork shader multiplies by
    /// </summary>
    private static readonly string[] TintProperties = { "_BaseColor", "_Color" };

    /// <summary>
    /// Colors an eye material
    /// </summary>
    public static void ApplyEyes(Material material, Color red, Color green, Color blue)
    {
        ArgumentNullException.ThrowIfNull(material);

        var wrote = SetColor(material, "_EyeRed", red);
        SetColor(material, "_EyeGreen", green);
        SetColor(material, "_EyeBlue", blue);
        
        SetColor(material, "_EyeTwinRed", red);
        SetColor(material, "_EyeTwinGreen", green);
        SetColor(material, "_EyeTwinBlue", blue);

        if (wrote) return;
        
        ApplyMaskChannels(material, red, green, blue);
    }

    /// <summary>
    /// Colors a mouth material
    /// </summary>
    public static void ApplyMouth(Material material, SlimePalette palette)
    {
        ArgumentNullException.ThrowIfNull(material);

        var wrote = SetColor(material, "_MouthTop", palette.Top);
        SetColor(material, "_MouthMid", palette.Middle);
        SetColor(material, "_MouthBot", palette.Bottom);

        SetColor(material, "_MouthTopTwin", palette.Top);
        SetColor(material, "_MouthMidTwin", palette.Middle);
        SetColor(material, "_MouthBotTwin", palette.Bottom);

        if (!wrote) Apply(material, palette);
    }

    /// <summary>
    /// Colors a material by mask channel
    /// </summary>
    public static void ApplyMaskChannels(Material material, Color red, Color green, Color blue)
    {
        ArgumentNullException.ThrowIfNull(material);

        foreach (var (prefix, colour) in new[] { ("_Red", red), ("_Green", green), ("_Blue", blue) })
        {
            SetColor(material, prefix + "TopColor", colour);
            SetColor(material, prefix + "MiddleColor", colour);
            SetColor(material, prefix + "BottomColor", colour);
        }
    }

    private static void ApplyToMaterials(
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material>? materials,
        SlimePalette palette)
    {
        foreach (var material in materials.AsEnumerable())
        {
            if (material == null) continue;
            Apply(material, palette);
        }
    }

    /// <summary>
    /// Sets a color property if the shader declares it
    /// </summary>
    /// <returns> True if it was written </returns>
    private static bool SetColor(Material material, string property, Color color)
    {
        if (!material.HasProperty(property)) return false;

        material.SetColor(property, color);
        return true;
    }
}