using Starlight.Prism.Data.Native;

namespace Starlight.Prism.Wrappers;

public class PrismBaseSlime : PrismSlime
{
    public Sprite GetIcon() => GetSlimeAppearance()._icon;
    public Sprite GetRadiantIcon() => TryGetSlimeAppearanceRadiant()._icon;
    internal bool AllowLargos;
    internal bool DisableAutoModdedLargos;
    internal int NonNativeBagSize = 1500;
    public static implicit operator PrismBaseSlime(PrismNativeBaseSlime nativeBaseSlime)
    {
        return nativeBaseSlime.GetPrismBaseSlime();
    }

    internal Material GetBaseMaterial()
    {
        try
        {
            foreach (var structure in GetSlimeAppearance()._structures)
                try
                {
                    if (structure.Element.Type == SlimeAppearanceElement.ElementType.BODY)
                        return structure.DefaultMaterials[0];
                } catch { }
        } catch { }

        return null;
    }
    internal PrismBaseSlime(SlimeDefinition slimeDefinition, bool isNative): base(slimeDefinition, isNative)
    {
        this.SlimeDefinition = slimeDefinition;
        this.IsNative = isNative;
        if (SlimeDefinition.CanLargofy)
            AllowLargos = true;
        if (!isNative)
            DisableAutoModdedLargos = true;
    }
}