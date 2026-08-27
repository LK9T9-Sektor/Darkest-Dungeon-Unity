# FEATURE_DESKTOP_CLIENT.md — Десктоп-клиент WPF

## Цель

Второй тонкий потребитель чистого ядра и сети — WPF-клиент (proof of concept): дуэль 1v1 с
переиспользованием логики из `src\Core` и транспортов Steam/Photon. Доказывает шов «логика без ссылок
на движок/UI»: один и тот же бой в Unity и в WPF; транспорт переиспользуем в другом клиенте.

## Состав

- **Расположение:** в DD-монорепо, `src\Wpf\Sektor.DarkestDungeon.Wpf\` (net8.0-windows, C# latest,
  ссылки на `src\Core` + `src\Lan`). Клиентская зона `src\` переопределяет
  `LangVersion`/`Nullable` из `src\Directory.Build.props` (см. AGENTS.md).
- **Зависимости (NuGet):** `CommunityToolkit.Mvvm` (ObservableObject/RelayCommand),
  `Microsoft.Xaml.Behaviors.Wpf`, `Newtonsoft.Json` (десериализация контента в адаптере).
- **Боевой экран:** MVVM-каркас по сцене боя (`unity\Assets\Scenes\Dungeon.unity`, canvas
  `UI_RaidInterface`) — три панели: верх (квест + квадратный «X» Retreat, факел, очередь хода),
  центр (юниты на «полу», номер раунда, всплывающий урон), низ (квадратные скиллы + MOVE + PASS,
  инфо ходящего, тултип в 2 колонки, LOG/INVENTORY/MAP). Карточки юнитов: HP блоками, стресс
  10 квадратами. Наведение — тултип, правый клик — лист статов со всеми скиллами и резистами.
- **Мини-слой дуэли** в `src\Wpf\...\Combat\DuelController.cs` (детерминированный локстап поверх
  `src\Core\Combat`): по сети идут только вводы (скилл+цель, pass/move) и `party_config`; сид сессии
  и генерация героев из сидов должны совпадать с Unity, чтобы стороны сходились.
- **Сеть:** `SteamTransport` (host/join по session id, уже работает); `PhotonTransport` — после Фазы 5.
- **Экраны (одно окно):** главное меню (VS AI / MULTIPLAYER) → лобби (выбор классов, активных
  скиллов и черт) → бой; `ShellViewModel` + `INavigationService` подменяют UserControl'ы.
- **Контент из unity (линк, без копирования):** `Data\Heroes\Info\*.bytes` (классы героев),
  `Data\JsonQuirks.json` (черты).
- **Этап A (без Spine):** плейсхолдеры — доказывает ядро+сеть+UI с нулевым риском.
- **Этап B (опционально):** Spine-контрол отдельным `UserControl` — spine-csharp 2.x (под ассеты DD,
  runtime 2.3) + SkiaSharp (фолбэк MonoGame.WpfCore); демо на sample-скелете 2.x. Аудио в PoC пропускаем.

## Механики боя (дуэль 1v1)

Дуэль 1v1: 2 отряда по 4 героя (классы — из контента `Heroes/Info`). Логика — `src\Core\Combat`
(`BattleSolver`, `Round`, `FormationParty`), оркестрация и снапшоты — `DuelController`/`DuelBattleViewModel`.

### Детерминизм (локстап)

Обе стороны строят **идентичные** отряды и гоняют одну симуляцию; по сети идут только вводы и
`party_config`. Тождество достигается: сид сессии (из id игроков), генерация героев из
индивидуальных сидов (`HeroGeneration`), одинаковый `party_config` (класс + сид + выбранные скиллы),
`RandomSolver` с фиксированным сидом на старте.

### Порядок хода

- Инициатива по скорости (`BattleGround.Round`), очередь раунда = `OrderedUnits`.
- Ход текущего юнита: `PreHeroTurn`/`PreMonsterTurn` → ожидание действия локальной стороны →
  `PostHeroTurn`/`PostMonsterTurn` → следующий юнит.
- Когда очередь пуста — новый раунд (`UpdateRound` для всех юнитов, `NextRound`).

### Действия хода (вводы)

| Ввод | Формат | Эффект |
|---|---|---|
| Скилл | `skillId\|targetId` | `BattleSolver.ExecuteSkill` по цели |
| Пропуск хода | `pass\|0` | `CompleteTurn()` без действия |
| Перемещение | `move\|rank` | обмен рангами с союзником в соседнем ранге (детерминированный `TryMove`) |

Перемещение меняет достижимость скиллов: `LaunchRanks`/`TargetRanks` проверяются от текущего ранга.

### Скиллы

- Категории: `Damage`, `Heal`, прочие (`SkillCategory`).
- Диапазоны: `LaunchRanks`/`TargetRanks` (`FormationSet`: ранги, self/party/random/multitarget),
  `Accuracy` (модификатор от оружия), `CritMod`, `DamageMod`, `Heal` (`HealComponent`).
- **Активные скиллы**: `Hero.SelectedCombatSkills` (макс. `number_of_selected_combat_skills_max`:
  4 у всех классов, 7 у Абоминации). `CurrentCombatSkills` = активные (или все, если выбор пуст).
  Выбор — в лобби (квадратные кнопки), передаётся в `party_config`.

### Результат удара

`BattleSolver.SkillResult.SkillEntries` — на каждую цель: тип
`Hit/Miss/Crit/Dodge/Heal/CritHeal/Utility`, `Amount`, `IsZeroed` (убийство). Из него формируются:
подробный лог боя и всплывающие числа над юнитами (урон/крит/хил/промах/уклонение).

### Характеристики героя

HP (текущее/макс), Stress 0–100, Speed, DMG (мин–макс), ACC (+%), CRIT (%), DODGE, PROT (%).
Сопротивления: stun, blight (poison), bleed, debuff, move, disease, death blow, trap (%). Читаются из
атрибутов персонажа (`GetSingleAttribute`/`GetPairedAttribute`).

### Стресс

0–100; на карточке юнита — 10 квадратов: 0–50 обычные (нормальные), 50–100 «stressed»
(в `StressOverlayPanel` Unity — переключение спрайтов на 50%).

### Черты (квирки)

Из `JsonQuirks.json` (`Quirk`/`QuirkMapper`): id, положительная/отрицательная, disease, buffs
(имена баффов), несовместимые. В лобби — «⟳» перебрасывает черты (1 положительная + 1 отрицательная
с учётом несовместимостей). Пока черты показываются, на статы боя не влияют.

### Лог и попапы

- Подробный лог: «кто чем кого и на сколько» (удар/крит/промах/уклонение/хил/убийство/ход/перемещение).
- Всплывающие числа урона анимируются над карточкой (~0.9 с, подъём + затухание).

## Gate

Мини-ядро в `src\Core\Combat` (создаётся в составе PoC) + `src\Lan` (есть). Полноценно — по мере выноса.

## Feature-flag

`desktop_client` (off по умолчанию).

## Статус

работает: меню → лобби (классы, активные скиллы, черты) → бой (полный HUD, лог, попапы,
MOVE/PASS) для vs AI и мультиплеера (Steam). Плейсхолдеры: инвентарь/карта/факел, локализация
квирков. Cross-cutting: переиспользует ядро и сеть.
