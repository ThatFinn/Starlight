using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Starlight.Utils;

public static class EmbeddedResourceEUtil
{
    static Assembly ToAssembly(MethodBase method)
    {
        if (method != null&&method.ReflectedType!=null)
        {
            return method.ReflectedType.Assembly;
        }
        return null;
    }
    public static Sprite LoadSprite(string fileName)
    {
        return LoadSprite(fileName,ToAssembly(new StackTrace().GetFrame(1)?.GetMethod()));
    }
    public static Sprite LoadSprite(string fileName, Assembly assembly) => LoadTexture2D(fileName,assembly).Texture2DToSprite();
    
    public static Texture2D LoadTexture2D(string fileName)
    {
        return LoadTexture2D(fileName, ToAssembly(new StackTrace().GetFrame(1)?.GetMethod()));
    }
    public static Texture2D LoadTexture2D(string filename, Assembly assembly)
    {
        if (assembly == null) return null;
        if (!(filename.Replace("/",".").EndsWith(".png") || filename.Replace("/",".").EndsWith(".jpg") || filename.Replace("/",".").EndsWith(".exr"))) return null;
        
        var stream = assembly.GetManifestResourceStream(GetPrefix(assembly) + "." + filename.Replace("/","."));
        if (stream != null)
        {
            byte[] array = new byte[stream.Length];
            var unused=stream.Read(array, 0, array.Length);

            var texture2D = new Texture2D(1, 1);
            try { Il2CppImageConversionManager.LoadImage(texture2D, array); }
            catch (Exception e) { LogError(e); return null; }
            texture2D.hideFlags = HideFlags.DontUnloadUnusedAsset;
            texture2D.filterMode = FilterMode.Bilinear;
            
            return texture2D;
        }

        return null;
    }

    public static Dictionary<string, byte[]> LoadResources(string folderNamespace, bool recursive = false)
    {
        var method = new StackTrace().GetFrame(1)?.GetMethod();
        if (method != null&&method.ReflectedType!=null)
        {
            var assembly = method.ReflectedType.Assembly;
            var baseNamespace = GetPrefix(assembly) + (string.IsNullOrEmpty(folderNamespace.Replace("/",".")) ? "":"." + folderNamespace.Replace("/","."));

            var resourceNames = assembly.GetManifestResourceNames().Where(r =>
                    recursive
                        ? r.StartsWith(baseNamespace + ".")
                        : r.Substring(0, r.LastIndexOf('.')).Equals(baseNamespace))
                .ToArray();
            var result = new Dictionary<string, byte[]>();

            foreach (var resourceName in resourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;
                byte[] bytes = new byte[stream.Length];
                var unused = stream.Read(bytes, 0, bytes.Length);

                string fileName = resourceName.Substring(baseNamespace.Length + 1);
                result[fileName] = bytes;
            }

            return result;
        }

        return null;
    }

    public static byte[] LoadResource(string filename)
    {
        var method = new StackTrace().GetFrame(1)?.GetMethod();
        if (method != null&&method.ReflectedType!=null)
        {
            var assembly = method.ReflectedType.Assembly;
            return LoadResource(filename.Replace("/","."), assembly);
        }

        return null;
    }
    public static byte[] LoadResource(string filename, Assembly assembly)
    {
        if(assembly == null) return null;
        var stream = assembly.GetManifestResourceStream(GetPrefix(assembly) + "." + filename.Replace("/","."));
        if (stream != null)
        {
            byte[] array = new byte[stream.Length];
            var unused = stream.Read(array, 0, array.Length);
            return array;
        }

        return null;
    }
    
    public static string LoadString(string filename)
    {
        var method = new StackTrace().GetFrame(1)?.GetMethod();
        if (method != null&&method.ReflectedType!=null)
        {
            var assembly = method.ReflectedType.Assembly;
            return LoadString(filename.Replace("/","."), assembly);
        }
        return null;
    }
    public static string LoadString(string filename, Assembly assembly)
    {
        if(assembly == null) return null;
        var stream = assembly.GetManifestResourceStream(GetPrefix(assembly) + "." + filename.Replace("/","."));
        if (stream != null)
        {
            byte[] array = new byte[stream.Length];
            var unused = stream.Read(array, 0, array.Length);
            return System.Text.Encoding.Default.GetString(array);
        }

        return null;
    }
    
    public static Il2CppAssetBundle LoadIl2CppBundle(string filename)
    {
        var method = new StackTrace().GetFrame(1)?.GetMethod();
        if (method != null&&method.ReflectedType!=null)
        {
            var assembly = method.ReflectedType.Assembly;
            return LoadIl2CppBundle(filename.Replace("/","."), assembly);
        }

        return null;
    }
    public static Il2CppAssetBundle LoadIl2CppBundle(string filename, Assembly assembly)
    {
        if(assembly == null) return null;
        var stream = assembly.GetManifestResourceStream(GetPrefix(assembly) + "." + filename.Replace("/","."));
        if (stream != null)
        {
            byte[] array = new byte[stream.Length];
            var unused = stream.Read(array, 0, array.Length);
            return Il2CppAssetBundleManager.LoadFromMemory(array);
        }

        return null;
    }
    
    public static AssetBundle LoadBundle(string filename)
    {
        var method = new StackTrace().GetFrame(1)?.GetMethod();
        if (method != null&&method.ReflectedType!=null)
        {
            var assembly = method.ReflectedType.Assembly;
            return LoadBundle(filename.Replace("/","."), assembly);
        }

        return null;
    }

    static string GetPrefix(Assembly assembly) => assembly.GetManifestResourceNames().FirstOrDefault()?.Split('.')[0];
    
    public static AssetBundle LoadBundle(string filename, Assembly assembly)
    {
        if(assembly == null) return null;
        var stream = assembly.GetManifestResourceStream(GetPrefix(assembly) + "." + filename.Replace("/","."));
        if (stream != null)
        {
            byte[] array = new byte[stream.Length];
            var unused = stream.Read(array, 0, array.Length);

            return AssetBundle.LoadFromMemory(array);
        }

        return null;
    }
}