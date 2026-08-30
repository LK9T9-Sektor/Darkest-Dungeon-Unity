# content_quirks.md — Квирки: модель, каталог, применение в дуэли

> Домен: `content` (ядро `Core.Content` + `Core.Combat`/`Core.Duel`). Статус: **данные + применение
> в дуэли** (паритет: квирки влияют на статы).

## 1. Назначение и когда работает

Квирки (черты героев) — постоянные баффы на персонажа. Применяются к герою при сборке отряда
(`DuelController.ApplyQuirks`) и влияют на боевые атрибуты. Загружаются из `JsonQuirks.json`.

## 2. Модель данных

- `Quirk` (`Core.Content/Character/`) — id, `Buffs` (id баффов), позитив/негатив.
- `QuirkCatalog` (`Core.Content/Character/QuirkCatalog.cs:8`) — `Load(JsonQuirkData)` (`:33`),
  `Get(id)` (`:54`).
- `QuirkMapper` — маппер JSON → `Quirk`.

## 3. Парсинг контента

`JsonQuirks.json` → `JsonQuirkData` → `QuirkMapper.Parse` → `QuirkCatalog`. Баффы квирка — id из
`JsonBuffs.json`, резолвятся отдельно через `IDuelContent.GetBuff`.

## 4. Порядок срабатывания (трассировка)

`DuelController.ApplyQuirks` (`DuelController.cs:557-578`):

1. Для каждого `quirkId`: `hero.AddQuirk(quirkId)` (`:564`), `GetQuirk` (`:565`).
2. Для каждого `quirk.Buffs`: `GetBuff(buffId)`; если бафф есть и у героя есть атрибут
   (`GetAttribute != null`) → `AddBuff(new BuffInfo(buff, Permanent, Quirk))` (`:570-572`).
3. Итог: `hp.CurrentValue = hp.ModifiedValue` (`:576-577`) — макс HP пересчитывается под баффы.

## 5. Очередь и обновления

- Применение — при сборке отряда (одноразово), до `StartBattle`.
- Баффы — `BuffDurationType.Permanent`, не снимаются `RemoveConditions`.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Квирк есть | `DuelController.cs:565-567` | иначе skip |
| Атрибут баффа у героя | `:571` | `GetAttribute != null` (защита от NRE) |

## 7. Нюансы и подводные камни

- **Защита `GetAttribute != null` обязательна** — был NRE: town-баффы `upgrade_discount` не имели
  атрибута у героя.
- `BuffSourceType.Quirk` — маркер происхождения (снятие в `RemoveBuff`).

## 8. Взаимодействия

- `09_buffs.md` — как баффы влияют на статы.
- `JsonBuffs.json` → `GetBuff`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Content/Character/QuirkCatalog.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Content/Database/QuirkMapper.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs` (`ApplyQuirks`)
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/QuirkBuffTests.cs`