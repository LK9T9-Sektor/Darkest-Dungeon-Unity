# TARGET_LAYOUT.md — Целевая декомпозиция ядра по доменам

> **Что это.** Предложение по целевой структуре `src\Core\` (+ клиентский shared) с принципом
> **данные = домен**: каждый домен владеет своими моделями, DTO, парсерами и каталогами.
> Обоснование «почему так», правила зависимостей и миграционный путь. Критика текущего состояния —
> `ARCHITECTURE_REVIEW.md` §2–3; карта легаси — `UNITY_LEGACY_MAP.md`; манифест — `EXTRACTION_STATUS.md`.
>
> **Критерии дизайна (из запроса):** переиспользование (клиенты Unity/WPF), гибкость (новые
> механики/хотелки), расширяемость (модуль = домен = папка), простота (KISS, DAG без циклов).

## 1. Принципы

1. **Данные = домен.** DTO/парсер/каталог живут в модуле своего домена (знание о `Quest` — в
   `Campaign`, о `CombatSkill` — в `Combat`, о `Curio` — в `Raid`). Нет «модуля всех данных».
2. **Модуль = папка = namespace = домен** (`AGENTS.md`); один публичный тип на файл.
3. **DAG без восходящих зависимостей**: данные/движок внизу, оркестрация сверху; ни один модуль
   не зависит от `Duel`/`Ui`/клиента.
4. **Десериализация — внешняя граница.** Newtonsoft и чтение файлов — на краю (клиенты/shared);
   домен знает только DTO (snake_case по legacy-JSON) + чистые мапперы.
5. **Клиенты разделяют всё ядро, не дублируя домен**: `Wpf` и оба Unity-дерева ссылаются на один
   набор сборок.

## 2. Целевая раскладка

```
src/
├── Core/
│   ├── Common        — Result/Result<T>, примитивы, IRng/RandomSolver, InvariantCulture,
│   │                   feature-flag, низкоуровневый токен-парсер (split-respecting-quotes/percent)
│   ├── Content       — контентный базис по подпапкам доменов: Character\ (Quirk/Buff/Trait/Trinket),
│   │                   Camping\, Loot\, Curio\, Provision\, Town\, Quest\, Building\, Narration\,
│   │                   PartyNames\, Inventory\, Roster\... (DTO+парсер+каталог в каждой подпапке)
│   ├── Combat        — боевой движок + боевые модели: Character\ (Hero/Monster/Character/Statuses),
│   │                   Mechanics\ (Battle/Skills/Effects/AI/RandomSolver), Raid\Party, Raid\Battle
│   ├── Campaign      — имение/здания/апгрейды/квесты/городские события/week log/ростер/провизия
│   │                   (модели + данные из Content\Town, Content\Quest, Content\Provision)
│   ├── Raid          — подземелья/энкаунтеры/боссы/curio-взаимодействия/loot/props-модели
│   │                   (модели + данные из Content\Curio, Content\Loot)
│   ├── Save          — DTO + бинарный кодек + версии + ISaveStorage
│   └── Duel          — PvP-оркестрация (DuelController/Seed/Payload/Ai) + Fight-раннер
│                       + TextFightContent (IDuelContent-мост из Core.Data)
├── Networking/       — транспорт: Contracts (Result, каналы) / Steam / Photon (переименование src\Lan)
└── Shared/           — клиентский shared (НЕ домен): токены UI (из Core.Ui), GameDataReader-фасад
                        (Newtonsoft на границе, читает файлы → каталоги доменов)
