# clients_reader.md — GameDataReader: Newtonsoft-фасад загрузки контента

> Домен: `clients` (клиентская граница `Clients.Content`). Статус: **реализовано**.

## 1. Назначение и когда работает

Единственный читатель всего набора JSON/CSV-файлов игры: десериализует текст файла в DTO/каталоги
доменов. Живёт **вне домена** (Newtonsoft на границе, принцип «десериализация — внешняя граница»,
`TARGET_LAYOUT.md` §1). Домен знает только DTO и чистые мапперы.

## 2. Модель данных

- `GameDataReader` (`Clients.Content/GameDataReader.cs:22`) — статический класс, фабрики `ReadX(text)`.
- Методы: `ReadCamping` (`:27`), `ReadQuests` (`:35`), `ReadTrinkets` (`:43`), `ReadProvision` (`:51`),
  `ReadRoster` (`:59`), `ReadTownEvents` (`:67`), `ReadCampaign` (`:75`), `ReadUpgrades` (`:83`),
  `ReadBuilding` (`:91`), `ReadCurioProps` (`:99`), `ReadTraits` (`:107`), `ReadCurios` (CSV, `:116`),
  `ReadLoot` (`:124`), `ReadNarration` (`:133`), `ReadPartyNames` (`:142`), `ReadBrains` (`:160`),
  `ReadBuffs` (`:169`), `ReadQuirks` (`:178`), `ReadEffects` (`:203`), `ReadHeroes` (`:212`),
  `ReadMonsters` (`:221`).

## 3. Парсинг контента

- JSON: `JsonConvert.DeserializeObject<T>(text)` (Newtonsoft).
- CSV: `CsvReader` (curio).
- Текстовые DSL: `EffectCatalog.Load`, `HeroCatalog.Load`, `MonsterCatalog.Load` (эффекты/герои/монстры).
- Traits: `TraitMapper` из `JsonTraitData`.

## 4. Порядок срабатывания (трассировка)

`GameDataReader.ReadX(fileText)` → DTO/каталог → вызывающий строит доменные каталоги. Пример:
`TextFightContent` читает `Effects.txt` → `ReadEffects` → `EffectCatalog` → `CombatSkill.Effects`.

## 5. Очередь и обновления

- Статические фабрики — без состояния; каждая загрузка — новый объект.
- Клиент (WPF/Unity/тесты) сам организует пути и кэширование.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Файл отсутствует | вызывающий | пропуск (каталог пуст) |
| `DeserializeObject` | `ReadX` | возврат DTO |

## 7. Нюансы и подводные камни

- **Newtonsoft не должен просачиваться в домен** — домен получает только DTO/чистые парсеры.
  Нарушение = перенос `JsonConvert` внутрь ядра (запрещено, `TARGET_LAYOUT.md` §2.8).
- `ReadCurios` — CSV (не JSON), `CsvReader` рядом.
- Файл → метод: один файл = один `ReadX` (напр. `ReadBuffs` → `JsonBuffs.json`).

## 8. Взаимодействия

- `duel_06_content.md` (`TextFightContent`), `content/*`, `campaign/*`, `raid/*`.
- Доменные каталоги (`content_buff_content.md`, `content_catalogs.md`).

## 9. Файлы-источники

- `src/Clients/Sektor.DarkestDungeon.Clients.Content/GameDataReader.cs`
- `tests/Clients/Sektor.DarkestDungeon.Clients.Content.Tests/`