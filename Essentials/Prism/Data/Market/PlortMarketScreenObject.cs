using System;
using System.Linq;
using UObject = UnityEngine.Object;

namespace Starlight.Prism.Data.Market;

/// <summary>
/// Adds another market screen
/// </summary>
public static class PlortMarketScreenObject
{
    private const string DecorPath =
        "zoneConservatory/cellConservatory/Sector/Ranch Features/techMarket (1)/decor";

    private const string ScreenName = "market_screen_upper";

    private static readonly Vector3 ScreenPosition = new(573.4398f, 23.4f, 337.0001f);
    private static readonly Vector3 ScreenRotation = new(0f, 270f, 0f);
    
    private static readonly (string Path, Vector3 Position)[] Objects =
    {
        ($"{DecorPath}/market_screen/market_back/market_curve (1)", new Vector3(573.78f, 19.6f, 343.0001f)),
        ($"{DecorPath}/Arcs/arch01", new Vector3(573.1699f, 19.6f, 337.0001f))
    };

    /// <summary>
    /// Upper screen offset from the original screen
    /// </summary>
    public static Vector3? UpperScreenOffset { get; private set; }
    
    private static GameObject? placed;

    /// <summary>
    /// Rearranges the screen if it is not arranged correctly
    /// </summary>
    /// <returns> True once the screen is rearranged correctly </returns>
    public static bool Apply()
    {
        if (placed != null) return true;

        placed = null;
        UpperScreenOffset = null;

        if (!StarlightEntryPoint.prismSecondMarketScreen) return false;
        
        var source = Find($"{DecorPath}/market_screen");
        if (source == null) return false;

        foreach (var (path, position) in Objects)
        {
            var target = Find(path);
            if (target == null)
                continue;

            target.transform.position = position;
        }

        var copy = UObject.Instantiate(source, source.transform.parent);
        copy.name = ScreenName;
        copy.transform.position = ScreenPosition;
        copy.transform.eulerAngles = ScreenRotation;
        copy.transform.localScale = source.transform.localScale;
        
        var back = FindChild(copy.transform, "market_back");
        if (back != null) back.gameObject.SetActive(false);

        placed = copy;
        UpperScreenOffset = ScreenPosition - source.transform.position;

        return true;
    }

    /// <summary>
    /// Hides or shows the extra screen
    /// </summary>
    public static void SetVisible(bool visible)
    {
        if (placed != null) placed.SetActive(visible);
    }

    private static GameObject? Find(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var root = UObject.FindObjectsOfType<GameObject>().FirstOrDefault(candidate =>
            candidate.transform.parent == null &&
            string.Equals(candidate.name.Trim(), segments[0].Trim(), StringComparison.Ordinal));

        if (root == null) return null;

        var current = root.transform;

        foreach (var segment in segments.Skip(1))
        {
            current = FindChild(current, segment);
            if (current == null) return null;
        }

        return current.gameObject;
    }

    private static Transform? FindChild(Transform parent, string name)
    {
        var wanted = name.Trim();

        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (string.Equals(child.name.Trim(), wanted, StringComparison.Ordinal)) return child;
        }

        return null;
    }
}