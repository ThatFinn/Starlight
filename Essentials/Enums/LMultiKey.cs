namespace Starlight.Enums;

public struct LMultiKey
{
    internal readonly LKey[] keys;

    /// <summary>
    /// Constructor: pass any number of keys in the combination
    /// </summary>
    public LMultiKey(List<LKey> keys)
    {
        this.keys = keys.ToArray();
    }
    public LMultiKey(params LKey[] keys)
    {
        this.keys = keys;
    }
}