using System;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Storage;

namespace Starlight.Components.Debug;

[InjectIntoIL]
internal class FPSDisplayFixer : MonoBehaviour
{
    private FPSDisplay display;
    void Start()
    {
        display = GetComponent<FPSDisplay>();
        display.displayText.gameObject.SetActive(true);
        display.versionText.gameObject.SetActive(true);
        display.versionText.text = (Application.version);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        display.frames++;
        display.duration += dt;

        if (dt < display.bestDuration) display.bestDuration = dt;
        if (dt > display.worstDuration) display.worstDuration = dt;

        if (display.duration < display.sampleDuration) return;

        float targetFps = display.frames / display.duration;
        float targetMs = (display.duration / display.frames) * 1000f;

        float bestFps = 1f / display.bestDuration;
        float worstFps = 1f / display.worstDuration;

        float bestMs = display.bestDuration * 1000f;
        float worstMs = display.worstDuration * 1000f;

        Color textColor = Color.white;

        if (targetFps < display.majorDropsValue)
            textColor = display.majorDropsColor;
        else if (targetFps < display.minorDropsValue)
            textColor = display.minorDropsColor;

        display.displayText.color = textColor;
        display.displayText.text = (
            "FPS / Ms\n" +
            (int)targetFps+" / "+(float)Math.Round(targetMs, 1)+"\n" +
            (int)bestFps+" / "+(float)Math.Round(bestMs, 1)+"\n" +
            (int)worstFps+" / "+(float)Math.Round(worstMs, 1)+"\n"
        );

        display.frames = 0;
        display.duration = 0f;
        display.bestDuration = float.MaxValue;
        display.worstDuration = 0f;
    }
}