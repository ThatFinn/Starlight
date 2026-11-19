using Starlight.EmberMode.Components;

namespace Starlight.EmberMode;

internal static class EmberModeMode
{
    internal enum Mode { Gameplay, Transform }
    
    internal static Mode Current = Mode.Gameplay;

    internal static void SetMode(Mode mode)
    {
        if (Current == mode) return;
        var previous = Current;
        Current = mode;
        
        switch (previous)
        {
            case Mode.Transform:
                EmberModeUIInfo.ClearToolInfo();
                break;
        }

        if (mode == Mode.Transform)
        {
            if (EmberModeTransformInspector.Instance && EmberModeTransformInspector.Instance.WindowRoot)
                EmberModeTransformInspector.Instance.WindowRoot.SetActive(true);
        }
        if(DebugLogging.HasFlag())
            Log($"[EmberMode/Mode] {previous} -> {mode}");
    }
}