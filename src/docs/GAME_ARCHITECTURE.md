# Darkest Dungeon — архитектура игры

Единый обзор Unity-порта Darkest Dungeon: что использует, как устроен код, как работает игра. См. также `KNOWN_ISSUES.md` (архитектурный долг).

## 1. Обзор

- Порт/реконструкция *Darkest Dungeon* на **Unity 2017.4.40f1**, scripting runtime **.NET 4.6** (Experimental; поднят с .NET 3.5/Stable — `scriptingRuntimeVersion: 1`, `apiCompatibilityLevel: 2`).
- Реализация почти идентична оригиналу.
- **Аудио работает**: музыка и звуки воспроизводятся через **FMOD**. Банки FMOD и графические/спрайтовые ассеты исключены из репозитория `.gitignore` из-за большого размера — изучается только код и документация.
- Готово: имение, все герои/монстры, эффекты/статусы, генерация подземелий и квестов, события города, инвентарь/curios, сюжетные карты, озвучка, простой мультиплеер.
- Не готово: часть анимаций, туториал, часть меню (настройки звука реализованы — см. `CHANGELOG.md`).

## 2. Что использует (зависимости)

| Технология | Назначение | Где лежит |
|---|---|---|
| **Photon PUN** (PunBehaviour, PhotonNetwork, RPC) | мультиплеер (лобби, комнаты, синхронизация боя) | `Assets\Photon Unity Networking` |
| **Spine** (Skeleton, Bone, FFD) | 2D-скелетные анимации героев, монстров, дверей, curios/препятствий/ловушек | `Assets\Spine` |
| **FMOD** (FMODUnity) | аудио (события, банки, студийные эммитеры); банки в репозиторий не входят (`.gitignore`) | `Assets\Plugins\FMOD` |
| **Newtonsoft.Json** | парсинг JSON-контента игры | `Assets\Plugins` |
| **System.Xml.Linq** | локализация (XML по языкам) | BCL |
| **UnityStandardAssets.ImageEffects** | пост-эффекты камеры | `Assets\Scripts\ImageEffects` |

Всё содержимое игры — data-driven: читается из `Resources\Data`, префабы и спрайты грузятся через `Resources.Load`.

## 3. Структура кода (`Assets\Scripts`)

Все 485 файлов — обычные MonoBehaviour-скрипты презентационного слоя (домен не вынесен в чистые библиотеки).

| Папка | Ответственность |
|---|---|
| `Campaign` | кампания, имение (`Estate`), здания с апгрейдами, квесты, week log, события города |
| `Character` | герои: `HeroClass`, `Character`, компоненты (атрибуты, статы), статусы, утилиты |
| `Database` | `DarkestDatabase` — мастер-загрузчик контента + типизированные классы данных |
| `Generation` | генерация подземелий (`DungeonGenerator`) и квестов (`QuestGenerator`) |
| `Managers` | корневые менеджеры и сценарии сцен (`DarkestDungeonManager`, `RaidSceneManager`, `EstateSceneManager`, `TownManager`, `ShopManager`, …) |
| `Mechanics` | бой (`BattleGround`, `Round`, `BattleSolver`), AI, 29 классов `Effect`, скиллы |
| `Networking` | Photon-слой: `DarkestPhotonLauncher`, `PhotonGameManager`, `RaidSceneMultiplayerManager` |
| `Setup` | запуск (`GameIntro`, `ScreenLoader`, `GameSetup`) и сохранения (`SaveLoadManager`, DTO) |
| `UI` | ~132 файла: панели, окна, слоты, окна зданий, инвентарь |
| `PlayerInput`, `Sounds` | пустые (только `.meta`) |
| `ImageEffects` | стоковый код Unity (пост-эффекты) |

## 4. Поток запуска

Сцены сборки (в порядке): `Intro` → `CampaignSelection` → `LoadingScreen` → `EstateManagement` → `Dungeon` / `DungeonMultiplayer`.

- `DarkestDungeonManager` — корень `DontDestroyOnLoad`; статические доступы `Data`, `Campaign`, `SaveData`, `ScreenFader`, `LoadingInfo`.
- `GameIntro` (логотипы) → `CampaignSelectionManager` (`SaveSelector` — соло, `RoomSelector` — мультиплеер) → `ScreenLoader` → имение или подземелье.
- Обратно из рейда результаты пишутся через `SaveLoadManager`, затем возврат в имение.

## 5. Загрузка контента (`DarkestDatabase`)

Один MonoBehaviour-загрузчик (~2600 строк): ~30 методов `Load*` читают `Resources\Data` при старте и собирают типизированные классы (`HeroClass`, `MonsterData`, `Quest`, `Effect`, `Buff`, `Curio`, `Trap`, `DungeonEnviromentData`, `TownEvent`, `UpgradeTree`, …).

Форматы данных:
- **Оригинальные DSL-файлы (`.bytes`/`.txt`)** с ручными построчными парсерами:
  - `Heroes\Info\*.bytes` — классы героев (`combat_skill:`, `resistances:`, `weapon:`, `armor:`, `.end`);
  - `Mechanics\Effects.txt` — эффекты (`effect: .name … .target … .chance 100% …`);
  - `Mechanics\MapGenerator.txt` — параметры генерации карт (`.size`, `.base_room_number`, `.connectivity`);
  - `Dungeons\<dungeon>\*` — окружение регионов (комнаты, коридоры, боссы, препятствия, ловушки);
  - `Inventory\Items` — таблица предметов; `Monsters\Skills\Skills_<n>` — скиллы монстров.
