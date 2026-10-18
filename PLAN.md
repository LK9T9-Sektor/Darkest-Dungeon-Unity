# PLAN.md — планы задач

Активный план — ниже на этой странице. Завершённые планы переносятся в `docs\plans\` (индекс и
статусы — `docs\plans\00_index.md`); при завершении план уезжает в архив целиком, чтобы контекст
не терялся при работе с другого ПК — архив в git, `.opencode\` для планов не используется.

---

# PLAN: Scene snapshot v2 — авто-опись сцены для раскладки

## Goal

Расширить диагностический дампер (`unity\Assets\Scripts\Diagnostics\`) до инструмента, который в
устоявшемся состоянии сцены сам снимает полную опись: экранные координаты каждого `RectTransform`
(пиксели + доли экрана), `CanvasScaler`, тексты, лейаут-компоненты, камеры, `DontDestroyOnLoad`.
На выходе — JSON + компактный layout-лист (CSV), по которому затем (фаза 2, отдельный план)
переделывается раскладка рейдового HUD и BattleTest в Unity, а WPF воссоздаёт похожие позиции.
Легаси-код не трогаем; инструмент живёт в `Scripts\Diagnostics` и гейм-код не использует.

## Design decisions (user-confirmed)

- Триггеры — только авто: `auto-settled` (после загрузки сцены: end of frame + ForceUpdateCanvases
  + кадр, заменяет t0), resize-снимок при смене `Screen.width/height`; t5/t15 и F9 остаются как
  резерв. Семантические хуки — позже, публичный `Capture(tag)` задел на будущее.
- Объём — расширенный: экранные координаты, CanvasScaler/канвасы/камеры, uGUI `Text`,
  лейаут-компоненты (LayoutGroup/ContentSizeFitter/LayoutElement/AspectRatioFitter).
- Артефакты — JSON (большой) + компактный layout-лист по UI-узлам рядом.
- Фаза 2 (раскладка HUD, фикс BattleTest, карта для WPF) — отдельным планом по этим данным.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] DTO + ридер: заголовок `ScreenWidth/Height/Aspect` + флаг DDOL; экстракторы CanvasScaler,
   Text (~80 симв.), LayoutGroup/CSF/LE/AspectRatioFitter, Image.fillMethod, Canvas.pixelPerfect,
   Camera.pixelRect/targetTexture; screen rect (px + доли экрана) в `ReadRectTransform`
   (root-canvas ищется с `includeInactive: true` — иначе неактивные панели молча теряли координаты).
3. [x] `SceneSnapshotLayoutWriter.cs` (+ `.meta`) — CSV рядом с JSON, та же фоновая запись.
4. [x] DDOL: псевдо-сцена (`GetSceneByName("DontDestroyOnLoad")`, fallback — обход
   `Resources.FindObjectsOfTypeAll`), отдельные файлы `DontDestroyOnLoad-<триггер>-<stamp>` на каждое
   capture-событие.
5. [x] Триггеры: `auto-settled` вместо t0, resize-триггер, публичный `Capture(tag)`.
6. [x] README дампера: поля/артефакты/триггеры; убрать ограничение «только активная сцена».
7. [x] Верификация (агент): `unity-compile-check.ps1` зелёный (дважды); билд + прогон игрока:
   на каждую сцену — `auto-settled`/`auto-t5s` (JSON+CSV+PNG), DDOL — отдельные JSON+CSV
   (1,4–2 МБ, 152 узла), шапка `Screen*` корректна (1920×1080/1.7778), screen rect сходится
   (проверено вручную: 488.5/1920 = 0.2544); на активных сценах screen rect у 100% UI-узлов,
   у неактивных — после правки includeInactive (перепроверено: DDOL 144/144 UI-строк с rect).
8. [ ] Пользователь: в редакторе открыть `Dungeon`, Game-вью 16:9 → 21:9 → 4:3 (без F9) —
   ожидаются `Dungeon-auto-settled-*` и `resize-<WxH>-*` файлы; в CSV строка
   `UI_RaidInterface/RaidPanel` при 1080p ≈ (0,720)–(1920,1080) px и (0,0.667)–(1,1).
9. [x] Коммит на `core/unity7-urp`: `19ea5aa` (7 файлов, +645/−37, hook зелёный); push — по запросу.

## Cleanup (по завершении плана)

- `unity\Build\` — вывод сборки (gitignored), нужен для быстрых прогонов; удаляем по желанию.
- `%LOCALAPPDATA%Low\RedHooks\Darkest Dungeon\SceneDumps\` — десятки МБ дампов; старые чистить.
- Логи сборки/чеков: `%TEMP%\dd-build*.log`, `%TEMP%\unity-build-*.log`, `%TEMP%\unity-compile-*.log` — временные.

## Affected files

- `unity\Assets\Scripts\Diagnostics\SceneSnapshot.cs`, `SceneNodeSnapshot.cs` (при необходимости),
  `SceneSnapshotPropertyReader.cs`, `SceneSnapshotDumper.cs`, новый `SceneSnapshotLayoutWriter.cs`.
- `unity\Assets\Scripts\Diagnostics\README.md`.
- `PLAN.md`.

## Acceptance criteria

- В SceneDumps на каждую загрузку сцены: JSON + CSV + PNG с тегом `auto-settled`; отдельные файлы
  `DontDestroyOnLoad-<триггер>-*` с содержимым `DontDestroyOnLoad` (менеджеры/хосты из стартовых сцен).
- CSV содержит `RaidPanel` со screen rect в пикселях и долях; `CanvasScaler` присутствует в JSON.
- Смена разрешения без F9 → файл `resize-<WxH>`; легаси-сцена не меняется; компиляционный чек зелёный.

## Out of scope (фаза 2, отдельный план)

- Адаптивность рейдового HUD (нижняя панель/верх/центр), рефактор в переиспользуемое.
- Фикс BattleTest (позиционирование юнитов с первого кадра, масштаб ~30–35% высоты кадра,
  аспект-безопасные места).
- Карта долей экрана → XAML для WPF.

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
