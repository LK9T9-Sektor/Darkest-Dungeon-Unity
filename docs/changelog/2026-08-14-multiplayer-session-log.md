# SESSION LOG: события Photon в лог + крупнее шрифты + единый стиль оверлеев

- События Photon (подключение, комнаты, регион, сбои) теперь пишутся в окно SESSION LOG
  (`MultiplayerSync.WriteLog`/`WriteError`) — «Connected!», «Disconnected!», join/create/fail комнат
  видны прямо в игре, в обоих проектах.
- Шрифты окна SESSION LOG увеличены: тело 18→22, заголовок 24→28, текст лога 17→20; окно расширено.
- Новый core-модуль `src\Core\Ui` (netstandard2.0, C# 7.3, engine-free): `UiStyle` (путь шрифта,
  семантические размеры, цвета `ArgbColor`). DLL доставляется пост-билдом в `Assets\Plugins\Internal`
  обоих проектов.
- Runtime-оверлеи (`MultiplayerLogUI`, `MultiplayerProviderMenu`, `SteamLobbyIdPanel`,
  `SoundSettingsUI`) в обоих проектах унифицированы: читают токены `UiStyle` и используют общий
  `RuntimeUiFactory` (canvas/text/button/EventSystem), убран дублированный код и расходящиеся цвета.
