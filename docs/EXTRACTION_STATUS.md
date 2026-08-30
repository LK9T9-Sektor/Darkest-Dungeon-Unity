# EXTRACTION_STATUS.md — Статус выноса в ядро

> Единый grep-таргет для агентов и людей: «что уже в ядре, а что осталось в Unity».
> Обновляется **в том же коммите**, что и вынос (правило «доки в том же коммите» из `AGENTS.md`).
> Проверка путей: `pwsh tools\check-extraction.ps1`.
>
> Статусы: **вынесено** (и unity-источник, и twin в ядре существуют) · **частично** · **не вынесено** (ядро — `—`).
> Пути — от корня репо.
>
> **Важно:** legacy-код в `unity\` остаётся живой реализацией Unity-игры до cutover и НЕ помечается
> `[Obsolete]` (семантически враньё + шум CS0618 в call-site'ах). `[Obsolete(error: true)]` — только
> на удаляемых Unity-дублях в момент cutover.
>
> **Паритет механик** (что реализовано в legacy vs в ядре, с file:line и скиллами-жертвами) —
> в `docs\BATTLE_PARITY.md`. Разрывы закрываются в ядре; legacy Unity не правится под паритет
> (до cutover, `EXTRACTION_PLAN.md` Фаза 6).
>
> **По-классная карта** всех 502 файлов легаси (ответственности, god-классы, статус выноса) —
> в `docs\UNITY_LEGACY_MAP.md`; манифест ниже — сжатая сверка.

## Вынесено в ядро

| Unity (legacy) | Core (twin в `src/Core/`) | Статус |
| --- | --- | --- |
| `unity/Assets/Scripts/Mechanics/Battle/BattleSolver.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/Battle/Round.cs` (вкл. сюрприз-упорядочивание 1-го раунда) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/Round.cs` | вынесено |
| `unity/Assets/Scripts/Raid/Battle/BattleGround.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Battle/BattleGround.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/Battle/FormationSet.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/FormationSet.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/Skills/CombatSkill.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/CombatSkill.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/Skills/Effect.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effect.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/Skills/Effects/` (29 SubEffect) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/` | вынесено |
| `unity/Assets/Scripts/Mechanics/Skills/Skill.cs` (MoveComponent) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/MoveComponent.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/RandomSolver.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/RandomSolver.cs` | вынесено |
| `unity/Assets/Scripts/Mechanics/AI/MonsterBrain.cs` + desires | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/AI/` | вынесено |
| `unity/Assets/Scripts/Mechanics/MechanicsDefines.cs` (AttributeType/enums) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/` | вынесено |
| `unity/Assets/Scripts/Raid/Party/FormationUnit.cs` + `FormationParty.cs` + `FormationUnitInfo.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Party/` | вынесено |
| `unity/Assets/Scripts/Character/Character.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Character.cs` | вынесено |
| `unity/Assets/Scripts/Character/Hero.cs` (вкл. моды Абоминации) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Hero.cs` | вынесено |
| `unity/Assets/Scripts/Character/Components/BattleModifier.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Components/IBattleModifier.cs` | вынесено |
| `unity/Assets/Scripts/Database/DarkestDatabase.cs` (загрузка героев/скиллов) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/HeroClassFileParser.cs` + `HeroCatalog.cs` | вынесено |
| `unity/Assets/Resources/Data/JsonQuirks.json` | `src/Core/Sektor.DarkestDungeon.Core.Content/Character/Quirk.cs` + `Database/QuirkMapper.cs` | вынесено |
| `unity/Assets/Resources/Data/JsonBuffs.json` | `src/Core/Sektor.DarkestDungeon.Core.Content/Character/BuffContent.cs` + `Database/BuffContentMapper.cs` | вынесено |
| `unity/Assets/Resources/Data/JsonTraits.json` (аффекции/виртуды дуэли) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/JsonTrait.cs` | вынесено |
| `unity/Assets/Scripts/Networking/RaidSceneMultiplayerManager.cs` (legacy-заглушка PvP, заменена дуэлью) | `src/Core/Sektor.DarkestDungeon.Core.Duel/` | вынесено |
| `unity/Assets/Scripts/Raid/TorchMeter.cs` (торч + сюрприз 1-го раунда в дуэли) | `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` | вынесено |
| `unity/Assets/Resources/Data/Mechanics/Effects.txt` (каталог: stress/heal/stun/dots/pull/push/cure/riposte/guard/shuffle/tag/stat-buff/buff_ids/torch/set_mode/unstun/unimmobilize/untag) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/EffectCatalog.cs` | вынесено |
| `unity/Assets/Scripts/Character/Monster.cs` + `MonsterData.cs` | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Monster.cs` | вынесено |
| `unity/Assets/Resources/Data/Monsters/` (460 `.txt`, парсер → `MonsterCatalog`) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/MonsterClassFileParser.cs` + `Character/MonsterCatalog.cs` | вынесено |
| `unity/Assets/Resources/Data/JsonAI.json` (brains → `MonsterBrainCatalog`) | `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/AI/JsonBrainParser.cs` (чистый) + `MonsterBrainCatalog.cs` | вынесено |
| `unity/Assets/Resources/Data/JsonBuffs.json` (DTO + каталоги, общий ридер) | `src/Clients/Sektor.DarkestDungeon.Clients.Content/GameDataReader.cs` (Newtonsoft-фасад) | вынесено |
| `unity/Assets/Scripts/Generation/DungeonGenerator.cs` (топология+население+enviroment+quest-цели) + `MapGenerator.txt` + `Dungeons/*.bytes` (парсеры) | `src/Core/Sektor.DarkestDungeon.Core.Raid/Generation/` (`DungeonGenerator`, модели, парсеры, `DungeonQuestGoal`) | вынесено (legacy — `DungeonGeneratorLegacy.cs`) |

