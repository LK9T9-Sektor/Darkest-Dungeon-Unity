# 08_immobilize.md — Immobilize: блок self-move и ручного move

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.4 закрыт).

## 1. Назначение и когда работает

Иммобилизация лишает юнита способности перемещаться по рангам: блокируется и self-move скилла, и
ручной move, и эффекты pull/push. Снимается только `.unimmobilize` (истечения по таймеру нет —
паритет с Unity).

## 2. Модель данных

- `FormationUnitInfo.IsImmobilized` (`Raid/Party/FormationUnitInfo.cs:25`) — флаг.
- `ImmobilizeEffect` (`Mechanics/Skills/Effects/ImmobilizeEffect.cs:9`) — установка.
- `UnimmobilizeEffect` (`:9`) — снятие.

## 3. Парсинг контента

`EffectCatalog`: `.immobilize` → `ImmobilizeEffect`; `.unimmobilize` → `UnimmobilizeEffect`.

```text
effect: .name "Immobilize" .chance 100% .immobilize 1
effect: .name "unimmobilize" .chance 100% .unimmobilize 1 .queue false
```

## 4. Порядок срабатывания (трассировка)

**Установка** — `ImmobilizeEffect.ApplyInstant` (`ImmobilizeEffect.cs:21`):

1. Если уже `IsImmobilized` → false (`:26`).
2. Иначе `IsImmobilized = true` + `Events.SetDefendAnimation(target, true)` (`:28-29`).

**Блокировки (три места):**

1. **Self-move** — `BattleSolver.ExecuteSkill` (`BattleSolver.cs:408`): `if (skill.Move != null &&
   !performerUnit.CombatInfo.IsImmobilized)`.
2. **Ручной move** — `TurnMover.TryMove (Duel/Mechanics)` (`TurnMover.cs`): `if (unit == null ||
   unit.CombatInfo.IsImmobilized) return false;`.
3. **Pull/Push эффекты** — `DuelBattleEvents.MoveUnit` (`DuelBattleEvents.cs:101`): `if (unit == null
   || unit.CombatInfo.IsImmobilized) return;`.

**Снятие** — `UnimmobilizeEffect.ApplyInstant` (`UnimmobilizeEffect.cs:21`): `IsImmobilized = false` +
`SetDefendAnimation(target, false)` (`:28-29`). Также сбрасывается в `PrepareForBattle`
(`FormationUnitInfo.cs:90`).

## 5. Очередь и обновления

- Флаг без истечения: `FormationUnitInfo.UpdateNextRound` НЕ сбрасывает `IsImmobilized` (как в Unity).
- Снятие — только через `.unimmobilize` (или пересоздание боя).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Уже иммобилизован | `ImmobilizeEffect.cs:26` | не переустанавливать |
| Self-move | `BattleSolver.cs:408` | блокировка |
| Ручной move | `TurnMover.cs` | блокировка |
| Pull/Push | `DuelBattleEvents.cs:101` | блокировка |

## 7. Нюансы и подводные камни

- **Нет истечения по таймеру** — иммобилизация держится, пока не снимут `.unimmobilize`. Не путать
  со станом (см. `03_stun.md`), где истечение = начало хода цели.
- **Блокирует ТРИ пути перемещения** — менять только один путь нельзя (иначе юнит всё ещё двигается).
- Иммобилизация не мешает атаковать/кастовать — только перемещению.
- Сбрасывается при подготовке к бою (`PrepareForBattle`), не при `UpdateNextRound`.

## 8. Взаимодействия

- Pull/Push/Shuffle (`07_rank_move.md`) — блокируются.
- Ранги влияют на `LaunchRanks`/`TargetRanks` — застрявший юнит может потерять/получить цели.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Party/FormationUnitInfo.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/ImmobilizeEffect.cs`,
  `UnimmobilizeEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`, `DuelBattleEvents.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs` (`..._IsBlockedWhileImmobilized`)

