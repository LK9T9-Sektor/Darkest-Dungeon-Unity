docs/NETWORK_ARCHITECTURE.md

Единый план миграции с Photon на альтернативный сетевой провайдер (например, Steam P2P)
Цель
Заменить Photon на любой другой сетевой провайдер (например, Steam P2P), полностью абстрагировав сетевой слой, чтобы игровая логика не зависела от конкретного транспорта. Поддержка до 8 игроков (1 хост + 7 клиентов).

1. Архитектурный принцип
Сеть выносится в отдельный слой, работающий через единый интерфейс.
Игровая логика общается только с этим интерфейсом, отправляя и получая команды (сериализованные данные).
Конкретная реализация (Photon или альтернативный провайдер, например Steam) подставляется через фабрику в зависимости от выбора или флага сборки.
Сетевой код не управляет состоянием игры – он только доставляет байты.

2. Роль папки /External/
Это база знаний для ИИ-агента, содержащая готовые обёртки над SDK провайдера (например, Steamworks.NET) и примеры работы с ним.
Код из этой папки не компилируется в основную сборку, используется только как референс при написании нового адаптера.

Референс Steamworks.NET 15.0.1 (MIT) лежит в `src/External/Steamworks.NET/`. Он используется как источник точных
сигнатур flat-API и layout'ов callback-структур для собственного interop-слоя (см. раздел 5).

3. Этапы миграции
Этап 1. Инкапсуляция Photon
Все прямые вызовы PhotonNetwork.RPC заменяются на вызовы через обёртку, реализующую интерфейс.
Пока обёртка внутри использует Photon – игра продолжает работать как раньше, но логика уже отвязана от Photon.

Этап 2. Создание адаптера нового транспорта
На основе референсов из /External/ пишется класс P2PTransportAdapter, реализующий тот же интерфейс.
Он использует сеть выбранного провайдера (например, Steam P2P) для отправки/приёма байтовых пакетов.

Этап 3. Фабрика сетевых подключений
В Unity-слое создаётся менеджер, который инициализирует нужный транспорт (Photon или альтернативный, например Steam) по выбору игрока.
Вся остальная работа идёт через интерфейс, без привязки к конкретной реализации.

Этап 4. (Опционально) Удаление Photon
После стабильной работы нового адаптера Photon можно вынести в отдельную библиотеку или полностью исключить из проекта.

4. Итог
Единый интерфейс транспорта обеспечивает независимость от SDK.
Миграция проходит поэтапно, без поломки существующей функциональности.
Папка /External/ – только для справки, код не используется напрямую.

5. Текущая реализация (ветка steam)

