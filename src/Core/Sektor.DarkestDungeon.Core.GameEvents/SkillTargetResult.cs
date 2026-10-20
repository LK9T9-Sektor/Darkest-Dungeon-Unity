namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Outcome of one skill against one target.</summary>
    /// <param name="TargetId">The target unit id.</param>
    /// <param name="ResultType">The result type (for example <c>Hit</c>, <c>Crit</c>, <c>Miss</c>).</param>
    /// <param name="Amount">Damage, healing, or stress amount; <c>0</c> for misses.</param>
    /// <param name="IsHit">Whether the target was hit.</param>
    /// <param name="IsZeroed">Whether the target reached zero HP.</param>
    /// <param name="IsHarmful">Whether the result is harmful to the target.</param>
    /// <param name="HpAfter">Target HP after the skill.</param>
    /// <param name="StressAfter">Target stress after the skill.</param>
    /// <param name="RankAfter">Target rank after movement effects, when known.</param>
    public sealed record SkillTargetResult(
        string TargetId,
        string ResultType,
        int Amount,
        bool IsHit,
        bool IsZeroed,
        bool IsHarmful,
        int HpAfter,
        int StressAfter,
        int? RankAfter = null);
}
