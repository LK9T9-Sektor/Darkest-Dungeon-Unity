# Запуск игры без редактора

- Скрипты `tools\unity-build-game.ps1`, `tools\unity-run-game.ps1`, `tools\unity-dev-run.ps1` и точка сборки `BuildGame.Build` (`Assets\Editor\BuildGame.cs`): сборка standalone Windows x64 через Unity batchmode (включает проверку блокировки проекта, доставку плагинов Lan, размещение `steam_appid.txt` рядом с exe) и запуск собранного плеера без открытия редактора. Вывод — `Build\Darkest Dungeon\Darkest Dungeon.exe`.
- `tools\unity-compile-check.ps1` — быстрая проверка компиляции скриптов (включая `Assets\Editor`) в таргетной Unity 2017.4 без сборки плеера: batchmode импорт + компиляция, разбор лога на `error CS`/`Compilation failed`.
