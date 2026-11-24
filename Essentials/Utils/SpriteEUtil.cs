namespace Starlight.Utils;

/// <summary>
/// Sprites made from code;
/// Recoloring & keeping it alive
/// </summary>
public static class SpriteEUtil
{
    private static readonly List<Object> Held = new();

    /// <summary>
    /// Holds on to a texture, sprite or mesh made from code, for as long as the game runs.
    /// </summary>
    public static T Hold<T>(this T value) where T : Object
    {
        if (value == null) return value;

        Held.Add(value);
        Prism.Patches.FaceMaterialOwnership.Claim(value);
        return value;
    }

    /// <summary>
    /// Holds a sprite and it's texturre
    /// </summary>
    public static Sprite? Hold(this Sprite? sprite)
    {
        if (sprite == null) return null;

        Hold<Object>(sprite);

        var texture = sprite.texture;
        if (texture != null) Hold<Object>(texture);

        return sprite;
    }

    /// <summary>
    /// Recolors a game's icon
    /// </summary>
    public static Sprite? Recolor(this Sprite? source, Color from, Color to) => source.Recolor(new ColorShift(from, to));

    /// <summary>
    /// Recolors a game's icon with a <see cref="ColorShift"/>
    /// </summary>
    public static Sprite? Recolor(this Sprite? source, ColorShift shift)
    {
        if (source == null) return null;

        if (!shift.Changes) return source;

        RenderTexture? target = null;
        var previous = RenderTexture.active;

        try
        {
            var texture = source.texture;

            var full    = source.rect;
            var area    = source.textureRect;
            var offset  = source.textureRectOffset;
            var width   = Mathf.RoundToInt(full.width);
            var height  = Mathf.RoundToInt(full.height);

            target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, target);
            RenderTexture.active = target;

            var copy = new Texture2D(width, height, TextureFormat.RGBA32, texture.mipmapCount > 1)
            {
                hideFlags  = HideFlags.DontUnloadUnusedAsset,
                filterMode = texture.filterMode,
                anisoLevel = texture.anisoLevel,
                wrapMode   = TextureWrapMode.Clamp
            };
            copy.SetPixels32(new Color32[width * height]);
            copy.ReadPixels(new Rect(area.x, area.y, area.width, area.height), Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y));

            var pixels = copy.GetPixels();

            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a >= 0.01f) pixels[i] = shift.Apply(pixels[i]);
            }

            copy.SetPixels(pixels);
            copy.Apply(true);

            copy.Hold();

            var pivot  = new Vector2(source.pivot.x / full.width, source.pivot.y / full.height);
            var sprite = Sprite.Create(copy, new Rect(0, 0, width, height), pivot, source.pixelsPerUnit);
            sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
            sprite.name = source.name + "Recolored";
            return sprite.Hold();
        }
        catch (Exception exception)
        {
            LogWarning($"Prism: Couldn't recolor the icon '{source.name}': {exception.Message}");
            return source;
        }
        finally
        {
            RenderTexture.active = previous;
            if (target != null) RenderTexture.ReleaseTemporary(target);
        }
    }
}