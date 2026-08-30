# duel_03_seed.md — DuelSeed: детерминированный сид сессии

> Домен: `duel` (ядро `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Вычисляет детерминированный сид сессии из упорядоченных id игроков. Используется обеими сторонами
дуэли, чтобы `RandomSolver` на старте давал одинаковые значения (локстап).

## 2. Модель данных

`DuelSeed` (`Core.Duel/DuelSeed.cs:6`) — статический класс:
- `ComputeSessionSeed(string[] orderedPlayerIds)` (`:11`) — сумма стабильных хэшей по сиду;
- `StableHash(string)` (`:26`) — 32-битный хэш (FNV-подобный, `hash = hash*31 + c`).

## 3. Порядок срабатывания (трассировка)

1. `ComputeSessionSeed(orderedPlayerIds)` (`:11-21`):
   - для каждого id: `RandomSolver.SetRandomSeed(StableHash(id))`, `sessionSeed += Next(2^16)`;
   - финальный `RandomSolver.SetRandomSeed(sessionSeed)`, возврат.
2. Стороны вызывают с **одинаковым порядком** id (local first, см. `NETWORK.md` §6).
3. `DuelController.StartDuel/StartFight` ставит этот сид в `RandomSolver`.

## 4. Очередь и обновления

- Сид фиксируется на старте; в течение боя не пересчитывается.
- Порядок id критичен: `[A,B]` и `[B,A]` дают разные сиды.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Стабильность | `StableHash` | детерминированный, без `Random` |
| Порядок | `ComputeSessionSeed` | сумма по порядку |

## 6. Нюансы и подводные камни

- **`StableHash` должен оставаться стабильным** — менять алгоритм нельзя (ломает совместимость
  реплеев/сейвов сессии).
- Сид — `int` (2^16 на шаг), коллизии возможны, но детерминизм важнее уникальности.

## 7. Взаимодействия

- Lockstep (`duel_01_lockstep.md`); `RandomSolver` (`common/RandomSolver`).

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelSeed.cs`
- `docs/NETWORK.md` §6