- **JSON** (Newtonsoft + POCO в `DarkestJsonReader.cs`, namespace `DarkestJson`): квесты, баффы, черты, реликвии, кемпинг, AI, события города, обмен валют, провизия, таблицы энкаунтеров регионов, озвучка, имена отрядов.
- **CSV**: `Curios\Curios.csv` (15 строк на curio) — результаты взаимодействий.
- **XML**: локализация (`english.xml`, `french.xml`, …) через `LocalizationManager.GetString("str_…")`.

## 6. Кампания и имение

- `Campaign` (233 строки) — текущее состояние кампании; `Estate` — ресурсы, постройки.
- 10 апгрейдируемых зданий: `Abbey`, `Tavern`, `Guild`, `Blacksmith`, `Sanitarium`, `NomadWagon`, `StageCoach`, `Graveyard`, `Statue`, `CampingTrainer` — деревья апгрейдов из JSON, окна в `UI\Windows\Buildings`.
- Week log (недельные записи), события города (хорошие/плохие/нейтральные) с системой требований, генерация списка квестов.

## 7. Генерация подземелий и квестов

- `DungeonGenerator` — сетка, комнаты, связность, пути (BFS `MinPath`).
- `DungeonEnviromentData` — по-регионно (crypts, warrens, weald, cove, darkest, town).
- `QuestGenerator` — цели квестов (исследовать/убить/собрать/активировать/…), сюжетные карты.

## 8. Рейд

- `RaidSceneManager` (~6000 строк) — весь рейд: карта, энкаунтеры, двери, curios/препятствия/ловушки, кемпинг, награды.
- События рейда последовательно разыгрываются корутинами (`StartCoroutine`, ~330 по кодовой базе).

## 9. Бой

- `BattleGround` (поле, позиции), `Round` (инициатива/очередь ходов), `BattleSolver` (правила боя).
- 29 подклассов `Effect` (`BleedEffect`, `PoisonEffect`, `StunEffect`, `GuardEffect`, `Push/PullEffect`, `RiposteEffect`, `StressEffect`, `SummonMonstersEffect`, …), 105+ скиллов из DSL-файлов.
- Статусы: кровотечение, яд, стан, метка, охрана, riposte, DOT; смерть на пороге, стресс, решимость, факел.

## 10. AI

- `MonsterBrain` — данные поведения; желания из `JsonAI`:
  - `Skill Desires` ×11, `Target Desires` ×11, `Bonus Desires` ×7 — скоринг выбора скилла/цели.

## 11. Сохранения

- `SaveLoadManager` (~1860 строк) — бинарные `.sav` (`SaveVersion "1"`), набор DTO (`SaveCampaignData`, `SaveHeroData`, `BattlegroundSaveData`, `RaidPartySaveData`, …).
- Соло: `SaveSelector`/`SaveSlot`; мультиплеер: `RoomSelector`.

## 12. Мультиплеер (Photon)

- `DarkestPhotonLauncher : PunBehaviour` — лобби/комнаты (`MaxPlayersPerRoom = 2`), `PhotonNetwork` RPC.
- `RaidSceneMultiplayerManager : RaidSceneManager` — co-op рейд; сид сессии собирается из ID игроков; сообщения-барки синхронизируются RPC на всех.

## 13. UI

- Панели, слоты, окна (в т.ч. зданий), инвентарь, тултипы (`ToolTipManager`), формирование отряда, магазин провизии (`ShopManager`).
- Настройки звука — runtime-окно `SoundSettingsUI` (`Assets\Scripts\UI\SoundSettingsUI.cs`) со спрайтами из `SoundSettingsSprites` (`Assets\Resources\UI\SoundSettingsSprites.asset`); подробности в `CHANGELOG.md` (1.0.4).

## 14. Версионирование

- **Единственный источник** — `GameInfo` (`Assets\Scripts\Setup\GameInfo.cs`): три константы `Major = 1`, `Minor = 0`, `Patch = 4`. Версия хранится числами, а не строкой — никакого парсинга.
- **Производные (композиция из констант, не парсинг):**
  - `GameInfo.Version` → `"Major.Minor.Patch"` (сейчас `1.0.4`) — Photon отделяет клиентов по версии: `DarkestPhotonLauncher.GameVersion` возвращает `GameInfo.Version` (`Assets\Scripts\Networking\DarkestPhotonLauncher.cs:25`);
  - `GameInfo.AndroidBundleVersionCode` → `Major×10000 + Minor×100 + Patch` (сейчас `10004`, строго растёт).
- **PlayerSettings** синхронизируются автоматически перед каждой сборкой через `IPreprocessBuildWithReport` в `GameInfoVersionSync` (`Assets\Editor\GameInfoVersionSync.cs`): `PlayerSettings.bundleVersion` ← `GameInfo.Version`, `PlayerSettings.AndroidBundleVersionCode` ← `GameInfo.AndroidBundleVersionCode`.
- Текущие значения в `ProjectSettings\ProjectSettings.asset`: `bundleVersion: 1.0.4`, `AndroidBundleVersionCode: 10004`.
- **Как бампить версию:** поменять `Major/Minor/Patch` в `GameInfo` и (для мгновенного обновления в редакторе) выполнить `Tools ▸ Game ▸ Sync Version`.

## 15. Смежные документы

- `CHANGELOG.md` — журнал изменений по версиям (см. `1.0.4` — звуковые настройки).
- `NETWORK_ARCHITECTURE.md` — план миграции сети Photon → альтернативный сетевой провайдер (например, Steam P2P).
- `COMPABILITY.md` — как собирать чистое C# ядро для Unity 2017.4 (netstandard2.0, C# 7.3).
- `KNOWN_ISSUES.md` — архитектурный долг и известные проблемы.
- `RUNTIME_MIGRATION.md` — переход runtime на .NET 4.6: анализ ошибок загрузки контента (`src\issues\Migration-Issues-01.txt`).