> **Реорганизация ядра (данные = домен, см. `TARGET_LAYOUT.md`):** `Core.Data` распущен; DTO/парсеры/
> каталоги разведены по доменам (`Core.Campaign`, `Core.Raid`, `Core.Content\Camping|Trinket`,
> `Core.Combat\AI`); `Result`/`IProportionValue` — в `Core.Common`; `IBinarySaveData` — в `Core.Save`;
> `TextFightContent` — в `Core.Duel\Fight`; `GameDataReader` (Newtonsoft) — на клиентской границе
> `Clients.Content`. Манифест ниже ведёт только Unity→core вынос.

## Не вынесено (Unity-side, по дорожной карте `PLAN.md`)

| Unity (legacy) | Core | Статус |
| --- | --- | --- |
| `unity/Assets/Scripts/Setup/SaveSystem/` | — | не вынесено (дорожная карта: Save) |
| `unity/Assets/Resources/Data/Buildings/` | — | не вынесено (дорожная карта: Campaign) |
| `unity/Assets/Resources/Data/Dungeons/` | — | не вынесено (дорожная карта: Encounters) |
| `unity/Assets/Resources/Data/Curios/` | — | не вынесено (дорожная карта: Curios) |
| `unity/Assets/Scripts/Networking/` | — | не вынесено (дорожная карта: Networking) |
| `unity/Assets/Scripts/UI/` | — | не вынесено (Presentation; остаётся Unity/WPF, бизнес-логика — в ядро) |

## Паритет-разрывы (механики, которые в ядре неполны)

> **Закрыто** (в ядре; подробности — `BATTLE_PARITY.md` §5): DoT-тик урона (в начале хода цели),
> stun-пропуск хода + `STUNRECOVERYBUFF`, riposte-контратака (+ парсинг `riposte_skill`),
> guard (`.guard`/`.swap_source_and_target`/`.clearguarding`/`.clearguarded` + редирект атак),
> pull/push/shuffle (реальные ранги), immobilize (блок `TryMove` + `.unimmobilize`/`.unstun`/`.untag`),
> `RemoveConditions` после скилла; buff-идемпотентность (`ApplyBuff`/`RevertBuff` с `IsApplied`-гейтом);
> **death's door** (вход при 0 HP, `DeathBlow`-ролл, survival-бафф, хил-снятие) + **heart attack**
> (стресс 200); `MonsterClass.CanDieFromDamage` (парсинг `death_class:`).

Остаётся отдельной задачей:

- Idle-юниты (0 ходов за раунд): DoT-тик ×1.5.
- `.kill`/`.kill_enemy_type` + корпус-подстановка (`MarkedForDeath` учитывается, смена класса — нет).