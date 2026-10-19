> Закрыт: 2026-10-18 (ветка `core/unity7-urp`, ввод архива планов). Индекс закрытых планов —
> `docs\plans\00_index.md`.

# PLAN: Хранение планов — активный `PLAN.md` + архив `docs\plans\`

## Что сделано

- `PLAN.md` разнесён на секции: активная URP-миграция осталась, 4 завершённых плана перенесены в `docs\plans\` (463 строки = 445 перенос + 18 структурных, потерь нет).
- Создан `docs\plans\00_index.md` — индекс архива + правило пополнения; ссылки обновлены в `docs\INDEX.md`, репозиторном `AGENTS.md` и глобальном `~/.config/opencode/AGENTS.md`.
- Удалён `.opencode\plans\scene-snapshot-json-dump.md` (был untracked).
- Схема имён позже расширена: файлы называются `<дата>-<слаг>-closed.md` (см. `00_index.md`).

---

_Ниже — исходный план без изменений._

## Goal

Планы задач не должны теряться при переходе на другой ПК и должны находиться агентом по регламенту
(`AGENTS.md` → `docs\INDEX.md`). Решение (утверждено пользователем): активный план — корневой
`PLAN.md` (глобальное правило и ~20 ссылок в `docs\` не ломаются); завершённые планы — в
`docs\plans\` (в git; один файл на план; индекс `docs\plans\00_index.md`). `.opencode\` остаётся
служебным каталогом — планы там не хранятся (untracked = потеря при смене ПК).

## Design decisions (user-confirmed)

- Хранилище архива — `docs\plans\` (коммитится, попадает в `docs\INDEX.md`), а не `.opencode\plans`.
- `PLAN.md` — файл активного плана; при завершении план переносится в `docs\plans\<slice>.md` целиком.
- Старый локальный план `.opencode\plans\scene-snapshot-json-dump.md` (мёртвый: все шаги пустые,
  дизайн перекрыт реализованным дампером `SceneSnapshotDumper`) — удалить.

## Steps / status

1. [x] Разнести `PLAN.md` (6 секций): активная URP-миграция осталась; 4 завершённых плана — в
   `docs\plans\`: `unity7-urp-diagnostics`, `battle-test-scene`, `wpf-size-aware-ranks`,
   `wpf-duel-formation-binding` (границы секций проверены ассертами скрипта; потеря строк
   исключена: 463 = 445 + 18 структурных).
2. [x] `docs\plans\00_index.md` — таблица «план | ветка | статус | коммиты | файл» + правило
   пополнения архива.
3. [x] Доки-ссылки: `docs\INDEX.md` (секция «Исполнение»), репозиторный `AGENTS.md` (список
   обязательного чтения), глобальный `~/.config/opencode/AGENTS.md` (Plan persistence — архив).
4. [x] Удалён `.opencode\plans\scene-snapshot-json-dump.md` (+ пустой каталог; каталог был
   untracked — git не затронут).
5. [x] Архив этого плана — `docs\plans\plans-storage.md`; `PLAN.md` — шапка + активная URP-миграция.
6. [x] Коммит в `core/unity7-urp` (docs + план + AGENTS.md).

## Affected files

- `PLAN.md`; `docs\plans\*.md` (новые); `docs\INDEX.md`; `AGENTS.md` (репо);
  `C:\Users\lk9t9\.config\opencode\AGENTS.md` (вне репо).

## Acceptance criteria

- После `git clone` на другом ПК: `AGENTS.md` → `PLAN.md` → `docs\plans\00_index.md` — всё в git.
- Ссылки на `PLAN.md` в `docs\` не ломаются (файл на месте).
- `.opencode\plans\` отсутствует.

## Cleanup (временные артефакты)

- `.opencode\plans\scene-snapshot-json-dump.md` (6.8 КБ) — удалён в этом плане.
- Временный скрипт разрезания (`%TEMP%\opencode\split-plan.ps1`) — удаляется после коммита.
