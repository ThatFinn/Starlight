using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Starlight.Utils;

public static class HttpEUtil
{
    private static readonly Dictionary<Image,string> OnGoingImages = new ();
    private static readonly Dictionary<RawImage,string> OnGoingRawImages = new ();

    private static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        if (newWidth <= 0 || newHeight <= 0) return source;
        var rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        var newTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        newTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTexture.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return newTexture;
    }
    public static void DownloadTexture2DAsync(string url, Action<Texture2D, string> onComplete)
    {
        StartCoroutine(_DownloadTexture2DCoroutine(url, onComplete));
    }

    public static void DownloadTexture2DIntoImageAsync(string url, Image image, bool useCache = false, int resizeX = -1, int resizeY = -1)
    {
        OnGoingImages[image]=url;
        var cachePath = Path.Combine(StarlightEntryPoint.tmpDataPath, "downloadcache."+Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(url)))+".png");
        if (useCache)
            try { image.sprite = ConvertEUtil.BytesToTexture2D(File.ReadAllBytes(cachePath)).Texture2DToSprite(); } catch { }

        StartCoroutine(_DownloadTexture2DCoroutine(url, ((texture, error) =>
        {
            if(OnGoingImages.ContainsKey(image))
                if (OnGoingImages[image] == url)
                {
                    OnGoingImages.Remove(image);
                    if (error == null && texture != null&&image!=null)
                    {
                        image.sprite = texture.Texture2DToSprite();
                        if (useCache)
                            File.WriteAllBytes(cachePath,ConvertEUtil.Texture2DToBytesPNG(ResizeTexture(texture,resizeX,resizeY)));
                    }
                }
        })));
    }
    public static void DownloadTexture2DIntoRawImageAsync(string url, RawImage image, bool useCache = false, int resizeX = -1, int resizeY = -1)
    {
        OnGoingRawImages[image]=url;
        var cachePath = Path.Combine(StarlightEntryPoint.tmpDataPath, "downloadcache."+Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(url)))+".png");
        if (useCache)
            try { image.texture = ConvertEUtil.BytesToTexture2D(File.ReadAllBytes(cachePath)); } catch { }

        StartCoroutine(_DownloadTexture2DCoroutine(url, ((texture, error) =>
        {
            if(OnGoingRawImages.ContainsKey(image))
                if (OnGoingRawImages[image] == url)
                {
                    OnGoingRawImages.Remove(image);
                    if (image != null)
                    {
                        if (error == null && texture != null)
                            image.texture = texture;
                        if (useCache)
                            File.WriteAllBytes(cachePath,ConvertEUtil.Texture2DToBytesPNG(ResizeTexture(texture,resizeX,resizeY)));
                    }
                }
        })));
    }
    private static IEnumerator _DownloadTexture2DCoroutine(string url, Action<Texture2D, string> onComplete)
    {
        byte[] imageBytes = null;
        string error = null;

        Task downloadTask = Task.Run(async () =>
        {
            try
            {
                HttpClient client = new HttpClient();
                imageBytes = await client.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        });

        while (!downloadTask.IsCompleted)
            yield return null;

        if (!string.IsNullOrEmpty(error) || imageBytes == null)
        {
            onComplete?.Invoke(null, error ?? "Unknown error while downloading image");
            yield break;
        }

        Texture2D texture = new Texture2D(2, 2);
        bool loaded = false;
        try { loaded = Il2CppImageConversionManager.LoadImage(texture, imageBytes, false); }
        catch (Exception e) { LogError(e); }

        if (!loaded)
        {
            error = "Failed to load image data into texture.";
            onComplete?.Invoke(null, error);
            yield break;
        }

        onComplete?.Invoke(texture, null);
    }
}