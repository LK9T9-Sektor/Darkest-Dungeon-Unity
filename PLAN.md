# PLAN.md — Активный план задач

## Задача: монстры в ядро + дорожная карта «всё в ядро»

### Цель

Ядро уже содержит движок боя (`BattleSolver`, `Round`, 29 `SubEffect`, AI-desires), `EffectCatalog`
(парсер `Effects.txt`), конкретную модель (`Character`, `Hero`, `FormationUnit`, `BattleGround`),
`IBattleModifier` и `MonsterBrain`. **Нет** `Monster`-класса и парсера `Data\Monsters\*.txt` (460 файлов),
`JsonAI.json` brains (дуэль использует дефолтный `BuildDefaultBrain`), multi-turn в `Round`.
Выносим монстров в ядро (модель → парсер → каталог → дуэль), затем по дорожной карте — остальной
Unity-контент (см. ниже). **Скоуп первой поставки: ядро + тесты, WPF-выбор монстра — потом.**

### Фаза M0 — Статус-манифест выноса

0. [x] `docs\EXTRACTION_STATUS.md` — таблица «Unity → twin в ядре → статус» (вынесено / частично /
    не вынесено); `tools\check-extraction.ps1` сверяет пути манифеста с файловой системой и печатает
    отчёт (31 строка, все пути существуют). Поддерживается в том же коммите, что и вынос (агентам —
    один grep-таргет вместо сканирования Unity-дерева). `[Obsolete]` на legacy-коде НЕ ставим
    (код живой до cutover; при cutover — `[Obsolete(error: true)]` на удаляемых типах).

### Фаза M1 — Модель и парсер монстров (в `Core.Combat`, зеркало `Assets\Scripts\`)

1. [ ] `Character\MonsterClass.cs` — контент-модель: StringId, TypeId, Size, Attributes,
    EnemyTypes (MonsterType), CombatSkills (+резолв `.effect` через `EffectCatalog`), PreferableSkill,
    MonsterBrainId, BattleModifier (флаги сюрприза), InitiativeTurns (`number_of_turns_per_round`).
    Loot/DeathClass/Companions/etc. — позже (M-фазы дальше или вообще не нужны дуэли).
2. [ ] `Character\MonsterClassFileParser.cs` — парсер DSL `Data\Monsters\*.txt`: `name`/`type`,
    `display: .size`, `enemy_type: .id`, `stats:` (.hp/.def/.prot/.spd/.stun|poison|bleed|debuff|move_resist),
    `skill:` (все `.effect`, `.move`, `.launch`/`.target`, `.is_crit_valid`, кулдауны — ключи уже
    читаются), `personality: .prefskill`, `initiative: .number_of_turns_per_round`,
    `monster_brain: .id`, `battle_modifier:` (флаги сюрприза), `death_class:` (лёгкий вариант).
3. [ ] `Character\MonsterCatalog.cs` — `Load(contents, effects)`.
4. [ ] `Character\Monster.cs` — конкретный персонаж (`ICharacter`): IsMonster=true, MonsterTypes,
    Size, CombatSkills/CurrentCombatSkills, BattleModifiers, PreferableSkill; атрибуты/резисты из
    MonsterClass; без стресса (как в DD). `Character\BattleModifier.cs` — реализация `IBattleModifier`.

### Фаза M2 — Контент-провайдер (ядро + тесты)

5. [ ] `IDuelContent.GetMonsterClass(string)`; тест `TestDuelContent` грузит реальные
    `Data\Monsters\*.txt` (через unity-путь, как герои) + `MonsterCatalog.Load`.
    WPF `DuelContent` — только если быстро (сейчас решено: WPF-выбор монстра позже).
6. [ ] *(отложено)* `JsonAI.json` → `MonsterBrainCatalog` + `GetMonsterBrain(id)`; пока дефолтный brain.

### Фаза M3 — Дуль-интеграция

7. [ ] `DuelController`: `DuelMonsterPick` (classId+seed) для клиентской («Monsters») стороны →
    `new Monster(class)`; AI — существующий `DuelAi`/`BuildDefaultBrain`.
8. [ ] Сюрприз-ролл гейтится на `BattleModifiers.CanSurprise/CanBeSurprised/AlwaysSurprise/
    AlwaysBeSurprised` (сейчас хардкод без гейта).
9. [ ] `Round`: поддержка `number_of_turns_per_round > 1` (монстр ходит N раз за раунд).

### Фаза M4 — Тесты и проверка

10. [ ] `MonsterClassFileParserTests` (реальные монстры: статы, enemy_type, резолв эффектов,
    battle_modifier); дуэль «герои vs монстры» (атаки/хилы, AI-выбор скилла, сюрприз-гейт, multi-turn).
    Все сьюты зелёные + navigation (WPF не ломается).

### Дорожная карта «всё в ядро» (после монстров)

