> Статус: завершён (коммит `d764a32`, `dotnet test`/`dotnet build` зелёные). Перенесён из
> `PLAN.md` при вводе архива планов; индекс архивных планов — `docs\plans\00_index.md`.

# PLAN: WPF — монстры/боссы размером 1–4 ранга: size-aware ранги в ядре + PvE-режим

## Goal

В Unity монстры и боссы занимают 1–4 позиции (`display: .size N`): size-2 монстр на ранге 1
занимает ранги 1–2, следующий юнит стартует с ранга 3; карточка рисуется шириной `×size`; ёмкость
формации = 4 слота (`4 − Σ size`). В WPF это сейчас не работает: (1) ядро парсит `size` и использует
его в таргетинге/запуске/AI, но **назначает ранги size-agnostic** (`i+1`); (2) WPF-бой — только
герои-vs-герои (`StartDuel`), монстры в бою не участвуют; (3) карточки фиксированной ширины, без
`Size`. Делаем: size-aware ранги в ядре (паритет Unity), рендер размера в WPF-виде, PvE-режим
(`StartFight` герои-vs-монстры) с водителем `PveBattleViewModel` (аналог Unity `CoreBattleDriver`).

## Design decisions (user-confirmed)

- **PvE-водитель** — отдельный `PveBattleViewModel` по образцу Unity `CoreBattleDriver`,
  переиспользует `DuelBattleView` для отображения. Герои игрока ходят через `ApplyRemoteSkill`
  (в `StartFight` герои — «remote»), монстры — `Solver.UseMonsterBrain` (локальная сторона).
- **Size-aware ранги в ядре** — да, в этом же плане (`FormationParty`, `TurnMover`, `SurpriseResolver`,
  `DuelBattleEvents.MoveUnit` пересчитывают ранги по кумулятивной сумме `Size`, как Unity).
- **Ёмкость формации** — 4 слота; слоты монстров в лобби ограничены `4 − Σ size`.
- Легаси Unity не правится (read-only); `src\External\` не трогается.

## Steps / status

### M1 — Core: size-aware назначение рангов (паритет Unity)

1. [x] `FormationParty.AddUnit` (core) — ранг = `последний.Rank + последний.Size` (или 1, если пусто);
   `RemoveUnit` — кумулятивный пересчёт по `Size`.
2. [x] `TurnMover.TryMove`, `SurpriseResolver.ShuffleParty`, `DuelBattleEvents.MoveUnit` — тот же
   кумулятивный пересчёт рангов (герои size=1 → ранги 1..4 не меняются).
3. [x] Тесты: формация size-2 + size-1 монстров → ранги 1,3 (не 1,2); таргетинг/запуск по рангам
   учитывает занимаемый диапазон; `dotnet test` зелёный.

### M2 — WPF вид: рендер размера

1. [x] `DuelUnitViewModel.Size` (из `unit.Size`); `DuelUnitCardView` ширина = `185 × Size` (конвертер
   или свойство); ранг-бейдж показывает фронтовый ранг.
2. [x] `DuelBattleView` колонки уже `MinWidth=820` (4×201px) — широкая карточка занимает N слотов,
   StackPanel растягивает; проверить вёрстку с size-2/3.

### M3 — WPF PvE-режим (`StartFight`, как Unity BattleTest)

1. [x] Кнопка **PvE** в `MainMenuView` → `PveLobbyViewModel`/`PveLobbyView`: 4 слота героев
   (переиспользовать `HeroSlotViewModel`) + слоты монстров из `DuelContent` (цикл по каталогу,
   суммарный size ≤ 4, seed).
2. [x] Старт: `DuelController.StartFight(heroSpecs, monsterSpecs, seed)`; `PveBattleViewModel` —
   водитель: поллинг/диффинг как `DuelBattleViewModel`, герои игрока → `ApplyRemoteSkill`, монстры →
   `UseMonsterBrain`, ввод через переиспользуемый `DuelBattleView`.
3. [x] Регистрация VM↔View в `MainWindow.xaml` (+ интерфейс `IDuelBattleViewData` для переиспользования
   `DuelBattleView` без правки `DuelBattleViewModel`). Тесты `PveBattleTests` (3) зелёные.

### M4 — Документация (в тех же коммитах)

1. [x] `docs\mechanics\combat\16_formation_size.md` (size-ранги: кумулятивный пересчёт, занимаемый
   диапазон, ёмкость 4, рендер); `BATTLE_PARITY.md` — строка size (2.6 + резюме).
2. [x] `TESTING.md` (PvE-чеклист + size-рендер + историческая строка), `CHANGELOG.md`,
   `00_index.md` (16), `INDEX.md`, `presentation_wpf.md` (gotcha).

## Affected files

- `src\Core\Sektor.DarkestDungeon.Core.Combat\Raid\Party\FormationParty.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\Mechanics\TurnMover.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\Mechanics\SurpriseResolver.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\DuelBattleEvents.cs`.
- `src\Wpf\...\ViewModels\DuelUnitViewModel.cs`, `Views\DuelUnitCardView.xaml`,
  `ViewModels\PveLobbyViewModel.cs` (новый), `Views\PveLobbyView.xaml` (новый),
  `ViewModels\PveBattleViewModel.cs` (новый), `Views\MainMenuView.xaml`, `MainWindow.xaml`.
- Тесты: `tests\Core\...` (size-ранги).
- Доки: `docs\BATTLE_PARITY.md`, `docs\TESTING.md`, `docs\CHANGELOG.md`,
  `docs\mechanics\combat\13_turn_order.md` (или новая), `docs\mechanics\00_index.md`.

## Acceptance criteria

- `dotnet test` зелёный (новые size-ранги тесты + существующие sweep/дуэль).
- `dotnet build` WPF зелёный.
- PvE-бой: герои игрока ходят, монстры (в т.ч. size 2–4) ходят AI, size-карточки занимают N слотов,
  ранги 1,3,… без перекрытия; команды не дрейфуют (прошлый фикс не сломан).
- Легаси Unity и `src\External\` не изменены.

## Out of scope

- Полный cutover игры (кампания/город/инвентарь в WPF) — отдельные фазы.
- `.summon/.control/.capture` и `GetMonsterSize`/`AvailableSummonSpace`-стабы — не обязательны для
  рендера размера (если не мешают PvE-свип-тестам, не трогаем; иначе — минимальный фикс).
- Мультиплеер PvE (герои игрока в сети) — не в этом плане.
