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

## 6. Детерминированный локстап (модель сетевого боя)

Мультиплеерный бой (Steam, сцена `DungeonMultiplayer`) построен как **детерминированный локстап**:
каждая сторона запускает полную боевую симуляцию локально, по сети передаются **только вводы**
и состав отряда. Состояние (HP, стресс, позиции, баффы) не синхронизируется — обе стороны считают
его одинаково и сходятся. Реализация: `RaidSceneMultiplayerManager` + `MultiplayerSync` +
`SteamRaidBridge` + `RandomSolver`.

### Детерминизм

- **Единый глобальный сид RNG.** Все роллы (урон, крит, додж, резисты, стресс, инициатива, решения
  AI) идут из одного потока `RandomSolver` (обёртка над `System.Random`); `SetRandomSeed`
  заменяет поток целиком.
- **Сид сессии** — детерминированный: для каждого id из упорядоченного `PlayerIds` (локальный
  первым, затем соперники) делается `SetRandomSeed(StableHash(id))`, затем
  `sessionSeed += Next(2^16)`; в конце `SetRandomSeed(sessionSeed)`
  (`RaidSceneMultiplayerManager.Awake`).
- **Генерация героев из персональных сидов.** В лобби каждый герой получает сид; он уходит
  сопернику в `party_config` (`MultiplayerPartyData.Seeds`) или в Photon custom properties
  (`HS1..4`). Обе стороны регенерируют одинаковых героев из тех же сидов (конструктор `Hero`).
- **Детерминированные `CombatId`** 1..8 (герои 1–4, враги 5–8) в одинаковом порядке на обеих
  сторонах; именно они — ключи целей на проводе.
- **Стороны**: герои = отряд хоста, враги = отряд соперника; обе стороны строят формирования
  одинаково.

### Wire (только вводы)

- `party_config` — состав отряда: `класс|имя|сид|флаги скиллов` × 4 слота.
- `rpc.<method>` — вводы: `PlayerLoaded` (барьер готовности), `HeroSkillSelected(slot)`,
  `HeroSkillButtonClicked(combatId)`, `HeroMoveButtonClicked(combatId)`,
  `HeroPassButtonClicked`, `HeroMoveSelected`/`HeroMoveDeselected`, `ExecuteBarkMessage(team,text)`.
- Отправка — «всем + локально» (семантика `PhotonTargets.All`): действие выполняет и отправитель.
- **Право ввода**: host действует за `Team.Heroes`, клиент за `Team.Monsters`; ввод транслируется,
  обе стороны применяют его и продолжают симуляцию.

### Победа

Сообщения game-over на проводе **нет**: обе стороны локально достигают одинакового финала
(`BattleStatus.Finished` при уничтожении формирования); кто победил — вычисляется локально.

### Следствие для других клиентов

Новый клиент (WPF-дуэль, `FEATURE_DESKTOP_CLIENT.md`) обязан воспроизвести те же правила сида
и разрешения боя, чтобы стороны сходились; транспорт и словарь сообщений переиспользуются
из `src\Lan` (см. `EXTRACTION_PLAN.md` Фаза 3).

## 7. Смежные документы

`NETWORK_RATIONALE.md` (почему байтовый транспорт) · `NETWORK_LAYER_REUSE.md` (план: ренейм, Photon,
session-id, удаление PUN) · `FEATURE_COOP.md` / `FEATURE_GAME_MODES.md` (сессии до 8, арена) ·
`ARCHITECTURE.md` (слои, швы выноса).
