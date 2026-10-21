# PLAN.md — активный план

## Цель

Переиспользуемые WPF-контролы для выбора героя и его активных способностей: карточка героя со
стрелками и именем, общий ряд абилок, оверлей выбора героя в лобби VS AI / мультиплеер / PvE и
приведение SPINE TEST к этому виду.

## Срезы

- [x] Срез 1. Переиспользуемая карточка героя со стрелками + раскладка SPINE TEST.
- [x] Срез 2. Переиспользуемый ряд абилок + правило Abomination (`can_select_combat_skills`) +
      иконки способностей в слотах лобби.
- [x] Срез 3. Компактные слоты лобби + full-window `LobbyHeroEditorHost` с широким
      `HeroEditorOverlayView` (гильд-фон, статы/оружие/тринкеты, абилки, квирки, резисты).
- [ ] Срез 4. Unity-вид: стоящий герой (`Content\Spine\<Hero>\Idle`), полный гильд-баннер,
      при необходимости расширение capture под экипировку.

После каждого среза — стоп и ожидание проверки пользователем; следующий срез только по команде.

## Срез 1 (выполнен, ожидает проверки)

**Цель:** карточка героя становится переиспользуемой (портрет + имя + опциональные стрелки
листания класса), а SPINE TEST перестраивается под неё.

**Файлы:**

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroPortraitView.xaml(.cs)` — добавить DP
  `ShowArrows`, `PreviousCommand`, `NextCommand`, `SelectCommand`; стрелки `◀`/`▶` вокруг
  портрета; при `SelectCommand = null` портрет не кликается.
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\SpineDemoView.xaml` — убрать блок `CHARACTER`;
  поставить стрелки вокруг карточки героя; перенести `SKILLS` вправо/выше карточки;
  `ANIMATION` и `SPEED` — в нижнюю строку; `Pause`, `Screenshot` и статус — справа в той же
  нижней строке.
- `docs\mechanics\presentation\presentation_wpf.md`, `docs\TESTING.md`,
  `docs\FEATURE_DESKTOP_CLIENT.md` — обновить описание SPINE TEST и карточки героя.
- `docs\changelog\2026-10-21-spine-test-hero-card.md` + строка в `docs\changelog\00_index.md`.

**Критерии приёмки:**

- На SPINE TEST нет блока `CHARACTER`; `◀`/`▶` листают героя; `SKILLS` — справа от карточки;
  `ANIMATION`/`SPEED` — ниже; `Pause`/`Screenshot`/статус — в одной нижней строке справа.
- Смена героя, смена анимации, скорость, one-shot превью скилла и скриншот работают как раньше.
- `HeroPortraitView` с `ShowArrows="False"` выглядит как прежняя карточка (для боя/read-only мест).
- WPF-тесты зелёные; Unity и ядро не трогаются.

**Проверка:** сборка WPF + `dotnet test` — 156/156; визуальная проверка экрана — за пользователем.

## Срез 2 (выполнен, ожидает проверки)

- `AbilityRowView` (`ItemsSource`, `SelectCommand`, `IconSize`, `ShowNames`) — общий ряд скиллов
  для SPINE TEST и лобби; `ISkillTileViewModel` — общий контракт для `DuelSkillViewModel` и
  `LobbySkillViewModel`.
- `LobbySkillViewModel` получил иконку, читаемое имя, `Level`/`BaseInfo`/`EffectRows` и состояние
  `IsSelected`/`IsUsable`; `HeroSlotViewModel` получает `IHeroIconCatalog`/`IHeroSkillCatalog`,
  заполняет портрет и иконки, берёт `CanSelectSkills` из `HeroClass.CanSelectCombatSkills`.
- При 4 выбранных невыбранные скиллы серые и не выбираются; снятие одного снова делает их
  доступными. У Abomination все 7 выбраны, клики игнорируются, в слоте видна подпись
  `skills locked`.
- Проверка: сборка WPF + `dotnet test` — 161/161; визуальная проверка — за пользователем.

## Срез 3 (доработан, ожидает проверки)

- Слот лобби сокращён до компактной карточки: портрет 56, имя класса, стрелки `◀`/`▶` и подсказка
  `RMB: skills`. Статы, скиллы, квирки и тринкеты переехали в оверлей.
- ПКМ по слоту открывает full-window `LobbyHeroEditorHost` (в VS AI один хост на две панели).
  Широкий `HeroEditorOverlayView` (до 1120×700): гильд-картинка `Header` фоном от левого верхнего
  угла, шапка `#позиция / иконка со стрелками / имя / класс / X`, слева статы + оружие/броня +
  тринкеты, справа `AbilityRowView` (56), квирки в две колонки и таблица резистов; центр пока
  свободен.
- Выбранные скиллы — оранжевая рамка, невыбранные — серая рамка + полупрозрачная маска поверх
  иконки (иконка не исчезает); тултип работает и на недоступных/read-only квадратах.
- Изменения сразу уходят в `HeroSlotViewModel`, отдельной кнопки «Применить» нет. Abomination —
  все 7 и `skills locked`.
- Проверка: сборка WPF + `dotnet test` — 166/166; визуальная проверка — за пользователем.

## Срез 4 (кратко)

- Стоящий герой из `Content\Spine\<Hero>\Idle` через `SpineHeroControl`, полный Unity-вид
  `CharacterWindow`; capture расширяется только если понадобится точная экипировка.
