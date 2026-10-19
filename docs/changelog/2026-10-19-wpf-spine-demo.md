# WPF: Spine-демо — скелетная анимация героя (прототип)

- **Кнопка SPINE TEST в главном меню** → отдельный экран с idle-циклом Crusader: скелет Spine 2.1.18
  (`crusader.sprite.idle`, binary + atlas) играет в реальном времени, PAUSE/Resume, возврат в меню
  крестиком. Ассеты линкуются из unity-контента (без копирования).
- **Селекторы героя, анимации и скорости** на демо-экране: стрелками ◀/▶ листаются все 15 героев,
  наборы анимаций героя (`Skeleton\`, Idle первым; при смене героя чужая анимация сбрасывается в
  Idle) и скорость воспроизведения 100…1000% (шаг 50%, wrap). Контент — `Content\Spine\<Hero>\<Anim>\`
  (591 линк генерирует `tools\spine-demo-content.ps1` в `SpineDemoContent.props`).
- **Рантайм — общая библиотека** `src\Spine\Sektor.DarkestDungeon.Spine` (netstandard2.0): компилирует
  вендорные `unity\Assets\Spine\SpineSharp\Source` по месту (одна физическая копия) — задел под
  переиспользование в Unity (своп копий на общую DLL — отдельная задача). Загрузка/плеер/рендер —
  `src\Wpf\Sektor.DarkestDungeon.Spine.Wpf` (SkiaSharp, `DrawVertices` + шейдер атласа; бленды
  normal/additive/multiply/screen). Использование в карточках дуэли — отдельная задача.
- **Кнопка SCREENSHOT на демо-экране** сохраняет PNG текущего окна в `screenshots\` рядом с exe
  (имя `screenshot-<timestamp>.png`, имя файла видно в строке статуса) — для ручной проверки и
  обмена снимками; прозрачность атласа при рендере сохраняется (тинт через `ColorFilter`, без
  white-артефакта vertex colors SkiaSharp 2.88).
