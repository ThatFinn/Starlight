namespace Starlight.Utils;

public static class StringEUtil
{
    /// <summary>
    /// Whether the given character is A-Z or a-z
    /// </summary>
    public static bool IsAsciiLetter(this char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    /// <summary>
    /// Uppercase letters derived from an FNV-1a hash of the string
    /// </summary>
    public static string ToLetterHash(this string value, int length = 4)
    {
        uint hash = 2166136261;
        foreach (var c in value)
            hash = (hash ^ c) * 16777619;

        var letters = new char[length];
        for (var i = 0; i < letters.Length; i++)
        {
            letters[i] = (char)('A' + hash % 26);
            hash /= 26;
        }

        return new string(letters);
    }

    /// <summary>
    /// The part after the last occurraence of the separator, or the whole string if there is none
    /// </summary>
    public static string AfterLast(this string value, char separator)
    {
        var index = value.LastIndexOf(separator);
        return index >= 0 ? value[(index + 1)..] : value;
    }
}