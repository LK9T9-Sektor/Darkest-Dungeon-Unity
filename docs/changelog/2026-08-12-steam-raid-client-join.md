# Надёжный вход клиента в Steam-рейд

- Загрузка сцены `DungeonMultiplayer` теперь ждёт готовности соперника: и хост, и клиент не входят в рейд, пока не получен состав отряда другого игрока (`party_config`) — `RoomSelector.LoadSteamRaidSceneRoutine` ожидает `MultiplayerSync.HasRivalParty` (таймаут 30 с: статус «Opponent did not join in time» и выход из сессии). Раньше клиент мог попасть в пустую «дефолтную» сцену рейда, если состав хоста ещё не дошёл до момента `RaidSceneMultiplayerManager.Awake`, а хост входил в рейд один и сразу «завершал» квест (`PlayerCount < 2`).
- Защитный фолбэк в `RaidSceneMultiplayerManager.Awake`: если состав соперника отсутствует на старте рейда, отряд строится из локального состава, чтобы сцена не оставалась пустой.
