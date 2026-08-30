# content_catalogs.md — Trinket / Camping-каталоги и прочий контент

> Домен: `content` (ядро `Core.Content`). Статус: **данные** (поведение/применение — позже).

## 1. Назначение и когда работает

Каталоги немеханик-контента: тринкеты (`JsonTrinkets`), camping-скиллы (`JsonCamping`), квирки,
баффы, traits. Сейчас — загрузка и хранение данных; боевое применение тринкетов в дуэли не
реализовано (см. `BATTLE_PARITY.md` §3/`PLAN.md` P1.4).

## 2. Модель данных

- `Core.Content/Trinket/` — `Trinket`, DTO `JsonTrinket*`, каталог.
- `Core.Content/Camping/` — `CampingSkill`, DTO `JsonCamping*`, каталог.
- `Core.Content/Database/` — общие JSON DTO/мапперы (`JsonCurrencyCost`, `BuffContentMapper`,
  `QuirkMapper`, `TraitMapper`).

## 3. Парсинг контента

`GameDataReader` (`clients/GameDataReader.cs`): `ReadCamping` (`:27`), `ReadTrinkets` (`:43`),
`ReadTraits` (`:107`), и др. → каталоги через мапперы.

## 4. Порядок срабатывания (трассировка)

Только загрузка (нет боевых вызовов): `GameDataReader.Read*` → `Json*` DTO → `*Mapper.Parse` →
каталог (`Get(id)`).

## 5. Очередь и обновления

- Данные загружаются один раз (статические фабрики `GameDataReader`).
- Применение в дуэли — нет (планируется, `PLAN.md` P1.4 тринкеты).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Чтение JSON | `GameDataReader.Read*` | `JsonConvert.DeserializeObject` |
| Неизвестный id | каталоги `Get` | `null` |

## 7. Нюансы и подводные камни

- **Тринкеты/camping — только данные**: применение (стат-влияние, слотирование) — будущая задача
  (`PLAN.md` P1.4). Не искать применения в боевом коде.
- Traits — используются `ResolveOverstress` (`14_death_stress.md`) и `TestDuelContent`.

## 8. Взаимодействия

- `clients/GameDataReader.md` — источник.
- `14_death_stress.md` — traits применяются в resolve-ролле.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Content/Trinket/`, `Core.Content/Camping/`,
  `Core.Content/Database/`
- `src/Clients/Sektor.DarkestDungeon.Clients.Content/GameDataReader.cs`