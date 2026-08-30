# 00_index.md — Механики: навигатор по спецификациям

> Детальные спецификации всех механик по доменам (зеркало `TARGET_LAYOUT.md`). Каждый документ —
> по единому шаблону: назначение/когда работает → модель данных → парсинг контента → порядок
> срабатывания (`file:line`) → очередь/обновления → проверки/клэмпы → **нюансы/подводные камни** →
> взаимодействия → файлы-источники. Правило обязательного расписывания — в `AGENTS.md` §«Документация».
> Обновлять статусы в этом навигаторе при закрытии/открытии разрывов `BATTLE_PARITY.md`.

Легенда статуса: **реализовано** (работает в ядре/дуэли) · **частично** (неполно) · **данные**
(модели/DTO без поведения, поведение — Фаза 4) · **стаб** (нет ни в ядре, ни в Unity).

## Бой (`combat/`)

| # | Механика | Файл | Статус |
|---|---|---|---|
| 01 | Урон / хил / крит / меткость | `01_damage.md` | реализовано |
| 02 | DoT (bleed / poison): наложение, тик, истечение | `02_dot.md` | реализовано |
| 03 | Stun: пропуск хода, `STUNRECOVERYBUFF`, истечение | `03_stun.md` | реализовано |
| 04 | Riposte: статус, контратака, `riposte_skill` | `04_riposte.md` | реализовано |
| 05 | Guard: статусы, редирект атак, clearguard | `05_guard.md` | реализовано |
| 06 | Mark/Tag: наложение, истечение, `buff_duration_type` | `06_mark.md` | частично |
| 07 | Pull / Push / Shuffle: перемещение рангов | `07_rank_move.md` | реализовано |
| 08 | Immobilize: блок self-move и ручного move | `08_immobilize.md` | реализовано |
| 09 | Buff-система: стат-баффы, `.buff_ids`, RemoveConditions | `09_buffs.md` | реализовано |
| 10 | Torch: `.torch_decrease/increase`, клэмп 0–100 | `10_torch.md` | реализовано |
| 11 | Modes (Абоминация): human/beast, continue-turn | `11_modes.md` | реализовано |
| 12 | Surprise 1-го раунда: шанс, -100 инициативы, shuffle | `12_surprise.md` | реализовано |
| 13 | Инициатива / порядок хода / per-turn обновления | `13_turn_order.md` | реализовано |
| 14 | Смерть, death's door, стресс отряда, resolve, heart attack | `14_death_stress.md` | реализовано |

## Домены вне боя

| Домен | Содержание | Файл/папка | Статус |
|---|---|---|---|
| Duel | lockstep, `DuelSeed`/`DuelPayload`, `DuelPhase`, `DuelAi`, `FightSession` | `duel/` | реализовано |
| Content | квирки, бафф-контент, trinket/camping-каталоги | `content/` | данные |
| Common | `Result`, `RandomSolver`/`IRng`, токен-парсер, feature-flag | `common/` | реализовано |
| Clients | `GameDataReader` (Newtonsoft-фасад) | `clients/` | реализовано |
| Save | `IBinarySaveData` (кодек/`ISaveStorage` — будущее) | `save/` | данные |
| Networking | Contracts/Steam/Photon (src\Lan) | `networking/` | частично |
| Presentation | WPF-экраны, Unity-оверлеи Тест-боя | `presentation/` | реализовано |
| Campaign | имение/здания/апгрейды/квесты/город/ростер/провизия | `campaign/` | данные |
| Raid | подземелья/энкаунтеры/боссы/curio/loot/пропы | `raid/` | данные |

## Связанные доки

- `GAME_RULES.md` — сводная таблица «как в репо vs оригинал DD» (ссылки на детали здесь).
- `BATTLE_PARITY.md` — разрывы Unity MP vs ядро (статусы согласованы с навигатором).
- `INDEX.md` — карта всех документов.