# networking_transport.md — Networking (src\Lan): Contracts/Steam/Photon

> Домен: `networking` (транспорт, вне домена). Статус: **частично** (см. `NETWORK.md`).

## 1. Назначение и когда работает

Транспортный слой для сети: контракты (каналы/события), Steam (P2P), Photon (будущее). Ядро доменов
**не знает о транспорте** (DAG без восходящих, `ARCHITECTURE.md` §4). Детальная спецификация — в
`NETWORK.md`/`NETWORK_LAYER_REUSE.md`/`NETWORK_RATIONALE.md`.

## 2. Модель данных

- `src/Lan/` — `Sektor.DarkestDungeon.Lan.Contracts`, `.Lan.Steam`, `.Lan.Cmd`.
- Каналы/события транспорта — в Contracts.
- `Result` (переезд в `Core.Common`) — возвраты операций.

## 3. Порядок срабатывания (трассировка)

Подробности — `NETWORK.md` §«Ответственность транспорта/фасада», `NETWORK_LAYER_REUSE.md`.
Общая схема: клиентская фасад-абстракция → транспорт (Steam/Photon) → wire (байтовый, `NETWORK_RATIONALE.md`).

## 4. Очередь и обновления

- Отправка/приём каналов — событийная модель транспорта.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Домен не знает транспорт | `ARCHITECTURE.md` §4 | запрет ссылок `Core → Lan` |

## 6. Нюансы и подводные камни

- **`src\Lan` → `src\Networking`** — планируемое переименование (`TARGET_LAYOUT.md` §3.4).
- Photon — будущее (`PLAN.md` Фаза 5); Steam — текущий транспорт WPF.

## 7. Взаимодействия

- WPF-клиент: `AiRivalLink`/`NetworkRivalLink` (тонкие обёртки над `DuelController`).
- Сид сессии из id игроков (`duel_03_seed.md`, `NETWORK.md` §6).

## 8. Файлы-источники

- `src/Lan/*`
- `docs/NETWORK.md`, `NETWORK_LAYER_REUSE.md`, `NETWORK_RATIONALE.md`