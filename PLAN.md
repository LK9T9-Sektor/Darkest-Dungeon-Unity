# PLAN.md — Полный HUD боя в живом DuelBattleView

## Цель

Встроить в живой экран дуэли (`DuelBattleView`) полный HUD из мокапа `BattleScreenView`
(три панели), подключив его к реальному состоянию `DuelController`:
верх — квест+Retreat / факел / очередь хода; центр — поле, раунд, тултип при наведении,
лист статов по правому клику, статус, лог; низ — скиллы ходящего, инфо ходящего,
тултип, инвентарь/карта (плейсхолдеры). Мокап-вью/VM переиспользуются, ядро не трогается.

## Решения

- Скиллы ходящего — **отдельный кликабельный ряд** над нижней панелью.
- Инвентарь/карта/факел — **плейсхолдеры** (в дуэли их нет).
- Реальные HP (не проценты); тултип/статы читают живые атрибуты персонажа.

## Шаги

1. [ ] **S1** `DuelBattleView.xaml`: раскладка в 3 панели как у `BattleScreenView`:
   верх `QuestLogView` + `TorchView` + `TurnOrderView`; центр — поле с карточками юнитов
   (+`EventsLayerView` раунд, Status-оверлей, лог справа-снизу); низ — ряд скиллов +
   `RaidHudView`. Карточки получают `Interaction.Triggers` (MouseEnter/Leave/RightButtonDown).
2. [x] **S2** `DuelUnitViewModel`: `Hp`→`HpCurrent` (реальные HP из `CurrentHealth`/`MaxHealth`),
   добавить `IsSelected`. Обновить `HpText`, карточку и `DuelRenderTests`.
3. [x] **S3** `DuelBattleViewModel`: добавить `TooltipTarget`, `Hover/Unhover/OpenStats/CloseStats`
   команды, `StatsTarget` (HeroStatsViewModel), `Events` (раунд), `Quest` (Title/Goal+Retreat→Leave),
   `RaidHud` (актёр=`CurrentUnit`), `Torch`. Обновление в `Refresh()`.
4. [x] **S4** `HeroStatsViewModel.Apply(...)`: живые статы из атрибутов персонажа
   (Speed/Damage/ACC/Crit/Dodge/Prot).
5. [x] **S5** `HeroViewModel`: наблюдаемые `Name/ClassName` + `Apply(имя, класс, скиллы, hp, stress)`;
   `RaidHudViewModel`: метод передачи ходящего. `QuestLogViewModel`: наблюдаемые `Title/Goal`
   + опциональный `onRetreat`-экшен.
6. [x] **S6** `TurnOrderView.xaml`: шаблон слота → `DuelTurnEntryViewModel` (имя, текущий, враг).
   Инлайн-полоса очереди из текущего `DuelBattleView` удаляется.
7. [x] **S7** Тесты: `DuelRenderTests` под `HpCurrent`; новые — тултип при наведении, статы
   правым кликом, раунд, актёр в `RaidHud`. `dotnet build` WPF (0 ошибок) + тесты
   WPF / Core.Combat / Core.Content зелёные.
8. [x] **S8** Доки: `TESTING.md` (ручные чеки HUD), `CHANGELOG.md` (полный HUD дуэли).
9. [ ] **S9** Коммит и пуш в `origin/wpf`.

## Затронутые файлы

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml` (S1, S6)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\TurnOrderView.xaml` (S6)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\DuelBattleViewModel.cs` (S3)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\DuelUnitViewModel.cs` (S2)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\HeroStatsViewModel.cs` (S4)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\HeroViewModel.cs` (S5)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\RaidHudViewModel.cs` (S5)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\QuestLogViewModel.cs` (S5)
- `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\DuelRenderTests.cs` (S7)
- `docs\TESTING.md`, `docs\CHANGELOG.md`, `PLAN.md` (S8)

Ядро (`src\Core\...\Core.Combat`) и `src\External\` не изменяются. Мокап `BattleScreenView`
остаётся как референс.

## Приёмка

- [ ] Три панели как в мокапе: квест+Retreat / факел / очередь сверху; поле+раунд в центре;
      скиллы, инфо ходящего, тултип, инвентарь/карта снизу.
- [ ] Наведение на юнита — тултип (HP/стресс), правый клик — лист статов.
- [ ] Очередь, раунд, актёр, скиллы, HP/стресс — живые из `DuelController`.
- [ ] `dotnet build` 0 ошибок; тесты WPF/Combat/Content зелёные.