// Originally taken from https://github.com/SlimeRancherModding/MelonSRML
// Original Code from Lionmeow
using System.Linq;
using System.Reflection;

namespace Starlight.EnumPatcher;

public class EnumHolderResolver
{
    public static void RegisterAllEnums(Assembly assembly)
    {
        foreach (var module in assembly.Modules)
            foreach (var type in module.GetTypes())
            {
                if (!type.GetCustomAttributes(true).Any((x) => x is EnumHolderAttribute)) continue;
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!field.FieldType.IsEnum) continue;

                    if ((int)field.GetValue(null) != 0) continue;
                    var newVal = EnumPatcher.GetFirstFreeValue(field.FieldType);
                    EnumPatcher.AddEnumValue(field.FieldType, newVal, field.Name);
                    field.SetValue(null, newVal);
                }
            }
    }
}