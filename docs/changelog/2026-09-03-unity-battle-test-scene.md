# Unity BattleTest: сцена гибкого боя на чистом ядре (первый потребитель ядра в Unity)

- **Новая сцена `Assets\Scenes\BattleTest.unity`** (в `unity\` и `unity-2017\`): настраиваемый бой
  (герои/монстры по слотам, режимы PLAYER/AI · AI vs AI · HOTSEAT, seed, torch) на чистом ядре
  (`Core.Combat`/`Core.Duel`, `DuelController`) с легаси-префабами юнитов (Spine). Unity — тонкий
  вид-слой: `CoreBattleDriver`, формации, HP/стресс, попапы, экран боя. Сцена пересобирается
  `tools\unity-generate-battle-test-scene.ps1`; легаси-движок (`RaidSceneManager`) не используется
  и не изменён.
- **Ядро PvE-сверка**: sweep-тест по всем монстрам кампании (`FightPveSweepTests`) — 225 монстров
  добиваются боя без исключений и детерминированно. Закрыт краш `.disease` (NRE при пустом пуле
  болезней); конкретные id болезней (`the_worries`, `rabies`) теперь парсятся и применяются
  (`IBattleContext.GetQuirk` → `Hero.AddQuirk`). Рандомный пул болезней — стаб.
