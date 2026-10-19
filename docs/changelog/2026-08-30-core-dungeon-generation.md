# Генерация подземелья в ядро (`Core.Raid\Generation`)

- **`DungeonGenerator` → ядро**: чистая детерминированная генерация (топология комнат/коридоров,
  население типами боев/curio/ловушек/голода, environment-пропы и энкаунтеры) — из
  `MapGenerator.txt` + `Dungeons/*.bytes`. `IRng`/`SystemRandomRng` в `Core.Common`.
- **Quest-цели в ядро**: `DungeonQuestGoal` + `ApplyQuestGoal` (kill_monster → boss-комната +
  энкаунтер; activate/gather → curio-комнаты по `MinPath`); Unity-адаптер конвертит `quest.Goal`.
- **Unity-адаптер** `DungeonGenerator.cs` — тонкая обёртка (ядро + маппинг в Unity-модели);
  старый код — `DungeonGeneratorLegacy.cs` до ручной проверки.
- Парсеры DSL (`DungeonGenerationDataParser`/`DungeonEnviromentDataParser`) + фасады
  `GameDataReader.ReadDungeonGenerationData`/`ReadDungeonEnviromentData`.
