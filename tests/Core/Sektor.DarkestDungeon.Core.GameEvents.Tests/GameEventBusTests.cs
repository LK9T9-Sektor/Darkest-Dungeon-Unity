using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Sektor.DarkestDungeon.Core.GameEvents.Tests
{
    /// <summary>Verifies the event bus: stamping, dispatch rules and unsubscription.</summary>
    [TestFixture]
    public class GameEventBusTests
    {
        /// <summary>Publishing stamps identity and a monotonic sequence while preserving the runtime type.</summary>
        [Test]
        public void Publish_StampsIdentityAndMonotonicSequence()
        {
            var bus = new GameEventBus();
            var received = new List<GameEvent>();
            bus.Subscribe<GameEvent>(received.Add);

            var before = DateTime.UtcNow;
            bus.Publish(new TorchChanged(100, 99, "corridorStep"));
            bus.Publish(new PartyMoved("room1_1", "hallroom2_1_room1_1"));

            Assert.That(received, Has.Count.EqualTo(2));
            Assert.That(received[0], Is.InstanceOf<TorchChanged>());
            Assert.That(received[1], Is.InstanceOf<PartyMoved>());

            Assert.That(received[0].EventId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(received[0].EventId, Is.Not.EqualTo(received[1].EventId));
            Assert.That(received[0].Timestamp, Is.GreaterThanOrEqualTo(before));
            Assert.That(received[0].Sequence, Is.EqualTo(1));
            Assert.That(received[1].Sequence, Is.EqualTo(2));

            var torch = (TorchChanged)received[0];
            Assert.That(torch.OldValue, Is.EqualTo(100));
            Assert.That(torch.Subsystem, Is.EqualTo(SubsystemNames.Torch));
        }

        /// <summary>Parallel publishes produce unique sequence numbers covering the full 1..N range.</summary>
        [Test]
        public void Publish_Parallel_ProducesUniqueSequences()
        {
            var bus = new GameEventBus();
            var sequences = new List<long>();
            var gate = new object();
            bus.Subscribe<GameEvent>(e =>
            {
                lock (gate)
                {
                    sequences.Add(e.Sequence);
                }
            });

            Parallel.For(0, 200, i => bus.Publish(new TorchChanged(i, i, "test")));

            Assert.That(sequences, Has.Count.EqualTo(200));
            Assert.That(sequences, Is.Unique);
            Assert.That(sequences.Min(), Is.EqualTo(1));
            Assert.That(sequences.Max(), Is.EqualTo(200));
        }

        /// <summary>A typed subscription receives only its type; the base-type subscription receives everything.</summary>
        [Test]
        public void Subscribe_DispatchesByAssignableType_AndDisposeUnsubscribes()
        {
            var bus = new GameEventBus();
            var attacks = new List<AttackExecuted>();
            var all = new List<GameEvent>();

            IDisposable typed = bus.Subscribe<AttackExecuted>(attacks.Add);
            bus.Subscribe<GameEvent>(all.Add);

            bus.Publish(new TorchChanged(60, 55, "snuffed"));
            bus.Publish(new AttackExecuted("crusader_01", "brigand_01", "smite"));

            Assert.That(attacks, Has.Count.EqualTo(1));
            Assert.That(attacks[0].SkillName, Is.EqualTo("smite"));
            Assert.That(all, Has.Count.EqualTo(2));

            typed.Dispose();
            bus.Publish(new AttackExecuted("crusader_01", "brigand_01", "smite"));
            Assert.That(attacks, Has.Count.EqualTo(1), "disposed subscription must stop receiving");
            Assert.That(all, Has.Count.EqualTo(3));
        }

        /// <summary>Null inputs are ignored instead of throwing (no exceptions in core).</summary>
        [Test]
        public void NullInputs_AreIgnored()
        {
            var bus = new GameEventBus();

            Assert.DoesNotThrow(() => bus.Publish<TorchChanged>(null));
            IDisposable subscription = null;
            Assert.DoesNotThrow(() => subscription = bus.Subscribe<GameEvent>(null));
            Assert.DoesNotThrow(() => subscription?.Dispose());
        }
    }
}
