# PLAN.md — Пост-рефакторный коммит: вернуть разметку, починить пумп лобби, убрать переусложнённость

## Цель

После коммита большого WPF-рефактора (навигация/меню/vs AI/ростер):
1. Вернуть в текущий код информативную разметку из предыдущего коммита — строку `Stress` на карточках юнитов боя.
2. Починить найденный баг: `DuelLobbyViewModel` не реализует `IPumpable`, поэтому пумп мультиплеер-лобби (транспорт + таймер ожидания) не работает.
3. Убрать мёртвый код и дублирование pump-таймера. Пограничные абстракции (`INavigationService`, `IDuelRivalLink`) оставить.

## Шаги

1. [x] **S1** Коммит и пуш стейдженного рефактора в `origin/wpf` (сообщение «Add WPF screen navigation, vs AI mode and full hero roster.»).
2. [x] **S2** `Views\DuelBattleView.xaml`: в `DuelUnitTemplate` добавить `TextBlock` `{Binding Stress, StringFormat=Stress {0}}` (свойство `Stress` во вью-модели уже есть).
3. [x] **S3** `ViewModels\DuelLobbyViewModel.cs`: добавить `IPumpable` к списку реализуемых интерфейсов (метод `Pump()` уже есть).
4. [x] **S4** `Views\DuelLobbyView.cs`: удалить пустой `OnDataContextChanged` + подписку, неиспользуемые `using`.
5. [x] **S5** Новый `Views\PumpableScreenBase.cs` (общий DispatcherTimer 50мс + `Loaded`/`Unloaded` + `(DataContext as IPumpable)?.Pump()`); `DuelLobbyView` и `DuelBattleView` наследуются от него.
6. [x] **S6** Проход по переусложнённости: только баг-фиксы/дубликаты (пп. S3–S5), абстракции не трогаем.
7. [x] **S7** Проверка: `dotnet build` WPF + Core.Combat (0 ошибок); тесты WPF / Core.Combat / Core.Content — зелёные.
8. [x] **S8** Документация: `TESTING.md` (Stress на карточках боя), `CHANGELOG.md` (починка пумпа лобби).
9. [x] **S9** Финальный коммит и пуш (сообщение «Fix multiplayer lobby pump, restore stress on duel cards and share screen pump.»).

## Затронутые файлы

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml` (S2)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\DuelLobbyViewModel.cs` (S3)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelLobbyView.cs` (S4, S5)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.cs` (S5)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\PumpableScreenBase.cs` (новый, S5)
- `docs\TESTING.md`, `docs\CHANGELOG.md`, `PLAN.md` (S8)

## Приёмка

- [ ] На карточках юнитов боя виден «Stress N».
- [ ] Мультиплеер-лобби: транспорт пумпится, счётчик «Waiting mm:ss» тикает (проверяется ручным запуском).
- [ ] Оба экрана (лобби и бой) используют общий pump-базис без дублирования.
- [ ] `dotnet build` 0 ошибок; тесты WPF/Combat/Content зелёные.