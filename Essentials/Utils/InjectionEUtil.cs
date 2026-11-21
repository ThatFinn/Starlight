using System;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;

namespace Starlight.Utils;

/// <summary>
/// Util for classes injected into il2cpp
/// </summary>
public static class InjectionEUtil
{
    /// <summary>
    /// Injects a type into il2cpp & marks it as holding references
    /// </summary>
    public static void Register(Type type, RegisterTypeOptions options)
    {
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(type))
            ClassInjector.RegisterTypeInIl2Cpp(type, options);

        MarkAsHoldingReferences(type);
    }

    /// <summary>
    /// Makes the garbage collector look through the fields of an injected class
    /// </summary>
    public static unsafe void MarkAsHoldingReferences(Type type)
    {
        try
        {
            var pointer = Il2CppClassPointerStore.GetNativeClassPointer(type);
            if (pointer == IntPtr.Zero) return;

            var klass = UnityVersionHandler.Wrap((Il2CppClass*)pointer);
            var flag = klass.GetType().GetProperty("HasReferences", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (flag == null)
            {
                LogWarning($"InjectionEUtil: Failed to mark class '{type.FullName}' as holding references: this il2cpp version has no such flag");
                return;
            }

            if (!(bool)flag.GetValue(klass)!) flag.SetValue(klass, true);
        }
        catch (Exception ex)
        {
            LogError($"InjectionEUtil: Failed to mark class '{type.FullName}' as holding references", ex);
        }
    }
}