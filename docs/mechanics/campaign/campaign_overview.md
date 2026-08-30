# campaign_overview.md — Campaign: модели и данные

> Домен: `campaign` (ядро `Core.Campaign`). Статус: **данные** (модели/DTO/парсеры; поведение —
> Фаза 4 `EXTRACTION_PLAN.md`).

## 1. Назначение и когда работает

Модели и данные кампании: имение, здания, апгрейды, квесты, городские события, week log, ростер,
провизия. Поведение (имение/здания/город/квесты) — будущая задача `PLAN.md` Фаза 4; сейчас ядро
только загружает и хранит.

## 2. Модель данных (инвентарь)

- `JsonBuilding` / `JsonUpgrades` / `JsonUpgradeTree` / `JsonUpgradeRequirement` —
  здания/апгрейды.
- `JsonQuests` / `JsonQuestGoal` / `JsonPrerequisiteRequirement` — квесты.
- `JsonTownEvent(s)` / `JsonTownEventDataEntry` / `JsonTownEventSetting` — городские события.
- `JsonCampaign` / `JsonRoster` / `JsonProvision` / `JsonInventoryItem` — кампания/ростер/провизия.
- `JsonHeirloomExchange*` / `HeirloomExchangeMapper` — обмен реликвий.
- `JsonNarration*` / `NarrationMapper` / `PartyNameMapper` — наррация/имена отрядов.
- `JsonHeroClassInventory` — инвентарь класса.

## 3. Парсинг контента

`GameDataReader` (`clients_reader.md`): `ReadCampaign`, `ReadQuests`, `ReadTownEvents`, `ReadUpgrades`,
`ReadBuilding`, `ReadProvision`, `ReadRoster`, `ReadNarration`, `ReadPartyNames` → DTO → мапперы
(`HeirloomExchangeMapper`, `NarrationMapper`, `PartyNameMapper`).

## 4. Порядок срабатывания (трассировка)

Только загрузка (нет боевых вызовов): `GameDataReader.Read*` → DTO → `*Mapper.Parse` → модели.

## 5. Очередь и обновления

- Данные — статические (загружаются один раз).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| JSON-валидность | `GameDataReader.Read*` | `DeserializeObject` |
| Маппинг | `*Mapper.Parse` | DTO → домен |

## 7. Нюансы и подводные камни

- **Поведение не реализовано** — не искать логику имения/зданий/квестов в `Core.Campaign`.
  Обновить документ, когда Фаза 4 вынесет поведение (`PLAN.md`).
- Legacy-поведение живёт в `unity\Assets\Scripts\Campaign\` (`UNITY_LEGACY_MAP.md`).

## 8. Взаимодействия

- `clients_reader.md`, `TARGET_LAYOUT.md`, `EXTRACTION_STATUS.md` (Campaign — вынесено: модели+DATA).
- `save_binary.md` — сейвы кампании (Фаза 2).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Campaign/*`
- `unity/Assets/Scripts/Campaign/*` (legacy-поведение, read-only до cutover)