# presentation_wpf.md — Presentation (WPF): экраны, входы, связи с ядром

> Домен: `presentation` (клиент `src\Wpf`). Статус: **реализовано**. Детальный UI — в
> `FEATURE_DESKTOP_CLIENT.md`; здесь — поток и связи, кратко.

## 1. Назначение и когда работает

WPF-клиент — тонкий потребитель ядра: экраны (меню → лобби → бой), вводы, снапшоты. Вся доменная
логика — в `Core.*`; ViewModel-слой транслирует вводы в `DuelController` и отображает состояние.

## 2. Модель данных

- Экраны: `MainMenuView`, `DuelLobbyView`, `SinglePlayerLobbyView`, `PartySelectionView`,
  `DuelBattleView`, `RaidHudView`, `ScreenHeaderView`.
- ViewModels: `MainMenuViewModel`, `SinglePlayerLobbyViewModel`, `DuelBattleViewModel`, ...
- Обёртки ИИ/сети: `AiRivalLink`, `NetworkRivalLink`, `IDuelRivalLink`.
- `Data/DuelContent` — реализация `IDuelContent` поверх каталогов.

## 3. Порядок срабатывания (трассировка)

1. Запуск → `MainWindow` (адаптивная раскладка, `*`-колонки) → меню.
2. Лобби: выбор классов/квирков/скиллов (`DuelHeroPick`), «Start Battle».
3. Бой: `DuelController.StartDuel(hostPicks, clientPicks, seed, isHost)` + `StartBattle`.
4. Ход: `DuelBattleViewModel` читает `IsLocalTurn`, применяет вводы (`ExecuteLocalSkill`), ИИ —
   `AiRivalLink` → `DuelAi.ChooseAction` → `ApplyRemoteSkill`; мультиплеер — `NetworkRivalLink`.
5. Отображение: снапшоты состояния (HP/стресс/ранги), лог (`DuelBattleEvents.Log`), попапы.

## 4. Очередь и обновления

- UI-поток (Dispatcher); `StateChanged` события боя → перерисовка.
- ИИ-ход с таймером (~0.5 с).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Ввод только на своём ходу | `IsLocalTurn` | иначе игнор |
| Адаптивная раскладка | `MainWindow` | ресайз, без Viewbox |

## 6. Нюансы и подводные камни

- **Не дублировать доменную логику во ViewModel** — вводы через `DuelController`, состояние из ядра.
- `DuelContent` (реализация `IDuelContent`) — единственное место загрузки контента в WPF.

## 7. Взаимодействия

- `duel/*` (DuelController, DuelAi), `common_random.md` (детерминизм), `clients_reader.md`
  (контент).
- `FEATURE_DESKTOP_CLIENT.md` — детали экранов/механик.
- Unity-оверлеи Тест-боя — `presentation_unity_fight.md`.

## 8. Файлы-источники

- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/*`, `ViewModels/*`, `Data/DuelContent.cs`,
  `Combat/AiRivalLink.cs`, `Networking/NetworkRivalLink.cs`
- `docs/FEATURE_DESKTOP_CLIENT.md`