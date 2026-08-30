# presentation_unity_fight.md — Presentation (Unity): оверлей Тест-боя

> Домен: `presentation` (клиенты `unity\`, `unity-2017\`). Статус: **реализовано** (стенд). Детали —
> `TESTING.md` §«Тест-бой».

## 1. Назначение и когда работает

Тонкий Unity-оверлей поверх TEST-меню для прогона боя на ядре (`FightSession`). Строит
спецификации юнитов, запускает `DuelController.StartFight`, отображает бой, водит героев вручную
или автобоем.

## 2. Модель данных

- `FightContentLoader` (`unity/Assets/Scripts/UI/Testing/FightContentLoader.cs`) — `Resources` →
  `TextFightContent` (файлы → каталоги через `GameDataReader`).
- `FightScreen` — оверлей конфигурации/боя; `FightBattleView` — карточки/скиллы/цели/лог.

## 3. Порядок срабатывания (трассировка)

1. TEST-меню → «Fight tester» → `FightScreen` (2×4 слота, seed, режим ИИ/Игра, FIGHT).
2. `FightContentLoader` читает `Resources/Data/*` → `TextFightContent`.
3. `FightSession.Start(playerSide, aiSide)` → `DuelController.StartFight` + `StartBattle`.
4. Ручной режим: клик по скиллу → `FightPlayerAction` → `FightSession.Tick(manual)`;
   AUTO — `RunToCompletion`/`Tick(null)`.
5. Победитель/завершение — баннер + возврат в конфигурацию.

## 4. Очередь и обновления

- Пошаговый `Tick` в Update/кнопках; автобой — последовательные тики.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Режим «Игра» | `FightSession.IsWaitingForPlayerAction` | ручной ввод |
| Режим AUTO | `Tick(null)` | авто-герои через `DuelAi` |

## 6. Нюансы и подводные камни

- **Legacy Unity не правится** — оверлей только слой представления; вся логика в ядре.
- `FightContentLoader` дублируется в двух деревьях (`unity`/`unity-2017`).

## 7. Взаимодействия

- `duel_05_fight.md`, `duel_06_content.md`, `clients_reader.md`.
- `TESTING.md` §«Тест-бой» — ручная проверка.

## 8. Файлы-источники

- `unity/Assets/Scripts/UI/Testing/FightContentLoader.cs`, `FightScreen.cs`, `FightBattleView.cs`
- `unity-2017/...` (аналогично)
- `docs/TESTING.md`