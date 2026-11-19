using System.Reflection;
using MelonLoader;
using Starlight.Expansion;

[assembly: AssemblyTitle(Starlight.BuildInfo.Name)]
[assembly: AssemblyDescription(Starlight.BuildInfo.Description)]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct(Starlight.BuildInfo.Name)]
[assembly: AssemblyCopyright($"Created by {Starlight.BuildInfo.Author}")]
[assembly: AssemblyTrademark(null)]
[assembly: VerifyLoaderVersion(0,7,3, true)]
[assembly: AssemblyVersion(Starlight.BuildInfo.CodeVersion)]
[assembly: MelonPriority(-20000)]
[assembly: AssemblyFileVersion(Starlight.BuildInfo.CodeVersion)]
[assembly: MelonInfo(typeof(Starlight.StarlightEntryPoint), Starlight.BuildInfo.Name, Starlight.BuildInfo.CodeVersion, Starlight.BuildInfo.Author, Starlight.BuildInfo.DownloadLink)]
[assembly: MelonGame("MonomiPark", "SlimeRancher2")]
[assembly: MelonColor(255, 35, 255, 35)]
[assembly: AssemblyMetadata(StarlightModInfoAttributes.DisplayVersion,Starlight.BuildInfo.DisplayVersion)]
[assembly: AssemblyMetadata(StarlightModInfoAttributes.Contributors,Starlight.BuildInfo.Contributors)]
[assembly: AssemblyMetadata(StarlightModInfoAttributes.CoAuthors,Starlight.BuildInfo.CoAuthors)]
[assembly: AssemblyMetadata(StarlightModInfoAttributes.SourceCode,Starlight.BuildInfo.SourceCode)]
[assembly: AssemblyMetadata(StarlightModInfoAttributes.Nexus,Starlight.BuildInfo.Nexus)]
[assembly: AssemblyMetadata(StarlightModInfoAttributes.Discord,Starlight.BuildInfo.Discord)]
[assembly: HarmonyDontPatchAll()]