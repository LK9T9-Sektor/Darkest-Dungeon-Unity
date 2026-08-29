# UNITY_LEGACY_MAP.md — Карта Unity-легаси

> **Что это.** По-классная карта активного Unity-дерева (`unity\Assets\Scripts\`, **502** `.cs`-файла):
> какая папка за что отвечает, какие классы/механики где живут и что из этого уже вынесено в чистое
> ядро. Позволяет агенту/человеку понять структуру легаси, не читая сотни файлов.
>
> **Статус выноса** (колонка «Core»): **в** = twin существует в `src\Core\` (сверяй с
> `EXTRACTION_STATUS.md`), **част.** = частично, **—** = не вынесено.
>
> **Правило:** легаси — живая реализация Unity-игры и не правится до cutover (Фаза 6
> `EXTRACTION_PLAN.md`). Разрывы механик — в `BATTLE_PARITY.md`, критика — в `ARCHITECTURE_REVIEW.md`,
> целевая декомпозиция — в `TARGET_LAYOUT.md`.

## 0. Обзор дерева

| Папка | Файлов | Ответственность | Домен/презентация |
|---|---|---|---|
| `Mechanics` | 79 | Боевой движок: Battle/Round/Effects/AI/навыки | домен (вынесен) |
| `Character` | 52 | Модель персонажей: Hero/Monster/Character/статусы/компоненты | домен (вынесен) |
| `Raid` | 61 | Рейд-сцена: формация, бой, props, события, партия | домен + view |
| `Campaign` | 56 | Кампания: имение, здания, квесты, week log, город | домен (не вынесен) |
| `Database` | 8 | Загрузка контента: JSON/CSV → каталоги | домен-инфра (част.) |
| `Managers` | 12 | Оркестрация сцен/игры (god-классы) | презентация |
| `Setup` | 27 | Старт игры, контент-загрузка, сейвы | презентация |
| `Networking` | 14 | Мультиплеер: Steam/Photon, дуэль-оркестрация | презентация |
| `Generation` | 6 | Генерация подземелий/квестов | домен (не вынесен) |
| `UI` | 149 | Все UI-окна/панели/слоты/контролы | презентация |
| `ImageEffects` | 38 | Пост-эффекты (Unity Standard Assets, vendored) | ассеты |
| `Sounds` / `PlayerInput` | 0 | Пусто | — |

Дерево `unity-2017\` идентично (502 файла); расхождения — только внутри Unity-кода
(API 2017.4 vs 6000), в карте не дублируются.

---

## 1. `Mechanics` (79) — боевой движок

Ядро боевой механики. Почти всё зеркалировано в
`src\Core\Sektor.DarkestDungeon.Core.Combat\Mechanics\`.

### 1.1 `Battle\` — исполнение боя

| Класс | Роль | Core |
|---|---|---|
| `BattleSolver.cs` | Исполнение скилла: hit/dodge/crit/урон/хил/стресс, `UseMonsterBrain`, `CalculateSkillPotential`, условные баффы | в |
| `Round.cs` | Порядок ходов: инициатива, сюрприз, pre/post-turn, `NextRound` | в |
| `FormationSet.cs` | Маска рангов (launch/target) | в |
| `HeroActionInfo.cs` | Превью потенциального урона/хита/крита скилла | в |
| `SkillCooldown.cs` | Кулдаун скилла (N ходов после использования) | в |
| `SkillResult.cs`, `SkillResultEntry.cs` | Результат исполнения скилла (список записей) | в |
| `SkillTargetInfo.cs` | Информация о цели/таргетинге скилла | в |

### 1.2 `Skills\` + `Skills\Effects\` — скиллы и эффекты

| Класс | Роль | Core |
|---|---|---|
| `CombatSkill.cs` | Модель боевого скилла: acc/dmg/crit, launch/target, `Effects`, `ModeEffects`, лимиты | в |
| `Effect.cs` | DSL-эффект (772 стр.): target-диспатч, `LoadData` всех ключей, target-условия | в |
| `Skill.cs`, `MoveSkill.cs`, `MoveComponent`-логика | Базовый скилл/self-move | в |
| `SubEffect.cs` | Базовый класс суб-эффекта (instant/queued) | в |
| `SkillArtInfo.cs`, `CampingSkill.cs`, `CampingSkillHelper.cs` | Анимация/иконки; camping-скиллы | част. (camping в ядре есть) |
| `Effects\*.cs` (29) | Все типы эффектов: Stress, StressHeal, Heal, Stun, Bleed, Poison, Cure, CombatStatBuff, Buff, Riposte, Guard, ClearGuard, Pull, Push, ShuffleTarget, Tag, Untag, Immobilize, Unimmobilize, SetMode, Kill, KillEnemyType, Disease, SummonMonsters, Control, Capture, PerformerRankTarget, ClearRankTarget, Unstun | в (классы есть; **парсинг не всех** — см. `BATTLE_PARITY.md` §2.8) |

### 1.3 `AI\` — мозги и желания

| Класс | Роль | Core |
|---|---|---|
| `MonsterBrain.cs`, `MonsterBrainDecision.cs` | Выбор скилла+цели монстра через desires | в |
| `Bonus Desires\` (7) | Бонусы инициативы (алли-класс, смерть, HP-ratio, гарантированный, последний скилл и т.п.) | в |
| `Skill Desires\` (11) | Выбор скилла: Heal, Preferred, Random, Specific, Status, AllyAlive/Dead, Restriction и др. | в |
| `Target Desires\` (10) | Выбор цели: Random, Marked, Health, Rank, Resistance, Stress, AllyClass, FillCaptor | в |

### 1.4 Корни

| Класс | Роль | Core |
|---|---|---|
| `RandomSolver.cs` | Детерминированный RNG (общий сид) | в |
| `RaidSolver.cs` | Тонкий фасад выбора монстра через brain | — (логика в `BattleSolver.UseMonsterBrain`) |
| `MechanicsDefines.cs` | `AttributeType`, `EffectTargetType`, `BuffRule`, `StatusType` и пр. | в |

---

## 2. `Character` (52) — модель персонажей

Зеркало — `src\Core\Sektor.DarkestDungeon.Core.Combat\Character\`.

| Класс | Роль | Core |
|---|---|---|
| `Character.cs` (1223) | Базовая модель: статы, buffs, статусы, правила баффов, `ApplyBuffRule`, урон/хил, `UpdateRound` | в |
| `Hero.cs` (774) | Герой: класс, квирки, тринкеты, моды, riposte-скилл | в (тринкеты/шмот — нет) |
| `Monster.cs`, `MonsterData.cs` | Монстр из `MonsterClass` | в |
| `HeroClass.cs` | Класс героя: скиллы, статы, режимы | в |
| `Attribute.cs`, `AttributeModifier.cs` | Статы `(Raw+Flat)·Mult` | в |
| `Buff.cs`, `BuffInfo.cs` | Бафф и его применение | в |
| `Quirk.cs`, `QuirkInfo.cs`, `Trait.cs`, `Resolve.cs` | Квирки/черты (аффекции/виртуды)/resolve | в |
| `Trinket.cs`, `Equipment.cs` | Тринкеты/экипировка | част. (модель `Trinket` есть, применения в бою нет) |
| `Components\` (19) | Компоненты: `BattleModifier`, `CharacterMode`, `DeathClass`, `DeathDoor`, `HeroGeneration`, `Initiative`, `LifeLink`, `Companion`, `Spawn`, `Controller`, `Shapeshifter`, `SharedHealth`, `LootDefinition` и др. | част. (IBattleModifier/ICharacterMode/HeroGeneration в ядре; DeathClass/Companion/Spawn и пр. — нет) |
| `Statuses\` (13) | Статусы: Bleed/Poison/DoT, Guard/Guarded, Mark, Riposte, Stun, DeathsDoor, DeathRecovery | в (классы; потребители не все — см. `BATTLE_PARITY.md`) |
| `Utils\` (3) | `CharacterHelper`, enum-расширения, локализация | част. |

---

## 3. `Raid` (61) — рейд-сцена: бой, формация, props, события

Частично вынесено в ядро (`BattleGround`, `FormationParty/Unit/Info`, `TorchMeter`-логика в Duel).

### 3.1 `Battle\` — поле боя

| Класс | Роль | Core |
|---|---|---|
| `BattleGround.cs` (1060) | Поле: спавн юнитов/энкаунтеров, таргетинг, события, проверки победы | в |
| `PositionedElement.cs`, `PositionSet.cs` | Позиции на поле (пространственное) | — |
| `UniqueEffectRecords.cs` | Учёт применённых `apply_once`-эффектов | — |

### 3.2 `Party\` — формация и юниты

| Класс | Роль | Core |
|---|---|---|
| `FormationUnit.cs` (675) | Юнит на поле: HP/stress-оверлеи, pull/push, анимации | в (ядро чистое; pull/push в ядре — только события) |
| `FormationUnitInfo.cs` | CombatInfo юнита: ранк, CombatId, статусы | в |
| `FormationParty.cs` | Партия юнитов | в |
| `FormationRanks.cs`, `FormationRanksSlot.cs` | Раскладка рангов на сцене (facing, порядок) | част. (`FormationDisplayOrder` в ядре) |
| `BattleFormation.cs` | Загрузка энкаунтера в формацию | — |
| `UnitAnimator.cs`, `AnimatedEffect.cs`, `FormationOverlay*`, `RankPlaceholders`, `SharedHealthInfo`, `FormationUnitStressOverlay` | View-слой юнитов | — |

### 3.3 `Events\` — события рейда

| Класс | Роль | Core |
|---|---|---|
| `RaidEvents.cs` | События рейда (хост-шина) | — |
| `EffectEvent.cs`, `PopupMessage.cs` | Очередь эффектов/попапы урона | част. (в ядре `EffectEvent` есть) |
| `RaidAnnouncment.cs`, `RoundIndicator.cs` | Объявления раундов | — |
| `Scroll*.cs` | Скролл-события (camping/интеракции/лут/голод/еда) | — |

### 3.4 `Props\` + `Area\` — окружение

| Класс | Роль | Core |
|---|---|---|
| `Props\Trap.cs`, `Obstacle.cs`, `Door.cs` | Модели пропов | част. (модели `Prop`/`Curio` в `Core.Content`) |
| `Props\PropObjects\Raid*.cs` | View-объекты пропов на сцене | — |
| `Area\` (Room/Hallway/HallSector + Raid-виды) | Геометрия подземелья | — |

### 3.5 Корни

| Класс | Роль | Core |
|---|---|---|
| `RaidManager.cs`, `RaidInterface.cs`, `RaidInfo.cs`, `RaidParty.cs`, `RaidRuleInfo.cs`, `TorchMeter.cs`, `CampController.cs`, `Dungeon.cs`, `RaidPartyController.cs`, `RaidPartyCamera.cs`, `PointerPartyMover.cs`, `RaidExtensions.cs`, `RaidHeroInfo.cs`, `Backdrop.cs` | Оркестрация рейд-сцены/UI/камеры; `TorchMeter` — торч/свет | част. (торч-логика в `DuelController`) |
| `Contents\BattleEncounter.cs` | Энкаунтер (состав монстров) | — |

---

## 4. `Campaign` (56) — кампания (не вынесена, Фаза 4)

Доменный слой имения/города/квестов. В ядре пока только разрозненные модели
(`HeirloomExchange`, `PartyNameEntry`, `NarrationEntry`, DTO `JsonCamping/JsonTrinket/JsonQuests/...`
в `Core.Data`).

| Группа | Классы | Роль | Core |
|---|---|---|---|
| Корень | `Campaign.cs` (237), `Estate.cs` (571), `RealmInventory.cs`, `ItemDefinition.cs`, `ItemData.cs`, `CurrencyCost.cs`, `CompletionReward.cs`, `Upgrade.cs`, `UpgradeTree.cs`, `DungeonProgress.cs`, `TownEvent.cs`, `EventModifiers.cs`, `PrerequisiteReqirement.cs` | Модель кампании: имение, инвентарь, апгрейды, городские события | — |
| `Quests\` | `Quest.cs`, `QuestGoal.cs`, `QuestGoalList.cs`, `IQuestData.cs`, `QuestActivateData.cs`, `QuestDeathDoorData.cs`, `QuestGatherData.cs`, `QuestKillMonsterData.cs`, `QuestTraitData.cs`, `QuestTutorialData.cs`, `QuestVisitedData.cs`, `QuestType.cs`, `PlotQuest.cs`, `PlotTrinketReward.cs`, `UpgradeTag.cs` | Квесты: цели, типы, данные (gather/kill/trait/visit), plot-квесты | — |
| `Town\` | `Building.cs`, `ActivityBuilding.cs`, `ActivitySlot.cs`, `TownActivity.cs`, `TownEffects.cs`, `TreatmentSlot.cs`, `UpgradePurchases.cs`, `DeathRecord.cs`, `GeneratedRarity.cs` + специализации (`Abbey`, `Blacksmith`, `CampingTrainer`, `DiseaseTreatmentActivity`, `Graveyard`, `Guild`, `NomadWagon`, `QuirkTreatmentActivity`, `Sanitarium`, `StageCoach`, `Statue`, `Tavern`) | Здания города, активности лечения/обучения, покупки апгрейдов | — |
| `Week Logs\` | `WeekActivityLog.cs`, `ActivityRecord.cs`, `ActorActivityRecord.cs` (777), `PartyActivityRecord.cs`, `ActivityType.cs`, `ActivityEffectType.cs`, `PartyActionType.cs` | Журнал недели (записи активностей героев) | — |

---

## 5. `Database` (8) — загрузка контента

| Класс | Роль | Core |
|---|---|---|
| `DarkestDatabase.cs` (2408) | Единый читатель всего контента: герои/монстры/скиллы/квирки/баффы/локализация/данные города → каталоги | част. (вынесены парсеры-твины: `HeroClassFileParser`, `MonsterClassFileParser`, каталоги в `Core.Data`) |
| `DarkestJsonReader.cs` (887) | JSON-десериализация через Newtonsoft (граница презентации; 2017.4 не читает net6.0-контракты — `KNOWN_ISSUES.md` §13) | — (ядро без Newtonsoft, см. `Core.Data`) |
| `CsvReader.cs`, `DataExtensions.cs` | CSV-чтение/расширения | част. (`CsvReader` в `Core.Content`) |
| `HeroSpriteDatabase.cs`, `ProvisionDatabase.cs`, `QuestDatabase.cs`, `TownEventDatabase.cs` | Специализированные каталоги | — |

---

## 6. `Managers` (12) — оркестрация сцен (презентация)

| Класс | Роль |
|---|---|
| `RaidSceneManager.cs` (6043) | **God-класс рейда**: вся оркестрация боя/хода/UI/событий/риппоста/гарда/стелла (наследуется мультиплеером) |
| `DarkestDungeonManager.cs` | God-менеджер игры: данные, сцены, переходы |
| `EstateSceneManager.cs` (793) | Оркестрация сцены имения/города |
| `TownManager.cs`, `ShopManager.cs` | Город/магазин |
| `PartyFormationManager.cs`, `RaidPreparationManager.cs`, `CampaignSelectionManager.cs` | Подготовка отряда/рейда, выбор кампании |
| `LocalizationManager.cs`, `DarkestSoundManager.cs`, `ToolTipManager.cs`, `DragManager.cs` | Локализация, звук, тултипы, drag&drop |

---

## 7. `Setup` (27) — старт, загрузка, сейвы

| Группа | Классы | Роль | Core |
|---|---|---|---|
| Корень | `GameInfo.cs`, `StartupCulture.cs`, `PrivilegedStarter.cs`, `LutConverter.cs` | Версия/культура/старт | — |
| `ContentLoading\` | `GameSetup.cs`, `ScreenLoader.cs`, `RoomSelector.cs` (530), `PreambleSkipper.cs`, `MoviePlayer.cs`, `LoadingScreenInfo.cs`, `GameIntro.cs`, `GameLogo.cs`, `MultiplayerRoomSlot.cs`, `PlayerNicknameInputField.cs` | Загрузка контента/экран загрузки/комнаты | — |
| `SaveSystem\` | `SaveLoadManager.cs` (1870), `SaveSelector.cs`, `SaveSlot.cs`, `SaveCampaignData.cs` (542), `SaveHeroData.cs`, `SaveActivitySlot.cs`, `SaveEventData.cs`, `IBinarySaveData.cs`, `FormationUnitSaveData.cs`, `BattlegroundSaveData.cs`, `BattleFormationSaveData.cs`, `RaidPartySaveData.cs`, `RaidPartyHeroInfoSaveData.cs` | Сериализация/загрузка сейвов | част. (только `IBinarySaveData` в `Core.Content\Save`) |

---

## 8. `Networking` (14) — мультиплеер (презентация, legacy-дуэль)

| Класс | Роль | Core |
|---|---|---|
| `RaidSceneMultiplayerManager.cs` (2285) | **Дуэль PvP**: партия приглашённого игрока = «сторона монстров», lockstep-сид, RPC-ходы, готовность, результаты | в (ре-имплементация `Core.Duel`; см. `DUEL_ARCHITECTURE.md`) |
| `MultiplayerSync.cs` (426) | `party_config`, readiness-барьер, RPC-входы | в (протокол в `DuelPayload`) |
| `SteamSessionManager.cs`, `SteamRaidBridge.cs` | Steam-сессии/мост | — (транспорт в `src\Lan`) |
| `DarkestPhotonLauncher.cs`, `PhotonGameManager.cs` | Photon-подключение/RPC | — (Фаза 5) |
| `MultiplayerPartyData.cs`, `MultiplayerPartyPanel.cs`, `MultiplayerPartySlot.cs` | DTO/UI партии | — |
| `MultiplayerLogUI.cs`, `MultiplayerProviderMenu.cs`, `SteamLobbyIdPanel.cs` | UI лобби/провайдеров | — |
| `BarkMessage.cs`, `BarkMessenger.cs` | bark-реплики героев (наррация) | — |

---

## 9. `Generation` (6) — генерация подземелий/квестов (не вынесено)

| Класс | Роль |
|---|---|
| `DungeonGenerator.cs` (712) | Процедурная генерация подземелья (комнаты/коридоры) |
| `DungeonGenerationData.cs`, `DungeonEnviromentData.cs`, `QuestGenerationData.cs`, `QuestGenerator.cs`, `CampaignGenerationData.cs` | Данные/генерация квестов и кампании |

---

## 10. `UI` (149) + `ImageEffects`/`Sounds`/`PlayerInput` — презентация (не выносится)

- **UI** — все окна/панели/слоты/контролы: `Windows` (27), `Slots` (43), `Panels` (32),
  `Controls` (23), `Inventory` (7), `Testing` (14 — в т.ч. стенд Тест-боя: `FightScreen`,
  `FightBattleView`, `FightContentLoader`), корень (`RuntimeUiFactory`, `SoundSettingsUI/Sprites`).
  View-слой остаётся Unity/WPF; бизнес-логика — в ядро (Фаза 6).
- **ImageEffects** (38) — post-processing Unity Standard Assets (vendored, не наш код).
- **Sounds** / **PlayerInput** — пустые папки.

---

## 11. Легенда и сверка

- Статус «в/част./—» — производный от `docs\EXTRACTION_STATUS.md` (манифест выноса, единый
  grep-таргет). При расхождении источник истины — манифест.
- Механики, которые в ядре есть, но не действуют до конца (DoT-тик, станы, рипост, гард,
  pull/push/shuffle) — в `BATTLE_PARITY.md`; причины и рекомендации — в `ARCHITECTURE_REVIEW.md`.
- Целевая декомпозиция «куда переезжает каждый блок» — в `TARGET_LAYOUT.md`.