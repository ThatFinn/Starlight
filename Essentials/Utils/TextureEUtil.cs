using System;

namespace Starlight.Utils;

public static class TextureEUtil
{
    /// <summary>
    /// Average color of a texture's visible pixels
    /// </summary>
    /// <returns>
    /// The average color, or null if the texture can't be read
    /// </returns>
    public static Color? AverageColor(this Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        RenderTexture? temporary = null;
        var previousActive = RenderTexture.active;

        try
        {
            temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, temporary);

            RenderTexture.active = temporary;
            var readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, mipChain: false);
            readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            readable.Apply();

            return Average(readable.GetPixels32());
        }
        catch
        {
            return null;
        }
        finally
        {
            RenderTexture.active = previousActive;
            if (temporary != null) RenderTexture.ReleaseTemporary(temporary);
        }
    }

    /// <summary>
    /// Averages opaque pixels by sampling
    /// </summary>
    private static Color? Average(Color32[] pixels)
    {
        if (pixels.Length == 0) return null;

        const int targetSamples = 2000;
        const byte minimumAlpha = 10;

        var step = Math.Max(1, pixels.Length / targetSamples);
        long r = 0, g = 0, b = 0;
        var counted = 0;

        for (var i = 0; i < pixels.Length; i += step)
        {
            var pixel = pixels[i];
            
            if (pixel.a < minimumAlpha) continue;

            r += pixel.r;
            g += pixel.g;
            b += pixel.b;
            counted++;
        }

        if (counted == 0) return null;
        return new Color(r / (float)counted / 255f, g / (float)counted / 255f, b / (float)counted / 255f);
    }
}