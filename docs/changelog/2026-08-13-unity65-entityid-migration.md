# Unity 6.5 (6000.5.8f1): EntityId-миграция

- Редактор обновлён 6000.4.5f1 → 6000.5.8f1: устаревшие API стали ошибками компиляции (CS0619): `GetInstanceID()`, `EditorApplication.hierarchyWindowItemOnGUI`, `EditorUtility.InstanceIDToObject(int)`.
- Свои скрипты переведены на `EntityId` (`Object.GetEntityId()`): RNG-сиды — через `GetEntityId().GetHashCode()` (`DarkestPhotonLauncher`, `MultiplayerPartyPanel`, `RoomSelector`).
- Spine: ключи атласных таблиц и сравнение материалов на `EntityId` (`SpriteAttacher`, `SkeletonRenderer`, `SpineEditorUtilities`); событие иерархии — `hierarchyWindowItemByEntityIdOnGUI`.
- FMOD: `EventBrowser` переведён на `hierarchyWindowItemByEntityIdOnGUI`/`EntityIdToObject` (vendored, минимальный дифф).
- Инструменты `tools\*.ps1` находят редактор 6000.5.8f1; доставка .NET Standard фасадов пропущена — на Unity 6 они не нужны (нативный type-forwarding).
