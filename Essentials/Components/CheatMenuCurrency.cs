using System;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppTMPro;
using Starlight.Storage;
using UnityEngine.UI;

namespace Starlight.Components;

[InjectIntoIL]
internal class CheatMenuCurrency : MonoBehaviour
{
    private Slider _amountSlider;
    private TextMeshProUGUI _handleText;
    private int _dontChange;
    private string _currencyID;
    private CurrencyDefinition definition {
        get
        {
            try { return gameContext.LookupDirector.CurrencyList.FindCurrencyByReferenceId(_currencyID); }
            catch { return null; }
        }
    }
    public void OnOpen(string id)
    {
        _currencyID = id;
        var text = definition.GetCompactName();
        if (text.Length > 25) text = _currencyID.Replace("CurrencyDefinition.","");
        gameObject.GetObjectRecursively<TextMeshProUGUI>("Text").text = " " + text+":";
        _amountSlider = gameObject.GetObjectRecursively<Slider>("Slider");
        _handleText = _amountSlider.gameObject.GetObjectRecursively<TextMeshProUGUI>("Text");
        _amountSlider.onValueChanged.AddListener((Action<float>)((value) =>
        {
            if (_dontChange>0)
            { _dontChange--; return; }
            _dontChange = 0;
            int newValue = Mathf.Clamp((int)Math.Pow(value, 3.51),0,sceneContext.PlayerState._model.maxCurrency);
            _handleText.text = (newValue.ToString());
            CurrencyEUtil.SetCurrency(_currencyID, newValue, newValue);
        }));
        try
        {
            double newValue = Math.Pow(CurrencyEUtil.GetCurrency(_currencyID), (1.0 / 3.51));
            if (newValue.ToString() == "NaN") newValue = 0;
            _dontChange = 2;
            _amountSlider.value = float.Parse(newValue.ToString());
            _handleText.text = (CurrencyEUtil.GetCurrency(_currencyID).ToString());
        }
        catch { }
    }
}