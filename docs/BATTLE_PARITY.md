# BATTLE_PARITY.md — Паритет: мультиплеерная дуэль Unity vs WPF-дуэль (ядро)

> **Что это.** Единый справочник по разрывам механик между двумя реализациями одного режима:
> legacy-мультиплеерная дуэль Unity (`unity\Assets\Scripts\Networking\RaidSceneMultiplayerManager.cs` +
> базовый `RaidSceneManager`) и WPF-дуэль, которая исполняется на чистом ядре
> (`src\Core\Sektor.DarkestDungeon.Core.Combat` + `src\Core\Sektor.DarkestDungeon.Core.Duel`).
>
> **Правило.** Legacy Unity — живая реализация и **не правится** в рамках закрытия разрывов
> (до cutover, см. `EXTRACTION_PLAN.md` Фаза 6). Разрывы закрываются в ядре; обе стороны —
> `unity\` и `unity-2017\` — наследуют ядро автоматически. Этот документ обновляется **в том же
> коммите**, что и код, меняющий задокументированные здесь факты (правило «доки в том же коммите»).
>
> **Связанные доки:** `DUEL_ARCHITECTURE.md` (состояние/происхождение), `UNITY_LEGACY_MAP.md`
> (карта всех классов Unity), `ARCHITECTURE_REVIEW.md` (критика), `TARGET_LAYOUT.md` (целевая
> декомпозиция), `GAME_RULES.md` (правила «как в этом репо» vs оригинал DD).

## 0. Модель исполнения (важно для понимания строк)

| | Unity MP | WPF-дуэль (ядро) |
|---|---|---|
| Движок | legacy `RaidSceneManager` (6043 стр.) + `BattleSolver` | `BattleSolver` в `Core.Combat` |
| Синхронизация | RPC-репликация общего состояния (оба клиента крутят один бой) | локстап: обмен только вводами `DuelPayload`, состояние считается детерминированно по `DuelSeed` |
| Стороны | обе стороны — герои; «монстры» — отряд приглашённого игрока | обе стороны — герои (`DuelHeroPick`); монстры — только в Fight-раннере |
| Цепочка скилла | `HeroTurn` → `ExecuteHeroSkill` (`RaidSceneMultiplayerManager.cs:1662`) → `RaidSceneManager.cs:4292-4380` → `ExecuteSkillBase` → `BattleSolver.ExecuteSkill` (`RaidSceneManager.cs:3958-4002`) | `DuelController.ExecuteSkill` (`DuelController.cs:516-525`) → `BattleSolver.ExecuteSkill` (`BattleSolver.cs:385-502`) → `ApplyEffects` (`BattleSolver.cs:559-567`) → `ProcessEventQueues` (`DuelController.cs:527-539`) |

## 1. Легенда

- ✅ **одинаково** — механика реализована и действует в обеих реализациях.
- ⚠️ **разрыв** — в Unity MP работает, в ядре/дуэли отсутствует или неполна.
- 🧊 **стаб в обоих** — не работает ни там, ни там (не разрыв, а общий долг).

## 2. Матрица по группам эффектов

### 2.1 Урон / хил / крит / меткость

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Hit/Dodge/крит, `acc − dodge`, `ceil(Lerp(Min,Max)·(1+DmgMod)·(1−prot))`, крит ×1.5 | `BattleSolver.cs:361-422` | `BattleSolver.cs:443-493` | ✅ |
| Heal-компонент скилла (крит-хил ×1.5, `HpHealPercent`) | `BattleSolver.cs:324-350` | `BattleSolver.cs:406-439` | ✅ |
| Крит-стресс цели «Stress 2» / крит-хил-снятие | `BattleSolver.cs:409` | `BattleSolver.cs:489`, `:424` | ✅ |

### 2.2 Stress / resolve / аффекции

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| `.stress`/`.healstress` (герои, реверт аффекции на 0) | `StressEffect.cs:14-47`, `StressHealEffect.cs:14-41` | `StressEffect.cs:28-110`, `StressHealEffect.cs:28-113` | ✅ |
| Resolve-ролл (аффекция/виртуда) при перегрузке | очередь в `StressEffect` (`:35-43`) | `DuelBattleContext.ResolveOverstress` (`DuelBattleContext.cs:143-198`) | ✅ |
| Стресс отряду при смерти героя («Stress 2»/«Stress 3») | `RaidSceneMultiplayerManager.cs:1955` | `DuelController.CheckDeaths`/`StressParty` (`DuelController.cs:594-631`) | ✅ |
| **Heart attack** (сердечный приступ при перегрузке) | очередь `StressEffect` | `DuelBattleEvents.AddHeartAttackCheck` → `HeartAttackHandler`: на death's door → смерть, иначе → 100% HP + стресс 75% + death's door | ✅ |
| **Death's door** (DeathResist + `DeathsDoorSurvivalDebuff`) | `RaidSceneMultiplayerManager.cs:112-120, 2020-2031` | `DeathCheck`: вход в death's door при 0 HP (баффы + `BarkStress` 6), ролл `DeathResist − resistIgnoreBonus(0.3)`, `DeathsDoorSurvivalDebuff`, хил снимает | ✅ |

### 2.3 DoT (bleed / poison)

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Наложение + резист (`chance − resist + performerChance`) | `BleedEffect.cs:13-39`, `PoisonEffect.cs:13-39` | `BleedEffect.cs:24-63`, `PoisonEffect.cs:24-63` | ✅ |
| **Тик урона** в начале хода цели (`CurrentTickDamage`) | `RaidSceneMultiplayerManager.cs:1129-1199` (bleed), `:1202-1274` (poison) | `DuelController.BeginTurn` применяет `CurrentTickDamage` (bleed+poison) в начале хода цели, `CheckDeaths` после | ✅ |
| Idle-юниты (нет ходов): тик ×1.5 | `RaidSceneMultiplayerManager.cs:1022-1104` | — (тиков нет) | ⚠️ |

### 2.4 Stun / immobilize / move-блокировки

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Наложение стана + резист + сброс guard | `StunEffect.cs:7-32` | `StunEffect.cs:15-58` | ✅ |
| **Пропуск хода** при стане + `STUNRECOVERYBUFF` | `RaidSceneMultiplayerManager.cs:1279-1296` | `DuelController.BeginTurn`: стан снимается, `STUNRECOVERYBUFF` применяется, ход пропускается (`CompleteTurn`) | ✅ |
| Истечение стана | `Character.ApplyStunRecovery` | стан снимается в начале хода цели (`BeginTurn`), затем recovery-бафф на 2 раунда | ✅ |
| Стартовый +40% stun-resist всем 8 юнитам (мультиплеер-хак) | `RaidSceneMultiplayerManager.cs:417-438` | — | ⚠️ (опционально, режимное) |
| Immobilize: блок self-move скилла | `BattleSolver.cs:312` | `BattleSolver.cs:398` | ✅ |
| **Immobilize: блок ручного Move + истечение** | `FormationUnit.cs:210, 233`; `RaidSceneMultiplayerManager.cs:2255, 2262` | `TryMove` (`DuelController.cs:440-460`) проверяет `IsImmobilized`; `.unimmobilize`/`.unstun`/`.untag` парсятся `EffectCatalog` | ✅ |

### 2.5 Riposte / guard / mark

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Наложение riposte-статуса + rule-баффы (`BuffRule.Riposting`) | `RiposteEffect.cs:16-42` | `RiposteEffect.cs:30-68` | ✅ |
| **Контратака** при атаке по рипост-юниту (`RiposteSkill`) | `ExecuteRiposteSkillActivation` (`RaidSceneManager.cs:3818-3863`) | `DuelController.ExecuteRiposte` после `ExecuteSkill`; `RiposteSkill` парсится (`HeroClassFileParser`, `riposte_skill`) | ✅ |
| **Guard**: парсинг `.guard` + редирект атак | `GuardEffect.cs:26-78`, `ExecuteGuardRedirection` (`RaidSceneManager.cs:3806-3816`) | `EffectCatalog` парсит `.guard`/`.swap_source_and_target`/`.clearguarding`/`.clearguarded`; `BattleSolver.ExecuteSkill` редиректит атаку на `Guarded.Guard` | ✅ |
| Mark/Tag + `buff_duration_type` (Round/Combat) | `TagEffect.cs:6-15`, `Effect.cs:360-363` | `TagEffect.cs:17-38` (duration ?? 3), `buff_duration_type` не читается | ⚠️ (незначительный) |
| Mark-таргетинг ИИ | `TargetSelectionMarked` | `DuelTargetSelectionMarked.cs:27-42` | ✅ |

**Скиллы-жертвы (в ядре/дуэли теперь соответствуют Unity):**
- ManAtArms **Defender** — `"MAA Guard 1"` парсится в `GuardEffect`; атаки по guarded-цели редиректятся на защитника.
- HoundMaster **Guard Dog** — `"HM Guard 1"` (guard-only) → guard-эффект активен.
- ManAtArms **Retribution** — рипост-статус ставится, контратака исполняется через `RiposteSkill`.
- ManAtArms `riposte_skill` **riposte1** — парсится в `HeroClass.RiposteSkill` и исполняется при контратаке.
- **DoT-скиллы** (блайт/блед) — накладывают статус + тик урона в начале хода цели.
- **Stun-скиллы** — цель пропускает ход.

### 2.6 Перемещение рангов: pull / push / shuffle

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Move-resist ролл | `PullEffect.cs:13-32`, `PushEffect.cs:13-32` | `PullEffect.cs:24-58`, `PushEffect.cs:24-58` | ✅ |
| **Фактическое перемещение** (`FormationUnit.Pull/Push`, уважает immobilize) | `FormationUnit.cs:208-252` | `DuelBattleEvents.Pull/Push` реально двигают юнита в партии (уважает `IsImmobilized`, границы), пересчитывают `Rank` | ✅ |
| Shuffle (одиночный/отрядный) | `ShuffleTargetEffect.cs` (+`:75-94` роллы) | `ShuffleTargetEffect.cs:25-127` → те же `Events.Pull/Push` — теперь реальное перемещение | ✅ |
| Self-move скилла (`.move`/`MoveComponent`) | `BattleSolver.cs:312-318` | `BattleSolver.cs:398-404` → `Events.Pull/Push` — реальное перемещение | ✅ |

### 2.7 Buff-система (стат-баффы/дебаффы, `.buff_ids`)

| Механика | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Стат-баффы/дебаффы (add/multiply) влияют на боевую математику (`(Raw+Flat)·Mult`) | `Character.cs:539-549`; роллы читают `ModifiedValue` | `Character.cs:385-396`, `SingleAttribute.cs:15`; `BattleSolver.cs:443-476` | ✅ |
| Ключи: `attack_rating_add`, `defense_rating_add`, `protection_rating_add`, `speed_rating[_add]`, `critical_rating`/`crit_chance_add`, `damage_low/high_multiply` | `Effect.cs:600-735` | `EffectCatalog.cs:112-122` | ✅ |
| Debuff-resist ролл (`chance − target.Debuff + performer.DebuffChance`) | `CombatStatBuffEffect.cs:121-145` | `CombatStatBuffEffect.cs:149-165` | ✅ |
| `.buff_ids` из `JsonBuffs.json` (негативные — через debuff-resist) | `Effect.cs:736-748`, `BuffEffect.cs:15-75` | `EffectCatalog.cs:152-167`, `BuffEffect.cs:29-202` через `IBattleContext.GetBuff` | ✅ |
| Rule-баффы (hp/stress/light/rank/skill-type/mode/monster-type) | `Character.ApplyAllBuffRules` (`Character.cs:432-436`), триггер `ApplyConditions`/`RemoveConditions` каждый скилл | применяются (`DuelBattleContext.ApplyCombatUnitRules`); **`RemoveConditions` теперь вызывается после каждого скилла** в `DuelController.ExecuteSkill` (перформер + цель) | ✅ |

### 2.8 Прочие эффекты

| Эффект | Unity MP | WPF-дуэль | Статус |
|---|---|---|---|
| Torch (`torch_decrease`/`torch_increase`), клэмп 0–100, сюрприз-бонус | `Effect.cs:69-79`; торч=100 в MP | `Effect.cs:85-92` → `DuelBattleEvents.cs:140-153` → `DuelController.cs:108` | ✅ |
| `set_mode` + `<mode>_effects` (Абоминация), continue-turn | `SetModeEffect.cs:11-31`, `CombatSkill.cs:340-367` | `EffectCatalog.cs:186-188`, `HeroClassFileParser.cs:340-376`, `DuelController.FinishSkillAction` (`:364-370`) | ✅ |
| `.kill` / `.kill_enemy_types` | `KillEffect.cs`, `KillEnemyTypeEffect.cs:11-23` | `EffectParser.cs` → `KillEffect`/`KillEnemyTypeEffect`; `MarkedForDeath` потребляется `DeathCheck` (`Duel\Mechanics\DeathCheck.cs:60,75,108`) | ✅ |
| `.disease` (квирк герою) | `DiseaseEffect.cs:13-40` | парсится только `any` → `DiseaseEffect(null, true)` (случайная болезнь); конкретные id не резолвятся (парсер без каталога квирков) | ⚠️ (частично) |
| `.summon` / `.control` (сирена) / `.capture` / `.clearguard` | `SummonMonstersEffect.cs`, `ControlEffect.cs:13-36`, `CaptureEffect.cs:9-47`, `ClearGuardEffect.cs` | **осознанно не парсятся** — кампанийные монстры, в дуэли не встречаются | ⚠️ (осознанно) |
| `.performer_rank_target` / `.clear_rank_target` | `PerformerRankTargetEffect.cs`, `ClearRankTargetEffect.cs` | `EffectParser.cs` → `PerformerRankTargetEffect`/`ClearRankTargetEffect` | ✅ |
| `.cure` (снять bleed+poison) | `CureEffect.cs:5-40` | `CureEffect.cs:15-51` | ✅ |

## 3. Стабы в обоих (не разрыв)

| Стаб | Unity MP | WPF-дуэль |
|---|---|---|
| `riposte_chance_add` — парсится и отбрасывается (прок всегда 100%) | `Effect.cs:436-440` | `EffectCatalog.cs:136-142` |
| Mark — нет бонуса урона по метке (только AI-таргетинг) | `TargetSelectionMarked` | `DuelTargetSelectionMarked.cs` |
| `ExtraTargetsChance` — только для монстр/AI-desires | `TargetSelectionDesire.cs:82-83` | — |

## 4. Мультиплеер-специфика Unity (режим, не механика)

Партия/сид/readiness/RPC (`RaidSceneMultiplayerManager.cs:31-49`, `MultiplayerSync.cs:370-389`),
фиксированный арена-квест (`tutorial_room`/`weald`, `RaidSceneMultiplayerManager.cs:54-94`),
retreat с trait-роллами (`:1685-1801`), bark-релеи (`:1588-1623`), результаты/победа (`:485-512`).
WPF-дуэль использует другую (lockstep) модель — сравнение см. в `DUEL_ARCHITECTURE.md` §2, §4.

## 5. Резюме: что закрыто в ядре (приоритет)

> Закрыто (в ядре; legacy Unity не менялся):
>
> 1. ✅ **DoT-тик урона** — `DuelController.BeginTurn` применяет `CurrentTickDamage` (bleed+poison)
>    в начале хода цели + `CheckDeaths`; статусы/баффы тикают per-turn (`UpdateRound` в `BeginTurn`).
> 2. ✅ **Stun: пропуск хода + истечение** — `BeginTurn` снимает стан, применяет `STUNRECOVERYBUFF`
>    (через `IDuelContent.GetBuff`), пропускает ход.
> 3. ✅ **Riposte-контратака** — `DuelController.ExecuteRiposte` исполняет `RiposteSkill` цели против
>    атакующего; `HeroClassFileParser` парсит `riposte_skill`.
> 4. ✅ **Guard** — `EffectCatalog` парсит `.guard`/`.swap_source_and_target`/`.clearguarding`/
>    `.clearguarded`; `BattleSolver.ExecuteSkill` редиректит атаку на `Guarded.Guard`.
> 5. ✅ **Pull/Push/Shuffle** — `DuelBattleEvents.Pull/Push` реально двигают юнитов в партии
>    (уважают `IsImmobilized`, границы), пересчитывают `Rank`.
> 6. ✅ **Immobilize** — `DuelController.TryMove` блокируется при `IsImmobilized`; `.unimmobilize`/
>    `.unstun`/`.untag` парсятся.
> 7. ✅ **RemoveConditions после скилла** — `DuelController.ExecuteSkill` вызывает `RemoveConditions`
>    для перформера и цели после `ProcessEventQueues`/`CheckDeaths`.
> 8. ✅ **Buff-идемпотентность** — `Character.ApplyBuff`/`RevertBuff` получили `IsApplied`-гейт
>    (как в Unity), чтобы повторное применение правил не накладывало бафф дважды.
> 9. ✅ **Kill-эффекты** — `EffectParser` парсит `.kill` (→ `KillEffect`) и `.kill_enemy_types`
>    (→ `KillEnemyTypeEffect`); `MarkedForDeath` потребляется `DeathCheck`.
> 10. ✅ **Rank-target эффекты** — `.performer_rank_target`/`.clear_rank_target` парсятся.
> 11. ✅ **Логирование** — ядро принимает структурный `ILogger` (`Core.Common`); WPF-клиент
>     подключает MS-абстракцию (`Microsoft.Extensions.Logging.Abstractions 3.1.12`) + файловый
>     логгер (`Logs\duel.log`) через `MsLoggerAdapter`/`FileLoggerProvider`.

Остаётся отдельной задачей (кампанийные механики, больше объём):

- Idle-юниты (0 ходов за раунд): DoT-тик ×1.5 (`RaidSceneMultiplayerManager.cs:1022-1104`).
- `.kill` death-class corpse-подстановка (смена класса на corpse-монстра после смерти не реализована).
- `.disease` с конкретным id квирка (парсится только `any`); `.summon`/`.control`/`.capture` —
  осознанно не парсятся (кампанийные монстры, в дуэли не встречаются).

Каждый пункт — задача в ядре (`PLAN.md`); в Unity ничего не меняется.