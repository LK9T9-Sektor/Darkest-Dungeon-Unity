# 11_modes.md — Modes (Абоминация): human/beast, continue-turn

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.8).

## 1. Назначение и когда работает

Режимы (modes) персонажа: герой стартует в raid-default моде; `transform`-скилл переключает
`CurrentMode` (human ↔ beast), открывая скиллы, ограниченные `valid_modes`. Скиллы с
`.is_continue_turn` дают повторный ход. Каждый режим может иметь собственные эффекты скилла
(`<mode>_effects`).

## 2. Модель данных

- `Hero.CurrentMode` (`Character/Hero.cs:146`) — старт в `IsRaidDefault`-моде; `HeroClass.Modes`.
- `CombatSkill.ValidModes` (`Mechanics/Skills/CombatSkill.cs`) — допустимые режимы.
- `CombatSkill.ModeEffects` — эффекты по режиму.
- `CharacterMode` — `Id`, `IsRaidDefault` (из `HeroClassFileParser`).

## 3. Парсинг контента

- `HeroClassFileParser`: `mode:` секции → `HeroClass.Modes` с `is_raid_default` (`:110-117`);
  `.valid_modes` → `Skill.ValidModes` (`:377-380`); `.X_effects` → `ModeEffects[X]`
  (`:347-369`, ключ `<mode>_effects`, режим без `_`).
- `EffectCatalog`: `.set_mode <mode>` → `SetModeEffect` (`EffectCatalog.cs:212-214`); `.on_miss`,
  `.queue`, `.apply_once` → флаги эффекта.

## 4. Порядок срабатывания (трассировка)

**Старт** — `Hero` конструктор выбирает `IsRaidDefault`-мод (`Hero.cs:143-148`).

**Usability** — `BattleSolver.IsSkillUsable` (`BattleSolver.cs:50-64`): `IsValidInCurrentMode` —
если `ValidModes.Count > 0`, то `CurrentMode.Id` должен входить в `ValidModes`.

**Применение эффектов режима** — `BattleSolver.ApplyEffects` (`:566-573`): если
`ValidModes.Count > 1 && CurrentMode != null` — сначала эффекты `ModeEffects[CurrentMode.Id]`, потом
общие `Effects`.

**Переключение** — `SetModeEffect` (`Mechanics/Skills/Effects/SetModeEffect.cs`) меняет
`CurrentMode`.

**Continue-turn** — `DuelController.FinishSkillAction` (`DuelController.cs:419-427`):
`if (skill.IsContinueTurn && !unit.CombatInfo.IsDead) BeginTurn(); else CompleteTurn();` — тот же юнит
действует снова.

**AI** — `BattleSolver.UseMonsterBrain` (герой-ветка, `:293-295`) фильтрует скиллы по
`ValidModes.Contains(CurrentMode.Id)`.

## 5. Очередь и обновления

- Переключение режима — мгновенно (SetModeEffect).
- Continue-turn — в рамках того же хода: `BeginTurn` вызывается повторно без `CompleteTurn`.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Скилл в моде | `BattleSolver.cs:57-64` | `ValidModes` пуст → любой режим |
| Эффекты режима | `:566-573` | только если `ValidModes.Count > 1` |
| Continue-turn | `DuelController.cs:421` | `IsContinueTurn && !IsDead` |
| Стартовая мода | `Hero.cs:143-148` | `IsRaidDefault` |

## 7. Нюансы и подводные камни

- **Правило «без accuracy-ролла»**: transform-скиллы — Support-категория (accuracy 0/self-target),
  выставляется в `HeroClassFileParser` (см. `01_damage.md`).
- **`ValidModes.Count > 1`** — условие для `ModeEffects`; если мод ровно один, эффекты не применяются
  по ветке модов.
- Continue-turn юнит действует снова **в том же раунде** (не снимает кулдауны).
- Мёртвый юнит не получает continue-turn (`!IsDead`).

## 8. Взаимодействия

- AI-выбор скилла (`AI_BEHAVIOR.md`) учитывает `ValidModes`.
- Support-категория и accuracy — `01_damage.md`.
- `RemoveConditions` после скилла снимает rule-баффы, режим не трогает.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Hero.cs`, `HeroClass.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/HeroClassFileParser.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/EffectCatalog.cs`,
  `Mechanics/Skills/Effects/SetModeEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ModeTests.cs`