using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sektor.DarkestDungeon.Core.Save
{
    /// <summary>
    /// Binary save codec: version header and collection serializers shared by the save DTOs.
    /// Pure logic (no Unity references); item creation for concrete domain types is injected
    /// through factory callbacks so the codec stays engine-free.
    /// </summary>
    public static class SaveCodec
    {
        /// <summary>Writes the current format version string.</summary>
        /// <param name="bw">The binary writer receiving the version.</param>
        public static void WriteVersion(BinaryWriter bw)
        {
            bw.Write(SaveVersion.Current);
        }

        /// <summary>Reads the format version and reports whether it matches the current one.</summary>
        /// <param name="br">The binary reader providing the version.</param>
        /// <returns>True when the stored version equals <see cref="SaveVersion.Current"/>.</returns>
        public static bool ReadVersion(BinaryReader br)
        {
            return br.ReadString() == SaveVersion.Current;
        }

        /// <summary>Writes a list of serializable items, skipping those that must not be saved.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="items">The items to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteList<T>(List<T> items, BinaryWriter bw)
            where T : class, IBinarySaveData, new()
        {
            List<T> toSave = items.FindAll(item => item.IsMeetingSaveCriteria);
            bw.Write(toSave.Count);
            for (int i = 0; i < toSave.Count; i++)
                toSave[i].Write(bw);
        }

        /// <summary>Reads a list of serializable items.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="target">The list that receives the loaded items.</param>
        /// <param name="br">The binary reader providing the data.</param>
        /// <param name="factory">Creates a new item and reads its state from the reader.</param>
        public static void ReadList<T>(List<T> target, BinaryReader br, Func<BinaryReader, T> factory)
            where T : class, IBinarySaveData
        {
            target.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                target.Add(factory(br));
        }

        /// <summary>Writes a nested list of serializable items.</summary>
        /// <typeparam name="T">The inner item type.</typeparam>
        /// <param name="lists">The nested lists to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteListList<T>(List<List<T>> lists, BinaryWriter bw)
            where T : class, IBinarySaveData, new()
        {
            bw.Write(lists.Count);
            for (int i = 0; i < lists.Count; i++)
                WriteList(lists[i], bw);
        }

        /// <summary>Reads a nested list of serializable items.</summary>
        /// <typeparam name="T">The inner item type.</typeparam>
        /// <param name="target">The outer list that receives the loaded inner lists.</param>
        /// <param name="br">The binary reader providing the data.</param>
        /// <param name="factory">Creates a new empty item for reading.</param>
        public static void ReadListList<T>(List<List<T>> target, BinaryReader br, Func<BinaryReader, T> factory)
            where T : class, IBinarySaveData
        {
            target.Clear();
            int listCount = br.ReadInt32();
            for (int i = 0; i < listCount; i++)
            {
                var inner = new List<T>();
                ReadList(inner, br, factory);
                target.Add(inner);
            }
        }

        /// <summary>Writes a string-keyed dictionary of serializable items.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="items">The dictionary to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteDictionary<T>(Dictionary<string, T> items, BinaryWriter bw)
            where T : class, IBinarySaveData, new()
        {
            bw.Write(items.Count(item => item.Value.IsMeetingSaveCriteria));
            foreach (KeyValuePair<string, T> entry in items)
            {
                if (entry.Value.IsMeetingSaveCriteria)
                    entry.Value.Write(bw);
            }
        }

        /// <summary>Reads a string-keyed dictionary of serializable items.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="target">The dictionary that receives the loaded entries.</param>
        /// <param name="br">The binary reader providing the data.</param>
        /// <param name="factory">Creates a new empty item for reading.</param>
        /// <param name="keySelector">Extracts the dictionary key from a loaded item.</param>
        public static void ReadDictionary<T>(Dictionary<string, T> target, BinaryReader br, Func<BinaryReader, T> factory, Func<T, string> keySelector)
            where T : class, IBinarySaveData
        {
            target.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                T item = factory(br);
                target.Add(keySelector(item), item);
            }
        }

        /// <summary>Writes an instance-keyed dictionary of string-keyed item dictionaries.</summary>
        /// <typeparam name="T">The innermost value type.</typeparam>
        /// <param name="instances">The nested dictionary to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteInstancedDictionary<T>(Dictionary<int, Dictionary<string, T>> instances, BinaryWriter bw)
            where T : class, IBinarySaveData, new()
        {
            bw.Write(instances.Count);
            foreach (KeyValuePair<int, Dictionary<string, T>> entry in instances)
            {
                bw.Write(entry.Key);
                WriteDictionary(entry.Value, bw);
            }
        }

        /// <summary>Reads an instance-keyed dictionary of string-keyed item dictionaries.</summary>
        /// <typeparam name="T">The innermost value type.</typeparam>
        /// <param name="target">The nested dictionary that receives the loaded entries.</param>
        /// <param name="br">The binary reader providing the data.</param>
        /// <param name="factory">Creates a new empty item for reading.</param>
        /// <param name="keySelector">Extracts the dictionary key from a loaded item.</param>
        public static void ReadInstancedDictionary<T>(Dictionary<int, Dictionary<string, T>> target, BinaryReader br, Func<BinaryReader, T> factory, Func<T, string> keySelector)
            where T : class, IBinarySaveData
        {
            target.Clear();
            int instanceCount = br.ReadInt32();
            for (int i = 0; i < instanceCount; i++)
            {
                var inner = new Dictionary<string, T>();
                int instanceId = br.ReadInt32();
                ReadDictionary(inner, br, factory, keySelector);
                target.Add(instanceId, inner);
            }
        }

        /// <summary>Writes a string-to-int dictionary.</summary>
        /// <param name="items">The dictionary to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteStringIntDictionary(Dictionary<string, int> items, BinaryWriter bw)
        {
            bw.Write(items.Count);
            foreach (KeyValuePair<string, int> entry in items)
            {
                bw.Write(entry.Key);
                bw.Write(entry.Value);
            }
        }

        /// <summary>Reads a string-to-int dictionary.</summary>
        /// <param name="target">The dictionary that receives the loaded entries.</param>
        /// <param name="br">The binary reader providing the data.</param>
        public static void ReadStringIntDictionary(Dictionary<string, int> target, BinaryReader br)
        {
            target.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                target.Add(br.ReadString(), br.ReadInt32());
        }

        /// <summary>Writes a list of integers.</summary>
        /// <param name="items">The items to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteIntList(List<int> items, BinaryWriter bw)
        {
            bw.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
                bw.Write(items[i]);
        }

        /// <summary>Reads a list of integers.</summary>
        /// <param name="target">The list that receives the loaded items.</param>
        /// <param name="br">The binary reader providing the data.</param>
        public static void ReadIntList(List<int> target, BinaryReader br)
        {
            target.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                target.Add(br.ReadInt32());
        }

        /// <summary>Writes a list of strings, normalizing null entries to empty.</summary>
        /// <param name="items">The items to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteStringList(List<string> items, BinaryWriter bw)
        {
            bw.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
                bw.Write(items[i] ?? string.Empty);
        }

        /// <summary>Reads a list of strings.</summary>
        /// <param name="target">The list that receives the loaded items.</param>
        /// <param name="br">The binary reader providing the data.</param>
        public static void ReadStringList(List<string> target, BinaryReader br)
        {
            target.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                target.Add(br.ReadString());
        }

        /// <summary>Writes a list of booleans.</summary>
        /// <param name="items">The items to write.</param>
        /// <param name="bw">The binary writer receiving the data.</param>
        public static void WriteBoolList(List<bool> items, BinaryWriter bw)
        {
            bw.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
                bw.Write(items[i]);
        }

        /// <summary>Reads a list of booleans.</summary>
        /// <param name="target">The list that receives the loaded items.</param>
        /// <param name="br">The binary reader providing the data.</param>
        public static void ReadBoolList(List<bool> target, BinaryReader br)
        {
            target.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                target.Add(br.ReadBoolean());
        }
    }
}