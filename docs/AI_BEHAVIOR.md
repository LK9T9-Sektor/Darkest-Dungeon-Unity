# AI_BEHAVIOR.md — ИИ противника в Darkest Dungeon и его зеркало в дуэли

Как устроено поведение ИИ в DD (данные + фреймворк), что из этого есть в чистом ядре, и как
дуэль (`src\Core\Sektor.DarkestDungeon.Core.Duel`) повторяет это «поверх» — без правок
оригинальной логики (`Core.Combat` и legacy Unity остаются нетронутыми).

## 1. Модель ИИ в DD

Каждый враг (монстр) имеет **brain** (`MonsterBrain`): набор взвешенных желаний и кулдауны.
Данные лежат в `unity\Assets\Resources\Data\JsonAI.json` (`monster_brains`, ~50+ брейнов) и
читаются `DarkestJsonReader.GetJsonAI`.

```
MonsterBrain {
  skill_cooldowns:           [ { combat_skill_id, amount } ]
  skill_selection_desires:   [ желания выбора скилла с base_chance ]
  target_selection_desires:  [ желания выбора цели с base_chance ]
  bonus_initiative_desires:  [ бонусы к порядку хода ]
}
```

**Выбор скилла** (`SkillSelectionDesire.SelectSkill`, цикл в `BattleSolver.UseMonsterBrain`):
1. Взвешенный случайный выбор желания из `SkillDesireSet` (`RandomSolver.ChooseByRandom` по
   `base_chance`).
2. Желание проверяет **ограничения** (`IsRestricted`: условия боя — число монстров/героев,
   отметки, инициатива, кулдауны, режим) и **валидность** скилла (`IsValidSkill`: используем
   с ранга + не на кулдауне + `per_turn_limit`/`per_battle_limit`/`is_continue_turn`).
3. Если желание выбрало скилл — заполняется `MonsterBrainDecision` (Perform + скилл + цели),
   скилл уходит на кулдаун (`SkillCooldown` копируется в `CombatInfo.SkillCooldowns`).
4. Невалидно → желание удаляется, пробуется следующее. Всё невалидно → **Pass**.

**Выбор цели** (`TargetSelectionDesire.SelectTarget`):
1. Взвешенный выбор желания из `TargetDesireSet`.
2. Желание фильтрует цели (`FilterTargets`: deaths door, последний герой, оверстресс/аффект/
   вирту, отметка) и выбирает (случайно / по HP / по рангу / по классу / по резисту/стрессу).

**Словарь желаний** (типы в `JsonAI.json` → классы):

| Тип (JSON) | Класс в ядре | Поведение |
|---|---|---|
| `preferred_skill` | `SkillSelectionPreferred` | скилл по индексу `PreferableSkill` |
| `random_skill` | `SkillSelectionRandom` | любой валидный скилл |
| `heal_skill` | `SkillSelectionHeal` | хил скилл, если у союзника HP < порога |
| `specific_skill` | `SkillSelectionSpecific` | конкретный скилл (+эскалация шанса по раундам) |
| `ally_dead_skill` / `ally_alive_skill` | `SkillSelectionAllyDead/Alive` | при гибели/наличии класса союзника |
| `performing_turn_skill` | `SkillSelectionPerformingTurn` | условие на текущий ход |
| `effect_key_status_skill` | `SkillSelectionStatus` | при статусе цели (отметка/яд/кровь/стан) |
| `random_target` | `TargetSelectionRandom` | случайная цель |
| `marked_target` | `TargetSelectionMarked` | только отмеченные |
| `health_target` | `TargetSelectionHealth` | по HP (самый раненый для хила) |
| `rank_target` / `stress_target` / `resistance_target` / `ally_class_target` | `TargetSelectionRank/Stress/Resistance/AllyClass` | по рангу/стрессу/резисту/классу |
| бонусы инициативы | `BonusInitiative*` | скорость/порядок хода |

**Ключи данных желаний** (общие): `base_chance`, `specific_combat_skill_id`,
`is_enemy_target_desire`, `is_friendly_target_desire`; скилл-желаний: `combat_skill_id`,
`hp_ratio_treshold`, `first_initiative_only`, `per_round_chance`, `ally_base_class_id`,
`effect_key_status`; цель-желаний: `can_target_deaths_door`, `can_target_last_hero`,
`can_target_not_overstressed`, `can_target_afflicted`, `can_target_virtued`, `is_greater_comparison`.

**Пример — «default» брейн** (база для большинства врагов):

