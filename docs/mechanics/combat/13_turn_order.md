# 13_turn_order.md — Инициатива / порядок хода / per-turn обновления

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Определяет, в каком порядке юниты действуют в раунде, и когда тикают per-turn статусы/баффы.
Инициатива — `Speed + бросок 0–10`; очередь раунда — `OrderedUnits`. Ход юнита проходит
`BeginTurn → PreTurn → действие → PostTurn → следующий`. **Ключевой момент: `UpdateRound` (декремент
статусов и раундовых баффов) вызывается per-turn в начале хода юнита, а не раз в раунд.**

## 2. Модель данных

- `Round` (`Mechanics/Battle/Round.cs:15`) — `OrderedUnits`, `RoundNumber`, `SelectedUnit`,
  `PreHeroTurn`/`PreMonsterTurn`/`PostHeroTurn`/`PostMonsterTurn`, `NextRound`.
- `FormationUnitInfo` (`Raid/Party/FormationUnitInfo.cs`) — `InitiativeRoll` (`:43`), `UpdateNextTurn`
  (`:107`), `UpdateNextRound` (`:115`), `IsSurprised` (`:22`).
- `DuelController` — машина фаз (`DuelPhase`), `BeginTurn`/`CompleteTurn`/`NextRound`.

## 3. Парсинг контента

Скорость — из `Heroes/*.bytes`/монстров (`speed`); монстры — `number_of_turns_per_round`
(`NumberOfTurns`, дублирование в очереди). Сюрприз-модификатор — `12_surprise.md`.

## 4. Порядок срабатывания (трассировка)

**Старт боя** — `DuelController.StartBattle` (`DuelController.cs:159-163`):
`CheckSurprise()` → `Round.StartBattle` (`:124-128`: `RoundNumber=0`, `NextRound`) → `BeginTurn`.

**Формирование очереди** — `Round.NextRound` (`Round.cs:133-169`):

1. `OrderedUnits.Clear()` (`:136`).
2. Для каждого героя: `UpdateNextRound()` + `InitiativeRoll = Speed + RandomSolver.Next(0,10) +
   NextDouble()` (`:140-141`).
3. Для каждого монстра: то же + `NumberOfTurns` дублей (`:147-151`).
4. Сортировка по `InitiativeRoll` убыванию (`:154`).
5. Раунд 0 + сюрприз: инициатива застигнутой стороны `−= 100`, ресорт (`:156-166`).

**Ход юнита** — `DuelController.BeginTurn` (`DuelController.cs:238`):

1. `IsSurprised = false` (`:330`); если юнит мёртв → удалить из очереди + `CompleteTurn` (`:332-337`).
2. `PreHeroTurn`/`PreMonsterTurn` (`:339-342`) — `UpdateNextTurn()` (инициатива) + `OrderedUnits.Remove`
   (`Round.cs:53,61,86,94`).
3. **`ApplyDotTicks`** → `CheckDeaths` → `UpdateRound()` (per-turn декремент) → стан-проверка
   (`DuelController.cs:344-361`).
4. Фаза `WaitingForHostAction`/`WaitingForClientAction` (`:363-365`).

**Завершение хода** — `DuelController.CompleteTurn` (`:518`): `PostHeroTurn`/`PostMonsterTurn`
(`Round.cs:71,104`) → если очередь пуста → `NextRound`, иначе `BeginTurn`.

**NextRound** — `DuelController.NextRound` (`:664-671`): `BattleGround.Round.NextRound` + `BeginTurn`.
**Важно**: здесь НЕ вызывается `UpdateRound` (перенесён в `BeginTurn` — см. п.7).

## 5. Очередь и обновления

- **Per-turn** (в `BeginTurn` текущего юнита): `UpdateRound` → декремент всех статусов
  (`UpdateNextTurn`) и раундовых баффов (`UpdateDurations(Round)`), DoT-тики (см. `02_dot.md`).
- **Per-round**: `UpdateNextRound` в `Round.NextRound` (сброс `SkillsUsedThisTurn`, блокировок,
  инициативы).
- `CombatInfo.UpdateNextTurn` (в `PreTurn`) — декремент инициативы/кулдаунов юнита.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Инициатива | `Round.cs:141,148` | `Speed + 0..10 + double` |
| Сюрприз раунда 0 | `:158-165` | `−100` |
| Multi-turn монстр | `:149-151` | `NumberOfTurns` дублей |
| Мёртвый юнит | `DuelController.cs:332-337` | удалить из очереди, `CompleteTurn` |

## 7. Нюансы и подводные камни

- **`UpdateRound` вызывается в `BeginTurn` (per-turn), а НЕ в `NextRound` (per-round)** — это паритет
  с Unity. Раньше был в `NextRound` — статусы тикали раз в раунд; DoT-тики и истечения работают только
  при per-turn. Не возвращать `UpdateRound` в `NextRound`.
- **`PreHeroTurn`/`PreMonsterTurn` удаляют юнита из `OrderedUnits`** — поэтому мёртвый/застанный юнит
  обрабатывается `CompleteTurn` (иначе бесконечная рекурсия `BeginTurn↔CompleteTurn`).
- Монстр с `NumberOfTurns > 1` появляется в очереди несколько раз — каждый заход — отдельный ход.
- `BeginTurn` при `IsDead` удаляет юнита из очереди вручную (`Round.OrderedUnits.Remove`) до `PreTurn`.

## 8. Взаимодействия

- DoT-тик и стан — в `BeginTurn` (`02_dot.md`, `03_stun.md`).
- Сюрприз меняет инициативу раунда 0 (`12_surprise.md`).
- Инициатива для AI/битвы — `Round.NextRound`; `InsertInitiativeRoll` (`:173-184`) — бонусная
  инициатива (используется редко).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/Round.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Party/FormationUnitInfo.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`BeginTurn`/`CompleteTurn`/`NextRound`)
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/DuelTurnFlowTests.cs`

