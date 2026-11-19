using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.Rendering;

namespace Starlight.Managers;

public static class StarlightVolumeProfileManager
{
    internal static readonly Dictionary<string,VolumeProfile> Presets = new();
    private static Volume _volumeHolder;

    /// <summary>
    /// Save a volume profile into an XML to load it later<br />
    /// Returns the XML as a byte array to be stored in a file
    /// </summary>
    /// <param name="profile">The profile to be saved</param>
    /// <returns>A byte array of the saved profile as XML</returns>
    public static byte[] SaveProfile(VolumeProfile profile)
    {
        var data = new VolumeProfileData();

        foreach (var comp in profile.components)
        {
            var compData = new VolumeProfileData.ComponentData
            {
                typeName = comp.GetIl2CppType().AssemblyQualifiedName,
                jsonData = JsonUtility.ToJson(comp)
            };
            data.components.Add(compData);
        }

        var ms = new MemoryStream();
        var serializer = new XmlSerializer(typeof(VolumeProfileData));
        serializer.Serialize(ms, data);
        return ms.ToArray();
    }
    
    /// <summary>
    /// Loads a VolumeProfile into a preset, which can be activated later<br />
    /// Returns true if successful
    /// </summary>
    /// <param name="preset">The preset name</param>
    /// <param name="bytes">The saved VolumeProfile in XML as a byte array</param>
    /// <returns></returns>
    public static bool LoadProfile(string preset, byte[] bytes)
    {
        try
        {

            if (preset == "NORMAL") return false;
            if(Presets.ContainsKey(preset)) return false;
            if (bytes == null || bytes.Length == 0) return false;
            
            VolumeProfileData data;
            var ms = new MemoryStream(bytes);
            
            var serializer = new XmlSerializer(typeof(VolumeProfileData));
            data = (VolumeProfileData)serializer.Deserialize(ms);
            
            var newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

            // ReSharper disable once PossibleNullReferenceException
            foreach (var compData in data.components)
            {
                try
                {
                    var type = Il2CppSystem.Type.GetType(compData.typeName);
                    if (type == null) continue;
                    var comp = ScriptableObject.CreateInstance(type).TryCast<VolumeComponent>();
                    // ReSharper disable once PossibleNullReferenceException
                    comp.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    JsonUtility.FromJsonOverwrite(compData.jsonData, comp);

                    newProfile.components.Add(comp);
                }
                catch (Exception e)
                {
                    if (TryFixingInvalidVolumePresets.HasFlag())
                        LogError(e);
                    else throw;
                }
            }
            
            newProfile.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            Presets[preset]=newProfile;
            return true;
        }
        catch (Exception e) { LogError("Error loading preset "+preset+"\n"+e); }
        return false;
    }
    /// <summary>
    /// Unload a currently loaded preset and disables it if active<br />
    /// Returns true if successful
    /// </summary>
    /// <param name="preset">The preset to be unloaded</param>
    /// <returns>bool</returns>
    public static bool UnloadProfile(string preset)
    {
        if(!Presets.ContainsKey(preset)) return false;
        if (_volumeHolder != null)
            if (_volumeHolder.profile == Presets[preset]) DisableProfile();
        Presets.Remove(preset);
        return true;
    }
    /// <summary>
    /// Enables a VolumeProfile<br />
    /// Returns true if successful
    /// </summary>
    /// <param name="profile">The profile to be enabled</param>
    /// <param name="getVolumeHolderIfNull">Search for the Volume Holder if null</param>
    /// <returns>bool</returns>
    public static bool EnableProfile(VolumeProfile profile, bool getVolumeHolderIfNull = true)
    {
        if (profile==null) return false;
        if (_volumeHolder==null)
        {
            if (getVolumeHolderIfNull)
            {
                _volumeHolder = Get<Volume>("StarlightVolumeHolder");
                return EnableProfile(profile,false);
            }
            return false;
        }

        _volumeHolder.priority = 999999;
        _volumeHolder.profile = profile;
        _volumeHolder.m_InternalProfile = profile;
        _volumeHolder.enabled = true;
        return true;
    }
    /// <summary>
    /// Enables a preset<br />
    /// Returns true if successful
    /// </summary>
    /// <param name="preset">The preset to be loaded</param>
    /// <param name="getVolumeHolderIfNull">Search for the Volume Holder if null</param>
    /// <returns>bool</returns>
    public static bool EnableProfile(string preset, bool getVolumeHolderIfNull = true)
    {
        if(!Presets.ContainsKey(preset)) return false;
        return EnableProfile(Presets[preset],getVolumeHolderIfNull);
    }

    /// <summary>
    /// Disables a VolumeProfile
    /// </summary>
    /// <param name="getVolumeHolderIfNull">Search for the Volume Holder if null</param>
    /// <returns>bool</returns>
    public static void DisableProfile(bool getVolumeHolderIfNull = true)
    {
        if (!_volumeHolder)
        {
            if (getVolumeHolderIfNull)
            {
                _volumeHolder = Get<Volume>("StarlightVolumeHolder");
                DisableProfile(false);
            }

            return;
        }
        _volumeHolder.profile = null;
        _volumeHolder.m_InternalProfile = null;
        _volumeHolder.enabled = false;
    }
    
    internal static void OnMainMenuUILoad()
    {
        if (_volumeHolder != null) return;
        if (_volumeHolder == null)
        {
            _volumeHolder = Get<Volume>("StarlightVolumeHolder");
            if (_volumeHolder != null) return;
        }
        var obj = new GameObject();
        obj.name = "StarlightVolumeHolder";
        obj.AddComponent<Volume>().enabled = false;
        Object.DontDestroyOnLoad(obj);
        _volumeHolder= obj.GetComponent<Volume>();
        foreach (var pair in EmbeddedResourceEUtil.LoadResources("Assets.VolumePresets"))
            LoadProfile(pair.Key, pair.Value);
        try
        {
            foreach (var path in Directory.GetFiles(StarlightEntryPoint.customVolumeProfilesPath))
            {
                try
                {
                    if (!path.EndsWith(".xml")) return;
                    var name = Path.GetFileNameWithoutExtension(path);
                    LoadProfile(name, File.ReadAllBytes(path));
                }
                catch (Exception e)
                {
                    LogError(e);
                    LogError("Error loading volume profile: "+path);
                }
            }
            if (ExportAllVolumePresets.HasFlag())
            {
                foreach (var pair in Presets)
                    File.WriteAllBytes(StarlightEntryPoint.dataPath + "/" + pair.Key, SaveProfile(pair.Value));
            }
        } catch { }
        
    }
    
    [Serializable]
    public class VolumeProfileData
    {
        public List<ComponentData> components = new ();

        [Serializable]
        public class ComponentData
        {
            public string typeName;
            public string jsonData;
        }
    }
}