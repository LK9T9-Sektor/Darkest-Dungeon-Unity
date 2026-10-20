# Unity/WPF: контракт событий 1.1 — формация, стресс, статусы и аудио

- **Боевой захват**: `stateSnapshot` теперь содержит `Rank`/`Size`, стресс, статусы/баффы,
  death's door/afflicted/virtued и состояние боевого цикла (`Round`, `Turn`, порядок ходов).
  Добавлены `skillResolved` по всем целям, `unitMoved`, `turnStarted`/`turnEnded`,
  `roundStarted`/`roundEnded` и `battleEnded`.
- **Аудио**: записывается `audioPlayed` с фактическим FMOD-путём, kind/action, громкостью,
  3D-позицией и параметрами; появился `audio_session.jsonl`. Каталог FMOD (2007 событий,
  40 банков, 20 music events) лежит в `docs\audio`.
- **WPF-реплей**: показывает ранги/размер, стресс, статусы/баффы, раунд/ход, победителя боя и
  журнал аудио; `skillResolved`/`unitMoved` меняют HP/стресс и перестраивают строй. Захваты
  `1.0.0` по-прежнему проигрываются (fallback-ранги).
- **Ограничения среза**: WPF пока не воспроизводит FMOD-звук и не строит карту; `skillResolved`
  содержит RNG только первой цели (hit/crit; `Seed` не пишется), у нарратива — только `Modifier`.
