# save_binary.md — Save: IBinarySaveData

> Домен: `save` (ядро `Core.Save`). Статус: **данные** (кодек/`ISaveStorage` — будущее, Фаза 2).

## 1. Назначение и когда работает

Базовый интерфейс бинарной сериализации сейв-данных. Сейчас — только контракт; полный кодек и
хранилище (`ISaveStorage`) — будущая задача (`PLAN.md` Фаза 2, `EXTRACTION_PLAN.md`).

## 2. Модель данных

`IBinarySaveData` (`Core.Save/IBinarySaveData.cs:9`):
- `IsMeetingSaveCriteria` (`:12`) — достаточно ли данных для сохранения;
- `Write(BinaryWriter bw)` (`:16`);
- `Read(BinaryReader br)` (`:20`).

## 3. Порядок срабатывания (трассировка)

Не применяется в бою. Реализации (`SaveHeroData`, `SaveCampaignData`, `SaveActivitySlot`,
`SaveEventData` в legacy Unity `unity\Assets\Scripts\Setup\SaveSystem\`) пишут/читают бинарные поля.

## 4. Очередь и обновления

- Нет (модель/контракт).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Готовность | `IsMeetingSaveCriteria` | зависит от реализации |

## 6. Нюансы и подводные камни

- **Не реализовано**: бинарный кодек, версии, `ISaveStorage`; вынос логики из `SaveLoadManager` —
  Фаза 2. Не добавлять боевую логику в `Core.Save`.
- Порядок `Write`/`Read` должен совпадать (версии формата — будущее).

## 7. Взаимодействия

- Legacy: `unity\Assets\Scripts\Setup\SaveSystem\SaveLoadManager.cs` (пока владеет сериализацией).

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Save/IBinarySaveData.cs`
- `docs/EXTRACTION_PLAN.md` (Фаза 2), `docs/UNITY_LEGACY_MAP.md` (SaveSystem)