> Отложен: 2026-10-23 (ветка `core/spine-hero-selector`). Общее поле боя и стрелка атаки в replay
> сделаны и приняты (`4d3c70e`, коммит с `BattleFieldView`; стрелка — `BattleArrowView`).
> Осталась только проводка бара сцены в бой: в дуэли (арена) нет сцены, комнаты и золота, а раунд
> и ход уже видны в `TorchView` и `TurnOrderView`. Индекс — `docs\plans\00_index.md`.

# PLAN: бар сцены в бою (`BattleSceneBarView`)

## Состояние

- **Сделано и закоммичено:** общий `BattleFieldView` для боя и replay, контракт `IBattleFieldUnit`,
  стрелка атаки и id скилла в replay (`BattleArrowView`, `AttackStarted`/`AttackFinished`).
- **Не сделано:** `BattleSceneBarView` в `DuelBattleView` — ячейки сцены/комнаты/золота.
- **Почему отложено:** у VS AI/мультиплеер-дуэли нет источника `SceneName`/`RoomName`/`Gold`:
  бой идёт на арене без рейда, `BattleGround` даёт только `Round.RoundNumber`/`TurnType`.
  Подключать бар «наполовину» (только раунд/ход) смысла нет — эти данные уже показывает верхний
  `TorchView` (номер раунда) и `TurnOrderView`.
- **При возобновлении:** когда бой пойдёт из рейда (сцена/комната/золото в контексте боя), добавить
  эти поля в `DuelBattleViewModel` (или его рейд-аналог) и подключить бар как отдельную строку над
  верхней панелью. Скрытие пустых ячеек: `Scene`/`Room`/`RoomType`/`Turn` — пустая строка,
  `Round`/`Torch`/`Gold` — отсутствие значения (ячейка схлопывается, без флагов режима).

## Исходный план

- [ ] `BattleSceneBarView` в бою: сцена/комната/раунд/ход/факел/золото из `InfoCellView`, когда
  в `DuelBattleViewModel` появятся `SceneName`/`RoomName`/`Round`/`Turn`/`Gold`; верхний
  `TorchView` пока остаётся.
