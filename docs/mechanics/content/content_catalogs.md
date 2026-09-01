# content_catalogs.md — Trinket / Camping-каталоги и прочий контент

> Домен: `content` (ядро `Core.Content`). Статус: **данные** (тринкеты — применяются в дуэли, см. `09_buffs.md`).

## 1. Назначение и когда работает

Каталоги немеханик-контента: тринкеты (`JsonTrinkets`), camping-скиллы (`JsonCamping`), квирки,
баффы, traits. Тринкеты применяются в дуэли (permanent-баффы в `DuelController.ApplyTrinkets`,
`09_buffs.md`); camping-скиллы пока только загружаются.

## 2. Модель данных

- `Core.Content/Trinket/` — `Trinket`, DTO `JsonTrinket*`, каталог.
- `Core.Content/Camping/` — `CampingSkill`, DTO `JsonCamping*`, каталог.
- `Core.Content/Database/` — общие JSON DTO/мапперы (`JsonCurrencyCost`, `BuffContentMapper`,
  `QuirkMapper`, `TraitMapper`).

## 3. Парсинг контента

`GameDataReader` (`clients/GameDataReader.cs`): `ReadCamping` (`:27`), `ReadTrinkets` (`:43`),
`ReadTraits` (`:107`), и др. → каталоги через мапперы.

## 4. Порядок срабатывания (трассировка)

- Загрузка: `GameDataReader.Read*` → `Json*` DTO → `*Mapper.Parse` → каталог (`Get(id)`).
- Применение тринкетов в дуэли: `DuelController.ApplyTrinkets` (`DuelController.cs:585-608`) →
  `IDuelContent.GetTrinket` → `GetBuff` → `AddBuff(Permanent, Trinket)`; id записываются на героя
  (`Hero.EquippedTrinketIds`). WPF-лобби фильтрует по `HeroClassRequirements`
  (`HeroSlotViewModel.TrinketPool`), 2 слота циклируются (`LobbyTrinketViewModel`).

## 5. Очередь и обновления

- Данные загружаются один раз (статические фабрики `GameDataReader`).
- Баффы тринкетов — permanent, живут всю дуэль; снимаются только `RevertBuff` при снятии.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Чтение JSON | `GameDataReader.Read*` | `JsonConvert.DeserializeObject` |
| Неизвестный id | каталоги `Get` | `null` (игнорируется) |
| Слотов на героя | `Hero.AddTrinket` | max 2, без дублей |
| Валидность класса | WPF `TrinketPool` | `HeroClassRequirements` пусто/содержит класс |

## 7. Нюансы и подводные камни

- **Тринкеты применяются в дуэли как permanent-баффы** (`BuffSourceType.Trinket`) — в кампании
  тринкеты дают rule-баффы; в дуэли рендерится только стат-модификатор (паритет частичный).
- Camping-скиллы — только данные; применение (пас в рейде) — будущая задача.
- Traits — используются `ResolveOverstress` (`14_death_stress.md`) и `TestDuelContent`.

## 8. Взаимодействия

- `clients/GameDataReader.md` — источник.
- `09_buffs.md` — применение тринкетов (permanent source) и слотирование в дуэли.
- `14_death_stress.md` — traits применяются в resolve-ролле.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Content/Trinket/`, `Core.Content/Camping/`,
  `Core.Content/Database/`
- `src/Clients/Sektor.DarkestDungeon.Clients.Content/GameDataReader.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/HeroSlotViewModel.cs`,
  `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/LobbyTrinketViewModel.cs`