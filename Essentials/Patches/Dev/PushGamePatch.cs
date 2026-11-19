using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Starlight.Storage;

namespace Starlight.Patches.Dev;

[FeatureFlagDependentPatch(DevMode)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushGame))]
internal static class PushGamePatch
{
    public static ActorIdProvider tmpActorIdProvider;
    public static dynamic tmpGameState;
    public static ISaveReferenceTranslation tmpSaveReferenceTranslation;
    public static GameModel tmpGameModel;
    public static void Postfix(ActorIdProvider actorIdProvider, ISaveReferenceTranslation saveReferenceTranslation, dynamic gameState, GameModel gameModel)
    {
        tmpActorIdProvider=actorIdProvider;
        tmpSaveReferenceTranslation=saveReferenceTranslation;
        tmpGameState=gameState;
        tmpGameModel=gameModel;
    }
}