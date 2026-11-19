using System;
using Il2CppTMPro;
using Starlight.Storage;
using UnityEngine.UI;

namespace Starlight.Components;

[InjectIntoIL]
internal class CheatMenuRefineryEntry : MonoBehaviour
{
    internal IdentifiableType item;
    private Image icon;
    private bool dontChange = false;
    private Slider amountSlider;
    private TextMeshProUGUI handleText;
    private TextMeshProUGUI itemName;
    private bool didStartRan = false;
    private void Start()
    {
        didStartRan = true;
        amountSlider = gameObject.GetObjectRecursively<Slider>("Slider");
        amountSlider.maxValue = GadgetDirector.REFINERY_MAX;
        handleText = amountSlider.gameObject.GetObjectRecursively<TextMeshProUGUI>("Text");
        itemName = gameObject.GetObjectRecursively<TextMeshProUGUI>("Name");
        icon = gameObject.GetObjectRecursively<Image>("Icon");
        icon.sprite = item.icon;
        itemName.text = item.GetName(false);
        amountSlider.onValueChanged.AddListener((Action<float>)((valueFloat) =>
        {
            if (dontChange)
            {
                dontChange = false; return;
            }

            int newValue = (int)valueFloat;
            
            //Adding one updates the new value everywhere. Not doing can cause issues
            sceneContext.GadgetDirector._model.SetCount(item,newValue-1);
            sceneContext.GadgetDirector.AddItem(item,1);
            
            handleText.text = (newValue.ToString());
        }));}

    internal void OnOpen()
    {
        if(!didStartRan) Start();

        dontChange = true;
        amountSlider.value = sceneContext.GadgetDirector.GetItemCount(item);
        handleText.text = (sceneContext.GadgetDirector.GetItemCount(item).ToString());
    }
}