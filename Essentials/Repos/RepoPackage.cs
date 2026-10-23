/*using System;
using System.Diagnostics.CodeAnalysis;
using Starlight.Enums;

namespace Starlight.Repos;

[System.Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class RepoPackage
{
    public string name;
    public string author;
    public string coauthors;
    public string contributors;
    public string description;
    public string company;
    public string trademark;
    public string team;
    public string copyright;
    public string sourcecode;
    public string github_repo;
    public string nexus;
    public string website;
    public string header_url;
    public string icon_url;
    public byte color_r;
    public byte color_g;
    public byte color_b;
    public byte color_a;
    public PackageType type = PackageType.MelonMod;
    public bool universal;
    public List<RepoPackageVersion> versions = new List<RepoPackageVersion>();

    public RepoPackageVersion getLatestVersion(string branch)
    {
        RepoPackageVersion latestVersion = null;
        foreach (var version in versions)
        {
            if (branch == version.branch)
            {
                if(latestVersion == null)
                    latestVersion = version;
                else
                {
                    var dateNew = DateTime.Parse(version.release_date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                    var dateOld = DateTime.Parse(latestVersion.release_date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                    if(dateNew>dateOld)
                        latestVersion = version;
                }
            }
        }

        return latestVersion;
    }
}*/