# Вынос контента в чистое ядро (Фаза 1, первый срез)

- Новый модуль `src\Core\Content` (netstandard2.0, C# 7.3): модели `Campaign\` (`HeirloomExchange`, `PartyNameEntry`) и `Database\` (DTO `Json*` + мапперы `HeirloomExchangeMapper`/`PartyNameMapper`). DLL доставляются пост-билдом в `Assets\Plugins\Internal` обоих проектов.
- `DarkestDatabase` стал тоньше: `LoadHeirloomExchanges`/`LoadPartyNames` сводятся к вызовам мапперов ядра; JSON-десериализация (`JsonConvert`) остаётся в адаптере презентации.
- NUnit-тесты `tests\Core\Content` на реальных данных (`HeirloomExchange.json`, `PartyNames.json`).
- DTO — snake_case-члены без `[JsonProperty]`, ядро не зависит от Newtonsoft: сборки Newtonsoft 11/12/13 (включая `net45`/`netstandard2.0`) ссылаются на контракты net6.0 и дают `CS0009` в Unity 2017.4, поэтому атрибуты невозможны до перехода проектов на совместимый Newtonsoft.
- Исправление стартовой загрузки данных/кампании после синка с веткой: `tools\unity-provision-plugins.ps1` теперь также собирает `src\Core\Content` и доставляет его DLL/PDB в `Assets\Plugins\Internal` обоих проектов (ранее сборка отсутствовала → `CS0246`, игра не стартовала).
