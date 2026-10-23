# План: единое поле боя и порядок отряда

## Статус

- Ветка: `core/spine-hero-selector`
- Текущий срез: **общее поле боя принято; следующий — бар сцены в бою**
- Два экрана-оболочки (`DuelBattleView`, `CaptureReplayView`) остаются; свои у них — верх и низ.

## Цель

Бой (VS AI и мультиплеер) и воспроизведение захвата используют один набор контролов:

- одно поле `BattleFieldView` — карточки обеих сторон, hover и клик по цели;
- одна карточка `BattleUnitCardView` для героя и монстра;
- `BattleTargetOverlay` (бой) и `BattleSceneBarView` (replay) — слои конкретного экрана;
- `TurnOrderView` и `UnitInfoOverlayView` не меняются;
- нижние панели у экранов свои: скиллы/рейд-HUD в бою, кнопки replay, статус, аудио и журнал —
  в воспроизведении.

## Шаги

- [ ] 1. `BattleSceneBarView` в бою: сцена/комната/раунд/ход/факел/золото из `InfoCellView`, когда
  в `DuelBattleViewModel` появятся `SceneName`/`RoomName`/`Round`/`Turn`/`Gold`; верхний
  `TorchView` пока остаётся.
- [ ] 2. Replay: стрелка атаки и подпись скилла.
  - `BattleArrowView` (линия + остриё, `Show(from, to)` / `Hide()`); replay передаёт его в
    `BattleFieldView.Overlay`, `CaptureReplayViewModel` отдаёт событие `AttackStarted(attacker, target)`
    — стрелка живёт на время атаки, гаснет на `FinishAttack`/`ClearHighlights`.
  - Бейдж `АТАКА` у `BattleUnitCardView` параметризуется (`ActivityLabel`), replay подставляет
    `AttackExecuted.SkillName` (технический id).
  - Человекочитаемое имя скилла — поздний отдельный срез: `ClassId` в `CombatantState`,
    перегенерация reference-захватов, правка `SPEC_GAME_EVENTS_SCHEMA.md`.

## Затрагиваемые файлы

- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/BattleSceneBarView.xaml` / `.xaml.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/DuelBattleViewModel.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/BattleArrowView.xaml` / `.xaml.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/CaptureReplayView.xaml`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/CaptureReplayViewModel.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/BattleUnitCardView.xaml` / `.xaml.cs`

## Приёмка

- В бою бар показывает сцену/комнату/раунд/ход/факел/золото, без дубля с верхним `TorchView`.
- В replay во время атаки видна стрелка от атакующего к цели и id скилла на карточке; после
  атаки обе подсказки гаснут.
- Сборка WPF и существующие тесты проходят; визуальная проверка пользователем.
