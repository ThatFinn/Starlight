using System;
using System.IO;
using System.IO.Compression;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Starlight.Utils;

/// <summary>
/// Reads a PNG into a <see cref="Texture2D"/> without going through Unity's decoder.
/// </summary>
public static class PngTexture
{
    /// <summary>
    /// Reads a PNG
    /// </summary>
    /// <param name="png"> The file's bytes. </param>
    /// <param name="failure"> Why it could not be read, when the result is null. </param>
    public static Texture2D Load(byte[] png, out string failure)
    {
        failure = null;

        try
        {
            if (png == null || png.Length < 8)
            {
                failure = "empty PNG";
                return null;
            }

            if (png[0] != 0x89 || png[1] != 'P' || png[2] != 'N' || png[3] != 'G')
            {
                failure = "not a PNG";
                return null;
            }

            var width = 0;
            var height = 0;
            var bitDepth = 0;
            var colourType = 0;
            var interlace = 0;

            using var compressed = new MemoryStream();

            var at = 8;

            while (at + 8 <= png.Length)
            {
                var length = ReadInt(png, at);
                var type = System.Text.Encoding.ASCII.GetString(png, at + 4, 4);
                var body = at + 8;

                if (length < 0 || body + length > png.Length)
                {
                    failure = "truncated";
                    return null;
                }

                switch (type)
                {
                    case "IHDR":
                        width      = ReadInt(png, body);
                        height     = ReadInt(png, body + 4);
                        bitDepth   = png[body + 8];
                        colourType = png[body + 9];
                        interlace  = png[body + 12];
                        break;

                    case "IDAT":
                        compressed.Write(png, body, length);
                        break;
                }

                if (type == "IEND") break;

                at = body + length + 4;
            }
            
            if (bitDepth != 8 || interlace != 0 || (colourType != 2 && colourType != 6))
            {
                failure = $"unsupported PNG (depth {bitDepth}, colour type {colourType}, interlace {interlace})";
                return null;
            }

            if (width <= 0 || height <= 0)
            {
                failure = "no dimensions";
                return null;
            }

            var channels = colourType == 6 ? 4 : 3;
            var stride = width * channels;

            compressed.Position = 0;
            
            using var inflate = new ZLibStream(compressed, CompressionMode.Decompress);

            var raw = new byte[(stride + 1) * height];
            var read = 0;

            while (read < raw.Length)
            {
                var got = inflate.Read(raw, read, raw.Length - read);
                if (got <= 0) break;

                read += got;
            }

            if (read < raw.Length)
            {
                failure = $"short pixel data ({read} of {raw.Length})";
                return null;
            }

            var pixels = new Il2CppStructArray<Color32>(width * height);

            Unfilter(raw, pixels, width, height, channels, stride);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            return texture;
        }
        catch (Exception exception)
        {
            failure = exception.Message;
            return null;
        }
    }

    /// <summary>
    /// Reverses PNG's per-scanline filtering and writes the result out as pixels.
    /// </summary>
    private static void Unfilter(byte[] raw, Il2CppStructArray<Color32> pixels, int width, int height, int channels, int stride)
    {
        var previous = new byte[stride];
        var line = new byte[stride];
        var at = 0;

        for (var y = 0; y < height; y++)
        {
            var filter = raw[at++];

            Buffer.BlockCopy(raw, at, line, 0, stride);
            at += stride;

            for (var x = 0; x < stride; x++)
            {
                int left = x >= channels ? line[x - channels] : 0;
                int up = previous[x];
                int upLeft = x >= channels ? previous[x - channels] : 0;

                line[x] = filter switch
                {
                    1 => (byte)(line[x] + left),
                    2 => (byte)(line[x] + up),
                    3 => (byte)(line[x] + ((left + up) >> 1)),
                    4 => (byte)(line[x] + Paeth(left, up, upLeft)),
                    _ => line[x]
                };
            }
            
            var row = (height - 1 - y) * width;

            for (var x = 0; x < width; x++)
            {
                var source = x * channels;

                pixels[row + x] = new Color32(
                    line[source],
                    line[source + 1],
                    line[source + 2],
                    channels == 4 ? line[source + 3] : (byte)255);
            }

            (previous, line) = (line, previous);
        }
    }

    /// <summary>
    /// The Paeth predictor
    /// </summary>
    private static int Paeth(int a, int b, int c)
    {
        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);

        if (pa <= pb && pa <= pc) return a;

        return pb <= pc ? b : c;
    }

    /// <summary>
    /// PNG stores its integers big-endian
    /// </summary>
    private static int ReadInt(byte[] data, int at)
        => (data[at] << 24) | (data[at + 1] << 16) | (data[at + 2] << 8) | data[at + 3];
}