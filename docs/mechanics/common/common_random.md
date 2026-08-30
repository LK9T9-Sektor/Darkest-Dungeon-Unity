# common_random.md — RandomSolver/IRng: детерминизм

> Домен: `common` (ядро `Core.Common` примитивы + `Core.Combat/Mechanics/RandomSolver`). Статус:
> **реализовано**.

## 1. Назначение и когда работает

Единый источник случайности для боя. `RandomSolver` — статический обёрточный слой над `System.Random`
с фиксируемым сидом (детерминизм локстапа). `IProportionValue`/`ISingleProportion` — интерфейсы
взвешенных выборов (`RandomSolver.ChooseByRandom`).

## 2. Модель данных

- `RandomSolver` (`Core.Combat/Mechanics/RandomSolver.cs:8`) — статический; `SetRandomSeed` (`:133`),
  `Next(int)`, `Next(min,max)` (`:110,119`), `CheckSuccess(float)` (`:99`), `ChooseByRandom<T>`
  (`:36`, взвешенный по `Chance`), `ChooseAnyExcept` (`:20`).
- `IProportionValue` (`Core.Common/IProportionValue.cs:4`) — `Chance`.
- `ISingleProportion` (`Core.Common/ISingleProportion.cs:4`).

## 3. Порядок срабатывания (трассировка)

1. `DuelController.StartDuel/StartFight` вызывает `RandomSolver.SetRandomSeed(sessionSeed)`
   (`DuelController.cs:119,147`).
2. Все роллы боя (меткость, крит, шансы эффектов, AI) используют `RandomSolver`.
3. `DuelSeed.ComputeSessionSeed` также устанавливает сид при вычислении.

## 4. Очередь и обновления

- Глобальное состояние: сид фиксируется на старт сессии; порядок вызовов определяет последовательность.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Сид | `SetRandomSeed` | `System.Random(seed)` |
| Шанс | `CheckSuccess` | `NextDouble() < chance` |
| Взвешенный | `ChooseByRandom` | сумма `Chance`, выбор по `Next(sum)` |

## 6. Нюансы и подводные камни

- **Детерминизм критичен для lockstep**: любой ролл в одном порядке на одной стороне обязан
  повторяться на другой. Добавлять новые `RandomSolver`-вызовы только в общий путь симуляции.
- Не смешивать `RandomSolver` с локальным `System.Random` в боевом коде (ломает сид).
- `RandomSolver` живёт в `Core.Combat`, хотя `IRng` — планируемый примитив `Core.Common`
  (`TARGET_LAYOUT.md`).

## 7. Взаимодействия

- `duel_01_lockstep.md`, `duel_03_seed.md`.
- Все шансы эффектов (`combat/*`).

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/RandomSolver.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Common/IProportionValue.cs`, `ISingleProportion.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelSeed.cs`, `DuelController.cs`
