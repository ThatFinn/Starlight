using System;
using System.Linq;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// Sets color properties on a model's materials
/// </summary>
public static class MaterialColors
{
    /// <summary>
    /// Gives every renderer under <paramref name="root"/> copies of the materials that have any of the <paramref name="colors"/>
    /// </summary>
    /// <returns> How many materials were copied </returns>
    public static int ApplyToHierarchy(GameObject root, IReadOnlyDictionary<string, Color> colors)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(colors);

        var copies = new Dictionary<IntPtr, Material>();

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            var materials = renderer.sharedMaterials;
            var changed   = false;

            for (var i = 0; i < materials.Length; i++)
            {
                var material = materials[i];

                if (material == null || !colors.Keys.Any(material.HasProperty)) continue;

                if (!copies.TryGetValue(material.Pointer, out var copy))
                {
                    copy = Recolored(material, colors);
                    copies[material.Pointer] = copy;
                }

                materials[i] = copy;
                changed      = true;
            }

            if (changed) renderer.sharedMaterials = materials;
        }

        return copies.Count;
    }

    private static Material Recolored(Material source, IReadOnlyDictionary<string, Color> colors)
    {
        var copy = new Material(source)
        {
            name      = source.name + "Recolored",
            hideFlags = HideFlags.DontUnloadUnusedAsset
        };

        foreach (var (property, color) in colors)
            if (copy.HasProperty(property)) copy.SetColor(property, color);

        return copy.Hold();
    }
}