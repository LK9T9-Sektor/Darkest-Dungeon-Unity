# Спецификация: система записи игровых событий для последующей реализации на WPF

## Назначение

Документ описывает пайплайн записи событий игры (на базе чистых C#-библиотек
из Reinisch/Darkest-Dungeon-Unity) в структурированный JSON. Цель — не реплей
1 в 1, а исполняемая спецификация: другой ИИ-агент читает логи + schema.md и
реализует ту же логику на WPF без догадок.

Источник истины — три вещи, которые должны совпадать:
1. DTO-модели событий в Core-библиотеке (C#).
2. schema.md — текстовое описание каждого события (когда возникает, поля, инварианты).
3. Логи (*.jsonl) — эталонные примеры.

---

## 1. Архитектура

### 1.1. Event Bus в Core-библиотеке

Не зависит от Unity и WPF. Все игровые системы (бой, карта, инвентарь, факел)
публикуют события в шину. Отдельный JsonEventLogger подписывается и пишет на диск.

    public interface IGameEventBus
    {
        void Publish<T>(T gameEvent) where T : GameEvent;
        IDisposable Subscribe<T>(Action<T> handler) where T : GameEvent;
    }

### 1.2. Полиморфные DTO

Использовать System.Text.Json с атрибутами (рекомендуется) либо Newtonsoft.Json
с TypeNameHandling.Auto (уже знаком проекту Reinisch).

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(SceneSnapshot), "sceneSnapshot")]
    [JsonDerivedType(typeof(StateSnapshot), "stateSnapshot")]
    [JsonDerivedType(typeof(AttackExecuted), "attackExecuted")]
    [JsonDerivedType(typeof(TorchChanged), "torchChanged")]
    [JsonDerivedType(typeof(ActionRejected), "actionRejected")]
    // ... и т.д.
    public abstract record GameEvent(
        Guid EventId,
        DateTime Timestamp,   // UTC
        long Sequence,        // монотонный счётчик
        string Subsystem      // "combat" | "map" | "inventory" | "torch" | "ai"
    );

Обязательные поля каждого события:
- EventId (Guid) — уникальность;
- Timestamp (UTC) — синхронизация;
- Sequence (long) — точный порядок, даже при одинаковых метках;
- Subsystem — подсказка агенту, куда отнести логику;
- Input — что привело к событию (см. §3.1);
- Rng — вероятностные броски (см. §3.2).

### 1.3. Формат хранения — JSON Lines

Одна строка = одно событие. Не единый JSON-массив: потоковая запись,
устойчивость к падению, построчное чтение при реплее и анализе.

---

## 2. Структура файлов

    /replays/run_<YYYY_MM_DD_HHMMSS>/
      manifest.json              # версия схемы, seed, список файлов, дата
      scene_dungeon.json         # полный снимок сцены + иерархия + параметры
      combat_01.jsonl            # бой 1: input -> events -> snapshot
      combat_02.jsonl
      map_session.jsonl          # перемещение, комнаты
      inventory_session.jsonl    # открытие, использование, перемещение предметов
      town_session.jsonl         # город

manifest.json — обязателен:

    {
      "schemaVersion": "1.0.0",
      "gameVersion": "…",
      "seed": 12345,
      "startedAt": "2026-10-19T12:00:00Z",
      "files": ["scene_dungeon.json", "combat_01.jsonl", "map_session.jsonl"],
      "notes": "сценарий: первый данж, party из 4, победа"
    }

schemaVersion критичен: WPF-реализация пишется параллельно, при изменении
формата агент должен понимать, что старые и новые логи несовместимы.

---

## 3. Обязательные поля событий (сверх базовых)

### 3.1. input — причинность

Что игрок или AI сделал, что привело к событию. Без этого агент не поймёт,
например, что клик по врагу с выбранным навыком -> атака, а не наоборот.

    "input": {
      "kind": "playerClick" | "aiDecision" | "tick" | "system",
      "target": "skeleton_02",
      "skill": "smite",
      "slot": "party_1"
    }

