# NETWORK_LAYER_REUSE.md — Переиспользуемый сетевой слой (Steam + Photon)

## 1. Цель

Вынести сетевое ядро в чистые C#-библиотеки (`src\Networking\`, netstandard2.0, C#7.3),
переиспользуемые между играми и движками. Steam и Photon — два взаимозаменяемых
`ITransport`-провайдера с одинаковым поведением. Игра (Darkest Dungeon) не знает ни один
из них — только контракт.

## 2. Решения (зафиксировано)

- **Photon-бэкенд:** движко-агностичный Photon Realtime/LoadBalancing-клиент (чистый C#),
  симметрично Steam.
- **Поведение Photon в игре:** как у Steam — вход/хост по session id. Браузер комнат и
  PUN-флоу удаляются.
- **Объём:** только сетевой слой; чистый C# ядро боевой симуляции — отдельный будущий трек.
- **Проекты:** изменения применяются в **оба** Unity-дерева — активное `unity\` и legacy
  `unity-2017\`.

## 3. Целевая архитектура

```
src\Networking\                       (чистый C#, без Unity/игровой завязки)
├── Contracts\  ITransport · TransportMessage · ITransportCodec · JsonTransportCodec · Result · TransportSettings
├── Steam\      SteamTransport (P/Invoke steam_api64.dll)
└── Photon\     PhotonTransport : ITransport (Photon Realtime)

доставка: post-build → unity\Assets\Plugins\Internal И unity-2017\Assets\Plugins\Internal

Игра (Unity, оба дерева):
  MultiplayerSync → SessionManager (generic, владеет ITransport)
  wire-протокол ("rpc.*", "party_config") владеет игра; один код для обоих провайдеров
```

### Маппинг `ITransport` → Photon

| ITransport | Photon |
|---|---|
| `CreateSession(name, max)` | создать комнату |
| `JoinSession(sessionId)` | войти в комнату по имени (session id) |
| `SendMessage(type, payload)` | reliable `RaiseEvent` (ReceiverGroup.Others) |
| `GetSessionPlayers` | акторы комнаты (без локального) |
| `LocalPlayerId/HostPlayerId` | actor № / master client |
| `RunCallbacks` | `Service()` + разбор `OnEvent` |
| `Disconnected` | `OnDisconnected` |

## 4. Фазы

### 0. Спайк: Photon-клиент под netstandard2.0

- Выбрать движко-агностичный клиент (NuGet/архив SDK). Проверить `dotnet restore` под
  netstandard2.0 + C#7.3 (риск NU1202 — как Steamworks.NET, `KNOWN_ISSUES.md §9`).
- Фолбэк: вендорить источники и компилировать в 7.3, либо собственный тонкий слой
  (образец — interop Steam).

### 1. Ренейм в общий неймспейс

- `Sektor.DarkestDungeon.Lan.*` → `Sektor.Networking.*`; папки `src\Lan`→`src\Networking`,
  `tests\Lan`→`tests\Networking`.
- `JsonTransportCodec` — из Steam в Contracts. Обновить `using` в Unity-слое (оба дерева)
  и в доках.

### 2. Contracts: малые расширения

- `TransportSettings` (constructor DI): AppId/регион Photon; Steam AppID — из клиента/
  `steam_appid.txt` (как сейчас). Больше ничего (KISS).

### 3. `PhotonTransport : ITransport` (src\Networking\Photon)

- Маппинг выше; тот же кодек; `Result`, без исключений; надёжный/упорядоченный канал
  (соответствие контракту `TransportMessage`).

### 4. Unity-слой: единый путь (оба дерева)

- `SteamSessionManager` → generic `SessionManager` + `TransportFactory.Create("steam"|"photon", settings)`.
- Вынос общего состояния из `PhotonGameManager` → нейтральный `MultiplayerRaidController`
  (`BarkMessages`, `PlayersPreparedCount`, `SkipMessagesOnClick`, хендлеры).
  `SteamRaidBridge` → `RaidBridge`.
- `DarkestPhotonLauncher` → `MultiplayerLobbyController` (пул героев, панель отряда,
  `CheckSelectedSkills`, `GameVersion`) — без `PunBehaviour`.
- `RoomSelector`/`MultiplayerRoomSlot`: единый флоу host + session id; PUN-браузер и
  `ConnectToRegion` выпиливаются.
- `Hero`/`RaidParty`: конструкторы от `PhotonPlayer` убираются.
- Удаление PUN (оба дерева): `Photon Unity Networking`, `Photon3Unity3D.dll`,
  `PhotonServerSettings`, PUN-колбэки из `SaveSelector`/`PlayerNicknameInputField`.

### 5. Тесты

- `tests\Networking`: переименование + `PhotonTransportTests` (host/join, lifecycle,
  round-trip, порядок) на in-memory/fake-клиенте. `dotnet test`.

### 6. Сборка/проверка

- `dotnet build` + `dotnet test`; `pwsh tools\compile-check.ps1` (проект закрыт;
  `-Provision` для плагинов); опционально `build-game.ps1`.

### 7. Документация (один коммит с кодом)

- Обновить: `NETWORK_ARCHITECTURE.md`, `ARCHITECTURE.md`, `KNOWN_ISSUES.md`,
  `CHANGELOG.md` (user-visible), ссылки на `src\Lan\` → `src\Networking\`.
  Коммиты на английском, с точкой.

## 5. Открытые моменты

1. **Пакет Photon:** официальный NuGet (Realtime) предпочтителен; если есть локальный SDK —
   уточнить на спайке.
2. **`Cmd`-консоль:** оставить `src\Networking\Cmd` как generic smoke-клиент (по умолчанию)
   или вынести в `samples\`.
3. **Порядок фазы 4 для двух деревьев:** сначала активное `unity\`, затем зеркально
   `unity-2017\` (легаси-дифф минимален по форме, но изменения там идентичны по функциональности).