```

## 3. Что куда переезжает (по шагам)

### 3.1 Безопасные переносы (без изменения поведения)
| Что | Откуда | Куда | Зачем |
|---|---|---|---|
| `TextFightContent` | `Core.Data\Content\` | `Core.Duel\Fight\` | убрать зависимость Data→Duel (критика §2.1) |
| `Result`/`Result<T>` | `src\Lan\...Contracts\Results\` | `Core.Common` (и ссылку из Contracts убрать/оставить дубль) | закрыть Result-инвариант (§2.3) |
| `UiStyle`, `ArgbColor` | `Core.Ui` | `src\Shared\` (или в Wpf) | ядро = домен (§2.9) |
| `GameDataReader` + Newtonsoft | `Core.Data` | `src\Shared\` (клиентская граница) | десериализация вне домена (§3.3-решения, §2.8) |

### 3.2 Складывание `Core.Data` в `Content` (механически, по подпапкам доменов)
- `Data\Dto\JsonCamping*`, `JsonTrinket*`, `JsonQuests*`, `JsonTownEvent*`, `JsonBuilding*`,
  `JsonProvision*`, `JsonRoster*`, `JsonInventoryItem*`, `JsonCurioProp*`, `JsonMonsterBrains` →
  `Content\<подпапка домена>\` (DTO + маппер/парсер/каталог рядом).
- `Data\Catalogs\*` (Buff/Quirk/Trinket/CampingSkill/MonsterBrain) → `Content\Character\`,
  `Content\Camping\`, `Content\Mechanics\AI\` (brains — AI-контент).
- `Data\Readers\JsonBrainParser` → рядом с `MonsterBrainCatalog` (AI-контент).
- Проект `Core.Data` удаляется; `GameDataReader` живёт в `src\Shared\`.

### 3.3 По мере выноса доменов (`EXTRACTION_PLAN` Фазы 2/4)
- **Campaign** (Фаза 4): модели `Campaign\` (Estate/Buildings/Quests/Town/WeekLog) + данные из
  `Content\Town|Quest|Provision|Roster` → `Core.Campaign`.
- **Raid/Encounter** (Фаза 4/3): Dungeons/Encounters/Bosses/Curios/Loot + `Content\Curio|Loot` →
  `Core.Raid`.
- **Save** (Фаза 2): сейвы из Unity + `Content\Save\IBinarySaveData` → `Core.Save`.
- **Combat** уже дома; оставшийся контент `Content\Character\*` переезжает в `Combat\Character\`
  по мере слияния моделей (Quirk/Buff/Trinket — собственность персонажа).

### 3.4 Дорожка состояния
- `Core.Content` после Фазы 2/4 схлопывается до «общего контентного базиса» (то, что ещё не
  приписано домену); по завершении выноса — почти пуст (остаются только истинно общие ресурсы).
- `src\Lan` → `src\Networking` (Contracts/Steam/Photon) без пересечения с `Core\` (домен не знает
  о транспорте).

## 4. Граф зависимостей (целевой)

```
                    ┌─────────────────────────────┐
                    │  Shared (клиентская граница) │  Newtonsoft, GameDataReader, токены UI
                    └───────▲──────────▲──────────┘
                            │          │
            ┌───────────────┴─────┐    │
            │  Duel  ←─ Combat    │    │
            │  (оркестрация PvP)  │    │
            └───▲────────▲────────┘    │
                │        │             │
   ┌────────────┴───┐  ┌─┴────────────┐└┐
   │  Campaign      │  │  Raid        │ │
   │  (модели+данные)│  │  (модели+данные)│
   └────▲───────────┘  └────▲─────────┘ │
        │                   │           │
        └─────── Content ────┘           │
        (контентный базис, DTO+парсеры)   │
                ▲                        │
                │                        │
             Common  ◄── Result, IRng,  ┘
             (токен-парсер, культура)      Save ← Content/Combat/Campaign
                                          Networking ← (Contracts; НЕ знает Core)
```

Правила: нет восходящих (ни один модуль не зависит от `Duel`/`Shared`/клиента); циклов нет;
`Save` — по желанию (может зависеть от Combat для боевых сейвов); `Networking` — отдельная ветка,
домен не зависит от транспорта (`ARCHITECTURE.md` §4, `NETWORK_LAYER_REUSE.md`).

## 5. Обоснование (переиспользование / гибкость / расширяемость / простота)

| Критерий | Как достигается |
|---|---|
| **Переиспользование** | Один набор доменных сборок для WPF и обоих Unity-деревьев; `GameDataReader` и токены — в `Shared`, а не в домене |
| **Гибкость** | Новый эффект/стат/статус/данные = новое определение в своём домене (data-resources, OCP), без switch |
| **Расширяемость** | Новый домен = новый модуль `Core.<Domain>`; правило «module growth lifecycle» (`AGENTS.md`) |
| **Простота** | DAG без циклов; «данные рядом с логикой»; один принцип размещения вместо двух (критика §2.2); Newtonsoft убран из ядра → меньше рисков 2017.4 (§2.8) |

## 6. Миграционный путь (инкрементально, минимальными диффами)

1. Переносы без поведения (§3.1) — отдельные коммиты: `TextFightContent` → Duel; `Result` → Common;
   `Core.Ui` → Shared; сборка/тесты зелёные после каждого.
2. Механическое складывание `Core.Data` в `Content` (§3.2) — коммиты по подпапкам; `GameDataReader`
   в `Shared`; поведение идентично (тесты `Core.Data.Tests` переезжают в `Core.Content.Tests`/Shared).
3. Удаление проекта `Core.Data`; `check-extraction.ps1` и `EXTRACTION_STATUS.md` обновляются в том же
   коммите (правило «доки в том же коммите»).
4. Доменные выносы (Campaign/Raid/Save) — в рамках своих фаз `EXTRACTION_PLAN.md`, забирая данные
   с собой из `Content`.

**Критерий приёмки каждого шага:** `dotnet build` + тесты зелёные; diff минимален; доки
(`EXTRACTION_STATUS`, `UNITY_LEGACY_MAP`, `ARCHITECTURE`, `PLAN.md`) в том же коммите; Unity-легаси
не изменён.