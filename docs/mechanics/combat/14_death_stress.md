# 14_death_stress.md — Смерть, death's door, стресс отряда, resolve, heart attack

> Домен: `combat` + `duel` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано**
> (death's door / heart attack закрыты; `.kill`/`.kill_enemy_types` парсятся — `KillEffect`/
> `KillEnemyTypeEffect` ставят `MarkedForDeath`; корпус-подстановка — остаток).

## 1. Назначение и когда работает

После урона/тиков проверяется, достиг ли юнит 0 HP. **Монстры** умирают сразу (если их
`death_class` не запрещает урон). **Герои** входят в death's door на первом ударе до 0 и на каждом
последующем роллят `DeathBlow`-резист; провал = смерть. Смерть героя даёт стресс выжившим.
Heart attack (стресс 200) — смерть на death's door или вход в неё. При стресс >= 100 — resolve-ролл.

## 2. Модель данных

- `DeathCheck` (`Core.Duel/Mechanics/DeathCheck.cs`) — Check/RollSurvival/EnterDeathsDoor/StressParty.
- `HeartAttackHandler` (`Core.Duel/Mechanics/HeartAttackHandler.cs`) — исполнение heart attack.
- `Hero` (`Core.Combat/Character/Hero.cs`) — `AtDeathsDoor`, `DeathResist` (DeathBlow, клэмп 0..0.87,
  дефолт 0.5), `ApplyDeathDoor`/`RevertDeathsDoor`/`ApplyMortality`/`RevertMortality`.
- `Character` — `SupportsDeathDoor` (Hero=true, base/Monster=false), `HealthRatio`, `IsDead`.
- `Monster`/`MonsterClass` — `CanDieFromDamage` (парсинг `death_class:`, default true).
- `DeathDoorStatusEffect`/`DeathRecoveryStatusEffect` — флаги `AtDeathsDoor`/`AtDeathRecovery`.
- `BattleConstants` — `MaxDeathResist 0.87`, `ResistOverrideBonus 0.3`, `DeathsDoorSurvivalDuration 3`,
  `DeathsDoorSurvivalValue -0.1`. `BuffIds.DeathsDoorSurvivalDebuff()`. `EffectIds.BarkStress`.

## 3. Парсинг контента

- `HeroClassFileParser`: `deaths_door:` секция (`.buffs`, `.recovery_buffs`,
  `.recovery_heart_attack_buffs`) → `HeroClass.DeathDoor` (`DeathDoor.cs`).
- `MonsterClassFileParser`: `death_class:` (`.can_die_from_damage`) → `MonsterClass.CanDieFromDamage`.
- Баффы (`deathsdoor*`, `mortality*`, `heartattack*`) — из `JsonBuffs.json`; ресолв
  **case-insensitive** (`BuffCatalog`/`TestDuelContent`).
- `Effects.txt`: `Stress 2` (15), `BarkStress` (6), `AfflictedAllyStress` (33%×5),
  `crit_heal_stress_heal` (4). `JsonTraits.json` → аффекции/виртуды.

## 4. Порядок срабатывания (трассировка)

`DuelController.ExecuteSkill` → `Solver.ExecuteSkill` → `ProcessEventQueues` → `CheckDeaths` →
`ExecuteRiposte` → `RemoveConditions` → `RecoverDeathsDoorIfHealed`.

**Смерть/death's door** — `DeathCheck.Check()` (`DeathCheck.cs`):

1. Для каждого юнита с `HealthRatio <= 0 && !IsDead`:
   - **монстр** (не `SupportsDeathDoor`): если `CanDieFromDamage` → `IsDead = true`; иначе выживает;
   - **герой**: если уже `AtDeathsDoor` или `MarkedForDeath` → шаг 3; иначе → `EnterDeathsDoor` (шаг 2).
2. `EnterDeathsDoor` (`DeathCheck.cs:EnterDeathsDoor`): `hero.ApplyDeathDoor(buffs)`, survival-бафф
   `DeathsDoorSurvivalDebuff`, `BarkStress` (6) цели, попап `DeathsDoor`. **Герой не умирает.**
3. `RollSurvival` (`:RollSurvival`): если `MarkedForDeath` → смерть; ролл
   `CheckSuccess(DeathResist − resistIgnoreBonus)`; бонус 0.3 при численном перевесе стороны;
   успех → survival-бафф + попап `DeathBlow` (выжил); провал → `IsDead = true`.
4. Для каждого умершего героя → `StressParty` (`:StressParty`): `Stress 2` (15) живым героям отряда +
   `ResolveOverstress`.
