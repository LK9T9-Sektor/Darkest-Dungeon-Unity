# duel_04_ai.md — FallbackDuelAi: default-брейн для противника vs AI

> Домен: `duel` (ядро `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Автоматизирует ходы стороны-соперника в vs-AI (WPF) и для героев-ИИ в `FightSession`. Строит
DD-«default» брейн из weighted desires и гоняет DD-цикл выбора (см. `AI_BEHAVIOR.md`). Монстры в
`FightSession` используют свои кампанийные мозги (`UseMonsterBrain`).

## 2. Модель данных

- `FallbackDuelAi` (`Core.Duel/FallbackDuelAi.cs:14`) — `brain` (`MonsterBrain`), `ChooseAction(duel)` (`:27`).
- Desires: `DuelSkillSelectionHeal` (`:57`), `DuelSkillSelection` (`:58`), `DuelTargetSelectionRandom`
  (`:59`), `DuelTargetSelectionMarked` (`:60`), `DuelTargetSelectionHealth` (`:61`).
- `MonsterBrainDecision` (`BrainDecisionType`, `SelectedSkill`, `TargetInfo`).

## 3. Порядок срабатывания (трассировка)

`ChooseAction` (`FallbackDuelAi.cs:27-59`):

1. `performer = duel.CurrentUnit`; если null/нет контекста → `PassAction` (`:29-31`).
2. Цикл по `brain.SkillDesireSet`: `RandomSolver.ChooseByRandom(desire)` → `SelectSkill(performer,
   decision, duel.Context)`; при успехе — кулдаун, выход из цикла (`:33-46`).
3. Если есть Perform/скилл/цели → `DuelPayload.Skill(skillId, firstTargetId)` (`:48-53`).
4. Иначе — легальный move (`duel.GetLegalMoveRank`, `:55-57`) → `DuelPayload.MoveAction`.
5. Иначе → `PassAction` (`:59`).

`BuildDefaultBrain` (`:54-63`): heal-желание (HP < 0.5, chance 100) + random-skill; цели —
random(2)/marked(1)/health-ally(100).

## 4. Очередь и обновления

- Выбор детерминирован: `RandomSolver` с сидом сессии.
- Кулдауны добавляются в `performer.CombatInfo.SkillCooldowns` (`:40-42`).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Нет юнита/контекста | `FallbackDuelAi.cs:29-31` | `PassAction` |
| Нет легального скилла | `:55-59` | move, затем `PassAction` |
| Кулдаун | `:40-42` | добавляется после выбора |

## 6. Нюансы и подводные камни

- **`FallbackDuelAi` — отдельный брейн от монстров кампании**: строится кодом (`BuildDefaultBrain`), не из
  `JsonAI.json`. Монстры (`FightSession`) — через `BattleSolver.UseMonsterBrain` (кампанийные мозги).
- Выбор скилла не проверяет `IsSkillUsable` явно — полагается на desires (см. `AI_BEHAVIOR.md`).
- Хилер лечит союзника < 50% HP (`DuelSkillSelectionHeal`).

## 7. Взаимодействия

- `AI_BEHAVIOR.md` — модель DD-ИИ (desires, кулдауны, цикл).
- `duel_05_fight.md` — как FallbackDuelAi используется в автобое.
- `RandomSolver` — `common/RandomSolver`.

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/FallbackDuelAi.cs`, `DuelSkillSelection.cs`,
  `DuelSkillSelectionHeal.cs`, `DuelTargetSelectionRandom.cs`, `DuelTargetSelectionMarked.cs`,
  `DuelTargetSelectionHealth.cs`
- `docs/AI_BEHAVIOR.md`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/FallbackDuelAiTests.cs`