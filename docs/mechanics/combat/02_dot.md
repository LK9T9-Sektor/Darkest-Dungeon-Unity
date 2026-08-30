# 02_dot.md — DoT (bleed / poison): наложение, тик, истечение

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.3 закрыт).

## 1. Назначение и когда работает

Кровотечение (bleed) и яд (poison) накладываются эффектами скиллов и наносят урон **в начале хода
цели** каждый тик, пока не истекут. Урон тика не зависит от защит, точности и уклонения цели.

## 2. Модель данных

- `DamageOverTimeStatusEffect` (`Character/Statuses/DamageOverTimeStatusEffect.cs:7`) — контейнер
  DoT: `CurrentTickDamage` (`:15`, сумма по инстансам), `CombinedDamage`, `ExpirationTime`;
  `AddInstanse(tickDamage, ticks)` (`:40`), `RemoveDoT()` (`:52`), `UpdateNextTurn` (`:24-29`,
  декремент через `CheckExpiration`).
- `DamageOverTimeInstanse` (`Character/Statuses/DamageOverTimeInstanse.cs:17`) — `CheckExpiration() =
  --TicksLeft <= 0` (удаляется после последнего тика).
- `BleedingStatusEffect`/`PoisonStatusEffect` — тонкие наследники (`Type`).
- `BleedEffect` (`Mechanics/Skills/Effects/BleedEffect.cs:24`) / `PoisonEffect` (`:24`) — наложение.

## 3. Парсинг контента

`EffectCatalog` парсит `.dotBleed N` / `.dotPoison N` (+ `.duration` тиков, по умолчанию 3):

```text
effect: .name "Bleed 2" .chance 110% .dotBleed 2 .duration 3
```

`BleedEffect.DotBleed`/`PoisonEffect.DotPoison` = урон за тик; `Duration` = число тиков.

## 4. Порядок срабатывания (трассировка)

**Наложение** — `DuelController.ExecuteSkill` → `ApplyEffects` → `BleedEffect.ApplyInstant`
(`BleedEffect.cs:24`):

1. Шанс = `chance/100 − target.Bleed + performer.BleedChance` (Poison — `target.Poison`/
   `performer.PoisonChance`), клэмп 0..0.95 (`:27-36`).
2. При успехе — `AddInstanse(DotBleed, duration ?? 3)` (`:40`).
3. `ApplyQueued` (`:47`) — попап `Bleed`/`BleedResist` + overlay.

**Тик урона** — в начале хода цели, `DuelController.BeginTurn` (`DuelController.cs:238`):

1. `ApplyDotTicks(current)` (`DuelController.cs:270-276,Mechanics/DotTickApplier`): для `StatusType.Bleeding` и `Poison`,
   если `IsApplied` — `TakeDamage(CurrentTickDamage)` + попап `Damage` + `UpdateOverlay`.
2. `CheckDeaths()` (`:345`) — юнит может умереть от тика (см. `14_death_stress.md`); если умер —
   `CompleteTurn` и ход не исполняется.
3. `((Character)current.Character).UpdateRound()` (`:348`) — декремент всех статусов (в т.ч. DoT
   тиков через `UpdateNextTurn`) и раундовых баффов.

**Истечение** — `DamageOverTimeStatusEffect.UpdateNextTurn` (`:24-29`): каждый тик в начале хода цели
`CheckExpiration` уменьшает `TicksLeft`; инстанс удаляется при `<= 0`.

## 5. Очередь и обновления

- DoT-тик применяется **per-turn** (в начале хода **цели**), а не раз в раунд: `UpdateRound`
  вызывается из `BeginTurn` для текущего юнита, а не массово в `NextRound` (см. `13_turn_order.md`).
- Тики истекают по ходам цели (каждый ход — один `CheckExpiration` на инстанс).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Шанс наложения | `BleedEffect.cs:27-36` | `chance/100 − резист + шанс`, клэмп 0..0.95 |
| Количество тиков | `AddInstanse` | `duration ?? 3` |
| Тик урона | `DuelController.cs:Mechanics/DotTickApplier` | сумма по всем инстансам, без защиты |
| Истечение | `DamageOverTimeInstanse.cs:17` | удаление при `TicksLeft <= 0` |

## 7. Нюансы и подводные камни

- **Тик происходит до обычного хода** — юнит, получивший DoT, сначала теряет HP, и только потом
  действует (или пропускает ход из-за стана).
- **Мёртвый от тика юнит не ходит**: `BeginTurn` вызывает `CompleteTurn` при `IsDead` после
  `ApplyDotTicks`/`CheckDeaths` — это не «передача хода», а пропуск мёртвого.
- **DoT не тикает в тот же ход, когда наложен** — первый тик — в начале следующего хода цели
  (наложение происходит в `ExecuteSkill`, тик — в `BeginTurn`).
- `CombinedDamage`/`ExpirationTime` — вспомогательные (UI), не участвуют в боевой математике.

## 8. Взаимодействия

- Смерть от тика → `CheckDeaths` → `StressParty` героям (см. `14_death_stress.md`).
- `CureEffect` снимает bleed+poison (`RemoveDoT`) — эффект `.cure`.
- DoT-статусы можно накладывать повторно — инстансы суммируются (`CurrentTickDamage` — сумма).
- Idle-юниты (0 ходов за раунд): тик ×1.5 — **не реализовано** (стаб, см. `BATTLE_PARITY.md` §2.3).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/DamageOverTimeStatusEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/DamageOverTimeInstanse.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/BleedingStatusEffect.cs`, `PoisonStatusEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/BleedEffect.cs`, `PoisonEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/CureEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs` (`DotTick_...`)

