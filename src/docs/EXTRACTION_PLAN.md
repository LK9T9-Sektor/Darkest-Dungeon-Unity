# EXTRACTION_PLAN.md — Вынос в чистое ядро (основная задача)

Коммитимый план реорганизации: **общее чистое C#-ядро (`src\Core\`) + чистая сеть (`src\Networking\`) +
два Unity-проекта как тонкая презентация**. Доменная логика выносится из презентационного слоя в чистые
библиотеки. «Хотелки» (кооп, режимы, десктоп-клиент и пр.) — **не здесь**, а в `FEATURE_*.md`: они
опциональны, частичны и встраиваются между этапами, если код позволяет.

Подробности по коду — в `ARCHITECTURE.md`, долг — в `KNOWN_ISSUES.md`.

## 1. Текущее состояние

- `Assets\Scripts` — ~496 MonoBehaviour-файлов в каждом из двух деревьев; весь домен (бой, кампания,
  данные) всё ещё в презентационном слое.
- `src\` — сетевой слой `Lan\` (Contracts/Steam/Cmd), netstandard2.0, C# 7.3; DLL доставляются
  пост-билдом в `Assets\Plugins\Internal` обоих деревьев. `src\Core\` и `src\Networking\` — целевые.
- `src\Core\Content\` — данные контента (Фаза 1, в работе): `Raid\` (пропы/курio: `Prop`,
  `Curio`, `CurioResult`, `IProportionValue`, `AreaType`), `Campaign\` (модели `HeirloomExchange`,
  `PartyNameEntry`, `NarrationEntry`/`NarrationAudioEvent`), `Save\` (бинарный интерфейс
  `IBinarySaveData` — старт Фазы 2), `Database\` (DTO `Json*` и мапперы-парсеры: `CurioCsvParser`
  для CSV, `NarrationMapper` для JSON). Ядро **не зависит от Newtonsoft**: DTO-члены названы
  snake_case в точности по legacy-JSON, поэтому десериализуются любой версией Newtonsoft без
  атрибутов. Причина: сборки Newtonsoft 11/12/13 (включая `net45`/`netstandard2.0`) ссылаются на
  контрактные сборки net6.0 (`System.Runtime, Version=6.0.0.0`) и не читаются компилятором Unity
  2017.4 (`CS0009`). JSON-десериализация пока остаётся на границе презентации
  (`JsonDarkestDeserializer.GetJsonObject<T>`); CSV-парсер — чистый, в ядре.
- `src\Core\Ui\` — презентационные токены runtime-оверлеев (engine-free): путь шрифта,
  семантические размеры текста и цвета (`ArgbColor`), доставляются DLL в оба проекта; Unity-side
  конструктор (`RuntimeUiFactory`) дублируется в деревьях и читает токены из ядра. Единый источник
  стилей для `MultiplayerLogUI`, `MultiplayerProviderMenu`, `SteamLobbyIdPanel`, `SoundSettingsUI`.
- `tests\Lan\` — NUnit-тесты сетевого слоя; `tests\Core\` — NUnit-тесты ядра контента (на реальных данных).

## 2. Целевая раскладка

```
repo/
├── AGENTS.md            # карта-манифест, правила
├── src/
│   ├── Core/            # домен: Common, Content, Save, Combat, Campaign, Modes
│   ├── Networking/      # транспорт: Contracts, Steam, Photon (из Lan\)
│   ├── External/        # вендоренный референс (read-only)
│   └── docs/
├── content/             # общие ресурсы-данные (трекаются): контент, локализация (см. FEATURE_SHARED_ASSETS)
├── assets/              # тяжёлые ассеты (локально, gitignored): spine, спрайты, аудио
├── tests/               # NUnit, зеркалит src/
├── unity/               # АКТИВНАЯ версия (Unity 6000); имя без версии
├── unity-2017/          # Unity 2017.4 (легаси, активная поддержка)
├── tools/               # с параметром -ProjectPath
└── Darkest-Dungeon-Unity.slnx       # единое решение (целевое)
```

Ключевые решения:

- Оба проекта активно поддерживаются; общий источник истины для домена — `src\Core\`, для сети —
  `src\Networking\`.
- Модуль = папка = namespace; god-классы не плодить. Легаси-файлы не удаляются, пока ядро не станет
  источником истины (минимальный дифф).
- `unity\` без номера версии — переживёт переход на Unity 7; `unity-2017\` наполняется вручную из ветки
  `steam` (2017.4.40f1 + Steam co-op).

## 3. Фазы выноса

**Сквозные правила:** оба дерева поддерживаются постоянно. Каждый этап: извлечь → NUnit-тесты →
адаптеры в обоих проектах → **швы выноса** (см. `ARCHITECTURE.md` §4) → compile-check активного `unity\`
(легаси `unity-2017\` — по возможности, иначе проверку проводит человек) → доки → коммит.

- **Фаза 0. Монорепо.** *(готово)* Перенос в `unity\`, `.gitignore`, тулзы `-ProjectPath`, доставка ядра
  в оба проекта. Не завершено: единое решение `Darkest-Dungeon-Unity.slnx`.
- **Фаза 0б. Фундамент** → `src\Core\Common` (netstandard2.0, C# 7.3): единый `Result`/`Result<T>`,
  примитивы (`IntVector2`, `IRng`), `InvariantCulture`, модель feature-flag; спайк Photon-клиента под
  netstandard2.0 (риск NU1202 — как Steamworks.NET, `KNOWN_ISSUES.md` §9); `tools\build-core.ps1`;
  `tools\sync-assets.ps1` (см. `FEATURE_SHARED_ASSETS.md`).
- **Фаза 1. Данные** → `src\Core\Content`: модели контента + парсеры (JSON/DSL/CSV/XML) из
  `DarkestDatabase`; `InvariantCulture`; `DarkestDatabase` → тонкий загрузчик; тесты на реальных данных.
  *(в работе)* Вынесен первый срез: `HeirloomExchange` + `PartyNames` (модели `Campaign\`, DTO
  `Json*` и мапперы `Database\`), доставка DLL в оба проекта, NUnit-тесты на реальных JSON. Ядро —
  чистое netstandard2.0 **без Newtonsoft**: DTO-члены snake_case по legacy-JSON (без `[JsonProperty]`),
  десериализация остаётся в адаптере презентации (`GetJsonObject<T>`), где Newtonsoft 4.0.2.0
  (оба проекта) мапит их напрямую. Следующий шаг: перенести `JsonConvert` в ядро и десериализовать
  JSON напрямую в модели — но только после того, как Unity-проекты получат Newtonsoft, читаемый
  компилятором 2017.4 (сейчас сборки Newtonsoft 11/12/13 ссылаются на контракты net6.0 и дают
  `CS0009` в 2017.4; см. `KNOWN_ISSUES.md` §13).
- **Фаза 2. Сейвы** → `src\Core\Save`: DTO + бинарный кодек + версии; IO в Unity через `ISaveStorage`.
- **Фаза 3. Бой** → `src\Core\Combat`: `BattleGround`/`Round`/`BattleSolver`/Effects/AI как чистая
  симуляция; архитектура — **симуляция + события для view** (решение принято). Кооп-PvE строится на
  результате этой фазы (см. `FEATURE_COOP.md`).
- **Фаза 4. Кампания** → `src\Core\Campaign`: имение, здания, квесты, week log, события города.
- **Фаза 5. Сеть** → `src\Networking` (Steam + Photon) по `NETWORK_LAYER_REUSE.md`: ренейм
  `Sektor.Networking`, `PhotonTransport`, generic `SessionManager`/`RaidBridge`, единый session-id флоу,
  удаление PUN из обоих проектов.
- **Фаза 6. Презентация** — только тонкие MonoBehaviour-адаптеры + UI; расхождения между проектами —
  только внутри Unity-кода (API Unity 2017.4 vs 6000).

## 4. `.gitignore`

Почти все правила уже «глубинные» (матчатся на любой глубине) и покроют и `unity\`, и `unity-2017\`.
Требуют правки (снятие якоря на корень): `/[Uu]serSettings/` → `[Uu]serSettings/`,
`!/[Aa]ssets/**/*.meta` → `![Aa]ssets/**/*.meta`, `/[Aa]ssets/AssetStoreTools*` → `[Aa]ssets/AssetStoreTools*`,
+ `UnityPackageManager/`. Плюс: `content\` — трекается, `assets\` и копии ассетов в проектах — игнорируются.

## 5. Инструменты

- `tools\*.ps1` получают параметр `-ProjectPath` (по умолчанию `unity\`): compile-check, сборка, запуск,
  провижнинг.
- `provision-unity-plugins.ps1` собирает ядро и доставляет DLL/PDB в `Assets\Plugins\Internal` обоих
  проектов; фасады .NET Standard — только для 2017-редактора.
- `sync-assets.ps1` — зеркалит общие ресурсы в оба проекта (см. `FEATURE_SHARED_ASSETS.md`).
- `build-core.ps1` — сборка + тесты всех `src\Core`/`src\Networking`.
- Единое решение `Darkest-Dungeon-Unity.slnx` — целевое; `dotnet build` по решению не выполняется
  (Unity-проекты требуют редактор), сборка ядра точечная по проектам `src\`.

## 6. Чтение для агентов

- Карта-манифест в `AGENTS.md`: домен — `src\Core\<модуль>`, презентация — `<проект>\Assets\Scripts\<область>`.
- Модуль = папка = namespace → поиск через glob/grep предсказуем.
- Документы: `INDEX.md` (карта), `ARCHITECTURE.md` (структура), `KNOWN_ISSUES.md` (долг),
  `CHANGELOG.md` (изменения), `FEATURE_*.md` (хотелки), `UNITY_MIGRATION.md` (переходы редактора).
