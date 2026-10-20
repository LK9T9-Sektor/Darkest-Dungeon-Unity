# presentation_wpf.md — Presentation (WPF): экраны, входы, связи с ядром

> Домен: `presentation` (клиент `src\Wpf`). Статус: **реализовано**. Детальный UI — в
> `FEATURE_DESKTOP_CLIENT.md`; здесь — поток и связи, кратко.

## 1. Назначение и когда работает

WPF-клиент — тонкий потребитель ядра: экраны (меню → лобби → бой), вводы, снапшоты. Вся доменная
логика — в `Core.*`; ViewModel-слой транслирует вводы в `DuelController` и отображает состояние.
Отдельный демо-экран `SpineDemoView` («SPINE TEST» в меню) играет скелетную анимацию героя
(Spine 2.1) — прототип Spine-рендера для карточек дуэли: стрелками листаются все 15 героев,
наборы анимаций героя (`Skeleton\`, Idle первым) и скорость воспроизведения 100…1000%.
Экран `SpineBattleView` («АРЕНА 1×1» в меню) — чисто визуальная арена: слева Crusader, справа
Highwayman, кнопка «АТАКА» проигрывает атаку как в Unity — слоями: base-слот принимает ударную
позу `AttackSword` (заглушка с нулевой длительностью), поверх временно играет оверлей-скилл
`Smite` (1.867 с, след меча); по завершению оверлея цель показывает красный флеш и попап урона,
оба слота возвращаются в Idle. Ядро боя не участвует.
Экран `CaptureReplayView` («ВОСПРОИЗВЕСТИ ЗАХВАТ» в арене) — первый потребитель контракта
игровых событий: читает JSONL-захват Unity (`manifest.json` + `*_session.jsonl`, схемы
`1.0.0`/`1.1.0`) и проигрывает его визуально. Состояние не симулируется — берётся ближайший
снапшот и к нему последовательно применяются события; атаки идут по одной и ждут окончания
клипа атакующего. В 1.1 добавлены формация `Rank`/`Size`, стресс, статусы/баффы, переходы
`SkillResolved`/`UnitMoved`, границы хода/раунда, победитель боя и журнал `AudioPlayed`.

## 2. Модель данных

- Экраны: `MainMenuView`, `DuelLobbyView`, `SinglePlayerLobbyView`, `PartySelectionView`,
  `DuelBattleView`, `RaidHudView`, `ScreenHeaderView`, `SpineDemoView`, `SpineBattleView`,
  `CaptureReplayView`, `EstateReplayView`.
- ViewModels: `MainMenuViewModel`, `SinglePlayerLobbyViewModel`, `DuelBattleViewModel`,
  `SpineDemoViewModel`, `SpineBattleViewModel`, `CaptureReplayViewModel`, `EstateReplayViewModel`
  (юниты `CaptureUnitViewModel`, estate-карточки `Estate*ViewModel`, таймлайн
  `CaptureTimelineEntryViewModel`, темп `IReplayPacer` / `DispatcherReplayPacer`), ...
- Estate-экран (`EstateReplayView`): шапка с заметкой захвата, полоса недели/ресурсов/town event,
  слева и в центре — сетка карточек зданий (`WrapPanel`, прогресс апгрейдов), справа — скролл
  карточек героев (класс, уровень, HP-бар, стресс, квирки/болезни/статус), внизу — таймлайн
  видимых шагов и аудио-журнал. Квесты на экране не показываются — они открываются отдельно.
- Захват: `Clients.EventLog.GameEventLogReader` + `GameEventCapture` (allowlist-биндер, схемы
  `1.0.0`/`1.1.0`, `Sequence` 1..N, `Subsystem` = имени файла); `CaptureAnimationCatalog`
  (`ICaptureAnimationCatalog`) мэппит `skeleton/animation` в `Content\Spine\<Skeleton>\<Animation>`;
  WPF ищет свежий `<repo>/captures/run_*`, иначе играет эталон из `Content\Captures\Reference`.
  `CaptureUnitViewModel` хранит `Rank`/`Size`/
  `Stress`/`MaxStress`/`AtDeathsDoor`/`IsAfflicted`/`IsVirtued` и производные подписи
  (`R1`/`R2 · x2`, `25/100`, `Bleeding 2`, `damage +0.15`); `AudioLog` — отдельная коллекция
  `CaptureTimelineEntryViewModel`, заполняемая `AudioPlayed` (без реального FMOD-воспроизведения).
- Spine-демо: рендер-контрол `SpineHeroControl` (`SKElement`) поверх библиотек
  `Sektor.Spine` (рантайм 2.1, netstandard2.0) и `Sektor.Spine.Skia`
  (загрузчик, плеер, Skia-рендер).
- Обёртки ИИ/сети: `AiRivalLink`, `NetworkRivalLink`, `IDuelRivalLink`.
- `Data/DuelContent` — реализация `IDuelContent` поверх каталогов.
- `Data/SpineDemoCatalog` (реализация `ISpineDemoCatalog`) — герои/анимации/пути контента
  демо-экрана; `SpineDemoViewModel` получает каталог через конструктор.

## 3. Порядок срабатывания (трассировка)

1. Запуск → `MainWindow` (адаптивная раскладка, `*`-колонки) → меню.
2. Лобби: выбор классов/квирков/скиллов (`DuelHeroPick`), «Start Battle».
3. Бой: `DuelController.StartDuel(hostPicks, clientPicks, seed, isHost)` + `StartBattle`.
4. Ход: `DuelBattleViewModel` читает `IsLocalTurn`, применяет вводы (`ExecuteLocalSkill`), ИИ —
   `AiRivalLink` → `DuelAi.ChooseAction` → `ApplyRemoteSkill`; мультиплеер — `NetworkRivalLink`.
5. Отображение: снапшоты состояния (HP/стресс/ранги), лог (`DuelBattleEvents.Log`), попапы.
6. Spine-демо: меню → «SPINE TEST» → `SpineDemoView`; `SpineHeroControl` на `Loaded`/
   смене `ContentDirectory` находит в `Content\Spine\<Hero>\<Anim>` первый `*.skel*` и `*.atlas*`,
   грузит `SpineContent` и играет первую анимацию сета. Селекторы (кнопки ◀/▶ с
   `AutomationProperties.Name`) листают героя/анимацию/скорость: VM меняет `ContentDirectory`
   (перезагрузка сета) и `SpeedMultiplier` → `SpinePlayer.TimeScale` (`AnimationState.TimeScale`,
   float). Кадры считаются в `CompositionTarget.Rendering`. Кнопка «SCREENSHOT» (`EventTrigger` →
   `SaveScreenshotAction`, Microsoft.Xaml.Behaviors) снимает текущее окно в `screenshots/` рядом
    с exe; имя файла показывает VM (`ScreenshotStatus`).
7. Арена: меню → «АРЕНА 1×1» → `SpineBattleViewModel` (логгер пишет `arena: opened/attack started/
   attack completed` в `Logs\duel.log`). «АТАКА» (`AttackCommand`) меняет два directory атакующего
   в одном потоке: base-слот → `Crusader\AttackSword` (поза) и оверлей-слот → `Crusader\Smite`
   (one-shot). Оверлей-контрол имеет `Loop="False"` и единственный несёт
   `AnimationCompletedCommand`; base-контрол команду завершения не получает (его поза —
   duration=0, событие от него никем не потребляется). По концу `Smite` состояние шлёт `Complete`
   → `SpinePlayer` поднимает `AnimationCompleted` (отложенный до конца `Update`) → команда оверлея
   → VM: оверлей → `""` (слот выключается), base → `Idle`, цель получает
   `TargetCardFlash="Damage"` (вход DataTrigger перезапускает флеш) и `TargetPopupVisible=true`
   с текстом `-6`; `IsAttacking=false` разблокирует кнопку (`NotifyCanExecuteChangedFor`).
8. Захват: арена → «ВОСПРОИЗВЕСТИ ЗАХВАТ» → `SpineBattleViewModel.CaptureReplayCommand` →
   `MainWindow` через `GameEventCaptureLocator` выбирает самый свежий `<repo>/captures/run_*`
   (если записей нет — эталон `Content\Captures\Reference`) и создаёт `CaptureReplayViewModel`
   с `GameEventLogReader`, этим путём и `DispatcherReplayPacer`. Конструктор читает захват
   (`Result`: ошибка → `ErrorText`/«Захват недоступен», без исключения), строит таймлайн из
   подписей и сразу запускает `StartReplay`.
   `Advance` применяет **одно** неатакующее событие и планирует следующее через `EventPacing`
   (100 мс), поэтому переходы/факел/HP видны пошагово; текущая строка таймлайна подсвечивается и
   скроллится в видимую область.
   - `SceneSnapshot`/`StateSnapshot`: `SetCombatants` создаёт `CaptureUnitViewModel` с
     `fallbackRank = index + 1`; `SortFormation` сортирует `Party`/`Enemies` по `Rank`, а
     `PartyFormation` — reverse `Party`, чтобы rank 1 был у центра. `StateSnapshot.Battle`
     обновляет `Round`/`Turn` (в старых записях `Battle == null`).
   - `SkillResolved` обновляет `Hp`/`Stress` каждой цели и `Rank` по `RankAfter`; `UnitMoved`
     обновляет ранг и перестраивает строй; `HeroStatChanged` (`hp`/`stress`) обновляет
     соответствующее значение.
   - `TurnStarted`/`RoundStarted` обновляют `Round`/`Turn`; `TurnEnded`/`RoundEnded` только
     подписываются в таймлайне; `BattleEnded` выставляет `BattleWinner` (`ПОБЕДА ГЕРОЕВ`/
     `ПОРАЖЕНИЕ`/`ОТСТУПЛЕНИЕ`/`ИТОГ НЕИЗВЕСТЕН`).
   - `AudioPlayed` добавляет строку в `AudioLog` (`event:/… · kind · action`), не запуская звук;
     `AttackExecuted` по-прежнему управляет анимацией/подсветкой (пауза `IsAwaitingAnimation`).
   На `attackExecuted` атакующий получает клип из `CaptureAnimationCatalog`, цель — подсветку; по
   `AnimationCompletedCommand` поза держится до `MinimumAttackDuration` (1.2 с), затем подсветки
   снимаются и `Advance` продолжает. Без Spine-контента атака завершается по
   `AttackFallbackDelay` (900 мс). После последнего события — `IsFinished`, «Повторить» сбрасывает
   проекцию и снова стартует.
9. Estate-захват: меню → «ИМЕНИЕ (ЗАХВАТ)» → `EstateReplayViewModel` (DataTemplate
   зарегистрирован в `MainWindow.xaml`; без него `ContentControl` рисует пустоту — чёрный экран).
   `MainWindow` выбирает самый свежий `run_*` с `Kind = estate` (иначе `ErrorText`/«Захват имения
   недоступен»); VM читает его, строит таймлайн и на `estateSnapshot` перестраивает ресурсы, здания,
   ростер и квесты. `AudioPlayed` не попадают в основной таймлайн: они копятся в `AudioLog` и
   применяются без паузы, поэтому видимые шаги идут с темпом 100 мс, а не тонут в музыке/UI-звуках.
   Action-события только подписываются в таймлайне; «ПОВТОРИТЬ»/«НАЗАД» — как у боевого replay.
   Арт/интерактивные окна в v1 не используются.

## 4. Очередь и обновления

- UI-поток (Dispatcher); `StateChanged` события боя → перерисовка.
- Spine-демо: собственный цикл `CompositionTarget.Rendering` (подписка на `Loaded`, отписка на
  `Unloaded`), дельта кадра клампится 0.1 c (`MaxFrameSeconds`); пауза — `IsPaused` (без отписки).
- ИИ-ход с таймером (100мс, `AiRivalLink`: один тик, `lastActedCombatId`-гвардия — действие
  соперника шлётся один раз за ход, дальше паком занимается `DuelBattleViewModel`).
- **Пак превью** (`DuelBattleViewModel.PaceState` + 50мс `DispatcherTimer`): при смене исполнителя
  хода — бит 0.5с + попап «ТВОЙ ХОД»; для действия соперника (ИИ и сеть одинаково) — превью 1с,
  затем `ApplyRivalActionPayload`. Паузы презентационные, lockstep не нарушают.
- Захват: UI-темп через `IReplayPacer` — 100 мс на неатакующее событие; атака ждёт
  `SpineHeroControl.AnimationCompletedCommand` (нецикличный клип) и держит позу минимум 1.2 с.
  Если у атакующего нет Spine-контента (монстр), `CaptureReplayViewModel` планирует завершение
  через 900 мс. Итоговый прогон эталона `1.0.0` — ~10.8 с вместо прежних ~1.3 с. Каждый шаг и
  атака пишутся в `Logs\duel.log` с метками времени (`capture replay: event/attack/hold/finished`).
  `AudioLog` только журналирует `AudioPlayed`; реальный FMOD playback и выбор банков — отдельный
  срез.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Ввод только на своём ходу | `IsLocalTurn` | иначе игнор |
| Адаптивная раскладка | `MainWindow` | ресайз, без Viewbox |
| Скорость Spine-демо | `SpineDemoViewModel` | 1.0…10.0 (шаг 0.5, wrap 100↔1000%); нижний кламп контрола 0.01…100 |
| Выбор захвата | `GameEventCaptureLocator` | новейший `<repo>/captures/run_*` с `manifest.json`; иначе эталон `Content\Captures\Reference` |
| Чтение захвата | `GameEventLogReader` | `SchemaVersion` = `1.0.0`/`1.1.0`; `Sequence` ровно 1..N; `Subsystem` = имени файла; allowlist-биндер |
| Fallback формации | `CaptureUnitViewModel` | `Rank <= 0` → `index + 1`; `Size <= 0` → 1 |
| Захват без Spine-клипа | `CaptureReplayViewModel` | idle-сет, иначе карточка; атака завершается по фолбэку 900 мс |
| Темп захвата | `CaptureReplayViewModel` | 100 мс на событие; атака — минимум 1.2 с (клип + удержание) |
| Аудио в захвате | `CaptureReplayViewModel`, `EstateReplayViewModel` | `AudioLog` пополняется только `AudioPlayed`; звук не проигрывается; в estate-таймлайн аудио не входит |
| Шаги estate | `EstateReplayViewModel` | `TotalSteps` = только видимые (не audio) события; `TotalEvents` = все события run |

## 6. Нюансы и подводные камни

- **Не дублировать доменную логику во ViewModel** — вводы через `DuelController`, состояние из ядра.
- `DuelContent` (реализация `IDuelContent`) — единственное место загрузки контента в WPF.
- **Валидация целей**: `DuelBattleViewModel.SelectTarget` исполняет скилл только по подсвеченной
  цели (`unit.IsTarget`) / смежному рангу (move); само ядро дополнительно отклоняет невалидную цель
  в `ExecuteLocalSkill` (`DuelController.cs:311`).
- **Стрелка цели** — `DuelBattleView.xaml.cs` + `Ui/TargetArrowMath.cs`: при выбранной способности
  над карточкой действующего юнита висит бейдж (имя скилла); на ховере валидной цели из центра
  бейджа в центр карточки-цели строится прямая `Line` + треугольник-стрелка (`TargetArrowMath.ArrowHead`,
  чистая математика; позиции через `TransformToVisual(TargetLayer)` в координаты Viewbox-сцены).
  Бейдж позиционируется на `LayoutUpdated`, пока скилл выбран; move-режим — линия без бейджа.
- **Превью хода соперника** (`DuelBattleViewModel.StartRivalReveal`): по wire-строке соперника
  (ИИ/сеть) показывается, что тот собирается сделать, и `1с` держится перед применением:
  скилл → `AiSkillPreview` (бейдж) + стрелка к цели; `move|rank` → `IsMovePreview=true` +
  ⇄-линия `DrawMoveArrow` к карточке нового ранга. Рисуется только когда `!IsLocalTurn`
  (`RedrawAiArrow`, реагирует на `IsMovePreview` в `DuelBattleView.cs`).
- **Поле привязано к половинам** — `DuelBattleView.xaml`: две `MinWidth=820`-колонки (4 карты × 201px
  + маржи) с фиксированным зазором 120 между ними; герои `HorizontalAlignment="Right"`, монстры
  `Left` — фронт (ранг 1) всегда у центра. Смерть не схлопывает `Auto`-колонку и не перецентрирует
  сетку: выжившие на своей половине рефлоу к центру (ядро `RemoveUnit` пересчитывает ранги),
  команды не дрейфуют. `TargetLayer` Canvas перекрывает все 3 колонки (`ColumnSpan=3`), стрелки
  считаются в координатах Viewbox от реальных позиций карточек.
- **Size юнитов (1–4)** — ранг назначается кумулятивно по `size` (ядро, `16_formation_size.md`),
  карточка `185 × size` (`DuelUnitViewModel.CardWidth`). Монстры без `Stress`/некоторых резистов:
  `PveBattleViewModel` защищён null-проверками (`?? 0`).
- **PvE-режим** — `PveBattleViewModel` (по образцу Unity `CoreBattleDriver`) + `PveLobbyViewModel`:
  `StartFight` инвертирует local/remote (герои — «remote», ввод через `ApplyRemoteSkill`; монстры —
  «local», AI через `ExecuteLocalSkill`/`UseMonsterBrain`). `DuelBattleView` переиспользуется через
  интерфейс `IDuelBattleViewData` (без правки `DuelBattleViewModel`).
- **Попап «ТВОЙ ХОД»** (`DuelUnitViewModel.TriggerTurnPopup`, `DetectTurnTransition`): при передаче
  хода новому исполнителю `TurnPopupVisible` на 1.2с + золотая вспышка `"Turn"` в
  `DuelUnitCardView.xaml`.
- **Баффы/дебаффы на карточках** — `DuelUnitViewModel.StatusEffects` заполняется из
  `Character.BuffInfos` (`DuelBattleViewModel.BuildStatusEffects`): id + остаток раундов.
- **Death's door мигает красным** — `DuelUnitViewModel.IsOnDeathsDoor` (из `character.AtDeathsDoor`,
  `DuelBattleViewModel.ToUnit`); карточка (`DuelUnitCardView.xaml`) при `True` проигрывает
  `ColorAnimation`-пульс (#33E83333 ↔ прозрачный, 0.75s AutoReverse) — паритет Unity
  `FormationUnit.Update` (R=1, G/B 1↔0.4, цикл ~1.5s). При хeлье `AtDeathsDoor → false` пульс гаснет.
- **Spine-рантайм — вынесенные исходники, доставка в Unity DLL** — `src\Spine\Sektor.Spine`
  (netstandard2.0, `namespace Spine`) собирается из своих сорсов `src\Spine\Sektor.Spine\Source\**\*.cs`
  (переехали из `unity\Assets\Spine\SpineSharp\Source`); post-build копирует `Sektor.Spine.dll`/`.pdb`
  в `Assets\Plugins\Internal` обоих Unity-деревьев (`CoreUnityPluginFolders`), где их подхватывает
  `SpineUnity`. `InternalsVisibleTo` (Assembly-CSharp, Assembly-CSharp-Editor) — доступ к internal
  членам из Unity-кода по этой DLL. WPF/Skia зависимостей нет.
- **Setup-поза требует world-трансформа** — конструктор `Skeleton(data)` не считает world-позиции
  костей, поэтому `SpinePlayer` в конструкторе делает `SetToSetupPose()` + `UpdateWorldTransform()`;
  без этого `Measure`/`Render` на «свежем» скелете дают нули до первого `Play/Update` (аттачи
  default skin в DD-скелете есть уже в setup-позе).
- **Fit — по анимации, а не по setup-позе** — `SpineFrameRenderer.MeasureAnimation` (12 равномерных
  сэмплов анимации на throwaway-скелете) даёт union bounds; в контроле
  scale = `min(w/W, h/H) × 0.92`, origin — центр bounds.
- **Тинт — только через ColorFilter, не через vertex colors** — `DrawVertices` с непустым массивом
  vertex colors в SkiaSharp 2.88.8 превращает прозрачные пиксели атласа в непрозрачный белый
  (белые прямоугольники вокруг частей персонажа). Полный разбор с минимальным репро —
  `docs\mechanics\presentation\spine_transparency_vertex_colors.md`. Рендер передаёт `colors: null`, а цвет накладывает
  `SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate)` (кэш paint'ов по текстуре/бленду/цвету);
  тинт = `skeleton × slot × attachment` (формула Unity `SkeletonRenderer`).
  Шейдер атласа — `SKShader.CreateBitmap(..., Clamp)`; UV — в пикселях текстуры; мапа блендов DD:
  `normal→SrcOver`, `additive→Plus`, `multiply→Modulate`, `screen→Screen`.
- **Клики — через Microsoft.Xaml.Behaviors, не code-behind** — SCREENSHOT: `EventTrigger EventName="Click"`
  → `Behaviors\SaveScreenshotAction` (DP `SavedCommand` получает путь PNG или null, статус — VM);
  info-кнопка карточки (`DuelUnitCardView.xaml`): `MarkEventHandledAction` гасит `Click`, чтобы он
  не дошёл до кнопки-слота (эквивалент прежнего `e.Handled = true`). Команды без capture-логики —
  `InvokeCommandAction` (прецедент — right-click на слоте в `DuelBattleView.xaml`). Mouse-события
  (стрелки/бейдж) и `Loaded`/`LayoutUpdated` остаются в code-behind: это view-геометрия, а не команды.
- **Скриншот окна** — `WindowScreenshot.Capture` (`RenderTargetBitmap` + `PngBitmapEncoder`, учёт DPI)
  пишет PNG (без системного заголовка) в `screenshots/` внутри каталога сборки
  (`AppContext.BaseDirectory`), имя `screenshot-<yyyy-MM-dd_HH-mm-ss-fff>.png`. Префикс имени — отдельная
  константа: в строке формата `DateTime.ToString` буквы `s`/`h`/`t` из слова «screenshot» были бы
  съедены как токены времени. Ошибки записи показываются в строке статуса, приложение не падает.
- **Выбор анимации = смена сета** — каждый `.skel` героя содержит ровно одну анимацию, поэтому
  «ANIMATION ◀/▶» переключает папку скелета (`Skeleton\<Anim>`), а не трек внутри одного скелета.
  При смене героя выбранная анимация сохраняется, если такой сет у нового героя есть, иначе — Idle
  (`SpineDemoViewModel.OnCurrentCharacterChanged`). `ContentDirectory` — композиция
  каталога (`Data\SpineDemoCatalog`).
- **Контент-линки Spine** — раскладка `Content\Spine\<Hero>\<Anim>\`: `.skel.bytes` линкуется как
  `*.skel`, `.atlas.txt` — как `.atlas` (загрузчик принимает и `.skel`, и `.skel.bytes`; атлас —
  только `.atlas`); имя PNG обязано совпадать с именем страницы атласа (`<hero>.sprite.<anim>.png`),
  иначе текстура не найдётся. Записи генерирует `tools\spine-demo-content.ps1` в
  `SpineDemoContent.props` (1005 записей / 335 сетов: `<Hero>\Skeleton\<Set>` + `<Hero>\Effects\<Skill>` —
  руками csproj не правится; Effects-сеты содержат страницу-текстуру рядом со скелетом, базовые — в `Outfits\A`).
- **Неймспейс `Sektor.Spine.Skia`** — квалифицированные ссылки `Spine.X` запрещены
  (shadowing сегментом `.Spine`): только `using Spine;` и простые имена.
- **One-shot анимация (арена)** — `SpinePlayer.Play(name, loop: false)` ведёт флаг
  `completionArmed` (его снимает только `AnimationState.Complete` по треку 0; лупинг-Complete
  его игнорирует) и поднимает `AnimationCompleted` **после** `state.Update/Apply` внутри
  `Update` — из обработчика можно синхронно перезагрузить/поменять контент (контроль это и
  делает: смена `ContentDirectory` в обработчике). `SpineHeroControl`: DP `Loop` (default true)
  + DP `AnimationCompletedCommand` (команда получает имя анимации). Готча: смена `Loop` или
  `AnimationName` **перезапускает** текущую анимацию (`PlayAnimation`), поэтому VM меняет
  «вернулся в idle»   атрибутами в одном потоке выполнения команды — между изменениями нет кадра
  `CompositionTarget.Rendering`, промежуточный рестарт невидим и безопасен.
- **Attack\*-сеты героев — заглушки с duration=0** (измерено по всем15 героям: `Skeleton\Attack*`
  ≈360 байт,4 кости, `Animation.Duration = 0`) — это статичные ударные позы, а не анимации;
  **настоящие таймовые скилл-анимации лежат в `<Hero>\Effects\<Skill>`** (например
  `Effects\Smite`, `smite = 1.867 с`, регион `sword_trail`) — так же слоями их играет Unity
  (base-поза + лист `Abilities` в префабе). Нулевая длительность ломает завершение one-shot:
  условие рантайма `lastTime < endTime` ложно с самого начала и `Complete` не приходит никогда,
  а трек чистится по первому тику — поэтому в `SpinePlayer.Update` есть guard: armed-one-shot с
  `EndTime <= 0` завершается на первом тике (иначе кнопка арены вечно заблокирована;
  тест `AttackSwordSet_ZeroDurationPose_StillCompletesOneShot`).
- **Пустой `ContentDirectory` = слот выключен** — `SpineHeroControl` в этом случае молча
  выгружает контент (без `FileNotFoundException` и оранжевой надписи): так оверлей арены
  прячется между ударами (`AttackerOverlayContentDirectory = ""`).
- **Повторный флеш урона** — DataTrigger `TargetCardFlash="Damage"` срабатывает только на
  входе в значение, поэтому VM перед каждым ударом сбрасывает `TargetCardFlash` в `""` (и
  `TargetPopupVisible` в false), иначе второй удар флеш не покажет.
- **Финальный снапшот эталона `1.0.0` — новый бой** — эталон заканчивается `stateSnapshot`
  (seq 38) с факелом 7 и врагами `brigand_blood`/`brigand_fusilier`; он применяется последним и
  замещает `monster_3`, которого довели до 0 HP на seq 28. Поэтому «финал с монстром 0 HP»
  неверен: проверять нужно последний снапшот (35/35 и 12/12), а смерть старого монстра видна в
  моменте. Synthetic `1.1.0` вместо этого заканчивается `BattleEnded` (10 событий).
- **Захваты — общая папка репозитория** — Unity-редактор пишет в `<repo>/captures/run_*`
  (собранная игра без репозитория — fallback `persistentDataPath/Replays`); WPF берёт самый
  свежий `run_*` при открытии экрана; battle replay берёт только `Kind = raid`, estate replay —
  только `Kind = estate`. Папка gitignored; копировать файлы вручную не нужно. Пустые слоты
  формации (без `Character`) в снапшот не попадают. Захват, который ещё пишется,
  лучше не открывать: reader может увидеть неполный хвост.
- **Таймлайн отмечает текущий момент** — `CurrentEntry` — последнее применённое событие; при
  пошаговом темпе каждая строка остаётся текущей ~100 мс, атака — ~1.2 с. `ListBox`
  `SelectedItem` привязан one-way, code-behind скроллит выбранную строку в видимую область.
- **Расстановка: `Rank` есть в 1.1, координат нет** — `CombatantState.Rank`/`Size` пишутся
  рекордером; `SortFormation` сортирует обе стороны по `Rank`, `PartyFormation` реверсит героев,
  чтобы rank 1 был у центра. Точные x/y и позиции вне рангов не пишутся; для записи `1.0.0`
  используется `fallbackRank = index + 1`, поэтому строй остаётся прежним.
- **`SkillResolved` — истина, `AttackExecuted` — визуал** — состояние HP/stress/рангов меняет
  `SkillResolved` (и `UnitMoved`), а `AttackExecuted` только запускает клип/подсветку. Если
  визуального события нет, цифры всё равно обновятся; если нет `SkillResolved` (старая запись),
  HP меняется через `HeroStatChanged`.
- **Статусы обновляются снапшотом, а не каждым событием** — `StatusText` берётся из
  `CombatantState.Statuses/Buffs` на `battleStart`/`turnEnd`/`battleEnd`; внутри применения
  скилла HP/stress меняются сразу, а список статусов — только следующим снапшотом.
- **`BattleWinner` — только после `BattleEnded`** — до него `"—"`; retreat (`Retreat=true`) даёт
  `ОТСТУПЛЕНИЕ`, иначе winner по `BattleWinners`; `unknown` → `ИТОГ НЕИЗВЕСТЕН`.
- **Аудио-журнал ≠ воспроизведение** — `AudioLog` показывает фактические `AudioPlayed`
  (`event:/… · kind · action`), но FMOD не запускает; рейд и estate пишут аудио, но
  title/town-музыка в старых `1.0.0`/`1.1.0` отсутствует. Реальный playback — отдельный срез.
- **Estate-таймлайн — без аудио** — `EstateReplayViewModel` не добавляет `AudioPlayed` в
  `Timeline` (они только в `AudioLog`) и не тратит на них темп: иначе 150+ звуков города
  растягивали бы 17 видимых шагов на десятки секунд. `TotalSteps` = число видимых шагов,
  `TotalEvents` = все события захвата (для подписи).
- **Забытый DataTemplate = чёрный экран** — новый экран (`EstateReplayViewModel`) обязан иметь
  `<DataTemplate>` в `MainWindow.xaml`; без него `ContentControl` не рисует ничего и виден только
  чёрный фон окна, даже если VM загрузилась без ошибок.
- **Монстр — карточка** — `CaptureAnimationCatalog` возвращает `null` для скелета без линкованного
  контента (`brigand_cutthroat`); юнит не падает, показывает имя/HP и подпись «карточка».
- **Ошибка чтения не роняет экран** — reader возвращает `Result`, VM пишет `ErrorText` и
  блокирует «Повторить»; оверлей ошибки перекрывает тело экрана.

## 7. Взаимодействия

- `duel/*` (DuelController, DuelAi), `common_random.md` (детерминизм), `clients_reader.md`
  (контент).
- `docs/SPEC_GAME_EVENTS_SCHEMA.md` — контракт событий `1.0.0`/`1.1.0` и правила чтения; эталон
  `docs/spec_game_events/examples/`; `docs/audio/00_index.md` — каталог FMOD и запись `AudioPlayed`.
- `FEATURE_DESKTOP_CLIENT.md` — детали экранов/механик.
- Unity-оверлеи Тест-боя — `presentation_unity_fight.md`.

## 8. Файлы-источники

- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/*`, `ViewModels/*`, `Ui/TargetArrowMath.cs`,
  `Data/DuelContent.cs`, `Combat/AiRivalLink.cs`, `Networking/NetworkRivalLink.cs`
- `src/Spine/Sektor.Spine/` (рантайм spine-csharp 2.1), `src/Spine/Sektor.Spine.Skia/` (loader/player/Skia-рендер)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/SpineHeroControl.cs`, `Views/SpineDemoView.xaml(.cs)`, `ViewModels/SpineDemoViewModel.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/SpineBattleView.xaml(.cs)`, `ViewModels/SpineBattleViewModel.cs` (арена 1×1)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/CaptureReplayView.xaml(.cs)`, `ViewModels/CaptureReplayViewModel.cs`,
  `ViewModels/CaptureUnitViewModel.cs`, `ViewModels/CaptureTimelineEntryViewModel.cs`,
  `ViewModels/IReplayPacer.cs`, `ViewModels/DispatcherReplayPacer.cs`,
  `Data/CaptureAnimationCatalog.cs` (плеер захвата)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/EstateReplayView.xaml(.cs)`,
  `ViewModels/EstateReplayViewModel.cs`, `ViewModels/Estate*ViewModel.cs` (estate-экран)
- `src/Clients/Sektor.DarkestDungeon.Clients.EventLog/GameEventLogReader.cs` (чтение/валидация JSONL),
  `GameEventTypeBinder.cs` (allowlist `List<T>`/`T[]`), `JsonEventLogger.cs` (`1.1.0`),
  `GameEventCaptureLocator.cs` (поиск `<repo>/captures` и свежего `run_*`)
- `src/Spine/Sektor.Spine.Skia/SpinePlayer.cs` (one-shot `Play` + событие `AnimationCompleted`)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Ui/WindowScreenshot.cs` (кнопка SCREENSHOT)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Behaviors/` (`SaveScreenshotAction`, `MarkEventHandledAction`)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Data/SpineDemoCatalog.cs`, `Data/ISpineDemoCatalog.cs` (герои/анимации демо)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/SpineDemoContent.props` + `tools/spine-demo-content.ps1` (генератор контент-линков)
- `docs/mechanics/presentation/spine_transparency_vertex_colors.md` (разбор дефекта vertex colors)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/DuelUnitCardView.xaml` (death's door pulse, «ТВОЙ ХОД»)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/DuelBattleView.cs` (превью), `DuelBattleViewModel.cs` (пак)
- `docs/FEATURE_DESKTOP_CLIENT.md`