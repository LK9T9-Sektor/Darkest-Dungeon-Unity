> Закрыт: 2026-10-19 (ветка `core/unity7-urp`). Коммит: `2a746aa` (behaviors и разбор вошли в
> прототип Spine-демо). Индекс — `docs\plans\00_index.md`.

# PLAN: Spine/SCREENSHOT — Microsoft.Xaml.Behaviors + документ о белых прямоугольниках

## Что сделано

- Click-обработчики переведены на `Microsoft.Xaml.Behaviors` (1.1.135): `Behaviors\SaveScreenshotAction.cs` (DP `SavedCommand` — путь PNG или null) и generic `Behaviors\MarkEventHandledAction.cs` (ставит `RoutedEventArgs.Handled`).
- `SpineDemoView.xaml`: `EventTrigger EventName="Click"` → action, статус — состояние VM (`ScreenshotStatus`), code-behind пустой; `DuelUnitCardView.xaml`: info-кнопка через `MarkEventHandledAction`, `OnInfoButtonClick` удалён.
- `SpineDemoViewModel`: `ScreenshotStatus` + `ScreenshotSavedCommand`; тест `SpineDemoViewModelTests` (путь → имя файла, null → `Screenshot failed`).
- Разбор «белых прямоугольников» — `docs\plans\2026-10-19-spine-transparency-vertex-colors-closed.md`; ссылка из `presentation_wpf.md`; `Click=` в XAML клиента не осталось.
- Проверки: `check-using-placement` OK, `dotnet test` 76/76, WPF-билд зелёный; UIA-клик SCREENSHOT → PNG + статус в окне.

## Осталось

- Ручная проверка info-кнопки в дуэли — за пользователем, отдельного плана нет: чек-лист —
  `docs\TESTING.md`, строка «Buff/debuff table popup» (таблица баффов открывается по `i`, повторный
  клик закрывает, цель не выбирается).

---

_Ниже — исходный план без изменений._
## Goal

Убрать code-behind-обработчики кликов в WPF-клиенте, заменив их `Microsoft.Xaml.Behaviors`
(пакет 1.1.135 уже подключён): SCREENSHOT и info-кнопка карточки дуэли. Mouse/lifecycle-обработчики
остаются (view-геометрия стрелок/бейджа). Отдельным документом зафиксировать разбор бага «белые
прямоугольники»: почему возник (vertex colors в SkiaSharp 2.88) и как решён (`SKColorFilter`).

## Design decisions (user-confirmed)

- Объём: переводим только `Click`-обработчики; `DuelBattleView` mouse-события, `PumpableScreenBase`
  и DP-callbacks кастомных контролов не трогаем — это view-логика, а не команды.
- Скриншот: `TriggerAction<FrameworkElement>` (`SaveScreenshotAction`) с DP `SavedCommand`;
  статус — состояние VM (`ScreenshotStatus`), TextBlock биндится, code-behind пустой.
- Info-кнопка: generic `MarkEventHandledAction` (ставит `RoutedEventArgs.Handled`) — семантика
  текущего `OnInfoButtonClick` сохраняется.
- Разбор бага — `docs\plans\2026-10-19-spine-transparency-vertex-colors-closed.md` (+ строка в `00_index.md`),
  минимальное репро вшивается в доку; ссылка из `presentation_wpf.md`.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] `Behaviors\SaveScreenshotAction.cs`, `Behaviors\MarkEventHandledAction.cs` (XML-доки).
3. [x] `SpineDemoViewModel`: `ScreenshotStatus` + `ScreenshotSavedCommand`; тест
   `SpineDemoViewModelTests` (путь → имя файла; null → «Screenshot failed»).
4. [x] XAML: `SpineDemoView` (EventTrigger → action, TextBlock → VM, убрать `Click=`/`x:Name`);
   `DuelUnitCardView` (EventTrigger → MarkEventHandled, удалить `OnInfoButtonClick`).
5. [x] Дока `docs\plans\2026-10-19-spine-transparency-vertex-colors-closed.md` (симптом → репро/сэмплы → причина →
   фикс → регресс-тест → gotcha при апгрейде SkiaSharp) + `00_index.md`; обновить
   `presentation_wpf.md` (behavior-обвязка, ссылка на разбор).
6. [x] Проверки: `check-using-placement`, `dotnet test` (76/76), WPF-билд; UIA-клик SCREENSHOT →
   PNG + статус.
7. [ ] Ручная проверка info-кнопки в дуэли (пользователь): таблица баффов открывается, цель не
   выбирается.

## Affected files

- Новые: `src\Wpf\Sektor.DarkestDungeon.Wpf\Behaviors\SaveScreenshotAction.cs`,
  `Behaviors\MarkEventHandledAction.cs`, `tests\Wpf\...\SpineDemoViewModelTests.cs`,
  `docs\plans\2026-10-19-spine-transparency-vertex-colors-closed.md`.
- Изменяемые: `Views\SpineDemoView.xaml(.cs)`, `Views\DuelUnitCardView.xaml(.cs)`,
  `ViewModels\SpineDemoViewModel.cs`, `docs\plans\00_index.md`,
  `docs\mechanics\presentation\presentation_wpf.md`, `PLAN.md`.

## Acceptance criteria

- В XAML не остаётся `Click=`; SCREENSHOT сохраняет PNG и пишет статус из VM; info-кнопка в дуэли
  ведёт себя как раньше.
- `check-using-placement` и `dotnet test` зелёные; WPF-билд без новых ошибок.
- Документ о белых прямоугольниках создан, проиндексирован и связан из спеки презентации.
