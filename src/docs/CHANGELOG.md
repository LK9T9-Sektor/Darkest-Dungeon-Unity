# Журнал изменений

## 1.0.6 — общий мультиплеер: выбор провайдера и гарантированное закрытие Steam

Версия 1.0.6 (ветка `steam`) убирает отдельную Steam-панель лобби: вход в мультиплеер теперь начинается с оверлея выбора провайдера PHOTON/STEAM, а дальше оба провайдера используют один и тот же экран списка комнат и выбор героев (панель отряда переиспользуется). Также исправлено зависание «Spacewar» в Steam после закрытия игры.

### Меню выбора провайдера

- Новый `MultiplayerProviderMenu` (`Assets\Scripts\Networking\MultiplayerProviderMenu.cs`) — runtime-оверлей на `CampaignSelection`: две строки PHOTON/STEAM, крупный шрифт (40), навигация стрелками ↑/↓ + Enter (Esc — закрыть), поддержка мыши (hover/клик). Текущий провайдер подсвечен при открытии.
- `RoomSelector.SaveSelectionStart` (кнопка «Multiplayer») открывает меню провайдера; выбор инициализирует провайдера (`MultiplayerSync.SetSteamProvider`; для Steam — `MultiplayerSync.EnsureSteamSession`, создаёт/инициализирует `SteamSessionManager` без панели) и открывает общий список комнат `RoomSelector.OpenRoomList()`.
- `SteamLauncher` (панель «Steam Co-op Lobby», переключатель PHOTON/STEAM, кнопка «Open STEAM Lobby») и `MultiplayerMenuState` удалены.

### Общий список комнат (переиспользование выбора героев)

- `RoomSelector` открывает единый экран для обоих провайдеров: выбор героев (`MultiplayerPartyPanel` + `CharacterWindow`) и статус-панель общие.
- В Steam-режиме три слота комнат служат вводом lobby ID: подтверждение слота вызывает `RoomSelector.JoinSteamLobby(id)` → `SteamSessionManager.JoinSession` (с проверкой выбора скиллов). Кнопка Play — хост новой сессии (`HostSession`).
- При создании/входе в сессию (`MultiplayerSync.SessionJoined`) отряд захватывается, отправляется сопернику (`party_config`) и загружается `DungeonMultiplayer`; возврат/разрыв сессии — через `MultiplayerSync.Steam.LeaveSession`.
- Steam не инициализируется в Photon-режиме и наоборот; недоступность Steam показывается в статус-панели («Steam unavailable: …»).

### Гарантированное закрытие Steam

- `SteamSessionManager.OnApplicationQuit` освобождает транспорт (`SteamAPI_Shutdown`), идемпотентно и с общим путём `Shutdown()` для `OnDestroy`. Steam-клиент больше не считает «Spacewar» запущенной после Stop в Play-режиме редактора и после выхода из собранного билда.

### Проверка

- NUnit `tests\Lan` — 14/14 зелёных (сетевое ядро не менялось).
- Версия в `GameInfo` — `1.0.6` (ProjectSettings синхронизированы: `bundleVersion 1.0.6`, Android `10006`).

## 1.0.5 — Steam co-op (боевая сессия 2 игроков)

Версия 1.0.5 (ветка `steam`) добавляет кооперативную арену над Steam P2P: лобби «Steam Co-op Lobby» на экране выбора кампании, создание/вход по ROOM_ID, синхронизация выбранных героев между игроками и бой 2×4 по старой схеме Photon-режима, но без облачных серверов — напрямую P2P через Steam.

### Сетевое ядро (чистые библиотеки, `src\Lan\`)

