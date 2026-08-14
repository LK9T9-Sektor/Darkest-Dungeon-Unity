using System.IO;
using Sektor.DarkestDungeon.Core.Content.Save;

public abstract class ActivityRecord : IBinarySaveData
{
    public bool IsMeetingSaveCriteria { get { return true; } }

    public virtual void Write(BinaryWriter bw)
    {
    }

    public virtual void Read(BinaryReader br)
    {
    }
}
