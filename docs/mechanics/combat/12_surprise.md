# 12_surprise.md — Surprise 1-го раунда: шанс, -100 инициативы, shuffle

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.8, сюрприз).

## 1. Назначение и когда работает

При старте боя роллится сюрприз: сторона, которую застали врасплох, в первом раунде действует
последней (инициатива −100), её юниты помечаются `IsSurprised` (снимается на их ходу), герои
дополнительно перемешиваются. Управляется `BattleModifiers` монстров и шансами героев.

## 2. Модель данных

- `SurpriseStatus` (`Raid/Battle`) — `None/HeroesSurprised/MonstersSurprised`.
- `BattleGround.SetSurpriseStatus` / `SurpriseStatus` — статус боя.
- `FormationUnitInfo.IsSurprised` (`Raid/Party/FormationUnitInfo.cs:22`) — флаг юнита.
- `IBattleModifier` — `CanSurprise/CanBeSurprised/AlwaysSurprise/AlwaysBeSurprised`.
- Атрибуты героев: `MonsterSurpirseChance` (монстры застаются героями), `PartySurpriseChance`
  (герои застаются монстрами).

## 3. Парсинг контента

`MonsterClassFileParser`: `battle_modifier:` секции → `MonsterClass.BattleModifier` (флаги сюрприза).
Атрибуты шансов героев — из `Heroes/*.bytes`/квирков/баффов.

## 4. Порядок срабатывания (трассировка)

`DuelController.CheckSurprise` (`DuelController.cs:210`) вызывается в `StartBattle` (`:150`):

1. **AlwaysBeSurprised** монстров (`:213-219`): `MonstersSurprised`, флаг на всех монстрах, выход.
2. **Шанс монстров** (`:221-239`): если нет `BattleModifiers` или `CanBeSurprised` —
   `monstersSurprised = 0.1 + TorchSurpriseBonus(torch, true) + Σ MonsterSurpirseChance героев`
   (`:224-229`), клэмп 0..0.65 (`:231`); при успехе — `MonstersSurprised`.
3. **AlwaysSurprise** монстров (`:241-248`): `HeroesSurprised` + shuffle героев.
4. **Шанс героев** (`:250-268`): `0.1 + TorchSurpriseBonus(torch, false) + Σ PartySurpriseChance`
   (`:253-258`), клэмп; при успехе — `HeroesSurprised` + shuffle.

`TorchSurpriseBonus` (`:281-291`): по диапазону торча (>75: монстры +0.25 / герои 0; 51-75: +0.15/0;
26-50: +0.10/+0.15; 1-25: +0.05/+0.25; 0: 0/+0.4).

`ShuffleParty` (`:294-308`): случайные обмены в списке + пересчёт `Rank`.

**Влияние на очередь** — `Round.NextRound` (`Round.cs:156-166`): если `RoundNumber == 0` и
`SurpriseStatus` — инициатива застигнутой стороны `−= 100`, повторная сортировка.

**Снятие флага** — `DuelController.BeginTurn` (`:330`): `current.CombatInfo.IsSurprised = false`.

## 5. Очередь и обновления

- Ролл — один раз при старте (`StartBattle`).
- Инициатива −100 — только в 1-м раунде (`RoundNumber == 0`).
- `IsSurprised` снимается в начале хода юнита.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Always-флаги | `DuelController.cs:213,241` | безусловный сюрприз |
| Базовый шанс | `:224,253` | `0.1 + торч-бонус` |
| Атрибуты героев | `:226-229,255-258` | суммарные шансы |
| Клэмп шанса | `:231,260` | 0..0.65 |
| Инициатива | `Round.cs:158-165` | `−100`, только раунд 0 |

## 7. Нюансы и подводные камни

- **`TorchSurpriseBonus` разный для сторон** — монстры больше боятся высокого торча, герои —
  низкого.
- **AlwaysSurprise монстров НЕ перекрывается шансом монстров** — ветка 3 выполняется только если
  не сработал сюрприз монстров (после `return` на `:238`).
- **Shuffle только героев** (монстры не перемешиваются).
- Порядок: «монстры застают героев» проверяется ПОСЛЕ «герои застают монстров» (но при `AlwaysSurprise`
  — до). Соблюдать последовательность веток — иначе флаги обеих сторон.
- Сюрприз влияет только на порядок, не на шансы/урон.

## 8. Взаимодействия

- Торч (`10_torch.md`) — источник бонуса.
- Очередь/инициатива (`13_turn_order.md`).
- `BattleModifiers` монстров (`MonsterClass`).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`CheckSurprise`, `ShuffleParty`,
  `TorchSurpriseBonus`)
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/Round.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Party/FormationUnitInfo.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/SurpriseTests.cs`