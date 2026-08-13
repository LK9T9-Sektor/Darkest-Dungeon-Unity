# Миграция Unity 2017.4 → 6000.4.5f1 (Unity 6.4)

Переход проекта с Unity 2017.4.40f1 на Unity 6000.4.5f1. Ниже — что изменилось, какие ошибки компиляции были устранены и какие runtime-проблемы остались на старте.

## 1. Что изменилось

- `ProjectSettings\ProjectVersion.txt`: `m_EditorVersion: 6000.4.5f1` (было 2017.4.40f1).
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
