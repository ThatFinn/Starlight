using Il2CppMonomiPark.SlimeRancher.Pedia;

namespace Starlight.Prism.Creators.Actions;

/// <summary>
/// A pedia entry that has just been unlocked.
/// </summary>
/// <param name="Entry"> The entry </param>
/// <param name="ShowedPopup"> Whether the game showed the unlock notification for it </param>
public sealed record PediaUnlock(PediaEntry Entry, bool ShowedPopup);