### 3.2. rng — вероятности

Сид и результат броска. Без этого агент не отличит вероятностное поведение
от детерминированного.

    "rng": { "seed": 12345, "roll": 0.03, "crit": true, "modifier": 0.15 }

Одна записанная пара roll + crit с известным modifier даёт агенту формулу
шанса крита.

---

## 4. Типы событий (минимальный набор)

### 4.1. Снимки (snapshots)

- sceneSnapshot — один раз при загрузке уровня.
- stateSnapshot — периодически: в начале боя, после каждого хода,
  при смене комнаты, при открытии инвентаря.

Правило: каждое событие должно быть применимо к ближайшему предшествующему
снимку без знания всей истории. Снимок самодостаточен.

Структура sceneSnapshot:

    {
      "type": "sceneSnapshot",
      "hierarchy": [
        { "id": "dungeon_root", "parent": null, "components": ["DungeonRenderer"] },
        { "id": "hero_1", "parent": "party_slot_1", "components": ["HeroView", "SpineAnimation"] }
      ],
      "globalParams": { "torch": 100, "gold": 500 },
      "heroes": [
        { "id": "vestal_01", "name": "Vestal", "hp": 42, "maxHp": 42,
          "stats": { ... }, "icon": "vestal_portrait", "skeleton": "vestal" }
      ],
      "party": ["vestal_01", "crusader_01", "..."],
      "inventory": [ { "itemId": "bandage", "slot": 0, "count": 3 } ]
    }

Иерархия — плоский список с parent, не вложенный JSON. Проще восстанавливать
в WPF через ObservableCollection + DataTemplate.

Ассеты — логические имена ("vestal_portrait", "skeleton"), а не пути к файлам.
WPF-клиент сам сопоставит с ресурсом.

### 4.2. Бой

attackExecuted — ключевое событие для визуального воспроизведения:

    {
      "type": "attackExecuted",
      "attackerId": "crusader_01",
      "targetId": "skeleton_02",
      "skillName": "smite",
      "attackerAnimation": { "skeleton": "crusader", "animation": "attack_smite", "track": 0, "loop": false },
      "targetAnimation":   { "skeleton": "skeleton",  "animation": "hit_reaction",   "track": 0, "loop": false }
    }

heroStatChanged — атомарное изменение стата:

    {
      "type": "heroStatChanged",
      "heroId": "vestal_01",
      "stat": "hp",
      "oldValue": 42,
      "newValue": 38,
      "source": "attack",
      "invariant": "newValue >= 0"
    }

spineEventTriggered — если в Spine-анимации есть кадровые события
(например, «удар нанёсся»), писать их отдельными записями для синхронизации
урона и визуала.

skillUnavailable — негативный случай (см. §5).

### 4.3. Карта

Записывать перемещение и изменение комнат, не полную сетку каждый ход:

    { "type": "partyMoved", "fromRoom": "corridor_3", "toRoom": "room_4" }
    { "type": "roomDiscovered", "roomId": "room_5", "type": "treasure" }

### 4.4. Инвентарь

Только дельты: ItemAdded, ItemRemoved, ItemUsed, ItemMoved, ItemEquipped.

    { "type": "itemUsed", "itemId": "bandage", "target": "crusader_01", "consumed": true, "healAmount": 5 }

### 4.5. Факел и глобальные параметры

    { "type": "torchChanged", "oldValue": 65, "newValue": 55, "reason": "corridorStep" }

---

## 5. Отрицательные события — обязательны

Это тест-кейсы для WPF-агента. Без них он реализует фичу без проверок и
получит баг, незаметный в логе.

    { "type": "actionRejected", "input": {"kind": "useItem", "item": "bandage", "target": "crusader_01"},
      "reason": "targetFullHp" }

    { "type": "skillUnavailable", "skill": "smite", "reason": "cooldown", "cooldownRemaining": 2 }

    { "type": "moveBlocked", "from": "room_4", "to": "room_6", "reason": "doorLocked" }

