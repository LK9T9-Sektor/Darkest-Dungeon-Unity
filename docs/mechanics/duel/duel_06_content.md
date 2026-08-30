# duel_06_content.md — IDuelContent и TextFightContent (мост контента)

> Домен: `duel` (ядро `Core.Duel`). Статус: **реализовано**.

## 1. Назначение и когда работает

`IDuelContent` — интерфейс контент-моста, который `DuelController` использует для получения классов,
квирков, баффов, эффектов, монстров и мозгов. `TextFightContent` — реализация для Тест-боя:
читает файлы кампании через `Clients.Content.GameDataReader` и строит каталоги.

## 2. Модель данных

- `IDuelContent` (`Core.Duel/IDuelContent.cs`) — `GetHeroClass/GetQuirk/GetBuff/GetEffect/GetMonsterClass/
  GetMonsterBrain/GetAfflictions/GetVirtues`.
- `TextFightContent` (`Core.Duel/Fight/TextFightContent.cs:13`) — реализация для файлов.
- Другие реализации: WPF `DuelContent` (`src/Wpf/.../Data/DuelContent.cs`), тест `TestDuelContent`.

## 3. Порядок срабатывания (трассировка)

1. `DuelController.StartFight` (`DuelController.cs:120`) получает `content` из конструктора.
2. `AddMonster` (`:194-207`) — `GetMonsterClass` + `GetMonsterBrain` → `new Monster(class)` +
   `AssignBrain`.
3. `AddPlayerUnit`/`AddHero` (`:155,604`) — `GetHeroClass` → `HeroGeneration.GenerateHero`.
4. Квирки/баффы — `ApplyQuirks` (`:557`); эффекты — `DuelBattleContext.ApplyEffectById`/`GetBuff`.

`TextFightContent` (`TextFightContent.cs`): читает `Heroes/*.bytes`, `Monsters/*.txt`,
`JsonBuffs.json`, `JsonQuirks.json`, `JsonTraits.json`, `Effects.txt`, `JsonAI.json` через
`GameDataReader` (см. `clients/GameDataReader`), строит каталоги (`HeroCatalog`, `MonsterCatalog`,
`MonsterBrainCatalog`, `EffectCatalog`, `QuirkCatalog`, `BuffCatalog`).

## 4. Очередь и обновления

- Контент читается один раз при создании `TextFightContent` (каталоги в память).
- `DuelController` обращается к контенту лениво (по мере сборки отрядов и применения эффектов).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Нет класса/монстра | `DuelController.cs:161,197` | `return` (юнит не добавлен) |
| Нет баффа/эффекта | `DuelBattleContext.cs:121-127,133-136` | `null`-безопасно |

## 6. Нюансы и подводные камни

- **`GetMonsterBrain(null)`/`GetMonsterClass(null)` допустимы** — `TextFightContent` возвращает null
  для неизвестных; `DuelController` тихо пропускает.
- Тест-`TestDuelContent` НЕ реализует монстров/мозгов (`GetMonsterClass` → null) — для дуэли это ок
  (обе стороны герои).
- Файлы контента линкуются в тесты через `Content/*` каталог (`TestDuelContent`).

## 7. Взаимодействия

- `duel_01_lockstep.md` — как контент используется оркестратором.
- `clients/GameDataReader` — источник файлов для `TextFightContent`.
- `content/*` — модели квирков/баффов.

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/IDuelContent.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/Fight/TextFightContent.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Data/DuelContent.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/TestDuelContent.cs`