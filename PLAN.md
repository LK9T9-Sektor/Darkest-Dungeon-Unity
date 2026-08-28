# PLAN.md — Активный план задач

## Задача: WPF-дуэль — фиксы UI (тултип, левая панель, Move/Pass, Абоминация, лобби)

### Цель

Привести боевой HUD дуэли и оба лобби (vs AI и мультиплеер) к единому широкоформатному
виду без перекрытий: починить обрезанный тултип, вернуть левой панели DD-корректный
состав, сделать Move/Pass квадратными с глифами, устранить крэш Абоминации при
превращении, перестроить лобби рядами сверху вниз с крестиком-возвратом сверху.

### Шаги

1. [x] **Крэш Абоминации** — в `DuelBattleView.xaml` убрана анимация
   `(UIElement.RenderTransform).(TranslateTransform.Y)` (замороженный Freezable в
   шаблоне карточки); анимируются только элементные DP — `Opacity` + `Margin`.
2. [x] **Левая панель (DD-корректно)** — `HeroBannerView`: убраны слоты скиллов;
   `HeroStatsView`: добавлен DP `ShowFullDetails` (default false), скрывающий секции
   SKILLS/RESISTANCES/QUIRKS; листы статов правого клика (`DuelBattleView`,
   `BattleScreenView`) — `ShowFullDetails="True"`.
3. [x] **Тултип** — `RaidHudView`: левая колонка 690→`Auto`, центр `*`;
   `UnitTooltipView`: убран жёсткий `Width="560"`, размер по контенту.
4. [x] **Move/Pass** — квадратные 64×64 как скиллы, глиф `⇄` у Move и `✕` у Pass,
   подпись под кнопкой.
5. [x] **Лобби** — новый общий `ScreenHeaderView` (заголовок + крестик-возврат);
   `DuelLobbyView` и `SinglePlayerLobbyView` → 1920×1080 рядами: верх = 4 героя игрока,
   середина = ИИ/второй игрок (неактивно для живого PvP), низ = кнопки.
6. [x] **Доки** — `TESTING.md` (WPF чек-лист + «Что проверить»), `CHANGELOG.md`;
   шаги здесь отмечены `[x]`. Добавлен `ScreenSmokeTests` (загрузка всех экранов).

### Затронутые файлы

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroBannerView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroStatsView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroStatsView.xaml.cs` (DP)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\RaidHudView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\UnitTooltipView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\BattleScreenView.xaml` (ShowFullDetails=True)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelLobbyView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\SinglePlayerLobbyView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\ScreenHeaderView.xaml` + `.xaml.cs` (новый)
- `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\ScreenSmokeTests.cs` (новый)
- `docs\TESTING.md`, `docs\CHANGELOG.md`, `PLAN.md`

### Критерии приёмки

- Превращение Абоминации не роняет WPF-клиент (`XamlParseException` исчез); обычные
  попапы урона/хила работают.
- Левая панель боя: портрет+имя, статы без резистов, шмот и 2 слота тринкетов; скиллов
  и секций SKILLS/RESISTANCES/QUIRKS нет. Лист статов правым кликом — со всем полным
  набором.
- Тултип при наведении полностью виден, не перекрывается левой панелью, не выходит
  за экран.
- MOVE/PASS квадратные (64×64), у MOVE стрелки, у PASS крестик, подписи снизу.
- Оба лобби на 1920×1080, элементы в рядах сверху вниз, ничего не прячется под другим,
  крестик-возврат сверху, единый стиль.
- `dotnet build` и `dotnet test tests/Wpf/...` зелёные; ручной прогон по `TESTING.md`.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Что уже сделано (зафиксировано)

WPF-дуэль работает: меню → лобби (классы, активные скиллы 4/7, черты+reroll, подсказки) →
бой (полный HUD, детерминированный локстап, Move/Pass, подробный лог, всплывающий урон,
HP блоками + стресс 10 квадратов, тултипы, лист статов со всеми скиллами/резистами/квирками).
Квирки влияют на статы (permanent-баффы `BuffSourceType.Quirk`). Вынесено в ядро: квирки,
буффы (`JsonBuffs.json`), константа активных скиллов, `SelectedCombatSkills`, `Hero.Quirks`.
Детали — в `CHANGELOG.md`, механики — в `FEATURE_DESKTOP_CLIENT.md` §«Механики боя».

## Осталось — по приоритету

### P1. Механики боя, которых ещё нет (самое ценное)

1. [ ] **Эффекты скиллов (статусы)** — парсер `.bytes` должен читать эффекты `combat_skill`
   (стун, блайт, блед, баффы/дебаффы, riposte, guard, пулл/пуш) и заполнять
   `CombatSkill.Effects`. Ядро готово (`BattleSolver.ApplyEffects`, классы эффектов в
   `Mechanics\Skills\Effects\`), но скиллы сейчас — чистый урон/хил (комментарий в
   `HeroClassFileParser.cs:13`). Самый большой механик-разрыв.
2. [ ] **Resolve / аффекция / добродетель / сердечный приступ** — триггер при 100 стресса
   (`Resolve`, `Trait`, `OverstressType` есть в ядре, вызова нет). Набор виртуадов/аффекций
   из контента (trait_buffs) пока не вынесен.
3. [ ] **Modes (Абоминация)** — парсинг `.mode` в контент, `CharacterMode`/`ModeEffects`
   (ядро поддерживает).
4. [ ] **Транкеты / экипировка** — Unity-дуэль несёт их с героя из имения; в WPF-дуэли нет.
   Большая фича (парсинг `JsonTrinkets`/`JsonBuffs`, слоты, влияние на статы).

### P2. Сопутствующее WPF-клиенту

5. [ ] **Факел/свет в бою** — механика света (light buffs, темнота) в Unity влияет на бой;
   сейчас факел — плейсхолдер.
6. [ ] **ИИ-соперник** — сейчас случайные легальные ходы (`AiRivalLink`); можно использовать
   `MonsterBrain`/предпочтения скиллов для «умного» противника.
7. [ ] **Локализация** — имена квирков/скиллов из `Localization\Quirks.xml` (сейчас id).

### P3. Полный вынос из Unity (фазы EXTRACTION_PLAN)

8. [ ] Фаза 2: сейвы (`src\Core\Save`).
9. [ ] Фаза 4: кампания/имение/здания/квесты/город.
10. [ ] Фаза 5: Photon-транспорт (WPF сейчас на Steam).
11. [ ] Фаза 6: тонкие адаптеры презентации в обоих Unity-проектах.

### P4. Проверка и качество

12. [ ] Ручной чек по `TESTING.md` (меню → vs AI → бой; мультиплеер 2 инстанса; лог/попапы/
    MOVE/PASS/черты).
13. [ ] Глядя на тесты: расширить покрытие локстапа (двусторонний одинаковый бой с квирками).

## Правила

- Сначала ядро (`src\Core`), потом адаптеры; `src\External\` — read-only.
- Доки обновляются в том же коммите; `CHANGELOG.md` — только user-visible.
- Всё новое — минимальными диффами, без оппортунистических рефакторов legacy.