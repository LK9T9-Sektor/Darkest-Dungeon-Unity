# PLAN.md — активный план

## Срез: иконки героев в WPF + карточка выбранного героя (SPINE TEST)

Цель: доставить в WPF иконки всех 15 героев (roster-портреты A–D и гильдейские баннеры), показать
иконку выбранного героя слева от блока селекторов на экране SPINE TEST через переиспользуемый
`UserControl`.

Ветка: `core/hero-icons-wpf`.

### Шаги

1. [x] `tools\hero-icons-content.ps1` — генератор `HeroIconsContent.props`: линкует
   `unity\Assets\Resources\Data\Heroes\Sprites\<class_id>\<A-D>\<class_id>_portrait_roster.png` в
   `Content\HeroIcons\<class_id>\portrait.<A-D>.png` и `<class_id>_guild_header.png` в
   `Content\HeroIcons\<class_id>\header.png`; валидирует 4 портрета + баннер на героя и совпадение
   набора героев со spine-набором (`unity\Assets\Sprites\Heroes`), иначе exit 1.
2. [x] Сгенерировать `src\Wpf\Sektor.DarkestDungeon.Wpf\HeroIconsContent.props` (75 записей) и
   импортировать его в `Sektor.DarkestDungeon.Wpf.csproj`.
3. [x] `src\Wpf\...\Ui\HeroIds.cs` — `ToClassId` (`ManAtArms` → `man_at_arms`).
4. [x] `src\Wpf\...\Data\IHeroIconCatalog.cs` + `Data\HeroIconCatalog.cs` — `ImageSource? GetPortrait(
   classId, outfit)` по `Content\HeroIcons\<classId>\portrait.<OUTFIT>.png`, frozen `BitmapImage`,
   кэш по пути, отсутствие файла/битый файл → `null`.
5. [x] `src\Wpf\...\Views\HeroPortraitView.xaml(.cs)` — `UserControl`: DP `Portrait` (ImageSource),
   `Caption` (string), `PortraitSize` (double, 96), плейсхолдер `?`, подпись под рамкой,
   `BitmapScalingMode=HighQuality`.
6. [x] `SpineDemoViewModel`: DI `IHeroIconCatalog`; `CurrentHeroClassId`, `CurrentHeroName`,
   `CurrentHeroPortrait` (аутфит `A`), уведомления при смене героя. `MainWindow` создаёт каталог.
7. [x] `SpineDemoView.xaml`: строка селекторов → `Grid` (карточка героя слева, селекторы по центру,
   симметричный спейсер справа).
8. [x] Тесты: `HeroIconCatalogTests` (15×4 + баннеры в output-контенте, unknown → null, кэш),
   `HeroIdsTests`, `SpineDemoViewModelTests` (портрет следует за героем + change notification),
   `ScreenSmokeTests` (`HeroPortraitView`).
9. [x] Документация: `docs\mechanics\presentation\presentation_wpf.md`, `docs\TESTING.md`,
   `docs\changelog\2026-10-21-hero-icons-wpf.md` + `00_index.md`.

### Затронутые файлы

`tools\hero-icons-content.ps1`, `src\Wpf\Sektor.DarkestDungeon.Wpf\{HeroIconsContent.props,
Sektor.DarkestDungeon.Wpf.csproj, Data\IHeroIconCatalog.cs, Data\HeroIconCatalog.cs, Ui\HeroIds.cs,
Views\HeroPortraitView.xaml(.cs), Views\SpineDemoView.xaml, ViewModels\SpineDemoViewModel.cs,
MainWindow.xaml.cs}`, `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\{HeroIconCatalogTests.cs,
HeroIdsTests.cs, SpineDemoViewModelTests.cs, ScreenSmokeTests.cs}`, `docs\...`.

### Критерии приёмки

- `pwsh tools\hero-icons-content.ps1` → 75 записей, exit 0; повторный запуск даёт тот же файл.
- В output WPF и тестов лежат портреты 15×4 и 15 баннеров.
- SPINE TEST: слева от селекторов карточка с иконкой текущего героя и его именем; меняется при
  листании ◀/▶.
- `HeroPortraitView` — отдельный `UserControl` с DP, без привязки к демо-экрану.
- Отсутствующий/битый файл иконки → `?`, без исключения и без падения экрана.
- `dotnet test tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests` зелёный; Unity не затронут.

### Временные артефакты

- `C:\Users\lk9t9\AppData\Local\Temp\opencode\spine-demo.png` (39 500 Б) — рендер SPINE TEST для
  проверки раскладки; `hero-card-placeholder.png` (2 381 Б) — проверка плейсхолдера `?`.
  Удаляются по завершении; в репозиторий не входят. Генератор и props — постоянные файлы.
