# Capture: Death's Door не считается смертью, выбор run для replay

- В WPF replay герой с 0 HP на Death's Door больше не затемняется как мёртвый; показывается
  «СМЕРТНЫЙ ПОРОГ».
- `GameEventRecording` пишет `IsDead` только при фактической смерти; старые записи с
  `IsDead = true` при `AtDeathsDoor = true` читаются корректно.
- Env `DARKEST_CAPTURE_RUN` позволяет открыть конкретный `captures/run_*` (например,
  `run_2026-10-21_16-59-45`), не удаляя более новые записи.
- Capture остаётся replay, а не повторной симуляцией: частичная запись может не иметь
  `BattleEnded` и seed.
