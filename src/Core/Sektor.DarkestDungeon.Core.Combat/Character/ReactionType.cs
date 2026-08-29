namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Reaction types for traits.</summary>
    public enum ReactionType
    {
        BlockMove, BlockHeal,
        BlockBuff, BlockItem,
        BlockRetreat, CommentSelfHit,
        CommentSelfMissed, CommentAllyHit,
        CommentAllyMissed, CommentAllyAttackHit,
        CommentAllyAttackMiss, CommentMove,
        CommentCurioInteraction, CommentTrapTriggered,
        BlockEffect
    }
}
