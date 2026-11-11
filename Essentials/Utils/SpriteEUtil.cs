namespace Starlight.Utils;

/// <summary>
/// Sprites made from code;
/// Recolouring & keeping it alive
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
    public static Sprite? Recolour(this Sprite? source, Color from, Color to)
    {
        if (source == null) return null;

        Color.RGBToHSV(from, out var fromHue, out var fromSaturation, out _);
        Color.RGBToHSV(to, out var toHue, out var toSaturation, out _);
        
        if (Mathf.Abs(fromHue - toHue) < 0.01f && Mathf.Abs(fromSaturation - toSaturation) < 0.05f) return source;

        RenderTexture? target = null;
        var previous = RenderTexture.active;

        try
        {
            var texture = source.texture;
            var area = source.textureRect;

            target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, target);
            RenderTexture.active = target;

            var copy = new Texture2D((int) area.width, (int) area.height, TextureFormat.RGBA32, false) { hideFlags = HideFlags.DontUnloadUnusedAsset };
            copy.ReadPixels(new Rect(area.x, area.y, area.width, area.height), 0, 0);

            var pixels = copy.GetPixels();
            var saturationScale = fromSaturation > 0.05f ? Mathf.Clamp(toSaturation / fromSaturation, 0.2f, 2.5f) : 1f;

            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                if (pixel.a < 0.01f) continue;

                Color.RGBToHSV(pixel, out var hue, out var saturation, out var value);
                
                if (saturation < 0.12f) continue;

                hue = Mathf.Repeat(hue + (toHue - fromHue), 1f);
                saturation = Mathf.Clamp01(saturation * saturationScale);

                var shifted = Color.HSVToRGB(hue, saturation, value);
                shifted.a = pixel.a;
                pixels[i] = shifted;
            }

            copy.SetPixels(pixels);
            copy.Apply();

            copy.Hold();

            var sprite = Sprite.Create(copy, new Rect(0, 0, copy.width, copy.height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
            sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
            sprite.name = source.name + "Recoloured";
            return sprite.Hold();
        }
        catch (Exception exception)
        {
            LogWarning($"Prism: Couldn't recolour the icon '{source.name}': {exception.Message}");
            return source;
        }
        finally
        {
            RenderTexture.active = previous;
            if (target != null) RenderTexture.ReleaseTemporary(target);
        }
    }
}