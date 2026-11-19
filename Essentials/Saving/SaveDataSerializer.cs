using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Starlight.Saving;

internal static class SaveDataSerializer
{
    static Dictionary<Type, FieldInfo[]> _cache = new();

    internal static byte[] Serialize(RootSave root) {
        try {
            var stringTable = new List<string>();
            
            using var payloadMs = new MemoryStream();
            using (var dataWriter = new BinaryWriter(payloadMs)) {
                WriteObject(dataWriter, root, stringTable);
            }

            var payloadBytes = payloadMs.ToArray();
            var checksum = CalculateChecksum(payloadBytes);

            using var finalMs = new MemoryStream();
            using (var w = new BinaryWriter(finalMs)) {
                // 1. Write String Table
                w.Write((ushort)stringTable.Count);
                foreach (var s in stringTable) w.Write(s);
                
                // 2. Write Checksum
                w.Write(checksum);

                // 3. Write Compressed Payload
                using var compressionStream = new GZipStream(finalMs, System.IO.Compression.CompressionLevel.Optimal, true);
                compressionStream.Write(payloadBytes, 0, payloadBytes.Length);
            }
            
            return finalMs.ToArray();
        }
        catch (Exception e) {
            LogError($"Failed to serialize save data (Aborting save): {e}");
            return null;
        }
    }

    internal static T Deserialize<T>(byte[] data) where T : RootSave {
        return Deserialize(data, typeof(T)) as T;
    }

    private static RootSave Deserialize(byte[] data, Type rootType) {
        var loadableObjects = new List<StarlightSaveableBase>();

        try {
            using var ms = new MemoryStream(data);
            using var r = new BinaryReader(ms);
            
            // 1. Read String Table
            var count = r.ReadUInt16();
            var table = new string[count];
            for (var i = 0; i < count; i++) table[i] = r.ReadString();
            
            // 2. Read Checksum
            var storedChecksum = r.ReadUInt32();

            // 3. Decompress Payload
            var decompressedMs = new MemoryStream();
            using (var decompressionStream = new GZipStream(ms, CompressionMode.Decompress)) {
                decompressionStream.CopyTo(decompressedMs);
            }
            
            var payloadBytes = decompressedMs.ToArray();
            if (storedChecksum != CalculateChecksum(payloadBytes)) {
                LogError("Save data corruption detected! Checksum mismatch.");
                return null;
            }

            // 4. Start reading objects
            decompressedMs.Position = 0;
            using var payloadReader = new BinaryReader(decompressedMs);
            
            // Pass the loadableObjects list to collect items during recursion
            var result = ReadObject(payloadReader, table, loadableObjects) as RootSave;

            foreach (var saveable in loadableObjects) {
                try {
                    saveable.OnLoad();
                } catch (Exception e) {
                    LogError($"Error in OnLoad for {saveable.GetType().Name}: {e}");
                }
            }

            return result;
        }
        catch (Exception e) {
            LogError($"Failed to deserialize save data (Aborting load): {e}");
            return null;
        }
    }
    
