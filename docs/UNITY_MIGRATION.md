# Миграция Unity 2017.4 → Unity 6

Переход проекта с Unity 2017.4.40f1 на Unity 6000.4.5f1. Ниже — что изменилось, какие ошибки компиляции были устранены и какие runtime-проблемы остались на старте.

## 1. Что изменилось

- `ProjectSettings\ProjectVersion.txt`: `m_EditorVersion: 6000.4.5f1` (было 2017.4.40f1). Актуальная версия — **7000.0.0a10** (Unity 7, см. §5–§6).
- Старые API удалены или помечены obsolete → правки компиляции:
  - `GUIText` → `UnityEngine.UI.Text` (`DemoBoxesGui.cs`);
  - `MovieTexture` удалён — `MoviePlayer` превращён в заглушку;
  - `BuildPlayer` теперь возвращает `BuildReport` вместо `string` (`Assets\Editor\BuildGame.cs`);
  - `UnityEngine.RPC` удалён — убрана детекция старых RPC-атрибутов (`PhotonEditor.cs`);
  - `TextureImporterFormat.AutomaticTruecolor` → `textureCompression = Uncompressed` (`SpineEditorUtilities.cs`);
  - `AnimatorController.AddAnimationClipToController` → `AddMotion` (`SkeletonBaker.cs`);
  - `AnimatorController.layerCount` (UnityEditor.Animations) больше недоступен в неактивной ветке `#if` → `m_Animator.layerCount` (`PhotonAnimatorViewEditor.cs`);
  - неоднозначность `Hashtable` в `LoadbalancingPeer.cs` → явный `ExitGames.Client.Photon.Hashtable`;
  - `Texture m_previewTex = new Texture()` → `null` (`SkeletonDataAssetInspector.cs`).

## 2. Runtime-проблемы на старте

Все ошибки при старте были каскадом одной первопричины — обрыва загрузки данных (см. п. 3). ИСПРАВЛЕНО.

1. **Интро.** `NullReferenceException` в `MoviePlayer.Play()` (`Assets\Scripts\Setup\ContentLoading\MoviePlayer.cs`) — `gameIntro` = null (после удаления `MovieTexture` класс стал заглушкой, иерархия интро не восстанавливает ссылку). Временное решение (оставлено): вызов `gameMovie.Play()` закомментирован, после логотипов сразу идёт `CampaignSelection` (`GameIntro.LogoEnded`).

2. **Тултипы.** `NullReferenceException` в `SkillTooltip.Initialize()` (`Assets\Scripts\UI\Controls\SkillTooltip.cs:27`) через `ToolTipManager.Start()` (`Assets\Scripts\Managers\ToolTipManager.cs:88`) — `DarkestDungeonManager.Data.HexColors` не инициализирован из-за обрыва `Load()`. → ИСПРАВЛЕНО фиксом п. 3.

3. **Никнейм.** `NullReferenceException` в `PlayerNicknameInputField.Start()` (`Assets\Scripts\Setup\ContentLoading\PlayerNicknameInputField.cs:23`) — `Data.HeroClasses` не загружен. → ИСПРАВЛЕНО фиксом п. 3.

4. **Сэйвы.** `NullReferenceException` в `SaveCampaignData.PopulateStartingEstateData()` (`Assets\Scripts\Setup\SaveSystem\SaveCampaignData.cs:182`) через `SaveLoadManager.WriteStartingSave` → `SaveSelector.Start()` (`SaveSelector.cs:46`). → ИСПРАВЛЕНО фиксом п. 3.

5. **Мультиплеер.** `NullReferenceException` в `DarkestPhotonLauncher.Start()` (`Assets\Scripts\Networking\DarkestPhotonLauncher.cs:116`) и `ArgumentOutOfRangeException` в `MultiplayerPartyPanel.SwapNextHero()` (`MultiplayerPartyPanel.cs:36`) — пустой `HeroPool` из-за пустых `HeroClasses`. → ИСПРАВЛЕНО фиксом п. 3.

6. **Звук и музыка.** Не играли музыка и звуки — следствие той же первопричины. Ожидается, что исправлено фиксом п. 3 (требует подтверждения на прогоне).

## 3. Первопричина и фикс

Строгий Newtonsoft Json.NET в Unity 6.4 (в отличие от парсера Unity 2017, который молча обрезал дробную часть) бросает `JsonReaderException`, когда JSON-число с дробной частью попадает в поле типа `int`:

```
JsonReaderException: Input string '0.33' is not a valid integer.
Path 'traits[0].reaction_act_outs[0].chance'
    at DarkestDatabase.LoadTraits() (DarkestDatabase.cs:1820)
    at DarkestDatabase.Load() (DarkestDatabase.cs:97)
    at DarkestDungeonManager.Awake() (DarkestDungeonManager.cs:81)
```

`Load()` прерывался на `LoadTraits()` **до** `LoadJsonHeroClasses()` и `LoadColours()`, поэтому `HeroClasses`/`HexColors` оставались пустыми → каскад NRE из п. 2.

