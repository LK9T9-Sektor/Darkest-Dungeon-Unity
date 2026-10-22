# План: единый боевой UI и порядок отряда

## Статус

- Ветка: `core/spine-hero-selector`
- Текущий срез: **п.1 — порядок 4-3-2-1 в лобби и номера позиций**
- Старые `DuelBattleView`, `DuelUnitCardView`, `CaptureReplayView` и Unity-файлы не трогаем.

## Цель

Свести боевые экраны VS AI, PvE и Capture Replay к одному набору контролов:

- один `BattleView`;
- одна карточка `BattleUnitCardView` для героя и монстра;
- `BattleSceneBarView` из отдельных `InfoCellView`;
- `BattleTargetOverlay` с бейджем скилла, стрелками, move и AI preview;
- `BuffTableOverlayView` с портретом, позицией, именем и классом, без стрелок;
- `TurnOrderView` и `UnitInfoOverlayView` не меняются.

Карточка: позиция слева, `[i]` справа, имя/класс убраны, Spine по центру, HP красными
сегментами над стрессом, стресс сине-стальными сегментами, значения по центру полос.
`АТАКА` / `ЦЕЛЬ` — по центру сверху между позицией и `[i]`.

## Шаги

- [x] 1. Лобби: показывать карточки героев в порядке `4-3-2-1` с номером позиции.
  - `HeroSlotsPanel` сортирует отображение по `Position` по убыванию, не меняя порядок `Slots`.
  - `ToPicks` и ранги остаются `1..4`.
- [ ] 2. `BattleSceneBarView`: сцена/комната/раунд/ход/факел/золото из `InfoCellView`.
  - [x] Capture Replay: вынесен отдельный бар; `PanelCaption`/`PanelValue` перенесены в `Hud.xaml`.
  - [ ] Бой: подключить после появления `SceneName`/`RoomName`/`Round`/`Turn`/`Gold` в battle VM;
    верхний `TorchView` пока остаётся.
- [x] 3. `BuffTableOverlayView`: вынести inline-таблицу из `DuelBattleView`, добавить
  портрет, позицию, имя и класс через существующие контролы без стрелок.
- [x] 4. `BattleUnitCardView` (Size = 1): новая карточка, первым делом в Capture Replay.
  - `[i]` в replay пока неактивна: у `CaptureUnitViewModel` нет таблицы `BuffRowViewModel`;
    подключить при встраивании карточки в бой.
- [ ] 5. `BattleTargetOverlay`: вынести `TargetLayer` из `DuelBattleView` без изменения
  поведения; эталон — VS AI. Исправить иконку AI-превью у правой команды.
- [ ] 6. `BattleView`: новый экран для VS AI/PvE на новых контролах; затем Capture Replay.
- [ ] 7. Единая лобби-сцена: герои и монстры в одной таблице со скроллом и поиском,
  выбор позиции, двойной клик, ПКМ — информация. Отдельный крупный срез.

## Затрагиваемые файлы (п.1)

- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/HeroSlotsPanel.xaml`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/HeroSlotsPanel.cs`
- `docs/changelog/00_index.md`
- `docs/changelog/2026-10-22-lobby-formation-order.md`

## Приёмка п.1

- Во всех трёх лобби (VS AI, PvE, мультиплеер) карточки слева направо идут `4, 3, 2, 1`.
- На каждой карточке виден номер позиции.
- ПКМ по слоту по-прежнему открывает редактор героя.
- `ToPicks` и боевые ранги не изменились.
- Сборка WPF и существующие тесты проходят; визуальная проверка пользователем.

## Временные артефакты

- После завершения: `PLAN.md` переносится в `docs/plans/`, временных файлов нет.
