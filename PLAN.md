# PLAN.md — активный план

## Цель

Оставшийся срез: Unity-вид оверлея героя — стоящий герой из Spine Idle и полный гильд-баннер.
Capture расширять только если понадобится точная экипировка/внешний вид.

## Срез 4 (ожидает выполнения)

**Шаги:**

- [ ] Добавить каталог Spine-контента героя (class id → `Content\Spine\<Hero>\Idle`), переиспользуя
      `SpineDemoCatalog`/`SpineHeroControl`, с безопасным `null` при отсутствии контента.
- [ ] Показать стоящего героя в `HeroEditorOverlayView` слева на фоне гильд-баннера; при смене
      класса контент переключается вместе с карточкой.
- [ ] Оставить fallback: если Spine Idle не найден — текущий `HeroPortraitView`/плейсхолдер.
- [ ] Обновить `docs/TESTING.md` и changelog.

**Файлы:**

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroEditorOverlayView.xaml(.cs)`;
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\HeroSlotViewModel.cs`;
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Data\SpineDemoCatalog.cs` / `ISpineDemoCatalog.cs`;
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\SpineHeroControl.cs`;
- `docs\TESTING.md`, `docs\changelog\`.

**Критерии приёмки:**

- В оверлее слева виден анимированный стоящий герой, меняется при смене класса.
- При отсутствии/битом Spine-контенте оверлей не падает и показывает fallback.
- Существующие WPF-тесты зелёные.

**Проверка:** сборка WPF + `dotnet test`; визуальная проверка — за пользователем.
