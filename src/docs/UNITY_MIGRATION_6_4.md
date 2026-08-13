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

## 2. Runtime-проблемы на старте (не исправлены, требуют отдельной задачи)

Все ошибки возникают при старте игры и выглядят как единый каскад проблемы инициализации данных.

1. **Интро.** `NullReferenceException` в `MoviePlayer.Play()` (`Assets\Scripts\Setup\ContentLoading\MoviePlayer.cs`) — `gameIntro` = null (после удаления `MovieTexture` класс стал заглушкой, иерархия интро не восстанавливает ссылку). Временное решение: вызов `gameMovie.Play()` закомментирован, после логотипов сразу идёт `CampaignSelection` (`GameIntro.LogoEnded`).

2. **Тултипы.** `NullReferenceException` в `SkillTooltip.Initialize()` (`Assets\Scripts\UI\Controls\SkillTooltip.cs:27`) через `ToolTipManager.Start()` (`Assets\Scripts\Managers\ToolTipManager.cs:88`) — `DarkestDungeonManager.Data.HexColors` пуст/не инициализирован.

3. **Никнейм.** `NullReferenceException` в `PlayerNicknameInputField.Start()` (`Assets\Scripts\Setup\ContentLoading\PlayerNicknameInputField.cs:23`) — `DarkestDungeonManager.Data.HeroClasses` пуст.

4. **Сэйвы.** `NullReferenceException` в `SaveCampaignData.PopulateStartingEstateData()` (`Assets\Scripts\Setup\SaveSystem\SaveCampaignData.cs:182`) через `SaveLoadManager.WriteStartingSave` → `SaveSelector.Start()` (`SaveSelector.cs:46`). Следствие: **в Campaign не загружаются/не создаются сэйвы**.

5. **Мультиплеер.** `NullReferenceException` в `DarkestPhotonLauncher.Start()` (`Assets\Scripts\Networking\DarkestPhotonLauncher.cs:116`). Из-за пустого `DarkestPhotonLauncher.HeroPool` пропали иконки выбора героев, а при нажатии стрелки — `ArgumentOutOfRangeException` в `MultiplayerPartyPanel.SwapNextHero()` (`MultiplayerPartyPanel.cs:36`).

6. **Звук и музыка.** Не играют музыка и звуки — вероятно, та же первопричина инициализации (не инициализирован менеджер данных / аудио-настройки).

## 3. Гипотеза первопричины

Все runtime-ошибки сводятся к одному: **`DarkestDungeonManager.Data` не инициализирован к моменту вызова `Start()`** у зависимых компонентов. Это пересекается с ранее задокументированной проблемой парсинга чисел в `src\docs\RUNTIME_MIGRATION.md` (обрыв `DarkestDatabase.Load()` оставляет `HexColors`/`HeroClasses` пустыми). Требуется отдельная задача по восстановлению порядка инициализации данных на старте.

## 4. Статус

- [x] Компиляция под Unity 6.4 (устаревшие API заменены).
- [x] Интро-видео временно отключено (закомментировано).
- [ ] Инициализация `DarkestDungeonManager.Data` на старте — не исправлено.
- [ ] Тултипы / никнейм / сэйвы / мультиплеер / звук — ждут починки первопричины.
