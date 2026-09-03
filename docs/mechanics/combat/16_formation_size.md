# 16_formation_size.md — Размер юнита и занимаемые ранги (size 1–4)

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Монстры и боссы занимают несколько рангов формации: `display: .size N` в `Data/Monsters/*.txt`
(N = 1–4). Ранг назначается **кумулятивно по размеру**: size-2 монстр на ранге 1 занимает ранги
1–2, следующий юнит стартует с ранга 3. Формация вмещает **4 слота** (`4 − Σ size`). В WPF карточка
юнита рисуется шириной `185 × size` и занимает N слотов. Паритет legacy Unity
(`FormationParty.AddUnit`/`CreateFormation(BattleEncounter)`: `summonRank += monster.Size`).

## 2. Модель данных

- `MonsterClass.Size` (`Character/MonsterClass.cs:17`, парсинг `display: .size`,
  `MonsterClassFileParser.cs:69-72`).
- `Monster.Size` (`Character/Monster.cs:63`) → `FormationUnit.Size` (`Raid/Party/FormationUnit.cs:29`)
  → `ICombatUnit.Size`. Герои всегда size 1 (`Character.Size` виртуальный, default 1).
- `FormationParty.RecalculateRanks` (`Raid/Party/FormationParty.cs`) — кумулятивный пересчёт рангов:
  ранг 1 у первого, каждый следующий = `предыдущий.Rank + предыдущий.Size`.

## 3. Парсинг контента

`display: .size N` — в секции `info:` файла монстра. Парсится `MonsterClassFileParser` в
`MonsterClass.Size`. Примеры: `ghoul_A`/`hag_A`/`shambler_A` size 2, `cyst_D`/`drowned_captain_A`
size 3, `ancestor_pod_D`/`ancestor_heart_D` size 4.

## 4. Порядок срабатывания (трассировка)

1. **Добавление юнита** — `DuelController.AddMonster` (`DuelController.cs:210`) →
   `MonsterParty.AddUnit` (`FormationParty.cs`): ранг = `last.Rank + last.Size` (или 1, если пусто).
2. **Удаление / смерть** — `DuelController.CheckDeaths` → `RemoveDeadNonMonsters`
   (`DuelController.cs:617`) → `FormationParty.RemoveUnit` → `RecalculateRanks` (выжившие сдвигаются
   вперёд, ранги снова кумулятивные). Герои (size 1) — ранги 1..4 не меняются.
3. **Move (ручной обмен)** — `TurnMover.TryMove` (`Core.Duel/Mechanics/TurnMover.cs`) после свапа
   вызывает `party.RecalculateRanks()`.
4. **Shuffle (сюрприз)** — `SurpriseResolver.ShuffleParty` (`SurpriseResolver.cs:123`) после перестановки
   вызывает `RecalculateRanks()`.
5. **Pull/Push** — `DuelBattleEvents.MoveUnit` (`DuelBattleEvents.cs:175`) после
   `RemoveAt/Insert` вызывает `RecalculateRanks()`.
6. **Отображение** — `FormationDisplayOrder.OrderLeftToRight` сортирует по `Rank`
   (`Raid/Party/FormationDisplayOrder.cs:46`); WPF `DuelUnitViewModel.Size`/`CardWidth = 185 × Size`
   (`Wpf/ViewModels/DuelUnitViewModel.cs`), карточка `DuelUnitCardView.xaml` биндит ширину.

## 5. Очередь и обновления

- Ранги пересчитываются **мгновенно** (синхронно) при любом изменении состава/перестановки — событий
  «ранг изменился» в ядре нет, вид поллит состояние.
- `Round.OrderedUnits` (очередь ходов) строится по юнитам независимо от ранга — size не влияет на
  порядок ходов.

## 6. Проверки и клэмпы

| Проверка | Где | Границы |
|---|---|---|
| Занимаемый диапазон рангов | `FormationSet.IsLaunchableFrom/IsTargetableUnit` (`FormationSet.cs:46,55`) | `[rank, rank+size−1]` |
| Ёмкость формации | `DuelBattleContext` (герои/монстры), WPF-лобби `PveLobbyViewModel` | `Σ size ≤ 4` |
| Карточка по размеру | `DuelUnitViewModel.CardWidth` | `185 × Size` |
| Кумулятивный ранг | `FormationParty.RecalculateRanks` | первый = 1, далее `+Size` |

## 7. Нюансы и подводные камни

- **Ранги кумулятивны по `Size`, а не по индексу.** Разрыв был закрыт в ядре
  (`FormationParty.AddUnit/RemoveUnit`, `TurnMover`, `SurpriseResolver`, `DuelBattleEvents.MoveUnit`
  пересчитывают по сумме размеров). До фикса size-2 + size-1 монстры получали ранги 1,2 вместо 1,3 —
  перекрытие занимаемых диапазонов.
- **Стартовый PvE-свип бьётся по одному монстру** (`FightPveSweepTests`) — он не ловит перекрытие
  рангов; покрытие — `FormationSizeRankTests` (size-2 + size-1 → 1,3).
- **Монстры без `Stress`/некоторых резистов**: `GetPairedAttribute(Stress)` и
  `GetSingleAttribute(Disease/DeathBlow/Trap)` могут вернуть null — WPF-вид (`PveBattleViewModel`)
  защищён (`?? 0`), `DuelBattleViewModel` для героев не затронут.
- **В `StartFight` герои — «remote»** (ввод через `ApplyRemoteSkill`), монстры — «local» (AI через
  `ExecuteLocalSkill`); см. `presentation_unity_battle_view.md` §7 и `PveBattleViewModel`.
- Легаси Unity не правится; `AvailableFreeSpace`/`AvailableSummonSpace` в ядре остаются стабами
  (`GetMonsterSize` → 1, `AvailableSummonSpace` → 0) — не влияют на рендер/ранги.

## 8. Взаимодействия

- Таргетинг/запуск скиллов учитывает size: `01_damage.md`, `FormationSet`.
- Смерть/сдвиг рангов — `14_death_stress.md`, `DuelController.RemoveDeadNonMonsters`.
- Move/pull/push — `07_rank_move.md`, `08_immobilize.md`.
- AI-желания по суммарному размеру (`MonstersSizeLimit`) — `AI_BEHAVIOR.md`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Party/FormationParty.cs`,
  `FormationUnit.cs`, `FormationDisplayOrder.cs`, `Character/MonsterClass.cs`,
  `Character/MonsterClassFileParser.cs`, `Mechanics/Battle/FormationSet.cs`.
- `src/Core/Sektor.DarkestDungeon.Core.Duel/Mechanics/TurnMover.cs`, `SurpriseResolver.cs`,
  `DuelBattleEvents.cs`.
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/DuelUnitViewModel.cs`, `PveBattleViewModel.cs`,
  `Views/DuelUnitCardView.xaml`.
- `tests/Core/Sektor.DarkestDungeon.Core.Combat.Tests/FormationSizeRankTests.cs`,
  `tests/Wpf/Sektor.DarkestDungeon.Wpf.Tests/PveBattleTests.cs`.