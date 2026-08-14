# ARCHITECTURE.md — Слои, модули и ответственности

Тонкая карта архитектуры: слои, ответственности модулей, инварианты и «швы выноса», версионирование.
Известные проблемы — в `KNOWN_ISSUES.md`, фазы выноса — в `EXTRACTION_PLAN.md`, сетевой слой — в `NETWORK.md`.

## 1. Слои

| Слой | Расположение | Ответственность |
|---|---|---|
| **Чистое ядро (домен)** | `src\Core\` | Игровая логика без движка/UI: данные, бой, кампания, режимы, общее |
| **Сеть** | `src\Networking\` | Байтовые транспорты (Steam, Photon) и контракты; без игровой логики |
| **Презентация** | `unity\Assets\Scripts`, `unity-2017\Assets\Scripts` | Тонкие MonoBehaviour-адаптеры, UI, движковые интеграции (FMOD, Spine, Resources) |

Проекты: `unity\` — активный (Unity 6000), `unity-2017\` — легаси (Unity 2017.4). Оба поддерживаются
постоянно и потребляют одни и те же ядро/сеть. Расхождения между деревьями — только внутри Unity-кода.

## 2. Модули и ответственности

### Чистое ядро (`src\Core\`)

| Модуль | Ответственность |
|---|---|
| `Common` | Общие типы: `Result`/`Result<T>`, примитивы (`IntVector2`, `IRng`), утилиты `InvariantCulture`, модель feature-flag |
| `Content` | Классы данных контента + парсеры (JSON/DSL/CSV/XML), валидация на загрузке. Раскладка зеркалирует значимые legacy-папки: `Raid\` — пропы/курio (`Prop`, `Curio`, `CurioResult`, `IProportionValue`, `AreaType`), `Campaign\` — модели (`HeirloomExchange`, `PartyNames`, `NarrationEntry`/`NarrationAudioEvent`), `Save\` — бинарный интерфейс `IBinarySaveData`, `Database\` — DTO и мапперы-парсеры (`CurioCsvParser` — CSV, `NarrationMapper`/`LootMapper` — JSON). Ядро **не зависит от Newtonsoft**: DTO-члены названы snake_case по legacy-JSON (без `[JsonProperty]`), десериализация — в адаптере презентации любой версией Newtonsoft. Причина: сборки Newtonsoft 11/12/13 ссылаются на контракты net6.0 и не компилируются в Unity 2017.4 (`CS0009`; см. `KNOWN_ISSUES.md` §13). Рукописные мапперы — переходное состояние; целевое — прямая десериализация Newtonsoft в ядре после перехода Unity-проектов на совместимый Newtonsoft |
| `Ui` | Презентационные токены runtime-оверлеев (engine-free): путь шрифта, семантические размеры текста и цвета (`ArgbColor`), потребляются обоими Unity-проектами через DLL. UI-конструктор (`RuntimeUiFactory`) остаётся Unity-side и дублируется в деревьях; стили — единый источник в ядре |
| `Save` | DTO, бинарный кодек, версии; IO — через `ISaveStorage` |
| `Combat` | Боевая симуляция (по Фазе 3): правила, ходы, эффекты, AI; события/команды наружу |
| `Campaign` | Кампания/имение, здания, квесты, week log, события города |
| `Modes` | Режимы: конфиг режима, нодовая карта, состояние забега |

### Сеть (`src\Networking\`)

| Модуль | Ответственность |
|---|---|
| `Contracts` | `ITransport`, `ITransportCodec`, `TransportMessage`, `TransportSettings` |
| `Steam` | `SteamTransport` поверх Steam P2P/лобби (interop-слой) |
| `Photon` | `PhotonTransport` поверх Photon Realtime (планируется, Фаза 5) |

### Презентация (Unity, оба дерева)

| Область | Ответственность |
|---|---|
| `Scripts\Networking\` | Фасад `MultiplayerSync`, `SessionManager`, `RaidBridge` — игровой glue над `ITransport` |
| `Scripts\UI\` и др. | Панели, окна, слоты, вьюхи; рендер по состоянию/событиям ядра |
| Интеграции | FMOD (аудио), Spine (анимации), Resources.Load (контент), префабы/сцены |

## 3. Инварианты (не-негот-прайс)

- Чистое ядро: **netstandard2.0, C# 7.3**, ноль ссылок на UnityEngine/движок/UI.
- Ошибки через `Result`/`Result<T>`; ядро не бросает исключений для бизнес-ошибок.
- **Строковые Id** вместо enum; контент валидируется на загрузке, а не в рантайме.
- Полиморфизм вместо enum/switch для ветвления поведения; данные-ресурсы (OCP).
- Constructor DI: без синглтонов/скрытых статик/хардкод-`new` в логике.
- Богатая доменная модель: состояние меняется только через валидирующие методы; DTO — исключение.
- Один публичный тип на файл; `_camelCase` поля; `nameof`; XML-доки на public; KISS/YAGNI.

## 4. Швы выноса (seams)

Обязательные границы, которые **каждый этап выноса обязан строить** — чтобы будущее выравнивание под
целевой фреймворк пошаговых игр и любые «хотелки» было дешёвым:

1. Ошибки — через `Result`/`Result<T>`, ядро без исключений.
2. Строковые Id; контент валидируется на загрузке (валидатор-паттерн).
3. Детерминированный RNG (общий сид) — фундамент сети/реплеев/коопа.
4. Логика без ссылок на движок/UI: наружу — состояние + доменные события; механизм отрисовки
   (очередь анимаций, снапшоты) отдаётся тонкому движку/UI (Unity, WPF и т.п.).
5. Данные-ресурсы вместо switch/enum (новый эффект/стат/статус = новое определение).
6. DI через конструкторы.

Это формула «чистота = гибкость под хотелки»: вынос изначально идёт в форме фреймворка.

## 5. Версионирование

- Источник — `GameInfo` (`Assets\Scripts\Setup\GameInfo.cs`): `Major = 1`, `Minor = 0`, `Patch = 6`.
- `GameInfo.Version` → `"Major.Minor.Patch"`; `GameInfo.AndroidBundleVersionCode` → `Major×10000+…`.
- `GameInfoVersionSync` (`Assets\Editor\GameInfoVersionSync.cs`) синхронизирует `PlayerSettings` перед сборкой.
- Бампить: `Major/Minor/Patch` в `GameInfo` → `Tools ▸ Game ▸ Sync Version`.

## 6. Смежные документы

`INDEX.md` (карта) · `NETWORK.md` (сеть) · `KNOWN_ISSUES.md` (долг) · `EXTRACTION_PLAN.md` (вынос) ·
`NETWORK_LAYER_REUSE.md` (сетевой трек) · `FEATURE_*.md` (хотелки) · `NETWORK_RATIONALE.md`,
`COMPABILITY.md`, `UNITY_MIGRATION.md`, `RUNTIME_MIGRATION.md` (решения/миграции).
