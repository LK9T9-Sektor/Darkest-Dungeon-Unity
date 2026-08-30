# duel_01_lockstep.md — Lockstep-модель PvP-дуэли и машина фаз

> Домен: `duel` (ядро `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Дуэль — PvP 1v1 в lockstep-режиме: **обе стороны строят идентичные отряды и гоняют одну
симуляцию**; по сети передаются только вводы (`DuelPayload`) и конфиг отряда. Тождество состояния —
залог детерминизма. Хост управляет героями, клиент — монстрами.

## 2. Модель данных

- `DuelController` (`Core.Duel/DuelController.cs:23`) — оркестратор; `HeroParty`/`MonsterParty`
  (`FormationParty`), `BattleGround`, `Context` (`DuelBattleContext`), `Solver` (`BattleSolver`),
  `Events` (`DuelBattleEvents`), `IsHost`, `Phase` (`DuelPhase`).
- `DuelPhase` (`DuelPhase.cs:4`) — `NotStarted`/`WaitingForHostAction`/`WaitingForClientAction`/`Finished`.
- `IDuelContent` — мост контента (`GetHeroClass/GetMonsterClass/GetQuirk/GetBuff/GetEffect/...`).

## 3. Парсинг контента

`DuelController` использует `IDuelContent` (WPF: `DuelContent`; тесты: `TestDuelContent`; Тест-бой:
`TextFightContent`). Герои генерируются `HeroGeneration.GenerateHero(class, seed)`.

## 4. Порядок срабатывания (трассировка)

1. **Старт** — `StartDuel(hostPicks, clientPicks, sessionSeed, isHost)` (`DuelController.cs:104`):
   построение отрядов (`AddHero` для каждой стороны), `BattleGround`, `Context`, `Solver`,
   регистрация `TorchDelta`, `RandomSolver.SetRandomSeed(sessionSeed)` (`:110,139`), `Phase=NotStarted`.
2. **Старт боя** — `StartBattle` (`:151`): `CheckSurprise` → `Round.StartBattle` → `BeginTurn`.
3. **Ход**: `BeginTurn` (`:312`) выбирает `CurrentUnit`, ставит фазу `WaitingForHost/ClientAction`.
   `IsLocalTurn` (`:50-57`) — true, когда локальная сторона должна действовать.
4. **Действие**: хост/клиент исполняет `ExecuteLocalSkill`/`ExecuteLocalPass`/`ExecuteLocalMove`
   (`:402,466,478`) или принимает `ApplyRemoteSkill(payload)` (`:429`). Каждое действие возвращает/
   принимает wire-строку (`DuelPayload`).
5. **Завершение**: `CompleteTurn` (`:518`) → `PostTurn` → следующий юнит или `NextRound` →
   `BeginTurn`.

**Детерминизм**: обе стороны вызывают одинаковые методы с одинаковым сидом `RandomSolver` —
состояние сходится (тест `DuelTurnFlowTests.TurnFlow_BothSides_RemainInLockstep`).

## 5. Очередь и обновления

- Действия — последовательные ходы; `Phase` — признак чьей стороны ход.
- `RandomSolver` глобальный с сидом сессии — любой ролл на одной стороне должен повторяться на другой
  (одинаковые вызовы в одинаковом порядке).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Локальный ход | `DuelController.cs:50-57` | `IsHost && WaitingForHostAction` и т.д. |
| Невалидное действие | `:402-417` | `null` (не меняет состояние) |
| Конец боя | `IsFinished` | `BattleGround.IsBattleEnded()` |

## 7. Нюансы и подводные камни

- **`RandomSolver` — синглтон-состояние**: сид устанавливается при старте; любое различие в порядке
  роллов между сторонами ломает локстап. Не добавлять роллы между `StartBattle` у сторон.
- `ExecuteLocalSkill` вызывает `ExecuteSkill` → `ProcessEventQueues` → `CheckDeaths` →
  `ExecuteRiposte` → `RemoveConditions` — вся цепочка должна быть детерминирована.
- `DuelPhase.Finished` ставится в `BeginTurn`/`CompleteTurn` при `IsBattleEnded`.

## 8. Взаимодействия

- Вводы — `DuelPayload` (`duel_02_payload.md`); сид — `duel_03_seed.md`.
- Боевые механики — `combat/*`.
- AI — `DuelAi` (`duel_04_ai.md`); автобой — `FightSession` (`duel_05_fight.md`).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`, `DuelPhase.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/DuelTurnFlowTests.cs`