```json
skill_selection_desires: [
  { "type": "preferred_skill", "data": { "base_chance": 1.0 } },
  { "type": "random_skill",    "data": { "base_chance": 1.0 } },
  { "type": "heal_skill",      "data": { "base_chance": 100.0, "hp_ratio_treshold": 0.5 } }
]
target_selection_desires: [
  { "type": "random_target",  "data": { "base_chance": 2.0, "specific_combat_skill_id": "",
      "is_exclusive_desire": false, "is_enemy_target_desire": true,  "is_friendly_target_desire": false } },
  { "type": "marked_target",  "data": { "base_chance": 1.0, "specific_combat_skill_id": "",
      "is_exclusive_desire": false, "is_enemy_target_desire": true,  "is_friendly_target_desire": false } },
  { "type": "health_target",  "data": { "base_chance": 100.0, "specific_combat_skill_id": "",
      "is_exclusive_desire": false, "is_enemy_target_desire": false, "is_friendly_target_desire": true,
      "is_greater_comparison": false } }
]
```

Поведение: хилер лечит самого раненого союзника (<50%), иначе — случайная/отмеченная атака.

## 2. Что есть в чистом ядре (Core.Combat)

- `MonsterBrain`, `SkillCooldown`, `MonsterBrainDecision`, `BrainDecisionType`,
  `SkillSelectionDesire`/`TargetSelectionDesire` (базовые) и **sealed** конкретные желания
  (данные-конструкторы `Dictionary<string, object>`), `RandomSolver.ChooseByRandom`.
- `BattleSolver.UseMonsterBrain(performer, override)` — DD-цикл выбора (для `IsMonster`),
  с применением кулдаунов.
- Ограничения и кулдауны: `SkillSelectRestriction`, `performer.CombatInfo.SkillCooldowns`,
  `skill.LimitPerTurn`/`IsContinueTurn`/`ExtraTargetsChance`.

**Известные несоответствия оригиналу** (не трогаем — «минимальный дифф»/«поверх»):
- `Character.Brain` и `Character.CombatSkills` у героев `null` — желания для героев требуют
  фолбэков (в дуэли решается внедрением брейна в сами желания).
- `SkillSelectionHeal.IsValidSkill` — инвертированное условие (`if (IsNullOrEmpty(CombatSkillId))
  return skill.Id == ""`): с пустым id хил не срабатывает. В дуэли не используется (свой хил-desire).
- `TargetSelectionHealth` не сортирует по HP (только фильтр по классу + случайный выбор).
  В дуэли — свой HP-сортированный health-desire.
- Парсер скиллов (`HeroClassFileParser`) не читает эффекты/кулдауны/`per_turn_limit` —
  кулдауны в дуэли пока не заполнены (см. `EXTRACTION_PLAN` P1.1).

## 3. Зеркало в дуэли (Core.Duel — «поверх»)

Соперник в vs-AI ходит как монстр DD: `DuelAi` строит «default» брейн и гоняет DD-цикл.

- `DuelSkillSelection` — аналог `random_skill`, но по `CurrentCombatSkills` (выбранные скиллы
  героя); базовый `SelectSkill` (RandomSolver + цели из внедрённого брейна).
- `DuelSkillSelectionHeal` — аналог `heal_skill` (корректная логика: `skill.Heal != null`,
  союзник с HP < порога, только Health-цель).
- `DuelTargetSelectionRandom` / `DuelTargetSelectionMarked` / `DuelTargetSelectionHealth` —
  аналоги DD (random-враг, marked-фильтр, HP-сортировка с `is_greater_comparison` +
  enemy/friendly; обязательный ключ `specific_combat_skill_id=""`, как в JSON).
- `DuelAi.ChooseAction(duel)` — DD-цикл: `RandomSolver.ChooseByRandom(SkillDesireSet)` →
  `SelectSkill` → кулдаун (`SkillCooldown.Copy` в `CombatInfo.SkillCooldowns`) → payload
  `DuelPayload.Skill(skillId, targetId)` или `pass|0`.
- Брейн «default»: heal 100/<0.5 + random 1; цели random 2/enemy + marked 1/enemy +
  health 100/friendly. Детерминирован (RandomSolver + сид сессии) — реплеи сходятся.

Желания-желания несут внедрённый `MonsterBrain` сами (переопределяют
`GetMonsterCombatSkills` → `CurrentCombatSkills` и `GetMonsterBrain` → брейн), поэтому
`Character.Brain` трогать не нужно.

## 4. Ручная проверка (vs AI)

- Хилер (Vestal/Plague Doctor/Occultist) при раненом союзнике <50% лечит самого раненого.
- Прочие атакуют случайно (и по отметке, когда механика отметок появится — P1.1).
- Бой детерминирован: тот же сид → тот же ход соперника.
- Кулдауны применятся, когда парсер начнёт читать `skill_cooldowns`/лимиты скиллов (P1.1).

## 5. Будущее

- P1.1 (`EXTRACTION_PLAN`): парсинг эффектов/кулдаунов/лимитов → кулдауны и отметки оживут.
- Расширение брейнов дуэли (специфические «герой-роли»: танк/хил/бюфер) — по образцу
  конкретных брейнов из `JsonAI.json`.
- Возможно сведение Unity-мультиплеера к `Core.Duel` (см. `DUEL_ARCHITECTURE.md`) — там же
  заработает DD-ИИ для «второй стороны», когда она понадобится.