- `Sektor.DarkestDungeon.Lan.Contracts` — интерфейс транспорта `ITransport` (сессия = лобби, сообщения = `TransportMessage`), `Result`/`Result<T>`, кодек `ITransportCodec`. Без зависимостей.
- `Sektor.DarkestDungeon.Lan.Steam` — `SteamTransport`: Steam P2P (надёжный канал, channel 1), сессии Steam Lobies, собственный interop-слой (`Interop\`, P/Invoke на flat-API с manual dispatch), rich presence `connect` для входа по приглашению.
- Автодоставка: post-build копирует `*.dll`/`*.pdb` в `Assets\Plugins\Internal\` (вместе с .NET Standard facade-шимами из редактора — см. `COMPABILITY.md`); `steam_api64.dll` — native-плагин `Assets\Plugins\x86_64\`; локальный `steam_appid.txt` (480) для dev-запусков.
- Скрипт доставки: `tools\provision-unity-plugins.ps1`.

### Unity-слой (`Assets\Scripts\Networking\`)

- `SteamRaidBridge` — диспетчер входящих сообщений: RPC-сообщения `rpc.<method>` повторяют вызовы легаси-обработчиков (`PhotonGameManager`), `party_config` — состав отряда соперника.
- `SteamSessionManager` (MonoBehaviour, живёт между сценами) — владеет транспортом, качает колбэки каждый кадр, переводит события сессии в `MultiplayerSync`; при разрыве сессии во время рейда возвращает в `CampaignSelection`.
- `MultiplayerPartyData` — DTO состава отряда (4 героя: класс, имя, сид генерации, флаги выбранных скиллов), сериализуется в строку для канала.
- `MultiplayerSync` — статический фасад для легаси-слоя: в Steam-режиме идёт в Steam, в Photon-режиме — в оригинальные пути; `PreparationCheck`, `SendRpc`, `LoadLevel`, `LeaveRoom`, `HostRaidParty`/`MonsterSideRaidParty`.
- `SteamLauncher` — runtime-панель лобби на `CampaignSelection` (создание/подключение, ROOM_ID, статус, таймаут подключения, переход в `DungeonMultiplayer`). Панель скрыта по умолчанию и открывается по требованию (`OpenPanel`, создаёт лаунчер при отсутствии): в Steam-режиме `RoomSelector.SaveSelectionStart` показывает её поверх экрана выбора кампании. На панели — переключатель провайдера PHOTON/STEAM (в PHOTON возвращаются к `RoomSelector`).
- Устойчивость к недоступному Steam: `SteamTransport` не вызывает нативный Steam API, пока runtime не инициализирован (защита в `RunCallbacks`, `Dispose`, rich-presence/lobby-data путях и всех public-методах — от краша при нулевых указателях интерфейсов); `SteamSessionManager.IsInitialized` отражает реальный успех init; при неудаче панель показывает причину («Steam unavailable: …»), отключает Host/Join, а повторное открытие панели после запуска Steam-клиента переинициализирует транспорт без перезагрузки.
- Панель лобби оформлена по образцу окна настроек (1.0.4): 1024×740 строго по центру экрана, фон `WindowFrame` (`SoundSettingsSprites`), крестик «X» (CloseIcon) в правом верхнем углу; кнопки Return нет — закрытие только крестиком или Escape. Экран выбора кампании при открытии лобби не скрывается, поэтому после закрытия панели меню остаётся на месте без восстановления.
- Переключение провайдера в обе стороны: `MultiplayerMenuState` (`Assets\Scripts\Networking\MultiplayerMenuState.cs`) — синглтон состояния меню мультиплеера (None/Steam/Photon). Из Steam-панели кнопка PHOTON открывает список комнат Photon, а поверх него появляется кнопка «Open STEAM Lobby» (вверху экрана), которая возвращает в Steam-лобби (список комнат закрывается). Выбор провайдера больше не запирает игрока: из любого окна всегда есть видимый переход в другое.

### Легаси-интеграция (минимальные правки)

- `RaidSceneMultiplayerManager`, `BattleGround`, `BattleFormation`/`FormationParty`, `RaidQuestPanel`, `BarkMessenger`, `MainMenuWindow`, `RaidParty`, `Hero`, `RoomSelector` — вызовы Photon заменены на `MultiplayerSync`/строковые имена; Photon-путь сохранён (ветки `MultiplayerSync.IsSteamSession`/`IsSteamProvider`).
- Новые конструкторы `RaidParty(MultiplayerPartyData)` и `Hero(int, MultiplayerPartyData)` рядом с существующими Photon-конструкторами.

### Проверка

- NUnit `tests\Lan` — 14/14 зелёных (кодек, жизненный цикл, round-trip на in-memory транспорте).
- Компиляция Unity 2017.4.40f1 batchmode: `Compilation succeeded`, без ошибок (в т.ч. после фикса краша при недоступном Steam).
- Версия в `GameInfo` — `1.0.5` (ProjectSettings синхронизированы: `bundleVersion 1.0.5`, Android `10005`).

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
