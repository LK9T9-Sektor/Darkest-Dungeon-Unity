# PLAN.md — планы задач

Активный план — ниже на этой странице. Завершённые планы переносятся в `docs\plans\` (индекс и
статусы — `docs\plans\00_index.md`); при завершении план уезжает в архив целиком, чтобы контекст
не терялся при работе с другого ПК — архив в git, `.opencode\` для планов не используется.

---

# PLAN: Фаза 2-A — фиксированная боевая сцена (пин высоты + оверлей)

## Goal

Заморозить центральную сцену боя (герои/враги + приклеенный HUD) от зависимости от аспекта:
камера пинит **высоту** кадра мира (141.9156 юнитов = 252.2945×9/16) вместо ширины, `UI_Overlay`
переводится на match-height, чтобы HP/стресс/лоток/рамка не отрывались от моделей. На 16:9
поведение не меняется (при 16:9 формулы эквивалентны; в тестовом рейде FOV 65.38171°, `dist`
считается в `Start()` и зависит от комнаты); на прочих аспектах расширяется/сужается только окружение по
бокам. Плита, чёрные поля вне 4:3–32:9 и решение по стенам комнат — этап B (после сверки A).

## Design decisions (user-confirmed)

- Full-bleed 4:3–32:9, бары вне диапазона — цель фазы (бары реализуются в этапе B).
- HUD неотделим от моделей (`FormationOverlaySlot.LateUpdate` проецирует позицию юнита; панели —
  статические оффсеты префаба `OverlaySlot`), поэтому фиксируется связка «модель+HUD» целиком.
- Фикс — по этапам: A = камера + оверлей + верификация по сетке аспектов; B = плита/бары/комнаты.
- Пиксельно-фиксированный стейдж через вторую камеру со слоями — не делаем (дороже; пин высоты
  даёт тот же эффект для вертикали и масштаба).
- Легаси-правки минимальные: точечная замена формул FOV, без рефактора сцены.

## Steps / status

1. [ ] PLAN.md зафиксирован.
2. [ ] Комната 16:9 (вопрос этапа B: покрывает ли стена видимые 252 юнита при её ~205) — снимок
   валиден и ПОСЛЕ A (на 16:9 ничего не меняется), снимается пользователем при верификации шага 5
   (зайти в данж-комнату, ресайз окна = триггер). Коридорный baseline 21:9 уже есть
   (`Dungeon-resize-2658x960-*`).
3. [x] `RaidPartyCamera.cs:25-26,68-76`: пин высоты — `frustumTargetHeight = frustumTargetWidth *
   9f / 16f` (141.9156); FOV = `2·atan(H/2/dist)`. На 16:9 значение не меняется (старая формула при
   16:9 эквивалентна; «26.62°» из ранней версии плана — ошибка от dist≈300 — реальный
   `frustumDistanceTarget` ≈ 110.5 → **65.38171°**, подтверждено CSV). BlurCamera зеркалит FOV
   автоматически (строка 60).
4. [x] `UI_Overlay` (`Dungeon.unity:67950`): CanvasScaler match 0→1. Инвентаризация детей: только
   4 — `RightMovementPointer`, `EncounterOverlay`, `PartyOverlay`, `LeftMovementPointer` (якорных
   панелей нет; стрелки движения станут height-relative — на 16:9 без изменений).
4b. [x] Селектор разрешения в окне настроек (`SoundSettingsUI`): строка «Разрешение» со степперами
   по 9 пресетам от 9:16 (портрет) до 32:9 (`Screen.SetResolution(..., FullScreenMode.Windowed)`);
   значение — текущие `Ш x В`; подписи — существующие ключи локализации
   (`menu_options_element_resolution` / `menu_options_value_seperation_resolution`). Нужен, потому
   что в билде окно не тянется — без него сетку аспектов не прогнать. Размеры пресетов подобраны
   так, чтобы аспект совпадал с редакторными (16:9=1920×1080, 1.6=1280×800, 4:3=960×720,
   2.14=1544×720, 2.2=1320×600, 21:9=1896×800, 2.77=1329×480, 32:9=1920×540, портрет=608×1080).
5. [ ] Верификация A (в процессе, прогоны 19.10 ночи):
   - [x] `unity-compile-check.ps1` зелёный; билд №2 (селектор + скрытие оверлеев на PNG) — ОК.
   - [x] 16:9: кадр совпал с pre-pin baseline `Dungeon-auto-t5s-20261018-235955-771.png`
     (слоты x=0.3808 идентично, y Δ≈1.4 px — idle-анимация).
   - [x] Пин высоты численно: на 2.77 слоты остались на тех же 419.25 юнитов (норм y 0.3882, как
     на 16:9 в том же месте карты) — вертикаль и масштаб от аспекта не зависят.
   - [x] Скрытие рантайм-канвасов на PNG подтверждено: ни панели настроек, ни шестерёнки.
   - [x] Плита обрезается снизу на 2.14/32:9 (`UI_RaidInterface`, match=0) — ожидаемо, объём B.
   - [ ] Чистый прогон оставшихся аспектов (4:3, 1.6, 21:9, 32:9) + комната 16:9 — рецепт в
     разделе «Требуется от пользователя»; портрет для A не требуется (вне полосы 4:3–32:9).
6. [ ] Доки A: `docs\mechanics\presentation\presentation_unity_urp_cameras.md` (пин высоты),
   `TESTING.md` (чек-лист аспектов).
7. [ ] Коммит на `core/unity7-urp` (сообщение по правилам; push — по запросу).

## Требуется от пользователя: чистый прогон сетки

В прогоне 19.10 клики по разрешению совпадали с триггерами — JSON/DTO/PNG снимались на **разных**
разрешениях (гонка с `Screen.SetResolution`), часть данных «лоскутная». Для приёмки A нужен
спокойный прогон. Рецепт на каждый аспект: шестерёнка → строка «Разрешение» → выбрать пресет →
**не кликать ~3 секунды** → F9. Снимки штатно падают в `SceneDumps` (папка вычищена 19.10),
панель настроек на PNG скрывается автоматически.

- [ ] 960×720 (4:3) — край полосы, обязателен.
- [ ] 1280×800 (1.6) — обязателен.
- [ ] 1896×800 (21:9) — обязателен.
- [ ] 1920×540 (32:9) — край полосы, обязателен.
- [ ] Комната 16:9 (шаг 2): на 1920×1080 зайти именно в комнату (не коридор), дождаться остановки
  камеры → F9 — решение по стенам для этапа B.
- 16:9 и портрет 608×1080 повторять не нужно (16:9 уже сверен с baseline; портрет — вне полосы).

После прогона сказать «снимки готовы» — агент разбирает PNG+CSV и закрывает шаг 5.

## Этап B (после сверки A, отдельный раздел при старте)

Плита RaidPanel тянется по полосе; верхние элементы (QuestLogPlate/TorchPanel/RoundIndicator) по
результатам; бары вне 4:3–32:9 (FOV-кламп + поля; проверить Screen Space Camera канвасы при
rect-камерах); решение по стенам комнат (>16:9) — растянуть/затайлить Image стены или принять.

## Affected files

- `unity\Assets\Scripts\Raid\RaidPartyCamera.cs`.
- `unity\Assets\Scenes\Dungeon.unity` (CanvasScaler UI_Overlay).
- `docs\mechanics\presentation\presentation_unity_urp_cameras.md`, `docs\TESTING.md`, `PLAN.md`.

## Acceptance criteria

- 16:9: FOV и позиции совпадают с baseline (PNG пиксель-в-пиксель; доли слота/панелей те же).
- 2.14/2.2/2.77: линия боя и слоты — те же доли, что на 16:9 (0.4209/…); HP/стресс/лоток прижаты
  к моделям; оверлей не растёт относительно высоты.
- 4:3/1.6: вертикальная композиция та же, по бокам видно меньше мира; без обрезков сцены.
- Портрет: документированный interim (сцена заморожена, поля — этап B).
- `unity-compile-check.ps1` зелёный; `unity-2017\` и `src\External\` не тронуты.

## Cleanup (по завершении этапа A)

- [x] 19.10: `SceneDumps` вычищен — 1010 МБ → 47 МБ. Оставлены pre-pin эталоны (невоспроизводимы):
  `Dungeon-auto-t5s-20261018-235955-771.*` (16:9, сверка шага 5) и
  `Dungeon-resize-2658x960-20261019-000051-101.*` (коридор 2.77, baseline шага 2), плюс PNG+CSV
  старых кадров 3088x1440 / 2640x1200 / 520x325 / 677x423 («до» для этапа B).
- [x] 19.10: `%TEMP%\unity-*.log` — удалено 12 старых логов, освобождено 7,1 МБ; оставлены 2
  последних (успешный билд №2 `unity-build-20261019-012552.log` и падение на залоченном
  `Assembly-CSharp.dll` `unity-build-20261019-011808.log`).
- [ ] После приёмки A и старта B — удалить снимки чистого прогона (этап B снимет свои).

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