11. [ ] **Campaign** (`EXTRACTION_PLAN` Фаза 4): `Data\Campaign\` — город/квесты/провизия → `Core.Campaign`.
12. [ ] **Save** (Фаза 2): `Data\Save` → `Core.Save` (DTO + IBinarySaveData + `ISaveStorage`).
13. [ ] **Encounters/Bosses/Curios/Loot** → `Core.Content`; **JsonAI brains** → `Core.Combat`.
14. [ ] **Networking** (Фаза 5): Steam + Photon (`Sektor.Networking`, `PhotonTransport`, SessionManager).
15. [ ] **Presentation cutover** (Фаза 6): view-слой остаётся Unity/WPF, бизнес-логика — в ядро.

## Задача: стресс по правилам кампании + каталог эффектов (по частям, простое → сложное)

### Цель

Стресс в дуэли должен работать «как в кампании». Ядро уже содержит стресс-движок
(`StressEffect`/`StressHealEffect`/`IStress`), но дуэль не применяет его: `DuelBattleContext.ApplyEffectById`
— пустой стаб, а каталога эффектов (`Effects.txt`) в ядре нет. Выносим по частям (от простого к
сложному): каталог эффектов → крит-стресс → потоковые стресс-правила → resolve → эффекты скиллов (P1.1).
Плюс заводим `docs\GAME_RULES.md` (вариант A): секции «Как в этом репо» / «Оригинал DD (позже)».

### Фаза 0 — Документ правил

1. [x] `docs\GAME_RULES.md`: секция «Стресс» по фактам кампании (крит 15 цели, смерть героя 15
    отряду, ретраит 15, голод 15, death's door 6, снятие 4; пасс — без стресса), сводная таблица,
    статусы, подблок «Оригинал DD»; `INDEX.md` обновить.

### Фаза 1 — Каталог эффектов + крит-стресс

2. [x] `Core.Combat`: `EffectCatalog` — парсер DSL `Effects.txt` (`.stress`→`StressEffect`,
    `.healstress`→`StressHealEffect`, остальное partial) → словарь; `Effect.ApplyIndependent` уже был.
3. [x] `IDuelContent.GetEffect`; `DuelBattleContext.ApplyEffectById` резолвит из каталога и применяет
    instantly → крит-стресс (15) и крит-хил-снятие (4) работают через core-классы.
4. [x] WPF `DuelContent.GetEffect` (груз `Effects.txt`); тесты линкуют `Effects.txt`; тест
    `Crit_AppliesStressToTheTargetHero` (6/6 зелёные). Доки/статусы.

### Фаза 2 — Стрессовые правила потока

5. [x] Смерть героя → «Stress 2» (15) выжившим союзникам (`DuelController.CheckDeaths`/`StressParty`,
    тест `HeroDeath_StressesTheSurvivingParty`). Пасс — как в кампании (без стресса).
    **Death's door → BarkStress (6)** — отложено: в дуэли 0 HP = смерть, механики death's door нет.

### Фаза 3 — Resolve-ролл (аффекция/виртуда)

6. [x] `Trait` + `BuffIds`; `Hero.ApplyTrait`/`RevertTrait` (удаляет trait-баффы); `JsonTrait`/
    `JsonTraitData`/`TraitMapper`; `IDuelContent.GetAfflictions/GetVirtues`; `DuelBattleContext.
    ResolveOverstress` (шанс виртуды 0.25+ResolveCheckPercent, клэмп 0.01–0.6, аффекция → стресс
    союзникам `AfflictedAllyStress` 33%×5, виртуда → стресс 20–40); WPF `DuelContent` грузит
    `JsonTraits.json`; тест `Overstress_TriggersResolveRoll` (8/8 зелёные).

### Фаза 4 — Эффекты скиллов (P1.1)

7. [x] `EffectCatalog` парсит общие не-бафф ключи `Effects.txt` (`.stress`, `.healstress`, `.heal`,
    `.stun`, `.dotBleed`/`.dotPoison`+`.duration`, `.pull`, `.push`, `.cure`, `.riposte`,
    `.shuffleparty`/`.shuffletarget`, `.tag`/`.mark`, `.immobilize`) → `SubEffect`; каталог
    case-insensitive. `HeroClassFileParser.Parse(content, effects)` / `HeroCatalog.Load(contents, effects)`
    резолвят `.effect "id"` (до 2) → `CombatSkill.Effects`; дуэль применяет через `BattleSolver.
    ApplyEffects`. WPF `DuelClasses` единый источник каталога. Тесты: `EffectCatalogTests`,
    `Parse_ResolvesSkillEffects_FromEffectsCatalog`, реальный ростер резолвит эффекты (38/38 combat).
7b. [x] Stat-баффы/дебаффы: `.combat_stat_buff` + `*_add`/`*_multiply`/`critical_rating`/
    `speed_rating[_add]` → `CombatStatBuffEffect`/`RiposteEffect` (StatAddBuffs/StatMultBuffs);
    `HeroClassFileParser.ParseTokens` собирает все значения ключа (`key#N`) → резолвятся все
    `.effect` (до N) скилла; **EventQueue drain** в `DuelController.ExecuteSkill`
    (`ProcessEventQueues` → `EffectEvent.Execute`) — иначе квеянные эффекты (стан/бафф/гард)
    никогда не применялись в дуэли. Тесты: `Load_ParsesStatBuffsAndRiposteStatMods`,
    `StatBuffSkill_AppliesStatBuffsToThePerformer` (take_aim: +6% acc, x1.12 dmg),
    `StunSkill_AppliesTheStunStatusToTheTarget` (10/10 duel, 39/39 combat).
7c. [x] `.buff_ids` → `BuffEffect.BuffIds`; `IBattleContext.GetBuff(string)` (дуэль резолвит через
    `IDuelContent.GetBuff`) → `BuffEffect.ApplyInstant/ApplyQueued` резолвят контент-баффы из
    `JsonBuffs.json` на лету; `EffectCatalog.ParseTokens` собирает все значения ключа (`key#N`).
    Тест `BuffIdEffect_AppliesContentBuffToTheTarget` (flashing_daggers → bleed_debuff_1: -20% к
    сопротивлению кровотечению; 11/11 duel).

### Фаза 5 — Прочее