Модули (чистый .NET Standard 2.0, C# 7.3):

- `src/Lan/Sektor.DarkestDungeon.Lan.Contracts` — интерфейсы транспорта и wire-контракты
  (`ITransport`, `ITransportCodec`, `TransportMessage`, `Result`/`Result<T>`). Никаких зависимостей.
- `src/Lan/Sektor.DarkestDungeon.Lan.Steam` — Steam P2P транспорт: `JsonTransportCodec` (самодостаточный
  минимальный JSON-кодек без внешних зависимостей) + `SteamTransport` поверх собственного interop-слоя `Interop/`.
- `src/Lan/Sektor.DarkestDungeon.Lan.Cmd` — консольный smoke-клиент: без аргументов интерактивное меню
  (хост / клиент / выход) со входом по Steam-приглашению; с аргументами скриптовый режим `host` / `join <sessionId>`,
  а также вход через `+connect_lobby <sessionId>` (Steam Invite URL).
- `tests/Lan/Sektor.DarkestDungeon.Lan.Tests` — NUnit: кодек, жизненный цикл, round-trip (in-memory транспорт).

Unity-фасад (презентационный слой, `Assets/Scripts/Networking/`, версия 1.0.6):

- `SteamRaidBridge` — диспетчер входящих сообщений: `rpc.<method>` повторяет RPC-вызовы легаси (`PhotonGameManager`), `party_config` — состав отряда соперника.
- `SteamSessionManager` — MonoBehaviour-фасад над `ITransport` (качает колбэки в `Update`), живёт между сценами; `OnApplicationQuit` надёжно освобождает транспорт (`SteamAPI_Shutdown`), чтобы Steam-клиент не считал игру запущенной после выхода.
- `MultiplayerPartyData` — DTO состава (классы, имена, сиды, флаги скиллов), сериализация для канала.
- `MultiplayerSync` — статический фасад для легаси: `IsSteamSession` → Steam, иначе исходные Photon-пути; `EnsureSteamSession()` создаёт/инициализирует `SteamSessionManager`.
- `MultiplayerProviderMenu` — runtime-оверлей выбора провайдера (PHOTON/STEAM) на `CampaignSelection`; крупный шрифт, стрелки ↑/↓ + Enter, мышь. Выбор инициализирует провайдера и открывает общий список комнат `RoomSelector.OpenRoomList()`.
- Общий список комнат `RoomSelector` переиспользует выбор героев (панель отряда) для обоих провайдеров: в Steam-режиме слоты служат вводом lobby ID (подтверждение → `JoinSession`), кнопка Play — хост новой сессии (`HostSession`), `SessionJoined` → отправка состава → загрузка `DungeonMultiplayer`.

Легаси интегрирован минимальными правками через `MultiplayerSync` (см. `CHANGELOG.md` 1.0.5): Photon-путь сохранён, ветвление по `IsSteamSession`.

Почему свой interop-слой, а не Steamworks.NET из NuGet:

- Все версии пакета Steamworks.NET (15.0.1 / 20.x / 2024.x) таргетят netstandard2.1, что несовместимо
  с netstandard2.0 (потолок для Unity 2017.4) — ошибка восстановления NU1202.
- Исходники Steamworks.NET компилируются под netstandard2.0 + C# 7.3 чисто, но по разделу 2 код из
  `src/External` в сборку не попадает.
- Поэтому написан собственный минимальный interop-слой `Interop/`: `SteamNative` (P/Invoke на flat API),
  `SteamEnums`, `SteamConstants`, `SteamCallbackIds`, `SteamCallbacks` (layout'ы структур), `NativeUtf8`,
  `SteamRuntime` (init/интерфейсы/manual-dispatch pump).

Ключевые решения:

- Колбэки получаются через manual dispatch (`SteamAPI_ManualDispatch_*`), как в Steamworks.NET —
  это заменяет `SteamAPI_RegisterCallback`.
- Инициализация — через `SteamInternal_SteamAPI_Init`: поставляемый `steam_api64.dll` от современного
  SDK (1.6x), где `SteamAPI_Init` удалён. Сигнатура и коды `ESteamAPIInitResult` сверены со Steamworks.NET
  2024.8.0. Версии интерфейсов жёстко не фиксируются: каждый интерфейс (ISteamClient, ISteamUser,
  ISteamMatchmaking, ISteamNetworking) резолвится перебором кандидатов (новейший первым), так что
  транспорт работает с разными билдами Steam-клиента (это чинит `VersionMismatch`/`No SteamClient021`).
  Ошибки init пробрасываются наружу как `Result.Failure` с текстом от SDK.
- Сессия = Steam Lobby (тип Public); сообщения — надёжный, упорядоченный P2P-канал
  (`k_EP2PSendReliable`, channel 1).
- При активной сессии публикуется rich presence `connect = steam://joinlobby/<appid>/<lobbyId>`
  (ISteamFriends) — хост помечается «Joinable» в Steam. AppID берётся через `ISteamUtils_GetAppID`,
   не хардкодится. Вход через Steam-приглашение (Join Game / `+connect_lobby`) пока НЕ работает —
   приходится вводить lobby ID вручную (в слот общего списка комнат; см. KNOWN_ISSUES.md §11).
- Идентификатор игрока/сессии — `ulong` steamID как строка; никаких хардкод-AppID (steam_appid.txt).
- Разбор входящих колбэков — через реестр делегатов по callback ID (без switch по идентификаторам).

