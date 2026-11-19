using System;
using Il2CppTMPro;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace Starlight.Utils;

public static class FontEUtil
{
    internal static void ReloadFont(StarlightPopUp popUp) => MenuEUtil.ReloadFont(popUp);
    internal static void ReloadFont(StarlightMenu menu) => MenuEUtil.ReloadFont(menu);
    public static TMP_FontAsset FontFromGame(string name)
    {
        try { return Get<TMP_FontAsset>(name); }
        catch { StarlightEntryPoint.SendFontError(name); }
        return null;
    }
    [Obsolete("Currently broken!")] public static TMP_FontAsset FontFromOS(string name)
    {
        try
        {
            string path = $"C:\\Windows\\Fonts\\{name}.ttf";
            if(!File.Exists(path)) throw new Exception();
            FontEngine.InitializeFontEngine();
            if (FontEngine.LoadFontFace(path, 90) != FontEngineError.Success) throw new Exception();
            TMP_FontAsset fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            fontAsset.m_Version = "1.1.0";
            fontAsset.faceInfo = FontEngine.GetFaceInfo();
            fontAsset.sourceFontFile = Font.CreateDynamicFontFromOSFont(name, 16);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.atlasWidth = 1024;
            fontAsset.atlasHeight = 1024;
            fontAsset.atlasPadding = 9;
            fontAsset.atlasRenderMode = GlyphRenderMode.SDFAA;
            fontAsset.atlasTextures = new Texture2D[1];
            Texture2D texture = new Texture2D(0, 0, TextureFormat.Alpha8, false);
            fontAsset.atlasTextures[0] = texture;
            fontAsset.isMultiAtlasTexturesEnabled = true;
            Material material = new Material(ShaderUtilities.ShaderRef_MobileSDF);
            material.SetTexture(ShaderUtilities.ID_MainTex, texture);
            material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
            material.SetFloat(ShaderUtilities.ID_TextureHeight, 1024);
            material.SetFloat(ShaderUtilities.ID_TextureWidth, 1024);
            material.SetFloat(ShaderUtilities.ID_GradientScale, 10);
            fontAsset.material = material;
            fontAsset.freeGlyphRects = new Il2CppSystem.Collections.Generic.List<GlyphRect>(8);
            fontAsset.freeGlyphRects.Add(new GlyphRect(0, 0, 1023, 1023));
            fontAsset.usedGlyphRects = new Il2CppSystem.Collections.Generic.List<GlyphRect>(8);
            fontAsset.ReadFontAssetDefinition();
            return fontAsset;
        }
        catch { StarlightEntryPoint.SendFontError(name); }
        return null;
    }
}