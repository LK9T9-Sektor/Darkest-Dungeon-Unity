# presentation_wpf.md — Presentation (WPF): экраны, входы, связи с ядром

> Домен: `presentation` (клиент `src\Wpf`). Статус: **реализовано**. Детальный UI — в
> `FEATURE_DESKTOP_CLIENT.md`; здесь — поток и связи, кратко.

## 1. Назначение и когда работает

WPF-клиент — тонкий потребитель ядра: экраны (меню → лобби → бой), вводы, снапшоты. Вся доменная
логика — в `Core.*`; ViewModel-слой транслирует вводы в `DuelController` и отображает состояние.
Отдельный демо-экран `SpineDemoView` («SPINE TEST» в меню) играет скелетную анимацию героя
(Spine 2.1) — прототип Spine-рендера для карточек дуэли: стрелками листаются все 15 героев,
наборы анимаций героя (`Skeleton\`, Idle первым) и скорость воспроизведения 100…1000%.
Карточка выбранного героя — переиспользуемый `HeroPortraitView` (roster-портрет аутфита `A`,
читаемое имя и опциональные стрелки `◀`/`▶`); стрелки героя стоят вокруг портрета и меняют
класс вместе с карточкой. Справа от карточки — строка «SKILLS»: базовые боевые скиллы героя
(`SkillSquareView`, иконка 72×72, имя и структурированный тултип); клик проигрывает эффект скилла
один раз и возвращает ранее выбранный набор анимаций. Ниже — селекторы `ANIMATION`/`SPEED`, а
`Pause`/`Screenshot` со строкой статуса стоят справа в той же нижней строке.
Лобби VS AI / мультиплеера: отряд — четыре компактные карточки `HeroPortraitView`
(портрет 56, имя, стрелки ◀/▶ в мультиплеере; в VS AI стрелок нет, класс/монстр выбирается
таблицей); ПКМ по карточке открывает full-window `UnitInfoOverlayView`
(хост `LobbyHeroEditorHost`): гильд-баннер `header.png` фоном, шапка позиция/иконка/имя с reroll/
класс/X (красный), слева статы/оружие/тринкеты, справа `AbilityRowView`, квирки и резисты. Выбранные
скиллы сразу уходят в `DuelHeroPick`/`HeroFightUnitSpec`. В бою VS AI/мультиплеера тот же оверлей
открывается по ПКМ по юниту и показывает живые статы; это окно информации, а не редактор загрузки.
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

- Экраны: `MainMenuView`, `DuelLobbyView`, `MultiplayerLobbyView`, `PartySelectionView`,
  `DuelBattleView`, `RaidHudView`, `ScreenHeaderView`, `SpineDemoView`, `SpineBattleView`,
  `CaptureReplayView`, `EstateReplayView`.
- ViewModels: `MainMenuViewModel`, `DuelLobbyViewModel`, `MultiplayerLobbyViewModel`,
  `DuelBattleViewModel`, `SpineDemoViewModel`, `SpineBattleViewModel`, `CaptureReplayViewModel`,
  `EstateReplayViewModel`
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
- Обёртки ИИ/сети: `MonsterBrainRivalLink`, `NetworkRivalLink`, `IDuelRivalLink`.
- `Data/DuelContent` — реализация `IDuelContent` поверх каталогов.
- `Data/SpineDemoCatalog` (реализация `ISpineDemoCatalog`) — герои/анимации/пути контента
  демо-экрана; `SpineDemoViewModel` получает каталог через конструктор.
- Иконки героев: `Data/IHeroIconCatalog` / `Data/HeroIconCatalog` — `ImageSource? GetPortrait(
  classId, outfit)` по `Content\HeroIcons\<ClassId>\portrait.<Outfit>.png` (frozen `BitmapImage`,
  кэш по пути, отсутствие/битый файл → `null`); `GetHeader(classId)` читает `header.png`;
  `GetAbilityIcon(classId, iconId)` читает `ability.<id>.png` (72×72) для боевых скиллов;
  `Ui/HeroIds.ToClassId` переводит имя папки героя (`ManAtArms`) в class id (`man_at_arms`).
  Карточка — `Views/HeroPortraitView.xaml(.cs)`: DP `Portrait` (`ImageSource`), `Caption`
  (string), `PortraitSize` (double, 96), `ArrowSize` (double, 40), `ShowArrows` (bool, false),
  `PreviousCommand`/`NextCommand` (класс-циклинг), `SelectCommand` (клик по портрету; null —
  карточка read-only), плейсхолдер `?`; контрол не знает про Spine/демо-экран и переиспользуется
  любым экраном: SPINE TEST показывает стрелки, бой — нет.
- Имена героев: `Data/HeroNameCatalog` читает английский пул `Content\Names\Names.xml` (те же
  `hero_name_0..555`, что использует Unity `StageCoach`/`DarkestPhotonLauncher`); `HeroSlotViewModel`
  получает случайное `HeroName` и `RerollNameCommand`, `Stats.HeroName` синхронизирован.
- Боевые скиллы героя: `Data/IHeroSkillCatalog` / `Data/HeroSkillCatalog` (`HeroSkillInfo`) —
  соединяет распарсенные `HeroClass.CombatSkills` с `art:`-секцией (`SkillArtInfo`) и иконкой;
  `GetSkills(classId)` — список для демо, `GetSkillIcon(classId, skillId)` — точечный поиск для
  боевых экранов (дуэль/VS AI заполняют `DuelSkillViewModel.Icon`).
- `Views/SkillSquareView` — переиспользуемый квадрат скилла (иконка + имя + тултип); DP
  `IconSize` (double, 42) задаёт область иконки, кнопка = `IconSize + 8`, корень = `IconSize + 16`
  (72 → кнопка 80, корень 88). Без `SelectCommand` квадрат только для просмотра.
- `Views/AbilityRowView` — общий ряд квадратов скиллов (`ItemsSource`, `SelectCommand`, `IconSize`,
  `ShowNames`): на SPINE TEST показывает превью всех скиллов, в оверлее лобби — выбор активных.
- `Views/UnitSelectionTableView` — таблица выбора юнита для лобби: колонки `Slots`/`Icon`/`Class`,
  две вкладки задаёт экран. Строка на 1 слот кликабельна (двойной клик), строки на 2+ слота
  показываются серыми и не назначаются. `HeroSlotViewModel` хранит либо героя (`ClassId`), либо
  монстра (`MonsterId`), умеет `AssignClass`/`AssignMonster`, а клик по слоту выставляет
  `IsSelected`; `HeroSlotsPanel.ShowClassArrows` скрывает стрелки в VS AI-лобби.
- `Views/LobbyHeroEditorHost` — full-window host модального редактора героя; лежит в корневом
  `Grid` экрана лобби поверх всех рядов (в VS AI — один хост на две панели отряда). Открывается
  через `Open(slot)` из события `PartySelectionView.SlotEditRequested`.
- `Views/UnitInfoOverlayView` — общий лист информации о юните: в лобби редактор загрузки героя
  (ПКМ по слоту), в бою — окно по ПКМ. Фоновая гильд-картинка `Header` (top-left, не листается),
  шапка с `HeroIdentityView` и кнопкой закрытия внутри скруглённой панели; портрет/стрелки/позиция/скин
  вынесены вниз в `HeroCharacterControlsView`, Idle выше; тело из четырёх строк, нижний ряд
  `EQUIPMENT`/`TRINKETS`/`DISEASES` прижат к низу, высота панели по контенту. В бою доступны имя и скин
  героя, а скиллы/лагерные скиллы read-only; стрелки класса, позиция, портрет и reroll тринькетов скрыты.
  Слот лобби поднимает `HeroSlotEditRequestedEventArgs`. Правила лобби/боя и capability-таблица —
  в `unit_info_overlay.md`.
- `Styles/Hud.xaml` → `SectionPanel` — общая рамка секций; `CloseButton` — общий красный стиль X
  (размер/отступ задаёт экран); шапка оверлея имеет нижнюю линию 2 px, как боевой `UnitHeaderView`.
- `Views/EquipmentSlotView` — переиспользуемый слот экипировки 96×144: слева рамка 22×52 с глифом
  (мечи/щит) и уровнем без префикса `Lv.`, справа изображение 72×144 (размер из Unity
  `CharacterWindow`). `Views/HeroEquipmentView` — два таких слота (weapon/armor) рядом.
- `Views/TrinketSlotView` — переиспользуемый слот тринькета 96×144: изображение 72×144 с tooltip и
  отдельная reroll-кнопка справа сверху; `Views/HeroTrinketsEditorView` — два слота в ряд + заголовок
  `TRINKETS`. `Views/HeroTrinketsView` остаётся read-only вариантом для боевого стат-листа.
- `Views/HeroIdentityView` — две кнопки (карандаш/reroll), имя и класс; inline-редактирование имени
  (лимит 24, 24 точки-хинта не входят в значение, Enter/потеря фокуса — сохранить, Esc — отмена).
  `Views/HeroCharacterControlsView` — скин, портрет, стрелки класса и позиция внизу. `Views/HeroSkinView` —
  квадратная кнопка с глифом и `SkinId`; `Views/HeroLevelView` — медальон уровня.
- `Views/BoundedScrollView` — общий список с `DefaultRows`/`MaxRows`/`RowHeight`. Default резервирует
  пустые строки/высоту; используется для quirks (5 строк в каждой колонке), stats (4×2), resistances (4×2),
  diseases (3 строки, как Quirks/Base Stats — без заливки и рамок). При превышении — вертикальный скролл. `ViewModels/QuirkRowViewModel` + разметка
  квирков дают строку `[LockIcon][DisplayName]`; `SpineHeroControl` масштабирует Idle по высоте с
  `BottomInset`, ширина ограничивает только при нехватке места.
- `ViewModels/ISkillTileViewModel` — общий контракт `Icon`/`DisplayNameUpper`/`IsUsable`/
  `IsSelected`/`IsLoadout`/`IsMasked` для `DuelSkillViewModel` и `LobbySkillViewModel`;
  `SkillSquareView` по нему включает/выключает кликабельный квадрат, пересчитывает `IsEnabled`
  по `PropertyChanged.IsUsable` и рисует полупрозрачную маску по `IsMasked` (иконка не исчезает).
- `HeroSlotViewModel` получает `IHeroIconCatalog`/`IHeroSkillCatalog` через конструктор: заполняет
  `Portrait` (аутфит `A`), `Header`, `HeroName` и `LobbySkillViewModel` (иконка, тултип,
  `Level`/`BaseInfo`/`EffectRows`); `CanSelectSkills` берётся из `HeroClass.CanSelectCombatSkills`,
  `MaxActiveSkills` — из `NumberOfSelectedCombatSkills`. При лимите 4 невыбранные скиллы получают
  `IsUsable=false` (серые); у Abomination все 7 выбраны, выбор заблокирован (`SkillsLocked=true`),
  отдельная подпись при этом не показывается. `MonsterId`/`IsMonster`/`DisplayName`/`UnitClassName`/
  `UnitPortrait` позволяют тому же слоту показывать монстра; `SelectCommand` выделяет слот в лобби.

## 3. Порядок срабатывания (трассировка)

1. Запуск → `MainWindow` (адаптивная раскладка, `*`-колонки) → меню.
2. Лобби VS AI: клик по слоту выделяет его, двойной клик по строке таблицы (`Heroes`/`Monsters`)
   назначает героя или монстра; выборы/квирки/скиллы героя задаются в оверлее (ПКМ). «Start Battle»
   собирает `FightUnitSpec` по типу каждого слота, поэтому обе стороны могут быть смешанными.
   В оверлее скиллы — `AbilityRowView`; `LobbySkillViewModel.IsUsable` true у выбранных и у свободных
   слотов, false при лимите; Abomination (`CanSelectCombatSkills=false`) — все выбраны, клики
   игнорируются. ПКМ по компактной карточке поднимает full-window оверлей через `LobbyHeroEditorHost`;
   в нём меняются класс, имя (reroll), активные скиллы, квирки и тринкеты; каждый тринкет рероллится
   своей кнопкой.
3. Бой VS AI: `DuelController.StartFight(playerSide, aiSide, seed)` +
   `SetLocalTeam(Team.Heroes)` + `StartBattle`; мультиплеер — `StartDuel(hostPicks, clientPicks, seed,
   isHost)`.
4. Ход: `DuelBattleViewModel` читает `IsLocalTurn`, применяет вводы (`ExecuteLocalSkill`), правая
   сторона — `MonsterBrainRivalLink` (монстр → кампанийный брейн, герой → `FallbackDuelAi`) →
   `ApplyRemoteSkill`; мультиплеер — `NetworkRivalLink`.
5. Отображение: снапшоты состояния (HP/стресс/ранги), лог (`DuelBattleEvents.Log`), попапы.
6. Spine-демо: меню → «SPINE TEST» → `SpineDemoView`; `SpineHeroControl` на `Loaded`/
   смене `ContentDirectory` находит в `Content\Spine\<Hero>\<Anim>` первый `*.skel*` и `*.atlas*`,
   грузит `SpineContent` и играет первую анимацию сета. Кнопки ◀/▶ карточки героя и селекторы
   `ANIMATION`/`SPEED` (с `AutomationProperties.Name`) листают класс/анимацию/скорость: VM меняет
   `ContentDirectory` (перезагрузка сета) и `SpeedMultiplier` → `SpinePlayer.TimeScale`
   (`AnimationState.TimeScale`, float). Кадры считаются в `CompositionTarget.Rendering`. Смена
   героя также пересчитывает `CurrentHeroClassId` → `CurrentHeroName`/`CurrentHeroPortrait`
   (`NotifyPropertyChangedFor`), и стрелки карточки `HeroPortraitView` перерисовываются. Кнопка
   «SCREENSHOT» (`EventTrigger` → `SaveScreenshotAction`, Microsoft.Xaml.Behaviors) снимает
   текущее окно в `screenshots/` рядом с exe; имя файла показывает VM (`ScreenshotStatus`) в
   нижней строке. Справа от карточки — строка «SKILLS» (`ItemsControl`/`WrapPanel`): VM собирает
   `Skills` из `IHeroSkillCatalog.GetSkills(CurrentHeroClassId)` при смене героя. Клик
   (`SelectSkillCommand`) помечает квадрат и играет `Effects\<skill>` как one-shot (`Loop=false`);
   команда завершения возвращает ранее выбранный сет. Если у скилла нет `.fx`-сета, играет базовая
   поза (`anim`) в обычном цикле.
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
   (env `DARKEST_CAPTURE_RUN` переопределяет путь; если записей нет — эталон
   `Content\Captures\Reference`) и создаёт `CaptureReplayViewModel`
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
- ИИ-ход с таймером (100мс, `MonsterBrainRivalLink`: один тик, `lastActedCombatId`-гвардия — действие
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
| Портрет героя | `IHeroIconCatalog`, `HeroPortraitView` | аутфит `A`; опциональные стрелки `◀`/`▶` и клик по портрету через DP; отсутствующий/битый/неизвестный id → `null` → плейсхолдер `?`, исключения не летят |
| Имя героя | `HeroNameCatalog`, `HeroIdentityView`, `HeroSlotViewModel`, `BattleHeroInfoViewModel` | английский пул `Names.xml`; случайное при создании слота, reroll и inline-редактирование (лимит 24, 24 точки-хинта не входят в имя); в бою имя меняется только у героев и не влияет на статы; нет файла/пула → `Hero` |
| Экипировка | `EquipmentSlotView`, `HeroEquipmentView` | слот 96×144: слева рамка 22×52 с глифом меча/щита и уровнем, справа изображение 72×144; картинка или `?` |
| Тринкеты | `TrinketSlotView`, `HeroTrinketsEditorView` | слот 96×144: изображение 72×144 и кнопка reroll 20×20 справа сверху вне изображения (в бою скрыта); две колонки, tooltip; пул фильтруется по классу |
| Ряд скиллов в оверлее | `AbilityRowView`, `ISkillTileViewModel` | `IconSize=48`, имена скрыты; при `ShowName=false` корень плитки 56 px, поэтому все 7 скиллов (и 7 лагерных) помещаются в один ряд; выбранные с золотой рамкой, невыбранные серые + маска; в бою `IsReadOnly=true`, команды выбора не срабатывают; Abomination — все 7 выбраны, `SkillsLocked` без отдельной подписи |
| Квирки | `QuirkRowViewModel`, `QuirkIconCatalog` | две цветные колонки (positive/negative) без заголовков, 5 строк в каждой даже при нехватке данных; в строке `[замок 14×14][имя]`; иконка `Sprites/Shared/Quirk/lockquirk.png`; положительный квирк помечен locked в лобби-моке |
| Уровень героя | `HeroLevelView` | переиспользуемый медальон: чёрная закруглённая рамка, круг и число; DP `Level` |
| Оверлей юнита | `LobbyHeroEditorHost`, `UnitInfoOverlayView` | ПКМ по слоту в лобби или по юниту в бою; full-window: `HeroIdentityView` в шапке, медальон уровня/Idle слева, внизу `HeroCharacterControlsView` (скин/портрет/класс/позиция), `QUIRKS`+`BASE STATS` и `EQUIPMENT`/`TRINKETS` в середине, `SKILLS`/`CAMPING SKILLS`/`RESISTANCES`/`DISEASES` справа; нижний ряд прижат к низу; закрытие — X, Esc или клик по затемнению |
| Боевой лист | `BattleUnitInfoViewModel`, `BattleHeroInfoViewModel`, `BattleMonsterInfoViewModel` | адаптер живого `DuelUnitViewModel`: все статы/сопротивления/HP/стресс, текущие скиллы, лагерные скиллы класса, квирки/тринькеты при наличии, 3 пустых disease-слота; герой — имя/скин, монстр — read-only; при отсутствии Spine — portrait fallback |
| Списки с default | `BoundedScrollView` | 5 строк quirks в каждой колонке, 4×2 stats, 4×2 resistances, 3 строки diseases (как Quirks/Base Stats, без заливки и рамок); default резервирует пустые строки/высоту, затем вертикальный скролл; skills/camping — 1 ряд по 7 плиток |
| Иконка скилла | `HeroSkillCatalog.GetSkillIcon`, `HeroIconCatalog.GetAbilityIcon` | нет класса/скилла/файла → `null` → квадрат без иконки |
| Размер иконки скилла | `SkillSquareView.IconSize` | нижний кламп 16; кнопка = размер + 8, корень = размер + 16 |
| Эффект скилла в демо | `SpineDemoViewModel` | нет `.fx`-сета → базовая поза в цикле; нет и позы → анимация не меняется |
| Выбор захвата | `GameEventCaptureLocator`, env `DARKEST_CAPTURE_RUN` | env задаёт конкретный run; иначе новейший `<repo>/captures/run_*` с `manifest.json`, потом эталон `Content\Captures\Reference` |
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
  `DuelBattleViewModel` защищён null-проверками (`?? 0`).
- **Единое VS AI-лобби** — `DuelLobbyViewModel` + `UnitSelectionTableView`: выбор слота и назначение
  героя/монстра через вкладки таблицы, обе стороны могут быть смешанными. Бой — общий
  `DuelBattleViewModel` на `StartFight` + `SetLocalTeam(Team.Heroes)`; правая сторона —
  `MonsterBrainRivalLink`. Отдельного PvE-экрана и `PveBattleViewModel` больше нет.
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
- **Иконки героев — отдельный контент-набор** — раскладка `Content\HeroIcons\<ClassId>\`:
  `portrait.<A-D>.png` (roster-портрет 85×85) и `header.png` (guild-баннер 716×632). Записи
  генерирует `tools\hero-icons-content.ps1` в `HeroIconsContent.props` (75 записей: 15 героев ×
  4 аутфита + баннер) и падает с exit 1, если у героя нет портрета/баннера или spine-герой
  (`unity\Assets\Sprites\Heroes`) не имеет папки иконок. `header.png` читается через
  `IHeroIconCatalog.GetHeader` и показывается в `UnitInfoOverlayView`.
- **Class id, а не имя папки** — spine-папки героев в PascalCase (`ManAtArms`), иконки и
  контент-каталоги — в snake_case (`man_at_arms`); конверсия только через `Ui\HeroIds.ToClassId`
  (не `ToLower`: иначе `ManAtArms` → `manatarms`). Новый герой без иконки не ломает экран:
  карточка показывает `?`.
- **Скилл-эффект ищется по имени файла скелета, а не по папке** — папки `Effects\<Skill>` местами
  содержат опечатки (`ZealosAccusation`); `SpineDemoCatalog.FindAnimationSet` матчит суффикс
  `.sprite.<anim>.skel` и кэширует результат (регистронезависимо). Поэтому `art: anim` из контента
  находит сет, даже если имя папки не совпадает с id.
- **Не у всех скиллов есть `.fx`** — 96 из 105 базовых скиллов имеют `Effects`-сет; у девяти
  (`abomination/manacles`, `bounty_hunter/come_hither` и `hook_and_slice`,
  `grave_robber/flashing_daggers` и `thrown_dagger`, `plague_doctor/noxious_blast`,
  `plague_grenade`, `blinding_gas`, `disorienting_blast`) его нет — демо играет базовую позу
  (`attack_sword` и т.п.) в цикле, а не пустоту.
- **One-shot превью и порядок свойств** — смена `Loop` или `ContentDirectory` перезапускает клип,
  поэтому VM сначала выставляет `IsSkillPlaying` (→ `Loop=false`), затем меняет `ContentDirectory`;
  завершение принимается только при `IsSkillPlaying`. Ручная смена анимации/героя во время
  проигрывания отменяет отложенное восстановление (`OnCurrentAnimationChanged`/`CancelSkillPlayback`).
- **`IconSize` — это область иконки, а не кнопки** — рамка `SkillSquareView` (border 2 + padding 2
  с каждой стороны) добавляет 8 px, поэтому иконка 72 даёт кнопку 80 и корень 88; `IconSize` ниже
  16 клампится. Иконки рендерятся с `BitmapScalingMode=HighQuality`.
- **Карточка героя — один контрол и для стрелок, и без** — `HeroPortraitView` получил DP
  `ShowArrows`/`PreviousCommand`/`NextCommand`/`SelectCommand`: SPINE TEST показывает стрелки
  вокруг портрета, а бой/read-only места — нет. Клик по портрету включается только при заданном
  `SelectCommand` (иначе `IsHitTestVisible=false` и курсор-стрелка); выключенные стрелки
  `Collapsed`, поэтому колонки `Auto` не занимают место. Реальная рамка портрета — `PortraitFrame`
  (`PortraitSize`), а не весь `UserControl` (он растягивается на предложенное пространство).
- **`IsUsable` в лобби — «можно кликнуть сейчас», а не «доступен в бою»** — выбранные скиллы
  всегда `true` (клик снимает выбор); невыбранные — `true`, пока `activeCount < MaxActiveSkills`;
  при лимите 4 они получают `false`. `SkillSquareView` пересчитывает `IsEnabled` по
  `PropertyChanged.IsUsable`, поэтому после снятия одного скилла остальные снова становятся
  кликабельными без пересборки ряда.
- **Выбор скилла: оранжевый / серый + маска, а не пустой квадрат** — `IsSelected` даёт оранжевую
  рамку; в лобби невыбранные получают серую рамку через `IsLoadout=false`-ветку, а `IsMasked`
  рисует полупрозрачный `#B0606060` поверх иконки (иконка остаётся видимой). В бою доступные
  скиллы остаются с золотой рамкой, недоступные — серые с маской. `ToolTipService.ShowOnDisabled`
  на `SkillSquareView` сохраняет тултип и на недоступных/read-only квадратах.
- **Abomination — правило из контента, не хардкод класса** — `HeroClassFileParser` читает
  `skill_selection: .can_select_combat_skills false .number_of_selected_combat_skills_max 7`;
  `HeroSlotViewModel` блокирует `ToggleSkill`, оставляет все 7 выбранными и держит
  `SkillsLocked=true`; отдельной подписи в оверлее нет. Любой другой класс с тем же флагом получит
  то же поведение.
- **Имена не фиксированные** — Unity генерирует их из `Names.xml` (`hero_name_0..555`, `StageCoach`/
  `DarkestPhotonLauncher`); WPF берёт английский пул через `HeroNameCatalog`. Имя создаётся вместе со
  слотом и сохраняется при смене класса; меняется только кнопкой reroll (`RerollNameCommand`).
- **Экипировка и тринькеты — 96×144** — изображение внутри остаётся 72×144 (размер взят из
  `CharacterWindow.prefab`). У экипировки слева отдельная рамка 22×52 с глифом и уровнем, у тринькета
  справа сверху отдельная кнопка reroll; поэтому `EquipmentSlotView` и `TrinketSlotView` одной ширины.
  `HeroEquipmentView`/`HeroTrinketsEditorView` раскладывают по два слота в одинаковых узких панелях
  (weapon/armor, два тринькета + `TRINKETS`). Боевой `HeroTrinketsView` — read-only вариант тех же слотов.
- **Квирки и уровень** — `QuirkRowViewModel` даёт строку `[LockIcon][DisplayName]`; `QuirkIconCatalog`
  грузит `lockquirk.png` из `Content\Quirks`. `HeroLevelView` — переиспользуемый медальон с DP `Level`,
  стоит над Idle-персонажем. Положительный квирк в лобби-моке помечается locked; отрицательные строки
  поддерживают тот же флаг.
- **Семь скиллов в один ряд** — `SkillSquareView` при `ShowName=false` больше не резервирует ширину
  под имя (56 px при `IconSize=48`), а правая колонка оверлея расширена: все 7 боевых и 7 лагерных
  плиток помещаются в один ряд.
- **Оверлей живёт в `LobbyHeroEditorHost` на уровне экрана, не в `HeroSlotsPanel`/`PartySelectionView`**
  — `HeroSlotsPanel` content-sized, а панель отряда в VS AI занимает только половину окна; широкий
  оверлей там был бы обрезан. Хост лежит в корневом `Grid` экрана лобби и растягивается на всё окно;
  события от обоих `PartySelectionView` ведут в один и тот же `Open(slot)`.
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
  только `Kind = estate`. Env `DARKEST_CAPTURE_RUN` (абсолютный или относительный путь) задаёт
  конкретный run для отладки и переопределяет выбор «самого свежего». Папка gitignored;
  копировать файлы вручную не нужно. Пустые слоты формации (без `Character`) в снапшот не
  попадают. Захват, который ещё пишется, лучше не открывать: reader может увидеть неполный хвост.
- **Capture — replay, а не повторная симуляция** — `manifest.Seed` может быть `null`, запись
  может начаться с середины боя (recorder стартует при загрузке сцены) и оборваться без
  `BattleEnded` при выходе в меню; `BattleWinner` тогда остаётся `"—"`. Герой на Death's Door
  (`Hp = 0`, `AtDeathsDoor = true`) не считается мёртвым, даже если старый capture записал
  `IsDead = true`.
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

- `duel/*` (DuelController, FallbackDuelAi), `common_random.md` (детерминизм), `clients_reader.md`
  (контент).
- `docs/SPEC_GAME_EVENTS_SCHEMA.md` — контракт событий `1.0.0`/`1.1.0` и правила чтения; эталон
  `docs/spec_game_events/examples/`; `docs/audio/00_index.md` — каталог FMOD и запись `AudioPlayed`.
- `FEATURE_DESKTOP_CLIENT.md` — детали экранов/механик.
- Unity-оверлеи Тест-боя — `presentation_unity_fight.md`.

## 8. Файлы-источники

- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/*`, `ViewModels/*`, `Ui/TargetArrowMath.cs`,
  `Data/DuelContent.cs`, `Combat/MonsterBrainRivalLink.cs`, `Networking/NetworkRivalLink.cs`
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
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/HeroPortraitView.xaml(.cs)`, `Data/IHeroIconCatalog.cs`,
  `Data/HeroIconCatalog.cs`, `Ui/HeroIds.cs` (карточка героя и иконки)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Data/IHeroSkillCatalog.cs`, `Data/HeroSkillCatalog.cs`,
  `Data/HeroSkillInfo.cs`, `Views/SkillSquareView.xaml(.cs)` (список скиллов и переиспользуемый квадрат)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Data/HeroNameCatalog.cs` + linked
  `unity\Assets\Resources\Data\Localization\Names.xml` (пул имён)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/EquipmentSlotView.xaml(.cs)`, `Views/HeroEquipmentView.xaml`,
  `Views/TrinketSlotView.xaml(.cs)`, `Views/HeroTrinketsEditorView.xaml(.cs)`,
  `Views/HeroTrinketsView.xaml` (переиспользуемые слоты экипировки/тринькетов)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/AbilityRowView.xaml(.cs)`, `Views/HeroSlotsPanel.xaml(.cs)`,
  `Views/PartySelectionView.xaml(.cs)`, `Views/LobbyHeroEditorHost.xaml(.cs)`,
  `Views/UnitInfoOverlayView.xaml(.cs)`, `Views/HeroSlotEditRequestedEventArgs.cs`,
  `ViewModels/ISkillTileViewModel.cs`, `ViewModels/LobbySkillViewModel.cs`,
  `ViewModels/HeroSlotViewModel.cs`, `ViewModels/UnitInfoViewModelBase.cs`,
  `ViewModels/BattleUnitInfoViewModel.cs` (ряд скиллов, слоты и общий оверлей)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/HeroIconsContent.props` + `tools/hero-icons-content.ps1`
  (генератор линков: 75 портретов/баннеров + 105 иконок способностей = 180 записей)
- `docs/mechanics/presentation/spine_transparency_vertex_colors.md` (разбор дефекта vertex colors)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/DuelUnitCardView.xaml` (death's door pulse, «ТВОЙ ХОД»)
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/DuelBattleView.cs` (превью), `DuelBattleViewModel.cs` (пак)
- `docs/FEATURE_DESKTOP_CLIENT.md`