8. [x] **Торч**: `EffectCatalog` парсит `.torch_decrease`/`.torch_increase` → `IntegerParams[Torch]`
    + Global target (торч-only эффекты сохраняются); `DuelBattleEvents.TorchDelta` → `DuelController`
    мьютит `Context.TorchAmount` (клэмп 0–100). Тест `TorchEvents_MutateTheDuelTorch`.
    **Лимиты скиллов**: `HeroClassFileParser` читает `.per_turn_limit`/`.per_battle_limit`/
    `.is_continue_turn`; `BattleSolver.IsSkillUsable` учитывает `SkillsUsedThisTurn`/
    `SkillsUsedInBattle`, `ExecuteSkill` пишет использование. Тест `SkillLimit_BlocksFurtherUsesAfterLimit`
    (13/13 duel, 41/41 combat).
    **Отложено**: кулдауны (только у монстров, в скиллах героев нет `.cooldown`), bark-реакции
    (кампанийная наррация).

### Фаза 7 — Сюрприз первого раунда

10. [x] `DuelController.CheckSurprise` при старте боя: шанс сюрприза монстров/героев
    `0.1 + torch-бонус диапазона` (Radiant 0.25 → Out 0.4) + `MonsterSurpirseChance`/
    `PartySurpriseChance` героев, клэмп 0.65; `BattleGround.SetSurpriseStatus`;
    `Round.NextRound` (раунд 0) ставит -100 к инициативе застигнутой стороны (действует последней);
    `IsSurprised` на юнитах, сюрприз-шаффл героев; снятие флага на своём ходу. Тест
    `SurpriseTests` (seeds 1/11: монстры/герои последними + флаг; 17/17 duel, 43/43 combat).

### Фаза 6 — Mode-система (Абоминация)

9. [x] Парсер: `mode:` секции → `HeroClass.Modes` (+`is_raid_default`); `.valid_modes` → `CombatSkill.
    ValidModes`; `.X_effects` → `ModeEffects[X]` (ключ DSL `<mode>_effects`, режим без `_`);
    Category=Support для accuracy-0/self-target скиллов (legacy-правило: без accuracy-ролла).
    `EffectCatalog`: `.set_mode` → `SetModeEffect`; `.on_miss`/`.queue`/`.apply_once`.
    `Hero` стартует в raid-default mode; `BattleSolver.IsSkillUsable` фильтрует по `CurrentMode`;
    `DuelController.FinishSkillAction` — continue-turn (тот же юнит действует снова);
    `FormationParty.AddUnit` линкует `unit.Party` (фикс `PerformersOther`-эффектов, напр.
    `beast_stress_party`). Тесты: `Parse_ReadsModesValidModesAndModeEffects`, `Load_ParsesSetMode`,
    `ModeTests` (human→transform→beast, rage/manacles по модам, continue-turn; 15/15 duel, 43/43 combat).

---

## Задача: фикс NRE квирков + полировка низа боя и шапки

### Выполнено

1. [x] **NRE-фикс**: `weapons_haggler`/`armor_haggler` (town-баффы «upgrade_discount») падали в
   `ApplyBuff` (`GetAttribute` null). `DuelController.ApplyQuirks` теперь применяет только баффы,
   атрибут которых есть у героя; регрессия `QuirkBuffTests` (все квирки применяются в дуэли).
2. [x] Низ боя — одна панель в рамке: скиллы + LOG/INVENTORY/MAP в верхней полосе (разделитель),
   ниже три секции (статы/тултип/лог) с вертикальными разделителями.
3. [x] Шапка едина везде: `ScreenHeaderView` (X + панель заголовка/квеста) с `Subtitle`; бой —
   «Duel» + статус; меню/лобби — без «скачков».
4. [x] Torch прибит к верху; скиллы меньше (50), текст под кнопкой капсом (как MOVE/PASS);
   Turn Order без «SPD» (только числа); имена по командам (красные/синие); лог — промахи и
   криты с шансом.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: боевой HUD, Turn Order по DD, общий хром, переиспользование в формировании отряда

### Цель

Отклик на визуальный проход: левая панель статов — к низу, панели низа одной высоты,
LOG/INVENTORY/MAP — в ряд со скиллами сверху, скиллы меньше и без «Skills», низ ниже → больше
центру; карточки к низу с отступом + пробел между отрядами; границы команд (красная/синяя) +
подсветка ходящего; «Round N» — ниже факела, не на поле; крестик всегда слева-сверху с панелью
заголовка/квеста вплотную; единый стиль закруглённых рамок; Turn Order шире, квадратики,
справа-налево, с реальной и нароленной скоростью, по правилам DD; слоты под изображения
(просто возможность); переиспользуемый тултип навыков; экран формирования отряда переиспользует
карточку боевого поля (портрет, стрелки вверх/вниз, статы, скиллы кроме Move/Pass, квирки).

### Фаза 1 — Core: инициатива наружу

1. [x] `InitiativeRoll` (double) на `IFormationUnitInfo`/`FormationUnitInfo`; `Round.NextRound`
    сохраняет `Speed + roll`. Ролл расширен 0-10 (DD-инициатива, не «всегда игрок 1 первый»);
    тест `Initiative_FirstActor_VariesBetweenTeams` (4/4 зелёные).

### Фаза 2 — Общий хром + верх

2. [x] `ScreenHeaderView`: крестик слева + панель заголовка справа вплотную (закруглённая рамка);
    бой: [X Retreat][QuestLogView «Duel»] вплотную.
3. [x] Закруглённые рамки (`OverlayPanel`) — на TURN ORDER, Duel, Torch и пр.
4. [x] `TurnOrderView`: шире (до Torch), «TURN ORDER» к правому краю, квадратики, скорость+
    нароленная, красные/синие рамки + белая у ходящего, слот изображения.
5. [x] «Round N» — из поля в верхнюю панель (под факелом).

