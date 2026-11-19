using System.Security.Cryptography;
using System.Text;

namespace Starlight.Utils;

public static class EncodingEUtil
{
    public static string CreateMD5(this string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.Unicode.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        var sb = new StringBuilder();
        foreach (var t in hashBytes)
            sb.Append(t.ToString("x2"));
        return sb.ToString();
    }
    public static string EncodeToBase128(this byte[] data)
    {
        var result = new StringBuilder((data.Length * 8 + 6) / 7);
        var storage = 0;
        var bits = 0;

        foreach (var b in data)
        {
            storage = (storage << 8) | b;
            bits += 8;
            while (bits >= 7)
            {
                bits -= 7;
                result.Append((char)((storage >> bits) & 0x7F));
            }
        }
        if (bits > 0)
            result.Append((char)((storage << (7 - bits)) & 0x7F));
        return result.ToString();
    }

    public static byte[] DecodeFromBase128(this string data)
    {
        var result = new List<byte>();
        var storage = 0;
        var bits = 0;

        foreach (char c in data)
        {
            storage = (storage << 7) | (c & 0x7F);
            bits += 7;
            if (bits >= 8)
            {
                bits -= 8;
                result.Add((byte)((storage >> bits) & 0xFF));
            }
        }
        return result.ToArray();
    }
}