using System;
using System.Linq;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// Makes materials from outside bundles that were not built for it render in the game
/// </summary>
public static class ShaderRemap
{
    private static readonly (string Prefix, string[] Candidates)[] Mappings =
    {
        // Transparency would be lost on a lit replacement
        ("Unlit/Transparent", new[] { "Universal Render Pipeline/Unlit", "HDRP/Unlit", "Unlit/Transparent" }),

        // Opaque geometry maps to a lit shader
        ("Unlit/Texture", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit", "Standard" }),
        ("Unlit/Color", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit", "Standard" }),
        ("Unlit", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit", "Standard" }),

        ("Particles/Additive", new[] { "Universal Render Pipeline/Particles/Unlit", "HDRP/Unlit" }),
        ("Particles/Alpha Blended", new[] { "Universal Render Pipeline/Particles/Unlit", "HDRP/Unlit" }),
        ("Particles", new[] { "Universal Render Pipeline/Particles/Unlit", "HDRP/Unlit" }),

        ("Standard", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit", "Standard" }),
        ("Legacy Shaders/Diffuse", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit" }),
        ("Legacy Shaders", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit" }),
        ("Mobile", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit" }),
        ("Diffuse", new[] { "Universal Render Pipeline/Lit", "HDRP/Lit" })
    };

    /// <summary>
    /// Shaders resolved this session
    /// </summary>
    private static readonly Dictionary<string, Shader?> Resolved = new(StringComparer.Ordinal);

    /// <summary>
    /// Whether a shader is one that the game can't use
    /// </summary>
    public static bool IsBroken(Shader? shader) =>
        shader == null ||
        shader.name.Contains("InternalErrorShader", StringComparison.OrdinalIgnoreCase) ||
        shader.name.Contains("Hidden/InternalError", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Repairs a material whose shader the game doesn't have
    /// </summary>
    /// <returns> True if the material can render afterward </returns>
    public static bool Repair(Material? material)
    {
        if (material == null) return false;

        var original = material.shader;
        if (!IsBroken(original) && Find(original.name) != null) return true;

        var replacement = FindReplacementFor(original?.name);
        if (replacement == null) return false;
        
        var mainTexture = material.mainTexture;
        var mainColor = material.HasProperty("_Color") ? material.color : (Color?)null;

        material.shader = replacement;

        if (mainTexture != null) ApplyArtwork(material, mainTexture);
        if (mainColor.HasValue && material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", mainColor.Value);
        if (mainColor.HasValue && material.HasProperty("_Color")) material.SetColor("_Color", mainColor.Value);

        return true;
    }

    /// <summary>
    /// Slots that show artwork
    /// </summary>
    private static readonly string[] ArtworkProperties = { "_BaseColorMap", "_BaseMap", "_MainTex", "_Albedo" };

    /// <summary>
    /// Puts the artwork where the new shader displays it
    /// </summary>
    private static void ApplyArtwork(Material material, Texture texture)
    {
        var slot = ArtworkProperties.FirstOrDefault(material.HasProperty);

        if (slot == null)
        {
            MaterialTextures.ApplyToAllTextureSlots(material, texture);
            return;
        }

        material.SetTexture(slot, texture);
    }

    /// <summary>
    /// Repairs every material on every renderer under an object
    /// </summary>
    /// <returns> The number of materials repaired </returns>
    public static int RepairHierarchy(GameObject root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var repaired = 0;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            var materials = renderer.materials;
            foreach (var material in materials)
                if (Repair(material)) repaired++;
            renderer.materials = materials;
        }

        return repaired;
    }

    /// <summary>
    /// Finds a replacement shader
    /// </summary>
    private static Shader? FindReplacementFor(string? shaderName)
    {
        if (string.IsNullOrEmpty(shaderName)) return Fallback();

        foreach (var (prefix, candidates) in Mappings)
        {
            if (!shaderName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var candidate in candidates)
            {
                var shader = Find(candidate);
                if (shader != null) return shader;
            }
        }

        return Fallback();
    }

    /// <summary>
    /// The shader used when nothing better matches
    /// </summary>
    private static Shader? Fallback() =>
        Find("Universal Render Pipeline/Lit") ?? Find("HDRP/Lit") ?? Find("Standard");

    private static Shader? Find(string name)
    {
        if (Resolved.TryGetValue(name, out var cached)) return cached;

        var shader = Shader.Find(name);
        if (IsBroken(shader)) shader = null;

        Resolved[name] = shader;
        return shader;
    }
}