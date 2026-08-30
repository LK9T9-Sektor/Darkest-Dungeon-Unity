# 10_torch.md — Torch: `.torch_decrease/increase`, клэмп 0–100

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.8).

## 1. Назначение и когда работает

Факел — глобальный показатель света (0–100). Эффекты скиллов меняют его
(`.torch_decrease`/`.torch_increase`), значение влияет на сюрприз первого раунда (см.
`12_surprise.md`) и rule-баффы `LightAbove`/`LightBelow`. В дуэли стартует со 75.

## 2. Модель данных

- `DuelBattleContext.TorchAmount` (`Core.Duel/DuelBattleContext.cs:25`) — значение (по умолчанию 75);
  `TorchMeter` (`:64`) — свойство для `IBattleContext`.
- `DuelBattleEvents.TorchDelta` (`Core.Duel/DuelBattleEvents.cs:18`) — колбэк мутации.
- `Effect.IntegerParams[EffectIntParams.Torch]` — суммарный дельт.

## 3. Парсинг контента

`EffectCatalog` (`EffectCatalog.cs:200-209`):

```text
.torch_decrease 5  → IntegerParams[Torch] = -5
.torch_increase 6  → IntegerParams[Torch] += 6 (суммируется с existing)
```

## 4. Порядок срабатывания (трассировка)

1. `DuelController.StartDuel`/`StartFight` регистрируют мутацию:
   `Events.TorchDelta = delta => Context.TorchAmount = Clamp(0..100, TorchAmount + delta)`
   (`DuelController.cs:109,138`).
2. Эффект с `EffectTargetType.Global` и `Torch != null` → `Effect.Apply` (`Effect.cs:85-92`):
   `DecreaseTorch`/`IncreaseTorch`.
3. `DuelBattleEvents.DecreaseTorch/IncreaseTorch` (`DuelBattleEvents.cs:171-184`) — лог + `TorchDelta`.
4. Клэмп 0..100 в колбэке (`DuelController.cs:109`).

## 5. Очередь и обновления

- Мгновенно (Global-эффекты применяются сразу в `Effect.Apply`).
- Торч-эффекты не ставят `SubEffect` — только `IntegerParams` (эффект «чистый» и не отбрасывается:
  `EffectCatalog.cs:228` проверяет `Torch` при возврате null).

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Дельт | `EffectCatalog.cs:200-209` | `torch_increase` суммируется с `torch_decrease` |
| Клэмп | `DuelController.cs:109,138` | 0..100 |
| Сюрприз-бонус | `DuelController.cs:281-291` | по диапазону торча |

## 7. Нюансы и подводные камни

- **Торч — не SubEffect**: применяется через `IntegerParams` в Global-ветке `Effect.Apply`, а не через
  `SubEffect.Apply`. Не пытаться создавать `SubEffect` для торча.
- Клэмп в колбэке (`Math.Max(0, Math.Min(100, ...))`), а не в событии — менять только в одном месте.
- Старт в дуэли — 75 (не 100, как в Unity MP — там торч фиксирован 100).

## 8. Взаимодействия

- Сюрприз 1-го раунда использует `TorchAmount` для бонуса шанса (`12_surprise.md`).
- Rule-баффы `LightAbove`/`LightBelow` (`Character.cs:463-468`) читают `rules.TorchAmount`
  (`DuelBattleContext.cs:107,115`).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/EffectCatalog.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Duel/DuelController.cs`, `DuelBattleContext.cs`,
  `DuelBattleEvents.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/SkillEffectsTests.cs` (`TorchEvents_...`)