---

## 6. schema.md — рядом с логами

Лог — это пример, а не описание. По 50 записям attackExecuted агент может
не вывести общее правило (например, что атака требует цели в зоне досягаемости).
Поэтому для каждого типа события в schema.md указать:

- когда возникает (триггер);
- обязательные поля и их типы;
- инварианты (например, newValue >= 0, targetId != attackerId);
- пример записи (ссылка на конкретную строку в логе).

Источник истины — DTO в C#. schema.md генерируется из них вручную или
полуавтоматически. Меняется DTO — меняется schema.md — агент видит изменение.

---

## 7. Пайплайн для ИИ-агента (порядок работ)

1. Core-библиотека: определить GameEvent (абстрактная запись) + конкретные
   типы из §4.
2. Event Bus: IGameEventBus + подписка в каждой игровой системе
   (бой, инвентарь, карта, факел).
3. JsonEventLogger: System.Text.Json + полиморфные атрибуты -> JSONL.
   Отдельный StreamWriter на подсистему.
4. При загрузке уровня — публиковать sceneSnapshot (иерархия, статы,
   ассеты, инвентарь, глобальные параметры).
5. Во время игры — публиковать дельты + input + rng + негативные события.
6. stateSnapshot — в начале боя, после хода, при смене комнаты,
   при открытии инвентаря.
7. schema.md — рядом с логами. Обновлять при изменении DTO.
8. WPF-клиент: читает JSONL построчно, восстанавливает сцену по sceneSnapshot,
   применяет события по Sequence. Spine через spine-csharp
   (AnimationState.SetAnimation), синхронизация по SpineEventTriggered.
   Рендер через SkiaSharp / WriteableBitmap.

---

## 8. Чек-лист «не гадать»

- [ ] Каждое событие содержит input и rng (или явный null с причиной).
- [ ] Каждое событие применимо к ближайшему stateSnapshot без знания истории.
- [ ] Каждое событие имеет негативный аналог (actionRejected, skillUnavailable, moveBlocked).
- [ ] Все ассеты — логические имена, не пути.
- [ ] Иерархия — плоский список с parent.
- [ ] manifest.json содержит schemaVersion, seed, gameVersion.
- [ ] schema.md покрывает 100% типов событий с инвариантами.
- [ ] DTO в C# = schema.md = структура логов.
- [ ] Отрицательные события покрывают: полный HP, кулдаун, блокировку двери,
      нехватку предмета, отсутствие цели в зоне.
- [ ] Записаны примеры всех типов Spine-анимаций (атака, hit reaction, idle,
      death) для каждого скелета.

---

## 9. Что взять из существующих решений

- Reinisch/Darkest-Dungeon-Unity — уже использует Newtonsoft.Json и
  Spine-Runtimes; логика data-driven, DTO можно переиспользовать.
- Unity-Replay-System (StepanLem) — идея «записываемых значений»
  (ReplayableValue); адаптировать под чистый C#.
- Combat Serialization Strategy (opencombatengine) — DTO + IStateful<T> +
  System.Text.Json для снимков состояния; паттерн применим к stateSnapshot.
- EventCoalTrain — лёгкая типизированная шина для IGameEventBus.
- GameplayRecorder (Google) — только идея записи в JSON; JsonUtility
  не поддерживает полиморфизм, брать код не надо.

---

## 10. Итог

Подход = Event Sourcing + исполняемая спецификация. Логи JSONL — не для
реплея 1 в 1, а как эталонные примеры + тест-кейсы для WPF-реализации.
Вместе с schema.md и DTO они образуют контракт, по которому ИИ-агент пишет
клиент без догадок: каждое решение либо подтверждено записью, либо описано
в схеме.