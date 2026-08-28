# Журнал изменений

## Не выпущено (после 1.0.6)

### Стресс в дуэли + каталог эффектов в ядро

- В ядро вынесен **каталог эффектов** (`EffectCatalog`, парсер `Data/Mechanics/Effects.txt` — пока
  stress-ключи: `.stress`/`.healstress`); `IBattleContext.ApplyEffectById` резолвит из него.
- **Крит по герою теперь даёт +15 стресса**, а крит-хил снимает 4 — как в кампании (`Effects["Stress 2"]`,
  `Effects["crit_heal_stress_heal"]`); применяется через core-классы `StressEffect`/`StressHealEffect`
  (overstress/resolve-события в ядре).
- **Смерть героя даёт +15 стресса выжившим союзникам** (`Effects["Stress 2"]`, как в кампании).
- **Resolve-ролл при стресс ≥ 100**: герой проходит проверку — аффекция или виртуда
  (`JsonTraits.json`, шанс виртуды 0.25 + resolve-check, клэмп 0.01–0.6); аффекция стрессует
  союзников (`AfflictedAllyStress` 33%×5), виртуда сбрасывает стресс в 20–40.
- **Эффекты скиллов в дуэли**: каталог `Effects.txt` парсит общие эффекты (стан, кровь/яд с
  длительностью, хил, пул/пуш, рипост, куре, шаффл, отметка, иммунизация), `HeroClassFileParser`
  резолвит `.effect` из `.bytes` в `CombatSkill.Effects` — скиллы героев применяют свои эффекты в бою.
  Stat-баффы/дебаффы (`.combat_stat_buff`) — следующий шаг.
- Заведён `docs\GAME_RULES.md` — правила и механики «как реализовано здесь» vs «оригинал DD (позже)».

### WPF-клиент: фикс квирков и полировка низа/шапки боя

- Починен крэш от town-квирков (`weapons_haggler`/`armor_haggler`): их баффы не являются боевыми
  статами и больше не применяются к герою (`ApplyQuirks` пропускает баффы без атрибута у героя).
- Низ боя — одна панель в рамке: скиллы и LOG/INVENTORY/MAP в верхней полосе, ниже три секции
  (статы ходящего / тултип / лог) с разделителями.
- Шапка едина на всех экранах: крестик + панель заголовка/квеста вплотную (меню «выберите режим»,
  лобби «сформируйте отряд», бой «Duel» + статус) — без смещений.
- Torch прибит к верху; кнопки скиллов меньше, текст под кнопкой капсом (как MOVE/PASS).
- Turn Order без «SPD» (просто числа); имена юнитов по командам (красные/синие).
- Лог боя пишет шансы промаха и крита.

### WPF-клиент: полировка боевого HUD, порядок хода по DD, переиспользование выбора отряда

- Боевой HUD: левая панель статов прижата к низу, три панели низа одной высоты,
  LOG/INVENTORY/MAP в ряд со скиллами (надпись «Skills» убрана, кнопки меньше); нижняя область
  компактнее — больше места полю.
- Карточки юнитов: рамки по командам (красная/синяя), у ходящего — белая (возврат после хода);
  между отрядами пробел; слоты под изображения (пока пустые).
- «Round N» перенесён под факел (не перекрывает поле).
- Turn Order: квадратики с реальной скоростью и нароленной инициативой; порядок по правилам DD
  (инициатива = скорость + бросок 0-10) — первым ходит не всегда игрок 1.
- Крестик всегда слева-сверху с панелью заголовка/квеста вплотную; единые закруглённые рамки.
- Формирование отряда переиспользует карточку боя: портрет/класс, стрелки вверх/вниз, панель
  статов, скиллы (кроме Move/Pass) с тултипами, квирки + реролл.
- Богатый тултип навыков (урон/меткость/крит/хил, ранги) — переиспользуется в бою и лобби.

### WPF-клиент: адаптивная раскладка, редактируемый отряд ИИ, богатые тултипы скиллов

- Клиент переведён на адаптивную (responsive) раскладку: окно ресайзится, экраны занимают всё
  доступное пространство (`*`-строки/колонки), убран фиксированный холст 1920×1080 с Viewbox.
