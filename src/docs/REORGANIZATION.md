# REORGANIZATION.md — План реорганизации в монорепо

Целевая структура репозитория: общее чистое C#-ядро (`src\`) + два Unity-проекта (`unity\` — активная версия, `unity-2017\` — легаси). Доменная логика постепенно выносится из презентационного слоя в чистые C#-библиотеки. Подробности по коду — в `ARCHITECTURE.md`, известные проблемы — в `KNOWN_ISSUES.md`.

## 1. Текущее состояние

- `Assets\Scripts` — ~485 MonoBehaviour-файлов; весь домен (бой, кампания, данные) всё ещё в презентационном слое.
- `src\` — только сетевой слой `Lan\` (Contracts/Steam/Cmd), netstandard2.0, C# 7.3; DLL доставляются пост-билдом в `Assets\Plugins\Internal`.
- `tests\Lan\` — NUnit-тесты сетевого слоя.
- Ветки: `master`/`1.0.4` (Unity 2017.3/2017.4), `steam`/`coop` (Unity 2017.4 + Steam co-op), `unity-6.4`/`unity-6.5` (Unity 6000.4/6000.5).

## 2. Целевая раскладка

```
repo/
├── AGENTS.md            # карта-манифест, правила
├── src/                 # чистое C# ядро (общее для ОБОИХ версий)
│   ├── Lan/             # транспорт
│   ├── Core/            # домен: Content, Save, Combat, Campaign…
│   ├── External/        # вендоренный референс (read-only)
│   └── docs/
├── tests/               # NUnit, зеркалит src/
├── unity/               # АКТИВНАЯ версия (Unity 6.5 → скоро 7); имя без версии
├── unity-2017/          # Unity 2017.4 (легаси, активная поддержка)
├── tools/               # с параметром -ProjectPath
└── Darkest-Dungeon-Unity.slnx       # единое решение (Unity-проекты + чистое C# ядро)
```

Ключевые решения:

- **`unity\` без номера версии** — переживёт переход на Unity 7 без переименования.
- **`unity-2017\`** наполняется вручную из ветки `steam` (2017.4.40f1 + Steam co-op), не сидится автоматически.
- **Оба проекта активно поддерживаются**; общий источник истины для домена — `src\Core\`.
- Модуль = папка = namespace; god-классы не плодить. Легаси-файлы не удаляются, пока ядро не станет источником истины (минимальный дифф).

## 3. Фазы

- **Фаза 0. Реструктуризация в монорепо.** *(в работе)* Перенос активного проекта в `unity\`; `.gitignore` на обе раскладки; тулзы `-ProjectPath`; доставка ядра в оба проекта; compile-check обеих версий.
- **Фаза 1. Данные** → `src\Core\Content`: модели контента + парсеры (JSON/DSL/CSV/XML) из `DarkestDatabase`; `InvariantCulture`; `DarkestDatabase` → тонкий загрузчик; тесты на реальных данных.
- **Фаза 2. Сейвы/кампания** → `src\Core\Save`: DTO + бинарный кодек + стартовая кампания.
- **Фаза 3. Бой** → `src\Core\Combat`: `BattleGround`/`Round`/`BattleSolver`/Effects/AI (детерминизм мультиплеера).
- **Фаза 4. Город/рейд-флоу** → `src\Core\Campaign`.
- **Фаза 5. Презентация** — только тонкие MonoBehaviour-адаптеры + UI.

Каждый этап: извлечь → тесты → адаптеры в обоих проектах → compile-check обеих версий → доки → коммит.

## 4. `.gitignore` под новую раскладку

Почти все правила уже «глубинные» (матчатся на любой глубине) и покроют и `unity\`, и `unity-2017\`. Требуют правки (снятие якоря на корень):

- `/[Uu]serSettings/` → `[Uu]serSettings/`
- `!/[Aa]ssets/**/*.meta` → `![Aa]ssets/**/*.meta`
- `/[Aa]ssets/AssetStoreTools*` → `[Aa]ssets/AssetStoreTools*`
- + `UnityPackageManager/`

Проверка — матрицей `git check-ignore`: игнорируются `Library`, `Temp`, `obj`, `Logs`, `Build`, `UserSettings`, `Assets/Audio`, `Assets/Sprites`, `Assets/StreamingAssets`, `Assets/Plugins/Internal`, `steam_appid.txt`, `*.log`; отслеживаются исходники `Assets/Scripts`, `ProjectSettings`, `Assets/Plugins/x86_64/steam_api64.dll`.

## 5. Инструменты

- `tools\*.ps1` получают параметр `-ProjectPath` (по умолчанию `unity\`): compile-check, сборка, запуск, провижнинг.
- `provision-unity-plugins.ps1` собирает ядро один раз (точечно: Steam-проект тянет Contracts) и доставляет DLL/PDB в `<project>\Assets\Plugins\Internal` обоих проектов; фасады .NET Standard — только для 2017-редактора; `steam_api64.dll` — в `<project>\Assets\Plugins\x86_64`.
- Единое решение `Darkest-Dungeon-Unity.slnx` содержит Unity-проекты и чистое C# ядро. `dotnet build` по решению не выполняется (Unity-проекты требуют редактор) — сборка ядра точечная по проектам `src\`.

## 6. Снижение нагрузки на чтение для агентов

- Карта-манифест в `AGENTS.md`: домен — `src\Core\<модуль>`, презентация — `<проект>\Assets\Scripts\<область>`.
- Модуль = папка = namespace → поиск через glob/grep предсказуем.
- Документы: `ARCHITECTURE.md` (структура), `KNOWN_ISSUES.md` (долг), `CHANGELOG.md` (изменения), `UNITY_MIGRATION.md` (переходы редактора).
