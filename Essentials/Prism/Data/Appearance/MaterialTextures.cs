using System;
using UnityEngine.Rendering;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// Puts a texture onto a material without knowing its shader
/// </summary>
public static class MaterialTextures
{
    /// <summary>
    /// The artwork a material carries, whichever slot it is in
    /// </summary>
    public static Texture2D? FirstTextureOf(Material material)
    {
        ArgumentNullException.ThrowIfNull(material);

        if (material.mainTexture is Texture2D main) return main;

        var shader = material.shader;
        if (shader != null)
        {
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; i < propertyCount; i++)
            {
                if (shader.GetPropertyType(i) != ShaderPropertyType.Texture) continue;

                if (Read(material, shader.GetPropertyName(i)) is { } declared) return declared;
            }
        }
        
        foreach (var name in new[] { "_PrimaryTex", "_MainTex", "_BaseMap", "_BaseColorMap", "_Diffuse" })
        {
            if (Read(material, name) is { } known) return known;
        }

        return null;
    }

    private static Texture2D? Read(Material material, string property)
    {
        try
        {
            return material.HasProperty(property) ? material.GetTexture(property) as Texture2D : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Assigns the texture to every texture property the material's shader declares
    /// </summary>
    /// <returns> The property names that were set </returns>
    public static IReadOnlyList<string> ApplyToAllTextureSlots(Material material, Texture texture)
    {
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(texture);

        var applied = new List<string>();

        var shader = material.shader;
        if (shader == null) return applied;

        var propertyCount = shader.GetPropertyCount();
        for (var i = 0; i < propertyCount; i++)
        {
            if (shader.GetPropertyType(i) != ShaderPropertyType.Texture) continue;

            var name = shader.GetPropertyName(i);
            try
            {
                material.SetTexture(name, texture);
                applied.Add(name);
            }
            catch
            {
                // Some slots reject assignment, such as cubemaps
            }
        }
        return applied;
    }

    private static bool IsEye(Renderer renderer, Material material) =>
        renderer.name.Contains("eye", StringComparison.OrdinalIgnoreCase)
        || material.name.Contains("eye", StringComparison.OrdinalIgnoreCase)
        || (material.shader != null && material.shader.name.Contains("eye", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Applies a texture to every renderer </summary>
    /// <returns> The number of material slots textured </returns>
    public static int ApplyToHierarchy(GameObject root, Texture texture)
    {
        ArgumentNullException.ThrowIfNull(root);

        var slots = 0;
        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
        {
            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;
                
                if (IsEye(renderer, materials[i])) continue;

                ApplyToAllTextureSlots(materials[i], texture);
                slots++;
            }
        }
        return slots;
    }
}