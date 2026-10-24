using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;

namespace Starlight.Storage;

public class LoadingGameSessionData
{
    public readonly ActorIdProvider ActorIdProvider;
    public readonly ISaveReferenceTranslation ISaveReferenceTranslation;
    public readonly SaveReferenceTranslation SaveReferenceTranslation;
    public GameV10 GameV10;
    public GameV11 GameV11;
    public readonly dynamic GameState;
    public readonly GameModel GameModel;

    internal LoadingGameSessionData(ActorIdProvider actorIdProvider, ISaveReferenceTranslation iSaveReferenceTranslation,
        SaveReferenceTranslation saveReferenceTranslation, dynamic gameState, GameModel gameModel)
    {
        this.ActorIdProvider = actorIdProvider;
        this.ISaveReferenceTranslation = iSaveReferenceTranslation;
        this.SaveReferenceTranslation = saveReferenceTranslation;
        this.GameState = gameState;
        this.GameModel = gameModel;
        try { ApplyGameVersions(); } catch { }
    }
    void ApplyGameVersions()
    {
        if(GameState is GameV11 gameV11)
            this.GameV11 = gameV11;
        else if(GameState is GameV10 gameV10)
            this.GameV10 = gameV10;
    }
}