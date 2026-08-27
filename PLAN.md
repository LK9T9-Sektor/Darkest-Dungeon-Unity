# PLAN.md — Квирки → баффы в дуэли (детерминированный локстап)

## Цель

Черты героев, выбранные/переброшенные в лобби, реально влияют на статы в бою дуэли —
как в Unity (permanent-баффы `BuffSourceType.Quirk`). Обе стороны локстапа применяют
одинаковые квирки (передаются в `party_config`), одинаковые баффы из `JsonBuffs.json` →
идентичные атрибуты.

## Факты из Unity (исследовано)

- `AddOrReplaceQuirk` (Unity `Hero`) → `AddBuff(new BuffInfo(quirk.Buffs[i], Permanent, Quirk))`.
- `Quirk.Buffs` — имена баффов; `JsonBuffs.json` даёт `stat_type` add/multiply + `stat_sub_type` + `amount`.
- Unity-дуэль берёт героев из имения → квирки влияют и в кампании, и в дуэли.
- У нас: `HeroGeneration` без квирков, `Quirk.Buffs` — строки, парсера Buff нет, квирки не в `party_config`.

## Шаги

1. [x] **S1** `Core.Content`: модель `Character\BuffContent` (Id, StatType, AttributeTypeName,
   Amount, RuleTypeName, IsFalseRule, RuleFloat, RuleString) + DTO `Database\JsonBuff`/
   `JsonBuffData` + `Database\BuffContentMapper`. Тесты `BuffContentMapperTests` на реальном
   `JsonBuffs.json` (линк в тестовый csproj).
2. [x] **S2** Wpf: линк `JsonBuffs.json` → `Content\Buffs\JsonBuffs.json`; `Data\BuffCatalog.cs` —
   десериализация (Newtonsoft) → `BuffContentMapper` → core `Buff` (Combat) через
   `CharacterHelper`/маппинг `stat_sub_type`→`AttributeType` и `rule_type`→`BuffRule`;
   словарь id→Buff.
3. [x] **S3** Core `Hero`: список `Quirks` (id) + `AddQuirk(string id)` (для отображения).
4. [x] **S4** `DuelHeroPick`/`DuelPartyConfig`: квирки на героя; wire «class|seed|skills|quirks»
   (обратная совместимость: без сегментов = пусто).
5. [x] **S5** `DuelController.AddHero`: назначить квирки (из pick), применить баффы
   `AddBuff(BuffInfo(buff, Permanent, Quirk))`; после баффов HP current = modified (бой стартует полным).
6. [x] **S6** Лобби: `HeroSlotViewModel.SelectedQuirkIds`; `DuelLobby`/`SinglePlayer` передают
   в picks/config; ИИ-отряд получает случайные квирки (детерминированно, локально).
7. [x] **S7** Отображение квирков в листе статов (правый клик) — список из `unit.Character`.
8. [x] **S8** Тесты: Buff-маппер; дуэль — герой с «tough» имеет больше MaxHealth; round-trip
   `DuelPartyConfig` с квирками; лобби-слот отдаёт `SelectedQuirkIds`. Build + тесты WPF/Combat/Content.
9. [x] **S9** Доки: `EXTRACTION_PLAN` (JsonBuffs), `CHANGELOG`, `TESTING`. Коммит и пуш.

## Затрагиваемые файлы

- `src\Core\Sektor.DarkestDungeon.Core.Content\Character\BuffContent.cs` (S1)
- `src\Core\Sektor.DarkestDungeon.Core.Content\Database\JsonBuff.cs`, `JsonBuffData.cs`, `BuffContentMapper.cs` (S1)
- `tests\Core\...\Content.Tests\Database\BuffContentMapperTests.cs` (+csproj link) (S1)
- `src\Wpf\...\Sektor.DarkestDungeon.Wpf.csproj` (link JsonBuffs.json) (S2)
- `src\Wpf\...\Data\BuffCatalog.cs` (S2)
- `src\Core\...\Combat\Character\Hero.cs` (S3)
- `src\Wpf\...\Combat\DuelController.cs`, `Networking\DuelPartyConfig.cs` (S4, S5)
- `src\Wpf\...\ViewModels\HeroSlotViewModel.cs`, `DuelLobbyViewModel.cs`, `SinglePlayerLobbyViewModel.cs` (S6)
- `src\Wpf\...\ViewModels\HeroStatsViewModel.cs` + `Views\HeroStatsView.xaml` (S7)
- `docs\EXTRACTION_PLAN.md`, `docs\CHANGELOG.md`, `docs\TESTING.md` (S9)

Ядро `src\External\` не трогаем. Управление `Buff`/`BuffInfo` уже есть в ядре (`Character.AddBuff`).

## Приёмка

- [ ] Герой с «tough» (+10% MAXHP) в дуэли имеет +10% к максимуму HP.
- [ ] Обе стороны локстапа применяют одинаковые квирки (передаются в `party_config`).
- [ ] Reroll черт в лобби влияет на бой (выбор уходит в конфиг отряда).
- [ ] Квирки видны в листе статов (правый клик).
- [ ] Build 0 ошибок; тесты WPF/Combat/Content зелёные.