**Фикс:** `JsonReactionActOut.chance` (`Assets\Scripts\Database\DarkestJsonReader.cs:180`) `int` → `float` — совпадает с рантайм-полем `ReactionActOut.Chance` (`Assets\Scripts\Character\Trait.cs:64`), шансы реакций начинают работать как задумано (0.33 = 33%). `JsonCombatStartTurnActOut.chance` намеренно оставлен `int` (данные целочисленные, рантайм-поле `int`).

**Проверка:** вне Unity собран временный консольный харнесс (модели `DarkestJsonReader.cs` + Newtonsoft.Json 13.0.3), прогнаны **все** `JsonDarkestDeserializer.GetJson*` по реальным файлам `Assets\Resources\Data\` (52 десериализации: 10 корневых JSON, 8 зданий, 16 апгрейдов героев, 8 апгрейдов зданий, ловушки/препятствия, цели квестов). До фикса — 1 ошибка (`JsonTraits.json`); после фикса — 0. Других несоответствий int/float в данных не найдено.

## 4. Статус

- [x] Компиляция под Unity 6.4 (устаревшие API заменены).
- [x] Интро-видео временно отключено (закомментировано).
- [x] Первопричина каскада — `JsonReaderException` в `LoadTraits` — исправлена (`JsonReactionActOut.chance` → `float`).
- [x] Аудит всех JSON-данных через внешний харнесс — 0 ошибок десериализации.
- [ ] Подтверждение на живом прогоне: список героев заполнен, NRE на старте нет.

## 5. Обновление 6000.4.5f1 → 6000.5.8f1 (Unity 6.5)

В Unity 6.5 ряд API стал obsolete-as-error (CS0619): `GetInstanceID()`, `hierarchyWindowItemOnGUI`, `EditorUtility.InstanceIDToObject(int)`. Проект переведён на `EntityId` (`Object.GetEntityId()`), у которого нет конверсии в int — используется как ключ коллекций напрямую либо через `GetHashCode()` для RNG-сидов.

- `ProjectSettings\ProjectVersion.txt`: `m_EditorVersion: 6000.5.8f1`.
- Свои скрипты:
  - `Assets\Scripts\Networking\DarkestPhotonLauncher.cs:121` — сид героя: `GetInstanceID()` → `GetEntityId().GetHashCode()`.
  - `Assets\Scripts\Networking\MultiplayerPartyPanel.cs:60` — сид перекатки черт: то же.
  - `Assets\Scripts\Setup\ContentLoading\RoomSelector.cs:74` — `Random.InitState`: то же.
- Spine runtime:
  - `Assets\Spine\SpineUnity\SpriteAttacher.cs` — `Dictionary<int, AtlasRegion>` → `Dictionary<EntityId, AtlasRegion>`, `List<int>` → `List<EntityId>`, ключи `tex.GetInstanceID()` → `tex.GetEntityId()`.
  - `Assets\Spine\SpineUnity\SkeletonRenderer.cs:259,796` — сравнение материалов через `GetEntityId() ==/!=`.
- Spine editor:
  - `Assets\Spine\SpineUnity\Editor\SpineEditorUtilities.cs` — три `Dictionary<int, …>` → `Dictionary<EntityId, …>`, событие → `hierarchyWindowItemByEntityIdOnGUI`, сигнатура обработчика `(EntityId instanceId, Rect)`.
- FMOD (vendored, минимальный дифф):
  - `Assets\Plugins\Editor\FMOD\EventBrowser.cs` — событие → `hierarchyWindowItemByEntityIdOnGUI`, обработчик `(EntityId instance, Rect rect)`, `InstanceIDToObject(instance)` → `EntityIdToObject(instance)`.
- Инструменты: `tools\unity-compile-check.ps1`/`unity-build-game.ps1`/`unity-provision-plugins.ps1` находят редактор 6000.5.8f1 (`C:\Program Files\Unity\Hub\Editor\6000.5.8f1`). Фасады .NET Standard для Unity 6+ не требуются (нативный type-forwarding; каталог `MonoBleedingEdge\lib\mono\unityjit\Facades` в 6.5 отсутствует) — доставка фасадов пропускается.
- Проверка: `pwsh tools\unity-compile-check.ps1 -UnityEditorPath <editor 6.5> -Provision` → `Compilation succeeded`.

## 6. Обновление 6000.5.8f1 → 7000.0.0a10 (Unity 7) и переход на URP

- `ProjectSettings\ProjectVersion.txt`: `m_EditorVersion: 7000.0.0a10`.
- Unity 7 отключила Built-in RP: без render pipeline asset сцены рисуются чёрным и на старте — баннер
  «Your project has no render pipeline asset». Переход — по официальному гайду
  (`docs.unity3d.com/7000.0/.../urp/upgrading-from-birp.html`): `com.unity.render-pipelines.universal`
  17.7.0 (built-in пакет редактора), URP Asset + Renderer в `Assets\Settings\DarkestDungeonURP*.asset`
  (коммит `b07cb27`), `GraphicsSettings.asset` → Default Render Pipeline = URP asset (коммит `29e7288`).
- Dynamic Batching в Unity 7 удалён: LogError `PlayerSettingsBatchingWarning.WarnIfDynamicBatchingEnabled()`
  на старте, галка в Player Settings серая → `m_DynamicBatching: 0` в `ProjectSettings.asset` правится
  при закрытом редакторе (коммит `29e7288`).
- Fixed-function пассы не поддерживаются SRP: `Skeleton.shader`/`SkeletonLit.shader` переписаны на CG
  unlit v/f (имена, свойства, теги сохранены; коммит `39881ec`). `SkeletonLit` временно unlit —
  освещение героев отложено, см. `KNOWN_ISSUES.md` §15.
- `error CS1703` после апгрейда: в `Assets\Plugins\Internal` (gitignored) остались 127 .NET-фасадов из
  провиженинга Unity 2017.4 — они дублируют `Editor\Data\UnityReferenceAssemblies\unity-4.8-api\Facades`
  Unity 7. Удалены; в папке обязаны оставаться только `Sektor.*` DLL/PDB (см. `COMPABILITY.md` §4).
- Инструменты: `tools\unity-compile-check.ps1`, `unity-build-game.ps1`, `unity-provision-plugins.ps1`,
  `unity-generate-battle-test-scene.ps1`, `unity-reserialize.ps1`, `unity-asset-diff.ps1` → редактор
  7000.0.0a10 (коммит `93802d1`).
- **Пересериализация ассетов (2026-10-23):** Unity 7 переписывает ассет старого формата при каждом
  открытии, поэтому в диффах появлялся случайный шум. Один проход зафиксирован:
  `tools\unity-reserialize.ps1` + `BatchReserialize.Run` (`Assets\Editor\BatchReserialize.cs`)
  перезаписали 275 префабов и 16 материалов (`Assets\Resources`, `Assets\Prefabs`, `Assets\Materials`);
  вендорные папки (`Plugins`, `Photon*`, `Spine`, `MobileDependencyResolver`) не тронуты. Префабы
  пишутся через `PrefabUtility.LoadPrefabContents`/`SaveAsPrefabAsset` — `AssetDatabase.ForceReserializeAssets`
  их текстовое представление не меняет. fileID объектов сохранены, все 7 сцен открываются без
  потерянных prefab-инстансов; проверка расхождений — `tools\unity-asset-diff.ps1` (сравнивает blob
  из `HEAD` с рабочей копией и печатает только изменения данных, а не формата).
- **Render Pipeline Converter не запускался** (отложено решением пользователя): материалы на built-in
  Standard-шейдере (`CoridorWall*.mat`, `Text-Title-Standard.mat` и др.) не конвертированы — под URP
  ожидаемо рисуются некорректно; визуально не проверялись. После прогона — отдельный коммит, см.
  `KNOWN_ISSUES.md` §15 и чек-лист в `TESTING.md`.
- Проверка: `pwsh tools\unity-compile-check.ps1` (редактор закрыт) → `Compilation succeeded` +
  script-reference check зелёные; оба предупреждения в консоли исчезли.
- **«Жёлтый экран» рейда (исправлено):** в `Dungeon`/`DungeonMultiplayer` `BlurCamera` и
  `UI_Dangeon/Camera` имели `Clear Flags = Nothing`, но оставались Base-камерами — Render Graph путь
  URP (`UniversalRendererRenderGraph.cs:478-485`) при `Nothing` чистит фон в `Color.yellow` (верхние
  2/3 экрана, мир скрыт). Исправлено переходом на камерный стек: `DungeonCamera` — Base со стеком
  `[BlurCamera, UI Camera]`, обе лишние — Overlay (`m_CameraType: 1`, `m_ClearDepth: 0`). Детали —
  `docs\mechanics\presentation\presentation_unity_urp_cameras.md`.
- **Решение IDE: Unity 7 генерирует `.slnx`, а не `.sln`** — новая SDK-style генерация проектов
  (`com.unity.ide.visualstudio` 2.0.27, встроенный в редактор): `ProjectGeneration\SdkStyleProjectGeneration.cs`
  (`SolutionFileImpl()` → `<имя>.slnx`, `SyncSolution()` перед записью удаляет одноимённый `.sln`);
  legacy-ветка `LegacyStyleProjectGeneration.cs` делает обратное (удаляет `.slnx`). Поэтому в `unity\`
  лежит `unity.slnx` (4× `Assembly-CSharp*`) и нет классического `unity.sln`. Решение **открывает сам
  Unity** («Open C# Project» → `VisualStudioEditor.OpenProject` → `generator.SolutionFile()`),
  перезаписывает — только при смене состава проектов (`ProjectGeneration.cs`, сравнение с предыдущим
  решением), поэтому файл трекается в git (в `.gitignore`: `*.sln` игнорируется, `!*.slnx` — исключение;
  добавлен коммитом `fd04e75`). **Не редактировать и не удалять** `unity\unity.slnx` — Unity создаст его
  заново. Точка входа для разработки — корневое `Darkest-Dungeon-Unity.slnx`, см. `ARCHITECTURE.md` §6.
