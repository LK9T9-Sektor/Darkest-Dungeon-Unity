using System;
using System.Collections.Generic;
using System.Threading;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// Synchronous thread-safe event bus. <see cref="Publish{T}"/> stamps identity/order fields on
    /// a copy of the record (events are immutable), then invokes matching subscribers outside the
    /// lock so handlers may subscribe/unsubscribe from within a callback.
    /// </summary>
    public sealed class GameEventBus : IGameEventBus
    {
        private readonly List<Subscription> subscriptions = new List<Subscription>();
        private readonly object gate = new object();
        private long sequence;

        /// <inheritdoc/>
        public void Publish<T>(T gameEvent) where T : GameEvent
        {
            if (gameEvent == null)
                return;

            GameEvent stamped = gameEvent with
            {
                EventId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Sequence = Interlocked.Increment(ref sequence),
            };

            Subscription[] snapshot;
            lock (gate)
            {
                if (subscriptions.Count == 0)
                    return;
                snapshot = subscriptions.ToArray();
            }

            foreach (Subscription subscription in snapshot)
                subscription.Invoke(stamped);
        }

        /// <inheritdoc/>
        public IDisposable Subscribe<T>(Action<T> handler) where T : GameEvent
        {
            if (handler == null)
                return EmptySubscription.Instance;

            var subscription = new Subscription(this, typeof(T), stamped => handler((T)stamped));
            lock (gate)
            {
                subscriptions.Add(subscription);
            }

            return subscription;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly GameEventBus owner;
            private readonly Type eventType;
            private readonly Action<GameEvent> handler;
            private bool disposed;

            public Subscription(GameEventBus owner, Type eventType, Action<GameEvent> handler)
            {
                this.owner = owner;
                this.eventType = eventType;
                this.handler = handler;
            }

            public void Invoke(GameEvent stamped)
            {
                if (eventType.IsInstanceOfType(stamped))
                    handler(stamped);
            }

            public void Dispose()
            {
                if (disposed)
                    return;
                disposed = true;
                lock (owner.gate)
                {
                    owner.subscriptions.Remove(this);
                }
            }
        }

        private sealed class EmptySubscription : IDisposable
        {
            public static readonly EmptySubscription Instance = new EmptySubscription();

            private EmptySubscription()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
