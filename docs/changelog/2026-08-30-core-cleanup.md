# Уборка кода ядра (без изменения поведения)

- **Именованные константы** вместо магических строк/чисел: `Core.Combat/Content/EffectIds`,
  `BuffIds`, `BattleConstants` (клэмпы/длительности/крит), `ChanceMath` (единый клэмп шансов).
- **Полиморфизм вместо switch**: `BuffRuleEvaluator` (реестр правил баффов), `EffectParser`
  (отдельный тестируемый парсер эффектов).
- **Вынос приватной логики** в тестируемые классы: `SurpriseResolver`, `DotTickApplier`,
  `StunRecoveryApplier`, `DeathCheck`, `TurnMover` (Core.Duel); `DamageResolver`, `HealResolver`
  (Core.Combat).
- **Логирование**: `ILogger`/`NullLogger` в `Core.Common`, DI в `DuelController`; бой пишет
  структурный лог ходов и скиллов (`[duel] ...`).
