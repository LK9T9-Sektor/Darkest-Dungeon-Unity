using System.IO;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Save;

namespace Sektor.DarkestDungeon.Core.Save.Tests
{
    [TestFixture]
    public class SaveCodecTests
    {
        private sealed class TestItem : IBinarySaveData
        {
            public string Id { get; set; }
            public int Value { get; set; }
            public bool Skip { get; set; }

            public bool IsMeetingSaveCriteria { get { return !Skip; } }

            public void Write(BinaryWriter bw)
            {
                bw.Write(Id);
                bw.Write(Value);
            }

            public void Read(BinaryReader br)
            {
                Id = br.ReadString();
                Value = br.ReadInt32();
            }
        }

        private static TestItem Item(string id, int value, bool skip = false)
        {
            return new TestItem { Id = id, Value = value, Skip = skip };
        }

        private static byte[] Write(System.Action<BinaryWriter> write)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    write(bw);
                }
                return ms.ToArray();
            }
        }

        private static void Read(byte[] data, System.Action<BinaryReader> read)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    read(br);
                }
            }
        }

        private static T ReadValue<T>(byte[] data, System.Func<BinaryReader, T> read)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    return read(br);
                }
            }
        }

        private static TestItem ItemFactory(BinaryReader br)
        {
            var item = new TestItem();
            item.Read(br);
            return item;
        }

        [Test]
        public void Version_RoundTrips()
        {
            byte[] data = Write(bw => SaveCodec.WriteVersion(bw));
            bool ok = ReadValue(data, br => SaveCodec.ReadVersion(br));
            Assert.That(ok, Is.True);
        }

        [Test]
        public void Version_WrongValueIsRejected()
        {
            byte[] data = Write(bw => bw.Write("999"));
            bool ok = ReadValue(data, br => SaveCodec.ReadVersion(br));
            Assert.That(ok, Is.False);
        }

        [Test]
        public void WriteList_FiltersOutSkippedItems()
        {
            byte[] data = Write(bw => SaveCodec.WriteList(
                new System.Collections.Generic.List<TestItem>
                {
                    Item("a", 1),
                    Item("b", 2, skip: true),
                    Item("c", 3),
                }, bw));

            var loaded = new System.Collections.Generic.List<TestItem>();
            Read(data, br => SaveCodec.ReadList(loaded, br, ItemFactory));

            Assert.That(loaded.Count, Is.EqualTo(2));
            Assert.That(loaded.Select(i => i.Id), Is.EqualTo(new[] { "a", "c" }));
            Assert.That(loaded[0].Value, Is.EqualTo(1));
            Assert.That(loaded[1].Value, Is.EqualTo(3));
        }

        [Test]
        public void WriteList_RoundTrips()
        {
            byte[] data = Write(bw => SaveCodec.WriteList(
                new System.Collections.Generic.List<TestItem> { Item("x", 5), Item("y", 7) }, bw));

            var loaded = new System.Collections.Generic.List<TestItem>();
            Read(data, br => SaveCodec.ReadList(loaded, br, ItemFactory));

            Assert.That(loaded.Count, Is.EqualTo(2));
            Assert.That(loaded[1].Id, Is.EqualTo("y"));
            Assert.That(loaded[1].Value, Is.EqualTo(7));
        }

        [Test]
        public void WriteListList_RoundTrips()
        {
            var nested = new System.Collections.Generic.List<System.Collections.Generic.List<TestItem>>
            {
                new System.Collections.Generic.List<TestItem> { Item("a", 1) },
                new System.Collections.Generic.List<TestItem> { Item("b", 2), Item("c", 3) },
            };
            byte[] data = Write(bw => SaveCodec.WriteListList(nested, bw));

            var loaded = new System.Collections.Generic.List<System.Collections.Generic.List<TestItem>>();
            Read(data, br => SaveCodec.ReadListList(loaded, br, ItemFactory));

            Assert.That(loaded.Count, Is.EqualTo(2));
            Assert.That(loaded[1].Count, Is.EqualTo(2));
            Assert.That(loaded[1][1].Id, Is.EqualTo("c"));
        }

        [Test]
        public void WriteDictionary_RoundTrips()
        {
            var dict = new System.Collections.Generic.Dictionary<string, TestItem>
            {
                { "one", Item("one", 1) },
                { "two", Item("two", 2) },
            };
            byte[] data = Write(bw => SaveCodec.WriteDictionary(dict, bw));

            var loaded = new System.Collections.Generic.Dictionary<string, TestItem>();
            Read(data, br => SaveCodec.ReadDictionary(loaded, br, ItemFactory, item => item.Id));

            Assert.That(loaded.Count, Is.EqualTo(2));
            Assert.That(loaded["two"].Value, Is.EqualTo(2));
        }

        [Test]
        public void WriteInstancedDictionary_RoundTrips()
        {
            var instanced = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, TestItem>>
            {
                { 5, new System.Collections.Generic.Dictionary<string, TestItem> { { "a", Item("a", 1) } } },
                { 6, new System.Collections.Generic.Dictionary<string, TestItem> { { "b", Item("b", 2) } } },
            };
            byte[] data = Write(bw => SaveCodec.WriteInstancedDictionary(instanced, bw));

            var loaded = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, TestItem>>();
            Read(data, br => SaveCodec.ReadInstancedDictionary(loaded, br, ItemFactory, item => item.Id));

            Assert.That(loaded.Count, Is.EqualTo(2));
            Assert.That(loaded[6]["b"].Value, Is.EqualTo(2));
        }

        [Test]
        public void WriteStringIntDictionary_RoundTrips()
        {
            byte[] data = Write(bw => SaveCodec.WriteStringIntDictionary(
                new System.Collections.Generic.Dictionary<string, int> { { "a", 1 }, { "b", 2 } }, bw));

            var loaded = new System.Collections.Generic.Dictionary<string, int>();
            Read(data, br => SaveCodec.ReadStringIntDictionary(loaded, br));

            Assert.That(loaded["b"], Is.EqualTo(2));
        }

        [Test]
        public void WriteIntList_RoundTrips()
        {
            byte[] data = Write(bw => SaveCodec.WriteIntList(
                new System.Collections.Generic.List<int> { 3, 1, 4 }, bw));

            var loaded = new System.Collections.Generic.List<int>();
            Read(data, br => SaveCodec.ReadIntList(loaded, br));

            CollectionAssert.AreEqual(new[] { 3, 1, 4 }, loaded);
        }

        [Test]
        public void WriteStringList_NormalizesNullEntries()
        {
            byte[] data = Write(bw => SaveCodec.WriteStringList(
                new System.Collections.Generic.List<string> { "a", null, "b" }, bw));

            var loaded = new System.Collections.Generic.List<string>();
            Read(data, br => SaveCodec.ReadStringList(loaded, br));

            CollectionAssert.AreEqual(new[] { "a", "", "b" }, loaded);
        }

        [Test]
        public void WriteBoolList_RoundTrips()
        {
            byte[] data = Write(bw => SaveCodec.WriteBoolList(
                new System.Collections.Generic.List<bool> { true, false, true }, bw));

            var loaded = new System.Collections.Generic.List<bool>();
            Read(data, br => SaveCodec.ReadBoolList(loaded, br));

            CollectionAssert.AreEqual(new[] { true, false, true }, loaded);
        }
    }
}