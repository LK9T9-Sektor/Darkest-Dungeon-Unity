# duel_02_payload.md — DuelPayload: wire-формат вводов

> Домен: `duel` (ядро `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

Единственный формат данных, передаваемых по сети между сторонами дуэли. Сериализует действие хода
в строку и парсит обратно. Применяется через `DuelController.ApplyRemoteSkill`/`ExecuteLocal*`.

## 2. Модель данных

`DuelPayload` (`Core.Duel/DuelPayload.cs:9`) — статические фабрики строк:
- `Skill(skillId, targetId)` (`:18`) → `"skillId|targetId"`;
- `MoveAction(rank)` (`:26`) → `"move|rank"`;
- `PassAction()` (`:33`) → `"pass|0"`.

Константы формата (`DuelPayload.Pass`/`Move`) — маркеры первой части строки.

## 3. Порядок срабатывания (трассировка)

1. Локальная сторона исполняет действие → возвращает payload (`ExecuteLocalSkill`/`Pass`/`Move`).
2. Payload передаётся удалённой стороне → `ApplyRemoteSkill(payload)` (`DuelController.cs:327`):
   - `split('|')` (`:454`);
   - `"pass"` → `CompleteTurn` (`:458-463`);
   - `"move"` → `int.TryParse(rank)` + `TryMove(CurrentUnit, rank)` → `CompleteTurn` (`:464-470`);
   - иначе `"skillId|targetId"` → найти юнит/скилл/цель, `ExecuteSkill` + `FinishSkillAction`
     (`:472-477`).

## 4. Очередь и обновления

- Payload — мгновенное применение (без очередей).
- Парсинг не валидирует существование цели до поиска (невалидное → тихий `return`).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Формат | `ApplyRemoteSkill` | `|`-разделение; 1–2 части |
| `move` | `:464-470` | ранг int, `TryMove` |
| `skill` | `:472-477` | ровно 2 части |

## 6. Нюансы и подводные камни

- **`pass`/`move` обрабатываются до проверки частей** — `skill` требует `parts.Length == 2`.
- `TryMove` возвращает false (например, immobilize) — `CompleteTurn` не вызывается, ход остаётся.
- Payload не содержит сида/снапшота — состояние считается детерминированно (см. `duel_01_lockstep.md`).

## 7. Взаимодействия

- Lockstep (`duel_01_lockstep.md`), AI (`duel_04_ai.md`), FightSession (`duel_05_fight.md`).

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelPayload.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`ApplyRemoteSkill`)
