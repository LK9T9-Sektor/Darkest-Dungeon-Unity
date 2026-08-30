# duel_05_fight.md — FightSession: автобой героев против монстров

> Домен: `duel` (ядро `Core.Duel\Fight`). Статус: **реализовано**.

## 1. Назначение и когда работает

Автоматический раннер боя (Тест-бой, стенд): строит `DuelController`, стартует бой и водит юнитов
автономно (герои — через `DuelAi` или ручные вводы, монстры — через кампанийные мозги
`UseMonsterBrain`) до конца. Используется `TextFightContent` как `IDuelContent`.

## 2. Модель данных

- `FightSession` (`Core.Duel/Fight/FightSession.cs:15`) — `Duel` (`DuelController`), `seed`,
  `rivalAi` (`DuelAi`), `IsStarted`/`IsFinished`/`IsWaitingForPlayerAction`.
- `FightUnitSpec`/`HeroFightUnitSpec`/`MonsterFightUnitSpec` — спецификации юнитов сторон.
- `FightPlayerAction` — ручной ввод (`SkillId`, `TargetCombatId`).

## 3. Порядок срабатывания (трассировка)

1. **Старт** — `Start(playerSide, aiSide)` (`:61-67`): `Duel.StartFight(playerSide, aiSide, seed)` +
   `Duel.StartBattle()`.
2. **Тик** — `Tick()`/`Tick(manual)` (`:71-115`):
   - если не в фазе действия → `false` (`:88-89`);
   - ручной ввод: `Duel.ApplyRemoteSkill(DuelPayload.Skill(...))` (`:96`);
   - авто: `DecideAction()` (`:134-156`): монстр → `Duel.Solver.UseMonsterBrain(unit)`
     (`:142-152`); иначе → `rivalAi.ChooseAction(Duel)` (`:155`);
   - `ExecuteLocalSkill` (если локальный ход) или `ApplyRemoteSkill`; невалидный → `ExecuteLocalPass`
     (`:104-112`).
3. **До конца** — `RunToCompletion()` (`:118-123`): цикл `Tick(null)`.

`IsWaitingForPlayerAction` (`:44-56`): true, когда ждёт ручного ввода героя (host-фаза,
`IsPlayerControlledActor`).

## 4. Очередь и обновления

- Пошаговый `Tick` — один юнит за вызов; автобой через `RunToCompletion`.
- `DuelPayload` — как в `duel_02_payload.md`.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Не стартовано/кончено | `:85-89` | `false` |
| Ручной ввод вне ожидания | `:93-94` | игнор (`false`) |
| Невалидный авто-скилл | `:106-108` | `ExecuteLocalPass` |

## 6. Нюансы и подводные камни

- **`IsWaitingForPlayerAction` гейтит ручной ввод** — ввод принимается только для живого героя
  host-стороны в фазе `WaitingForHostAction`.
- Авто-герои ходят через `DuelAi` (default brain), не через кампанийные мозги.
- Монстры используют `UseMonsterBrain` (кампанийные брейны из `JsonAI.json`) — это ключевое отличие
  от дуэли (там обе стороны герои).

## 7. Взаимодействия

- `IDuelContent`/`TextFightContent` (`duel_06_content.md`), `DuelAi` (`duel_04_ai.md`).
- Боевые механики — `combat/*`.
- Unity-стенд — `unity\Assets\Scripts\UI\Testing\FightContentLoader.cs`/`FightScreen.cs`.

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/Fight/FightSession.cs`, `FightUnitSpec.cs`,
  `HeroFightUnitSpec.cs`, `MonsterFightUnitSpec.cs`, `FightPlayerAction.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/Fight/TextFightContent.cs`
- `docs/TESTING.md` §«Тест-бой»