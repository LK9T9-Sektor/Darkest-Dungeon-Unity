# PLAN.md — активный план

## Срез: иконки способностей в WPF + превью скиллов на SPINE TEST

Цель: доставить в WPF иконки боевых способностей (72×72), показать их в строке «SKILLS» на
SPINE TEST через переиспользуемый `SkillSquareView` (иконка + имя + структурированный тултип),
проигрывать эффект скилла по клику один раз с возвратом предыдущей анимации и заполнить иконки
в полосе способностей дуэли/PvE.

Ветка: `core/skill-icons-wpf` (от `core/hero-icons-wpf`).

### Шаги

1. [x] Ядро: `HeroClassFileParser` читает `art:`-секцию (`CombatSkillPrefix`/`RiposteSkillPrefix`),
   `.end` корректно завершает секции; `SkillArtInfo` — конструктор из словаря + `GetText`/`GetFlag`/
   `GetNumber`; тесты парсера обновлены (71/71).
2. [x] `tools\hero-icons-content.ps1` линкует `ability.<icon>.png` и валидирует art-иконки;
   сгенерирован `HeroIconsContent.props` — 180 записей (75 портретов/баннеров + 105 иконок).
3. [x] `Data\IHeroSkillCatalog` / `HeroSkillCatalog` / `HeroSkillInfo` — соединение распарсенных
   скиллов, `SkillArtInfo` и иконок; `GetSkills`, `GetSkillIcon(classId, skillId)`.
4. [x] `IHeroIconCatalog.GetAbilityIcon` + реализация (frozen-кэш, отсутствие файла → `null`).
5. [x] `ISpineDemoCatalog.FindAnimationSet` + реализация: поиск сета по суффиксу
   `.sprite.<anim>.skel` (лечит опечатку папки `ZealosAccusation`), кэш.
6. [x] `SkillSquareView`: DP `IconSize` (по умолчанию 42; кнопка = размер + 8, корень = размер + 16,
   нижний кламп 16), `HighQuality` для иконок.
7. [x] `SpineDemoViewModel`: DI `IHeroSkillCatalog`, коллекция `Skills`, `SelectSkillCommand`,
   one-shot превью эффекта (`Loop=false`) и восстановление предыдущего сета; фолбэк на базовую
   позу для скиллов без `.fx`; отмена при ручной смене анимации/героя.
8. [x] `SpineDemoView.xaml`: строка «SKILLS» (`WrapPanel`, `SkillSquareView IconSize="72"`),
   привязки `Loop`/`AnimationCompletedCommand`; `DuelClasses.AllClasses`.
9. [x] Дуэль/PvE: `DuelBattleViewModel`/`PveBattleViewModel` заполняют `DuelSkillViewModel.Icon`
   через `IHeroSkillCatalog`; лобби получают каталог, `MainWindow` раздаёт общий экземпляр.
10. [x] Тесты: `HeroSkillCatalogTests`, `SkillSquareViewTests`, расширены `HeroIconCatalogTests`,
    `SpineDemoCatalogTests`, `SpineDemoViewModelTests`, фейки боевых VM; 154/154 зелёные.
11. [x] Документация: `docs\mechanics\presentation\presentation_wpf.md`, `docs\TESTING.md`,
    `docs\changelog\2026-10-21-skill-icons-wpf.md` + `00_index.md`.
12. [x] Коммит и пуш ветки `core/skill-icons-wpf` (файлы Unity пользователя не трогаются).

### Затронутые файлы

`src\Core\Sektor.DarkestDungeon.Core.Combat\{Character\HeroClassFileParser.cs,
Mechanics\Skills\SkillArtInfo.cs}`, `tools\hero-icons-content.ps1`,
`src\Wpf\Sektor.DarkestDungeon.Wpf\{HeroIconsContent.props, MainWindow.xaml.cs,
Combat\DuelClasses.cs, Data\{IHeroIconCatalog,HeroIconCatalog,ISpineDemoCatalog,SpineDemoCatalog,
IHeroSkillCatalog,HeroSkillCatalog,HeroSkillInfo}.cs, ViewModels\{SpineDemoViewModel,
DuelBattleViewModel,PveBattleViewModel,DuelLobbyViewModel,SinglePlayerLobbyViewModel,
PveLobbyViewModel}.cs, Views\{SpineDemoView.xaml, SkillSquareView.xaml(.cs)}}`,
`tests\Core\...HeroClassFileParserTests.cs`, `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\...`, `docs\...`.

### Критерии приёмки

- `pwsh tools\hero-icons-content.ps1` → 180 записей, exit 0; 105/105 иконок существуют (72×72).
- SPINE TEST: строка из 7 квадратов с иконками 72×72, именами и тултипом; клик играет эффект
  один раз и возвращает предыдущий сет; у 9 скиллов без `.fx` играет базовая поза.
- Дуэль/PvE: в полосе способностей видны иконки; неизвестный скилл → пустой квадрат без падения.
- `dotnet test tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests` — 154/154; core-тесты — 71/71.
- Unity-файлы пользователя (`UpgradeButton.mat`, `HeroSlot.prefab`) не изменены.

### Временные артефакты

- `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\TempSpineRender.cs` и
  `...\bin\Debug\net8.0-windows\spine-skills.png` — временный offscreen-рендер для визуальной
  проверки строки SKILLS; удалены, в репозиторий не входят.
