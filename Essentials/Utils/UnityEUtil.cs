using System.Linq;

namespace Starlight.Utils;

public static class UnityEUtil
{
    public static T GetObjectRecursively<T>(this GameObject obj, string name) where T : class
    {
        var transform = obj.transform;

        var totalChildren = GetAllChildren(transform);
        foreach (var child in totalChildren)
            if (child.name == name)
            {
                if (typeof(T) == typeof(GameObject)) return child as T;
                if (typeof(T) == typeof(Transform)) return child.transform as T;
                if (child.GetComponent<T>() != null) return child.GetComponent<T>();
            }

        return null;
    }

    public static T GetObjectRecursively<T>(this Transform transform, string name) where T : class => GetObjectRecursively<T>(transform.gameObject, name);
    public static List<Transform> GetChildren(this Transform obj)
    {
        var children = new List<Transform>();
        for (int i = 0; i < obj.childCount; i++)
            children.Add(obj.GetChild(i));
        return children;
    }
    public static List<GameObject> GetChildren(this GameObject obj)
    {
        var children = new List<GameObject>();
        for (int i = 0; i < obj.transform.childCount; i++)
            children.Add(obj.transform.GetChild(i).gameObject);
        return children;
    }
    public static void DestroyAllChildren(this Transform obj)
    {
        for (int i = 0; i < obj.childCount; i++)
            Object.Destroy(obj.GetChild(i).gameObject);
    }

    public static void DestroyAllChildren(this GameObject obj) => obj.transform.DestroyAllChildren();
    public static void DestroyImmediateAllChildren(this Transform obj)
    {
        for (int i = 0; i < obj.childCount; i++)
            Object.DestroyImmediate(obj.GetChild(i).gameObject);
    }

    public static void DestroyImmediateAllChildren(this GameObject obj) => obj.transform.DestroyImmediateAllChildren();
    
    public static List<GameObject> GetAllChildren(this GameObject obj)
    {
        var container = obj.transform;
        var allChildren = new List<GameObject>();
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            allChildren.Add(child.gameObject);
            allChildren.AddRange(GetAllChildren(child));
        }

        return allChildren;
    }

    public static List<GameObject> GetAllChildren(this Transform container) => container.gameObject.GetAllChildren();

    public static T[] GetAllChildrenOfType<T>(this GameObject obj) where T : Component
    {
        List<T> children = new List<T>();
        foreach (var child in obj.GetAllChildren())
        {
            if (child.GetComponent<T>() != null)
            {
                children.Add(child.GetComponent<T>());
            }
        }

        return children.ToArray();
    }

    public static T[] GetAllChildrenOfType<T>(this Transform obj) where T : Component => GetAllChildrenOfType<T>(obj.gameObject);
    
    public static T? Get<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(x => x.name == name);
    public static T? GetAny<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
    public static T? GetOrAny<T>(string preferredName) where T : Object => Get<T>(preferredName) ?? GetAny<T>();
    public static List<T> GetAll<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>().ToList();
    public static List<T> GetAll<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().Where(x => x.name == name).ToList();
    
    public static T? GetInScene<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(x =>
    {
        if (!x) return false;
        GameObject obj = null;
        if (x.TryCast<Component>()) obj = x.TryCast<Component>()?.gameObject;
        else if (x.TryCast<GameObject>()) obj = x.TryCast<GameObject>();
        if (!obj || !obj.scene.IsValid() || !obj.scene.isLoaded) return false;
        return obj.name == name;
    });
    public static T? GetAnyInScene<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(x =>
    {
        if (!x) return false;
        GameObject obj = null;
        if (x.TryCast<Component>()) obj = x.TryCast<Component>()?.gameObject;
        else if (x.TryCast<GameObject>()) obj = x.TryCast<GameObject>();
        return obj && obj.scene.IsValid() && obj.scene.isLoaded;
    });
    public static List<T> GetAllInScene<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>().Where(x =>
    {
        if (!x) return false;
        GameObject obj = null;
        if (x.TryCast<Component>()) obj = x.TryCast<Component>()?.gameObject;
        else if (x.TryCast<GameObject>()) obj = x.TryCast<GameObject>();
        return obj && obj.scene.IsValid() && obj.scene.isLoaded;
    }).ToList();
    public static List<T> GetAllInScene<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().Where(x =>
    {
        if (!x) return false;
        GameObject obj = null;
        if (x.TryCast<Component>()) obj = x.TryCast<Component>()?.gameObject;
        else if (x.TryCast<GameObject>()) obj = x.TryCast<GameObject>();
        return obj && obj.name == name && obj.scene.IsValid() && obj.scene.isLoaded;
    }).ToList();
    
    public static T AddComponent<T>(this Component obj) where T : Component => obj.gameObject.AddComponent<T>();
    public static T AddComponent<T>(this Transform obj) where T : Component => obj.gameObject.AddComponent<T>();
    public static bool AddComponent(this Transform obj, Il2CppSystem.Type componentType) => obj.gameObject.AddComponent(componentType);
    public static bool AddComponent(this Transform obj, System.Type componentType) => obj.gameObject.AddComponent(componentType.IL2CPPTypeof());
    public static bool AddComponent(this GameObject obj, System.Type componentType) => obj.AddComponent(componentType.IL2CPPTypeof());
    
    public static bool HasComponent<T>(this Transform obj) where T : Component => HasComponent<T>(obj.gameObject);
    public static bool HasComponent<T>(this GameObject obj) where T : Component
    {
        try { return obj.GetComponent<T>()!=null; } catch { return false; }
    }
    public static bool RemoveComponent<T>(this Transform obj) where T : Component => RemoveComponent<T>(obj.gameObject);
    public static bool RemoveComponent<T>(this GameObject obj) where T : Component
    {
        try
        {
            var comp = obj.GetComponent<T>();
            // Used to throw if comp is invalid
            var unused = comp.gameObject;
            Object.Destroy(comp);
            return true;
        }
        catch { return false; }
    }
    
    public static GameObject CopyObject(this GameObject obj) => Object.Instantiate(obj, PrefabHolder.transform);

    public static void MakePrefab(this GameObject obj)
    {
        Object.DontDestroyOnLoad(obj);
        obj.transform.parent = PrefabHolder.transform;
    }
    public static GameObject CreatePrefab(string name, GameObject obj)
    {
        var copy = obj.CopyObject();
        Object.DontDestroyOnLoad(copy);
        
        copy.name = name;
        copy.transform.parent = PrefabHolder.transform;

        return copy;
    }
    public static GameObject CreatePrefab(GameObject obj)
    {
        var copy = obj.CopyObject();
        Object.DontDestroyOnLoad(copy);
        
        copy.transform.parent = PrefabHolder.transform;

        return copy;
    }
}