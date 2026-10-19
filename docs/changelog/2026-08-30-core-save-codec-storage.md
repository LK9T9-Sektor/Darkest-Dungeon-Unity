# Save: бинарный кодек + версии + `ISaveStorage` в ядре (Фаза 2)

- **`SaveCodec`** (`Core.Save`): версия формата (`SaveVersion.Current`), коллекции `IBinarySaveData`
  (`WriteList/ReadList`, вложенные списки, `Dictionary<string,T>`, `Dictionary<int,Dictionary<string,T>>`)
  и примитивные коллекции (`List<int>/<string>/<bool>`, `Dictionary<string,int>`), null-строки → `""`.
- **`ISaveStorage`** (`Core.Save`): контракт файл↔поток (имена файлов, каталоги, существование/удаление,
  открытие потоков) — реализация остаётся в презентации.
- **Legacy Unity**: `BinarySaveDataHelper` делегирует коллекции/версию коду ядра; `SaveLoadManager`
  пишет/читает версию через `SaveCodec`. Поле-маппинг `SaveCampaignData` и `Create<T>` (дискриминаторы
  Quest/Prop) остаются в Unity — полный DTO-перенос вместе с Фазой 4.
- Новые тесты `Core.Save.Tests` (14): round-trip кодеков, версия, фильтр `IsMeetingSaveCriteria`,
  хранилище.
