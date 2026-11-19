namespace Starlight.Saving;

internal enum DataType : byte {
    Null = 0,
    
    Boolean = 1, Byte = 2, SByte = 3, Char = 4,
    Decimal = 5, Double = 6, Float = 7, Integer = 8,
    UInt = 9, Long = 10, ULong = 11, Short = 12,
    UShort = 13, String = 14,
    
    Vector3 = 20, Quaternion = 21,

    Array = 30, List = 31, Dictionary = 32, HashSet = 33,
    Il2CppArray = 40, Il2CppList = 41, Il2CppDictionary = 42, Il2CppHashSet = 43,

    Object = 100
}