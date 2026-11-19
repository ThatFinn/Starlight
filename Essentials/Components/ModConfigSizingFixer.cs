using Il2CppTMPro;
using Starlight.Storage;

namespace Starlight.Components;

[InjectIntoIL]
internal class ModConfigSizingFixer : MonoBehaviour
{
    private void Start()
    {
        ExecuteInTicks(() =>
        {
            var rectT = GetComponent<RectTransform>();
            var newValue = gameObject.GetObjectRecursively<TextMeshProUGUI>("NameAndDescription").GetRenderedHeight() + 5;
            if (newValue < rectT.sizeDelta.y) newValue = rectT.sizeDelta.y;
            rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, newValue);
        },1);
        
    }
}