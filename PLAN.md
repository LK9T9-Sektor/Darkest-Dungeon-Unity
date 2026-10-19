# PLAN.md — планы задач

Два состояния плана, оба — в git:

- **активный** — на этой странице: цель, шаги, статусы;
- **отложенный** (незавершённый) — `docs\plans\<ГГГГ-ММ-ДД>-<слаг>-deferred.md` (дата — момент
  отложения; внутри «Состояние» и исходный план), индекс — `docs\plans\00_index.md`.

Завершённые планы не хранятся (их держит git-история); user-visible итог — коротким файлом в
`docs\changelog\<ГГГГ-ММ-ДД>-<слаг>.md` (индекс — `docs\changelog\00_index.md`).
Мелкие задачи без плана в архив не попадают. `.opencode\` для планов не используется.

---

## Отложенные планы

- `docs\plans\2026-10-19-fixed-battle-scene-deferred.md` — фаза 2-A (пин высоты боевой сцены +
  `UI_Overlay` match-height). Возобновление: когда будут доступны эталонные снимки с другого ПК и
  прогон сетки аспектов.

## Активный план

# PLAN: S2 — контракт игровых событий (GameEvents + EventLog)

Ветка: `core/game-events-contract` (от `main` 0633d8bd). Спека: `docs\SPEC_GAME_EVENTS.md`.

## Goal

(1) Библиотека DTO+шины игровых событий в чистом Core и (2) JSONL-логгер на границе клиента —
готовая база для S3 (Unity-рекордер сценария «старт сэйва → коридор → комната → бой → факел»)
и для WPF-потребителя. Данные вместо догадок: DTO в C# = schema.md = примеры логов.

## Design decisions (user-confirmed)

- Новые модули `Core.GameEvents` + `Clients.EventLog`; **TFM netstandard2.0** (доставка в оба
  Unity-дерева через `CoreUnityPluginFolders`), только `LangVersion latest` локально в csproj
  (записи/record); Newtonsoft `TypeNameHandling.Auto` (STJ в проекте нет) — только в
  `Clients.EventLog`, чистый Core без JSON (правило AGENTS).
- Обязательные поля `Input`/`Rng`: nullable; `null` = «не инструментировано», причина
  фиксируется в schema.md (чек-лист §8 спеки допускает явный null с причиной).
- Scope v1 — только типы под сценарий S3: `sceneSnapshot`, `stateSnapshot`, `partyMoved`,
  `roomDiscovered`, `attackExecuted`, `heroStatChanged`, `torchChanged`, `skillUnavailable`,
  `actionRejected`. Инвентарь (`item*`) и заполнение rng — отдельными шагами позже.
- `Publish` шины проставляет `EventId`/`Timestamp`/`Sequence` (Interlocked, монотонность) —
  единая точка нумерации; издатели дают `Subsystem`/payload/`Input`/`Rng`.

## Steps / status

1. [x] Ветка `core/game-events-contract`, PLAN.md зафиксирован.
2. [x] `src\Core\Sektor.DarkestDungeon.Core.GameEvents` (netstandard2.0 + `LangVersion latest`
   + полифилл `IsExternalInit`): `GameEvent` (abstract record), `GameInput`/`GameInputKinds`,
   `GameRng`, `SubsystemNames`,9 событий v1, DTO (`SceneNode`/`SceneGlobals`/`CombatantState`/
   `AnimationRef`/`StateSnapshotContexts`), `IGameEventBus`/`GameEventBus` (stамп EventId/
   Timestamp/Sequence через `with`-копию, dispatch по assignability, отписка-`IDisposable`,
   guard-return вместо исключений). PostBuild доставляет DLL в оба Unity-дерева (проверено).
   Тесты `GameEventBusTests` (4): стамп/порядок, параллельная монотонность (1..200), типизированная
   доставка/отписка, null-guard.
3. [x] `src\Clients\Sektor.DarkestDungeon.Clients.EventLog`: `JsonEventLogger` (подписка на шину,
   ленивый `StreamWriter` на подсистему `<subsystem>_session.jsonl`, `AutoFlush`,
   `manifest.json` при создании/новом файле/Dispose, fallback `unspecified`) +
   `GameEventManifest`; Newtonsoft `TypeNameHandling.Auto` с declared-типом `GameEvent` (иначе
   `$type` не пишется у корня — ловушка закрыта тестом roundtrip). Доставка в оба Unity-дерева
   (проверено). Тесты `JsonEventLoggerTests` (3): per-subsystem порядок/полиморфный roundtrip,
   manifest (schemaVersion/seed/notes/files), fallback пустой подсистемы.
4. [x] Регистрация4 проектов в `Darkest-Dungeon-Unity.slnx`; `TARGET_LAYOUT.md` (таблица + дерево
   `src/`) и `ARCHITECTURE.md` §2 (строки `GameEvents`, `Clients.EventLog`) обновлены.
5. [x] `docs\SPEC_GAME_EVENTS_SCHEMA.md`: формат/manifest/чтение, базовые поля (кто стампит,
   null-причины),9 типов с триггерами/полями/инвариантами и **реальными** строками-примерами
   (сняты из тестового вывода), чек-лист §8 (покрыто/отложено), отклонения от спеки;
   строка в `docs\INDEX.md`.
6. [x] `dotnet test` по всем12 тестовым проектам — exit 0; `tools\check-using-placement.ps1` — OK.
7. [x] Коммит/пуш по подтверждению пользователя.

## Affected files

- Новые: `src\Core\Sektor.DarkestDungeon.Core.GameEvents\*`,
  `src\Clients\Sektor.DarkestDungeon.Clients.EventLog\*`, `docs\SPEC_GAME_EVENTS_SCHEMA.md`.
- `Darkest-Dungeon-Unity.slnx`, `docs\TARGET_LAYOUT.md`, `docs\ARCHITECTURE.md`, `PLAN.md`.

## Acceptance criteria

- Обе библиотеки собираются netstandard2.0, тесты зелёные; логгер пишет валидный JSONL +
  manifest и читается обратно полиморфно (`$type`/дискриминатор).
- Newtonsoft отсутствует в `Core.GameEvents`; чистый Core не бросает исключений (Result/данные).
- Легаси (`unity\`, `unity-2017\`, `src\External\`) не изменены.

## Out of scope (S2)

- Публикации из Unity и WPF (S3/потребитель), `item*`-события, заполнение rng в солверах,
  spine-хук анимаций (timebox — в S3).

## План (почти завершён): Unity 7 (7000.0.0a10) — URP-миграция после апгрейда

## Goal

Проект апгрейднут на Unity 7000.0.0a10 (альфа), где Built-in RP отключён (рендер чёрный, баннер
«Your project has no render pipeline asset») и Dynamic Batching удалён. Цель: (1) убрать сообщение о
Dynamic Batching; (2) перевести активный `unity\` на URP по официальному гайду
(`docs.unity3d.com/7000.0/.../upgrading-from-birp.html`); (3) заменить fixed-function пассы
Spine-шейдеров на URP-совместимые (иначе герои/монстры не рисуются); (4) привести инструменты к
редактору 7000.0.0a10; (5) зафиксировать в доках. `unity-2017\` и `src\External\` не трогаются.

## Design decisions (user-confirmed)

- Откат на 6.x LTS не делаем — остаёмся на Unity 7 + URP.
- `SkeletonLit` (освещение героев) — порт с освещением отложен; временно unlit (имя и свойства
  шейдера сохраняются), потеря свечения — в KNOWN_ISSUES.
- Шаги в редакторе (Dynamic Batching toggle, установка URP, URP asset, Graphics, конвертер)
  выполняет пользователь; файловую часть (шейдеры, манифест при необходимости, tools, доки) — агент.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] Dynamic Batching → 0: галка в UI серая (настройка помечена legacy в Unity 7) — агент поправил
   `ProjectSettings.asset` при закрытом редакторе (коммит `29e7288`; уходит LogError
   `PlayerSettingsBatchingWarning`).
3. [x] Пользователь: Package Manager → Unity Registry → Universal Render Pipeline → Install
   (`manifest.json`: URP 17.7.0).
4. [x] URP Asset создан пользователем (`Assets\Settings\DarkestDungeonURP[,_Renderer].asset`); Default
   Render Pipeline прописан агентом в `GraphicsSettings.asset` (guid `3c6b23f4...`, коммит `29e7288`).
4b. [x] Агент: удалены 127 устаревших .NET-фасадов (gitignored) из `Assets\Plugins\Internal` —
   дублировали `unity-4.8-api\Facades` Unity 7 и давали `error CS1703`; остались только `Sektor.*`.
5. [x] Пользователь: Window > Rendering > Render Pipeline Converter → Source: Built-in, Target:
   URP (Universal Renderer) → Rendering Settings + Material Reference Converter + Material Shader
   Converter → Initialize and Convert. Fallout закоммичен (коммит A секции URP-диагностики).
6. [x] Агент: порт fixed-function Spine-шейдеров на CG unlit v/f (имена/свойства те же):
   `unity\Assets\Spine\SpineUnity\Shaders\Skeleton.shader`,
   `unity\Assets\Spine\SpineUnity\Shaders\SkeletonLit.shader`.
7. [x] Агент: tools — `knownRoots` 6000.5.8f1 → 7000.0.0a10 в `unity-compile-check.ps1`,
   `unity-build-game.ps1`, `unity-provision-plugins.ps1`, `unity-generate-battle-test-scene.ps1`.
8. [ ] Пользователь: прогон Intro → CampaignSelection → EstateManagement → Dungeon (герои/монстры
   видимы, Death's Door пульс, грейскейл, торч-флейм, UI, карта).
9. [x] Агент: `unity-compile-check.ps1` при закрытом редакторе — зелёный (компиляция +
   script-reference check, уже после правок шага 4 и чистки фасадов).
10. [x] Агент: доки — `UNITY_MIGRATION.md` §6 (миграция 6.5 → 7.0, URP), `ARCHITECTURE.md` (Unity 7),
    `KNOWN_ISSUES.md` (BlurOptimized мёртв под URP; SkeletonLit unlit; фасады 2017 удалены),
    `TESTING.md` (URP-чеклист), `COMPABILITY.md` §4 — все разделы на месте.

## Affected files

- `unity\ProjectSettings\ProjectSettings.asset` (m_DynamicBatching → 0 — файловая правка: галка в UI
  серая).
- `unity\Packages\manifest.json` + `packages-lock.json` (URP, через Package Manager).
- Новые: `unity\Assets\Settings\*.asset` (URP Asset + Renderer), `.meta` обязательны.
- `unity\Assets\Spine\SpineUnity\Shaders\Skeleton.shader`, `SkeletonLit.shader`.
- `tools\unity-*.ps1` (4 файла), `PLAN.md`.
- Доки: `docs\UNITY_MIGRATION.md`, `docs\ARCHITECTURE.md`, `docs\KNOWN_ISSUES.md`,
  `docs\TESTING.md`, `docs\COMPABILITY.md`.

## Acceptance criteria

- Оба сообщения в консоли исчезли; Game view не чёрный (URP asset назначен).
- Герои и монстры видимы в бою, Death's Door пульс и грейскейл-материалы работают.
- `pwsh tools\unity-compile-check.ps1` зелёный на 7000.0.0a10 (редактор закрыт).
- Легаси (`unity-2017\`, `src\External\`) не изменён.

## Out of scope

- Порт освещения SkeletonLit (URP lit) — отдельная задача.
- Замена BlurOptimized (showoff-блюр) URP-аналогом — отдельная задача.
- Мёртвый `CharacterSprite.shader` (Spine/DungeonSprite, 0 ссылок) — не трогаем.

## Backlog (следующие шаги, план ещё не заведён)

- **Спека игровых событий → S2/S3** (`docs\SPEC_GAME_EVENTS.md`, план открыть отдельным срезом):
  S2 — `Core.GameEvents` (GameEvent-иерархия + `IGameEventBus`) + `Clients.EventLog`
  (`JsonEventLogger` → JSONL, manifest.json), регистрация в `.slnx`, `TARGET_LAYOUT`/`ARCHITECTURE`,
  `SPEC_GAME_EVENTS_SCHEMA.md`; S3 — Unity-рекордер (бутстрап как `SceneSnapshotDumper`,
  `persistentDataPath\Replays\run_*`), публикации sceneSnapshot/partyMoved/комната/бой/факел/
  skillUnavailable, эталонные `*.jsonl`. Решения: TFM остаётся netstandard2.0 (доставка в оба
  Unity), только `LangVersion latest` в новых csproj; сериализатор Newtonsoft
  `TypeNameHandling.Auto` (STJ в проекте нет); `Input`/`Rng` обязательны (`null`+причина пока);
  хук фактических spine-имён — timebox; ресурсы клиентов (фоны, иконки, спайн бандитов) —
  экстракцией в gitignored-общую папку под все клиенты (WPF/Avalonia).
- Далее по этой линии: WPF-потребитель JSONL («Воспроизвести захват»), экстракция ресурсов +
  арена hero vs бандиты (`brigand_cutthroat_A`, контент уже грузится), инвентарь/движение-дельты,
  заполнение rng.
- Фикс `tools\unity-compile-check.ps1`: сверка версии падает на этом ПК — установка 2017.4.40f1
  в `E:\ProgramFiles\Unity2017.4.40f1` отдаёт `ProductVersion 2017.4.40.7214086` (без `f1`), а
  `ProjectVersion.txt` ждёт `2017.4.40f1` (`StartsWith` не срабатывает). `unity-2017`
  проверен ручным batch-прогоном — зелёный (компиляция + script-reference check).
- Монстры и остальные герои в карточке дуэли; интеграция анимаций боя (атаки, скиллы).
