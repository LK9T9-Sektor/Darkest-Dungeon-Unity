# PLAN.md — Фаза 3: Извлечение боя и ИИ в чистое ядро

## Цель

Вынести всю боевую симуляцию, систему эффектов и ИИ монстров из презентационного слоя (`unity\Assets\Scripts\Mechanics\`) в чистое C#-ядро (`src\Core\Combat\`), чтобы WPF-клиент и оба Unity-проекта могли переиспользовать единую боевую логику.

## Структура ядра

Структура папок сохраняется по `Assets\Scripts\` (правило AGENTS.md «Preserve Folder Structure on Extraction»; корень `Assets\Scripts\` игнорируется):

```
src\Core\Sektor.DarkestDungeon.Core.Combat/
├── Mechanics/                  # enums из MechanicsDefines + RandomSolver
│   ├── Battle/                 # Round, SkillResult, FormationSet, ICombatUnit, IBattleContext…
│   ├── Skills/                 # Skill, CombatSkill, Effect, SubEffect + конкретные эффекты
│   │   └── Effects/            # 29 SubEffect-реализаций (после Этапа 5)
│   └── AI/                     # MonsterBrain, BrainDecisionType, базовые desire
│       ├── SkillDesires/       # 9 + SkillSelectRestriction
│       ├── TargetDesires/      # 8 + TargetSelectParameter, TargetDesireType
│       └── BonusDesires/       # 6
├── Raid/
│   ├── Battle/                 # enums из BattleGround + IBattleGround
│   └── Events/                 # EffectEvent + IEffectEvent
├── Character/                  # enums из Buff/Hero/Trait + ICharacter, IStatusEffect, IAttribute
│   ├── Components/             # IBattleModifier, ICharacterMode
│   └── Utils/                  # CharacterHelper
├── Campaign/                   # CurrencyCost
└── Sektor.DarkestDungeon.Core.Combat.csproj
```

## Шаги

### Этап 1. Инфраструктура (3 файла)

- [x] **1.1** Создать `src\Core\Sektor.DarkestDungeon.Core.Combat\Sektor.DarkestDungeon.Core.Combat.csproj` (netstandard2.0, C# 7.3, PostBuild доставка DLL)
- [x] **1.2** Добавить ссылку на `Core.Content` (для `IProportionValue`, `ISingleProportion`)
- [x] **1.3** Создать `src\Core\Sektor.DarkestDungeon.Core.Combat\Enums\` (пустая папка для Enums)

### Этап 2. Enums (40 файлов)

Создать по одному файлу на каждый enum из:
- `MechanicsDefines.cs` (21 enum)
- `BattleGround.cs` (6 enum)
- `CampingSkill.cs` (3 enum)
- `MonsterBrainDecision.cs` (1 enum)
- `SkillSelectRestriction.cs`, `TargetSelectParameter.cs`, `TargetDesireType.cs` (3 enum)
- `Buff.cs`, `Hero.cs`, `Trait.cs` (6 enum)

- [x] **2.1** Enum-файлы из `MechanicsDefines.cs`: `AttributeType.cs`, `AttributeCategory.cs`, `EffectBoolParams.cs`, `EffectIntParams.cs`, `EffectTargetType.cs`, `EffectSubType.cs`, `TrinketSlot.cs`, `Rarity.cs`, `MonsterType.cs`, `DeathClassType.cs`, `StatusType.cs`, `DurationType.cs`, `HeroEquipmentSlot.cs`, `CampingPhase.cs`, `BuffDurationType.cs`, `BuffSourceType.cs`, `QuestVisitType.cs`, `SkillResultType.cs`, `SkillTargetType.cs`, `HeroTurnAction.cs`, `SkillCategory.cs`
- [x] **2.2** Enum-файлы из `BattleGround.cs`: `Team.cs`, `TurnType.cs`, `RoundStatus.cs`, `TurnStatus.cs`, `BattleStatus.cs`, `SurpriseStatus.cs`
- [x] **2.3** Enum-файлы из AI: `BrainDecisionType.cs`, `SkillSelectRestriction.cs`, `TargetSelectParameter.cs`, `TargetDesireType.cs`
- [x] **2.4** Enum-файлы из CampingSkill: `CampTargetType.cs`, `CampEffectRequirement.cs`, `CampEffectType.cs`
- [x] **2.5** Enum-файлы из Character: `BuffType.cs`, `BuffRule.cs`, `HeroStatus.cs`, `OverstressType.cs`, `StartTurnActType.cs`, `ReactionType.cs`

### Этап 3. Интерфейсы (3 файла)

- [x] **3.1** `ICombatUnit.cs` — абстракция юнита на поле боя (Rank, Team, CombatInfo, Character)
- [x] **3.2** `IBattleGround.cs` — абстракция поля боя (HeroParty, MonsterParty, Round, состояние)
- [x] **3.3** `IBattleContext.cs` — computed-свойства battlefield (MonsterNumber, MarkedHeroes, HeroNumber и т.д.)

### Этап 4. Базовые типы скиллов (14 файлов)

- [x] **4.1** `Skill.cs` (перенести из `Mechanics\Skills\Skill.cs` — чистый)
- [x] **4.2** `HealComponent.cs` (выделить из `Skill.cs`)
- [x] **4.3** `MoveComponent.cs` (выделить из `Skill.cs`)
- [x] **4.4** `CombatSkill.cs` (адаптировать: заменить `FormationUnit` → `ICombatUnit`)
- [x] **4.5** `MoveSkill.cs` (перенести)
- [x] **4.6** `CampingSkill.cs` + `CampEffect.cs` + `CampEffectRequirement.cs` (перенести, отделить от UI)
- [x] **4.7** `FormationSet.cs` (перенести, заменить `FormationUnit` → `ICombatUnit`)
- [x] **4.8** `SkillArtInfo.cs` (перенести, заменить `Vector2` → `float x, float y`)
- [x] **4.9** `SkillCooldown.cs` (перенести — чистый)
- [x] **4.10** `SkillTargetInfo.cs` (адаптировать: `FormationUnit` → `ICombatUnit`)
- [x] **4.11** `HeroActionInfo.cs` (перенести — чистый)
- [x] **4.12** `SkillResult.cs` (адаптировать: `CombatSkill` остаётся в ядре)
- [x] **4.13** `SkillResultEntry.cs` (адаптировать: `FormationUnit` → `ICombatUnit`)
- [x] **4.14** `SubEffect.cs` (адаптировать: `FormationUnit` → `ICombatUnit`, `Effect` → core)

### Этап 5. Эффекты (31 файл)

- [x] **5.1** `Effect.cs` (адаптировать: заменить `RaidSceneManager` → `IBattleContext`, `FormationUnit` → `ICombatUnit`)
- [x] **5.2–5.30** Перенести 29 конкретных SubEffect'ов, каждый адаптировать:
  - Заменить `FormationUnit` → `ICombatUnit`
  - Заменить `RaidSceneManager` → `IBattleContext`/`IBattleEvents` (popup/halo/overlay/звук/суммон/контроль/торч)
  - Заменить `UnityEngine.Random` → `RandomSolver` (ядро)
  - Заменить `Mathf` → локальные хелперы (Clamp/Approximately/RoundToInt)
  - Заменить `Debug`/`LocalizationManager`/`Resources` → убраны/за абстракцией
  - Статусы: `BleedingStatusEffect` и др. → интерфейсы `IDotStatusEffect`/`IStunStatusEffect`/`IMarkStatusEffect`/`IRiposteStatusEffect`/`IGuardStatusEffect`/`IGuardedStatusEffect`
  - `Buff`/`BuffInfo` вынесены в ядро (`Character\`); `ICharacter` расширен (`Heal`, `TakeDamagePercent`, `AddBuff`, `Stress`, `RevertTrait`, `AddQuirk`, `CurrentMode`, `ControllerCaptor`, `EmptyCaptor`)

**Список эффектов (все готово, в `Mechanics\Skills\Effects\`):**
`BleedEffect`, `BuffEffect`, `CaptureEffect`, `ClearGuardEffect`, `ClearRankTargetEffect`, `CombatStatBuffEffect`, `ControlEffect`, `CureEffect`, `DiseaseEffect`, `GuardEffect`, `HealEffect`, `ImmobilizeEffect`, `KillEffect`, `KillEnemyTypeEffect`, `PerformerRankTargetEffect`, `PoisonEffect`, `PullEffect`, `PushEffect`, `RiposteEffect`, `SetModeEffect`, `ShuffleTargetEffect`, `StressEffect`, `StressHealEffect`, `StunEffect`, `SummonMonstersEffect`, `TagEffect`, `UnimmobilizeEffect`, `UnstunEffect`, `UntagEffect`

### Этап 6. AI (39 файлов)

- [x] **6.1** `MonsterBrain.cs` (перенести — чистый контейнер)
- [x] **6.2** `MonsterBrainDecision.cs` (перенести — DTO)

**Skill Desires (11 файлов):**
- [x] **6.3** `SkillSelectionDesire.cs` (базовый, адаптировать: `RaidSceneManager` → `IBattleContext`)
- [x] **6.4–6.12** Конкретные: `SkillSelectionRandom`, `SkillSelectionPreferred`, `SkillSelectionSpecific`, `SkillSelectionHeal`, `SkillSelectionStatus`, `SkillSelectionPerformingTurn`, `SkillSelectionAllyDead`, `SkillSelectionAllyAlive`, `SkillSelectionFillEmptyCaptor`

**Target Desires (10 файлов):**
- [x] **6.13** `TargetSelectionDesire.cs` (базовый, заменить `Random.Range` → `IRng`)
- [x] **6.14–6.21** Конкретные: `TargetSelectionRandom`, `TargetSelectionMarked`, `TargetSelectionHealth`, `TargetSelectionStress`, `TargetSelectionRank`, `TargetSelectionResistance`, `TargetSelectionFillCaptor`, `TargetSelectionAllyClass`

**Bonus Desires (7 файлов):**
- [x] **6.22** `BonusInitiativeDesire.cs` (базовый)
- [x] **6.23–6.28** Конкретные: `BonusInitiativeGuaranteed`, `BonusInitiativeHpRatio`, `BonusInitiativeLastSkill`, `BonusInitiativeDeath`, `BonusInitiativeAllyLastDamaged`, `BonusInitiativeAllyClassCount`

### Этап 7. Симуляция (3 файла)

- [x] **7.1** `RandomSolver.cs` (перенести, заменить `UnityEngine.Random.Range` → `IRng`)
- [x] **7.2** `Round.cs` (адаптировать: `FormationUnit` → `ICombatUnit`, `RaidSceneManager` → `IBattleGround`)
- [x] **7.3** `BattleSolver.cs` (адаптировать: все зависимости → интерфейсы)
  - Экземплярный класс с DI (`BattleSolver(IBattleContext)`), 13 методов
  - `ICharacter` расширен: `Dodge`, `Protection`, `MinDamage`, `MaxDamage`, `DamageMod`, `TakeDamage`, `RemoveConditionalBuffs`, `RiposteSkill`, `CurrentCombatSkills`, `IsReligious`
  - `IBattleContext` расширен: `CampingTimeLeft`, `ApplyCombatUnitRules`, `ApplyIdleUnitRules`, `ApplyEffectById`
  - `IBattleEvents.Pull/Push` + `changeUnitOrder`

### Этап 8. Тесты (5+ файлов)

- [ ] **8.1** Создать `tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests\` проект
- [ ] **8.2** Тесты симуляции: `RoundTests.cs`, `BattleSolverTests.cs`
- [ ] **8.3** Тесты эффектов: `EffectTests.cs`
- [ ] **8.4** Тесты AI: `MonsterBrainTests.cs`
- [ ] **8.5** Тесты RNG: `RandomSolverTests.cs`

### Этап 9. Интеграция с WPF

- [ ] **9.1** Добавить ссылку `Sektor.DarkestDungeon.Core.Combat` в `Sektor.DarkestDungeon.Wpf.csproj`
- [ ] **9.2** Обновить `BattleScreenViewModel` — данные из ядра вместо хардкода

### Этап 10. Интеграция с Unity (модификация presentation-layer)

- [ ] **10.1** `FormationUnit.cs` — реализовать `ICombatUnit`
- [ ] **10.2** `BattleGround.cs` — реализовать `IBattleGround`
- [ ] **10.3** `RaidSceneManager.cs` — заменить прямые вызовы `BattleSolver.*` на `using Core.Combat`
- [ ] **10.4** Остальные файлы в `Raid\`, `Managers\`, `UI\`, `Setup\` — обновить using'и

### Этап 11. Верификация

- [ ] **11.1** `dotnet build src\Core\Sektor.DarkestDungeon.Core.Combat\`
- [ ] **11.2** `dotnet test tests\Core\`
- [ ] **11.3** `pwsh tools\unity-compile-check.ps1 -ProjectPath unity`
- [ ] **11.4** `pwsh tools\unity-compile-check.ps1 -ProjectPath unity-2017`

## Приёмка

- [ ] WPF-проект ссылается на `Core.Combat` и может создать `BattleSolver` без Unity
- [ ] Все 40 enums в отдельных файлах (один публичный тип на файл)
- [ ] `BattleSolver`, `Round`, `Effect`, AI desires — без `using UnityEngine`
- [ ] NUnit-тесты проходят
- [ ] Оба Unity-проекта компилируются
- [ ] `MechanicsDefines.cs` удалён (все enums в отдельных файлах)

## Инвентарь классов (файл → назначение)

### Бой (`Mechanics\Battle\`)
| Файл | Назначение |
|---|---|
| `BattleSolver` | Центральный солвер: юзабельность скиллов, резолюция урона/хила, AI-мозг, условия баффов |
| `Round` | State-машина раунда/хода (HeroTurn/MonsterTurn, инициатива) |
| `SkillResult` / `SkillResultEntry` | Контейнеры результата применения скилла (хит/крит/мисс/хил) |
| `SkillTargetInfo` | Контекст целей: список целей, тип, скилл, арт |
| `SkillCooldown` | Кулдаун скилла (по ходам) |
| `HeroActionInfo` | Превью действия героя (шанс хита/крита, мин/макс урон) |
| `FormationSet` | Парсинг строки формации (`@~?1234`): ранги запуска/целей |
| `PopupType` | Типы всплывающих сообщений боя |
| `IBattleContext` | Абстракция контекста боя: computed-свойства, солвер-сервисы, события |
| `IBattleEvents` | Сервис фидбека эффектов: попуп/хало/звук/суммон/контроль/торч |
| `ICombatUnit`, `IFormationParty`, `IFormationUnitInfo` | Абстракции юнита/отряда/состояния юнита |

### Скиллы (`Mechanics\Skills\`)
| Файл | Назначение |
|---|---|
| `Skill` (абстр.), `HealComponent`, `MoveComponent` | База скилла + хил/мув-компоненты |
| `CombatSkill` | Боевой скилл: урон/аккураси/крит, ранги, эффекты, режимы, лимиты |
| `MoveSkill` | Скилл перемещения |
| `CampingSkill` / `CampEffect` | Кемпинг-скиллы и их эффекты |
| `FormationSet` | см. Бой (парсинг формаций) |
| `SkillArtInfo` | Визуальные токены скилла (fx/offset, engine-free) |
| `Effect` / `SubEffect` / `EffectEvent` | Контейнер эффектов, базовый sub-effect, очередь событий |
| `ITorchHandler` → удалён (заменён `IBattleEvents`) |

### Эффекты (`Mechanics\Skills\Effects\`, 29)
`BleedEffect`, `PoisonEffect`, `StunEffect`, `UnstunEffect`, `CureEffect`, `HealEffect`, `StressEffect`, `StressHealEffect`, `TagEffect`, `UntagEffect`, `RiposteEffect`, `BuffEffect`, `CombatStatBuffEffect`, `GuardEffect`, `ClearGuardEffect`, `ImmobilizeEffect`, `UnimmobilizeEffect`, `KillEffect`, `KillEnemyTypeEffect`, `PullEffect`, `PushEffect`, `ShuffleTargetEffect`, `ClearRankTargetEffect`, `PerformerRankTargetEffect`, `ControlEffect`, `DiseaseEffect`, `SetModeEffect`, `SummonMonstersEffect`, `CaptureEffect`

### AI (`Mechanics\AI\` + подпапки)
| Файл | Назначение |
|---|---|
| `MonsterBrain` / `MonsterBrainDecision` | Контейнер AI-желаний + результат решения (Pass/Perform) |
| `BrainDecisionType`, `SkillSelectRestriction`, `TargetSelectParameter`, `TargetDesireType` | Enums AI |
| `SkillSelectionDesire` (база) + 9 в `SkillDesires\` | Выбор скилла (random/preferred/specific/heal/status/performing_turn/ally_alive/ally_dead/fill_captor) |
| `TargetSelectionDesire` (база) + 8 в `TargetDesires\` | Выбор целей (random/marked/health/stress/rank/resistance/fill_captor/ally_class) |
| `BonusInitiativeDesire` (база) + 6 в `BonusDesires\` | Бонусная инициатива (guaranteed/hp_ratio/last_skill/death/ally_last_damaged/ally_class_count) |

### Механика (`Mechanics\`, корень)
| Файл | Назначение |
|---|---|
| `RandomSolver` | Детерминированный RNG (взвешенный выбор, сид) |
| 21 enum (`AttributeType`…`SkillCategory`) | Типы из `MechanicsDefines` |

### Персонажи (`Character\`)
| Файл | Назначение |
|---|---|
| `ICharacter` | Абстракция персонажа (статы, хил, баффы, стресс, статусы, квирки) |
| `IStress`, `IQuirk`, `IEmptyCaptor`, `IAttribute`, `IStatusEffect` | Абстракции стресса/квирков/каптора/атрибутов/статусов |
| `Buff` / `BuffInfo` | Бафф и применённый бафф (данные, без локализации) |
| `IDotStatusEffect`, `IStunStatusEffect`, `IMarkStatusEffect`, `IRiposteStatusEffect`, `IGuardStatusEffect`, `IGuardedStatusEffect`, `IResetableStatusEffect` | Абстракции статус-эффектов |
| 6 enum (`BuffType`, `BuffRule`, `HeroStatus`, `OverstressType`, `StartTurnActType`, `ReactionType`) | из `Buff`/`Hero`/`Trait` |
| `Components\IBattleModifier`, `ICharacterMode` | Боевые модификаторы и режимы персонажа |
| `Utils\CharacterHelper` | Парсинг строк → `AttributeType` |

### Прочее
| Файл | Назначение |
|---|---|
| `Raid\Battle\` (6 enum + `IBattleGround`) | Enum'ы поля боя (`Team`, `RoundStatus`…) + абстракция поля |
| `Raid\Events\EffectEvent`, `IEffectEvent` | Очередь событий эффектов |
| `Campaign\CurrencyCost` | Стоимость кемпинг-скиллов в валюте |
