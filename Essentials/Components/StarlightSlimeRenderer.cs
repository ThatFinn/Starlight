using Starlight.Patches.Context;
using Starlight.Storage;

namespace Starlight.Components;

[InjectIntoIL]
public class StarlightSlimeRenderer : MonoBehaviour
{
    private GameObject _renderedObj;
    private bool _oldUnscaledTime;
    public bool unscaledTime
    {
        get
        {
            try { if(_renderedObj.GetObjectRecursively<Animator>("Appearance").updateMode==AnimatorUpdateMode.UnscaledTime) return true; }
            catch {}
            return false;
        }
        set
        {
            _oldUnscaledTime = value;
            if(value)
                try { _renderedObj.GetObjectRecursively<Animator>("Appearance").updateMode=AnimatorUpdateMode.UnscaledTime; }
                catch {}
            else
                try { _renderedObj.GetObjectRecursively<Animator>("Appearance").updateMode=AnimatorUpdateMode.Normal; }
                catch {}
        }
    }

    public void DestroyRender()
    {
        if(_renderedObj) Destroy(_renderedObj);
    }

    public GameObject RenderAppearance(SlimeDefinition slime, float scale = 1.0f) => RenderAppearance(slime.GetDefaultAppearance(), scale, slime);
    public GameObject RenderAppearance(SlimeAppearance appearance,float scale = 1.0f,SlimeDefinition customBones = null)
    {
        if(_renderedObj) Destroy(_renderedObj);
        if(!customBones)
            _renderedObj = Instantiate(GameContextPatch.SlimeRendererPrefab,transform);
        else
        {
            var prefab = GenerateFittingPrefab(customBones);
            _renderedObj = Instantiate(prefab,transform);
            Destroy(prefab);
        }
        var app = _renderedObj.transform.GetChild(0).gameObject.GetComponent<SlimeAppearanceApplicator>();
        app.ClearAppearance();
        app.Appearance = appearance;
        app.AppearanceObjectProvider = GameContextPatch.RendererPool;
        _renderedObj.SetActive(true);
        app.transform.localScale = new Vector3(scale, scale, scale);
        app.ApplyAppearance();
        app.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        SetLayerRecursive(_renderedObj, gameObject.layer);
        unscaledTime = _oldUnscaledTime;
        return _renderedObj;
    }

    private GameObject GenerateFittingPrefab(SlimeDefinition definition)
    {
        var prefab = new GameObject("SlimeRenderer"+definition.name);
        prefab.SetActive(false);
        var instance = GameObject.Instantiate(definition.prefab,prefab.transform);
        foreach (var obj in instance.GetChildren()) if(obj.name!="Appearance") Object.DestroyImmediate(obj);
        foreach (var comp in instance.GetComponents<Component>())
        {
            if (comp is Transform || comp.TryCast<SlimeAppearanceApplicator>()) continue;
            Object.DestroyImmediate(comp);
        }
        var app = instance.GetComponent<SlimeAppearanceApplicator>();
        app.SlimeDefinition = definition;
        app.Appearance = definition.AppearancesDefault[0];
        return prefab;
    }
    void SetLayerRecursive(GameObject obj, int newLayer)
    {
        if (!obj) return;
        obj.layer = newLayer;
        foreach (var child in obj.transform.GetChildren())
        {
            if (!child) continue;
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }
}