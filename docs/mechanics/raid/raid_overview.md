# raid_overview.md — Raid: подземелья, curio, loot, пропы

> Домен: `raid` (ядро `Core.Raid`). Статус: **данные** (модели/DTO/парсеры; поведение —
> Фаза 4 `EXTRACTION_PLAN.md`).

## 1. Назначение и когда работает

Модели и данные рейда: типы областей, curio (взаимодействия/результаты), loot (базы), пропы
(двери/ловушки/препятствия). Поведение (энкаунтеры/боссы/curio-взаимодействия/генерация) — будущее
(`PLAN.md` Фаза 4/Generation).

## 2. Модель данных (инвентарь)

- `AreaType`, `Curio`, `CurioInteraction`, `CurioResult`, `ItemInteraction`, `Prop` — модели.
- `JsonCurioProp(s)` / `JsonCurioPropVariation` — DTO curio-пропов.
- `JsonLootDatabase` / `LootDatabase` / `LootMapper` — лoot-базы.
- `CsvReader` / `CurioCsvParser` — CSV-парсинг curio.

## 3. Парсинг контента

`GameDataReader` (`clients_reader.md`): `ReadCurioProps` (JSON), `ReadCurios` (CSV через `CsvReader`),
`ReadLoot` (JSON) → `CurioCsvParser`/`LootMapper` → каталоги.

## 4. Порядок срабатывания (трассировка)

Только загрузка (нет боевых вызовов): `GameDataReader.Read*` → парсеры/мапперы → модели.

## 5. Очередь и обновления

- Данные — статические.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| CSV/JSON-валидность | `CsvReader`/`DeserializeObject` | парсинг по формату |

## 7. Нюансы и подводные камни

- **Поведение не реализовано** — энкаунтеры/боссы/curio-взаимодействия/генерация — будущее
  (`PLAN.md`). Legacy: `unity\Assets\Scripts\Raid\` (read-only).
- Curio-парсинг — CSV (`CurioCsvParser`), не JSON.

## 8. Взаимодействия

- `clients_reader.md`, `TARGET_LAYOUT.md`, `EXTRACTION_STATUS.md` (Raid — вынесено: модели+DATA).
- `content_catalogs.md` — соседний контент-базис.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Raid/*`
- `unity/Assets/Scripts/Raid/*` (legacy-поведение, read-only до cutover)