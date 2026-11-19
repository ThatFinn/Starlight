using System.Globalization;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Newtonsoft.Json;
using Starlight.Enums;
using Starlight.Managers;
using Starlight.Patches.General;

namespace Starlight.Storage;

[System.Serializable]
public class Warp
{
    public string sceneGroup = "None";
    public float x;
    public float y;
    public float z;

    internal Vector3 position => new Vector3(x, y, z);

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    internal Quaternion rotation => new Quaternion(rotX, rotY, rotZ, rotW);
    
    public bool IsValid()
    {
        foreach (var g in systemContext.SceneLoader.SceneGroupList.items)
            if (g.ReferenceId == sceneGroup)
                return g.IsGameplay;
        return false;
    }

    bool IsInCorrectSceneGroup(string refIDCurrent, string refIDNext)
    {
        if (refIDCurrent == refIDNext) return true;
        if (refIDCurrent == "SceneGroup.AllZones")
        {
            if (refIDNext == "SceneGroup.ConservatoryFields") return true;
            if (refIDNext == "SceneGroup.PowderfallBluffs") return true;
            if (refIDNext == "SceneGroup.RumblingGorge") return true;
        }
        return false;
    }
    public StarlightError WarpPlayerThere()
    {
        if (!inGame) return StarlightError.NotInGame;
        if(!IsValid()) return StarlightError.Invalid;
        if (!sceneContext.Player) return StarlightError.PlayerNull;
        
        var p = sceneContext.Player.GetComponent<TeleportablePlayer>();
        if (!p) return StarlightError.TeleportablePlayerNull;
        
        var cc = sceneContext.Player.GetComponent<SRCharacterController>();
        if (!cc) return StarlightError.SRCharacterControllerNull;
        
        MenuEUtil.CloseOpenMenu();
        
        if (IsInCorrectSceneGroup(p.SceneGroup.ReferenceId,sceneGroup))
        {
            cc.Position = position;
            cc.Rotation = rotation;
            cc.BaseVelocity = Vector3.zero;
            cc.ResetVelocity(false);
        }
        else
        {
            try
            {
                SceneGroup sc = null;
                foreach (var g in systemContext.SceneLoader.SceneGroupList.items)
                    if (g.ReferenceId == sceneGroup)
                    {
                        if(!g.IsGameplay) return StarlightError.SceneGroupNotSupported;
                        sc = g;
                        break;
                    }
                if(!sc) return StarlightError.SceneGroupNotSupported;
                StarlightWarpManager.WarpTo = this;
                SceneLoaderLoadSceneGroupPatch.isTeleportingPlayer = true;
                LocationBookmarksUtil.GoToLocationPlayer(sc,position+new Vector3(0,LocationBookmarksUtil.PLAYER_HEIGHT/2,0),rotation.eulerAngles);

            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        return StarlightError.NoError;
    }

    internal LocationBookmarksUtil.LocationBookmark ToNative()
    {
        var mark = new LocationBookmarksUtil.LocationBookmark
        {
            position = position,
            rotationEuler = rotation.eulerAngles,
            sceneGroupName = sceneGroup
        };
        if (mark.sceneGroupName.StartsWith("SceneGroup."))
            mark.sceneGroupName = sceneGroup.Substring("SceneGroup.".Length);
        return mark;
    }
    internal static Warp FromNative(LocationBookmarksUtil.LocationBookmark mark)
    {
        if (mark == null) return null;
        return new Warp("SceneGroup." + mark.sceneGroupName, mark.position, mark.rotationEuler);
        
    }

    public static Warp CurrentLocation()
    {
        try { return FromNative(LocationBookmarksUtil.GetNewPlayerLocationBookmark()); } catch { }

        return new Warp();
    }
    public static Warp FromString(string stringed)
    {
        if (!LocationBookmarksUtil.ValidLocationString(stringed)) return null;
        var parts = stringed.Split('|');

        var posParts = parts[1].Split(',');
        var position = new Vector3(
            float.Parse(posParts[0], CultureInfo.InvariantCulture),
            float.Parse(posParts[1], CultureInfo.InvariantCulture),
            float.Parse(posParts[2], CultureInfo.InvariantCulture)
        );

        var rotParts = parts[2].Split(',');
        var rotation = new Vector3(
            float.Parse(rotParts[0], CultureInfo.InvariantCulture),
            float.Parse(rotParts[1], CultureInfo.InvariantCulture),
            float.Parse(rotParts[2], CultureInfo.InvariantCulture)
        );
        return new Warp(parts[0], position, rotation);
    }

    public override string ToString() => LocationBookmarksUtil.GetLocationStringFromBookmark(ToNative());
    
    private Warp() {}
    [JsonConstructor]
    public Warp(string sceneGroup, Vector3 position, Quaternion rotation)
    {
        this.sceneGroup = sceneGroup;
        x = position.x; y = position.y; z = position.z;

        rotX = rotation.x; rotY = rotation.y; rotZ = rotation.z; rotW = rotation.w;
    }
    public Warp(string sceneGroup, Vector3 position, Vector3 rotationEuler)
    {
        this.sceneGroup = sceneGroup;
        x = position.x; y = position.y; z = position.z;

        var rotationXYZ = Quaternion.Euler(rotationEuler);
        rotX = rotationXYZ.x; rotY = rotationXYZ.y; rotZ = rotationXYZ.z; rotW = rotationXYZ.w;
    }
    public Warp(SceneGroup sceneGroup, Vector3 position, Quaternion rotation)
    {
        this.sceneGroup = sceneGroup.ReferenceId;
        x = position.x;
        y = position.y;
        z = position.z;

        rotX = rotation.x; rotY = rotation.y; rotZ = rotation.z; rotW = rotation.w;
    }
    public Warp(SceneGroup sceneGroup, Vector3 position, Vector3 rotationEuler)
    {
        this.sceneGroup = sceneGroup.ReferenceId;
        x = position.x; y = position.y; z = position.z;

        var rotationXYZ = Quaternion.Euler(rotationEuler);
        rotX = rotationXYZ.x; rotY = rotationXYZ.y; rotZ = rotation.z; rotW = rotationXYZ.w;
    }
}