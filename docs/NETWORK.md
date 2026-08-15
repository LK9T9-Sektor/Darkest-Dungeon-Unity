# NETWORK.md — Сетевой слой: ответственность и текущий факт

Сущность «сеть»: байтовые транспорты и игровой фасад над ними. План развития — `NETWORK_LAYER_REUSE.md`,
обоснование — `NETWORK_RATIONALE.md`, session-модель/хотелки — `FEATURE_COOP.md` и `FEATURE_GAME_MODES.md`.

## 1. Принцип

- Сеть вынесена в отдельный слой за единым интерфейсом `ITransport`. Игровая логика общается только с
  интерфейсом, отправляя и получая сообщения (`TransportMessage`). Транспорт **только доставляет байты**
  и не владеет состоянием игры.
- Конкретный провайдер (Steam, Photon) подставляется фабрикой; игра не ветвится на провайдера напрямую
  (в Unity-слое — через статический фасад `MultiplayerSync`).

## 2. Ответственности

| Компонент | Ответственность |
|---|---|
| `ITransport` (Contracts) | Сессии (создать/вступить/покинуть), надёжная упорядоченная доставка сообщений, `RunCallbacks`-помп, идентификаторы игроков/хоста |
| `ITransportCodec` | Сериализация сообщения в текст (JSON-кодек) |
| `SteamTransport` | Steam-лобби как сессии + P2P-канал (reliable, channel 1); interop-слой |
| `PhotonTransport` | То же над Photon Realtime (планируется, Фаза 5) |
| Unity-фасад (`Scripts\Networking\`) | `SessionManager` (владеет транспортом, качает колбэки каждый кадр), `RaidBridge` (wire-протокол `rpc.*`/`party_config`), `MultiplayerSync` (фасад для легаси) |

## 3. Семантика `ITransport`

- Сессия = лобби/комната. `CreateSession(name, maxPlayers)` / `JoinSession(sessionId)` / `LeaveSession()`.
- Сообщения — `SendMessage(type, payload)`; надёжные и упорядоченные.
- События: `SessionJoined`, `PlayerJoined`/`PlayerLeft`, `MessageReceived`, `SessionInviteReceived`, `Disconnected`.
- Ошибки — `Result`/`Result<T>`, без исключений.

## 4. Текущий факт

- Чистые библиотеки `src\Lan\Sektor.DarkestDungeon.Lan.Contracts` + `.Steam` (netstandard2.0, C# 7.3);
  доставляются пост-билдом в `Assets\Plugins\Internal` обоих деревьев.
- Steam: P/Invoke на flat-API с manual-dispatch (`Interop\`), `SteamInternal_SteamAPI_Init`, версии
  интерфейсов перебираются (SteamClient021/SteamUser023 и т.д.), AppID не хардкодится
  (`steam_appid.txt`/клиент). Вход по session id работает; вход по Steam-приглашению пока нет
  (`KNOWN_ISSUES.md` §11).
- Unity-фасад (версия 1.0.6): `SteamSessionManager`, `SteamRaidBridge`, `MultiplayerSync`,
  `MultiplayerPartyData`, `MultiplayerProviderMenu`, `SteamLobbyIdPanel`.
- Легаси Photon-путь (`DarkestPhotonLauncher`/`PhotonGameManager`) сохранён, ветвление —
  `IsSteamSession`/`IsSteamProvider`; выпиливается в Фазе 5 (`NETWORK_LAYER_REUSE.md`).

## 5. Смежные документы

`NETWORK_RATIONALE.md` (почему байтовый транспорт) · `NETWORK_LAYER_REUSE.md` (план: ренейм, Photon,
session-id, удаление PUN) · `FEATURE_COOP.md` / `FEATURE_GAME_MODES.md` (сессии до 8, арена) ·
`ARCHITECTURE.md` (слои).
