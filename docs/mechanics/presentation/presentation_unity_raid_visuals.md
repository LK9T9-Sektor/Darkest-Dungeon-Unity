# presentation_unity_raid_visuals.md — Легаси Unity-рейд: камеры, фоны, торч, юниты, UI

> Домен: `presentation` (легаси `unity-2017\` + `unity\`, сцена `Dungeon.unity`). Статус: **справка**
> (read-only, легаси не правится). Назначение — документальная фиксация того, как легаси-рейд
> рендерит кадр, чтобы: (1) не переоткрывать поведение заново, (2) обоснованно решать, что тестовая
> сцена/будущий cutover-вид может упростить или переиспользовать. Все номера строк — от текущих файлов.

## 1. Сводка: 3 камеры + 2 света, мир — это UI-канвасы

Весь мир рейда (комната, юниты, фоны) — **Canvas-контент** (world-space канвас `DungeonView`,
scale `0.10693`, `Dungeon.unity:56800-56811`), не 3D-геометрия. Камеры рендерят канвасные плоскости.

| # | Камера | Настройки | Роль |
|---|---|---|---|
| 1 | `DungeonCamera` (`Dungeon.unity:14167-14203`) | **Perspective** FOV 60, depth 0, mask ALL, clear SolidColor чёрный; на ней `RaidPartyCamera`, `AudioListener`, `BlurOptimized` (выкл), FMOD `StudioListener`; позиция `(-1069.303, 0, -300)` | весь мир в перспективе — чтобы работали FOV-зум и follow-пан |
| 2 | `BlurCamera` (`Dungeon.unity:34804-34840`) | Perspective FOV 60, **depth 0.3**, mask **layer 9 Showoff** только, clear Depth only; child `DungeonCamera` | перерисовывает «актёра» (юнита, вынесенного на layer 9) **резко** поверх размытого мира |
| 3 | UI `Camera` (tag **Main UI Camera**, `Dungeon.unity:333-369`) | **Orthographic size 57.75**, depth 1, mask **UI(5)+MainMenu(10)**, clear Depth only; `(-1069, 0, -427.92)` | весь экранный UI (ScreenSpace-Camera канвасы) поверх мира |

Свет:
- **Area Light** (`Dungeon.unity:27039-27103`) — point, белый, `intensity 7`, `range 150`, child `DungeonCamera` (едет за камерой); даёт локальную подсветку **vertex-lit** скелетам героев (эффект «факельного свечения»).
- **Directional light** (`Dungeon.unity:67119-67186`, int.4.65, внутри префаба `UI_RaidInterface`, layer 5) — ключевой свет для vertex-lit скелетов.
- Амбиент Trilight тёмно-синий (`Dungeon.unity:14-41`).

### Почему размеры такие
- Канвас 1920×1080 × scale `0.10693` → «дизайн-пиксель» в мировые единицы (1080 × 0.1069 ≈ 115).
- `orthographic size 57.75` = ровно `1080/2 × 0.10693` — 1:1 маппинг пикселей UI.
- DungeonCamera FOV 60, канвас на z=-200, камера на z=-300 (дистанция 100) → ширина кадра ≈ `2·100·tan(30°) ≈ 115`.
- `RaidPartyCamera` держит **горизонтальный охват ≈252.2945 мировых** при любом аспекте, пересчитывая `StandardFOV` (`RaidPartyCamera.cs:61-77`), чтобы поле всегда было в кадре.

## 2. `RaidPartyCamera` (`Scripts\Raid\RaidPartyCamera.cs`, 121 стр.)

- Поля: `target`, `smoothTime=0.3` (сцена), `raidLight`, `mode`, `blur` (BlurOptimized), `blurCamera`.
- `Awake` (37-41): кэш Camera/Transform. `Start` (43-52): `StandardFOV=TargetFOV=60`, `frustumDistanceTarget` = дистанция от `defaultRoomCameraPosition (-1069.303, 0, -300)` до `RoomView.RaidRoom.position`.
- `LateUpdate` (54-89): (1) плавный FOV-зум к `TargetFOV` + зеркалит в `blurCamera.fieldOfView` (56-60); (2) подгонка под аспект (61-77); (3) `Follow`-режим пан по X (79-88).
- `Zoom(targetFOV, time)` (108-113); `SwitchBlur(activate)` (115-120, кроме Android/iOS); `SetCampingLight` (91-98) — красный Spot; `SetRaidingLight(torchRange)` (100-106) — белый Point, **аргумент игнорируется**.
- Вызовы (`RaidSceneManager.cs`): каст скилла → `Zoom(50)+SwitchBlur(true)` (4100/4103, 4300/4303); resolve-проверка → `Zoom(45)` (4637/4641); дверной проём → `Zoom(30)` (673/688); кемпинг → rotate + красный свет (863-865). Восстановление — `Zoom(StandardFOV,0.1)+SwitchBlur(false)`.
- `Target` переустанавливается в `HeroFormation.Ranks` по концу боя (`BattleGround.cs:505`).

## 3. Фоны — **параллакса нет**

Поиск `parallax` по всему дереву находит только неиспользуемые `_Parallax/_ParallaxMap`-свойства стандартных `.mat` — **нет ни одного parallax-скрипта, ни слоёв, ни смещения по камере**. Фоны — одиночные статичные спрайты:

- `Backdrop.cs` (21 стр.): `Image`+`Animator`; `Activate(name)` → `Resources.Load<Sprite>("Dungeons/shared/"+name)` + `SetBool("IsActive", true)` (11-15), `Deactivate` (17-20). Драйвер: `BattleGround` при спавне монстра с `battle_backdrop:` (`BattleGround.cs:417-418, 608-609`) и `Deactivate` по концу боя (510). Значения: `heartroom`, `starfield`, `secretroom` (только они в `Resources\Dungeons\shared\`).
- Стены комнат/коридоров — предрендеренные затемнённые PNG: `RaidRoom.UpdateEnviroment` → `roomWall.sprite = DungeonSprites[quest+".room_wall."+TextureId]` (`RaidRoom.cs:115-118`); коридоры — `corridor_wall._0/_1`, far/mid фоны `corridor.back/mid` (`RaidHallwayView.cs:78-94`, `RaidHallSector.cs:161-221`). Вариантов под факел нет.

## 4. Торч — **только статы и UI, затенения юнитов нет**

- `TorchMeter.cs` — чистая стат-система: диапазоны Radiant/Dim/Shadowy/Dark/Out (124-131) с `HeroBuffs/MonsterBuffs` (133-181), `ApplyBuffs/ApplyBuffsForUnit` (210-216, 435-462). Визуал — только виджет: пламя-петля Spine `torchFlame.state.SetAnimation` (423), слайдер (403-433), искры.
- `SetRaidingLight(torchRange)` **игнорирует** диапазон — свет всегда белый Point int.7 (`RaidPartyCamera.cs:100-106`).
- **Ни одного** `material.SetColor`/`_TintColor`/`_Color` по юнитам. Шейдеры: герои — `Spine/SkeletonLit` (vertex-lit, реагируют на свет), монстры — `Spine/Skeleton` (unlit). Единственная цветовая динамика — красный пульс Death Door через `Skeleton.R/G/B` (`FormationUnit.cs:59-123`).
- Вывод: «затемнение от факела», как в оригинале DD, в этом порте **отсутствует**; темнота запечена в текстуры, свет константный.

## 5. Префабы юнитов

- `crusader.prefab` / `brigand_cutthroat.prefab`: root = `RectTransform` + `FormationUnit`; child `CrusaderAnimator`/`Animator` = `Animator` + `UnitAnimator` + 3 контейнера `Abilities`/`States`/`Effects`, где каждый лист — Spine `SkeletonAnimation` со своим `skeletonDataAsset` + `_animationName` (idle/combat/attack_* и т.д.).
- Сортировка — `MeshRenderer.sortingOrder` (`UnitAnimator.cs`), States=8 / Effects=9 / halo=state+1.
- Материалы Spine-импортера живут в **gitignored** `Library\SpineAssets`; шейдер по умолчанию `Spine/Skeleton` (`SpineEditorUtilities.cs:157`); герои — `SkeletonLit`, монстры — `Skeleton`.
- `UnitAnimator.cs` только ставит sortingOrder и играет анимации; вся работа с `RaidSceneManager`-статикой — в `FormationUnit.cs`.

## 6. UI рейда (способности / карта / предметы / тултип)

Иерархия: `UI_RaidInterface` → `RaidPanel` (`Dungeon.unity:51142`, якорь 0,0–1,0.333 — **нижняя треть**) → `LeftPanel` (`PanelBanner`, `PanelHero`) + `RightPanel` (`PanelMap`, `PanelInventory`) + `PanelMonster`.

- **Способности**: `PanelBanner` → `SkillsPanel` (`RaidCombatSkillsPanel`) — 4 слота скиллов + `MoveSkillSlot` + `PassSkillSlot` (+4 camping). Иконки: `HeroSpriteDatabase.GetCombatSkillIcon` (`BattleSkillSlot.cs:93`). Таргетинг: `HeroSkillSelected` (`RaidSceneManager.cs:1326-1397`) подсвечивает оверлей-слоты → клик → `HeroSkillTargetSelected` (1474-1506) → `Round.HeroActionSelected`. `SetUsableCombatSkills` (100-154) проверяет лимиты/ранги/иммобилизацию.
- **Карта**: `RaidMapPanel` — `ScrollRect` + масштабируемый `mapContent` (1.0–3.2×), сетка `RaidMapRoomSlot`/`RaidMapHallwaySlot`/`RaidMapHallSectorSlot` по знанию (`Knowledge.Hidden`→тёмный спрайт, Scouted/Visited→иконка типа), маркер «moving_room».
- **Предметы**: `PartyInventory` (слоты = `GetComponentsInChildren<InventorySlot>`, провизия по квесту `LoadInitialSetup` 56-88, стаки `DistributeItem` 213-264); юз-флоу `HeroItemActivated` → `ExecuteHeroItemUsage` (`RaidSceneManager.cs:4382-4569`: провизия/бандажи/противоядия/факел).
- **Тултип монстра**: `PanelMonster` = `MonsterTooltip` (ховер по `FormationOverlaySlot.OnPointerEnter`, предикт урона через `BattleSolver.CalculateSkillPotential` при ходе героя).
- **Поле боя** (`FormationOverlaySlot`) портретов/иконок **не использует** — только HealthBar/StressOverlayPanel/TrayPanel + выбор.

## 7. Showoff: blur + layer 9

- Актёр во время каста выносится на **layer 9 Showoff** + sorting order `ShowoffOrder+4−rank` (`PartyFormationManager.cs:40-41, 301, 315`, `FormationUnit.cs:256-260`), мир размывается `BlurOptimized` на DungeonCamera, `BlurCamera` (depth 0.3, mask 9) перерисовывает его резко. Обратно — в `UnitSkillOutro/UnitDefendOutro/UnitBuffedOutro` (322-394).
- Итог: размытый фон, резкий «фокусный» юнит при касте.

## 8. Как собирается финальный кадр

1. `DungeonCamera` (depth 0) чистит экран чёрным, рисует мир по sorting order: `DungeonView` (order 1: комната/юниты) → `DungeonOverlay` (order 4: Backdrop + позиционные слоты атак/защиты).
2. `BlurOptimized` (только при showoff) размывает весь мир.
3. `BlurCamera` (depth 0.3) рисует layer-9 актёра резко поверх.
4. UI `Camera` (depth 1, орто) рисует экранные канвасы (HUD, факел, баннеры, оверлеи).
Порядок по depth 0 → 0.3 → 1; внутри мировой камеры — по sorting order канвасов.

## 9. Что можно упростить (выводы для теста/cutover)

- **BlurCamera + BlurOptimized + layer-9 Showoff — убирается целиком** (чистая драматургия каста).
- **UI-камера не нужна**, если канвасы `ScreenSpaceOverlay` (тестовая `BattleHud` уже так делает).
- Для теста достаточно **одной орто-камеры**; перспектива + FOV-зум нужны только при честном cutover-виде (Фаза 6) ради зум-драмы.
- **Area Light** нужен, только пока герои на `SkeletonLit`; перевести всё на unlit — свет не нужен.
- **Параллакса и факельного затенения юнитов в легаси нет** — копировать нечего.
- Координатная база: канвас 1920×1080 @ scale 0.10693, камера на z=-300, FOV 60, охват ~252 ед. — юниты следует ставить в мировые позиции слотов из сцены, а не подбирать.

## 10. Файлы-источники

- `unity-2017\Assets\Scenes\Dungeon.unity` (камеры 301-401, 14088-14230, 34775-34840; Area Light 27039-27103; DungeonView 56774-56872; RaidPanel 51142+).
- `unity-2017\Assets\Scripts\Raid\RaidPartyCamera.cs`, `Backdrop.cs`, `TorchMeter.cs`.
- `unity-2017\Assets\Scripts\Raid\Party\{UnitAnimator,FormationUnit,FormationOverlaySlot,PartyFormationManager}.cs`.
- `unity-2017\Assets\Scripts\UI\Panels\{RaidPanel,RaidBannerPanel,RaidCombatSkillsPanel,RaidMapPanel,RaidInventoryPanel}.cs`, `Scripts\UI\Controls\MonsterTooltip.cs`, `Scripts\UI\Inventory\{PartyInventory,InventorySlot,InventoryItem}.cs`.
- `unity-2017\Assets\Scripts\Database\HeroSpriteDatabase.cs`, `DarkestDatabase.cs`.
- `unity-2017\Assets\Scripts\Raid\Area\RaidRoom.cs`, `RaidHallwayView.cs`, `RaidHallSector.cs`.
- См. также: `docs\mechanics\presentation\presentation_unity_battle_view.md` (тестовая сцена), `docs\UNITY_LEGACY_MAP.md`, `docs\EXTRACTION_PLAN.md` (Фаза 6).