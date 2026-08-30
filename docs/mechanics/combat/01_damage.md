# 01_damage.md — Урон / хил / крит / меткость

> Домен: `combat` (ядро `Core.Combat` + оркестрация `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Основная ветка исполнения скилла: определить, попадает ли атака (меткость vs уклонение), нанести
урон (или вылечить), учесть крит (×1.5) и применить эффекты скилла. Срабатывает при каждом
`BattleSolver.ExecuteSkill` (дуэль: `DuelController.ExecuteSkill`). Ветка `Damage`/`Heal`/`Support`
выбирается по `skill.Category`.

## 2. Модель данных

- `BattleSolver` (`Core.Combat/Mechanics/Battle/BattleSolver.cs:12`) — исполнение;
  `SkillResult`/`SkillResultEntry` — результат (тип `Hit/Miss/Crit/Dodge/Heal/CritHeal/Utility`).
- `Character` (`Character/Character.cs:70,79,210,230`) — `HealthRatio`, `HasZeroHealth`,
  `Heal`, `TakeDamage`; атрибуты `Accuracy/Dodge/Protection/MinDamage/MaxDamage/CritChance`.
- `CombatSkill` — `Category` (Damage/Heal/Support), `Accuracy`, `DamageMod`, `CritMod`,
  `IsCritValid`, `CanMiss`, `Heal` (`HealComponent`), `Effects`.

## 3. Парсинг контента

Скилл из `Heroes/*.bytes` (`HeroClassFileParser`): `.atk` → Accuracy, `.dmg` → DamageMod, `.crit` →
CritMod, `.is_crit_valid`, `.heal` → HealComponent, `.effect` → `Effects` (резолв через `EffectCatalog`).

## 4. Порядок срабатывания (трассировка)

`DuelController.ExecuteSkill` (`DuelController.cs:443`) →
`BattleSolver.ExecuteSkill` (`BattleSolver.cs:388`):

1. **Guard-редирект** (`BattleSolver.cs:396-399`): если цель — враг и под `Guarded` → цель заменяется
   на `Guarded.Guard` **до** всех расчётов.
2. `SkillsUsedThisTurn/SkillsUsedInBattle` пополняются (`:390-391`).
3. `ApplyConditions(performer, target, skill)` (`:403`) — применяет rule-баффы обоих.
4. Self-move скилла (`:405-410`): `skill.Move.Pullforward/Pushback` (если не `IsImmobilized`) →
   `Events.Pull/Push` (реальное перемещение рангов).
5. **Ветка Heal/Support** (`:406-440`):
   - `initialHeal = RandomSolver.Next(min, max+1) * (1 + HpHealPercent)` (`:417`).
   - Крит-хил (`IsCritValid`, шанс = `CritChance + CritMod/100`): `Heal(initialHeal*1.5)` → entry
     `CritHeal`, применяются эффекты + `ApplyEffectById("crit_heal_stress_heal")` (`:422-431`).
   - Обычный хил: `Heal(initialHeal, true)` → entry `Heal` → `ApplyEffects` (`:436-440`).
   - Без `Heal` — entry `Utility` + `ApplyEffects` (`:444-445`).
6. **Ветка Damage** (`:449-507`):
   - `hitChance = Clamp(accuracy − target.Dodge, 0, 0.95)` (`:451`); `CanBeHit=false` → промах
     гарантирован (`:453`).
   - `roll > hitChance` → Miss/Dodge (`roll > min(accuracy,0.95)` → Miss, иначе Dodge) + эффекты,
     выход (`:456-466`). `CanMiss=false`/`CanBeMissed=false` — не промахивается.
   - `initialDamage`: герой — `Lerp(MinDmg,MaxDmg,rand)*(1+DamageMod)`; монстр —
     `Lerp(skill.DamageMin,Max)*(DamageMod)` (`:470-471`).
   - `damage = Ceil(initialDamage*(1−Protection))` (`:474`); `CanBeDamagedDirectly=false` → 0 (`:478`).
   - Крит (`IsCritValid`, шанс `CritChance + CritMod`): `TakeDamage(damage*1.5)` (`:486`), entry
     `Crit`, эффекты + **крит-стресс** `ApplyEffectById("Stress 2")` для героев (`:496`).
   - Обычный удар: `TakeDamage(damage)` (`:500`), entry `Hit` (или `Hit` с `IsZeroed` при `HasZeroHealth`),
     эффекты (`:507`).
7. Возврат в `DuelController.ExecuteSkill`: `ProcessEventQueues()` (квеянные эффекты) → `CheckDeaths()`
   → `ExecuteRiposte` (контратака) → `RemoveConditions(performer, target)` (`DuelController.cs:443-490`).

## 5. Очередь и обновления

- Эффекты скилла применяются **сразу** (`ApplyEffects`, `:566`) — мгновенно, пока `queue` не задан.
- Квеянные эффекты (`EffectBoolParams.Queue`) попадают в `target.EventQueue` и исполняются после
  `ProcessEventQueues` в `DuelController` (`:575-583`).
- Урон/хил применяются к HP до эффектов (крит-стресс и эффекты — после фактического урона).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Меткость | `DamageResolver.cs (Mechanics/Battle)` | `acc−dodge`, клэмп 0..0.95 |
| Урон | `:474` | `Ceil(Lerp·(1+DmgMod)·(1−prot))`, минимум 0 |
| Крит-шанс героя | `:483` | `CritChance + skill.CritMod` |
| Крит-шанс хила | `:422` | `CritChance + skill.CritMod/100` |
| HpHealPercent | `:417` | без клэмпа (модификатор) |

## 7. Нюансы и подводные камни

- **Guard-редирект происходит до урона** — защитник принимает урон вместо охраняемого, и крит-стресс
  тоже идёт по защитнику.
- **Крит-стресс** (`Stress 2`) применяется **только к героям-целям** (`IsMonster == false`, `:496`).
- **`CanMiss`/`CanBeMissed`/`CanBeHit`** — `BattleModifiers` монстров; `CanBeDamagedDirectly=false`
  обнуляет урон, но эффекты всё равно применяются.
- `HasZeroHealth` не проверяется при TakeDamage-уроне — смерть определяется в `CheckDeaths`.
- Self-move скилла выполняется **до** расчёта урона, но **после** guard-редиректа.

## 8. Взаимодействия

- `RemoveConditions` после скилла снимает rule-баффы (см. `09_buffs.md`).
- `ExecuteRiposte` (см. `04_riposte.md`) — после попадания по рипост-юниту.
- DoT-смерть и `CheckDeaths` — см. `02_dot.md`, `14_death_stress.md`.
- Guard-редирект — см. `05_guard.md`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Character.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/HeroClassFileParser.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Combat.Tests/Mechanics/BattleSolverTests.cs`


