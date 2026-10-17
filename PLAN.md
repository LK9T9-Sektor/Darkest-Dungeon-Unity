# PLAN: URP-диагностика — JSON-дамп сцены + коммит fallout конвертера

## Goal

После шага 5 URP-миграции (Render Pipeline Converter, выполнен пользователем) в рабочем дереве
скопился большой fallout: 61 изменённый файл (+101 263 / −84 291) — конвертированные материалы,
пересериализованные префабы/сцена, ProjectSettings. Его нужно закоммитить в `core/unity7-urp` как
baseline. Затем — установить причину «жёлтой» стены комнаты
(`Dungeon→DungeonView→RoomView→RoomWall→Wall`) по данным, а не догадкам: отдельный диагностический
класс снимает полную иерархию сцены с характеристиками компонентов в JSON (на старте и с задержкой,
без блокирования; F9 — снимок по требованию в момент визуального бага).

## Design decisions (user-confirmed)

- Дампер — отдельный класс-тул в `unity/Assets/Scripts/Diagnostics/` (только активное дерево;
  `unity-2017\` не трогаем). Авто-старт `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, снимки на
  задержках 0/5/15 c + F9. JSON через `JsonUtility` (Newtonsoft в манифесте нет — зависимость не
  добавляем), сбор DTO на main thread корутиной (yield каждые N узлов), запись файла — в фоне.
- `.opencode/` не коммитим (остаётся неотслеживаемым).
- Fix стеновых материалов (`CoridorWall.mat`/`CoridorWall 1.mat`: конвертер сменил built-in шейдер
  `fileID 10800` на URP/SimpleLit — измеренный факт, главный подозреваемый) — только после
  подтверждения данными снимка, отдельным коммитом.

## Steps / status

1. [x] PLAN.md зафиксирован (эта секция).
2. [x] Коммит A: fallout конвертера (61 файл + `DungeonSettings.lighting` + `.meta`; `.opencode/` мимо) —
   `c4f1cc6`. Побочно вскрылись и закрыты два дефекта после пересериализации: мёртвый
   `TouchInputModule` (missing script в `Dungeon.unity`) — `6d6725d`; script-reference check не
   индексировал пакетные meta (`com.unity.ugui`/URP гуиды) — `5da0f5f` (тул + AGENTS.md).
3. [x] Дампер: `SceneSnapshotDumper` + DTO + README (+ `.meta`), строка в `docs\INDEX.md` —
   написано: `unity\Assets\Scripts\Diagnostics\{SceneSnapshotDumper, SceneSnapshot,
   SceneNodeSnapshot, ComponentSnapshot, ComponentPropertySnapshot, SceneSnapshotPropertyReader}.cs`
   + `README.md` (все с `.meta`).
4. [x] Коммит B: дампер (`unity-compile-check.ps1` при закрытом редакторе — зелёный; первая
   попытка поймала удаление `CanvasRenderer.alpha` в Unity 7 → `GetAlpha()`).
5. [x] Прогон (пользователь, редактор → Play), F9 в момент «жёлтой стены»: снят manual-дамп
   `Dungeon-manual-20261018-000838-926.json` (SceneDumps). Факты: узел
   `…/DungeonView/RoomView/RoomWall/Wall` — спрайт `weald.room_wall.effigy_0` и alpha 1 корректны,
   но material `CoridorWall` = `Universal Render Pipeline/Simple Lit` (следствие конвертера).
   Все 27 UI Image с Simple Lit в сцене — `CoridorWall` (23) + `CoridorWall 1` (4). `Sprites/Default`
   (built-in fileID 10753) — измеренно проверенный canvas-шейдер проекта (`PipMaterial` стресс-пипов
   рендерится им). Причина подтверждена данными, не догадкой.
6. [x] Коммит C: `CoridorWall`/`CoridorWall 1` — unlit `Sprites/Default` (база: до-конвертерный файл
   из `c4f1cc6^`, заменена одна строка шейдера). Доки в том же коммите: `TESTING.md` (URP-чеклист —
   стены и GUI Layer), `CHANGELOG.md`.
7. [x] Коммит D (побочное, как D1/D2): снесён мёртвый `GUILayer` (`!u!92` — нет в Unity 7) из сцен
   (Intro, LoadingScreen, EstateManagement, DungeonMultiplayer ×2, CampaignSelection) и префабов
   (`Main Camera`, `UIDungeonCamera`) — снимает предупреждение «Component GUI Layer … is no longer
   available» (стек `UnityEditor.GUIView.ProcessEvent`, показан пользователем после прогона).
8. [x] Повторный прогон пользователем (00:37): предупреждение GUI Layer ушло, но «жёлтое» осталось —
   то есть материал стен не был причиной (см. шаг 10).
9. [x] Пост-прогон фиксы по логам: (a) дампер переживает уничтожение объектов во время снимка
   (`BuildNodeRoutine`/`DumpRoutine` — null-guard'ы; `MissingReferenceException` на переходе
   комната↔коридор больше не роняет авто-снимок); (b) из `LoadingScreen.unity`, `Main Camera.prefab`,
   `UIDungeonCamera.prefab` убраны оставшиеся ссылки старого формата `- 92: {fileID: …}`
   («Broken text PPtr» — хвост от снятия `GUILayer`).
 10. [x] Сверка lighting/камер (по запросу): `DungeonSettings.lighting` — placeholder конвертера
    (`m_EnableBakedLightmaps: 0`); ambient/fog, камеры (depth 0 / 0.3 / 1; masks −1/512/1056) и
    Light-блоки (point 7/150, directional 4.65) — идентичны `unity-2017`; `m_RenderPostProcessing: 0`;
    color space — Gamma в обоих деревьях. Повторный прогон в комнате (00:37: manual t=2.17 c + auto t0):
    стены уже на `Sprites/Default`, герои активны и геометрически **ближе** к камере, чем стена
    (z −210.7 vs −200; камера −300) — перекрыть героев стена не может, но «жёлтое» сохранилось →
    материал/свет/камеры исключены. `ScreenFader` исключён (чёрный RawImage без текстуры; в сцене
    Dungeon отсутствует). Сканы дампов (все UI Image, не-Default материалы, узлы fade/overlay/light,
    крупные размеры) не находят жёлтого объекта ни в Dungeon, ни в Intro/CampaignSelection.
 11. [x] Пиксельная диагностика (прогон 00:54–00:55, 6 дампов + PNG). Факты скриншота (757×426):
     жёлтый = однородный (255,235,4) ровно по прямоугольнику `Wall` (0..284 px; ниже — тёмная полоса
     1..17, владелец не установлен). В исходных PNG жёлтого нет (`weald.room_wall.effigy_0` —
     1920×720, 24bpp, MD5 8B87A1C32CA448CA840682B104224935 идентичен в обоих деревьях, максимум
     (255,120,67) — оранжевый закат; effigy_1/corridor — тоже) → цвет НЕ тексель текстуры, теория
     «UV схлопнулись в один тексель» убита. Камеры/clear, цвета Image, `CoridorWall` — чисты. В сцене
     guid спрайта стены (`a5f81c4f…`) в метах отсутствует (висячая ссылка) — в Edit mode стена без
     спрайта, жёлтый только в Play (пользователь) → корреляция с рантайм-назначением (`RaidRoom.cs:117`
     из `Resources/Dungeons/<dungeon>`). Импорт-диф подозрителен: effigy_0 crunched=1/textureFormat
     −5/quality −1 vs рабочий rank_target_enemy (0/−1/50); в папке Dungeons 153 из 228 мет crunched=1.
     Ложная «ошибка» в логе опознана: `Failed to capture screen shot` 00:54:57 — 4 быстрых F9, один
     PNG не успел записаться (безвредно).
 12. [x] Прогон с локализацией (01:22–01:23, авто-дампы Dungeon t0/t5/t15 + PNG; пользователь:
     отключение `Wall`/`Room`/`RoomView` жёлтое НЕ убирает, превью Source Image и миниатюры PNG —
     нормальный арт). Факты: жёлтый = идеально однородный (255,235,4) в верхних 2/3 экрана (0..283 px;
     8847/8850 сэмплов) = ровно Unity `Color.yellow` (1, 0.9216, 0.0157). Wall в рантайме: sprite
     `weald.room_wall.effigy_0`, mat `CoridorWall` (Sprites/Default), color white, текстура 1920×720
     `DXT1Crunched` — как у рабочих UI-панелей (`PanelBanner`/`PanelMap`: UI/Default + DXT1Crunched,
     рисуются нормально → crunch не виноват). В дампе НЕТ ни одного компонента/материала/шейдера с
     жёлтым цветом; шейдеров всего 3 (Spine/Skeleton Lit, Sprites/Default, UI/Default); слоёв только
     Default/UI (слои 9/10 для Blur/UI-камер пусты); активных MeshRenderer — 3 (факелы); backdrop
     (`starfield`) неактивен; камеры все full-screen (rect 0,0,1,1), clear не жёлтый. Расширенный
     дампер работает (`renderMaterial` читается). Стена/материал/текстура — исключены как источник.
 13. [x] ПРИЧИНА НАЙДЕНА (рендеринг URP, подтверждено кодом пакета): `UniversalRendererRenderGraph.cs`
     :478-485 — Render Graph-путь чистит ЦВЕТ у каждой **Base**-камеры, а при
     `clearFlags == Nothing` (и `targetTexture == null`) цвет очистки = **`Color.yellow`**
     (= (1, 0.9216, 0.0157) = измеренные (255,235,4)). В `Dungeon.unity` и `DungeonMultiplayer.unity`
     по 2 Base-камеры с clear=Nothing — `…/DungeonCamera/BlurCamera` и `DungeonSceneManager/
     UI_Dangeon/Camera`; обе затирают кадр жёлтым, UI-камера (depth 1, последняя) закрывает мир. В
     `Intro`/`CampaignSelection` камеры clear=Skybox/Color — потому жёлтого нет (0 сэмплов; в Dungeon
     12724/20330 ≈ верхние 2/3, низ добит тёмными панелями HUD). Стена/материал/текстуры/crunch —
     окончательно исключены.
 14. [x] Фикс внесён в YAML при закрытом редакторе (`Dungeon.unity`, `DungeonMultiplayer.unity`):
     `BlurCamera` и `UI_Dangeon/Camera` → `UniversalAdditionalCameraData` с `m_CameraType: 1`
     (Overlay) + `m_ClearDepth: 0`; `DungeonCamera` получила `UniversalAdditionalCameraData`
     (`m_CameraType: 0`, Base) со стеком `m_Cameras: [BlurCamera, UI Camera]`. Итог по обеим сценам —
     ровно 1 Base + 2 Overlay. **Прогон пользователем: жёлтое исчезло.** Новый док
     `docs\mechanics\presentation\presentation_unity_urp_cameras.md`; ссылки — `INDEX.md`,
     `mechanics\00_index.md`, `UNITY_MIGRATION.md` §6, `KNOWN_ISSUES.md` §15, `CHANGELOG.md`,
     `TESTING.md` §«URP и рендеринг».
 15. [x] Коммит сцен + доков на `core/unity7-urp`.

## Cleanup (временные артефакты, можно удалять)

- Дампы диагностики: `%LOCALAPPDATA%Low\RedHooks\Darkest Dungeon\SceneDumps\*.json|*.png` (десятки
  файлов по ~16 МБ) — больше не нужны.
- Временные скрипты: `C:\Users\lk9t9\AppData\Local\Temp\opencode\` (`scan-yellow2.ps1`,
  `dump-positions.ps1`, `dump-type-paths.ps1`, `scan2_*.txt`).
- Диагностический дампер `unity\Assets\Scripts\Diagnostics\` — **оставить** (пока нужен; README
  описывает его как временный слой, удаляется вместе с папкой, когда перестанет быть нужен).

## Affected files

- `PLAN.md`; `unity\Assets\Scenes\DungeonSettings.lighting` (+`.meta`).
- `unity\Assets\Scripts\Diagnostics\*.cs` (+`.meta`), `README.md`.
- Фикс (коммит C): `unity\Assets\Materials\CoridorWall.mat`, `CoridorWall 1.mat`; доки —
  `docs\TESTING.md` (URP-чеклист), `docs\CHANGELOG.md`.
- GUI Layer (коммит D): `unity\Assets\Scenes\{Intro,LoadingScreen,EstateManagement,DungeonMultiplayer,CampaignSelection}.unity`,
  `unity\Assets\Prefabs\{Main Camera,UIDungeonCamera}.prefab`.
- `docs\INDEX.md` (строка про дампер; уже в коммите B).

## Acceptance criteria

- Коммиты A/B в `core/unity7-urp`; script-reference check (hook) и компиляция зелёные.
- JSON-снимки создаются на старте и с задержкой, путь логируется в консоль; F9 снимает снимок.
- Причина «жёлтой» стены установлена по данным снимка (sprite/shader/alpha/active-цепочка), фикс
  подтверждён повторным прогоном.

---

# PLAN: Unity 7 (7000.0.0a10) — URP-миграция после апгрейда

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
10. [ ] Агент: доки — `UNITY_MIGRATION.md` §6 (миграция 6.5 → 7.0, URP), `ARCHITECTURE.md` (Unity 7),
   `KNOWN_ISSUES.md` (BlurOptimized мёртв под URP; SkeletonLit unlit; фасады 2017 удалены),
   `TESTING.md` (URP-чеклист), `COMPABILITY.md`.

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

---

# PLAN: BattleTest — тест-сцена боёв как первый Unity-потребитель ядра (прототип Фазы 6)

---

# PLAN: WPF — монстры/боссы размером 1–4 ранга: size-aware ранги в ядре + PvE-режим

## Goal

В Unity монстры и боссы занимают 1–4 позиции (`display: .size N`): size-2 монстр на ранге 1
занимает ранги 1–2, следующий юнит стартует с ранга 3; карточка рисуется шириной `×size`; ёмкость
формации = 4 слота (`4 − Σ size`). В WPF это сейчас не работает: (1) ядро парсит `size` и использует
его в таргетинге/запуске/AI, но **назначает ранги size-agnostic** (`i+1`); (2) WPF-бой — только
герои-vs-герои (`StartDuel`), монстры в бою не участвуют; (3) карточки фиксированной ширины, без
`Size`. Делаем: size-aware ранги в ядре (паритет Unity), рендер размера в WPF-виде, PvE-режим
(`StartFight` герои-vs-монстры) с водителем `PveBattleViewModel` (аналог Unity `CoreBattleDriver`).

## Design decisions (user-confirmed)

- **PvE-водитель** — отдельный `PveBattleViewModel` по образцу Unity `CoreBattleDriver`,
  переиспользует `DuelBattleView` для отображения. Герои игрока ходят через `ApplyRemoteSkill`
  (в `StartFight` герои — «remote»), монстры — `Solver.UseMonsterBrain` (локальная сторона).
- **Size-aware ранги в ядре** — да, в этом же плане (`FormationParty`, `TurnMover`, `SurpriseResolver`,
  `DuelBattleEvents.MoveUnit` пересчитывают ранги по кумулятивной сумме `Size`, как Unity).
- **Ёмкость формации** — 4 слота; слоты монстров в лобби ограничены `4 − Σ size`.
- Легаси Unity не правится (read-only); `src\External\` не трогается.

## Steps / status

### M1 — Core: size-aware назначение рангов (паритет Unity)

1. [x] `FormationParty.AddUnit` (core) — ранг = `последний.Rank + последний.Size` (или 1, если пусто);
   `RemoveUnit` — кумулятивный пересчёт по `Size`.
2. [x] `TurnMover.TryMove`, `SurpriseResolver.ShuffleParty`, `DuelBattleEvents.MoveUnit` — тот же
   кумулятивный пересчёт рангов (герои size=1 → ранги 1..4 не меняются).
3. [x] Тесты: формация size-2 + size-1 монстров → ранги 1,3 (не 1,2); таргетинг/запуск по рангам
   учитывает занимаемый диапазон; `dotnet test` зелёный.

### M2 — WPF вид: рендер размера

1. [x] `DuelUnitViewModel.Size` (из `unit.Size`); `DuelUnitCardView` ширина = `185 × Size` (конвертер
   или свойство); ранг-бейдж показывает фронтовый ранг.
2. [x] `DuelBattleView` колонки уже `MinWidth=820` (4×201px) — широкая карточка занимает N слотов,
   StackPanel растягивает; проверить вёрстку с size-2/3.

### M3 — WPF PvE-режим (`StartFight`, как Unity BattleTest)

1. [x] Кнопка **PvE** в `MainMenuView` → `PveLobbyViewModel`/`PveLobbyView`: 4 слота героев
   (переиспользовать `HeroSlotViewModel`) + слоты монстров из `DuelContent` (цикл по каталогу,
   суммарный size ≤ 4, seed).
2. [x] Старт: `DuelController.StartFight(heroSpecs, monsterSpecs, seed)`; `PveBattleViewModel` —
   водитель: поллинг/диффинг как `DuelBattleViewModel`, герои игрока → `ApplyRemoteSkill`, монстры →
   `UseMonsterBrain`, ввод через переиспользуемый `DuelBattleView`.
3. [x] Регистрация VM↔View в `MainWindow.xaml` (+ интерфейс `IDuelBattleViewData` для переиспользования
   `DuelBattleView` без правки `DuelBattleViewModel`). Тесты `PveBattleTests` (3) зелёные.

### M4 — Документация (в тех же коммитах)

1. [x] `docs\mechanics\combat\16_formation_size.md` (size-ранги: кумулятивный пересчёт, занимаемый
   диапазон, ёмкость 4, рендер); `BATTLE_PARITY.md` — строка size (2.6 + резюме).
2. [x] `TESTING.md` (PvE-чеклист + size-рендер + историческая строка), `CHANGELOG.md`,
   `00_index.md` (16), `INDEX.md`, `presentation_wpf.md` (gotcha).

## Affected files

- `src\Core\Sektor.DarkestDungeon.Core.Combat\Raid\Party\FormationParty.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\Mechanics\TurnMover.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\Mechanics\SurpriseResolver.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\DuelBattleEvents.cs`.
- `src\Wpf\...\ViewModels\DuelUnitViewModel.cs`, `Views\DuelUnitCardView.xaml`,
  `ViewModels\PveLobbyViewModel.cs` (новый), `Views\PveLobbyView.xaml` (новый),
  `ViewModels\PveBattleViewModel.cs` (новый), `Views\MainMenuView.xaml`, `MainWindow.xaml`.
- Тесты: `tests\Core\...` (size-ранги).
- Доки: `docs\BATTLE_PARITY.md`, `docs\TESTING.md`, `docs\CHANGELOG.md`,
  `docs\mechanics\combat\13_turn_order.md` (или новая), `docs\mechanics\00_index.md`.

## Acceptance criteria

- `dotnet test` зелёный (новые size-ранги тесты + существующие sweep/дуэль).
- `dotnet build` WPF зелёный.
- PvE-бой: герои игрока ходят, монстры (в т.ч. size 2–4) ходят AI, size-карточки занимают N слотов,
  ранги 1,3,… без перекрытия; команды не дрейфуют (прошлый фикс не сломан).
- Легаси Unity и `src\External\` не изменены.

## Out of scope

- Полный cutover игры (кампания/город/инвентарь в WPF) — отдельные фазы.
- `.summon/.control/.capture` и `GetMonsterSize`/`AvailableSummonSpace`-стабы — не обязательны для
  рендера размера (если не мешают PvE-свип-тестам, не трогаем; иначе — минимальный фикс).
- Мультиплеер PvE (герои игрока в сети) — не в этом плане.

---

# PLAN: WPF-дуэль — привязка формаций к своим половинам (умершие не двигают команды)

## Goal

Исправить снос поля при смерти юнитов в WPF-бою (`DuelBattleView.xaml`): когда персонаж умирает,
коллекция сжимается, `Auto`-колонка схлопывается, сетка перецентрируется и правая команда съезжает
влево. Требуется: команды привязаны к своим половинам, фронт (ранг 1) у центра, центр разделяет
их на две — по 4 позиции слева и справа. Решение пользователя: **reflow к центру** (выжившие
сдвигаются вперёд, как в ядре `RemoveUnit`), а не резерв пустых слотов.

## Steps / status

1. [x] `DuelBattleView.xaml`: колонки `Auto` → слот-пол: герои `MinWidth≈820` (4×201px+margin),
   центр `Width≈120`, монстры `MinWidth≈820`.
2. [x] Пере-якорить стеки: герои `HorizontalAlignment="Right"` (колонка 0), монстры
   `HorizontalAlignment="Left"` (колонка 2).
3. [x] `TargetLayer` Canvas → `Grid.ColumnSpan="3"` (математика стрелок в координатах Viewbox
   остаётся корректной).
4. [x] Доки в том же коммите: `docs\mechanics\presentation\presentation_wpf.md` (gotcha),
   `docs\CHANGELOG.md`, `docs\TESTING.md` (дуэльный чеклист).
5. [x] `dotnet build` WPF-проекта зелёный. Ручной прогон боя (герои/монстры умирают — команды
   не дрейфуют) — **на пользователе** (см. `TESTING.md` § WPF-дуэль).

## Affected files

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml`.
- Доки: `docs\mechanics\presentation\presentation_wpf.md`, `docs\CHANGELOG.md`, `docs\TESTING.md`.

## Acceptance criteria

- Умершие юниты уходят, выжившие на своей половине сдвигаются к центру (фронт у центра).
- Правая команда не смещается влево при смерти героев; центр всегда разделяет команды.
- `TargetLayer` стрелки/бейдж не ломаются (ColumnSpan учтён).
- `dotnet build` зелёный; легаси не тронуто.

---

# PLAN (прошлый): BattleTest

## Goal

Новая сцена `BattleTest` в обоих деревьях (`unity\` активное, `unity-2017\` легаси): гибкий
настраиваемый бой (герои/монстры по слотам, скиллы/квирки/тринкеты, контроль сторон, seed, torch)
на **чистом ядре боя** (`Core.Combat`/`Core.Duel`) с полной детализацией визуала и **нулевой
связанностью** — Unity = тонкий вид-слой. Это первый реальный потребитель вынесенного ядра боя в
Unity = прототип Фазы 6 (`EXTRACTION_PLAN.md`). Легаси-движок и игра не трогаются (read-only).

## Design decisions (user-confirmed)

- **Движок:** `DuelController` напрямую (паттерн WPF), НЕ `FightSession` (у `FightSession` только
  skill+target и инвертированный local/remote). `StartFight` (heroes-vs-monsters) / `StartDuel`+`isHost`
  (hero-vs-hero). AI: монстры — `Solver.UseMonsterBrain`, AI-герои — `DuelAi`.
- **Вид — путь 2 (чистый новый слой):** переиспользуем только активы: Spine-префабы юнитов
  (`Resources/Prefabs/Heroes|Monsters`), спрайты/портреты, `UnitAnimator`, `AnimatedEffect`,
  `RaidPartyCamera` (с фиксом 1 ссылки на `RaidSceneManager.RoomView...`), чистые виджеты
  (`RoundIndicator`, `RaidAnnouncment`, `Backdrop`, `RankPlaceholders`, `HealthBar`,
  `StressOverlayPanel`, `TrayPanel`, `PopupDialog`). **НЕ переиспользуем:** `RaidSceneManager`,
  `BattleGround`, `PartyFormationManager`, `RaidPanel`-семейство, `RaidEvents` (слой C).
- **Событий в ядре нет** → вид поллит/диффит состояние после каждого действия: `CurrentUnit`
  → ход, `RoundNumber` → раунд, `IsDead` → смерть, `Rank`/`Party.Units` → движение,
  `Solver.SkillResult.SkillEntries` → урон/крит/хил/мисс, `Events.PopupShown` → статусы,
  `Events.Log` → лог. Паттерн — `DuelBattleViewModel`.
- **Сцену собирает editor-тула** (`Assets\Editor\BattleTestSceneBuilder.cs`, batch `-executeMethod`)
  в обоих деревьях; компоновка камеры/UI — чистая, без dungeon/raid-блота.
- Оба дерева; легаси не правим; новые `.cs` — с `.meta`; доки — в том же коммите.
- Ветка: `core/battle-test-scene` (создана).

## Steps / status

### M1 — Core PvE-сверка (ядро, тесты)

1. [x] `tests\Clients\Sektor.DarkestDungeon.Clients.Content.Tests\FightPveSweepTests.cs` —
   hero-vs-monster sweep по всем монстрам (225 из 230; 5 босс-пропов исключены с причиной:
   cauldron_empty_* — captor-сосуд хагги `prot 1`/initiative 0, ancestor_nebula/small_D —
   стадии босса), детерминизм по сиду, чистое завершение, без исключений.
2. [x] Закрыты в ядре дыры, всплывшие в sweep: **NRE в `DiseaseEffect.ApplyQueued`**
   (`AddRandomDisease()` == null); конкретные id `.disease` (`the_worries`, `rabies`) теперь
   парсятся и резолвятся (`IBattleContext.GetQuirk` → `Hero.AddQuirk(IQuirk)`). Рандомный пул
   болезней — стаб (документировано). `.summon/.control/.capture` — осознанно не парсятся
   (не всплыли); idle-DoT ×1.5 и корпус-подстановка `.kill` — остаются задокументированными
   разрывами (sweep их не воспроизводит).
3. [x] `dotnet test` зелёный (все 10 тест-проектов). `BATTLE_PARITY.md` + `00_index.md` +
   `docs\mechanics\combat\15_disease.md` обновлены в коммите.

### M2 — Новый Unity-вид-слой (оба дерева, `Assets\Scripts\Testing\BattleTest\`)

1. [x] Конфиг: `BattleTestConfig` / `BattleTestSideSpec` / `BattleTestSlotSpec` (+ `.meta`).
2. [x] `CoreBattleDriver` (MonoBehaviour): владеет `DuelController`, сборка из конфига
   (`StartFight` heroes-vs-monsters / `StartDuel` hero-vs-hero), маппинг `ICombatUnit`↔вид,
   поллинг/диффинг, гейт ввода по `IsPlayerControlled` + `IsLocalTurn`, AI-роутинг
   (`UseMonsterBrain`/`DuelAi`), пейсинг, сид, торч.
3. [x] `BattleEventsAdapter` — подписчик `DuelBattleEvents.PopupShown` → попап-слой (паттерн WPF;
   отдельная реализация `IBattleEvents` не нужна — ядро уже шлёт события).
4. [x] Вид Stage-1: `BattleUnitView` (префаб + HP/stress бары + выделение + смерть),
   `BattleFormationView` (по `FormationDisplayOrder`), `BattleHud` (раунд/аннонс/скиллы/цели/лог/
   победитель), `BattlePopupLayer` (урон/статусы), `BattleTestConfigPanel` (2×4 слота, режим,
   seed, torch, FIGHT). Stage-2 (анимации скиллов, слайдинг, гало, move-хендлинг) — позже.
5. [x] Инфраструктура: вместо `WorldToScreenBridge`/`RulesSource` — новый чистый код без проекции
   (бары/попапы в мире, UI в ScreenSpaceOverlay); FMOD-стаб не нужен (вид не вызывает звук).
6. [x] Оба дерева компилируются: `unity-compile-check.ps1` (6000 и 2017.4) зелёные.
   Примечание: legacy-твины (`CombatSkill`, `SkillResult`, `Team`, …) в глобальном неймспейсе
   перекрывают core-типы из `using` — конфликт решён полной квалификацией.

### M3 — Editor-тула + сцена (оба дерева, `Assets\Editor\`)

1. [x] `BattleTestSceneBuilder.cs` (MenuItem + batch `Generate`): собирает `BattleTest.unity` —
   чистая ортокамера, `Battlefield` (WorldSpace-канвас + 2 формации), `BattleHud`, драйвер +
   конфиг-панель, без EventSystem в сцене (создаётся рантаймом `RuntimeUiFactory.EnsureEventSystem`,
   чтобы не ссылаться на engine-GUID'ы). Без `DarkestDungeonManager.prefab` (контент грузит
   `FightContentLoader`, локализация не нужна).
2. [x] `tools\unity-generate-battle-test-scene.ps1` (batch в обоих проектах).
3. [x] Сцены сгенерированы в обоих деревьях и закоммичены (+ `.meta`). Script-reference check
   обоих деревьев зелёный.

### M4 — Интеграция и верификация

1. [x] `unity-compile-check.ps1` (оба дерева) — компиляция + script-reference check.
2. [ ] Ручной чеклист `TESTING.md`: конфиг → запуск → контроль игроком (skill/target/pass/move)
   → AI → попапы/анимации → победа → сид-детерминизм. **(требует открытия сцены в редакторе —
   автоматически не проверяется; Stage-1 вид ни разу не запускался в Play Mode — вероятны
   runtime-нюансы компоновки.)**
3. [x] Кросс-деревные отличия (Spine/uGUI 2017.4 vs 6000) — скомпилировано в обоих; вёрстка
   проверяется при ручном прогоне.

### M5 — Документация (в тех же коммитах)

1. [x] `docs\mechanics\presentation\presentation_unity_battle_view.md` — порядок срабатывания +
   gotchas (событий нет → poll/diff; `StartFight` инвертирует local/remote; legacy-твины в глобальном
   неймспейсе).
2. [x] `TESTING.md` (раздел), `INDEX.md`, `docs\mechanics\00_index.md` (уже — `15_disease.md`),
   `CHANGELOG.md`.
3. [x] `docs\mechanics\presentation\presentation_unity_battle_rig.md` — продакшн-риг (дизайн-решение),
   ссылки из `INDEX.md`.

## Affected files

- `src\Core\Sektor.DarkestDungeon.Core.Combat\...`, `src\Core\Sektor.DarkestDungeon.Core.Duel\...`
  (только M1-фиксы).
- `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\FightPveSweepTests.cs`.
- `unity\Assets\Scripts\Testing\BattleTest\*.cs` (+ `.meta`) — 15–20 файлов.
- `unity-2017\Assets\Scripts\Testing\BattleTest\*.cs` (+ `.meta`) — зеркально.
- `unity\Assets\Editor\BattleTestSceneBuilder.cs`, `unity-2017\Assets\Editor\...` (+ `.meta`).
- `unity\{,-2017}\Assets\Scenes\BattleTest.unity` (+ `.meta`).
- `tools\unity-generate-battle-test-scene.ps1`.
- Доки: `docs\BATTLE_PARITY.md`, `docs\EXTRACTION_STATUS.md`, `docs\TESTING.md`,
  `docs\INDEX.md`, `docs\mechanics\00_index.md`, `docs\mechanics\presentation\presentation_unity_battle_view.md`,
  `docs\CHANGELOG.md`.

## Acceptance criteria

- `dotnet test` зелёный (включая новые PvE-sweep тесты).
- `unity-compile-check.ps1` зелёный в обоих деревьях.
- Ручной чеклист боя проходит (TESTING.md): конфиг → бой → контроль игроком → AI → попапы/анимации
  → победа → сид-детерминизм.
- Легаси-файлы не изменены (только чтение).

## Out of scope

- Полный cutover игры (Campaign/Networking/остальное) — позже, отдельные фазы.
- «Игрок управляет настоящими монстрами» — требует расширения ядра; сначала Player-heroes-vs-AI,
  AI-vs-AI, hotseat hero-vs-hero.
- `Difficulty` — если в ядре нет уровней монстров, маппим на resolve-уровень героев или откладываем.