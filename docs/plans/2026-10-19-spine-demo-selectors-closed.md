> Закрыт: 2026-10-19 (ветка `core/unity7-urp`). Коммиты: `2a746aa` (прототип + демо),
> `9b20728` (селекторы), `aa5fcaa` (статусы в PLAN.md), `4c7e36b` (проекты Spine в решении),
> `ad0d984` (доки про `.slnx`). Перенесён из `PLAN.md`; индекс закрытых планов —
> `docs\plans\00_index.md`.

# PLAN: Spine-демо — все герои + выбор анимации и скорости

## Что сделано

- Демо-экран SPINE TEST получил селекторы ◀/▶: все 15 героев (wrap), наборы анимаций героя (Idle первым, чужая анимация сбрасывается в Idle), скорость 100…1000% (шаг 50%, wrap); заголовок показывает выбранного героя.
- Контент: раскладка `Content\Spine\<Hero>\<Anim>\` (591 линк) генерируется `tools\spine-demo-content.ps1` → `SpineDemoContent.props`; csproj подключает `<Import>` вместо ручных 3 записей Crusader.
- Рантайм/вид: `SpinePlayer.TimeScale` (float, `AnimationState.TimeScale`), `SpineHeroControl.SpeedMultiplier` DP (кламп 0.01…100).
- Каталог `Data\ISpineDemoCatalog`/`SpineDemoCatalog` (ctor-DI в `MainWindow`); VM-состояние и команды Previous/Next для героя, анимации, скорости.
- Проверки: build зелёный, related-тесты 18/18 (`SpineDemoViewModelTests`, `SpineDemoCatalogTests`, `SpinePlayerTests`, `SpineSkeletonLoaderTests`, `SpineFrameRendererTests`, `ScreenSmokeTests`), `check-using-placement` OK; UIA-прогон в приложении — Crusader → GraveRobber → Hellion, Idle → Afflicted, 100% → 300%.
- Доки: `presentation_wpf.md`, `TESTING.md`, `CHANGELOG.md`, `FEATURE_DESKTOP_CLIENT.md`; оба Spine-проекта добавлены в корневое решение.

---

_Ниже — исходный план без изменений._

## Goal

Расширить демо-экран SPINE TEST: все 15 героев с пролистыванием стрелками, выбор анимации
группы (нередактируемое поле + стрелки ◀/▶, не выпадающий список), выбор скорости
воспроизведения (100%…1000%, стрелки). По завершении — коммит и пуш.

## Design decisions

- Контент: плоская раскладка `Content\Spine\<Hero>\<Anim>\` из `unity\Assets\Sprites\Heroes`
  (`skel.bytes` → `.skel`, `atlas.txt` → `.atlas`, PNG outfit A с исходным именем = страница
  атласа). Линковка — сгенерированный `SpineDemoContent.props` (~591 запись) скриптом
  `tools\spine-demo-content.ps1`; ручные 3 записи Crusader из csproj убираются.
- Каждый skel содержит ровно одну анимацию → выбор анимации = переключение папок
  (подпапки `Skeleton\` каталога героя); Idle первым, далее по алфавиту.
- Скорость: `AnimationState.TimeScale` — float; диапазон 1.0…10.0 (100%…1000%), шаг 0.5,
  wrap по краям; `SpinePlayer.TimeScale` (forwarder), `SpineHeroControl.SpeedMultiplier` DP.
- Каталог через ctor-DI: `ISpineDemoCatalog` (список героев/анимаций, путь контента) +
  `SpineDemoCatalog` поверх base directory; VM тестируется фейком.
- VM: `Characters`/`Animations`/`CurrentCharacter`/`CurrentAnimation`/`SpeedMultiplier`/
  `SpeedText`; команды Previous/Next (wrap), анимация сохраняется при смене героя, иначе Idle.
- UI: строка селекторов ◀ label ▶ (герой/анимация/скорость) над строкой PAUSE/SCREENSHOT.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] `tools\spine-demo-content.ps1` + сгенерированный `SpineDemoContent.props`; csproj: убрать
   ручные записи Crusader, подключить props.
3. [x] `SpinePlayer.TimeScale` (float) + тест; `SpineHeroControl.SpeedMultiplier` DP.
4. [x] `Data\ISpineDemoCatalog.cs`, `Data\SpineDemoCatalog.cs` (+ тест каталога).
5. [x] `SpineDemoViewModel` (состояние/команды) + `MainWindow.xaml.cs` (DI каталога) + тесты VM.
6. [x] `SpineDemoView.xaml`: селекторы ◀/▶ + биндинги (`ContentDirectory`, `SpeedMultiplier`).
7. [x] Проверки: build, related-тесты (18/18), `check-using-placement`; UIA-верификация (клики
   стрелок, смена героя/анимации/скорости, рендер без ошибок).
8. [x] Доки: `presentation_wpf.md`, `TESTING.md`, `CHANGELOG.md`, `FEATURE_DESKTOP_CLIENT.md`.
9. [x] Коммит + пуш.

## Affected files

- Новые: `tools\spine-demo-content.ps1`, `src\Wpf\Sektor.DarkestDungeon.Wpf\SpineDemoContent.props`,
  `src\Wpf\Sektor.DarkestDungeon.Wpf\Data\ISpineDemoCatalog.cs` / `SpineDemoCatalog.cs`,
  `tests\Wpf\...\SpineDemoCatalogTests.cs`, `SpinePlayerTests.cs`.
- Изменяемые: `Sektor.DarkestDungeon.Wpf.csproj`, `SpinePlayer.cs`, `SpineHeroControl.cs`,
  `SpineDemoViewModel.cs`, `SpineDemoView.xaml`, `MainWindow.xaml.cs`,
  `SpineDemoViewModelTests.cs`, доки.

## Acceptance criteria

- Стрелками листаются все 15 героев, анимации героя (Idle первой) и скорость 100…1000%
  (wrap, шаг 50%); каждая комбинация рендерится без ошибок загрузки.
- `dotnet build` и related-тесты зелёные; `check-using-placement` OK.
- Кнопка SCREENSHOT и ползунок/пауза не сломаны.
