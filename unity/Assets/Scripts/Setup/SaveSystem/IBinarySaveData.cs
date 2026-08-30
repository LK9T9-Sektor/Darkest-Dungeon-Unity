using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sektor.DarkestDungeon.Core.Raid;
using Sektor.DarkestDungeon.Core.Save;

public static class BinarySaveDataHelper
{
    public static void Write<T>(this List<T> binaryDataList, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        SaveCodec.WriteList(binaryDataList, bw);
    }

    public static void Read<T>(this List<T> binaryDataList, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        SaveCodec.ReadList(binaryDataList, br, Create<T>);
    }

    public static void Write<T>(this List<List<T>> binaryDataLists, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        SaveCodec.WriteListList(binaryDataLists, bw);
    }

    public static void Read<T>(this List<List<T>> binaryDataLists, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        SaveCodec.ReadListList(binaryDataLists, br, Create<T>);
    }

    public static void Write<T>(this Dictionary<string, T> binaryDataDictionary, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        SaveCodec.WriteDictionary(binaryDataDictionary, bw);
    }

    public static void Read<T>(this Dictionary<string, T> binaryDataDictionary, Func<T, string> keySelector, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        SaveCodec.ReadDictionary(binaryDataDictionary, br, Create<T>, keySelector);
    }

    public static void Write<T>(this Dictionary<int, Dictionary<string, T>> instancedDictionary, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        SaveCodec.WriteInstancedDictionary(instancedDictionary, bw);
    }

    public static void Read<T>(this Dictionary<int, Dictionary<string, T>> instancedDictionary, Func<T, string> keySelector, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        SaveCodec.ReadInstancedDictionary(instancedDictionary, br, Create<T>, keySelector);
    }

    public static void Write(this Dictionary<string, int> binaryDataDictionary, BinaryWriter bw)
    {
        SaveCodec.WriteStringIntDictionary(binaryDataDictionary, bw);
    }

    public static void Read(this Dictionary<string, int> binaryDataDictionary, BinaryReader br)
    {
        SaveCodec.ReadStringIntDictionary(binaryDataDictionary, br);
    }

    public static void Write(this List<int> binaryDataList, BinaryWriter bw)
    {
        SaveCodec.WriteIntList(binaryDataList, bw);
    }

    public static void Read(this List<int> binaryDataList, BinaryReader br)
    {
        SaveCodec.ReadIntList(binaryDataList, br);
    }

    public static void Write(this List<string> binaryDataList, BinaryWriter bw)
    {
        SaveCodec.WriteStringList(binaryDataList, bw);
    }

    public static void Read(this List<string> binaryDataList, BinaryReader br)
    {
        SaveCodec.ReadStringList(binaryDataList, br);
    }

    public static void Write(this List<bool> binaryDataList, BinaryWriter bw)
    {
        SaveCodec.WriteBoolList(binaryDataList, bw);
    }

    public static void Read(this List<bool> binaryDataList, BinaryReader br)
    {
        SaveCodec.ReadBoolList(binaryDataList, br);
    }

    public static T Create<T>(BinaryReader br) where T : class, IBinarySaveData
    {
        var saveDataType = typeof(T);
        T newBinaryData = null;

        if (typeof(Quest).IsAssignableFrom(saveDataType))
        {
            string plotGenId = br.ReadString();
            if (plotGenId == "tutorial")
                newBinaryData = new PlotQuest(plotGenId, new PlotTrinketReward { Amount = 0, Rarity = "very_common" }) as T;
            else if (plotGenId != "")
                newBinaryData = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(plQuest => plQuest.Id == plotGenId).Copy() as T;
            else
                newBinaryData = new Quest() as T;
        }
        else if (typeof(Prop).IsAssignableFrom(saveDataType))
        {
            AreaType propType = (AreaType)br.ReadInt32();

            switch (propType)
            {
                case AreaType.Door:
                    newBinaryData = new Door() as T;
                    break;
                case AreaType.Curio:
                    bool isQuestCurio = br.ReadBoolean();
                    string curioName = br.ReadString();

                    if (isQuestCurio)
                        newBinaryData = new Curio { IsQuestCurio = true, StringId = curioName} as T;
                    else
                        newBinaryData = DarkestDungeonManager.Data.Curios[curioName] as T;
                    break;
                case AreaType.Obstacle:
                    newBinaryData = DarkestDungeonManager.Data.Obstacles[br.ReadString()] as T;
                    break;
                case AreaType.Trap:
                    newBinaryData = DarkestDungeonManager.Data.Traps[br.ReadString()] as T;
                    break;
            }
        }

        if (newBinaryData == null)
            newBinaryData = Activator.CreateInstance<T>();

        newBinaryData.Read(br);
        return newBinaryData;
    }
}