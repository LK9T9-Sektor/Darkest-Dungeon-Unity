> Закрыт: 2026-10-18 (ветка `core/unity7-urp`). Перенесён из `PLAN.md` при вводе архива планов;
> индекс закрытых планов — `docs\plans\00_index.md`.

# PLAN: URP-диагностика — JSON-дамп сцены + коммит fallout конвертера

## Что сделано

- Закоммичен fallout URP-конвертера: 61 файл (+101 263 / −84 291) + `DungeonSettings.lighting` + `.meta` (`c4f1cc6`); попутно закрыты мёртвый `TouchInputModule` в `Dungeon.unity` (`6d6725d`) и отсутствие индексации пакетных meta `com.unity.ugui`/URP в script-reference check (`5da0f5f`).
- Написан дампер `unity\Assets\Scripts\Diagnostics\` (`SceneSnapshotDumper`, `SceneSnapshot`, `SceneNodeSnapshot`, `ComponentSnapshot`, `ComponentPropertySnapshot`, `SceneSnapshotPropertyReader` + `README.md`) — JSON-иерархия сцены, авто-триггеры 0/5/15 c + F9, фоновая запись; учтено удаление `CanvasRenderer.alpha` в Unity 7.
- Установлена причина «жёлтой стены»: Base-камеры с `clearFlags == Nothing` получают очистку `Color.yellow` (1, 0.9216, 0.0157) в URP Render Graph (`UniversalRendererRenderGraph.cs:478-485`). Фикс в YAML при закрытом редакторе: `BlurCamera` и `UI_Dangeon/Camera` → Overlay (`m_CameraType: 1`, `m_ClearDepth: 0`), `DungeonCamera` — Base со стеком `[BlurCamera, UI Camera]` в обеих сценах; прогон пользователем: жёлтое исчезло.
- Попутно: `CoridorWall`/`CoridorWall 1` на unlit `Sprites/Default`, снесён мёртвый `GUILayer` (`!u!92`) из 5 сцен и 2 префабов, null-guard'ы дампера.
- Доки: новый `docs\mechanics\presentation\presentation_unity_urp_cameras.md`; ссылки в `INDEX.md`, `mechanics\00_index.md`, `UNITY_MIGRATION.md` §6, `KNOWN_ISSUES.md` §15, `CHANGELOG.md`, `TESTING.md`; компиляция и script-reference check зелёные.

## Осталось / за пользователем

- Дампер `unity\Assets\Scripts\Diagnostics\` ещё нужен (временный слой, удаляется вместе с папкой).
- Cleanup по желанию: дампы в `%LOCALAPPDATA%Low\RedHooks\Darkest Dungeon\SceneDumps\`, временные скрипты в `%TEMP%\opencode\`.

---

_Ниже — исходный план без изменений._

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
