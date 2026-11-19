using Starlight.Storage;

namespace Starlight.Components.Debug;
[InjectIntoIL]
internal class DevelopmentBuildText : MonoBehaviour
{

    private int fontSize = 11;
    void OnGUI()
    {
        string text = "Development Build";

        string json = JsonUtility.ToJson(GUI.skin.label);
        var style = JsonUtility.FromJson<GUIStyle>(json);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.LowerRight;

        style.normal.textColor = Color.black;
        Vector2 offset = new Vector2(1, 1);
        Rect rect = new Rect(Screen.width - 10 - 200, Screen.height - 30, 200, 30);

        GUI.Label(new Rect(rect.x - offset.x, rect.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x + offset.x, rect.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x, rect.y - offset.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x, rect.y + offset.y, rect.width, rect.height), text, style);

        style.normal.textColor = Color.white;
        GUI.Label(rect, text, style);
    }
}