using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// One expression's materials
/// </summary>
/// <param name="Expression"> Which expression this entry is for </param>
/// <param name="Eyes"> The eye material, or null if the entry has none </param>
/// <param name="Mouth"> The mouth material, or null if the entry has none </param>
public readonly record struct FaceEntry(SlimeFace.SlimeExpression Expression, Material? Eyes, Material? Mouth);

// I know this is (one of the) the most hacky way(s) imaginable :P
// The interop exposes the array as an Il2CppReferenceArray<SlimeExpressionFace>,
// but SlimeExpressionFace is a struct that the game stores inline in the array, one entry after another
// A reference array reads each slot as an 8 byte object pointer, so indexing it would read the wrong memory
// That is why this goes through the raw memory instead, with the entry size and field offsets read from IL2CPP at runtime

/// <summary>
/// Reads and writes <see cref="SlimeFace.ExpressionFaces"/>
/// </summary>
public static class FaceExpressionTable
{
    /// <summary>
    /// Offset from an il2cpp array object to its first element
    /// </summary>
    private const int ArrayDataOffset = 0x20;

    /// <summary>
    /// Size of an il2cpp object header
    /// </summary>
    private const int ObjectHeaderSize = 0x10;

    /// <summary>
    /// Where the parts of one entry sit
    /// </summary>
    private readonly record struct Layout(int Stride, int ExpressionOffset, int EyesOffset, int MouthOffset);

    private static Layout? _layout;
    private static bool layoutFailed;

    /// <summary>
    /// The number of expressions the face defines, or 0 if it cannot be read
    /// </summary>
    public static int Count(SlimeFace face)
    {
        ArgumentNullException.ThrowIfNull(face);

        var array = face.ExpressionFaces;
        
        return array == null ? 0 : array.Length;
    }

    /// <summary>
    /// Reads an entry
    /// </summary>
    /// <returns> False if the layout could not be established </returns>
    public static bool TryGet(SlimeFace face, int index, out FaceEntry entry)
    {
        entry = default;

        if (!TryAddress(face, index, out var element, out var layout)) return false;

        entry = new FaceEntry(
            (SlimeFace.SlimeExpression)Marshal.ReadInt32(element + layout.ExpressionOffset),
            Wrap(Marshal.ReadIntPtr(element + layout.EyesOffset)),
            Wrap(Marshal.ReadIntPtr(element + layout.MouthOffset)));

        return true;
    }

    /// <summary>
    /// Replaces one entry's materials
    /// </summary>
    /// <returns> True if the entry was written </returns>
    public static bool TrySetMaterials(SlimeFace face, int index, Material? eyes, Material? mouth)
    {
        if (!TryAddress(face, index, out var element, out var layout)) return false;
        
        var array = face.ExpressionFaces.Pointer;
        IL2CPP.il2cpp_gc_wbarrier_set_field(array, element + layout.EyesOffset, Unwrap(eyes));
        IL2CPP.il2cpp_gc_wbarrier_set_field(array, element + layout.MouthOffset, Unwrap(mouth));

        return true;
    }

    private static Material? Wrap(IntPtr pointer) => pointer == IntPtr.Zero ? null : new Material(pointer);

    private static IntPtr Unwrap(Material? material) => material == null ? IntPtr.Zero : material.Pointer;

    /// <summary>
    /// Resolves the address of one entry
    /// </summary>
    private static bool TryAddress(SlimeFace face, int index, out IntPtr element, out Layout layout)
    {
        element = IntPtr.Zero;
        layout = default;

        ArgumentNullException.ThrowIfNull(face);

        var array = face.ExpressionFaces;
        if (array == null) return false;

        if (index < 0 || index >= array.Length) return false;

        if (!TryResolveLayout(array.Pointer, out layout)) return false;

        element = array.Pointer + ArrayDataOffset + (index * layout.Stride);
        return true;
    }

    private static bool TryResolveLayout(IntPtr array, out Layout layout)
    {
        if (_layout.HasValue)
        {
            layout = _layout.Value;
            return true;
        }

        layout = default;
        if (layoutFailed) return false;

        try
        {
            var elementClass = IL2CPP.il2cpp_class_get_element_class(IL2CPP.il2cpp_object_get_class(array));
            if (elementClass == IntPtr.Zero) return Fail("couldn't resolve the entry type");

            var stride = IL2CPP.il2cpp_class_array_element_size(elementClass);

            int? expression = null, eyes = null, mouth = null;
            var iterator = IntPtr.Zero;

            while (true)
            {
                var field = IL2CPP.il2cpp_class_get_fields(elementClass, ref iterator);
                if (field == IntPtr.Zero) break;

                var name = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_field_get_name(field));
                var offset = (int)IL2CPP.il2cpp_field_get_offset(field);

                switch (name)
                {
                    case "SlimeExpression": expression = offset; break;
                    case "Eyes": eyes = offset; break;
                    case "Mouth": mouth = offset; break;
                }
            }

            if (expression == null || eyes == null || mouth == null)
                return Fail("the entry type is missing an expected field");
            
            var smallest = Math.Min(expression.Value, Math.Min(eyes.Value, mouth.Value));
            if (smallest >= ObjectHeaderSize)
            {
                expression -= ObjectHeaderSize;
                eyes -= ObjectHeaderSize;
                mouth -= ObjectHeaderSize;
            }
            
            var required = Math.Max(expression.Value, Math.Max(eyes.Value, mouth.Value)) + IntPtr.Size;
            if (stride < required || stride > 0x100)
                return Fail($"entry size {stride} doesn't fit its fields");

            layout = new Layout(stride, expression.Value, eyes.Value, mouth.Value);
            _layout = layout;
            
            return true;
        }
        catch (Exception ex)
        {
            return Fail(ex.ToString());
        }
    }

    private static bool Fail(string reason)
    {
        layoutFailed = true;
        LogError($"Prism: Custom faces disabled, {reason}");
        return false;
    }
}