using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Managers;
using Starlight.Storage;
using UnityEngine.SceneManagement;

namespace Starlight.Utils;

public static class SpawnEUtil
{
    
    public static GadgetModel SpawnGadget(this GadgetDefinition def, Vector3 pos) => SpawnGadget(def, pos, Quaternion.identity);
    public static GadgetModel SpawnGadget(this GadgetDefinition def, Vector3 pos, Vector3 rot)=> SpawnGadget(def, pos, Quaternion.Euler(rot));
    public static GadgetModel SpawnGadget(this GadgetDefinition def, Vector3 pos, Quaternion rot)
    {
        if (def == null) return null;
        var modelGadget = sceneContext.GameModel.InstantiateGadgetModel(def, systemContext.SceneLoader.CurrentSceneGroup, pos,false);
        GadgetDirector.InstantiateGadgetFromModel(modelGadget);
        modelGadget.eulerRotation = rot.ToEuler();
        return modelGadget;
    }
    public static GameObject SpawnActor(this IdentifiableType ident, Vector3 pos) => SpawnActor(ident, pos, Quaternion.identity);
    public static GameObject SpawnActor(this IdentifiableType ident, Vector3 pos, Vector3 rot)=> SpawnActor(ident, pos, Quaternion.Euler(rot));
    public static GameObject SpawnActor(this IdentifiableType ident, Vector3 pos, Quaternion rot)
    {
        if (!ident) return null;
        if (ident.TryCast<GadgetDefinition>()) return SpawnGadget(ident.TryCast<GadgetDefinition>(), pos, rot).GetGameObject();
        return InstantiationHelpers.InstantiateActorFromModel(sceneContext.GameModel.InstantiateActorModel(ident, sceneContext.RegionRegistry.CurrentSceneGroup, pos, rot, false));
    }
    public static GameObject SpawnDynamic(this GameObject obj, Vector3 pos, Quaternion rot)
    {
        return InstantiationHelpers.InstantiateDynamic(obj, pos, rot);
    }
    
    public static GameObject SpawnFX(this GameObject fx, Vector3 pos) => SpawnFX(fx, pos, Quaternion.identity);
    
    public static GameObject SpawnFX(this GameObject fx, Vector3 pos, Quaternion rot)
    {
        return FXHelpers.SpawnFX(fx, pos, rot);
    }
    
    internal static List<LandPlotLocation> LandPlotLocations = new();
    private static Dictionary<string, GameObject> _rootObjects = new ();

