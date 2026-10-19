> Закрыт: 2026-09-03 (коммит `8e83db9`; ручной прогон боя — за пользователем, см. `TESTING.md`).
> Перенесён из `PLAN.md` при вводе архива планов; индекс закрытых планов — `docs\plans\00_index.md`.

# PLAN: WPF-дуэль — привязка формаций к своим половинам (умершие не двигают команды)

## Что сделано

- `DuelBattleView.xaml`: `Auto`-колонки заменены на слот-пол (герои `HorizontalAlignment="Right"`, `MinWidth≈820`; центр ≈120; монстры `Left`, `MinWidth≈820`) — смерть юнита не схлопывает колонку и не сдвигает команды.
- `TargetLayer` → `Grid.ColumnSpan="3"`; математика стрелок в координатах Viewbox не изменилась.
- Доки в том же коммите: `presentation_wpf.md` (gotcha), `CHANGELOG.md`, `TESTING.md`; `dotnet build` WPF зелёный.

## Осталось / за пользователем

- Ручной прогон боя (герои/монстры умирают — команды не дрейфуют), см. `TESTING.md` § WPF-дуэль.

---

_Ниже — исходный план без изменений._

## Goal

Исправить снос поля при смерти юнитов в WPF-бою (`DuelBattleView.xaml`): когда персонаж умирает,
коллекция сжимается, `Auto`-колонка схлопывается, сетка перецентрируется и правая команда съезжает
влево. Требуется: команды привязаны к своим половинам, фронт (ранг 1) у центра, центр разделяет
их на две — по 4 позиции слева и справа. Решение пользователя: **reflow к центру** (выжившие
сдвигаются вперёд, как в ядре `RemoveUnit`), а не резерв пустых слотов.

## Steps / status

1. [x] `DuelBattleView.xaml`: колонки `Auto` → слот-пол: герои `MinWidth≈820` (4×201px+margin),
   центр `Width≈120`, монстры `MinWidth≈820`.
2. [x] Пере-якорить стеки: герои `HorizontalAlignment="Right"` (колонка 0), монстры
   `HorizontalAlignment="Left"` (колонка 2).
3. [x] `TargetLayer` Canvas → `Grid.ColumnSpan="3"` (математика стрелок в координатах Viewbox
   остаётся корректной).
4. [x] Доки в том же коммите: `docs\mechanics\presentation\presentation_wpf.md` (gotcha),
   `docs\CHANGELOG.md`, `docs\TESTING.md` (дуэльный чеклист).
5. [x] `dotnet build` WPF-проекта зелёный. Ручной прогон боя (герои/монстры умирают — команды
   не дрейфуют) — **на пользователе** (см. `TESTING.md` § WPF-дуэль).

## Affected files

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml`.
- Доки: `docs\mechanics\presentation\presentation_wpf.md`, `docs\CHANGELOG.md`, `docs\TESTING.md`.

## Acceptance criteria

- Умершие юниты уходят, выжившие на своей половине сдвигаются к центру (фронт у центра).
- Правая команда не смещается влево при смерти героев; центр всегда разделяет команды.
- `TargetLayer` стрелки/бейдж не ломаются (ColumnSpan учтён).
- `dotnet build` зелёный; легаси не тронуто.
