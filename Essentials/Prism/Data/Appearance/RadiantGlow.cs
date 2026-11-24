using System;
using System.Linq;
using Starlight.Prism.Data.Native;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// Gives a radiant appearance the glow of the native radiant slimes, or removes it
/// </summary>
public static class RadiantGlow
{
    private const string AuraPiece  = "RadiantAuraVFX";
    private const string LightPiece = "RadiantSphereLight";

    private const string AuraShader = "SR/AMP/VFX/RadiantAura";

    private const string StrongAura = "RadiantAura_strong";
    private const string LightAura  = "RadiantAura_light";

    private const string EffectKeyword = "_ENABLERADIANTEFFECT_ON";

    private static readonly int EffectProperty = Shader.PropertyToID("_EnableRadiantEffect");
    private static readonly int LightColor     = Shader.PropertyToID("_Color");

    private static readonly int[] EffectTextures =
    {
        Shader.PropertyToID("_RadiantNoisePattern"),
        Shader.PropertyToID("_RadiantParallaxPattern"),
        Shader.PropertyToID("_RadiantFresnelGradient"),
        Shader.PropertyToID("_ColorShiftGradient")
    };

    private static Material? strongAura;
    private static Material? lightAura;

    /// <summary>
    /// Whether the appearance glows
    /// </summary>
    public static bool Has(SlimeAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        return appearance.Structures.AsEnumerable().Any(structure => IsPiece(structure, AuraPiece));
    }

    /// <summary>
    /// Makes the appearance glow like a native radiant slime
    /// </summary>
    /// <param name="appearance"> The radiant appearance </param>
    /// <param name="lightColor"> The color of the light sphere around the slime </param>
    /// <returns> False if the native radiant to copy the glow from is not loaded </returns>
    public static bool Add(SlimeAppearance appearance, Color lightColor)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        var template = PrismNativeBaseSlime.Pink.Find()?.RadiantBase;
        var shimmer  = template?.Structures.AsEnumerable()
            .SelectMany(structure => structure?.DefaultMaterials.AsEnumerable() ?? Enumerable.Empty<Material>())
            .FirstOrDefault(material => material != null && material.HasProperty(EffectProperty) && material.GetFloat(EffectProperty) > 0f);

        if (template == null || shimmer == null) return false;

        foreach (var structure in appearance.Structures.AsEnumerable())
        {
            if (structure == null || structure.SupportsFaces || IsPiece(structure)) continue;

            if (!Shimmer(structure.DefaultMaterials, shimmer)) continue;

            var aura = IsBody(structure) ? StrongAuraMaterial() : LightAuraMaterial();
            if (aura == null) continue;

            structure.DefaultMaterials = WithAura(structure.DefaultMaterials, aura);

            foreach (var entry in structure.ElementMaterials.AsEnumerable())
            {
                if (entry is not { OverrideDefaults: true }) continue;

                Shimmer(entry.Materials, shimmer);
                entry.Materials = WithAura(entry.Materials, aura);
            }
        }

        foreach (var piece in new[] { AuraPiece, LightPiece })
        {
            if (appearance.Structures.AsEnumerable().Any(structure => IsPiece(structure, piece))) continue;

            var source = template.Structures.AsEnumerable().FirstOrDefault(structure => IsPiece(structure, piece));
            if (source != null) appearance.Structures = appearance.Structures.AddToNew(AppearanceCloner.CloneStructure(source));
        }

        lightColor.a = 1f;
        foreach (var structure in appearance.Structures.AsEnumerable().Where(structure => IsPiece(structure, LightPiece)))
            foreach (var material in structure.DefaultMaterials.AsEnumerable())
                if (material != null && material.HasProperty(LightColor)) material.SetColor(LightColor, lightColor);

        return true;
    }

    /// <summary>
    /// Removes the glow from the appearance
    /// </summary>
    public static void Remove(SlimeAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        appearance.Structures = appearance.Structures.AsEnumerable()
            .Where(structure => structure != null && !IsPiece(structure))
            .ToIl2CppArray();

        foreach (var structure in appearance.Structures.AsEnumerable())
        {
            structure.DefaultMaterials = WithoutGlow(structure.DefaultMaterials);

            foreach (var entry in structure.ElementMaterials.AsEnumerable())
                if (entry != null) entry.Materials = WithoutGlow(entry.Materials);
        }
    }

    /// <summary>
    /// Turns on the radiant shimmer of the slime materials
    /// </summary>
    /// <returns> True if any of the materials can shimmer </returns>
    private static bool Shimmer(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material>? materials, Material shimmer)
    {
        var any = false;

        foreach (var material in materials.AsEnumerable())
        {
            if (material == null || !material.HasProperty(EffectProperty)) continue;

            material.SetFloat(EffectProperty, 1f);
            material.EnableKeyword(EffectKeyword);

            foreach (var texture in EffectTextures)
                if (material.HasProperty(texture) && material.GetTexture(texture) == null)
                    material.SetTexture(texture, shimmer.GetTexture(texture));

            any = true;
        }

        return any;
    }

    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material> WithAura(
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material>? materials,
        Material aura)
    {
        var list = materials.AsEnumerable().Where(material => material != null).ToList();
        if (!list.Any(IsAura)) list.Add(aura);

        return list.ToIl2CppArray();
    }

    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material> WithoutGlow(
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material>? materials)
    {
        var list = materials.AsEnumerable().Where(material => material != null && !IsAura(material)).ToList();

        foreach (var material in list.Where(material => material.HasProperty(EffectProperty)))
        {
            material.SetFloat(EffectProperty, 0f);
            material.DisableKeyword(EffectKeyword);
        }

        return list.ToIl2CppArray();
    }

    private static bool IsAura(Material material) => material.shader != null && material.shader.name == AuraShader;

    private static bool IsBody(SlimeAppearanceStructure structure) =>
        structure.Element != null && structure.Element.Type == SlimeAppearanceElement.ElementType.BODY;

    private static bool IsPiece(SlimeAppearanceStructure? structure) => IsPiece(structure, AuraPiece) || IsPiece(structure, LightPiece);

    private static bool IsPiece(SlimeAppearanceStructure? structure, string piece) =>
        structure?.Element != null && structure.Element.name.Contains(piece, StringComparison.Ordinal);

    private static Material? StrongAuraMaterial() => strongAura != null ? strongAura : strongAura = Get<Material>(StrongAura);

    private static Material? LightAuraMaterial() => lightAura != null ? lightAura : lightAura = Get<Material>(LightAura) ?? StrongAuraMaterial();
}