    internal static void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        ExecuteInTicks((() =>
        {
            if (sceneName == "MainMenuUI")
            {
                LandPlotLocations = new();
                _rootObjects = new();
            }
            if(inGame&&!StarlightCounterGateManager.srleActive)
                if(StarlightSaveManager.inGameData!=null)
                    foreach (var plot in StarlightSaveManager.inGameData.CustomPlots)
                        if (plot.Value.SceneName == sceneName)
                        {
                            try
                            {
                                SpawnLandPlot(plot.Key, plot.Value);
                            } catch (Exception e) { LogError(e); }
                        }
            
        }),2);
    }
    
    /// <summary>
    /// Note, custom landplots don't support drones yet!
    /// </summary>
    /// <param name="id"></param>
    /// <param name="loc"></param>
    public static void AddCustomLandPlot(string id,StarlightLandPlotLocation loc)
    {
        if (!inGame) return;
        if (loc == null || string.IsNullOrWhiteSpace(id)) return;
        var scene = SceneManager.GetSceneByName(loc.SceneName);
        if (StarlightSaveManager.inGameData.CustomPlots.ContainsKey(id)) return;
        
        StarlightSaveManager.inGameData.CustomPlots.Add(id,loc);
        if (scene.isLoaded)
            SpawnLandPlot(id, loc);

    }
    public static bool HasCustomLandPlot(string id) => StarlightSaveManager.inGameData.CustomPlots.ContainsKey(id);
    public static void RemoveCustomLandPlot(string id)
    {
        if (!inGame) return;
        if (string.IsNullOrWhiteSpace(id)) return;
        if (!StarlightSaveManager.inGameData.CustomPlots.ContainsKey(id)) return;
        StarlightSaveManager.inGameData.CustomPlots.Remove(id);
        try { Object.Destroy(sceneContext.GameModel.landPlots[id].gameObj); } catch { }
        try { Object.Destroy(sceneContext.GameModel.landPlots["plot"+id].gameObj); } catch { }
        try { sceneContext.GameModel.UnregisterLandPlot(id); }catch { }
        try { sceneContext.GameModel.UnregisterLandPlot("plot"+id); }catch { }
        if (sceneContext.GameModel.landPlots.ContainsKey(id))
            sceneContext.GameModel.landPlots.Remove(id);
        if (sceneContext.GameModel.landPlots.ContainsKey("plot"+id))
            sceneContext.GameModel.landPlots.Remove(id);
    }

    private static void SpawnLandPlot(string plotKey, StarlightLandPlotLocation loc)
    {
        var landplotRoot = GetNewLandPlotRoot(loc.SceneName);
        var obj = new GameObject(plotKey);
        var lpl = obj.AddComponent<LandPlotLocation>();
        LandPlotLocations.Add(lpl);
        lpl._id = "plot" + plotKey;
        obj.transform.SetParent(landplotRoot.transform);
        obj.transform.position = loc.Position;
        obj.transform.rotation = loc.Rotation;
        obj.transform.localScale = loc.Scale;
        var id = loc.DefaultPlot;
        if (id == LandPlot.Id.NONE) id = LandPlot.Id.EMPTY;
        ExecuteInTicks(() =>
        {
            var prefab = gameContext.LookupDirector.GetPlotPrefab(id);
            var plotObj = Object.Instantiate(prefab, obj.transform);
            lpl.enabled = true;
            ExecuteInTicks(() =>
            {
                var landPlot = plotObj.GetComponent<LandPlot>();
                try { sceneContext.GameModel.RegisterLandPlot(lpl._id,obj); }catch { }
                //Yes, it's a different key on purpose
                landPlot.InitModel(sceneContext.GameModel.InitializeLandPlotModel(plotKey));
            },2);
        },2);
    }
    private static GameObject GetNewLandPlotRoot(string sceneName)
    {
        if(_rootObjects.ContainsKey(sceneName))
            if (_rootObjects[sceneName])
                return _rootObjects[sceneName];
        var gameObj = new GameObject("StarlightLandPlotRoots-" + sceneName);
        SceneManager.MoveGameObjectToScene(gameObj, SceneManager.GetSceneByName(sceneName));
        var foundACell = false;
        foreach (var dir in GetAllInScene<CellDirector>())
            if (dir.gameObject.scene.name == sceneName)
            {
                gameObj.transform.SetParent(dir.transform);
                foundACell = true;
                break;
            }
        if(!foundACell)
        {
            var anyDir = GetAnyInScene<CellDirector>();
            if(!anyDir) Log("Oh oh... A landplot is outside a CellDirector. Things are about to get sideways");
            else gameObj.transform.SetParent(anyDir.transform);
        }
        _rootObjects[sceneName] = gameObj;
        return gameObj;
    }

    public static bool SetAnyRadiantSlimeAppearance(IdentifiableActor actor)
    {
        if (!SupportRadiant.HasFlag()) return false;
        var applicator = actor.transform.GetComponent<SlimeAppearanceApplicator>();
        if (applicator && actor && SlimeDefinition.IsSlimeDefinition(actor.identType))
        {
            var def = actor.identType.Cast<SlimeDefinition>();
            var newAppearance = def.GetRadiantAppearance();
            if (!newAppearance) return false;
            RotateSlimeActorAppearance_Radiant(actor, newAppearance);
            applicator.Appearance = newAppearance;
            applicator.ApplyAppearance();
            applicator.HandleChosenAppearanceChanged(def,newAppearance);
            return true;
        }

        return false;
    }
    public static bool RotateSlimeActorAppearance(IdentifiableActor actor)
    {
        var applicator = actor.transform.GetComponent<SlimeAppearanceApplicator>();
        if (applicator && actor && SlimeDefinition.IsSlimeDefinition(actor.identType))
        {
            var currentIndex = 0;
            var def = actor.identType.Cast<SlimeDefinition>();
            for (int i = 0; i < def.AppearancesDefault.Count; i++)
                if (def.AppearancesDefault[i] == applicator.Appearance)
                {
                    currentIndex = i; break;
                }

            currentIndex++;
            if (currentIndex >= def.AppearancesDefault.Count) currentIndex = 0;
            var newAppearance = def.AppearancesDefault[currentIndex];
            if(SupportRadiant.HasFlag()) RotateSlimeActorAppearance_Radiant(actor, newAppearance);
            applicator.Appearance = newAppearance;
            applicator.ApplyAppearance();
            applicator.HandleChosenAppearanceChanged(def,newAppearance);
            var animator = actor.transform.GetObjectRecursively<Animator>("Appearance");
            if (animator)
            {
                animator.enabled = false;
                animator.enabled = true;
            }

            foreach (var ui in GetAllInScene<TargetingUI>())
            {
                try
                {
                    ui._currentTarget = null;
                    ui.Update();
                } catch {}
            }

            return true;
        }
        return false;
    }

    private static void RotateSlimeActorAppearance_Radiant(IdentifiableActor actor, SlimeAppearance newAppearance)
    {
        var slimeRadiant = actor.transform.GetComponent<SlimeRadiant>();
        if(slimeRadiant)
        {
            if (newAppearance.name.Contains("Radiant"))
            {
                slimeRadiant.SetRadiant();
                slimeRadiant.SetRadiantAppearance();
            }
            else
            {
                slimeRadiant._slimeModel.RadiantBaseType = null;
            }
        }
    }
}