    static void WriteObject(BinaryWriter w, object obj, List<string> strings) {
        if (obj == null) { w.Write((byte)DataType.Null); return; }
        if (obj is StarlightSaveableBase s) try { s.OnSave(); } catch (Exception e) { LogError($"Error in OSave for {s.GetType().Name}: {e}"); }

        var t = obj.GetType();

        // Primitives
        if (t == typeof(bool)) { w.Write((byte)DataType.Boolean); w.Write((bool)obj); }
        else if (t == typeof(byte)) { w.Write((byte)DataType.Byte); w.Write((byte)obj); }
        else if (t == typeof(sbyte)) { w.Write((byte)DataType.SByte); w.Write((sbyte)obj); }
        else if (t == typeof(char)) { w.Write((byte)DataType.Char); w.Write((char)obj); }
        else if (t == typeof(decimal)) { w.Write((byte)DataType.Decimal); w.Write((decimal)obj); }
        else if (t == typeof(double)) { w.Write((byte)DataType.Double); w.Write((double)obj); }
        else if (t == typeof(float)) { w.Write((byte)DataType.Float); w.Write((float)obj); }
        else if (t == typeof(int)) { w.Write((byte)DataType.Integer); w.Write((int)obj); }
        else if (t == typeof(uint)) { w.Write((byte)DataType.UInt); w.Write((uint)obj); }
        else if (t == typeof(long)) { w.Write((byte)DataType.Long); w.Write((long)obj); }
        else if (t == typeof(ulong)) { w.Write((byte)DataType.ULong); w.Write((ulong)obj); }
        else if (t == typeof(short)) { w.Write((byte)DataType.Short); w.Write((short)obj); }
        else if (t == typeof(ushort)) { w.Write((byte)DataType.UShort); w.Write((ushort)obj); }
        else if (t == typeof(string)) { w.Write((byte)DataType.String); w.Write((string)obj); }

        // Unity
        else if (t == typeof(Vector3)) {
            w.Write((byte)DataType.Vector3);
            var v = (Vector3)obj;
            w.Write(v.x); w.Write(v.y); w.Write(v.z);
        }
        else if (t == typeof(Quaternion)) {
            w.Write((byte)DataType.Quaternion);
            var q = (Quaternion)obj;
            w.Write(q.x); w.Write(q.y); w.Write(q.z); w.Write(q.w);
        }

        // DotNet Collections
        else if (t.IsArray) {
            w.Write((byte)DataType.Array);
            var arr = (Array)obj;
            w.Write(arr.Length);
            w.Write(t.GetElementType()?.AssemblyQualifiedName);
            foreach (var item in arr) WriteObject(w, item, strings);
        }
        else if (IsGenericType(t, typeof(List<>))) {
            w.Write((byte)DataType.List);
            var list = (IList)obj;
            w.Write(list.Count);
            w.Write(t.GetGenericArguments()[0].AssemblyQualifiedName);
            foreach (var item in list) WriteObject(w, item, strings);
        }
        else if (IsGenericType(t, typeof(Dictionary<,>))) {
            w.Write((byte)DataType.Dictionary);
            var dict = (IDictionary)obj;
            w.Write(dict.Count);
            var args = t.GetGenericArguments();
            w.Write(args[0].AssemblyQualifiedName);
            w.Write(args[1].AssemblyQualifiedName);
            foreach (DictionaryEntry kvp in dict) {
                WriteObject(w, kvp.Key, strings);
                WriteObject(w, kvp.Value, strings);
            }
        }
        else if (IsGenericType(t, typeof(HashSet<>))) {
            w.Write((byte)DataType.HashSet);
            dynamic set = obj;
            w.Write((int)set.Count);
            w.Write(t.GetGenericArguments()[0].AssemblyQualifiedName);
            foreach (var item in set) WriteObject(w, item, strings);
        }

        // Il2Cpp Collections
        else if (IsGenericType(t, typeof(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<>))) {
            w.Write((byte)DataType.Il2CppArray);
            dynamic ilArr = obj;
            var netArr = MiscEUtil.ToNetArray(ilArr);
            w.Write(netArr.Length);
            w.Write(netArr.GetType().GetElementType().AssemblyQualifiedName);
            foreach (var item in netArr) WriteObject(w, item, strings);
        }
        else if (IsGenericType(t, typeof(Il2CppSystem.Collections.Generic.List<>))) {
            w.Write((byte)DataType.Il2CppList);
            dynamic ilList = obj;
            var netList = MiscEUtil.ToNetList(ilList);
            w.Write(netList.Count);
            w.Write(t.GetGenericArguments()[0].AssemblyQualifiedName);
            foreach (var item in netList) WriteObject(w, item, strings);
        }
        else if (IsGenericType(t, typeof(Il2CppSystem.Collections.Generic.Dictionary<,>))) {
            w.Write((byte)DataType.Il2CppDictionary);
            dynamic ilDict = obj;
            var netDict = MiscEUtil.ToNetDictionary(ilDict);
            w.Write(netDict.Count);
            var args = t.GetGenericArguments();
            w.Write(args[0].AssemblyQualifiedName);
            w.Write(args[1].AssemblyQualifiedName);
            foreach (var kvp in netDict) {
                WriteObject(w, kvp.Key, strings);
                WriteObject(w, kvp.Value, strings);
            }
        }
        else if (IsGenericType(t, typeof(Il2CppSystem.Collections.Generic.HashSet<>))) {
            w.Write((byte)DataType.Il2CppHashSet);
            dynamic ilSet = obj;
            var netSet = MiscEUtil.ToNetHashSet(ilSet);
            w.Write((int)netSet.Count);
            w.Write(t.GetGenericArguments()[0].AssemblyQualifiedName);
            foreach (var item in netSet) WriteObject(w, item, strings);
        }

        // Recursive Objects
        else {
            w.Write((byte)DataType.Object);
            w.Write(t.AssemblyQualifiedName);
            var fields = GetFields(t);
            w.Write((ushort)fields.Length);
            foreach (var f in fields) {
                var idx = strings.IndexOf(f.Name);
                if (idx == -1) { idx = strings.Count; strings.Add(f.Name); }
                w.Write((ushort)idx);
                WriteObject(w, f.GetValue(obj), strings);
            }
        }
    }