### Фаза 3 — Низ боя

6. [x] Левая панель статов к низу; три панели одной высоты (заполняют низ).
7. [x] LOG/INVENTORY/MAP — в верхний ряд со скиллами (вплотную); скиллы меньше, «Skills» убрана;
    низ компактнее (1*) — больше центр (1.6*).

### Фаза 4 — Поле боя

8. [x] Карточки к низу с отступом, пробел между отрядами; рамки красная/синяя по командам,
    белая у ходящего (возврат); слоты изображений в карточках/скиллах/очередности (пока пустые).

### Фаза 5 — Формирование отряда (переиспользование)

9. [x] Слот в `HeroSlotsPanel` — как карточка боя: портрет/класс, стрелки вверх/вниз,
    статы (`HeroStatsView` + `HeroSlotViewModel.Stats`), скиллы-кнопки (все кроме Move/Pass)
    с тултипами (`Ui.SkillDetails`), квирки+реролл.

### Фаза 6 — Тултип

10. [x] Стилизованный переиспользуемый `ToolTip` (тёмная рамка) + богатый `Ui.SkillDetails` —
    на скиллах боя/лобби, классах, квирках.

### Фаза 7 — Проверка и доки

11. [x] build + тесты (duel 4, combat 35, content 15, wpf 16) + запуск + навигация меню→лобби→бой;
    `TESTING.md`, `CHANGELOG.md`, `PLAN.md` обновлены. Ручной визуальный проход — за пользователем.

### Затронутые файлы

`Round.cs`, `IFormationUnitInfo.cs`, `FormationUnitInfo.cs`, `ScreenHeaderView`, `Hud.xaml`,
`DuelBattleView.xaml`, `RaidHudView.xaml`, `TurnOrderView.xaml`, `DuelTurnEntryViewModel.cs`,
`DuelBattleViewModel.cs`, `EventsLayerView.xaml`, `TorchView.xaml`, `QuestLogView.xaml`,
`HeroSlotsPanel.xaml`, `HeroSlotViewModel.cs`, `PartySelectionView`, `MainMenuView`,
`DuelLobbyView`, `SinglePlayerLobbyView`, доки.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: адаптивный WPF + исправления боя и выбора отряда

### Цель

Клиент WPF не должен зависеть от фиксированного холста (1920×1080 + Viewbox): окно ресайзится,
экраны забирают всё доступное пространство, раскладка на `*`-строках/колонках. Заодно: починить
дефолт классов («все abomination»), богатый тултип скиллов, широкие тултип/лог/левая панель,
центр боя меньше / низ выше, Retreat «X» в квест-панели, переиспользуемая панель выбора отряда
(`#1 Player`/`#2 AI`), крестик в меню закрывает приложение. Коммиты по фазам.

### Фаза 1 — Адаптивная оболочка

1. [x] `MainWindow`: убран `Viewbox`/фикс. размер → ресайз (min 1100×700), `ContentControl`
   растягивается; все экраны — без фикс. `Width/Height`, на `*`-раскладке.
2. [x] `MainMenuView`: растягивается, заголовок «ВЫБЕРИТЕ РЕЖИМ», крестик закрывает приложение;
   `MainMenuViewModel.CloseCommand`.

### Фаза 2 — Боевой экран

3. [x] `DuelBattleView`: строки верх `Auto` / центр `*` / низ `2*` (центр меньше, низ выше);
   карточки в масштабируемой сцене (`Viewbox` поля), Retreat «X» в квест-панели, стат-лист —
   оверлей поверх всего экрана.
4. [x] `RaidHudView`: колонки левая 360 / центр `*` / правая `*`; `UnitTooltipView` растягивается;
   `LogView` крупнее (15pt); левая панель шире.

### Фаза 3 — Тултип скиллов

5. [x] `BuildSkillDetails` богаче (урон/меткость/крит/хил/ранги, лимит за ход); глобальный
   стилизованный `ToolTip` (тёмная рамка) в `Hud.xaml`.

### Фаза 4 — Выбор отряда

