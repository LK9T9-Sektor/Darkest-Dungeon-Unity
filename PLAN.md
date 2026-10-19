# PLAN.md — планы задач

Три состояния плана, все — в git:

- **активный** — на этой странице: цель, шаги, статусы;
- **отложенный** — `docs\plans\<ГГГГ-ММ-ДД>-<слаг>-deferred.md` (дата — момент отложения; внутри
  «Состояние» и исходный план), индекс — `docs\plans\00_index.md`;
- **закрытый** — `docs\plans\<ГГГГ-ММ-ДД>-<слаг>-closed.md` (дата — завершение; внутри «Что сделано»
  и исходный план), индекс — тот же.

Мелкие задачи без плана в архив не попадают. `.opencode\` для планов не используется.

---

## Отложенные планы

- `docs\plans\2026-10-19-fixed-battle-scene-deferred.md` — фаза 2-A (пин высоты боевой сцены +
  `UI_Overlay` match-height). Возобновление: когда будут доступны эталонные снимки с другого ПК и
  прогон сетки аспектов.

## Активный план

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

- Вендорный рантайм: spine-csharp 2.1 из `unity\Assets\Spine\SpineSharp\Source` (байт-в-байт
  идентичен в обоих деревьях, чистый C# без UnityEngine/UNITY_EDITOR).
- Этап 2 (не сейчас): `git mv` сорсов в `src\Spine`, удаление копий из обоих Unity-деревьев,
  DLL+pdb в оба `Plugins\Internal` (паттерн `CoreUnityPluginFolders`), InternalsVisibleTo
  (SpineUnity использует internal: `RegionAttachment.uvs` — `SkeletonRenderer.cs:384`),
  `unity-compile-check` обоих деревьев, ручной чек-лист в редакторах.
- Своп Unity/Unity-2017 на общую DLL Spine.
- Монстры и остальные герои в карточке дуэли; интеграция анимаций боя (атаки, скиллы).
