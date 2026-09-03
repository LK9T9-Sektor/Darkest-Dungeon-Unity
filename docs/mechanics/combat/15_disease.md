# 15_disease.md — Disease: `.disease any|<id>`, резист, применение квирка

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **частично** (конкретные id — 
> реализовано; рандомный пул болезней — стаб). Паритет §2.8.

## 1. Назначение и когда работает

Эффекты скиллов монстров накладывают болезнь (негативный квирк) на героя:
`.disease any` (случайная болезнь) или `.disease <quirk_id>` (конкретная, напр. `the_worries`,
`rabies`). Применяется только к героям (не монстрам), после триггер-чанса эффекта и
резист-ролла `1 − Disease.ModifiedValue`.

## 2. Модель данных

- `DiseaseEffect` (`Core.Combat/Mechanics/Skills/Effects/DiseaseEffect.cs`) — `SubEffect`
  (`EffectSubType.Disease`), хранит `DiseaseId` (null ⇒ случайная болезнь).
- `IQuirk` (`Core.Combat/Character/IQuirk.cs`) — минимальная абстракция (`string Id`).
- `IBattleContext.GetQuirk(string)` (`Mechanics/Battle/IBattleContext.cs:95`) — резолв квирка по id
  (паттерн `GetBuff`).
- `Hero.Quirks` (`Core.Combat/Character/Hero.cs:105`) — список id применённых квирков;
  `Hero.AddQuirk(IQuirk)` (`:116`) — переопределение `Character.AddQuirk`.

## 3. Парсинг контента

`EffectParser.cs:157-160`:

```text
.disease any        → new DiseaseEffect(null)          (случайная болезнь)
.disease the_worries → new DiseaseEffect("the_worries") (id сохраняется)
```

Раньше не-`any` id молча отбрасывались (`DiseaseEffect(null, true)`), теперь парсятся.

## 4. Порядок срабатывания (трассировка)

1. Скилл выполняется → `Effect.Apply` → `DiseaseEffect` ставится в очередь цели
   (`EffectEvent`), `ApplyQueued` (`DiseaseEffect.cs:41-60`).
2. `RollDiseaseChance` (`:82-85`): `chance = 1 − Disease.ModifiedValue`; провал → попап
   `DiseaseResist`, эффект не применяется.
3. `TryResolveDisease` (`:66-80`): триггер-чанс эффекта, затем:
   - конкретный id → `battleContext.GetQuirk(id)` (`DuelBattleContext.GetQuirk`
     `Core.Duel/DuelBattleContext.cs:141` → `IDuelContent.GetQuirk` → адаптер `QuirkReference`);
   - `any` → `target.Character.AddRandomDisease()` (**стаб**: в ядре пул болезней не вынесен,
     возвращает null ⇒ болезнь не применяется).
4. `target.Character.AddQuirk(disease)` — при успехе: `SetHalo(target,"disease")` +
   `ShowPopup(target, PopupType.Disease, disease.Id)` (`DiseaseEffect.cs:53-57`).
5. `ApplyInstant` (`:28-38`) — та же логика без фидбека (попапы только в `ApplyQueued`).

## 5. Очередь и обновления

- Queued (обычный путь скилла). Instant — только для `DuelBattleContext.ApplyEffectById`
  (`DuelBattleContext.cs:127`, рейд-эффекты типа `Stress 2`).
- Квирк пишется в `Hero.Quirks` сразу; длительность/снятие болезней — вне боя (кампания, Фаза 4).

## 6. Проверки и клэмпы

| Условие | Где |
|---|---|
| Цель — не монстр | `DiseaseEffect.cs:22,43` |
| Триггер-чанс эффекта | `effect.IntegerParams[EffectIntParams.Chance] / 100` |
| Резист болезни | `1 − Disease.ModifiedValue` |
| Null-резолв (нет квирка / пул не вынесен) | `TryResolveDisease` возвращает false — **без NRE** |

## 7. Нюансы и подводные камни

- **NRE-ловушка (закрыта):** раньше `ApplyQueued` разыменовывал `AddRandomDisease().Id`
  (`DiseaseEffect.cs:83`), а `Character.AddRandomDisease()` возвращает null → крах боя
  (`maggot_C`). Теперь любой null-резолв = «не применено», без исключения.
- **Рандомный пул — стаб:** `.disease any` не применяет болезнь, пока в ядре нет пула болезней
  (нужен каталог квирков в контексте боя). Конкретные id работают через `GetQuirk`.
- **`QuirkReference`** — приватный адаптер `IQuirk` в `DuelBattleContext` (контентный `Quirk`
  из `Core.Content` не реализует доменный `IQuirk`; модуль `Content` не зависит от `Combat`).
- Попап `DiseaseResist` показывается только в queued-пути (instant — без UI-фидбека).

## 8. Взаимодействия

- `AttributeType.Disease` — стат-атрибут героя (резист болезни).
- `IQuirk`/`Hero.Quirks` — используются будущим видом Тест-боя для отображения болезней героев.
- `GetQuirk` в `IBattleContext` — тот же паттерн, что `GetBuff` (`.buff_ids`), см. `09_buffs.md`.

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/DiseaseEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/EffectParser.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/IBattleContext.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/{Hero,Character,ICharacter,IQuirk}.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelBattleContext.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Combat.Tests/Mechanics/DiseaseEffectTests.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Combat.Tests/EffectCatalogTests.cs`
- `tests/Clients/Sektor.DarkestDungeon.Clients.Content.Tests/FightPveSweepTests.cs`