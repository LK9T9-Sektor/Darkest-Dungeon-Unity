# duel_04_ai.md — DuelAi: default-брейн для противника vs AI

> Домен: `duel` (ядро `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Автоматизирует ходы стороны-соперника в vs-AI (WPF) и для героев-ИИ в `FightSession`. Строит
DD-«default» брейн из weighted desires и гоняет DD-цикл выбора (см. `AI_BEHAVIOR.md`). Монстры в
`FightSession` используют свои кампанийные мозги (`UseMonsterBrain`).

## 2. Модель данных

- `DuelAi` (`Core.Duel/DuelAi.cs:14`) — `brain` (`MonsterBrain`), `ChooseAction(duel)` (`:27`).
- Desires: `DuelSkillSelectionHeal` (`:57`), `DuelSkillSelection` (`:58`), `DuelTargetSelectionRandom`
  (`:59`), `DuelTargetSelectionMarked` (`:60`), `DuelTargetSelectionHealth` (`:61`).
- `MonsterBrainDecision` (`BrainDecisionType`, `SelectedSkill`, `TargetInfo`).

## 3. Порядок срабатывания (трассировка)

`ChooseAction` (`DuelAi.cs:27-52`):

1. `performer = duel.CurrentUnit`; если null/нет контекста → `PassAction` (`:29-31`).
2. Цикл по `brain.SkillDesireSet`: `RandomSolver.ChooseByRandom(desire)` → `SelectSkill(performer,
   decision, duel.Context)`; при успехе — кулдаун, выход из цикла (`:33-46`).
3. Если нет Perform/скилла/целей → `PassAction` (`:48-49`).
4. Иначе `DuelPayload.Skill(skillId, firstTargetId)` (`:51`).

`BuildDefaultBrain` (`:54-63`): heal-желание (HP < 0.5, chance 100) + random-skill; цели —
random(2)/marked(1)/health-ally(100).

## 4. Очередь и обновления

- Выбор детерминирован: `RandomSolver` с сидом сессии.
- Кулдауны добавляются в `performer.CombatInfo.SkillCooldowns` (`:40-42`).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Нет юнита/контекста | `DuelAi.cs:29-31` | `PassAction` |
| Нет легального скилла | `:48` | `PassAction` |
| Кулдаун | `:40-42` | добавляется после выбора |

## 6. Нюансы и подводные камни

- **`DuelAi` — отдельный брейн от монстров кампании**: строится кодом (`BuildDefaultBrain`), не из
  `JsonAI.json`. Монстры (`FightSession`) — через `BattleSolver.UseMonsterBrain` (кампанийные мозги).
- Выбор скилла не проверяет `IsSkillUsable` явно — полагается на desires (см. `AI_BEHAVIOR.md`).
- Хилер лечит союзника < 50% HP (`DuelSkillSelectionHeal`).

## 7. Взаимодействия

- `AI_BEHAVIOR.md` — модель DD-ИИ (desires, кулдауны, цикл).
- `duel_05_fight.md` — как DuelAi используется в автобое.
- `RandomSolver` — `common/RandomSolver`.

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelAi.cs`, `DuelSkillSelection.cs`,
  `DuelSkillSelectionHeal.cs`, `DuelTargetSelectionRandom.cs`, `DuelTargetSelectionMarked.cs`,
  `DuelTargetSelectionHealth.cs`
- `docs/AI_BEHAVIOR.md`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/DuelAiTests.cs`