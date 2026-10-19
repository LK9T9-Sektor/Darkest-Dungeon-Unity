> Закрыт: 2026-10-19 (ветка `core/unity7-urp`). Коммиты: `2a746aa` (прототип + демо), `9b20728`
> (селекторы), `4c7e36b` (проекты Spine в решении). Отдельного файла у плана не было — он оставался в
> `PLAN.md`; заархивирован при переносе планов. Индекс — `docs\plans\00_index.md`.

# PLAN: Spine 2.1 — скелетная анимация героя в WPF (прототип, демо-экран)

## Что сделано

- Общая netstandard2.0-библиотека `src\Spine\Sektor.DarkestDungeon.Spine`: сорсы spine-csharp 2.1 из
  `unity\Assets\Spine\SpineSharp\Source` компилируются по месту (одна физическая копия), свой
  `AssemblyInfo.cs` с InternalsVisibleTo — задел под этап 2.
- Рендер-обвязка `src\Wpf\Sektor.DarkestDungeon.Spine.Wpf`: `SpineSkeletonLoader`, `SpinePlayer`,
  `SpineFrameRenderer` (SkiaSharp CPU; region/mesh/skinned-mesh; бленды
  normal/additive/multiply/screen).
- Демо-экран «SPINE TEST» в WPF: idle-цикл Crusader (Spine 2.1.18), PAUSE/Resume, возврат в меню,
  кнопка SCREENSHOT (PNG окна в `screenshots\`, `Ui\WindowScreenshot`).
- Селекторы героя/анимации/скорости — см. `2026-10-19-spine-demo-selectors-closed.md`; регресс
  прозрачности атласа — см. `2026-10-19-spine-transparency-vertex-colors-closed.md`.
- Тесты `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests`: загрузка idle, setup-поза, рендер двух кадров,
  прозрачность, smoke-тест вида; `unity\*` не изменён.

## Осталось

- Этап 2 и далее — в backlog `PLAN.md`: `git mv` сорсов в `src\Spine`, своп Unity/Unity-2017 на общую
  DLL Spine, монстры/остальные герои и анимации боя в карточке дуэли.

---

_Ниже — исходный план без изменений._

## Goal

Прототип: воспроизвести скелетную анимацию героя DD (Spine 2.1.18, `crusader.sprite.idle`)
в WPF-клиенте. Сначала демо-экран (idle-цикл Crusader), после ревью — карточка дуэли.
Архитектура сразу готовится к переиспользованию в `unity\` и `unity-2017\`: Spine-рантайм
собирается как отдельная netstandard2.0-библиотека (своп Unity на общую DLL — этап 2, отдельный PR).

## Design decisions (user-confirmed)

- Вендорный рантайм: spine-csharp 2.1 из `unity\Assets\Spine\SpineSharp\Source` (байт-в-байт
  идентичен в обоих деревьях, чистый C# без UnityEngine/UNITY_EDITOR). `src/External/` в
  `.gitignore` — вендорить туда нельзя; библиотека компилирует сорсы по месту (одна физическая копия).
- Рендер — SkiaSharp (CPU, `SKCanvas.DrawVertices` с шейдером атласа), без WPF-зависимостей в библиотеке.
- Загрузка: `SkeletonBinary` + `Atlas`, принимает `.skel`/`.skel.bytes`; текстура — outfit A PNG
  (имя из атласа сохраняется).
- Вход в демо — кнопка в главном меню.
- Этап 2 (не сейчас): `git mv` сорсов в `src\Spine`, удаление копий из обоих Unity-деревьев,
  DLL+pdb в оба `Plugins\Internal` (паттерн `CoreUnityPluginFolders`), InternalsVisibleTo
  (SpineUnity использует internal: `RegionAttachment.uvs` — `SkeletonRenderer.cs:384`),
  `unity-compile-check` обоих деревьев, ручной чек-лист в редакторах.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] Библиотека `src\Spine\Sektor.DarkestDungeon.Spine\` (netstandard2.0): сорсы по месту из
   `unity\Assets\Spine\SpineSharp\Source\**\*.cs` + свой `AssemblyInfo.cs` с InternalsVisibleTo
   (Assembly-CSharp, Assembly-CSharp-Editor) — задел под этап 2. Сборка чистая (0 warnings).
3. [x] Библиотека `src\Wpf\Sektor.DarkestDungeon.Spine.Wpf\` (net8.0): `SpineSkeletonLoader`,
   `SpinePlayer`, `SpineFrameRenderer` (Region/Mesh/SkinnedMesh → SKBitmap; тинт слота; бленды
   normal/additive/multiply/screen).
4. [x] WPF-клиент: ProjectReference на обе библиотеки; контент-линки в `Content\Spine\Crusader\`
   (`crusader.sprite.idle.skel`, `.atlas`, outfit A `crusader.sprite.idle.png`); SkiaSharp.Views.WPF.
5. [x] UI: `SpineHeroControl` (SKElement + CompositionTarget.Rendering), `SpineDemoView` +
   `SpineDemoViewModel`, кнопка «SPINE TEST» в главном меню + обвязка Shell (MainWindow DataTemplate +
   фабрика). Сборка приложения — 0 ошибок (44 warning — все существующие, не из новых файлов).
6. [x] Тесты `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests`: загрузка idle (версия 2.1.18/анимация/скин),
   setup-поза измерима сразу после `CreatePlayer`, рендер двух кадров (ненулевая альфа, кадры
   различаются) + регресс прозрачности: `DrawVertices` с vertex colors в SkiaSharp 2.88 красил
   прозрачные пиксели атласа в белый — тинт переведён на `SKColorFilter` (Modulate), colors не
   передаются; smoke-тест `SpineDemoView`; контент приходит транзитивно через ProjectReference.
   Все 74 теста зелёные.
7. [x] Доки: `docs\mechanics\presentation\presentation_wpf.md` (экран, трассировка, нюансы Spine),
   `docs\FEATURE_DESKTOP_CLIENT.md` (этап B — сделано), `docs\TESTING.md` (чек-лист демо + сводка).
8. [x] Проверки: сборки и тесты зелёные; `unity\*` не изменён (git status чист); ложные EOL-мутации
   `ProjectAuditorSettings.asset`/`QualitySettings.asset` откатаны.
9. [ ] Ручная приёмка демо (пользователь): SPINE TEST — плавность idle, PAUSE/Resume, возврат в меню.
   После приёмки — коммит (по запросу) и, при желании, переезд плана в `docs\plans\`. Фактически
   принято: экран использован и расширен (селекторы, скриншот), замечаний не было.
10. [x] Кнопка SCREENSHOT на демо-экране (запрос пользователя): `Ui\WindowScreenshot`
    (`RenderTargetBitmap` + PNG, учёт DPI) пишет окно в `screenshots\` каталога сборки, имя файла —
    в строке статуса; тест `WindowScreenshotTests` (74/74).

## Affected files

- Новые: `src\Spine\Sektor.DarkestDungeon.Spine\`, `src\Wpf\Sektor.DarkestDungeon.Spine.Wpf\`,
  `tests\Wpf\...` (новые тест-файлы), `SpineHeroControl`/`SpineDemoView`/`SpineDemoViewModel`,
  `Ui\WindowScreenshot.cs`.
- Изменяемые: `Sektor.DarkestDungeon.Wpf.csproj`, `MainMenuView.xaml(.cs)`, `MainMenuViewModel.cs`,
  Shell/MainWindow, `tests\Wpf\...csproj`.
- Доки: `docs\mechanics\presentation\presentation_wpf.md`, `docs\FEATURE_DESKTOP_CLIENT.md`,
  `docs\TESTING.md`, `docs\CHANGELOG.md`.

## Acceptance criteria

- Демо: idle Crusader играет плавно (≥30 FPS при ~400×500), без артефактов; SCREENSHOT сохраняет
  PNG окна в `screenshots\` каталога сборки.
- `dotnet build` и `dotnet test tests\Wpf\...` зелёные; библиотека — чистый netstandard2.0
  (без Unity/WPF/Skia зависимостей), готова к этапу 2.
- `unity\*` не изменён (git status).

## Out of scope (этап 2 и далее)

- Своп Unity/Unity-2017 на общую DLL Spine (отдельный PR после ревью прототипа).
- Монстры и остальные герои в карточке дуэли; интеграция анимаций боя (атаки, скиллы).
