# CLEANUP.md — Каталог уборки кода ядра (`Core.*`)

> Правило: **ядро чистится**, legacy Unity (`unity\`, `unity-2017\`) и `src\External\` — нет
> (read-only до cutover). Уборка не меняет поведение: охранник — тесты (`dotnet test`), lockstep.
> Прогресс — в `PLAN.md` (задача «уборка кода ядра»), статусы — ниже. Каталог ведётся в том же
> коммите, что и изменения.

## 1. Принципы уборки (из AGENTS.md)

- **Константы вместо магических строк/чисел** — контент-id эффектов/баффов, клэмпы, длительности,
  шансы (named-константы в `Core.Combat/Content/`).
- **Полиморфизм вместо ветвлений** — switch/if по `BuffRule`, строковые диспетчеры, парсеры ключей
  DSL (OCP, реестры-словари).
- **Тестируемые классы вместо приватных методов god-классов** — вынос логики из `DuelController`,
  `BattleSolver`, `Character` в классы с конструкторным DI.
- **Структурное логирование** — `ILogger` в `Core.Common`, DI; без синглтонов и статик-состояния.
- **Один публичный тип на файл**, `using` сверху, XML-документация (AGENTS.md).

## 2. Карта проблем (по модулям)

| Модуль | Проблема | file:line (пример) | Приоритет |
|---|---|---|---|
| `Core.Duel` | god-класс `DuelController` (719 стр.), приватные методы (`CheckSurprise`, `ApplyDotTicks`, `StressParty`, `TryMove`, `RemoveConditions`), магические id эффектов/баффов (`Stress 2`, `STUNRECOVERYBUFF`, `AfflictedAllyStress`) | `DuelController.cs:210,369,673,696` | P0 |
| `Core.Combat` | god-класс `BattleSolver` (616 стр.), магические строки эффектов (`Stress 2`, `crit_heal_stress_heal`), числа (0.95, 0.1) | `BattleSolver.cs:451,486,496` | P0 |
| `Core.Combat` | god-класс `Character` (584 стр.), switch `ApplyBuffRule` (~90 строк по `BuffRule`), `StringToStatusType`/`StringToMonsterType` | `Character.cs:432-528,546-574` | P1 |
| `Core.Combat` | `EffectCatalog.ParseEffect` — if-цепочка ключей (~140 строк) без реестра | `EffectCatalog.cs:66-205` | P1 |
| `Core.Combat` | AI-desires — switch по строкам-ключам (`base_chance`, `marked_heroes_min`, ...) | `SkillSelectionDesire.cs:169-211`, `TargetSelection*.cs` | P2 |
| `Core.Combat` | `HeroClassFileParser`/`MonsterClassFileParser` — большие парсеры с if-цепочками секций | `HeroClassFileParser.cs:68-112` | P2 |
| `Core.Combat` | Магические числа: шансы, клэмпы, длительности по умолчанию (0.95, 0.65, 0.25, 0.6, 3, 1, 75) | `EffectCatalog.cs`, `DuelController.cs`, эффекты | P1 |
| `Core.Duel` | `DuelBattleEvents.Log` — строки-теги `[popup]`/`[pull]` без структуры | `DuelBattleEvents.cs:23-157` | P2 |

## 3. Статус по фазам (`PLAN.md`)

| Фаза | Содержание | Статус |
|---|---|---|
| C0 | Этот каталог + правило в `AGENTS.md` | [x] |
| C1 | Константы: `EffectIds`/`BuffIds`/`BattleConstants`/`ChanceMath` | [x] |
| C2 | Полиморфизм: `BuffRuleEvaluator`, `EffectParser`, реестр `MapTarget` | [x] |
| C3 | Вынос: `SurpriseResolver`/`DotTickApplier`/`StunRecoveryApplier`/`DeathCheck`/`TurnMover` (Duel), `DamageResolver`/`HealResolver` (Combat) | [x] |
| C4 | `ILogger`/`NullLogger` + логирование `DuelController` (ходы/скиллы/стан) | [x] |
| C5 | Тесты/build + синхронизация `docs/mechanics/*` | [x] |
| C6 | MS-абстракция на границе: WPF подключает `Microsoft.Extensions.Logging.Abstractions 3.1.12`, `MsLoggerAdapter`/`FileLogger`/`FileLoggerProvider` (запись в `Logs\duel.log`); ядро без внешних пакетов | [x] |

## 3b. Осталось (отложено/будущее)

- AI-desires (`SkillSelectionDesire`/`TargetSelection*`) — switch по строкам-ключам → реестр
  (P2, `CLEANUP.md` §2). Не трогается в этой итерации — риски для `JsonBrainParser`.
- `HeroClassFileParser`/`MonsterClassFileParser` — большие секционные парсеры (P2).
- `DuelBattleEvents.Log` — строки-теги → структурированные записи (P2, затронет WPF-UI).
- `docs/mechanics/*` — ссылки на вынесенные классы актуализированы (номера в `file:line`).

## 4. Правило сопровождения

- Новая логика в ядре — без магических строк/чисел, без switch-диспетчеров, с DI и XML-доками.
- Магическую константу добавлять в именованный каталог, а не в место вызова.
- Изменение `file:line` в `docs/mechanics/*` — обновлять в том же коммите.