# Журнал изменений

## 1.0.4 — звуковые настройки

Версия 1.0.4 (ветка `1.0.4`) добавляет в игру окно звуковых настроек: регулировка громкости музыки и SFX, кнопки закрытия и выхода, а также связывает громкость SFX с боевыми и мировыми звуками.

### Новая функциональность

- Runtime-окно настроек `SoundSettingsUI` (`Assets\Scripts\UI\SoundSettingsUI.cs`): создаётся автоматически через `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` и живёт во всех сценах (`DontDestroyOnLoad`, Canvas sortingOrder 30000, CanvasScaler 1920×1080).
- Два степпера громкости — музыка и SFX: 10 шагов (клик = 10% шкалы), текущие значения синхронизируются из `DarkestSoundManager` при каждом открытии.
- Кнопка-шестерёнка «AudioButton» (56×56, спрайт `settings.button`) в правом верхнем углу экрана — открытие окна.
- Окно 1024×740 по центру экрана (фон `menu.background`), заголовок «Settings», шрифт Deutsch, цвет текста — оригинальный `_labelColor`.
- Кнопка закрытия «X» (32×32, `progression_close.png`) в правом верхнем углу окна — скрывает панель.
- Кнопка «Exit to Main Menu» (466×97, `menu.element_text_button_overlay.png`) внизу окна — возврат в главное меню.
- Локализация через существующие ключи `Menu.xml`: `menu_options_title`, `menu_options_element_music_volume`, `menu_options_element_sfx_volume`, `menu_base_element_exit_campaign` (с фолбэком на английский при недоступности менеджера).

### Спрайты и ассеты

- Спрайты собраны в ScriptableObject `SoundSettingsSprites` (`Assets\Resources\UI\SoundSettingsSprites.asset`): `WindowFrame`, `MinusArrow`, `PlusArrow`, `CloseIcon`, `ExitButtonOverlay`.
- Ссылки на оригинальные ассеты (`Assets\Sprites\...`, исключены из репозитория) — по GUID; при отсутствии спрайта UI использует тёмную заливку вместо него.

### Звук

- `DarkestSoundManager.PlayOneShot` переписан: create → `setVolume(SfxVolume)` → start → release, без общего поля — одновременные боевые звуки не режутся и подчиняются громкости SFX.
- 135 прямых вызовов `FMODUnity.RuntimeManager.PlayOneShot` переведены на `DarkestSoundManager.PlayOneShot` в 9 файлах (`RaidSceneManager`, `RaidEvents`, `RaidSceneMultiplayerManager`, `HealEffect`, `BattleGround`, `EstateCurrencyPanel`, `HallSector`, `Room`, `FormationOverlaySlot`).
- 3 звука через `CreateInstance` (`combat/start` ×2, `props/curios`) получили `setVolume(SfxVolume)`.
- Музыка не затрагивается (воспроизводится не через `PlayOneShot`).

### Выход из игры

- Кнопка «Exit to Main Menu» не закрывает игру напрямую, а возвращает в главное меню: вызывает `MainMenuWindow.ReturnToCampaignSelection()` (сохранение, выход из мультиплеерной комнаты, `SilenceNarrator`), с фолбэком на `SceneManager.LoadScene("CampaignSelection")`.
- Полный выход из игры происходит только из главного меню через собственную кнопку «Return To Desktop» → `MainMenuWindow.QuitGame()`.
- В редакторе Unity `MainMenuWindow.QuitGame()` останавливает Play Mode (`UnityEditor.EditorApplication.isPlaying`); в сборке — `Application.Quit()`.

### Изменённые файлы

| Файл | Изменение |
|---|---|
| `Assets\Scripts\UI\SoundSettingsUI.cs` | окно настроек, степперы, кнопки |
| `Assets\Scripts\UI\SoundSettingsSprites.cs` | ScriptableObject со спрайтами |
| `Assets\Resources\UI\SoundSettingsSprites.asset` | ссылки на оригинальные спрайты |
| `Assets\Scripts\Managers\DarkestSoundManager.cs` | `PlayOneShot` + громкость SFX |
| `Assets\Scripts\UI\Windows\MainMenuWindow.cs` | выход из игры: Play Mode в редакторе, `Application.Quit()` в сборке |
| `Assets\Scripts\Managers\RaidSceneManager.cs` и др. (9 файлов) | маршрутизация SFX через громкость |

### Проверка

- Компиляция Unity 2017.4.40f1 batchmode: `Compilation succeeded`, без ошибок.
