using System;

namespace Starlight.Utils;

public static class VersionedEUtil
{
    public static Il2CppSystem.Type GetLatestGameVXX()
    {
        var namespaceName = "MonomiPark.SlimeRancher.Persist";
        Il2CppSystem.Type highestType = null;
        int currentVersion = 1;

        while (true)
        {
            var versionString = currentVersion.ToString("D2");
            var fullTypeName = $"{namespaceName}.GameV{versionString}";

            var currentType = FindType(fullTypeName);
            if (currentType != null)
            {
                highestType = currentType;
                currentVersion++;
            }
            else break;
        }

        if (highestType != null)
            return highestType;
        
        return null;
    }
    public static Type GetLatestSystemGameVXX()
    {
        var namespaceName = "Il2CppMonomiPark.SlimeRancher.Persist";
        Type highestType = null;
        int currentVersion = 1;

        while (true)
        {
            var versionString = currentVersion.ToString("D2");
            var fullTypeName = $"{namespaceName}.GameV{versionString}";

            var currentType = FindSystemType(fullTypeName);
            if (currentType != null)
            {
                highestType = currentType;
                currentVersion++;
            }
            else break;
        }

        if (highestType != null)
            return highestType;
        
        return null;
    }
    internal static Type FindSystemType(string fullName)
    {
        var type = Type.GetType(fullName);
        if (type != null) return type;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(fullName);
            if (type != null) return type;
        }
        return null;
    }
    internal static Il2CppSystem.Type FindType(string fullName)
    {
        var type = Il2CppSystem.Type.GetType(fullName);
        if (type != null) return type;
        foreach (var assembly in Il2CppSystem.AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(fullName);
            if (type != null) return type;
        }
        return null;
    }
}