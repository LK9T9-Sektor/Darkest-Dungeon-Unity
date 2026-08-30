# 05_guard.md — Guard: статусы, редирект атак, clearguard

> Домен: `combat` (ядро `Core.Combat` + `Core.Duel`). Статус: **реализовано** (паритет §2.5 закрыт).

## 1. Назначение и когда работает

Юнит-«защитник» берёт под защиту союзника: атаки по охраняемому **перенаправляются на защитника**.
Guard моделируется парой статусов: `Guard` (сторона защитника, список охраняемых) и `Guarded`
(сторона охраняемого, ссылка на защитника). Снимается истечением, `clearguard`, станом или смертью.

## 2. Модель данных

- `GuardStatusEffect` (`Character/Statuses/GuardStatusEffect.cs:7`) — `Targets` (охраняемые),
  `IsApplied = Targets.Count > 0`, `UpdateNextTurn` (декремент `GuardDuration` охраняемых, `:26-37`),
  `ResetStatus` (`:40-49`).
- `GuardedStatusEffect` (`Character/Statuses/GuardedStatusEffect.cs:7`) — `Guard` (защитник),
  `GuardDuration`, `IsApplied = Guard != null && GuardDuration > 0`, `ResetStatus` (симметрично снимает
  из `Guard.Targets`).
- `GuardEffect` (`Mechanics/Skills/Effects/GuardEffect.cs:11`) — наложение; `SwapTargets` (swap-режим).
- `ClearGuardEffect` (`Mechanics/Skills/Effects/ClearGuardEffect.cs:11`) — `SetFlags(ClearGuarding,
  ClearGuarded)`.

## 3. Парсинг контента

`EffectCatalog`: `.guard` → `GuardEffect`; `.swap_source_and_target true` → `SwapTargets`; `.duration` →
`GuardDuration`; `.clearguarding`/`.clearguarded` → `ClearGuardEffect` флаги.

```text
effect: .name "MAA Guard 1" .target "target" .guard 1 .duration 2
effect: .name "Antiq ProtectMe Guard" .target "target" .guard 1 .swap_source_and_target true .duration 2
```

## 4. Порядок срабатывания (трассировка)

**Наложение** — `GuardEffect.ApplyInstant` (`GuardEffect.cs:45`):

1. Если защитник сам `Guarded` — сначала `ResetStatus` его охраны (`:56-57`).
2. Если защитник уже `Guard`:
   - цель уже в `Targets` → продлить `GuardDuration` (`:61-65`);
   - иначе — сбросить старые `Guard`/`Guarded` цели, установить `Guarded.Guard = performer`,
     `GuardDuration = duration ?? 1`, добавить в `performerGuard.Targets` (`:67-78`).
3. Иначе (нет активного guard) — то же самое без ветки продления (`:80-89`).

**Редирект атак** — `BattleSolver.ExecuteSkill` (`BattleSolver.cs:393-397`):

```csharp
var guarded = targetUnit.Character.GetStatusEffect(StatusType.Guarded) as IGuardedStatusEffect;
if (targetUnit.Team != performerUnit.Team && guarded != null && guarded.IsApplied
    && guarded.Guard != null && guarded.Guard != performerUnit)
    targetUnit = guarded.Guard;
```

- Применяется **только к вражеским целям** (`Team !=`) и **до** всех расчётов урона.

**Снятие**:
- Истечение: `GuardStatusEffect.UpdateNextTurn` (`:26-37`) декрементит `GuardDuration` охраняемых
  в начале их хода (per-turn, через `UpdateRound`).
- `ClearGuardEffect` (`.clearguarding`/`.clearguarded`): `ResetStatus` соответствующих статусов
  (`ClearGuardEffect.cs:43-52`).
- Стан цели: `StunEffect.ApplyInstant` сбрасывает `Guard` (`StunEffect.cs:36`).
- Смерть/снятие через `GuardedStatusEffect.ResetStatus` — симметричная очистка.

## 5. Очередь и обновления

- Наложение — мгновенно (`ApplyInstant`) или в `EventQueue` (если `.queue`).
- Истечение — per-turn через `UpdateRound` (`GuardStatusEffect.UpdateNextTurn` вызывается на ходу
  **охраняемого**).
- Редирект — до урона/эффектов скилла.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Только вражеская атака | `BattleSolver.cs:394` | `Team !=` |
| Статус применён | `:394` | `Guarded.IsApplied` |
| Защитник жив/не атакующий | `:395` | `Guard != null && Guard != performer` |
| Длительность | `GuardEffect.cs:63,73` | `duration ?? 1` |

## 7. Нюансы и подводные камни

- **Редирект — в `BattleSolver`, а не в `DuelController`** — потому что применяется и к ИИ-ходам
  (`UseMonsterBrain`), и к ручным. Менять только в одном месте нельзя.
- **`Guarded.IsApplied` требует `Guard != null`** — редирект не сработает, если цель «охраняема» по
  таймеру, но защитник уже мёртв/снят (`Guard` обнулён).
- **Стан цели сбрасывает её guard-защиту** (`StunEffect.cs:36`), но НЕ guard самой цели на других.
- `ClearGuardEffect` возвращает `false` из `ApplyInstant` (`:61`) — это норма: эффект «пустой», но
  снимает guard; в каталог добавляется (не отбрасывается).
- Swap-режим (`Antiq ProtectMe Guard`): защитник = цель, охраняемый = performer — **обязательно**
  учитывать при трассировке (эффект с `target`-селектором).

## 8. Взаимодействия

- Стан (`03_stun.md`) сбрасывает guard цели.
- Урон/крит (`01_damage.md`): крит-стресс по защитнику (он принял удар).
- Riposte (`04_riposte.md`): контратака идёт по атакующему, а не по охраняемому.
- RemoveConditions (`09_buffs.md`) не трогает guard-статусы (это не rule-бафф).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Combat/Character/Statuses/GuardStatusEffect.cs`,
  `GuardedStatusEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Skills/Effects/GuardEffect.cs`,
  `ClearGuardEffect.cs`, `StunEffect.cs`
- `src/Core/Sektor.DarkestDungeon.Core.Combat/Mechanics/Battle/BattleSolver.cs`
- `tests/Core/Sektor.DarkestDungeon.Core.Duel.Tests/ParityMechanicsTests.cs` (`Guard_...`)