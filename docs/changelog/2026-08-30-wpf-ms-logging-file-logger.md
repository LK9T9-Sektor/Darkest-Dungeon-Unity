# Логирование на MS-абстракции + файловый логгер (WPF-клиент)

- **MS-абстракция на границе**: WPF-клиент подключил `Microsoft.Extensions.Logging.Abstractions`
  3.1.12 (чистый netstandard2.0, совместим с Unity 2017.4). Ядро осталось без внешних пакетов:
  структурный `ILogger` (`Core.Common`) принимает любую реализацию через конструкторы.
- **`MsLoggerAdapter`** (WPF) — оборачивает MS `ILogger` (`Log`→`LogInformation`, `Warn`→`LogWarning`).
- **`FileLogger`/`FileLoggerProvider`** (WPF) — запись `[timestamp] LEVEL category message` в
  `Logs\duel.log` рядом с исполняемым файлом. `DuelLobbyViewModel`/`SinglePlayerLobbyViewModel`
  передают логгер в `DuelController`.
- Парсинг боевых эффектов в ядре закрыт: `.kill` (→ `KillEffect`), `.kill_enemy_types`
  (→ `KillEnemyTypeEffect`), `.performer_rank_target`/`.clear_rank_target`, `.disease any`
  (→ `DiseaseEffect(null, true)`). `MarkedForDeath` уже учитывается `DeathCheck`.
