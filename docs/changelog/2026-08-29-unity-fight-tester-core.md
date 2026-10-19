# Тест-бой (FightTester) на чистом ядре в обоих Unity-клиентах

- **Стенд Тест-боя в TEST-меню** (`unity\` и `unity-2017\`): выбор 2×4 слотов
  (`пусто → герой → монстр` по имени), seed, режимы «ИИ vs ИИ» и «Игрок/ИИ», кнопка FIGHT —
  бой героев и монстров кампании на чистом ядре поверх TEST-меню.
- **Данные кампании в ядро** — новый модуль `Core.Data`: DTO всех JSON-бандлов +
  `GameDataReader`, каталоги баффов/квирков/мозгов (`BuffCatalog`, `QuirkCatalog`,
  `MonsterBrainCatalog`); `MonsterClassFileParser` + `MonsterCatalog` (M1: `Data/Monsters/*.txt`,
  статы/`enemy_type`/резолв эффектов/`battle_modifier`) и `JsonBrainParser` (M2: `Data/JsonAI.json`).
- **FightSession** (`Core.Duel\Fight\`): автоматический прогон боя герои vs монстры до конца
  (герои — через default brain, монстры — кампанийные мозги), детерминизм по сиду; в режиме
  «Игрок/ИИ» героями ходит игрок (скилл → цель, PASS/AUTO), бой паркуется на ожидании ввода.
- WPF-клиент переведён на те же каталоги ядра (квирки/баффы/монстры/мозги через
  `GameDataReader`), чтобы два клиента и ядро читали одни данные.
