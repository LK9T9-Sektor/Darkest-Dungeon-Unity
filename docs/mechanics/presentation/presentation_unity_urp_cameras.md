# presentation_unity_urp_cameras.md — URP: очистка камер и стек («жёлтый экран» Dungeon)

> Домен: `presentation` (активное `unity\`, сцены `Dungeon`, `DungeonMultiplayer`; URP 17.7.0,
> Unity 7000.0.0a10). Статус: **исправлено**. Назначение — зафиксировать реальную причину «жёлтого
> экрана» после перехода на URP и правило настройки нескольких камер рейда, чтобы не переоткрывать
> это заново. Все `file:line`/fileID — от текущих файлов.

## 1. Симптом

В Play-режиме сцены `Dungeon` верхние ~2/3 экрана заливались **плоским жёлтым** (`(255,235,4)`),
закрывая мир (стены, пол, героев); поверх оставался только UI (HUD). В `Intro`/`CampaignSelection`
и в редакторе (Scene-вид) жёлтого не было. Отключение объектов сцены (стена, `RoomView`, `Room`)
жёлтый не убирало.

## 2. Первопричина

Это поведение URP в пути **Render Graph** — `Runtime\UniversalRendererRenderGraph.cs:478-485`:

```csharp
ClearCameraParams GetClearCameraParams(UniversalCameraData cameraData)
{
    bool clearColor = cameraData.renderType == CameraRenderType.Base;   // Base всегда чистит ЦВЕТ
    bool clearDepth = cameraData.renderType == CameraRenderType.Base || cameraData.clearDepth;
    // ...
    Color clearVal = (cameraData.camera.clearFlags == CameraClearFlags.Nothing
                      && cameraData.targetTexture == null) ? Color.yellow : cameraData.backgroundColor;
```

- **Base-камера всегда очищает цветовой буфер.** «Не чистить и лечь поверх» (`clearFlags = Nothing`)
  в URP работает только у камер-оверлеев (`m_CameraType = Overlay`).
- Если у Base-камеры `clearFlags == Nothing` (и нет `targetTexture`), цвет очистки — **`Color.yellow`**,
  намеренный debug-цвет Unity для «неинициализированного» фона. `Color.yellow` = `(1, 0.9216, 0.0157)`
  = наблюдаемый `(255,235,4)`.
- В 2D-рендерере та же логика: `Runtime\2D\Rendergraph\Renderer2DRendergraph.cs:288-289`.
- В non-Render-Graph (Compatibility) пути иначе: `Nothing` = «очистить только глубину»
  (`Runtime\ScriptableRenderer.cs:1130-1172`) — там жёлтого нет.

В сцене `Dungeon` три камеры (в легаси-рейде они и задумывались как слои друг поверх друга):
`DungeonCamera` (Base, clear `Color`), `BlurCamera` (clear `Nothing`) и `UI_Dangeon/Camera` (clear
`Nothing`). Две последние тоже были Base (у `BlurCamera` вообще не было `UniversalAdditionalCameraData`,
у UI-камеры `m_CameraType: 0`) → каждая заливала кадр `Color.yellow`, а UI-камера (depth 1, последняя)
перекрывала мир.

## 3. Как локализовали

- Дампер сцены (`unity\Assets\Scripts\Diagnostics\`) + скриншоты: жёлтый = однородный `(255,235,4)`
  ровно в верхних 2/3; в исходных текстурах, материалах и компонентах жёлтого нет (версии «crunch/
  UV/материал» исключены измерениями).
- Жёлтый есть **только** в `Dungeon`/`DungeonMultiplayer`; в их камерах `m_ClearFlags: 4` (Nothing),
  а в `Intro`/`CampaignSelection` — `Skybox`/`Color`.
- Сопоставление с кодом пакета URP дало точную строку (см. §2).

## 4. Фикс

URP **camera stacking** — привести камеры к схеме «1 Base + N Overlay»:

- `DungeonCamera` — `UniversalAdditionalCameraData` (`m_CameraType: 0`, Base) со стеком
  `m_Cameras: [BlurCamera, UI Camera]`.
- `BlurCamera`, `UI_Dangeon/Camera` — `m_CameraType: 1` (Overlay), `m_ClearDepth: 0` (не чистят
  ни цвет, ни глубину — как `Nothing` в Built-in RP).
- Overlay-камеры не очищают цвет → жёлтый уходит, мир остаётся, UI рисуется поверх.

Файлы/fileID (`Dungeon.unity`; в `DungeonMultiplayer.unity` — те же id):

| Камера | GameObject | Camera (u!20) | `UniversalAdditionalCameraData` |
|---|---|---|---|
| `DungeonCamera` | `&447450556` | `&447450562` | `&447450565` (добавлен) |
| `BlurCamera` | `&1081479256` | `&1081479260` | `&1081479261` (добавлен) |
| UI `Camera` | `&9263985` | `&9263987` | `&9263991` (стал Overlay) |

Скрипт компонента — пакетный guid `a79441f348de89743a2939f4d699eac1`
(`UniversalAdditionalCameraData.cs`).

## 5. Проверка

Прогон `Dungeon` в Play: жёлтого нет, видны мир (стены/пол/герои), UI поверх; пройти коридор и
комнату. `DungeonMultiplayer` проверяется только сетевой игрой.

## 6. Нюансы / подводные камни

- **`clearFlags` больше не определяет «не чистить».** В URP «лечь поверх» задаётся типом камеры
  (`Overlay`), а не `Clear Flags = Nothing`. Base + `Nothing` = жёлтый (Render Graph путь).
- Жёлтый — осознанный debug-цвет Unity (`Color.yellow`), поэтому его **нельзя найти** в текстурах,
  материалах или компонентах сцены; искать там бессмысленно.
- Проявляется **только в сценах с несколькими камерами, где «лишние» имеют `clearFlags = Nothing`**;
  поэтому `Intro`/`CampaignSelection` (одна камера) не пострадали.
- Легаси-описание «3 камеры: clear Depth only» (`presentation_unity_raid_visuals.md` §1) к текущему
  `unity\` не относится: в сценах было `m_ClearFlags: 4` (Nothing).
- Альтернативный обход (не выбран): Project Settings → Graphics → Render Graph **Compatibility Mode** —
  non-RG путь трактует `Nothing` как «clear depth only»; режим deprecated.
- `BlurOptimized` под URP всё равно мёртв (`OnRenderImage` в SRP не вызывается) — `KNOWN_ISSUES.md` §15;
  камеры-слои теперь корректны, но эффекта блюра нет.

## 7. Взаимодействия

- `RaidPartyCamera` (на `DungeonCamera`) управляет FOV/паном — не затронут.
- UI-канвасы `ScreenSpace-Camera` смотрят на UI-камеру (`worldCamera`) — камера осталась, теперь Overlay.
- `BlurCamera` рисует layer 9 (showoff), UI `Camera` — слои UI(5) + MainMenu(10).

## 8. Файлы-источники

- Сцены: `unity\Assets\Scenes\Dungeon.unity`, `unity\Assets\Scenes\DungeonMultiplayer.unity`.
- Пакет URP: `...\com.unity.render-pipelines.universal@e5a036f795be\Runtime\UniversalRendererRenderGraph.cs`,
  `...\Runtime\2D\Rendergraph\Renderer2DRendergraph.cs`, `...\Runtime\ScriptableRenderer.cs`.
- Диагностика: `unity\Assets\Scripts\Diagnostics\` (дампер сцены + скриншоты).
- Смежное: `docs\UNITY_MIGRATION.md` §6, `docs\KNOWN_ISSUES.md` §15,
  `docs\mechanics\presentation\presentation_unity_raid_visuals.md`.
