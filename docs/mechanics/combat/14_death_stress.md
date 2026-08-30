# 14_death_stress.md — Смерть, стресс отряда, resolve-ролл, heart attack

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **частично** (resolve-ролл работает;
> death's door / heart attack — отдельная задача).

## 1. Назначение и когда работает

После урона/тиков проверяется смерть юнитов (`HealthRatio <= 0`); смерть героя даёт стресс
выжившим союзникам. При стресс >= 100 герой проходит resolve-ролл (аффекция/виртуда). Death's door
и heart attack — **не реализованы** в дуэли (0 HP = смерть; `AddHeartAttackCheck` — только событие).

## 2. Модель данных

- `DuelController.CheckDeaths` (`DuelController.cs:549`) — сбор умерших, `StressParty`.
- `DuelController.StressParty` (`:696`) — стресс союзникам.
- `DuelBattleContext.ResolveOverstress` (`DuelBattleContext.cs:143`) — resolve-ролл.
- `StressEffect` (`Mechanics/Skills/Effects/StressEffect.cs:10`) — стресс; `HandleOverstress`
  (`:112-124`) — resolve/heart attack события.
- `Hero` — `IsStressed` (`>=50`), `IsOverstressed` (`>=100`), `IsAfflicted`/`IsVirtued` (`Hero.cs:45-54`),
  `ApplyTrait`/`RevertTrait` (`:152,165`).
- `DeathsDoorStatusEffect`/`DeathRecoveryStatusEffect` (`Character/Statuses/`) — флаги, но не
  устанавливаются в дуэли.

## 3. Парсинг контента

`Effects.txt`: `Stress 2` (15), `AfflictedAllyStress` (33%×5), `crit_heal_stress_heal` (4).
`JsonTraits.json` → `Trait` (аффекции/виртуды, `buff_ids`). `JsonBuffs.json` → trait-баффы.

## 4. Порядок срабатывания (трассировка)

**Смерть** — `DuelController.CheckDeaths` (`DuelController.cs:549`) вызывается из `ExecuteSkill`
(`:575`), `ExecuteRiposte` (`:597`), `BeginTurn` после DoT-тиков (`:345`):

1. Для всех юнитов: `HealthRatio <= 0 && !IsDead` → `IsDead = true`, в список (`:676-681`).
2. Для каждого умершего **героя** (`:684-688`): `StressParty(party)` (`:696-710`) — эффект `Stress 2`
   каждому живому герою отряда (не монстру), затем `ResolveOverstress` (`:706`).

**Resolve-ролл** — `DuelBattleContext.ResolveOverstress` (`DuelBattleContext.cs:143-198`):

1. Гейт: не монстр, `IsOverstressed`, не аффектирован/виртуед (`:145-148`).
2. Шанс виртуды = `0.25 + ResolveCheckPercent`, клэмп 0.01..0.6 (`:150-155`).
3. Случайная черта: `GetVirtues()`/`GetAfflictions()` → случайный `Trait` (`:158-161`).
4. `ApplyTrait(trait, buffs)` — permanent trait-баффы (`:171-173`).
5. Виртуда: стресс → 20–40 (`:175-184`). Аффекция: `AfflictedAllyStress` союзникам (`:187-196`).

**Стресс по событиям** — `StressEffect.HandleOverstress` (`StressEffect.cs:112-124`): если
`IsOverstressed`: если не аффектирован → `AddResolveCheck`; иначе → `AddHeartAttackCheck` (только
событие/лог, исполнения нет).

## 5. Очередь и обновления

- `CheckDeaths` — после каждого скилла, контратаки и DoT-тиков.
- `StressParty` — мгновенно (по `Stress 2`), до resolve-ролла.
- `ResolveOverstress` — мгновенно в конце `StressParty`.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Смерть | `DuelController.cs:553` | `HealthRatio <= 0 && !IsDead` |
| Стресс союзникам | `:696-710` | только герои, не мёртвые |
| Resolve-шанс | `DuelBattleContext.cs:150-155` | `0.25 + ResolveCheck`, клэмп 0.01..0.6 |
| Виртуда-стресс | `:177-183` | 20–40 |

## 7. Нюансы и подводные камни

- **`ResolveOverstress` вызывается только внутри `StressParty`** (смерть героя) и из
  `ApplyEffectById` (`DuelBattleContext.cs:129`) — после крит-стресса. Прямого вызова при
  «стресс достиг 100» в общем случае нет — только через эти точки.
- **Heart attack не реализован** — `AddHeartAttackCheck` пишет в лог (`DuelBattleEvents.cs:63-66`).
- **Death's door не реализован** — 0 HP = смерть; `DeathsDoorStatusEffect.AtDeathsDoor` нигде не
  устанавливается в дуэли (grep: только объявление).
- Мёртвый юнит: `IsDead` — флаг `FormationUnitInfo`; сам юнит остаётся в партии (для стресса/лога).
- `StressParty` использует `Effects["Stress 2"]` — если эффект отсутствует в контенте, стресс не
  применяется (`DuelController.cs:Mechanics/DeathCheck`).

## 8. Взаимодействия

- DoT-смерть (`02_dot.md`), крит-стресс (`01_damage.md`).
- Guard/riposte: смерть после контратаки (`04_riposte.md`).
- Resolve/стресс — `GAME_RULES.md` §«Стресс».

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`CheckDeaths`, `StressParty`)
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelBattleContext.cs` (`ResolveOverstress`)
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/StressEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Hero.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/DeathsDoorStatusEffect.cs`,
  `DeathRecoveryStatusEffect.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/StressTests.cs`

