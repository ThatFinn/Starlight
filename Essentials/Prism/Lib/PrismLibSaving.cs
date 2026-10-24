namespace Starlight.Prism.Lib;

/// <summary>
/// A library of helper functions for dealing with saving and loading
/// </summary>
public class PrismLibSaving
{
    /// <summary>
    /// Setups an identifiable type for saving
    /// USE IT ONLY WHEN YOU DONT USE THE CREATOR CLASSES
    /// </summary>
    /// <param name="ident">The identifiable type to setup</param>
    /// <param name="refID">The reference ID to use for saving</param>
    public static void SetupForSaving(IdentifiableType ident, string refID = null)
    {
        if (ident == null) return;
        SaveRegistry.Register(ident, refID);
    }
}