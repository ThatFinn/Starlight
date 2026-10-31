namespace Starlight.Saving;

/// <summary>
/// The reason a save backup is created
/// </summary>
/// <param name="Id">
/// Identifier; <c>sr2mp.before_host</c>
/// </param>
/// <param name="Owner">
/// Package id of whoever registered it; <c>com.author.modname</c>
/// </param>
/// <param name="Keep">
/// How many backups to keep <b>within this <see cref="BackupReason"/></b>, independent of other reasons
/// </param>
/// <param name="Periodic">
/// Whether this backup reason fires periodically
/// </param>
/// <param name="Forced">
/// Whether this reason bypasses duplication and other measures
/// </param>
public sealed record BackupReason(
    string Id,
    string Owner,
    int Keep = 5,
    bool Periodic = false,
    bool Forced = false);