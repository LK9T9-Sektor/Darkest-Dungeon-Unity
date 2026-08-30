# 04_riposte.md — Riposte: статус, контратака, `riposte_skill`

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.5 закрыт).

## 1. Назначение и когда работает

Юнит с активным рипост-статусом **контратакует** атакующего после того, как по нему попали.
Контратака исполняет `RiposteSkill` юнита против атакующего. Работает для героев и монстров.

## 2. Модель данных

- `RiposteStatusEffect` (`Character/Statuses/RiposteStatusEffect.cs:6`) — `RiposteDuration`,
  `DurationType` (`Combat`/`Round`), `IsApplied = RiposteDuration > 0`, `UpdateNextTurn` (декремент,
  кроме `DurationType.Combat`, `:21-28`).
- `RiposteEffect` (`Mechanics/Skills/Effects/RiposteEffect.cs:11`) — наложение статуса + stat-баффы
  (`StatAddBuffs`/`StatMultBuffs` с `BuffRule.Riposting`).
- `Hero.RiposteSkill` (`Character/Hero.cs:69`) → `HeroClass.RiposteSkill`
  (`Character/HeroClass.cs:32`); `Monster` аналогично.

## 3. Парсинг контента

- `EffectCatalog`: ключ `.riposte` → `RiposteEffect` (опционально стат-модификаторы рядом).
- `HeroClassFileParser`: блок `riposte_skill:` → `HeroClass.RiposteSkill` (скилл уровня 0,
  `:80-86` в парсере). Пример (ManAtArms):
  `riposte_skill: .id "riposte1" .level 0 .type "melee" .atk 90% .dmg -40% .launch 1234 .target 1234`.

## 4. Порядок срабатывания (трассировка)

**Наложение** — `RiposteEffect.ApplyInstant` (`RiposteEffect.cs:30`):

1. `duration = effect.Duration ?? 1`; `DurationType = Combat`, если `.duration` отсутствует
   (`:40` — см. `TagEffect`-паттерн: `Combat` = пока бой); иначе `Round`.
2. `RiposteDuration = duration` (`:44`).
3. stat-баффы накладываются с `BuffRule.Riposting` (`:46-53`), которые активны, только когда
   `rules.IsRiposting` (см. `09_buffs.md`).

**Контратака** — `DuelController.ExecuteSkill` (`DuelController.cs:571`):

1. `Solver.ExecuteSkill(unit, target, skill)` → урон/хил/эффекты (`:573`).
2. `ProcessEventQueues()` (`:574`), `CheckDeaths()` (`:575`).
3. `ExecuteRiposte(unit, target)` (`DuelController.cs:585-600`):
   - если цель мертва → выход;
   - если `Riposte.IsApplied` и `target.RiposteSkill != null` → `Solver.ExecuteSkill(target, attacker,
     riposteSkill, null)` + `ProcessEventQueues` + `CheckDeaths`.
4. `RemoveConditions(unit, target)` (`:602-608`) — после контратаки.

**Истечение** — `RiposteStatusEffect.UpdateNextTurn` (`:21-28`): декремент per-turn, кроме
`DurationType.Combat` (не истекает до конца боя).

## 5. Очередь и обновления

- Контратака выполняется **после** основной атаки и проверки смертей, **до** `RemoveConditions`.
- Рипост-скилл сам может нанести урон и применить эффекты (через тот же `Solver.ExecuteSkill`).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Статус применён | `DuelController.cs:588` | `Riposte.IsApplied` |
| Скилл есть | `:590` | `RiposteSkill != null` |
| Цель жива | `:586` | не `IsDead` |
| Длительность | `RiposteEffect.cs:40-44` | `duration ?? 1`; `Combat` = без истечения |

## 7. Нюансы и подводные камни

- **Контратакует ТОЛЬКО цель удара** (в дуэли — одиночная цель). Мультитаргет: контратака за каждого
  попадающего — не реализовано (в дуэли `ExecuteSkill` — одна цель).
- **Рипост не срабатывает на промах по рипост-юниту** — контратака триггерится после `ExecuteSkill`
  независимо от того, попал ли атакующий (Unity исполняет после скилла; здесь — всегда, т.к.
  `ExecuteRiposte` не проверяет `SkillResult`). Это допустимое расхождение (не паритет-разрыв).
- **`DurationType.Combat`** — статус живёт до конца боя (не истекает по `UpdateNextTurn`).
- stat-баффы рипоста активны только пока юнит «рипостит» (`BuffRule.Riposting`), гейт в
  `BattleSolver.ApplyConditions` (`IsRiposte`).

## 8. Взаимодействия

- Guard: контратака идёт по атакующему, а не по охраняемому (guard-редирект применяется к атаке,
  не к контратаке).
- RemoveConditions (см. `09_buffs.md`) вызывается после контратаки — снимает rule-баффы.
- `BuffRule.Riposting` — см. `09_buffs.md`; инициатива/очередь — `13_turn_order.md`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/RiposteStatusEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/RiposteEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Hero.cs`, `HeroClass.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/HeroClassFileParser.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`ExecuteRiposte`)
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs` (`Riposte_...`)