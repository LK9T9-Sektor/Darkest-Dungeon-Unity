# Darkest Dungeon — известные проблемы и архитектурный долг

Заметки для будущего рефакторинга. Детали по коду — в `ARCHITECTURE.md`.

## 1. Синглтоны вместо DI

- ~13 статических синглтонов с опечаткой `Instanse` (503 вхождения в 64 файлах) + паттерн `Awake { if (Instanse == null) … else Destroy(gameObject) }`, напр. `RaidSceneManager.cs:16`.
- Нет внедрения зависимостей: код тянет состояние через статики (`DarkestDungeonManager.Data/Campaign`, `RaidSceneManager.Instanse`).

## 2. Год-классы

| Файл | Строк | Роль |
|---|---|---|
| `Managers\RaidSceneManager.cs` | ~6000 | весь рейд + бой + кемп + события |
| `Database\DarkestDatabase.cs` | 2600 | вся загрузка контента |
| `Setup\SaveSystem\SaveLoadManager.cs` | ~1860 | бинарные сейвы + DTO |
| `Character\Character.cs` | 1223 | герой |

## 3. Слабая событийная связь; весь игровой флоу на корутинах

- C#-`event` есть только в UI-слое (панели/слоты/окна, `DragManager`, `ScreenFader`). Доменный флоу (рейд, бой, кампания) связывается прямыми вызовами + корутинами (~330 `StartCoroutine`).
- Игровые сценарии последовательно разыгрываются корутинами — сложно отлаживать, тестировать и повторно использовать.

## 4. Магические строки

- Имена сцен (`GameIntro.cs:34`, `ScreenLoader.cs:22/27`), пути данных, ID квестов/регионов.
- Многочисленные хардкод-пути к `Resources\Data` и `Prefabs\…`.

## 5. Мультиплеер: нестабильный сид и хардкод

- Сид сессии = `player.ID + player.ToString().GetHashCode()` (`RaidSceneMultiplayerManager.cs:32`) — нестабилен между запусками (GetHashCode не гарантирован).
- Хардкод тестовых данных: `new SaveCampaignData(4, "MultiplayerTestSave")` (`:24`).
- `RaidManager.cs:20-27` — «быстрый старт»-хак.

## 6. Мёртвый код и пустышки

- `PrivilegedStarter.cs` — пустой стаб; `GameSetup.cs` — почти полностью закомментирован.
- Папки `PlayerInput` и `Sounds` пусты (только `.meta`).
- Опечатка `QuestsComleted` в `Campaign.cs:6`.

## 7. Расхождение с AGENTS.md

- Основной домен всё ещё лежит в презентационном слое (`Assets\Scripts`); чистый C# ядро существует пока только
  для сетевого слоя (`src\Lan\`): интерфейсы, `Result`-типы вместо исключений, NUnit-тесты, пост-билд доставка
  DLL в `Assets\Plugins\Internal` (см. `NETWORK.md` §4).
- Сетевые контракты компилируются под netstandard2.0; основной игровой код — нет.
- Доставка в `Assets\Plugins\Internal` пост-билдом копирует собранные DLL/PDB (см. `EXTRACTION_PLAN.md` §5).
  .NET Standard facade-шимы (`COMPABILITY.md` §1) требовались старому Mono Unity 2017.4; после перехода на
  Unity 6000.5.8f1 фасады не нужны (нативный type-forwarding) — `tools\provision-unity-plugins.ps1` их
  пропускает. Доставка идёт автоматически: `-UnityEditorPath` → `UNITY_EDITOR_PATH` → `editors.json` Unity Hub
  → типовые каталоги установки; собранные DLL и `steam_appid.txt` остаются gitignored.

## 8. Культуро-зависимый парсинг чисел

- `float.Parse`/`float.TryParse`/`int.Parse`/`Convert.To*` вызываются без `CultureInfo.InvariantCulture` — **164 места** в `Assets\Scripts`.
- После перехода runtime на .NET 4.6 (Mono берёт OS-локаль, десятичный разделитель `,`) загрузка контента падает: `FormatException` в `HeroClass.cs:111` + каскад NRE/KeyNotFoundException. Полный разбор — в `RUNTIME_MIGRATION.md`.

## 9. Steamworks.NET несовместим с netstandard2.0

- Все версии NuGet-пакета Steamworks.NET таргетят netstandard2.1 — не восстанавливаются в проектах
  netstandard2.0 (потолок Unity 2017.4): NU1202. Поэтому Steam-транспорт использует собственный interop-слой
  (`src\Lan\Sektor.DarkestDungeon.Lan.Steam\Interop\`), написанный по референсу `src\External\Steamworks.NET`
  (15.0.1). Обновление SDK-обёрток/структур — вручную, по тому же референсу.
- Поставляемый `steam_api64.dll` — от современного SDK (1.6x), в котором удалён `SteamAPI_Init`. Инициализация
  идёт через `SteamInternal_SteamAPI_Init` (сигнатура и коды `ESteamAPIInitResult` сверены со Steamworks.NET
  2024.8.0), версии интерфейсов (`SteamClient021`, `SteamUser023`) — с экспортами/строками самого бинарника.

## 10. Прочее

- `ImageEffects` — стоковый код Unity, не домен.

## 11. Сейв-экран в Unity 6: именование/удаление/загрузка сейва

- Дедлок при именовании нового сейва: `SaveNamingStart` отключал именуемый слот
  (`DisableInteraction`), а `titleInput` живёт внутри кнопки Save → ввод не работал,
  `onEndEdit` не срабатывал, слоты и крестик удаления блокировались навсегда.
  Частично исправлено в `b16ea18` (именуемый слот остаётся активным, пустое имя
  разблокирует слоты, Escape отменяет именование, `ActivateInputField`).
  Подробности и чек-лист проверки — `ISSUE_SAVE_UI.md`.
- `GetVirtualKey: Could not map char` — старый Input Manager не мапит вводимые
  символы (кириллица/раскладка); возможно, отдельный issue.

## 12. Steam-приглашение (Join Game / +connect_lobby) не срабатывает

- Транспорт, лобби, P2P-канал и rich presence работают; вход по ROOM_ID проверен. Не работает именно
  получение приглашения со стороны Steam: клиент не получает `GameLobbyJoinRequested` (315), кнопка
  Join Game у хоста не срабатывает, `+connect_lobby <id>` не приводит к подключению.
- Приходится вручную вводить lobby ID (в слот общего списка комнат в Steam-режиме) — это текущий рабочий
  способ подключения клиента. После создания сессии ID хост видит в интерфейсе: панель `SteamLobbyIdPanel`
  в списке комнат и в подземелье с кнопкой копирования в буфер обмена (раньше ID выводился только в логе).
- Вероятные причины (требуют дальнейшего разбора): приложение не зарегистрировано как Steam-игра
  (steam_appid.txt локальный, не поставляется в сборке), связка AppID ↔ аккаунт, обработка
  `steam://joinlobby/` самим клиентом Steam.
