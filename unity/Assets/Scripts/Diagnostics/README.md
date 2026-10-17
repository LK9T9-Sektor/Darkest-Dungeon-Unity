# Diagnostics — JSON-снимок иерархии сцены

Диагностический инструмент для разбора визуальных багов в Unity 7 + URP (первый потребитель —
«жёлтая» стена комнаты в `Dungeon`). Снимает **полное дерево активной сцены** (включая неактивные
объекты) со свойствами компонентов в JSON — на старте, с задержкой и вручную.

## Что делает

- Авто-старт в Play Mode (`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, объект
  `SceneSnapshotDumper`, `DontDestroyOnLoad`).
- Для каждой загруженной сцены — авто-снимки через **0 / 5 / 15 c** после загрузки.
- **F9** — ручной снимок текущей активной сцены в любой момент (момент визуального бага).
- Снимок сохраняет и **скриншот** (`.png` рядом с `.json`, `ScreenCapture.CaptureScreenshot`) — для
  пиксельного разбора (например, версии «жёлтая заливка»).
- Сбор DTO на главном потоке игры чанками (yield каждые `NodesPerYield` узлов — кадр не
  блокируется), запись файла — в фоновом потоке.
- Сборка JSON — `JsonUtility` (Newtonsoft в проекте нет — зависимостей не требует).

## Куда пишет

- Папка: `%USERPROFILE%\AppData\LocalLow\RedHooks\Darkest Dungeon\SceneDumps\`
  (`Application.persistentDataPath`).
- Имя файла: `<сцена>-<триггер>-<yyyyMMdd-HHmmss-fff>.json`, например
  `Dungeon-manual-20261017-213005-123.json`; рядом — скриншот `<то же имя>.png`.
- Путь к папке и каждому файлу логируется в консоль Unity с префиксом `[SceneDump]`.

## Формат

`SceneSnapshot` → `Roots[]` → `SceneNodeSnapshot` → `Children[]`:

- `SceneNodeSnapshot`: `Name`, `Path` (`/Root/Child`), `SiblingIndex`, `ActiveSelf`,
  `ActiveInHierarchy`, `Tag`, `Layer`, `LayerName`, `Components[]`, `Children[]`.
- `ComponentSnapshot`: `Type` (полное имя; `<MissingScript>` — потерянный скрипт), `Enabled`,
  `Properties[]` (`Name`/`Value`, значения — строки в invariant-формате).
- Извлекаемые свойства (реестр `SceneSnapshotPropertyReader`): Transform/RectTransform (позиции,
  масштаб, rect), Renderer/SpriteRenderer (материал, шейдер, спрайт, цвет, bounds, `isVisible`),
  Camera (включая `clearFlags`, `backgroundColor`, `cullingMask`, `depth`), Canvas/CanvasGroup,
  CanvasRenderer (рендер-материал `GetMaterial()`: имя/shader/mainTexture/размер), Graphic/Image
  (цвет, материал, шейдер, спрайт + `spriteRect`/`textureRect`/`pixelsPerUnit` и размер/формат/
  mip/readable фильтр текстуры), `enabled` у Behaviour. Свойства базовых типов идут раньше частных.

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

- Снимается только активная сцена (не `DontDestroyOnLoad`, не additive-сцены по отдельности —
  каждая additive-сцена снимается своим `sceneLoaded`).
- Значения — строки (для diff’а; числа в invariant). `Renderer.material` не читается (только
  `sharedMaterial`), чтобы не создавать копии материалов.
- Инструмент — временный диагностический слой: удаляется вместе с папкой `Scripts\Diagnostics`,
  гейм-код его не использует.
