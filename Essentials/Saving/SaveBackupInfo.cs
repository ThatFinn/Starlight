using System;

namespace Starlight.Saving;

/// <summary>
/// One backup
/// </summary>
/// <param name="Save"> Save identity </param>
/// <param name="ReasonId"> Id of the <see cref="BackupReason"/> this was taken for </param>
/// <param name="TakenUtc"> When it was taken </param>
/// <param name="Folder"> Absolute path to the backup's own directory </param>
/// <param name="FileCount"> How many files it holds </param>
public sealed record SaveBackupInfo(
    string Save,
    string ReasonId,
    DateTime TakenUtc,
    string Folder,
    int FileCount)
{
    /// <inheritdoc/>
    public override string ToString()
        => $"{Save} - {ReasonId} - {TakenUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss} - {FileCount} file(s)";
}