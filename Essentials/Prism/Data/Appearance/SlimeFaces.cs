using System;
using UObject = UnityEngine.Object;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Patches;

namespace Starlight.Prism.Data.Appearance;

/// <summary> One custom facial expression </summary>
/// <param name="Expression"> The expression being overridden </param>
/// <param name="Eyes"> Eye texture, or null to keep the base slime's eyes </param>
/// <param name="Mouth"> Mouth texture, or null to keep the base slime's mouth </param>
public readonly record struct FaceOverride(SlimeFace.SlimeExpression Expression, Texture2D? Eyes, Texture2D? Mouth);

/// <summary> Colors for a face </summary>
/// <param name="EyeRed"> Color for the eye mask's red region </param>
/// <param name="EyeGreen"> Color for the eye mask's green region </param>
/// <param name="EyeBlue"> Color for the eye mask's blue region </param>
/// <param name="Mouth"> Gradient for the mouth </param>
public readonly record struct FaceColors(Color EyeRed, Color EyeGreen, Color EyeBlue, SlimePalette Mouth);

/// <summary>
/// How a face differs from the look it started from
/// </summary>
public sealed record FaceLook
{
    /// <summary>
    /// The native slime's copy
    /// </summary>
    public PrismNativeBaseSlime? CopyFrom { get; init; }

    /// <summary>
    /// Recoloring every expression of the face
    /// </summary>
    public FaceColors? Colours { get; init; }

    /// <summary>
    /// Artwork for individual expressions
    /// </summary>
    public IReadOnlyCollection<FaceOverride> Overrides { get; init; } = Array.Empty<FaceOverride>();
}

/// <summary>
/// Custom slime faces
/// </summary>
public static class SlimeFaces
{
    /// <summary>
    /// Applies custom expression artwork to a cloned appearance's face
    /// </summary>
    /// <returns> The number of expressions changed </returns>
    public static int Apply(SlimeAppearance appearance, IReadOnlyCollection<FaceOverride> overrides)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        if (overrides.Count == 0) return 0;

        var face = appearance._face;
        if (face == null)
        {
            LogWarning("Prism: Can't apply custom faces: the appearance has no face");
            return 0;
        }
        
        var byExpression = new Dictionary<SlimeFace.SlimeExpression, FaceOverride>();
        foreach (var replacement in overrides)
            byExpression[replacement.Expression] = replacement;

        var applied = 0;

        for (var i = 0; i < FaceExpressionTable.Count(face); i++)
        {
            if (!FaceExpressionTable.TryGet(face, i, out var existing)) break;
            if (!byExpression.TryGetValue(existing.Expression, out var replacement)) continue;

            if (FaceExpressionTable.TrySetMaterials(
                    face, i,
                    CloneWithTexture(existing.Eyes, replacement.Eyes),
                    CloneWithTexture(existing.Mouth, replacement.Mouth)))
                applied++;
        }
        
        face.UpdateCache();

        return applied;
    }

    /// <summary> Recolors a cloned appearance's face </summary>
    /// <param name="eyeRed"> Color for the eye mask's red region </param>
    /// <param name="eyeGreen"> Color for the eye mask's green region </param>
    /// <param name="eyeBlue"> Color for the eye mask's blue region </param>
    /// <param name="mouth"> Gradient for the mouth </param>
    /// <returns> The number of expressions recolored </returns>
    public static int Recolour(
        SlimeAppearance appearance,
        Color eyeRed,
        Color eyeGreen,
        Color eyeBlue,
        SlimePalette mouth)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        var face = appearance._face;
        if (face == null)
        {
            LogWarning("Prism: Can't recolour the face: the appearance has none.");
            return 0;
        }

        var recoloured = 0;

        for (var i = 0; i < FaceExpressionTable.Count(face); i++)
        {
            if (!FaceExpressionTable.TryGet(face, i, out var existing)) break;

            if (FaceExpressionTable.TrySetMaterials(
                    face, i,
                    TintChannels(existing.Eyes, eyeRed, eyeGreen, eyeBlue),
                    TintGradient(existing.Mouth, mouth)))
                recoloured++;
        }
        
        face.UpdateCache();

        return recoloured;
    }

    /// <summary>
    /// Replaces a cloned appearance's face with a copy of another's
    /// </summary>
    /// <returns> True if a face was copied </returns>
    public static bool CopyFaceFrom(SlimeAppearance appearance, SlimeAppearance? source)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        if (source?._face == null) return false;
        
        appearance._face = UObject.Instantiate(source._face);
        appearance._face.hideFlags = HideFlags.DontUnloadUnusedAsset;
        appearance._face.UpdateCache();

        return true;
    }

    /// <summary>
    /// Returns a copy of a face material with its mask channels filled in
    /// </summary>
    private static Material? TintChannels(Material? source, Color red, Color green, Color blue)
    {
        var material = CopyOf(source);
        if (material == null) return null;

        SlimeColors.ApplyEyes(material, red, green, blue);
        return material;
    }

    /// <summary>
    /// Returns a copy of a face material tinted with a gradient
    /// </summary>
    private static Material? TintGradient(Material? source, SlimePalette palette)
    {
        var material = CopyOf(source);
        if (material == null) return null;

        SlimeColors.ApplyMouth(material, palette);
        return material;
    }

    private static Material? CopyOf(Material? source)
    {
        if (source == null) return null;

        var material = UObject.Instantiate(source);
        material.hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        FaceMaterialOwnership.Claim(material);

        return material;
    }

    /// <summary>
    /// Returns a copy of the material with the texture applied, or the original if there's no texture
    /// </summary>
    private static Material? CloneWithTexture(Material? source, Texture2D? texture)
    {
        if (source == null) return null;
        if (texture == null) return source;

        var material = UObject.Instantiate(source);
        material.hideFlags = HideFlags.DontUnloadUnusedAsset;
        FaceMaterialOwnership.Claim(material);

        MaterialTextures.ApplyToAllTextureSlots(material, texture);
        return material;
    }

    /// <summary>
    /// Maps an expression name to the game's expression enum, accepting a few alternative spellings
    /// </summary>
    public static SlimeFace.SlimeExpression? ParseExpression(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        
        var normalized = name.Trim().ToUpperInvariant() switch
        {
            "SLEEP" or "SLEEPING" => "ASLEEP",
            "EAT" or "EATING" => "CHOMP_OPEN",
            "SCARE" => "SCARED",
            _ => name.Trim().ToUpperInvariant()
        };

        return Enum.TryParse<SlimeFace.SlimeExpression>(normalized, ignoreCase: true, out var expression)
            ? expression
            : null;
    }
}