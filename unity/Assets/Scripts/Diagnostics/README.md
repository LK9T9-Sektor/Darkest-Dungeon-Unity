# Diagnostics — JSON-снимок иерархии сцены

Диагностический инструмент для разбора визуальных багов в Unity 7 + URP (первый потребитель —
«жёлтая» стена комнаты в `Dungeon`) и для снятия карты раскладки сцен: координаты, канвасы,
спрайты и тексты — чтобы пересобрать расстановку в Unity и повторить её в WPF. Снимает **полное
дерево сцены** (включая неактивные объекты) со свойствами компонентов в JSON, плюс компактный
layout-лист (CSV) рядом — авто, по смене разрешения и вручную.

## Что делает

- Авто-старт в Play Mode (`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, объект
  `SceneSnapshotDumper`, `DontDestroyOnLoad`).
- Для каждой загруженной сцены — авто-снимки: `auto-settled` (первый отрисованный кадр с
  принудительным `Canvas.ForceUpdateCanvases` — раскладка уже финальная), затем через **5 / 15 c**.
- Смена разрешения Game view — снимок `resize-<WxH>` (после стабилизации раскладки), без F9.
- Рядом с каждым снимком сцены снимается псевдо-сцена **`DontDestroyOnLoad`** (отдельные файлы
  `DontDestroyOnLoad-<триггер>-*`; без скриншота).
- **F9** — ручной снимок активной сцены в любой момент; `SceneSnapshotDumper.Capture("tag")` —
  программный триггер из кода инициализации (задел для семантических моментов, гейм-код не зовёт).
- Снимки сцены сохраняют и **скриншот** (`.png` рядом с `.json`,
  `ScreenCapture.CaptureScreenshot`) — для пиксельного разбора (например, версии «жёлтая заливка»);
  на кадр съёмки временно скрываются зарегистрированные runtime-оверлеи
  (`SceneSnapshotDumper.RegisterCaptureHiddenCanvas` — окно настроек и его кнопка через
  `RuntimeUiFactory`), JSON/CSV при этом не меняются.
- Сбор DTO на главном потоке игры чанками (yield каждые `NodesPerYield` узлов — кадр не
  блокируется), запись файлов — в фоновом потоке.
- Сборка JSON — `JsonUtility` (Newtonsoft в проекте нет — зависимостей не требует).

## Куда пишет

- Папка: `%USERPROFILE%\AppData\LocalLow\RedHooks\Darkest Dungeon\SceneDumps\`
  (`Application.persistentDataPath`).
- Имя файла: `<сцена>-<триггер>-<yyyyMMdd-HHmmss-fff>.json`, например
  `Dungeon-manual-20261017-213005-123.json`; рядом — скриншот `<то же имя>.png` (только для
  снимков сцены) и layout-лист `<то же имя>.csv` (см. ниже).
- Путь к папке и каждому файлу логируется в консоль Unity с префиксом `[SceneDump]`.

## Формат

`SceneSnapshot` (шапка: `SceneName`, `Trigger`, `TakenAtUtc`, `FrameCount`, `TimeSinceLevelLoad`,
`ScreenWidth/Height/Aspect`, `IsDontDestroyOnLoad`, `NodeCount`) → `Roots[]` →
`SceneNodeSnapshot` → `Children[]`:

- `SceneNodeSnapshot`: `Name`, `Path` (`/Root/Child`), `SiblingIndex`, `ActiveSelf`,
  `ActiveInHierarchy`, `Tag`, `Layer`, `LayerName`, `Components[]`, `Children[]`.
- `ComponentSnapshot`: `Type` (полное имя; `<MissingScript>` — потерянный скрипт), `Enabled`,
  `Properties[]` (`Name`/`Value`, значения — строки в invariant-формате).
- Извлекаемые свойства (реестр `SceneSnapshotPropertyReader`): Transform/RectTransform (позиции,
  масштаб, rect, якоря, `screenRectPx`/`screenRectNorm` — экранный прямоугольник в пикселях и в
  долях экрана через root-canvas), Renderer/SpriteRenderer (материал, шейдер, спрайт, цвет,
  bounds, `isVisible`), Camera (включая `clearFlags`, `backgroundColor`, `cullingMask`, `depth`,
  `pixelRect`, `targetTexture`), Canvas (включая `pixelPerfect`)/CanvasGroup, `CanvasScaler`
  (`uiScaleMode`/`referenceResolution`/`screenMatchMode`/`matchWidthOrHeight`), CanvasRenderer
  (рендер-материал `GetMaterial()`: имя/shader/mainTexture/размер), Graphic/Image (цвет,
  материал, шейдер, спрайт + `spriteRect`/`textureRect`/`pixelsPerUnit`, `imageType`/`fillMethod`,
  размер/формат/mip/readable фильтр текстуры), Text (`text` — обрезка до 80 символов, `font`,
  `fontSize`, `alignment`), лейаут-компоненты (LayoutGroup: padding/alignment/spacing/expand,
  GridLayoutGroup, ContentSizeFitter, LayoutElement, AspectRatioFitter), `enabled` у Behaviour.
  Свойства базовых типов идут раньше частных.
- **Layout-лист** (`<то же имя>.csv`, рядом с JSON): по строке на узел, колонка `Kind` —
  `Canvas` (renderMode/sorting/scale + CanvasScaler), `UI` (якоря, pivot, позиция, sizeDelta,
  `ScreenRectPx`/`ScreenRectNorm`, спрайт/тип картинки, текст/размер шрифта, путь канваса) или
  `Camera` (ortho/размер/FOV/clearFlags/depth/pixelRect). Лист — основной артефакт для
  пересборки раскладки и для WPF; значения — те же строки, что в JSON.

Разбор «жёлтой» стены (итог): причина оказалась не в стене, а в камерах — `BlurCamera`/
`UI_Dangeon/Camera` были Base-камерами с `Clear Flags = Nothing`, и URP Render Graph чистил фон в
`Color.yellow` (мир скрыт). Инструмент снял подозрения с ассетов (спрайт/материал/текстура/тексты
не содержали жёлтого). Детали — `docs\mechanics\presentation\presentation_unity_urp_cameras.md`.

## Зависимости и запуск

- Зависимостей вне Unity нет; отдельная установка не нужна (см. выше про JsonUtility).
- Запуск: открыть сцену в редакторе → Play; либо собрать билд (`tools\unity-build-game.ps1`) —
  инструмент активен и в билде; снимки всегда пишутся в `persistentDataPath`.
- Проверка, что работает: в консоли при старте есть строки `[SceneDump] dumps folder: ...` и
  `[SceneDump] auto-capture scheduled ...`; после — `[SceneDump] snapshot ... queued: <путь>`.
  Если папки/строк нет — инструмент не собрался (проверить компиляцию) или F9 нажата не в Play Mode.

## Ограничения

- Активная сцена снимается целиком; `DontDestroyOnLoad` — отдельными файлами на каждое
  capture-событие; каждая additive-сцена снимается своим `sceneLoaded`.
- Значения — строки (для diff’а; числа в invariant). `Renderer.material` не читается (только
  `sharedMaterial`), чтобы не создавать копии материалов.
- Экранный прямоугольник (`screenRectPx`/`screenRectNorm`) считается под разрешение на момент
  снимка; для world-space канваса без камеры пишется `<no-camera>`, у неактивных канвасов, которые
  ни разу не позиционировались, пиксели могут быть смещены (якоря/позиция при этом верны).
- Текст в CSV/JSON обрезается до 80 символов; для снимков `DontDestroyOnLoad` скриншот не пишется.
- Инструмент живёт отдельно от гейм-кода (`Scripts\Diagnostics`); из рантайма его зовут только
  `Capture(tag)` (точка входа на будущее) и `RegisterCaptureHiddenCanvas` (скрытие оверлеев на
  скриншотах). Скриншоты и JSON/CSV — диагностический слой, который перестаёт быть нужен после
  завершения работ по раскладке.

## GameEventRecorder — запись игровых событий в JSONL (авто)

Второй инструмент в этой папке: пишет **игровые события** (не снимки сцены) по контракту
`docs\SPEC_GAME_EVENTS_SCHEMA.md` / спеке `docs\SPEC_GAME_EVENTS.md`. Использует доставленные в
`Assets\Plugins\Internal` библиотеки `Sektor.DarkestDungeon.Core.GameEvents` (шина
`GameEventBus`, стампует `EventId`/`Timestamp`/`Sequence`) и `Sektor.DarkestDungeon.Clients.EventLog`
(`JsonEventLogger` → JSONL + `manifest.json`).

- **Клавиш нет:** каждый рейд записывается автоматически. Старт — когда загружена сцена с
  `RaidSceneManager` (включая Play сразу из `Dungeon`), стоп — когда эта сцена выгружается
  (выход в меню/загрузка) или останавливается Play. Строки JSONL пишутся с `AutoFlush`, а
  `Dispose` дописывает `manifest.json`; при аварийном выходе теряется максимум последняя строка,
  а manifest может быть неполным.
  Вывод: `%USERPROFILE%\AppData\LocalLow\<компания>\<игра>\Replays\run_<yyyy-MM-dd_HH-mm-ss>\` —
  `manifest.json` + `<subsystem>_session.jsonl` (`combat_`, `map_`, `torch_`). В консоли строки
  `[GameEventLog] recording started/stopped: <путь>`; все публикации — no-op, пока запись не идёт.
- **Что публикуется (v1, минимальные вставки в legacy-код):** `sceneSnapshot` (старт рейда в
  `RaidSceneManager.Start`), `partyMoved` (сектор коридора + вход в комнату), `roomDiscovered`
  (комната с `Knowledge.Hidden`), `stateSnapshot` battleStart (после `SpawnEncounter`),
  `attackExecuted` (после `ExecuteSkillAnimationIntro` + кадр; анимация атаки — фактическое имя
  клипа из `FormationUnit.CurrentState.AnimationName`, при переключении скелета — fallback на
  текущий/следующий клип `Animator`; цель — `null`), `heroStatChanged` (дельта HP
  в `ExecuteSkillBase`), `torchChanged` (детект в `TorchMeter.UpdateTorch`,
  причины: `corridorStep`/`snuffed`/`torchUsed`/`modifier`).
- **Id юнитов:** герой `hero_<RosterId>` (стабильно во всём рейде), монстр `monster_<CombatId>`
  (scope боя; battleStart-снапшот публикуется уже после присвоения CombatId).
- **Частичная запись:** recorder стартует при загрузке raid-сцены. Если бой уже идёт (загрузка
  in-raid save), capture начинается с середины; `manifest.Seed` обычно `null`, поэтому точное
  повторение боя невозможно. `BattleEnded` публикуется только при нормальном выходе из
  battle-loop (`RaidSceneManager.cs:2092`); при выходе в меню сцена выгружается, и запись может
  закончиться без него.
- **`IsDead` ≠ Death's Door:** `IsDead` — фактическая смерть (`FormationUnitInfo.IsDead`); герой
  с `Hp = 0` на Death's Door остаётся живым (`AtDeathsDoor = true`). Старые записи могли писать
  `IsDead = true` для 0 HP; WPF отдаёт приоритет `AtDeathsDoor`.
- **Gotcha (измерено):** Unity 7 инжектит глобальные using'и и её сборки содержат типы-двойники
  наших DTO (глобальный `StateSnapshot`, `Unity.VectorGraphics.SceneNode`, `UnityEngine.AnimationRef`,
  а в Assets есть свой глобальный `SceneSnapshot` от дампера) — в `GameEventRecording.cs` все типы
  контракта подключены **алиасами** (`using TorchChanged = Sektor...TorchChanged;`), `SceneSnapshot`
  квалифицируется полностью (CS0576/CS0246/CS0104/CS1729 иначе).
- Проверка, что работает: после Play в консоли есть `[GameEventLog] ready. Raid runs are recorded
  automatically.`; на входе в рейд — `recording started`, после выхода в меню (или остановки Play) —
  `recording stopped` и существование `Replays\run_*\manifest.json` + JSONL.
- **Эталон:** реальный прогон S3 (38 событий, 6 атак с `attack_*`, комната `room2_1`) —
  `docs\spec_game_events\examples\`; он же исполняемая спецификация для WPF-потребителя.
