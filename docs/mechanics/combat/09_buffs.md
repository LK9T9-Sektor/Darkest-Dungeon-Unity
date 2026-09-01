# 09_buffs.md — Buff-система: стат-баффы, `.buff_ids`, ApplyConditions/RemoveConditions

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.7 закрыт).

## 1. Назначение и когда работает

Баффы/дебаффы модифицируют атрибуты персонажа (стат-адд/мульт) и управляются «правилами»
(`BuffRule`: always/по стану/по HP/по рангу/по статусу цели и т.д.). Ключевой механизм — пара
`ApplyConditions` (применить rule-баффы к скиллу) и `RemoveConditions` (снять условные баффы после
скилла). Свойства применяются идемпотентно через `IsApplied`-гейт.

## 2. Модель данных

- `Character` (`Character/Character.cs`): `BuffInfo` (список), `AddBuff` (`:194`), `ApplyBuff`
  (`:385`, гейт `IsApplied`), `RevertBuff` (`:404`, гейт), `RemoveBuff` (`:423`), `UpdateRound`
  (`:166`, декремент статусов + раундовых баффов), `RemoveConditionalBuffs` (`:184`, снимает
  `BuffSourceType.Condition`), `ApplyBuffRule` (`:432`, диспетчер `BuffRule`).
- `Buff`/`BuffInfo` — определение/инстанс (`ModifierValue`, `DurationType`, `SourceType`, `IsApplied`).
- `BuffCatalog` (`Character/BuffCatalog.cs`) — контент-баффы из `JsonBuffs.json`.
- **Тринкеты** (`Core.Content\Trinket\Trinket.cs`): `BuffIds` → permanent-баффы
  (`DuelController.ApplyTrinkets`, `DuelController.cs:585-608`). При надевании каждый бафф тринкета
  накладывается как `BuffDurationType.Permanent` с `BuffSourceType.Trinket`, HP приводится к полному.
  На герое записываются `Hero.EquippedTrinketIds` (до 2, `Hero.AddTrinket`).

## 3. Парсинг контента

`EffectCatalog`: `.combat_stat_buff` + стат-ключи (`attack_rating_add`, `crit_chance_add`,
`protection_rating_add`, `speed_rating[_add]`, `damage_low/high_multiply`) → `CombatStatBuffEffect`
(`Mechanics/Skills/Effects/CombatStatBuffEffect.cs:13`); `.buff_ids "..." "..."` → `BuffEffect`
(`:10`). Контент-баффы — из `JsonBuffs.json` через `IBattleContext.GetBuff` (дуэль →
`IDuelContent.GetBuff`).

## 4. Порядок срабатывания (трассировка)

**Наложение стат-баффа** — `CombatStatBuffEffect.ApplyBuff` (`CombatStatBuffEffect.cs:240`):

1. `BuffType.StatAdd`/`StatMultiply`, `BuffDurationType.Round` (по умолчанию 3) /
   `Camp` (если `.duration -1` или curio), `BuffSourceType.Adventure` (`:244-279`).
2. Условные (статус/тип монстра) — `ApplyConditional` (`:283-291`): `BuffSourceType.Condition`.

**Применение правила** — `Character.ApplyBuffRule` (`:432-528`): для каждого баффа по `RuleType`
вычисляется `apply`, затем `ApplyBuff`/`RevertBuff`. `IsApplied`-гейт гарантирует, что повторное
применение не наложит дважды.

**`ApplyConditions`** — `BattleSolver.ApplyConditions` (`BattleSolver.cs:515-524`):

1. `BattleContext.ApplyCombatUnitRules(performer, target, skill, isRiposte)` — rule-баффы обоих.
2. `effect.ApplyTargetConditions(...)` для каждого эффекта скилла (условные баффы).

**`RemoveConditions`** — вызывается **после каждого скилла**:

- `DuelController.ExecuteSkill` → `RemoveConditions(unit, targets)` (`DuelController.cs:518-524`) —
  перформер + **все** цели мультитаргета →
  `Solver.RemoveConditions` (`BattleSolver.cs:526-530`): `ApplyIdleUnitRules` + `RemoveConditionalBuffs`.
- Также в `CalculateSkillPotential` (превью) (`:556-557`).

**Идемпотентность** — `ApplyBuff`/`RevertBuff` (`Character.cs:385-419`) проверяют `buffEntry.IsApplied`
и выходят, если состояние не меняется.

## 5. Очередь и обновления

- Мгновенные баффы применяются сразу; `.queue` → `EventQueue`.
- Раундовые баффы (`BuffDurationType.Round`) декрементятся `UpdateDurations` через `UpdateRound`
  per-turn (`BeginTurn`, см. `13_turn_order.md`).
- Условные баффы (`BuffSourceType.Condition`) снимаются `RemoveConditions` после скилла — не ждут
  раундового таймера.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Rule-бафф активен | `Character.cs:432-437 (BuffRuleEvaluator)` | по `BuffRule` (always/статус/HP/ранг/...) |
| Идемпотентность | `:385,404` | `IsApplied`-гейт |
| Условные снимаются | `:184-191` | `BuffSourceType.Condition` |
| Debuff-resist `.buff_ids` | `BuffEffect.cs:47-58` | `chance/100 − Debuff + DebuffChance`, клэмп 0.95 |

## 7. Нюансы и подводные камни

- **`IsApplied`-гейт критичен**: без него `ApplyConditions`→`ApplyBuffRule`→`ApplyBuff` накладывает
  бафф повторно (двойной модификатор). Это была реальная ошибка паритета.
- **`RemoveConditions` после каждого скилла** — обязателен, иначе rule-баффы висят до конца раунда.
  Он снимает `Condition`, но НЕ `Adventure`-баффы (те живут по таймеру).
- `ApplyConditions` в `CalculateSkillPotential` (превью) **применяет** условия, а `RemoveConditions`
  там же снимает — превью не должно оставлять следов на персонаже.
- `BuffRule.Riposting` (`:506-508`) активен только когда `rules.IsRiposte` (контратака, см.
  `04_riposte.md`).
- `AddBuff` для `BuffRule.Always` применяет сразу (`:197-198`), для остальных — только через
  `ApplyBuffRule`.

## 8. Взаимодействия

- RemoveConditions после скилла — с ним связаны `01_damage.md`, `04_riposte.md`.
- DoT/стан/guard — это статусы, а не баффы; `RemoveConditions` их не трогает.
- Торч-бонус в `BattleRulesContext` (`DuelBattleContext.cs:107,115`) — правило `LightAbove/LightBelow`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Character.cs`, `Buff.cs`, `BuffInfo.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/BuffCatalog.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/CombatStatBuffEffect.cs`,
  `BuffEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`, `DuelBattleContext.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs`
  (`RemoveConditions_...`), `SkillEffectsTests.cs`

