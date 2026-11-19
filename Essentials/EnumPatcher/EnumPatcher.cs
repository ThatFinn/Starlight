// Originally taken from https://github.com/SlimeRancherModding/MelonSRML
// Original Code from Lionmeow
using System;
using System.Globalization;
using System.Linq;
using Il2CppInterop.Runtime;

namespace Starlight.EnumPatcher;

public static class EnumPatcher
{
    private static Dictionary<Type, EnumPatch> _patches = new ();
    private static Dictionary<int, EnumPatch> _il2CpPpatches = new ();

    internal static bool TryAsNumber(this object value, Type type, out object result)
    {
        if (type.IsSubclassOf(typeof(IConvertible)))
            throw new ArgumentException("The type must inherit the IConvertible interface", "type");
        result = null;
        if (type.IsInstanceOfType(value))
        {
            result = value;
            return true;
        }

        if (value is IConvertible convertible)
        {
            if (type.IsEnum)
            {
                result = Enum.ToObject(type, convertible);
                return true;
            }

            var format = NumberFormatInfo.CurrentInfo;
            result = convertible.ToType(type, format);
            return true;
        }

        return false;
    }

    public static object GetFirstFreeValue(Type enumType)
    {
        if (enumType == null) throw new ArgumentNullException("enumType");
        if (!enumType.IsEnum) throw new Exception($"{enumType} is not a valid Enum!");

        var vals = Enum.GetValues(enumType);
        long l = 0;
        for (ulong i = 0; i <= ulong.MaxValue; i++)
        {
            if (!i.TryAsNumber(enumType, out var v))
                break;
            for (; l < vals.Length; l++)
                if (Convert.ToUInt64(vals.GetValue(l)).Equals(Convert.ToUInt32(v)))
                    goto skip;
            return v;
            skip: ;

        }

        for (long i = -1; i >= long.MinValue; i--)
        {
            if (!i.TryAsNumber(enumType, out var v))
                break;
            for (; l < vals.Length; l++)
                if (Convert.ToUInt64(vals.GetValue(l)).Equals(Convert.ToUInt32(v)))
                    goto skip;
            return v;
            skip: ;
        }

        throw new Exception("No unused values in enum " + enumType.FullName);
    }

    public static TEnum GetFirstFreeValue<TEnum>() => (TEnum)GetFirstFreeValue(typeof(TEnum));

    public static void AddEnumValue(Type enumType, object value, string name)
    {
        if (enumType == null) throw new ArgumentNullException("enumType");
        if (!enumType.IsEnum) throw new Exception($"{enumType} is not a valid Enum!");
        if (AlreadyHasName(enumType, name) || EnumEUtil.HasEnumValue(enumType, name))
            throw new Exception($"The enum ({enumType.FullName}) already has a value with the name \"{name}\"");

        value = (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);
        if (!_patches.TryGetValue(enumType, out var patch))
        {
            patch = new EnumPatch();
            _patches.Add(enumType, patch);
        }

        var from = Il2CppType.From(enumType, false);
        if (from != null)
        {
            if (!_il2CpPpatches.TryGetValue(from.GetHashCode(), out var Il2CPPpatch))
            {
                Il2CPPpatch = new EnumPatch();
                _il2CpPpatches.Add(from.GetHashCode(), Il2CPPpatch);
            }

            Il2CPPpatch.AddValue((ulong)value, name);
        }

        patch.AddValue((ulong)value, name);
    }

    public static void AddEnumValue<T>(object value, string name) => AddEnumValue(typeof(T), value, name);

    public static object AddEnumValue(Type enumType, string name)
    {
        var newVal = GetFirstFreeValue(enumType);
        AddEnumValue(enumType, newVal, name);
        return newVal;
    }

    internal static bool AlreadyHasName(Type enumType, string name)
    {
        if (TryGetRawPatch(enumType, out EnumPatch patch))
            return patch.HasName(name);
        return false;
    }

    internal static bool TryGetRawPatch(Type enumType, out EnumPatch patch)
    {
        return _patches.TryGetValue(enumType, out patch);
    }

    internal static bool TryGetRawPatchInIL2CPP(Il2CppSystem.Type enumType, out EnumPatch patch)
    {
        return _il2CpPpatches.TryGetValue(enumType.GetHashCode(), out patch);
    }

    public class EnumPatch
    {
        private Dictionary<ulong, List<string>> _values = new ();

        public void AddValue(ulong enumValue, string name)
        {
            if (_values.ContainsKey(enumValue))
                _values[enumValue].Add(name);
            else
                _values.Add(enumValue, new List<string> { name });
        }

        public List<KeyValuePair<ulong, string>> GetPairs()
        {
            return (from pair in _values
                from value in pair.Value
                select new KeyValuePair<ulong, string>(pair.Key, value)).ToList();
        }

        public bool HasName(string name)
        {
            return this._values.Values.SelectMany(l => l).Any(enumName => name.Equals(enumName));
        }
    }
}