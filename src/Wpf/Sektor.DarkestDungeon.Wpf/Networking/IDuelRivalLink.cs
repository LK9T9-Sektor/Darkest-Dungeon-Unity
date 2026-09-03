using System;
using Sektor.DarkestDungeon.Core.Duel;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>Delivers the rival's duel inputs into the local simulation (network or AI).</summary>
    public interface IDuelRivalLink : IDisposable
    {
        /// <summary>Occurs when the rival performed an action; carries the raw payload ("skillId|targetId").</summary>
        event Action<string>? RivalActionReceived;

        /// <summary>Occurs when the rival (AI) is about to use a skill; carries the skill id (pacing preview).</summary>
        event Action<string>? SkillPreviewed;

        /// <summary>Occurs when the rival (AI) is about to target a unit; carries the combat id (pacing preview).</summary>
        event Action<int>? TargetPreviewed;

        /// <summary>Delivers the local action payload to the rival side.</summary>
        /// <param name="payload">The action payload ("skillId|targetId").</param>
        void SendLocalAction(string payload);

        /// <summary>Binds the link to the running duel.</summary>
        /// <param name="controller">The duel controller.</param>
        void Attach(DuelController controller);

        /// <summary>Stops observing the duel.</summary>
        void Detach();

        /// <summary>Pumps pending background work of the underlying channel.</summary>
        void Pump();
    }
}
