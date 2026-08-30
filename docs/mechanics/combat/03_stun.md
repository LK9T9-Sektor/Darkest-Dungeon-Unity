# 03_stun.md — Stun: пропуск хода, `STUNRECOVERYBUFF`, истечение

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.4 закрыт).

## 1. Назначение и когда работает

Стан лишает юнита хода: когда у застанного юнита наступает ход, стан снимается, применяется
`STUNRECOVERYBUFF`, и ход пропускается (`CompleteTurn`). Действует на юнитов обоих сторон.

## 2. Модель данных

- `StunStatusEffect` (`Character/Statuses/StunStatusEffect.cs:6`) — `StunApplied` (флаг),
  `IsApplied = StunApplied`, `ResetStatus`.
- `StunEffect` (`Mechanics/Skills/Effects/StunEffect.cs:9`) — наложение.
- `UnstunEffect` (`Mechanics/Skills/Effects/UnstunEffect.cs:9`) — снятие.
- `STUNRECOVERYBUFF` — бафф из `JsonBuffs.json`: `stat_type=resistance`, `stat_sub_type=stun`,
  `amount=0.4` (т.е. +40% к stun-резисту), `rule_type=always`, длительность 2 раунда.

## 3. Парсинг контента

`EffectCatalog`: ключ `.stun` → `StunEffect` (значение-флаг); ключи `.unstun` → `UnstunEffect`,
`.clearguarding`/`.clearguarded` → `ClearGuardEffect` (снимает guard, не стан). `.chance` — шанс.

## 4. Порядок срабатывания (трассировка)

**Наложение** — `DuelController.ExecuteSkill` → `ApplyEffects` → `StunEffect.ApplyInstant`
(`StunEffect.cs:15`):

1. Если стан уже применён — выйти (не перезаписывать) (`:21`).
2. Шанс = `chance/100 − target.Stun + performer.StunChance` (только если performer не монстр),
   клэмп 0..0.95 (`:24-31`).
3. При успехе: `StunApplied = true`, `Events.SetHalo(target, "stunned")` (`:34-35`), **сброс guard**
   цели (`((IResetableStatusEffect)GetStatusEffect(Guard)).ResetStatus()`, `:36`).
4. `ApplyQueued` (`:43`) — попап `Stunned`/`StunResist`.

**Пропуск хода** — `DuelController.BeginTurn` (`DuelController.cs:312`):

1. `ApplyDotTicks(current)` + `CheckDeaths` (тик/смерть до стана) (`:344-346`).
2. `UpdateRound()` (`:348`) — декремент статусов/раундовых баффов.
3. Если `Stun.IsApplied` (`:350-361`): `StunApplied = false`, попап `Unstun`, `ResetHalo`,
   `ApplyStunRecovery(current)` (`:359`), затем `CompleteTurn()` — ход пропущен.

`ApplyStunRecovery` (`DuelController.cs:390-399`): `content.GetBuff("STUNRECOVERYBUFF")` →
`AddBuff(new BuffInfo(buff, BuffDurationType.Round, BuffSourceType.Adventure, 2))`.

**Истечение** — стан сам по себе не тикает (`StunStatusEffect.UpdateNextTurn` пуст, `:18`); снятие
происходит **только** в начале хода цели (шаг 3 выше) или через `UnstunEffect`.

## 5. Очередь и обновления

- Стан — не статус с длительностью-таймером, а **флаг**: истекает при наступлении хода цели.
- `STUNRECOVERYBUFF` — раундовый бафф (2 раунда), декремент через `UpdateRound` per-turn.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Уже застан | `StunEffect.cs:21` | выход без перезаписи |
| Шанс | `StunEffect.cs:24-31` | `chance/100 − Stun + StunChance`, клэмп 0..0.95 |
| Пропуск хода | `DuelController.cs:350-361` | стан снят + recovery + `CompleteTurn` |
| Recovery | `DuelController.cs:390-399` | +0.4 stun-resist, 2 раунда |

## 7. Нюансы и подводные камни

- **Стан снимается в начале хода цели, а не по таймеру** — `StunStatusEffect.UpdateNextTurn` пуст,
  его не надо «чинить»: истечение = пропуск хода.
- **Порядок в `BeginTurn` критичен**: тики/смерть → `UpdateRound` → проверка стана. Если поставить
  проверку стана до `UpdateRound`, recovery-бафф не успеет тикнуть.
- **Стан сбрасывает guard цели** при наложении (`StunEffect.cs:36`).
- **Застанный юнит не может действовать** — `DuelAi`/ручной ввод никогда его не получает (ход
  завершается автоматически).
- Мёртвый юнит (`IsDead`) не проверяется на стан — `BeginTurn` сначала обрабатывает смерть.

## 8. Взаимодействия

- Guard: стан сбрасывает `Guard` у цели (`03_stun` × `05_guard`).
- DoT: тик применяется до стан-проверки (`02_dot.md`).
- `UnstunEffect` (`.unstun`) — немедленное снятие без recovery-баффа.
- `RemoveConditions` (см. `09_buffs.md`) не влияет на стан (это не rule-бафф).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/StunStatusEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/StunEffect.cs`, `UnstunEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`BeginTurn`, `ApplyStunRecovery`)
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs` (`Stun_...`)