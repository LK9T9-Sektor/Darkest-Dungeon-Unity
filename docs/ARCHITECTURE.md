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
|---|---|---|
| `Common` | Общие типы: `Result`/`Result<T>`, примитивы (`IRng`), `IProportionValue`/`ISingleProportion` (весовой выбор), утилиты `InvariantCulture`, модель feature-flag, низкоуровневый токен-парсер |
| `Content` | Персонажно-боевой контент: `Character\` (`Quirk`, `BuffContent`), `Camping\`, `Trinket\` + их DTO (`Database\Json*`) и мапперы (`QuirkMapper`, `BuffContentMapper`). Ядро **не зависит от Newtonsoft**: DTO-члены названы snake_case по legacy-JSON, десериализация — на клиентской границе (`Clients.Content`/презентация). Причина: сборки Newtonsoft 11/12/13 ссылаются на контракты net6.0 и не компилируются в Unity 2017.4 (`CS0009`; `KNOWN_ISSUES.md` §13) |
| `Campaign` | Кампания/имение, здания, апгрейды, квесты, week log, события города + их данные (модели/DTO/парсеры/каталоги) |
| `Raid` | Подземелья, энкаунтеры, боссы, curio-взаимодействия, loot, пропы + их данные |
| `Save` | DTO, бинарный кодек, версии; `IBinarySaveData`; IO — через `ISaveStorage` |
| `Combat` | Боевая симуляция (Фаза 3, **вынесена**): скиллы, эффекты (29 SubEffect), раунды, `BattleSolver`, AI (desires + мозги кампании `MonsterBrainCatalog`), RNG, баффы. Раскладка зеркалирует legacy-структуру после `Assets\Scripts\`: `Mechanics\` (Battle/Skills/Effects/AI + enums + RandomSolver), `Raid\` (Battle/Events), `Character\` (модель + Buff/BuffInfo + статусы). Границы наружу — интерфейсы: `ICharacter`, `ICombatUnit`, `IBattleGround`, `IBattleContext`, `IBattleEvents` |
| `Duel` | Оркестрация дуэли (PvP 1v1, локстап): `DuelController`, `DuelPhase`, `DuelSeed`, `DuelPayload`, адаптеры `DuelBattleContext`/`DuelBattleEvents`, `IDuelContent`, ИИ `DuelAi`. Раннер боя кампании `Fight\FightSession` (+ `Fight\TextFightContent`) — движок Тест-боя и будущего PvE-боя |
| `Clients.Content` | Клиентская граница (НЕ домен): `GameDataReader` — Newtonsoft-фасад «файл → каталоги доменов». Потребляется WPF и Unity-стендами (Тест-бой). Каталоги стали чистыми и живут в доменах (`Combat\Character\BuffCatalog`, `Content\Character\QuirkCatalog`) |
| `Ui` | Презентационные токены runtime-оверлеев (engine-free): `UiStyle`, `ArgbColor`; кандидат на перенос в клиентскую границу |

Дополнительно в `Combat`: парсер легаси-контента героев `Character\HeroClassFileParser` +
`HeroCatalog` — читают формат `Data/Heroes/Info` (базовый ранг прокачки: атрибуты оружия/брони,
сопротивления, скилы уровня 0; эффекты скиллов резолвятся из `.bytes` в `CombatSkill.Effects`).
WPF-клиент линкует `.bytes`-файлы из unity-контента и грузит полный ростер (15 классов) на старте.
Монстры кампании (M1) — `Character\Monster.cs` + `MonsterClassFileParser`/`MonsterCatalog`:
читают `Data/Monsters/*.txt` (статы, `enemy_type`, резолв эффектов из `Effects.txt`, `battle_modifier`).

### Сеть (`src\Networking\`)

| Модуль | Ответственность |
|---|---|
| `Contracts` | `ITransport`, `ITransportCodec`, `TransportMessage`, `TransportSettings` |
| `Steam` | `SteamTransport` поверх Steam P2P/лобби (interop-слой) |
| `Photon` | `PhotonTransport` поверх Photon Realtime (планируется, Фаза 5) |

### Презентация (Unity, оба дерева)

| Область | Ответственность |
|---|---|
| `Scripts\Networking\` | Фасад `MultiplayerSync`, `SessionManager`, `RaidBridge` — игровой glue над `ITransport`. **Мультиплеерный PvP-бой (дуэль)**: `RaidSceneMultiplayerManager` (god-class 2285 строк) — партия соперника = сторона монстров, lockstep-сид, обмен `party_config`, RPC-входы. Оркестрация не разнесена и живёт в презентации; ядро `Core.Duel` Unity не потребляет (cutover — фаза 6, см. `DUEL_ARCHITECTURE.md`) |
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
3. Детерминированный RNG (общий сид) — фундамент сети/реплеев/коопа; сетевой бой — детерминированный
   локстап (обмениваются только вводами, состояние считается локально), см. `NETWORK.md` §6.
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