- Боевой экран: центр поля меньше, нижняя панель выше (инфо ходящего/тултип/лог помещаются),
  тултип и лог шире, Retreat «X» прибит к квест-панели.
- Тултипы скиллов: богатое описание (урон/меткость/крит/хил, ранги запуска/цели) в стилизованной
  тёмной подсказке.
- Выбор отряда: переиспользуемая панель `PartySelectionView` (#1 Player / #2 AI) — отряд ИИ теперь
  полностью редактируется как свой (классы, активные скиллы, черты); «Reroll AI» — быстрый рандом.
- Классы слотов стартуют случайными разными (а не все «abomination»); имена классов читаемые.
- Крестик в главном меню закрывает приложение.

### WPF-клиент: дуэльная оркестрация вынесена в ядро, ИИ через MonsterBrain

- Дуэльная оркестрация переехала из `src\Wpf` в новый чистый core-модуль
  `Sektor.DarkestDungeon.Core.Duel` (netstandard2.0, C# 7.3): `DuelController`, фазы, локстап-сид,
  wire-протокол, адаптеры контекста/событий, порт `IDuelContent`. WPF-клиент стал тоньше.
- ИИ соперника в дуэли теперь ходит как монстр в Darkest Dungeon (`DuelAi` поверх core-brain):
  зеркалит «default»-брейн — хилер лечит самого раненого союзника (<50% HP), прочие атакуют
  случайно/по отметке, применяются кулдауны; выбор детерминирован от сида сессии.
  Оригинальная логика ядра/Unity не тронута — реализация «поверх», в `Core.Duel`.
  Поведение заметно в режиме VS AI.

### WPF-клиент: фиксы боевого HUD и лобби

- Починен крэш дуэли при превращении Абоминации (`XamlParseException "Cannot animate ... constant
  instance of an object"`): всплывающий урон больше не анимирует замороженный `RenderTransform`
  (Freezable в шаблоне карточки), а поднимается анимацией `Margin` + `Opacity`.
- Левая панель боя приведена к виду Darkest Dungeon: портрет+имя/класс, статы без резистов,
  шмот и 2 слота тринкетов; скиллы/резисты/квирки остались только в листе статов правого клика
  (у левой панели больше нет слотов скиллов и секций SKILLS/RESISTANCES/QUIRKS).
- Тултип при наведении на юнита теперь полностью виден (не прячется под левой нижней панелью).
- MOVE и PASS стали квадратными как скиллы, с глифами (стрелки у MOVE, крестик у PASS) и подписью снизу.
- Оба лобби (vs AI и мультиплеер) перестроены в широкоформатные ряды сверху вниз: герои игрока
  сверху, отряд ИИ/соперник в центре, кнопки внизу, крестик-возврат сверху (общий `ScreenHeaderView`);
  элементы больше не перекрываются.
- Добавлен smoke-тест загрузки всех экранов WPF (`ScreenSmokeTests`).

### Вынос из Unity: квирки, активные скиллы, Move/Pass

- Из контента игры вынесены квирки героев: `JsonQuirks.json` → ядро (`Quirk` + `QuirkMapper`,
  положительные/отрицательные, buffs, несовместимости) — фундамент для reroll черт в выборе героев.
- Активные боевые скиллы: из контента классов читается `number_of_selected_combat_skills_max`
  (4 у всех классов, 7 у Абоминации); герой несёт выбранный набор (`SelectedCombatSkills`),
  конфиг отряда по сети передаёт выбранные скиллы.
- В дуэль добавлены реальные действия **Move** (смена ранга с союзником) и **PASS** (пропуск хода)
  — детерминированные, синхронизируются как обычные входы.
- Выбор героев в лобби (vs AI и мультиплеер): у каждого героя выбираются активные боевые
  скиллы (квадратные кнопки), наведение на класс показывает статы и скиллы, кнопка «⟳»
  перебрасывает черты (положительные/отрицательные из `JsonQuirks.json`).
- Боевой экран: подробный лог боя (удары/криты/промахи/хилы/убийства/ходы), всплывающие
  числа урона над юнитами, HP блоками и стресс 10 квадратами, квадратные скиллы с
  подсказкой рядом, тултип юнита в 2 колонки, карточки на «полу».
- Черты теперь **влияют на бой**: вынесен парсер `JsonBuffs.json` (буффы), квирки героя
  применяются как permanent-баффы (`BuffSourceType.Quirk`) при создании героя в дуэли
  (как в Unity); выбор/reroll черт в лобби передаётся в конфиг отряда (локстап);
  черты видны в листе статов (правый клик).

### WPF-клиент: экранная навигация, меню vs AI, полный ростер героев

- Все экраны WPF-клиента теперь подменяются внутри одного окна (`MainWindow`,
  `ContentControl` + MVVM-навигация `INavigationService`/`ShellViewModel`): отдельные
  окна лобби и дуэли (`ShowDialog`) убраны.
- Главное меню в стиле игры: две кнопки по центру — **VS AI** и **MULTIPLAYER**.
- Режим VS AI: то же лобби выбора героев без инициализации Steam; отряд ИИ генерируется
  случайно из героев (без боссов). Соперник полиморфный: `NetworkRivalLink` (сеть) или
  `AiRivalLink` (случайные легальные ходы) — одна и та же боевая VM без ветвлений по режиму.
- Выбор героев больше не ограничен 4 захардкоженными классами: новый парсер контент-файлов
  `Data/Heroes/Info` в ядре (`HeroClassFileParser` + `HeroCatalog`, базовый ранг прокачки)
  загружает весь ростер (15 классов); определения линкуются в вывод WPF из unity-контента.
- Исправлен баг мультиплеера: хост после создания комнаты не попадал в бой при входе второго
  игрока — handshake готовности теперь симметричный (обе стороны шлют `player_loaded`).
- В мультиплеер-лобби виден счётчик ожидания соперника («Waiting mm:ss»).
- Боевой экран дуэли перерисован в раскладке боевого поля как у Unity-макета: ранги лицом
  друг к другу, полоса порядка хода сверху, скиллы и лог внизу; «Retreat» возвращает в меню.
- Починка после рефактора: счётчик «Waiting mm:ss» и обработка событий транспорта в
  мультиплеер-лобби снова работают (лобби пумпится как и боевой экран); на карточках юнитов
  боя снова виден уровень стресса.
- Боевой экран дуэли получил полный HUD в раскладке мокапа: три панели — верх (квест +
  квадратный красный «X» Retreat, факел, живая очередь хода), центр (поле, номер раунда,
  статус), низ (скиллы ходящего, инфо ходящего, тултип при наведении, переключатель
  LOG/INVENTORY/MAP с логом боя). Правый клик по юниту открывает лист статов с живыми
  характеристиками (HP, стресс, скорость, урон, меткость, крит, уклонение, защита).

### SESSION LOG: события Photon в лог + крупнее шрифты + единый стиль оверлеев

- События Photon (подключение, комнаты, регион, сбои) теперь пишутся в окно SESSION LOG
  (`MultiplayerSync.WriteLog`/`WriteError`) — «Connected!», «Disconnected!», join/create/fail комнат
  видны прямо в игре, в обоих проектах.
- Шрифты окна SESSION LOG увеличены: тело 18→22, заголовок 24→28, текст лога 17→20; окно расширено.
- Новый core-модуль `src\Core\Ui` (netstandard2.0, C# 7.3, engine-free): `UiStyle` (путь шрифта,
  семантические размеры, цвета `ArgbColor`). DLL доставляется пост-билдом в `Assets\Plugins\Internal`
  обоих проектов.
- Runtime-оверлеи (`MultiplayerLogUI`, `MultiplayerProviderMenu`, `SteamLobbyIdPanel`,
  `SoundSettingsUI`) в обоих проектах унифицированы: читают токены `UiStyle` и используют общий
  `RuntimeUiFactory` (canvas/text/button/EventSystem), убран дублированный код и расходящиеся цвета.

### Вынос контента в чистое ядро (Фаза 1, первый срез)

- Новый модуль `src\Core\Content` (netstandard2.0, C# 7.3): модели `Campaign\` (`HeirloomExchange`, `PartyNameEntry`) и `Database\` (DTO `Json*` + мапперы `HeirloomExchangeMapper`/`PartyNameMapper`). DLL доставляются пост-билдом в `Assets\Plugins\Internal` обоих проектов.
- `DarkestDatabase` стал тоньше: `LoadHeirloomExchanges`/`LoadPartyNames` сводятся к вызовам мапперов ядра; JSON-десериализация (`JsonConvert`) остаётся в адаптере презентации.
- NUnit-тесты `tests\Core\Content` на реальных данных (`HeirloomExchange.json`, `PartyNames.json`).
- DTO — snake_case-члены без `[JsonProperty]`, ядро не зависит от Newtonsoft: сборки Newtonsoft 11/12/13 (включая `net45`/`netstandard2.0`) ссылаются на контракты net6.0 и дают `CS0009` в Unity 2017.4, поэтому атрибуты невозможны до перехода проектов на совместимый Newtonsoft.
- Исправление стартовой загрузки данных/кампании после синка с веткой: `tools\unity-provision-plugins.ps1` теперь также собирает `src\Core\Content` и доставляет его DLL/PDB в `Assets\Plugins\Internal` обоих проектов (ранее сборка отсутствовала → `CS0246`, игра не стартовала).


### Unity 6.5 (6000.5.8f1): EntityId-миграция

- Редактор обновлён 6000.4.5f1 → 6000.5.8f1: устаревшие API стали ошибками компиляции (CS0619): `GetInstanceID()`, `EditorApplication.hierarchyWindowItemOnGUI`, `EditorUtility.InstanceIDToObject(int)`.
- Свои скрипты переведены на `EntityId` (`Object.GetEntityId()`): RNG-сиды — через `GetEntityId().GetHashCode()` (`DarkestPhotonLauncher`, `MultiplayerPartyPanel`, `RoomSelector`).
- Spine: ключи атласных таблиц и сравнение материалов на `EntityId` (`SpriteAttacher`, `SkeletonRenderer`, `SpineEditorUtilities`); событие иерархии — `hierarchyWindowItemByEntityIdOnGUI`.
- FMOD: `EventBrowser` переведён на `hierarchyWindowItemByEntityIdOnGUI`/`EntityIdToObject` (vendored, минимальный дифф).
- Инструменты `tools\*.ps1` находят редактор 6000.5.8f1; доставка .NET Standard фасадов пропущена — на Unity 6 они не нужны (нативный type-forwarding).

### Надёжный вход клиента в Steam-рейд

- Загрузка сцены `DungeonMultiplayer` теперь ждёт готовности соперника: и хост, и клиент не входят в рейд, пока не получен состав отряда другого игрока (`party_config`) — `RoomSelector.LoadSteamRaidSceneRoutine` ожидает `MultiplayerSync.HasRivalParty` (таймаут 30 с: статус «Opponent did not join in time» и выход из сессии). Раньше клиент мог попасть в пустую «дефолтную» сцену рейда, если состав хоста ещё не дошёл до момента `RaidSceneMultiplayerManager.Awake`, а хост входил в рейд один и сразу «завершал» квест (`PlayerCount < 2`).
- Защитный фолбэк в `RaidSceneMultiplayerManager.Awake`: если состав соперника отсутствует на старте рейда, отряд строится из локального состава, чтобы сцена не оставалась пустой.

### Лог мультиплеера в игре

- Новый `MultiplayerLogUI` (`Assets\Scripts\Networking\MultiplayerLogUI.cs`): постоянная кнопка «SEND» под кнопкой настроек звука открывает обычное окно с логом сессии (чёрный фон, заголовок, прокрутка, крестик закрытия). Строки лога — в формате консоли Steam-реализации: `[HH:mm:ss.fff] [Категория] сообщение`.
- Лог хранится в состоянии сессии (`MultiplayerSync.WriteLog`/`WriteError`/`LogLines`), дублируется в консоль Unity и сбрасывается при входе в сессию и при её завершении. Все сообщения `[STEAM]`/`[MULTIPLAYER]` сетевого слоя переведены на него (включая события сессии `ROOM_ID`, подключение/уход игрока, обрыв, ошибки инициализации).

### Мелкие правки UI мультиплеера

- Панель Steam Lobby ID перенесена выше и левее кнопки настроек (справа сверху, слева от шестерёнки).
- Меню выбора провайдера: возвращён видимый крестик закрытия (справа сверху), фон панели — простая чёрная заливка вместо рамки `WindowFrame`.

### Запуск игры без редактора

- Скрипты `tools\unity-build-game.ps1`, `tools\unity-run-game.ps1`, `tools\unity-dev-run.ps1` и точка сборки `BuildGame.Build` (`Assets\Editor\BuildGame.cs`): сборка standalone Windows x64 через Unity batchmode (включает проверку блокировки проекта, доставку плагинов Lan, размещение `steam_appid.txt` рядом с exe) и запуск собранного плеера без открытия редактора. Вывод — `Build\Darkest Dungeon\Darkest Dungeon.exe`.
- `tools\unity-compile-check.ps1` — быстрая проверка компиляции скриптов (включая `Assets\Editor`) в таргетной Unity 2017.4 без сборки плеера: batchmode импорт + компиляция, разбор лога на `error CS`/`Compilation failed`.

### Steam Lobby ID в интерфейсе

- Новый переиспользуемый компонент `SteamLobbyIdPanel` (`Assets\Scripts\Networking\SteamLobbyIdPanel.cs`): DontDestroyOnLoad панель справа сверху (под кнопкой звука) с текстом «Steam Lobby ID: …» и кнопкой Copy, которая копирует ID лобби в буфер обмена (с кратким «Copied!»). Один и тот же компонент показывается и в списке комнат сразу после создания сессии хостом, и в подземелье мультиплеера; скрывается, когда Steam-сессия не активна.
- Исправлен чёрный экран после выхода из подземелья в Steam-режиме: `SteamSessionManager.LeaveSession` теперь загружает `CampaignSelection`, если сессия завершается в сцене `DungeonMultiplayer` (ранее сцену загружали только обработчики ухода соперника/обрыва соединения).

### Исправления Steam co-op и UI (после теста билда)

- Исправлен вход клиента в подземелье: хост теперь переотправляет состав отряда (`party_config`) в момент подключения клиента (`SteamSessionManager.OnPlayerJoined`). Раньше клиент не получал отряд хоста и попадал в пустую «дефолтную» сцену рейда.
- Панель Steam Lobby ID отображается при любой активной Steam-сессии (без привязки к сцене), фон плотнее, текст ярче, шрифт игры.
- Шрифт runtime-панелей (`MultiplayerProviderMenu`, `SoundSettingsUI`, `SteamLobbyIdPanel`) заменён с `Deutsch` на шрифт игры **DwarvenAxe** (`Assets\Resources\Fonts\DwarvenAxe.ttf`); ряды PHOTON/STEAM переведены в стиль обычного текста игры (35px, золото), подсказка — с переносом и нормальными якорями.
- Окно настроек звука: фон `WindowFrame` рендерится через `Image.Type.Sliced` и заполняет весь контрол (крестик закрытия больше не «висит» вне картинки).
- Статусная панель списка комнат в Steam-режиме перецентрирована и текст с переносом больше не уезжает за левую часть экрана.

## 1.0.6 — общий мультиплеер: выбор провайдера и гарантированное закрытие Steam

Версия 1.0.6 (ветка `steam`) убирает отдельную Steam-панель лобби: вход в мультиплеер теперь начинается с оверлея выбора провайдера PHOTON/STEAM, а дальше оба провайдера используют один и тот же экран списка комнат и выбор героев (панель отряда переиспользуется). Также исправлено зависание «Spacewar» в Steam после закрытия игры.

### Меню выбора провайдера

- Новый `MultiplayerProviderMenu` (`Assets\Scripts\Networking\MultiplayerProviderMenu.cs`) — runtime-оверлей на `CampaignSelection`: две строки PHOTON/STEAM, крупный шрифт (40), навигация стрелками ↑/↓ + Enter (Esc — закрыть), поддержка мыши (hover/клик). Текущий провайдер подсвечен при открытии.
- `RoomSelector.SaveSelectionStart` (кнопка «Multiplayer») открывает меню провайдера; выбор инициализирует провайдера (`MultiplayerSync.SetSteamProvider`; для Steam — `MultiplayerSync.EnsureSteamSession`, создаёт/инициализирует `SteamSessionManager` без панели) и открывает общий список комнат `RoomSelector.OpenRoomList()`.
- `SteamLauncher` (панель «Steam Co-op Lobby», переключатель PHOTON/STEAM, кнопка «Open STEAM Lobby») и `MultiplayerMenuState` удалены.

### Общий список комнат (переиспользование выбора героев)

- `RoomSelector` открывает единый экран для обоих провайдеров: выбор героев (`MultiplayerPartyPanel` + `CharacterWindow`) и статус-панель общие.
- В Steam-режиме три слота комнат служат вводом lobby ID: подтверждение слота вызывает `RoomSelector.JoinSteamLobby(id)` → `SteamSessionManager.JoinSession` (с проверкой выбора скиллов). Кнопка Play — хост новой сессии (`HostSession`).
- При создании/входе в сессию (`MultiplayerSync.SessionJoined`) отряд захватывается, отправляется сопернику (`party_config`) и загружается `DungeonMultiplayer`; возврат/разрыв сессии — через `MultiplayerSync.Steam.LeaveSession`.
- Steam не инициализируется в Photon-режиме и наоборот; недоступность Steam показывается в статус-панели («Steam unavailable: …»).

### Гарантированное закрытие Steam

- `SteamSessionManager.OnApplicationQuit` освобождает транспорт (`SteamAPI_Shutdown`), идемпотентно и с общим путём `Shutdown()` для `OnDestroy`. Steam-клиент больше не считает «Spacewar» запущенной после Stop в Play-режиме редактора и после выхода из собранного билда.

### Проверка

- NUnit `tests\Lan` — 14/14 зелёных (сетевое ядро не менялось).
- Версия в `GameInfo` — `1.0.6` (ProjectSettings синхронизированы: `bundleVersion 1.0.6`, Android `10006`).

## 1.0.5 — Steam co-op (боевая сессия 2 игроков)

Версия 1.0.5 (ветка `steam`) добавляет кооперативную арену над Steam P2P: лобби «Steam Co-op Lobby» на экране выбора кампании, создание/вход по ROOM_ID, синхронизация выбранных героев между игроками и бой 2×4 по старой схеме Photon-режима, но без облачных серверов — напрямую P2P через Steam.

### Сетевое ядро (чистые библиотеки, `src\Lan\`)

- `Sektor.DarkestDungeon.Lan.Contracts` — интерфейс транспорта `ITransport` (сессия = лобби, сообщения = `TransportMessage`), `Result`/`Result<T>`, кодек `ITransportCodec`. Без зависимостей.
- `Sektor.DarkestDungeon.Lan.Steam` — `SteamTransport`: Steam P2P (надёжный канал, channel 1), сессии Steam Lobies, собственный interop-слой (`Interop\`, P/Invoke на flat-API с manual dispatch), rich presence `connect` для входа по приглашению.
- Автодоставка: post-build копирует `*.dll`/`*.pdb` в `Assets\Plugins\Internal\` (вместе с .NET Standard facade-шимами из редактора — см. `COMPABILITY.md`); `steam_api64.dll` — native-плагин `Assets\Plugins\x86_64\`; локальный `steam_appid.txt` (480) для dev-запусков.
- Скрипт доставки: `tools\unity-provision-plugins.ps1`.

### Unity-слой (`Assets\Scripts\Networking\`)

- `SteamRaidBridge` — диспетчер входящих сообщений: RPC-сообщения `rpc.<method>` повторяют вызовы легаси-обработчиков (`PhotonGameManager`), `party_config` — состав отряда соперника.
- `SteamSessionManager` (MonoBehaviour, живёт между сценами) — владеет транспортом, качает колбэки каждый кадр, переводит события сессии в `MultiplayerSync`; при разрыве сессии во время рейда возвращает в `CampaignSelection`.
- `MultiplayerPartyData` — DTO состава отряда (4 героя: класс, имя, сид генерации, флаги выбранных скиллов), сериализуется в строку для канала.
- `MultiplayerSync` — статический фасад для легаси-слоя: в Steam-режиме идёт в Steam, в Photon-режиме — в оригинальные пути; `PreparationCheck`, `SendRpc`, `LoadLevel`, `LeaveRoom`, `HostRaidParty`/`MonsterSideRaidParty`.
- `SteamLauncher` — runtime-панель лобби на `CampaignSelection` (создание/подключение, ROOM_ID, статус, таймаут подключения, переход в `DungeonMultiplayer`). Панель скрыта по умолчанию и открывается по требованию (`OpenPanel`, создаёт лаунчер при отсутствии): в Steam-режиме `RoomSelector.SaveSelectionStart` показывает её поверх экрана выбора кампании. На панели — переключатель провайдера PHOTON/STEAM (в PHOTON возвращаются к `RoomSelector`).
- Устойчивость к недоступному Steam: `SteamTransport` не вызывает нативный Steam API, пока runtime не инициализирован (защита в `RunCallbacks`, `Dispose`, rich-presence/lobby-data путях и всех public-методах — от краша при нулевых указателях интерфейсов); `SteamSessionManager.IsInitialized` отражает реальный успех init; при неудаче панель показывает причину («Steam unavailable: …»), отключает Host/Join, а повторное открытие панели после запуска Steam-клиента переинициализирует транспорт без перезагрузки.
- Панель лобби оформлена по образцу окна настроек (1.0.4): 1024×740 строго по центру экрана, фон `WindowFrame` (`SoundSettingsSprites`), крестик «X» (CloseIcon) в правом верхнем углу; кнопки Return нет — закрытие только крестиком или Escape. Экран выбора кампании при открытии лобби не скрывается, поэтому после закрытия панели меню остаётся на месте без восстановления.
- Переключение провайдера в обе стороны: `MultiplayerMenuState` (`Assets\Scripts\Networking\MultiplayerMenuState.cs`) — синглтон состояния меню мультиплеера (None/Steam/Photon). Из Steam-панели кнопка PHOTON открывает список комнат Photon, а поверх него появляется кнопка «Open STEAM Lobby» (вверху экрана), которая возвращает в Steam-лобби (список комнат закрывается). Выбор провайдера больше не запирает игрока: из любого окна всегда есть видимый переход в другое.

### Легаси-интеграция (минимальные правки)

- `RaidSceneMultiplayerManager`, `BattleGround`, `BattleFormation`/`FormationParty`, `RaidQuestPanel`, `BarkMessenger`, `MainMenuWindow`, `RaidParty`, `Hero`, `RoomSelector` — вызовы Photon заменены на `MultiplayerSync`/строковые имена; Photon-путь сохранён (ветки `MultiplayerSync.IsSteamSession`/`IsSteamProvider`).
- Новые конструкторы `RaidParty(MultiplayerPartyData)` и `Hero(int, MultiplayerPartyData)` рядом с существующими Photon-конструкторами.

### Проверка

- NUnit `tests\Lan` — 14/14 зелёных (кодек, жизненный цикл, round-trip на in-memory транспорте).
- Компиляция Unity 2017.4.40f1 batchmode: `Compilation succeeded`, без ошибок (в т.ч. после фикса краша при недоступном Steam).
- Версия в `GameInfo` — `1.0.5` (ProjectSettings синхронизированы: `bundleVersion 1.0.5`, Android `10005`).

## 1.0.4 — звуковые настройки

Версия 1.0.4 (ветка `1.0.4`) добавляет в игру окно звуковых настроек: регулировка громкости музыки и SFX, кнопки закрытия и выхода, а также связывает громкость SFX с боевыми и мировыми звуками.

### Новая функциональность

- Runtime-окно настроек `SoundSettingsUI` (`Assets\Scripts\UI\SoundSettingsUI.cs`): создаётся автоматически через `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` и живёт во всех сценах (`DontDestroyOnLoad`, Canvas sortingOrder 30000, CanvasScaler 1920×1080).
- Два степпера громкости — музыка и SFX: 10 шагов (клик = 10% шкалы), текущие значения синхронизируются из `DarkestSoundManager` при каждом открытии.
- Кнопка-шестерёнка «AudioButton» (56×56, спрайт `settings.button`) в правом верхнем углу экрана — открытие окна.
- Окно 1024×740 по центру экрана (фон `menu.background`), заголовок «Settings», шрифт Deutsch, цвет текста — оригинальный `_labelColor`.
- Кнопка закрытия «X» (32×32, `progression_close.png`) в правом верхнем углу окна — скрывает панель.
- Кнопка «Exit to Main Menu» (466×97, `menu.element_text_button_overlay.png`) внизу окна — возврат в главное меню.
- Локализация через существующие ключи `Menu.xml`: `menu_options_title`, `menu_options_element_music_volume`, `menu_options_element_sfx_volume`, `menu_base_element_exit_campaign` (с фолбэком на английский при недоступности менеджера).

### Спрайты и ассеты

- Спрайты собраны в ScriptableObject `SoundSettingsSprites` (`Assets\Resources\UI\SoundSettingsSprites.asset`): `WindowFrame`, `MinusArrow`, `PlusArrow`, `CloseIcon`, `ExitButtonOverlay`.
- Ссылки на оригинальные ассеты (`Assets\Sprites\...`, исключены из репозитория) — по GUID; при отсутствии спрайта UI использует тёмную заливку вместо него.

### Звук

- `DarkestSoundManager.PlayOneShot` переписан: create → `setVolume(SfxVolume)` → start → release, без общего поля — одновременные боевые звуки не режутся и подчиняются громкости SFX.
- 135 прямых вызовов `FMODUnity.RuntimeManager.PlayOneShot` переведены на `DarkestSoundManager.PlayOneShot` в 9 файлах (`RaidSceneManager`, `RaidEvents`, `RaidSceneMultiplayerManager`, `HealEffect`, `BattleGround`, `EstateCurrencyPanel`, `HallSector`, `Room`, `FormationOverlaySlot`).
- 3 звука через `CreateInstance` (`combat/start` ×2, `props/curios`) получили `setVolume(SfxVolume)`.
- Музыка не затрагивается (воспроизводится не через `PlayOneShot`).

### Выход из игры

- Кнопка «Exit to Main Menu» не закрывает игру напрямую, а возвращает в главное меню: вызывает `MainMenuWindow.ReturnToCampaignSelection()` (сохранение, выход из мультиплеерной комнаты, `SilenceNarrator`), с фолбэком на `SceneManager.LoadScene("CampaignSelection")`.
- Полный выход из игры происходит только из главного меню через собственную кнопку «Return To Desktop» → `MainMenuWindow.QuitGame()`.
- В редакторе Unity `MainMenuWindow.QuitGame()` останавливает Play Mode (`UnityEditor.EditorApplication.isPlaying`); в сборке — `Application.Quit()`.

### Изменённые файлы

| Файл | Изменение |
|---|---|
| `Assets\Scripts\UI\SoundSettingsUI.cs` | окно настроек, степперы, кнопки |
| `Assets\Scripts\UI\SoundSettingsSprites.cs` | ScriptableObject со спрайтами |
| `Assets\Resources\UI\SoundSettingsSprites.asset` | ссылки на оригинальные спрайты |
| `Assets\Scripts\Managers\DarkestSoundManager.cs` | `PlayOneShot` + громкость SFX |
| `Assets\Scripts\UI\Windows\MainMenuWindow.cs` | выход из игры: Play Mode в редакторе, `Application.Quit()` в сборке |
| `Assets\Scripts\Managers\RaidSceneManager.cs` и др. (9 файлов) | маршрутизация SFX через громкость |

### Проверка

- Компиляция Unity 2017.4.40f1 batchmode: `Compilation succeeded`, без ошибок.
