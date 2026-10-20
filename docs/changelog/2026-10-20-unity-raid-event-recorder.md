# Unity: авто-запись событий рейда в JSONL + эталонный захват

- **Каждый рейд в `unity\` записывается автоматически** (без клавиш): старт при загрузке сцены с
  `RaidSceneManager`, стоп при выходе в меню или остановке Play. Файлы —
  `Replays\run_<ts>\manifest.json` + `<subsystem>_session.jsonl` (`map`/`combat`/`torch`); JSONL
  пишется с `AutoFlush`, поэтому аварийный выход теряет максимум одну строку.
- **События v1** (`docs\SPEC_GAME_EVENTS_SCHEMA.md`): `sceneSnapshot`, `partyMoved`,
  `roomDiscovered`, `stateSnapshot` (`battleStart`), `attackExecuted` (фактическое имя анимации,
  включая fallback на клип Mecanim-аниматора), `heroStatChanged`, `torchChanged`.
- **Эталонный прогон** — `docs\spec_game_events\examples\` (2026-10-20, сцена `Dungeon`, 38 событий,
  `Sequence` 1..38 без дыр): комната `room2_1`, 6 атак (`attack_slice`, `attack_uppercut`,
  `attack_stun`, `attack_pistol`, `attack_scroll`), 5 изменений HP, 16 изменений факела. Служит
  исполняемой спецификацией для WPF-потребителя JSONL.
- Проверка: `pwsh tools\unity-compile-check.ps1 -ProjectPath unity` — компиляция и script-reference
  зелёные.
