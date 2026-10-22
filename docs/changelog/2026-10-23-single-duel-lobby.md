# WPF: одно одиночное лобби, PvE-пункт убран

- В главном меню больше нет отдельного **PvE**; **VS AI** — единственное одиночное лобби.
- Одиночное лобби переименовано в `DuelLobbyViewModel`/`DuelLobbyView`, мультиплеерное —
  в `MultiplayerLobbyViewModel`/`MultiplayerLobbyView`.
- Удалены `PveLobbyViewModel`, `PveLobbyView`, `PveMonsterSlotViewModel` и `PveBattleViewModel`;
  бой VS AI идёт через общий `DuelBattleViewModel`.
- Монстры по-прежнему выбираются во вкладке `Monsters` таблицы и могут стоять на стороне игрока
  или ИИ (пока только юниты на 1 слот).