6. [x] Новый `PartySelectionView` (заголовок + слоты, DPs `PlayerLabel`/`Slots`); `HeroSlotsPanel`
   на `UniformGrid` 4 колонки (замощается по ширине); `SinglePlayerLobbyView` — две панели
   (#1 Player, #2 AI) + «Reroll AI»; `SinglePlayerLobbyViewModel.AiSlots` (редактируемые);
   `DuelLobbyView` — панель #1 Player + статус + session-контролы; случайные разные дефолты
   классов слотов (`AssignClass`); `DisplayNames.Class` (читаемые имена классов).

### Фаза 5 — Кнопка закрыть

7. [x] `ScreenHeaderView` везде: меню — закрыть приложение, лобби — назад, заголовки
   «ВЫБЕРИТЕ РЕЖИМ» / «СФОРМИРУЙТЕ ОТРЯД».

### Фаза 6 — Проверка и доки

8. [x] build + тесты (duel 3, combat 35, content 15, wpf 16) + запуск + навигация меню→лобби→бой
   (UI Automation) — работает; `TESTING.md`, `CHANGELOG.md`, `PLAN.md` обновлены. Ручной
   визуальный проход и подгонка пропорций — за пользователем.

### Затронутые файлы

`MainWindow.xaml`, `MainMenuView(.xaml)`, `MainMenuViewModel.cs`, `DuelBattleView.xaml`,
`RaidHudView.xaml`, `UnitTooltipView.xaml`, `LogView.xaml`, `HeroInfoPanelView.xaml`,
`ScreenHeaderView(.xaml)`, `HeroSlotsPanel.xaml`, `HeroSlotViewModel.cs`,
`SinglePlayerLobbyView(.xaml)`, `SinglePlayerLobbyViewModel.cs`, `DuelLobbyView(.xaml)`,
`DuelBattleViewModel.cs`, новый `PartySelectionView.xaml(.cs)`, `AGENTS.md` (правило
адаптивной раскладки), доки.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: ИИ дуэли ведёт себя как в Darkest Dungeon (поверх ядра, без правок оригинала)

### Цель

Соперник в vs-AI должен действовать как монстр в DD: взвешенные skill/target-desires
(по `base_chance`), хилер лечит раненого союзника ниже порога HP, цели — random/marked/health,
кулдауны после использования скилла, выбор через детерминированный `RandomSolver`.
**Оригинал не трогаем** (Core.Combat и legacy Unity остаются как есть) — реализация «поверх»:
DD-зеркальные желания и брейн в `src\Core\Sektor.DarkestDungeon.Core.Duel`, расширяя базовые
(не-sealed) классы `SkillSelectionDesire`/`TargetSelectionDesire`. Коммиты по фазам.

### Фаза 1 — Core.Duel: DD-зеркальный ИИ

1. [x] `DuelSkillSelection` (rework): случайный скилл через **базовый** `SelectSkill`
   (RandomSolver, DD-цикл), `GetMonsterCombatSkills` → `CurrentCombatSkills`,
   `GetMonsterBrain` → внедрённый брейн.
2. [x] `DuelSkillSelectionHeal` (новый): `skill.Heal != null`, цель с HP < порога,
   только Health-цель-desire (корректная DD-логика хила).
3. [x] `DuelTargetSelectionRandom` / `DuelTargetSelectionMarked` / `DuelTargetSelectionHealth`
   (новые): random (враж.), marked-фильтр, health — сортировка по `HealthRatio` с
   `is_greater_comparison` и enemy/friendly-флагами (обязательный ключ
   `specific_combat_skill_id=""` — DD-JSON всегда его задаёт).
4. [x] `DuelAi`: строит DD-«default» брейн (heal 100/<0.5 + random 1; цели random 2/enemy +
   marked 1/enemy + health 100/friendly), гоняет DD-цикл (`ChooseByRandom` → `SelectSkill` →
   кулдаун), возвращает payload. Старый `DuelTargetSelection` (min-HP) удалён.
5. [x] `DuelAiTests`: lockstep (зелёный) + «хилер лечит раненого союзника» (детерминированный).

### Фаза 2 — Документация

6. [x] Новый `docs\AI_BEHAVIOR.md`: модель AI DD (брейн, desires, кулдауны/лимиты, `JsonAI.json`,
   цикл выбора, зеркало в дуэли, разрывы/будущее: парсинг эффектов P1.1).
7. [x] Правки: `INDEX.md`, `DUEL_ARCHITECTURE.md`, `PLAN.md`, `TESTING.md`, `CHANGELOG.md`.

### Затронутые файлы

- `src\Core\Sektor.DarkestDungeon.Core.Duel\DuelAi.cs`, `DuelSkillSelection.cs`,
  `DuelSkillSelectionHeal.cs`, `DuelTargetSelectionRandom.cs`, `DuelTargetSelectionMarked.cs`,
  `DuelTargetSelectionHealth.cs`; удалить `DuelTargetSelection.cs`.
- `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\DuelAiTests.cs`; документы.

### Критерии приёмки

- vs-AI: хилеры лечат раненого союзника (<50%), прочие — random/marked атаки; кулдауны применяются;
  поведение детерминировано (RandomSolver, сид). Core.Combat/Unity — без изменений.
- Тесты зелёные; доки обновлены.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: тонкий WPF — вынос дуэли в ядро (A), ИИ на MonsterBrain (B), документация (C)

### Цель

Дуэльная оркестрация (локстап PvP: host=герои, rival=«сторона монстров») сейчас живёт в
WPF-клиенте (`Combat\DuelController.cs` 413 строк и др.). Это ре-имплементация Unity-мультиплеера,
который в `unity\Assets\Scripts\Networking\RaidSceneMultiplayerManager.cs` (2285 строк) +
`MultiplayerSync.cs` (426 строк) не «разнесён» по слоям. Фаза A: вынести оркестрацию в чистый
core-модуль `Sektor.DarkestDungeon.Core.Duel`; WPF становится тонким. Фаза B: ИИ соперника на
`MonsterBrain`-инфраструктуре ядра. Фаза C: документация (новый `docs\DUEL_ARCHITECTURE.md` +
правки INDEX/ARCHITECTURE/KNOWN_ISSUES/FEATURE_DESKTOP_CLIENT/AGENTS/EXTRACTION_PLAN/CHANGELOG),
чтобы агенты быстро ориентировались. Коммиты: A, B, C — отдельными.

### Фаза A — вынос в `src\Core\Sektor.DarkestDungeon.Core.Duel`

1. [x] Новый модуль (netstandard2.0, C# 7.3, Nullable disable; ссылки Core.Combat + Core.Content;
   post-build доставка в `Assets\Plugins\Internal` обоих деревьев — как `Core.Combat.csproj`).
2. [x] Переезд: `DuelController` + `DuelHeroPick`, `DuelPhase`, `DuelSeed`, `DuelBattleContext`,
   `DuelBattleEvents`; новые `IDuelContent` (`GetHeroClass/GetQuirk/GetBuff`) и `DuelPayload`
   (wire-парсинг `skill|target` / `move|rank` / `pass|0`). Снять nullable-аннотации под C# 7.3.
3. [x] WPF: `DuelContent` (реализация `IDuelContent` поверх `DuelClasses`/`QuirkCatalog`/`BuffCatalog`);
   обновить `using`/точки создания в VMs и линках (`AiRivalLink`, `NetworkRivalLink`, `IDuelRivalLink`);
   удалить переехавшие файлы.
4. [x] Тесты: `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests` (net10.0, NUnit+NSubstitute) —
   `DuelTurnFlowTests` (локстап, `TestDuelContent` из связанного контента); WPF VM-тесты остаются.
5. [x] Проверка: build + dotnet test (duel 1, combat 35, content 15, wpf 16) + запуск приложения;
   Core.Duel.dll доставлен в оба `Assets\Plugins\Internal`.

### Фаза B — ИИ на MonsterBrain

6. [x] Core `DuelAi` в Core.Duel: выбор скилла+цели соперника через AI-инфраструктуру ядра
   (`MonsterBrain`/`DuelSkillSelection`/`DuelTargetSelection`/`MonsterBrainDecision`). Выбор — на
   клиент-локальном `System.Random`, чтобы не трогать `RandomSolver` и сохранить локстап;
   цель — по минимальному HP (умнее случайного).
7. [x] WPF `AiRivalLink` → тонкая обёртка (таймер + `DuelAi` + `RivalActionReceived`);
   тесты: `DuelAiTests` (локстап обеих сторон с ИИ) — 2/2 зелёные; WPF 16/16.

### Фаза C — документация

8. [x] Новый `docs\DUEL_ARCHITECTURE.md`: что такое дуэль, происхождение (Unity-мультиплеер PvP),
   инвентарь по слоям, критика (логика в презентации, god-classes, дубли оркестрации/протокола,
   случайный ИИ, нестабильный сид), роадмап (B, cutover Unity, фаза 6).
9. [x] Правки: `INDEX.md`, `ARCHITECTURE.md`, `KNOWN_ISSUES.md`, `FEATURE_DESKTOP_CLIENT.md`,
   `AGENTS.md`, `EXTRACTION_PLAN.md`, `CHANGELOG.md` (только B — видимое поведение ИИ).

### Затронутые файлы

- Новые: `src\Core\Sektor.DarkestDungeon.Core.Duel\*`, `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\*`,
  `docs\DUEL_ARCHITECTURE.md`.
- Изменённые: `src\Wpf\...\ViewModels\*`, `...\Combat\AiRivalLink.cs`, `...\Networking\*`,
  `src\Wpf\...\Data\DuelContent.cs`, документы.

### Критерии приёмки

- WPF теряет ~700+ строк доменной логики; Core.Duel чистый (netstandard2.0, C# 7.3, без engine-ссылок).
- После A поведение дуэли идентично (тесты зелёные). B: ИИ через MonsterBrain, `AiRivalLink` тонкий.
- Документация обновлена; агенты ориентируются по AGENTS.md + INDEX.md + DUEL_ARCHITECTURE.md.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: ускорить pre-commit проверку скрипт-GUID (ripgrep + параллельно + fast-path)

### Цель

Pre-commit хук тратит ~104 с на каждый коммит (скан `unity-check-script-references.ps1`
на `unity` и `unity-2017` последовательно). Сделать: скан на ripgrep (~2-5 с/проект),
параллельный запуск обоих проектов в хуке, и быстрый путь — пропуск скана, когда не
менялись файлы под `unity/`/`unity-2017/` (WPF-коммиты ~0.5 с). Защита от stale-GUID
сохраняется.

### Шаги

1. [x] `tools/unity-check-script-references.ps1` — переписан на ripgrep:
   индекс guid `rg -o --no-filename --replace '$1' '^guid: ([0-9a-f]+)' -g '*.meta'`;
   ссылки `rg -o --no-heading --replace '$1' 'm_Script: ... guid: ([0-9a-f]+)' -g '*.unity' -g '*.prefab'`
   (разбор path:guid по длине — последние 32 hex); `.cs.meta` через `rg --files -g '*.cs'` +
   `Test-Path`. Фолбэк на прежнюю PS-реализацию, если `rg` отсутствует. Контракт
   (`builtinGuids`, формат ошибок, exit code) — без изменений. Замеры: unity ~1.5 с,
   unity-2017 ~2.7 с (было ~52 с/проект).
2. [x] `.githooks/pre-commit` — быстрый путь: если `git diff --name-only HEAD` +
   `git ls-files --others --exclude-standard` не содержат путей `unity/`/`unity-2017/` →
   «No Unity changes, skipping», `exit 0` (~0.35 с). Иначе оба проекта параллельно
   (`&` + `wait`, exit 1 при любой ошибке); Unity-коммит ~1.9 с wall.
3. [x] Документация: `AGENTS.md` (хук гоняет проверку параллельно/ripgrep и пропускает
   при отсутствии изменений в Unity), `TESTING.md` (заметка в «Автопроверки»), `PLAN.md`.
4. [x] Проверка: скан обоих проектов чистый и быстрее; фолбэк без `rg`; fast-path
   (только `src/` → мгновенно, `unity/` → сканирует параллельно); синтетика ловит
   stale-GUID в rg-пути.

### Затронутые файлы

- `tools/unity-check-script-references.ps1`
- `.githooks/pre-commit`
- `AGENTS.md`, `TESTING.md`, `PLAN.md`

### Критерии приёмки

- WPF-коммит: хук завершается < 1 с, скан не запускается.
- Unity-коммит: оба проекта сканируются параллельно на ripgrep, ~5-10 с.
- Скан по-прежнему ловит stale-GUID (ошибки как раньше, exit 1).

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

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

## Задача: WPF-дуэль — порядок рангов отряда 1 (4→1) + вынос правила раскладки в ядро

### Цель

В WPF-дуэли отряд 1 (герои, левая сторона) отображался слева-направо рангами 1,2,3,4 —
фронт (ранг 1) оказывался у левого края, вдали от врага. В DD фронт должен смотреть на
врага: слева-направо **4,3,2,1** (ранг 1 — у центра). Вражеская сторона (1,2,3,4) корректна.
Причина: `DuelBattleViewModel.RefreshUnits()` кладёт `Heroes` в порядке `HeroParty.Units`
(`Units[0]` = ранг 1), XAML рендерит в left-aligned StackPanel.

Правило «ранг 1 — фронт, герои слева и смотрят вправо» в Unity есть, но зашито в презентацию
(`unity\`/`unity-2017\`: `FormationRanks` — `facingRight`, `Slots.Reverse()`, `Units[Count-i-1]`;
`FormationRanksSlot` — `SetSiblingIndex(4-Rank)`/`Rank-1`; `BattleFormation`). Выносим сущность
правила в чистое ядро как инстансный класс (движковые типы в netstandard2.0 не переносятся;
имя `FormationRanks` в ядре занято маркерами таргетинга). **Объём A (согласован):** ядро + WPF;
Unity не трогаем до cutover `Core.Duel` (фаза 6 EXTRACTION_PLAN).

### Шаги

1. [x] `src\Core\Sektor.DarkestDungeon.Core.Combat\Raid\Party\FormationDisplayOrder.cs` (новый,
   netstandard2.0/C# 7.3, один публичный тип, XML-доки): поле `bool facesRight` (семантика
   Unity `FacingRight`); `List<ICombatUnit> OrderLeftToRight(IFormationParty party)` — сортировка
   по `Rank` + реверс при `facesRight`; фабрики `HeroSide()` (facesRight: true) / `MonsterSide()`
   (false) — конвенция «герои слева, фронт к врагу» в одном месте.
2. [x] `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\DuelBattleViewModel.cs` — `RefreshUnits()`:
   герои через `FormationDisplayOrder.HeroSide()`, монстры через `MonsterSide()` (+`using
   Sektor.DarkestDungeon.Core.Combat.Raid.Party;`). Таргетинг/ход не затрагиваются
   (`CombatId`/`Rank`).
3. [x] Тесты: `tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests\Raid\Party\
   FormationDisplayOrderTests.cs` (NSubstitute): `HeroSide`→[4,3,2,1], `MonsterSide`→[1,2,3,4],
   устойчивость к разбросанному порядку списка. Регрессия в `tests\Wpf\...\DuelRenderTests.cs`:
   ранги `view.Heroes` = реверс рангов `duel.HeroParty.Units`; `view.Monsters` = как в
   `duel.MonsterParty.Units` (устойчиво к surprise-shuffle).
4. [x] Доки: `docs\TESTING.md` (шаг 4 — «отряд игрока слева→направо 4→1, фронт к врагу»),
   `docs\CHANGELOG.md` (фикс + вынос правила в ядро).
5. [x] Проверка: `dotnet test tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests`,
   `dotnet test tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests`, `dotnet build src\Wpf\...\Wpf.csproj`.
   Unity-compile-check не нужен (нет правок под `unity/`).

### Затронутые файлы

- Новые: `src\Core\...\Raid\Party\FormationDisplayOrder.cs`,
  `tests\Core\...\Raid\Party\FormationDisplayOrderTests.cs`.
- Изменённые: `src\Wpf\...\ViewModels\DuelBattleViewModel.cs`,
  `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\DuelRenderTests.cs`, `docs\TESTING.md`,
  `docs\CHANGELOG.md`, `PLAN.md`.

### Критерии приёмки

- Отряд 1 в дуэли слева-направо = ранги 4,3,2,1; враг — 1,2,3,4; Move/Pass, таргетинг,
  поп-апы, Turn Order работают как раньше (по `CombatId`/`Rank`).
- Правило живёт в ядре один раз (`FormationDisplayOrder`); Unity-код не изменён.
- Тесты зелёные; доки обновлены.

---

## Задача: единый порядок usings — снаружи namespace (owned src + tests)

### Цель

В 43 owned-файлах (`src\Lan` — 8, `tests\` — 35) `using`-директивы лежат внутри тела
`namespace` (стилистика StyleCop SA1200). Остальной код (`src\Core`, `src\Wpf`) использует
usings в начале файла, до `namespace`. Требование: привести все owned `/src`/`/tests` к единому
порядку — usings первым, затем `namespace`. Функциональных изменений нет (алиасы резолвятся на
уровне compilation unit); это чистый рефакторинг оформления.

Объём согласован: **только owned** — `src\Lan` и `tests\`. Legacy `unity\`/`unity-2017\`
(включая `SaveLoadManager.cs`) и vendored (Photon/Spine/FMOD) не трогаем. Анализаторов и
`.editorconfig` в репо нет — регрессии стиля не будет ни с той, ни с другой стороны.

### Шаги

1. [x] `src\Lan\` (8): `Sektor.DarkestDungeon.Lan.Cmd\Program.cs`,
   `Sektor.DarkestDungeon.Lan.Contracts\Transport\ITransport.cs`,
   `Sektor.DarkestDungeon.Lan.Steam\SteamTransport.cs`, `...\JsonTransportCodec.cs`,
   `...\Interop\SteamRuntime.cs`, `...\Interop\SteamNative.cs`, `...\Interop\SteamCallbacks.cs`,
   `...\Interop\NativeUtf8.cs`.
2. [x] `tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests` (12): `EffectCatalogTests`,
   `HeroClassFileParserTests`, `HeroSkillSelectionTests`, `Mechanics\BattleSolverTests`,
   `Mechanics\DeterminismTests`, `Mechanics\EffectTests`, `Mechanics\FormationSetTests`,
   `Mechanics\MonsterBrainTests`, `Mechanics\RandomSolverTests`, `Mechanics\RecordingSubEffect`,
   `Mechanics\RoundTests`, `Raid\Party\FormationDisplayOrderTests`.
3. [x] `tests\Core\Sektor.DarkestDungeon.Core.Content.Tests` (7) и
   `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests` (7): Database-тесты (`BuffContentMapperTests`,
   `CurioCsvParserTests`, `HeirloomExchangeMapperTests`, `LootMapperTests`, `NarrationMapperTests`,
   `PartyNameMapperTests`, `QuirkMapperTests`) и `DuelAiTests`, `DuelTurnFlowTests`, `ModeTests`,
   `QuirkBuffTests`, `SkillEffectsTests`, `StressTests`, `SurpriseTests`.
4. [x] Остальные тесты (9): `UiStyleTests`; `Lan.Tests` (`Codec\JsonTransportCodecTests`,
   `Support\InMemoryTransport`, `Transport\MessageRoundTripTests`, `Transport\TransportLifecycleTests`);
   `Wpf.Tests` (`DuelFlowTests`, `DuelRenderTests`, `LobbySlotTests`, `ScreenSmokeTests`).
5. [x] Проверка: `dotnet build` всех затронутых проектов (Lan.*, тестовые) + полный прогон
   тестов (`dotnet test`, 111 зелёных); линтер `tools\check-using-placement.ps1` = 0.
   Unity-compile-check не нужен (правок под `unity/` нет).

### Механика правки (одинаково для всех 43)

- Вырезать блок `using ...;` из тела `namespace` (после `{`), вставить в начало файла до
  `namespace`; убрать одинаковый отступ (4 пробела). Сохранить существующие пустые строки и
  порядок групп (System / third-party / first-party) как есть. Пустую строку между usings и
  `namespace` — оставить.

### Затронутые файлы

- 43 owned-файла: 8 в `src\Lan\`, 35 в `tests\`. Документация обновляется только в `PLAN.md`;
  `docs\CHANGELOG.md` — косметика, не user-visible, не трогаем.

### Критерии приёмки

- Во всех owned файлах `src\` и `tests\` `using` стоят до `namespace`.
- Все сборки компилируются; все тестовые сьюты зелёные; под `unity/`, `unity-2017\`,
  `src\External\`, `src\Wpf\Sources\`, `src\Core\` — диффа нет.

---

## Задача: защитить правило «using до namespace» (3 слоя + AGENTS.md)

### Цель (план на будущее, после основного рефакторинга)

Паттерн «using внутри namespace» — легальная конвенция StyleCop SA1200, от которой уже нет
смысла что-то отстаивать: всюду по проекту (включая `src\Core`, `src\Wpf`) принят противоположный
порядок — usings снаружи. Нужно зафиксировать стандарт и не пускать регрессию. Объём защиты —
только owned `src\` и `tests\` (vendored и legacy Unity не трогаем ничем).

### Шаги

1. [x] `AGENTS.md` — добавлен пункт про Using Placement в секцию «III. Clean Code &
   Documentation»: в owned C# (`src\`, `tests\`) все `using`-директивы в начале файла, до
   `namespace`; не внутри тела namespace (SA1200).
2. [x] Корневой `.editorconfig`: секции `[src/**/*.cs]` и `[tests/**/*.cs]` с
   `csharp_using_directive_placement = outside_namespace:warning` (IDE0065),
   `dotnet_sort_system_directives_first = true` и `dotnet_separate_import_directive_groups = true`.
   Legacy/vendored Unity (`unity\`, `unity-2017\`) исключены.
3. [x] `tools\check-using-placement.ps1`: сканирует `src\` и `tests\` (кроме `src\External\`,
   obj/bin), находит owned-файлы с `using`-директивой с отступом (после `namespace {`),
   exit code 1 при находке. `using (...)`-statement'ы и `using var` в телах методов не считаются
   директивами и игнорируются.
4. [x] `.githooks\pre-commit`: перед Unity-веткой добавлен вызов
   `tools\check-using-placement.ps1`, когда коммит содержит owned `.cs` под `src/`/`tests/`
   (staged + untracked). Синтаксис проверен (`bash -n`, exit 0).
5. [x] Verify: staged-проба owned-файла с using внутри namespace → хук падает (exit 1, файл
   назван); чистый индекс → exit 0 (fast-path «No Unity changes»). Негативный кейс линтера
   через `-Roots` на sandbox: `Bad.cs` пойман (exit 1), `Good.cs` пропущен.

### Затронутые файлы

- `AGENTS.md`, новый `src\.editorconfig` (или корневой), новый `tools\check-using-placement.ps1`,
  `.githooks\pre-commit`, `PLAN.md`. Сам рефакторинг 43 файлов — отдельная задача выше.

### Критерии приёмки

- ИИ-агент (по AGENTS.md) и человек (по IDE) больше не создают owned-файл с using внутри namespace.
- Защита не пересекается: vendored/legacy Unity исключены из проверки; pre-commit не замедляет
  C#/docs-коммиты ощутимо (быстрый rg-проход по staged owned `.cs`).

---

## Правила

- Сначала ядро (`src\Core`), потом адаптеры; `src\External\` — read-only.
- Доки обновляются в том же коммите; `CHANGELOG.md` — только user-visible.
- Всё новое — минимальными диффами, без оппортунистических рефакторов legacy.