5. `DuelController.CheckDeaths` (`DuelController.cs:617`) → после `DeathCheck.Check()` вызывает
   `RemoveDeadNonMonsters(HeroParty)`/`RemoveDeadNonMonsters(MonsterParty)`: любой мёртвый не-монстр
   (герой) удаляется из партии (`FormationParty.RemoveUnit`), и оставшиеся сзади сдвигаются вперёд
   на один ранг. **Герои не оставляют труп**; corpse-монстры (`IsCorpse`) не удаляются и остаются
   на ранге.

**Хил снимает death's door** — `DuelController.RecoverDeathsDoorIfHealed` (после скилла): герой
`AtDeathsDoor` и `CurrentHealth > 0` → `RevertDeathsDoor(recovery-баффы)` + mortality-баффы.

**Heart attack** — `StressEffect.HandleOverstress` (`StressEffect.cs:112-125`) при `Stress ≈ 200`
вызывает `AddHeartAttackCheck` → `DuelBattleEvents.HeartAttackHandler` → `HeartAttackHandler.Apply`:

- на death's door → `MarkedForDeath = true`, `DeathCheck.Check()` (смерть), попапы
  `HeartAttack`+`DeathBlow`, `StressParty`;
- иначе → `TakeDamagePercent(1.0)` (HP=0), `Stress.ValueRatio = 0.75`, `DeathCheck.Check()` (вход
  в death's door).

## 5. Очередь и обновления

- `CheckDeaths` — после каждого скилла, контратаки и DoT-тиков (см. `02_dot.md`).
- `StressParty` — мгновенно (до resolve-ролла).
- Heart attack — синхронно через колбэк события (не очередь, как в Unity-корутинах).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Смерть монстра | `DeathCheck.cs` | `HealthRatio <= 0 && CanDieFromDamage` |
| Вход героя | `DeathCheck.cs` | не `AtDeathsDoor` и не `MarkedForDeath` |
| DeathResist | `Hero.cs` | клэмп 0..0.87, дефолт 0.5 |
| Бонус перевеса | `DeathCheck.cs` | 0.3 |
| Heart attack стресс | `StressEffect.cs:123` | `≈ 200` |

## 7. Нюансы и подводные камни

- **`MonsterClass.CanDieFromDamage` default = true** (в Unity `bool?` null → умирает). Без этого
  монстры без `death_class` не умирали (реальная ошибка).
- **`SupportsDeathDoor` гейт** — ветвление по свойству модели, а не `IsMonster` (расширяемо:
  монстры с death's door возможны через переопределение + данные).
- **Ресолв баффов case-insensitive** — `ParseTokens` в `HeroClassFileParser` lower-ит значения,
  а id в `JsonBuffs.json` смешанного регистра (`deathsdoorACCDebuff`).
- **Герой не умирает на первый удар до 0** — входит в death's door (это ломало старые тесты,
  ожидавшие мгновенную смерть).
- **Бой может затянуться**, если герой на death's door выживает роллы, а ИИ не добивает
  (`FightSessionTests` с боссом-свинкой) — канон, не баг.
- `MarkedForDeath` (от `.kill`/heart attack) — `RollSurvival` всегда смерть для помеченного.
- **Мёртвый герой удаляется из партии** (`DuelController.RemoveDeadNonMonsters`) — сдвиг рангов
  (пересчёт `Rank` в `FormationParty.RemoveUnit`) влияет на таргетинг/позицию; корпус-монстры
  исключены (не монстр → пропуск по `IsMonster`). WPF-карточки строятся из отряда, поэтому после
  удаления слот героя исчезает и линия перестраивается автоматически.
- **Обход удаления с конца** — `RemoveDeadNonMonsters` итерирует `party.Units` с конца, чтобы
  `RemoveUnit` (переиндексирует список) не сдвинул ещё непросмотренные элементы.

## 8. Взаимодействия

- DoT-смерть (`02_dot.md`), крит-стресс (`01_damage.md`).
- Guard/riposte: смерть после контратаки (`04_riposte.md`).
- Resolve/стресс — `GAME_RULES.md` §«Стресс».
- `MarkedForDeath` — `FormationUnitInfo.cs:19`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Duel/Mechanics/DeathCheck.cs`, `HeartAttackHandler.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`CheckDeaths`,
  `RemoveDeadNonMonsters`, `RecoverDeathsDoorIfHealed`), `DuelBattleEvents.cs` (`HeartAttackHandler`)
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Raid/Party/FormationParty.cs` (`RemoveUnit` — сдвиг рангов)
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Hero.cs`, `DeathDoor.cs`, `HeroClass.cs`,
  `Monster.cs`, `MonsterClass.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/StressEffect.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/DeathsDoorTests.cs`, `StressTests.cs`