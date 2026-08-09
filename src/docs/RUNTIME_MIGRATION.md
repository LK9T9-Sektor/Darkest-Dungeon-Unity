# Миграция scripting runtime: .NET 3.5 → .NET 4.6

Что изменилось, какие ошибки появились и почему.

## 1. Что изменилось

- Unity 2017.4.40f1. Scripting Runtime Version поднят с **Stable (.NET 3.5 Equivalent)** до **Experimental (.NET 4.6 Equivalent)**.
- `ProjectSettings\ProjectSettings.asset`: `scriptingRuntimeVersion: 1`, `apiCompatibilityLevel: 2` (.NET 4.x).
- После переключения при старте игры появились ошибки загрузки контента — полный лог в `src\issues\Migration-Issues-01.txt`.

## 2. Симптомы (сводка по логу)

Все ошибки возникают на старте в `DarkestDungeonManager.Awake` → `DarkestDatabase.Load()` и в следующих за ним `Start()`-методах. Три группы:

1. **`Failed to parse X to float in .summon_chances / .protection_rating_add`** — `Effect.LoadData` (`Assets\Scripts\Mechanics\Skills\Effect.cs:554,624`). Не критично по коду, но **данные молча теряются** (шансы призыва, штрафы защиты).
2. **`FormatException: Input string was not in a correct format.`** — `HeroClass.LoadData` (`Assets\Scripts\Character\HeroClass.cs:111`, `float.Parse`). **Фатально**: прерывает `DarkestDatabase.Load()`.
3. **Каскад после обрыва загрузки:**
   - `NullReferenceException` в `SkillTooltip.Initialize` (`Assets\Scripts\UI\Controls\SkillTooltip.cs:27`) — `Data.HexColors` не инициализирован (`DarkestDatabase.cs:1925`);
   - `KeyNotFoundException` в `SaveCampaignData.PopulateStartingEstateData` (`Assets\Scripts\Setup\SaveSystem\SaveCampaignData.cs:182`) — `Data.HeroClasses["crusader"]` отсутствует (загрузка героев не завершилась).

## 3. Первопричина

**Культуро-зависимый парсинг чисел.** Весь код парсит числа без `CultureInfo.InvariantCulture` (`float.Parse`, `float.TryParse`, `int.Parse`, `double.Parse`, `Convert.To*` — **164 места** в `Assets\Scripts`).

- Под **.NET 3.5** (Mono 2.x) текущая культура по умолчанию была en-US → строки `"10.0"`, `"-10.0"`, `"0.5"` парсились без проблем.
- Под **.NET 4.6** (Mono 4.x) текущая культура берётся из OS-локали. На машине с русской локалью десятичный разделитель — запятая `,`, поэтому все числа с точкой `10.0` перестают парситься.

Отсюда и вид ошибок: падают именно десятичные значения (включая отрицательные `-10.0`), а целые числа `int.Parse` продолжают работать.

## 4. Цепочка отказа (каскад)

```
float.Parse без InvariantCulture (HeroClass.cs:111)
  → FormatException
  → DarkestDatabase.Load() обрывается (LoadJsonHeroClasses)
  → HexColors = null                       → NRE в SkillTooltip.cs:27
  → HeroClasses пуст/неполон                → KeyNotFoundException в SaveCampaignData.cs:182
```

Дополнительно `float.TryParse` в `Effect.cs` не бросает, а логирует — эффекты призыва/защиты загружаются частично с потерей данных.

## 5. Исправление (рекомендации, код не менялся)

- **Вариант A (быстро, глобально).** Одна строка в `DarkestDungeonManager.Awake`:
  ```csharp
  CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
  ```
  Лечит все 164 места сразу. Минус — это «обход», сам парсинг остаётся культуро-зависимым для любого нового кода.
- **Вариант B (правильно, точечно).** Добавить `CultureInfo.InvariantCulture` во все вызовы `Parse`/`TryParse`/`Convert.To*` (164 места). Надёжно, но трудоёмко.
- **Рекомендация:** вариант A для немедленного запуска + постепенный переход на вариант B (например, через helper-методы).

## 6. Что проверить после фикса

- Старт игры без ошибок в логе (`DarkestDungeonManager.Awake`).
- Загрузка эффектов: `.summon_chances` и `.protection_rating_add` не теряют данные.
- Стартовый сейв (`SaveSelector`/`PopulateStartingEstateData`) создаётся без `KeyNotFoundException`.
- Тултипы навыков (`ToolTipManager.Start`) не падают с NRE.
