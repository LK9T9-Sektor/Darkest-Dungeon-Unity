namespace Sektor.DarkestDungeon.Core.Combat.Content
{
    /// <summary>
    /// Named identifiers of content buffs referenced by id from <c>Data/JsonBuffs.json</c>.
    /// Using constants keeps buff lookups refactor-safe and avoids magic strings.
    /// </summary>
    public static class BuffIds
    {
        /// <summary>Stun recovery buff: +40% stun resistance for 2 rounds, applied when a stun wears off.</summary>
        public const string StunRecovery = "STUNRECOVERYBUFF";
    }
}