# 06_mark.md — Mark/Tag: наложение, истечение, `buff_duration_type`

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **частично** (паритет §2.5, незначительный).

## 1. Назначение и когда работает

Отметка (`mark`/`tag`) вешает на цель видимый маркер; используется ИИ для выбора цели
(`TargetSelectionMarked`) и rule-баффами (`BuffRule.Status` с `StringParam = "marked"`). Сам по себе
урон не даёт (нет бонуса по отмеченной цели — стаб в обоих, см. `BATTLE_PARITY.md` §3).

## 2. Модель данных

- `MarkStatusEffect` (`Character/Statuses/MarkStatusEffect.cs:6`) — `MarkDuration`,
  `DurationType` (`Round`/`Combat`), `IsApplied = MarkDuration > 0`, `UpdateNextTurn` (декремент,
  кроме `Combat`, `:21-30`).
- `TagEffect` (`Mechanics/Skills/Effects/TagEffect.cs:9`) — наложение.
- `UntagEffect` (`Mechanics/Skills/Effects/UntagEffect.cs:9`) — снятие.

## 3. Парсинг контента

`EffectCatalog`: `.tag`/`.mark` → `TagEffect` (`EffectCatalog.cs`); `.duration` → тики. Ключ
`buff_duration_type` (Round/Combat) **не читается** — `DurationType` всегда `Round` по умолчанию
(паритет-разрыв, незначительный).

## 4. Порядок срабатывания (трассировка)

**Наложение** — `TagEffect.ApplyInstant` (`TagEffect.cs:17`):

1. `MarkDuration = effect.Duration ?? 3` (`:23`).
2. `DurationType = DurationType` (из конструктора; всегда `Round` в ядре) (`:24`).
3. `ApplyQueued` (`:31`) — попап `Tagged`.

**Истечение** — `MarkStatusEffect.UpdateNextTurn` (`:21-30`): per-turn декремент, кроме
`DurationType.Combat`.

**Снятие** — `UntagEffect` (`.untag`): `MarkDuration = 0` (`UntagEffect.cs:23`).

## 5. Очередь и обновления

- Наложение мгновенное; истечение — per-turn через `UpdateRound` в `BeginTurn`.
- `Combat`-длительность не истекает до конца боя (не реализовано чтение `buff_duration_type`).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Наложение | `TagEffect.cs:23` | `duration ?? 3` |
| Истечение | `MarkStatusEffect.cs:21-30` | декремент per-turn; `Combat` — без истечения |
| Снятие | `UntagEffect.cs:23` | `MarkDuration = 0` |

## 7. Нюансы и подводные камни

- **`buff_duration_type` не читается** — все метки `Round` (3 тика по умолчанию). Закрытие разрыва =
  парсинг ключа в `EffectCatalog` → `TagEffect.DurationType`.
- Метка не влияет на урон; только ИИ-таргетинг и rule-баффы (`BuffRule.Status`).
- Перекрытие: `MarkDuration` перезаписывается, не суммируется.

## 8. Взаимодействия

- ИИ: `DuelTargetSelectionMarked` (см. `AI_BEHAVIOR.md`) выбирает отмеченные цели.
- Rule-бафф `BuffRule.Status` (`Character.cs:BuffRuleEvaluator/Status`) — активен, пока цель отмечена.
- `.untag` снимает метку (скиллы «Flare», «Clear Marked Target»).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/MarkStatusEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/TagEffect.cs`, `UntagEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/EffectCatalog.cs`
