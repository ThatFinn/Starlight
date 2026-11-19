using Il2CppMonomiPark.SlimeRancher;

namespace Starlight.Utils;

public static class ConvertEUtil
{
    public static ISaveReferenceTranslation ToIVariant(this SaveReferenceTranslation saveReferenceTranslation) => saveReferenceTranslation.TryCast<ISaveReferenceTranslation>();
    public static SaveReferenceTranslation ToNonIVariant(this ISaveReferenceTranslation iSaveReferenceTranslation) => iSaveReferenceTranslation.TryCast<SaveReferenceTranslation>();
    public static ILoadReferenceTranslation ToIVariant(this LoadReferenceTranslation saveReferenceTranslation) => saveReferenceTranslation.TryCast<ILoadReferenceTranslation>();
    public static LoadReferenceTranslation ToNonIVariant(this ILoadReferenceTranslation saveReferenceTranslation) => saveReferenceTranslation.TryCast<LoadReferenceTranslation>();

    public static Sprite Texture2DToSprite(this Texture2D texture)
    {
        if (!texture) return null;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }
    public static Texture2D Base64ToTexture2D(string base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        byte[] bytes = System.Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
        try { if (Il2CppImageConversionManager.LoadImage(texture, bytes,false)) return texture; }
        catch (Exception e) { LogError(e); }
        return null;
    }
    public static Texture2D BytesToTexture2D(byte[] bytes)
    {
        if (bytes==null) return null;
        Texture2D texture = new Texture2D(2, 2);
        try { if (Il2CppImageConversionManager.LoadImage(texture, bytes,false)) return texture; }
        catch (Exception e) { LogError(e); }
        return null;
    }
    public static byte[] Texture2DToBytesPNG(Texture2D texture)
    {
        if (texture==null) return null;
        return Il2CppImageConversionManager.EncodeToPNG(texture);
    }
    public static byte[] Texture2DToBytesJPG(Texture2D texture)
    {
        if (texture==null) return null;
        return Il2CppImageConversionManager.EncodeToJPG(texture);
    }
    public static byte[] Texture2DToBytesTGA(Texture2D texture)
    {
        if (texture==null) return null;
        return Il2CppImageConversionManager.EncodeToTGA(texture);
    }
    public static string Texture2DToBase64PNG(Texture2D texture)
    {
        if (texture==null) return null;
        return System.Convert.ToBase64String(Il2CppImageConversionManager.EncodeToPNG(texture));
    }
    public static string Texture2DToBase64JPG(Texture2D texture)
    {
        if (texture==null) return null;
        return System.Convert.ToBase64String(Il2CppImageConversionManager.EncodeToJPG(texture));
    }
    public static string Texture2DToBase64TGA(Texture2D texture)
    {
        if (texture==null) return null;
        return System.Convert.ToBase64String(Il2CppImageConversionManager.EncodeToTGA(texture));
    }
}