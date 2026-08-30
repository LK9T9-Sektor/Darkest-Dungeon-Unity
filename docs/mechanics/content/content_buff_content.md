# content_buff_content.md — Бафф-контент: `JsonBuffs.json` → `Buff`

> Домен: `content` (ядро `Core.Content` + `Core.Combat`). Статус: **данные + резолв в дуэли**.

## 1. Назначение и когда работает

Определения баффов из `JsonBuffs.json` — контентные баффы, на которые ссылаются `.buff_ids`
(эффекты скиллов) и квирки. Резолвятся через `IBattleContext.GetBuff` (дуэль → `IDuelContent.GetBuff`).

## 2. Модель данных

- `Buff` (`Core.Combat/Character/Buff.cs`) — `Type` (StatAdd/StatMultiply), `RuleType` (`BuffRule`),
  `AttributeType`, `ModifierValue`, `IsFalseRule`, `SingleParam`/`StringParam`.
- `BuffCatalog` (`Core.Combat/Character/BuffCatalog.cs`) — каталог; `Load(JsonBuffData)`.
- `BuffContentMapper` (`Core.Content/Database/BuffContentMapper.cs`) — JSON → промежуточный контент.

## 3. Парсинг контента

`JsonBuffs.json` → `JsonBuffData` → `BuffContentMapper.Parse` → `BuffCatalog`.
`CharacterHelper.StringToAttributeType`/`StringToBuffType` — маппинг строк (напр. `stun` →
`AttributeType.Stun`, `resistance`/`stun` → StatAdd на Stun). `StringToBuffRule` — правило.

## 4. Порядок срабатывания (трассировка)

1. `GameDataReader.ReadBuffs(jsonText)` (`clients/GameDataReader.cs:169`) → `BuffCatalog`.
2. `DuelBattleContext.GetBuff(buffId)` (`DuelBattleContext.cs:133-136`) → `content.GetBuff`.
3. `BuffEffect` применяет баффы (`Mechanics/Skills/Effects/BuffEffect.cs:29+`): негативные — через
   debuff-resist ролл (`chance/100 − Debuff + DebuffChance`, клэмп 0.95), позитивные — сразу.

## 5. Очередь и обновления

- Контент-баффы применяются как обычные (`AddBuff`), длительность/правило из определения.
- `.buff_ids` поддерживают несколько id (`buff_ids`, `buff_ids#2`, ...) — `EffectCatalog`.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Неизвестный атрибут | `StringToAttributeType` | `Undefined` → бафф пропускается |
| Debuff-resist | `BuffEffect.cs:47-58` | `chance/100 − Debuff + DebuffChance`, клэмп 0.95 |

## 7. Нюансы и подводные камни

- **Неизвестный атрибут → бафф отбрасывается** (`AttributeType.Undefined`) — в `TestDuelContent` это
  явная проверка (`CharacterHelper.StringToAttributeType`).
- Баффы с `rule_type != always` активны только при выполнении правила (см. `09_buffs.md`).

## 8. Взаимодействия

- `09_buffs.md` — применение и правила.
- `content_quirks.md` — квирки ссылаются на buff-id.
- `STUNRECOVERYBUFF` — контентный бафф, применяется станом (`03_stun.md`).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/BuffCatalog.cs`, `Buff.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Content/Database/BuffContentMapper.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Utils/CharacterHelper.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/BuffEffect.cs`