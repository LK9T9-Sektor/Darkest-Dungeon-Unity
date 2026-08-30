# save_binary.md — Save: кодек, версии, хранилище

> Домен: `save` (ядро `Core.Save`). Статус: **реализовано** (кодек + версии + `ISaveStorage`;
> полный DTO-перенос `SaveCampaignData` — вместе с Фазой 4, т.к. его поля зависят от кампанийных
> runtime-моделей Quest/Dungeon/WeekActivityLog/DeathRecord/UpgradePurchases).

## 1. Назначение и когда работает

Бинарная сериализация сейвов: кодек коллекций/примитивов + версия формата в чистом ядре
(`Core.Save`, netstandard2.0), файловый IO — за `ISaveStorage` (реализация в презентации).
Legacy Unity (`SaveLoadManager`) по-прежнему владеет поле-маппингом `SaveCampaignData`, но
коллекции/версию делегирует коду ядра через `BinarySaveDataHelper` → `SaveCodec`.

## 2. Модель данных

- `SaveVersion` (`Core.Save/SaveVersion.cs:8`) — `Current = "1"`.
- `SaveCodec` (`Core.Save/SaveCodec.cs:14`) — статический кодек:
  - `WriteVersion/ReadVersion` (`:20-33`) — заголовок версии (read возвращает совпадение);
  - коллекции `IBinarySaveData`: `WriteList/ReadList` (`:38-70`), `WriteListList/ReadListList`
    (`:74-101`), `WriteDictionary/ReadDictionary` (`:106-139`),
    `WriteInstancedDictionary/ReadInstancedDictionary` (`:145-178`);
  - примитивные коллекции: `WriteStringIntDictionary/ReadStringIntDictionary` (`:183-205`),
    `WriteIntList/ReadIntList`, `WriteStringList/ReadStringList` (null → ""), `WriteBoolList/ReadBoolList`.
- `ISaveStorage` (`Core.Save/ISaveStorage.cs:13`) — контракт файл↔поток: имена файлов
  (`GetSaveFileName/GetMapFileName`), каталоги (`EnsureSaveDirectory/EnsureMapDirectory`),
  существование/удаление (`SaveExists/DeleteSave`), открытие потоков
  (`OpenSaveForWrite/Read`, `OpenMapForWrite/Read`).
- `BinarySaveDataHelper` (Unity) — legacy-адаптер: extension-методы делегируют в `SaveCodec`;
  `Create<T>` (дискриминаторы Quest/Prop + `Read`) остаётся Unity-специфичным.

## 3. Порядок срабатывания (трассировка)

**Запись** — `SaveLoadManager.WriteSave` (`SaveLoadManager.cs:29`) → `SaveCodec.WriteVersion(bw)`
→ примитивы `bw.Write(...)` → коллекции через `BinarySaveDataHelper.Write` → `SaveCodec.WriteList`
(фильтрует `IsMeetingSaveCriteria`).

**Чтение** — `SaveLoadManager.ReadSave` (`:187`) → `SaveCodec.ReadVersion(br)` (несовпадение →
`NotImplementedException`) → примитивы → коллекции через `BinarySaveDataHelper.Read` →
`SaveCodec.ReadList` → `Create<T>` (дискриминатор + `Read`).

## 4. Очередь и обновления

- Нет (модель/сериализация, не бой).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Версия | `SaveCodec.ReadVersion` | должна равняться `SaveVersion.Current` |
| Фильтр записи | `SaveCodec.WriteList`/`WriteDictionary` | `IsMeetingSaveCriteria` |
| null-строки | `SaveCodec.WriteStringList` | null → `""` |

## 6. Нюансы и подводные камни

- **Порядок `Write`/`Read` должен совпадать** — кодек не знает содержимого элементов; ответственность
  за последовательность полей у владельцев DTO.
- **Фабрика полностью читает элемент** (`factory(br)` делает дискриминатор + `Read`) — кодек только
  вызывает её; иначе двойной `Read`.
- **Полный DTO-перенос отложен**: `SaveCampaignData` ссылается на кампанийные runtime-модели
  (Quest/Dungeon/WeekActivityLog/DeathRecord/UpgradePurchases/QuirkInfo) — их вынос = Фаза 4.
- `BinarySaveDataHelper.Create<T>` остаётся в Unity (дискриминаторы Quest/Prop через
  `DarkestDungeonManager.Data`); ядро от него свободно.

## 7. Взаимодействия

- Unity: `SaveLoadManager` (поле-маппинг), `BinarySaveDataHelper` (адаптер), save-DTO (`SaveHeroData`,
  `SaveEventData`, ... реализуют `IBinarySaveData`).
- Файловые пути: `Application.persistentDataPath`/Saves, /Maps — в презентации (реализация
  `ISaveStorage`).

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Save/SaveVersion.cs`, `SaveCodec.cs`, `ISaveStorage.cs`,
  `IBinarySaveData.cs`
- `unity/Assets/Scripts/Setup/SaveSystem/SaveLoadManager.cs`, `IBinarySaveData.cs`
  (`BinarySaveDataHelper`)
- `docs/EXTRACTION_PLAN.md` (Фаза 2), `docs/UNITY_LEGACY_MAP.md` (SaveSystem)