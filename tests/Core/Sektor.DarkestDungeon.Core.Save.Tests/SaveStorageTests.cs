using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Save;

namespace Sektor.DarkestDungeon.Core.Save.Tests
{
    [TestFixture]
    public class SaveStorageTests
    {
        private sealed class MemorySaveStorage : ISaveStorage
        {
            private readonly Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

            public string GetSaveFileName(int slotId) { return "save" + slotId; }
            public string GetMapFileName(string mapName) { return "map_" + mapName; }

            public void EnsureSaveDirectory() { }
            public void EnsureMapDirectory() { }

            public bool SaveExists(int slotId) { return files.ContainsKey(GetSaveFileName(slotId)); }
            public void DeleteSave(int slotId) { files.Remove(GetSaveFileName(slotId)); }

            public Stream OpenSaveForWrite(int slotId)
            {
                return new MemoryStream();
            }

            public Stream OpenSaveForRead(int slotId)
            {
                byte[] data;
                return files.TryGetValue(GetSaveFileName(slotId), out data) ? new MemoryStream(data) : null;
            }

            public Stream OpenMapForWrite(string mapName) { return new MemoryStream(); }
            public Stream OpenMapForRead(string mapName) { return null; }

            public void Store(int slotId, byte[] data)
            {
                files[GetSaveFileName(slotId)] = data;
            }
        }

        [Test]
        public void WriteThenRead_RoundTripsThroughStorage()
        {
            var storage = new MemorySaveStorage();

            using (var ms = storage.OpenSaveForWrite(1))
            {
                using (var bw = new BinaryWriter(ms))
                {
                    SaveCodec.WriteVersion(bw);
                    bw.Write(42);
                    bw.Write("hello");
                }
                storage.Store(1, ((MemoryStream)ms).ToArray());
            }

            Assert.That(storage.SaveExists(1), Is.True);

            using (Stream stream = storage.OpenSaveForRead(1))
            {
                Assert.That(stream, Is.Not.Null);
                using (var br = new BinaryReader(stream))
                {
                    Assert.That(SaveCodec.ReadVersion(br), Is.True);
                    Assert.That(br.ReadInt32(), Is.EqualTo(42));
                    Assert.That(br.ReadString(), Is.EqualTo("hello"));
                }
            }
        }

        [Test]
        public void DeleteSave_RemovesSlot()
        {
            var storage = new MemorySaveStorage();
            storage.Store(2, new byte[] { 1, 2, 3 });
            Assert.That(storage.SaveExists(2), Is.True);

            storage.DeleteSave(2);

            Assert.That(storage.SaveExists(2), Is.False);
        }

        [Test]
        public void OpenSaveForRead_MissingSlotReturnsNull()
        {
            var storage = new MemorySaveStorage();
            Assert.That(storage.OpenSaveForRead(99), Is.Null);
        }
    }
}