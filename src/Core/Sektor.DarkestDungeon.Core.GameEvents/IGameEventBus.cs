using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// The game event bus of the recording pipeline (spec §1.1): game systems publish events,
    /// an event logger subscribes and writes them to disk. Pure domain - no serialization here.
    /// </summary>
    public interface IGameEventBus
    {
        /// <summary>Publishes an event; stamps <see cref="GameEvent.EventId"/>, <see cref="GameEvent.Timestamp"/> and <see cref="GameEvent.Sequence"/> on the copy that reaches subscribers.</summary>
        /// <typeparam name="T">The static event type.</typeparam>
        /// <param name="gameEvent">The event to publish; ignored when <c>null</c>.</param>
        void Publish<T>(T gameEvent) where T : GameEvent;

        /// <summary>Subscribes to events assignable to <typeparamref name="T"/> (a subscription on <see cref="GameEvent"/> receives everything).</summary>
        /// <typeparam name="T">The event type (or a base type) to receive.</typeparam>
        /// <param name="handler">The handler; when <c>null</c>, nothing is subscribed and a no-op disposable is returned.</param>
        /// <returns>A disposable removing the subscription.</returns>
        IDisposable Subscribe<T>(Action<T> handler) where T : GameEvent;
    }
}
