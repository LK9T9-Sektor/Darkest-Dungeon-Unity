# 07_rank_move.md — Pull / Push / Shuffle: перемещение рангов

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.6 закрыт).

## 1. Назначение и когда работает

Эффекты скиллов двигают юнитов по рангам партии: `pull` (вперёд, к рангу 1), `push` (назад),
`shuffle` (перемешивание). Перемещение реально меняет `Rank` и влияет на достижимость скиллов.
Блокируется `IsImmobilized`. Также self-move скилла (`.move`) двигает самого исполнителя.

## 2. Модель данных

- `PullEffect` (`Mechanics/Skills/Effects/PullEffect.cs:9`) / `PushEffect` (`:9`) — эффекты.
- `ShuffleTargetEffect` (`:10`) — `IsPartyShuffle` (party vs одиночный).
- `DuelBattleEvents.Pull/Push` (`Core.Duel/DuelBattleEvents.cs:86-96`) → `MoveUnit` (`:99-125`) —
  фактическое перемещение + пересчёт `Rank`.
- `FormationParty.Units` — список, `Rank = index + 1`; `FormationUnit.Rank` — авто-свойство.

## 3. Парсинг контента

`EffectCatalog`: `.pull N` → `PullEffect`, `.push N` → `PushEffect`, `.shuffleparty` →
`ShuffleTargetEffect(true)`, `.shuffletarget` → `ShuffleTargetEffect(false)`. Self-move: `skill.Move`
(`MoveComponent` `Pullforward`/`Pushback`).

## 4. Порядок срабатывания (трассировка)

**Pull/Push** — `PullEffect.ApplyInstant` (`PullEffect.cs:24`):

1. Шанс = `chance/100 − target.Move + performer.MoveChance` (performer только если не монстр),
   клэмп 0..0.95 (`:29-36`).
2. При успехе — `Events.Pull(target, amount)` (`:39`) / `Events.Push` (`PushEffect.cs:39`).
3. `ApplyQueued` (`:46`) — попап `MoveResist` при провале.

**Фактическое перемещение** — `DuelBattleEvents.MoveUnit` (`DuelBattleEvents.cs:99-125`):

1. Если `unit == null || unit.CombatInfo.IsImmobilized` → выход (`:101`).
2. `target = clamp(index + delta, 0, Count−1)`; если не изменилось → выход (`:109-116`).
3. `Units.RemoveAt(index)` + `Units.Insert(target, unit)` (`:120-121`).
4. Пересчёт `Rank = i+1` для всех (`:124`).

**Shuffle** — `ShuffleTargetEffect.ApplyInstant` (`ShuffleTargetEffect.cs:25`): выбирает случайного
партнёра из партии и тянет/толкает через `Events.Pull/Push` на разницу рангов (`:38-41`, `:61-64`).
`ApplyQueued` (`:72`) — тот же поток через EventQueue (поштучно с move-resist).

**Self-move** — `BattleSolver.ExecuteSkill` (`BattleSolver.cs:408-413`): `skill.Move.Pullforward` →
`Events.Pull(performer, ...)`, `Pushback` → `Events.Push`; блокируется `IsImmobilized` (`:405`).

## 5. Очередь и обновления

- Pull/Push/Shuffle — мгновенно (`ApplyInstant`) или через `EventQueue` (`.queue`).
- `DuelBattleEvents.MoveUnit` — синхронная перестановка списка; порядок ходов (`OrderedUnits`) не
  меняется (инициатива остаётся как была).
- Иммунитет: `IsImmobilized` блокирует и Pull/Push, и self-move, и ручной move (`DuelController.TryMove`).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Шанс перемещения | `PullEffect.cs:29-36` | `chance/100 − Move + MoveChance`, клэмп 0..0.95 |
| Immobilize | `DuelBattleEvents.cs:101` | выход без перемещения |
| Границы партии | `:112-114` | клэмп 0..`Count−1` |
| Ранг | `:124` | пересчёт `Rank = i+1` |

## 7. Нюансы и подводные камни

- **Immobilize блокирует перемещение на трёх уровнях**: эффект (`MoveUnit`), self-move
  (`BattleSolver.cs:408`), ручной move (`DuelController.TryMove`).
- **Перемещение не меняет очередь ходов** — юнит переместился по рангу, но его инициатива в раунде
  остаётся.
- **Pull/Push двигают на `amount` позиций**, не «до ранга N» — при `amount > расстояние` юнит
  упирается в границу партии.
- Shuffle party проходит по всем юнитам (включая исполнителя), каждый двигается к случайному партнёру.
- Порядок `RemoveAt`+`Insert` критичен: после `Insert` юнит уже на новой позиции — пересчёт рангов
  по всему списку обязателен.

## 8. Взаимодействия

- Immobilize (`08_immobilize.md`) блокирует всё перемещение.
- Ранги влияют на `LaunchRanks`/`TargetRanks` (`IsLaunchableFrom`, `IsTargetableUnit`) — см.
  `01_damage.md`/`13_turn_order.md`.
- DoT/guard/status-проверки — по юниту, независимо от ранга.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/PullEffect.cs`,
  `PushEffect.cs`, `ShuffleTargetEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelBattleEvents.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs` (`Pull_...`, `Push_...`)