    static object ReadObject(BinaryReader r, string[] table, List<StarlightSaveableBase> onLoadList) {
        var type = (DataType)r.ReadByte();
        
        switch (type) {
            case DataType.Null: return null;
            case DataType.Boolean: return r.ReadBoolean();
            case DataType.Byte: return r.ReadByte();
            case DataType.SByte: return r.ReadSByte();
            case DataType.Char: return r.ReadChar();
            case DataType.Decimal: return r.ReadDecimal();
            case DataType.Double: return r.ReadDouble();
            case DataType.Float: return r.ReadSingle();
            case DataType.Integer: return r.ReadInt32();
            case DataType.UInt: return r.ReadUInt32();
            case DataType.Long: return r.ReadInt64();
            case DataType.ULong: return r.ReadUInt64();
            case DataType.Short: return r.ReadInt16();
            case DataType.UShort: return r.ReadUInt16();
            case DataType.String: return r.ReadString();
                
            case DataType.Vector3: return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            case DataType.Quaternion: return new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

            //Dotnet
            case DataType.Array: {
                var len = r.ReadInt32();
                var typeName = r.ReadString();
                var eType = Type.GetType(typeName);
                Array arr = null;
                if (eType != null) arr = Array.CreateInstance(eType, len);
                for (var i = 0; i < len; i++) {
                    var val = ReadObject(r, table, onLoadList);
                    if (arr != null) arr.SetValue(val, i);
                }
                return arr;
            }
            case DataType.List: {
                var count = r.ReadInt32();
                var typeName = r.ReadString();
                var eType = Type.GetType(typeName);
                IList list = null;
                if (eType != null) {
                    var netType = typeof(List<>).MakeGenericType(eType);
                    list = (IList)Activator.CreateInstance(netType);
                }
                for (var i = 0; i < count; i++) {
                    var val = ReadObject(r, table, onLoadList);
                    if (list != null) list.Add(val);
                }
                return list;
            }
            case DataType.Dictionary: {
                var count = r.ReadInt32();
                var kName = r.ReadString();
                var vName = r.ReadString();
                var kType = Type.GetType(kName);
                var vType = Type.GetType(vName);
                IDictionary dict = null;
                if (kType != null && vType != null) {
                    var netType = typeof(Dictionary<,>).MakeGenericType(kType, vType);
                    dict = (IDictionary)Activator.CreateInstance(netType);
                }
                for (var i = 0; i < count; i++) {
                    var key = ReadObject(r, table, onLoadList);
                    var val = ReadObject(r, table, onLoadList);
                    if (dict != null) dict.Add(key, val);
                }
                return dict;
            }
            case DataType.HashSet: {
                var count = r.ReadInt32();
                var typeName = r.ReadString();
                var eType = Type.GetType(typeName);
                dynamic set = null;
                if (eType != null) {
                    var netType = typeof(HashSet<>).MakeGenericType(eType);
                    set = Activator.CreateInstance(netType);
                }
                for (var i = 0; i < count; i++) {
                    var val = ReadObject(r, table, onLoadList);
                    if (set != null) set.Add((dynamic)val);
                }
                return set;
            }
                
            //Il2Cpp
            case DataType.Il2CppArray: {
                var len = r.ReadInt32();
                var typeName = r.ReadString();
                var eType = Type.GetType(typeName);
                dynamic netArr = null;
                if (eType != null) netArr = Array.CreateInstance(eType, len);
                for (var i = 0; i < len; i++) {
                    var val = ReadObject(r, table, onLoadList);
                    if (netArr != null) netArr.SetValue(val, i);
                }
                if (netArr == null) return null;
                return MiscEUtil.ToIl2CppArray(netArr);
            }
            case DataType.Il2CppList: {
                var count = r.ReadInt32();
                var typeName = r.ReadString();
                var eType = Type.GetType(typeName);
                dynamic list = null;
                if (eType != null) {
                    var netType = typeof(List<>).MakeGenericType(eType);
                    list = Activator.CreateInstance(netType);
                }
                for (var i = 0; i < count; i++) {
                    var val = ReadObject(r, table, onLoadList);
                    if (list != null) list.Add((dynamic)val);
                }
                if (list == null) return null;
                return MiscEUtil.ToIl2CppList(list);
            }
            case DataType.Il2CppDictionary: {
                var count = r.ReadInt32();
                var kName = r.ReadString();
                var vName = r.ReadString();
                var kType = Type.GetType(kName);
                var vType = Type.GetType(vName);
                dynamic dict = null;
                if (kType != null && vType != null) {
                    var netType = typeof(Dictionary<,>).MakeGenericType(kType, vType);
                    dict = Activator.CreateInstance(netType);
                }
                for (var i = 0; i < count; i++) {
                    var key = ReadObject(r, table, onLoadList);
                    var val = ReadObject(r, table, onLoadList);
                    if (dict != null) dict.Add((dynamic)key, (dynamic)val);
                }
                if (dict == null) return null;
                return MiscEUtil.ToIl2CppDictionary(dict);
            }
            case DataType.Il2CppHashSet: {
                var count = r.ReadInt32();
                var typeName = r.ReadString();
                var eType = Type.GetType(typeName);
                dynamic set = null;
                if (eType != null) {
                    var netType = typeof(HashSet<>).MakeGenericType(eType);
                    set = Activator.CreateInstance(netType);
                }
                for (var i = 0; i < count; i++) {
                    var val = ReadObject(r, table, onLoadList);
                    if (set != null) set.Add((dynamic)val);
                }
                if (set == null) return null;
                return MiscEUtil.ToIl2CppHashSet(set);
            }

            //Recursive Object
            case DataType.Object: {
                var typeName = r.ReadString();
                var oType = Type.GetType(typeName);
                
                object instance = null;
                if (oType != null) instance = Activator.CreateInstance(oType);

                var fCount = r.ReadUInt16();
                
                Dictionary<string, FieldInfo> fieldDict = null;
                if (oType != null) fieldDict = GetFields(oType).ToDictionary(f => f.Name);
                
                for (var i = 0; i < fCount; i++) {
                    var fName = table[r.ReadUInt16()];
                    var val = ReadObject(r, table, onLoadList);
                    
                    if (instance != null && fieldDict.TryGetValue(fName, out var f)) {
                        if (val != null) {
                            if (!f.FieldType.IsAssignableFrom(val.GetType())) {
                                try {
                                    val = Convert.ChangeType(val, f.FieldType);
                                }catch {
                                    throw new InvalidCastException($"Cannot assign {val.GetType().Name} to field {fName} of type {f.FieldType.Name}");
                                }
                            }
                        }
                        f.SetValue(instance, val);
                    }
                }
                
                if (instance is StarlightSaveableBase s) {
                    onLoadList.Add(s);
                }
                return instance;
            }
            default: throw new Exception($"Unknown DataType encountered: {type}");
        }
    }

    static bool IsGenericType(Type t, Type genericDef) => t.IsGenericType && t.GetGenericTypeDefinition() == genericDef;
    
    static FieldInfo[] GetFields(Type t) {
        if (!_cache.TryGetValue(t, out var validFields)) {
            var allFields = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            var validList = new List<FieldInfo>();

            foreach (var field in allFields)
            {
                if (field.GetCustomAttribute<StoreInSaveAttribute>() == null) continue;
                if (field.IsStatic)
                    throw new Exception($"[SaveSystem] Error on '{t.Name}.{field.Name}': You cannot use [StoreInSave] on static fields.");
                if (field.IsLiteral)
                    throw new Exception($"[SaveSystem] Error on '{t.Name}.{field.Name}': You cannot use [StoreInSave] on const fields.");
                
                validList.Add(field);
            }
            validFields = validList.ToArray();
            _cache[t] = validFields;
        }
        return validFields;
    }

    static uint CalculateChecksum(byte[] data) {
        var crc = 0xFFFFFFFF;
        foreach (var b in data) {
            crc ^= b;
            for (var i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            
        }
        return ~crc;
    }
}