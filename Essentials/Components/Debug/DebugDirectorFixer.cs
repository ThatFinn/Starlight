using Il2CppMonomiPark.SlimeRancher.DebugTool;
using Starlight.Storage;

namespace Starlight.Components.Debug;

[InjectIntoIL]
internal class DebugDirectorFixer : MonoBehaviour
{
    internal static DebugDirectorFixer Instance;
    internal DebugDirector Director;
    void Start()
    {
        Instance = this;
        Director = GetComponent<DebugDirector>();
    }
}