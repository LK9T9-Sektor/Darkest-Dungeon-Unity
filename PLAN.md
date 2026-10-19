# PLAN.md — планы задач

Активный план — ниже на этой странице. Завершённые планы переносятся в `docs\plans\` (индекс и
статусы — `docs\plans\00_index.md`); при завершении план уезжает в архив целиком, чтобы контекст
не терялся при работе с другого ПК — архив в git, `.opencode\` для планов не используется.

---

# PLAN: Spine-демо — все герои + выбор анимации и скорости

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
9. [ ] Коммит + пуш.

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

---

# PLAN: Spine/SCREENSHOT — Microsoft.Xaml.Behaviors + документ о белых прямоугольниках

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
- Разбор бага — `docs\plans\spine-transparency-vertex-colors.md` (+ строка в `00_index.md`),
  минимальное репро вшивается в доку; ссылка из `presentation_wpf.md`.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] `Behaviors\SaveScreenshotAction.cs`, `Behaviors\MarkEventHandledAction.cs` (XML-доки).
3. [x] `SpineDemoViewModel`: `ScreenshotStatus` + `ScreenshotSavedCommand`; тест
   `SpineDemoViewModelTests` (путь → имя файла; null → «Screenshot failed»).
4. [x] XAML: `SpineDemoView` (EventTrigger → action, TextBlock → VM, убрать `Click=`/`x:Name`);
   `DuelUnitCardView` (EventTrigger → MarkEventHandled, удалить `OnInfoButtonClick`).
5. [x] Дока `docs\plans\spine-transparency-vertex-colors.md` (симптом → репро/сэмплы → причина →
   фикс → регресс-тест → gotcha при апгрейде SkiaSharp) + `00_index.md`; обновить
   `presentation_wpf.md` (behavior-обвязка, ссылка на разбор).
6. [x] Проверки: `check-using-placement`, `dotnet test` (76/76), WPF-билд; UIA-клик SCREENSHOT →
   PNG + статус.
7. [ ] Ручная проверка info-кнопки в дуэли (пользователь): таблица баффов открывается, цель не
   выбирается.

## Affected files

- Новые: `src\Wpf\Sektor.DarkestDungeon.Wpf\Behaviors\SaveScreenshotAction.cs`,
  `Behaviors\MarkEventHandledAction.cs`, `tests\Wpf\...\SpineDemoViewModelTests.cs`,
  `docs\plans\spine-transparency-vertex-colors.md`.
- Изменяемые: `Views\SpineDemoView.xaml(.cs)`, `Views\DuelUnitCardView.xaml(.cs)`,
  `ViewModels\SpineDemoViewModel.cs`, `docs\plans\00_index.md`,
  `docs\mechanics\presentation\presentation_wpf.md`, `PLAN.md`.

## Acceptance criteria

- В XAML не остаётся `Click=`; SCREENSHOT сохраняет PNG и пишет статус из VM; info-кнопка в дуэли
  ведёт себя как раньше.
- `check-using-placement` и `dotnet test` зелёные; WPF-билд без новых ошибок.
- Документ о белых прямоугольниках создан, проиндексирован и связан из спеки презентации.

---

# PLAN: Spine 2.1 — скелетная анимация героя в WPF (прототип, демо-экран)

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
   После приёмки — коммит (по запросу) и, при желании, переезд плана в `docs\plans\`.
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

---

# PLAN: Фаза 2-A — фиксированная боевая сцена (пин высоты + оверлей)

## Goal

Заморозить центральную сцену боя (герои/враги + приклеенный HUD) от зависимости от аспекта:
камера пинит **высоту** кадра мира (141.9156 юнитов = 252.2945×9/16) вместо ширины, `UI_Overlay`
переводится на match-height, чтобы HP/стресс/лоток/рамка не отрывались от моделей. На 16:9
поведение не меняется (при 16:9 формулы эквивалентны; в тестовом рейде FOV 65.38171°, `dist`
считается в `Start()` и зависит от комнаты); на прочих аспектах расширяется/сужается только окружение по
бокам. Плита, чёрные поля вне 4:3–32:9 и решение по стенам комнат — этап B (после сверки A).

## Design decisions (user-confirmed)

- Full-bleed 4:3–32:9, бары вне диапазона — цель фазы (бары реализуются в этапе B).
- HUD неотделим от моделей (`FormationOverlaySlot.LateUpdate` проецирует позицию юнита; панели —
  статические оффсеты префаба `OverlaySlot`), поэтому фиксируется связка «модель+HUD» целиком.
- Фикс — по этапам: A = камера + оверлей + верификация по сетке аспектов; B = плита/бары/комнаты.
- Пиксельно-фиксированный стейдж через вторую камеру со слоями — не делаем (дороже; пин высоты
  даёт тот же эффект для вертикали и масштаба).
- Легаси-правки минимальные: точечная замена формул FOV, без рефактора сцены.

## Steps / status

1. [ ] PLAN.md зафиксирован.
2. [ ] Комната 16:9 (вопрос этапа B: покрывает ли стена видимые 252 юнита при её ~205) — снимок
   валиден и ПОСЛЕ A (на 16:9 ничего не меняется), снимается пользователем при верификации шага 5
   (зайти в данж-комнату, ресайз окна = триггер). Коридорный baseline 21:9 уже есть
   (`Dungeon-resize-2658x960-*`).
3. [x] `RaidPartyCamera.cs:25-26,68-76`: пин высоты — `frustumTargetHeight = frustumTargetWidth *
   9f / 16f` (141.9156); FOV = `2·atan(H/2/dist)`. На 16:9 значение не меняется (старая формула при
   16:9 эквивалентна; «26.62°» из ранней версии плана — ошибка от dist≈300 — реальный
   `frustumDistanceTarget` ≈ 110.5 → **65.38171°**, подтверждено CSV). BlurCamera зеркалит FOV
   автоматически (строка 60).
4. [x] `UI_Overlay` (`Dungeon.unity:67950`): CanvasScaler match 0→1. Инвентаризация детей: только
   4 — `RightMovementPointer`, `EncounterOverlay`, `PartyOverlay`, `LeftMovementPointer` (якорных
   панелей нет; стрелки движения станут height-relative — на 16:9 без изменений).
4b. [x] Селектор разрешения в окне настроек (`SoundSettingsUI`): строка «Разрешение» со степперами
   по 9 пресетам от 9:16 (портрет) до 32:9 (`Screen.SetResolution(..., FullScreenMode.Windowed)`);
   значение — текущие `Ш x В`; подписи — существующие ключи локализации
   (`menu_options_element_resolution` / `menu_options_value_seperation_resolution`). Нужен, потому
   что в билде окно не тянется — без него сетку аспектов не прогнать. Размеры пресетов подобраны
   так, чтобы аспект совпадал с редакторными (16:9=1920×1080, 1.6=1280×800, 4:3=960×720,
   2.14=1544×720, 2.2=1320×600, 21:9=1896×800, 2.77=1329×480, 32:9=1920×540, портрет=608×1080).
5. [ ] Верификация A (в процессе, прогоны 19.10 ночи):
   - [x] `unity-compile-check.ps1` зелёный; билд №2 (селектор + скрытие оверлеев на PNG) — ОК.
   - [x] 16:9: кадр совпал с pre-pin baseline `Dungeon-auto-t5s-20261018-235955-771.png`
     (слоты x=0.3808 идентично, y Δ≈1.4 px — idle-анимация).
   - [x] Пин высоты численно: на 2.77 слоты остались на тех же 419.25 юнитов (норм y 0.3882, как
     на 16:9 в том же месте карты) — вертикаль и масштаб от аспекта не зависят.
   - [x] Скрытие рантайм-канвасов на PNG подтверждено: ни панели настроек, ни шестерёнки.
   - [x] Плита обрезается снизу на 2.14/32:9 (`UI_RaidInterface`, match=0) — ожидаемо, объём B.
   - [ ] Чистый прогон оставшихся аспектов (4:3, 1.6, 21:9, 32:9) + комната 16:9 — рецепт в
     разделе «Требуется от пользователя»; портрет для A не требуется (вне полосы 4:3–32:9).
6. [ ] Доки A: `docs\mechanics\presentation\presentation_unity_urp_cameras.md` (пин высоты),
   `TESTING.md` (чек-лист аспектов).
7. [ ] Коммит на `core/unity7-urp` (сообщение по правилам; push — по запросу).

## Требуется от пользователя: чистый прогон сетки

В прогоне 19.10 клики по разрешению совпадали с триггерами — JSON/DTO/PNG снимались на **разных**
разрешениях (гонка с `Screen.SetResolution`), часть данных «лоскутная». Для приёмки A нужен
спокойный прогон. Рецепт на каждый аспект: шестерёнка → строка «Разрешение» → выбрать пресет →
**не кликать ~3 секунды** → F9. Снимки штатно падают в `SceneDumps` (папка вычищена 19.10),
панель настроек на PNG скрывается автоматически.

- [ ] 960×720 (4:3) — край полосы, обязателен.
- [ ] 1280×800 (1.6) — обязателен.
- [ ] 1896×800 (21:9) — обязателен.
- [ ] 1920×540 (32:9) — край полосы, обязателен.
- [ ] Комната 16:9 (шаг 2): на 1920×1080 зайти именно в комнату (не коридор), дождаться остановки
  камеры → F9 — решение по стенам для этапа B.
- 16:9 и портрет 608×1080 повторять не нужно (16:9 уже сверен с baseline; портрет — вне полосы).

После прогона сказать «снимки готовы» — агент разбирает PNG+CSV и закрывает шаг 5.

## Этап B (после сверки A, отдельный раздел при старте)

Плита RaidPanel тянется по полосе; верхние элементы (QuestLogPlate/TorchPanel/RoundIndicator) по
результатам; бары вне 4:3–32:9 (FOV-кламп + поля; проверить Screen Space Camera канвасы при
rect-камерах); решение по стенам комнат (>16:9) — растянуть/затайлить Image стены или принять.

## Affected files

- `unity\Assets\Scripts\Raid\RaidPartyCamera.cs`.
- `unity\Assets\Scenes\Dungeon.unity` (CanvasScaler UI_Overlay).
- `docs\mechanics\presentation\presentation_unity_urp_cameras.md`, `docs\TESTING.md`, `PLAN.md`.

## Acceptance criteria

- 16:9: FOV и позиции совпадают с baseline (PNG пиксель-в-пиксель; доли слота/панелей те же).
- 2.14/2.2/2.77: линия боя и слоты — те же доли, что на 16:9 (0.4209/…); HP/стресс/лоток прижаты
  к моделям; оверлей не растёт относительно высоты.
- 4:3/1.6: вертикальная композиция та же, по бокам видно меньше мира; без обрезков сцены.
- Портрет: документированный interim (сцена заморожена, поля — этап B).
- `unity-compile-check.ps1` зелёный; `unity-2017\` и `src\External\` не тронуты.

## Cleanup (по завершении этапа A)

- [x] 19.10: `SceneDumps` вычищен — 1010 МБ → 47 МБ. Оставлены pre-pin эталоны (невоспроизводимы):
  `Dungeon-auto-t5s-20261018-235955-771.*` (16:9, сверка шага 5) и
  `Dungeon-resize-2658x960-20261019-000051-101.*` (коридор 2.77, baseline шага 2), плюс PNG+CSV
  старых кадров 3088x1440 / 2640x1200 / 520x325 / 677x423 («до» для этапа B).
- [x] 19.10: `%TEMP%\unity-*.log` — удалено 12 старых логов, освобождено 7,1 МБ; оставлены 2
  последних (успешный билд №2 `unity-build-20261019-012552.log` и падение на залоченном
  `Assembly-CSharp.dll` `unity-build-20261019-011808.log`).
- [ ] После приёмки A и старта B — удалить снимки чистого прогона (этап B снимет свои).

---

# PLAN: Unity 7 (7000.0.0a10) — URP-миграция после апгрейда

## Goal

Проект апгрейднут на Unity 7000.0.0a10 (альфа), где Built-in RP отключён (рендер чёрный, баннер
«Your project has no render pipeline asset») и Dynamic Batching удалён. Цель: (1) убрать сообщение о
Dynamic Batching; (2) перевести активный `unity\` на URP по официальному гайду
(`docs.unity3d.com/7000.0/.../upgrading-from-birp.html`); (3) заменить fixed-function пассы
Spine-шейдеров на URP-совместимые (иначе герои/монстры не рисуются); (4) привести инструменты к
редактору 7000.0.0a10; (5) зафиксировать в доках. `unity-2017\` и `src\External\` не трогаются.

## Design decisions (user-confirmed)

- Откат на 6.x LTS не делаем — остаёмся на Unity 7 + URP.
- `SkeletonLit` (освещение героев) — порт с освещением отложен; временно unlit (имя и свойства
  шейдера сохраняются), потеря свечения — в KNOWN_ISSUES.
- Шаги в редакторе (Dynamic Batching toggle, установка URP, URP asset, Graphics, конвертер)
  выполняет пользователь; файловую часть (шейдеры, манифест при необходимости, tools, доки) — агент.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] Dynamic Batching → 0: галка в UI серая (настройка помечена legacy в Unity 7) — агент поправил
   `ProjectSettings.asset` при закрытом редакторе (коммит `29e7288`; уходит LogError
   `PlayerSettingsBatchingWarning`).
3. [x] Пользователь: Package Manager → Unity Registry → Universal Render Pipeline → Install
   (`manifest.json`: URP 17.7.0).
4. [x] URP Asset создан пользователем (`Assets\Settings\DarkestDungeonURP[,_Renderer].asset`); Default
   Render Pipeline прописан агентом в `GraphicsSettings.asset` (guid `3c6b23f4...`, коммит `29e7288`).
4b. [x] Агент: удалены 127 устаревших .NET-фасадов (gitignored) из `Assets\Plugins\Internal` —
   дублировали `unity-4.8-api\Facades` Unity 7 и давали `error CS1703`; остались только `Sektor.*`.
5. [x] Пользователь: Window > Rendering > Render Pipeline Converter → Source: Built-in, Target:
   URP (Universal Renderer) → Rendering Settings + Material Reference Converter + Material Shader
   Converter → Initialize and Convert. Fallout закоммичен (коммит A секции URP-диагностики).
6. [x] Агент: порт fixed-function Spine-шейдеров на CG unlit v/f (имена/свойства те же):
   `unity\Assets\Spine\SpineUnity\Shaders\Skeleton.shader`,
   `unity\Assets\Spine\SpineUnity\Shaders\SkeletonLit.shader`.
7. [x] Агент: tools — `knownRoots` 6000.5.8f1 → 7000.0.0a10 в `unity-compile-check.ps1`,
   `unity-build-game.ps1`, `unity-provision-plugins.ps1`, `unity-generate-battle-test-scene.ps1`.
8. [ ] Пользователь: прогон Intro → CampaignSelection → EstateManagement → Dungeon (герои/монстры
   видимы, Death's Door пульс, грейскейл, торч-флейм, UI, карта).
9. [x] Агент: `unity-compile-check.ps1` при закрытом редакторе — зелёный (компиляция +
   script-reference check, уже после правок шага 4 и чистки фасадов).
10. [ ] Агент: доки — `UNITY_MIGRATION.md` §6 (миграция 6.5 → 7.0, URP), `ARCHITECTURE.md` (Unity 7),
    `KNOWN_ISSUES.md` (BlurOptimized мёртв под URP; SkeletonLit unlit; фасады 2017 удалены),
    `TESTING.md` (URP-чеклист), `COMPABILITY.md`.

## Affected files

- `unity\ProjectSettings\ProjectSettings.asset` (m_DynamicBatching → 0 — файловая правка: галка в UI
  серая).
- `unity\Packages\manifest.json` + `packages-lock.json` (URP, через Package Manager).
- Новые: `unity\Assets\Settings\*.asset` (URP Asset + Renderer), `.meta` обязательны.
- `unity\Assets\Spine\SpineUnity\Shaders\Skeleton.shader`, `SkeletonLit.shader`.
- `tools\unity-*.ps1` (4 файла), `PLAN.md`.
- Доки: `docs\UNITY_MIGRATION.md`, `docs\ARCHITECTURE.md`, `docs\KNOWN_ISSUES.md`,
  `docs\TESTING.md`, `docs\COMPABILITY.md`.

## Acceptance criteria

- Оба сообщения в консоли исчезли; Game view не чёрный (URP asset назначен).
- Герои и монстры видимы в бою, Death's Door пульс и грейскейл-материалы работают.
- `pwsh tools\unity-compile-check.ps1` зелёный на 7000.0.0a10 (редактор закрыт).
- Легаси (`unity-2017\`, `src\External\`) не изменён.

## Out of scope

- Порт освещения SkeletonLit (URP lit) — отдельная задача.
- Замена BlurOptimized (showoff-блюр) URP-аналогом — отдельная задача.
- Мёртвый `CharacterSprite.shader` (Spine/DungeonSprite, 0 ссылок) — не трогаем.
