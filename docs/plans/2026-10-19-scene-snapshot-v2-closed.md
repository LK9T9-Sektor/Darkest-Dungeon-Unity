> Закрыт: 2026-10-19 (ветка `core/unity7-urp`). Ключевые коммиты: `19ea5aa` (дампер v2),
> `1184459` (ресериализация Unity 7), `9b704f2` (статусы в PLAN.md). Перенесён из `PLAN.md` при
> старте плана фазы 2-A; индекс закрытых планов — `docs\plans\00_index.md`.

# PLAN: Scene snapshot v2 — авто-опись сцены для раскладки

## Что сделано

- DTO/ридер расширены: заголовок `ScreenWidth/Height/Aspect`, флаг DDOL, экстракторы `CanvasScaler`, `Text` (~80 симв.), LayoutGroup/CSF/LE/AspectRatioFitter, `Image.fillMethod`, `Canvas.pixelPerfect`, `Camera.pixelRect/targetTexture`; screen rect (px + доли экрана) в `ReadRectTransform`.
- Новый `SceneSnapshotLayoutWriter.cs` (+`.meta`) — компактный CSV рядом с JSON; DDOL вынесен в псевдо-сцену с отдельными файлами `DontDestroyOnLoad-<триггер>-<stamp>`.
- Триггеры: `auto-settled` вместо t0, resize при смене `Screen.width/height`, публичный `Capture(tag)`; README дампера обновлён.
- Верификация: `unity-compile-check.ps1` зелёный; на каждую сцену `auto-settled`/`auto-t5s` (JSON+CSV+PNG), шапка 1920×1080/1.7778, screen rect сходится (488.5/1920 = 0.2544), DDOL 144/144 UI-строк. Коммит `19ea5aa`: 7 файлов, +645/−37.
- По снимкам пользователя найдена причина проблемы раскладки: `RaidPanel` = (0,0,1,1/3) во всех снимках, арт-полоса фикс 360 юнитов занимает 52% экрана на 2.77:1.

## Осталось / за пользователем

- Фаза 2 (адаптивность рейдового HUD, фикс BattleTest, карта долей экрана → XAML для WPF) — отдельным планом (см. `PLAN.md` «Фаза 2-A»).
- Cleanup артефактов по желанию: `unity\Build\`, `%LOCALAPPDATA%Low\RedHooks\Darkest Dungeon\SceneDumps\`, временные логи.

---

_Ниже — исходный план без изменений._

## Goal

Расширить диагностический дампер (`unity\Assets\Scripts\Diagnostics\`) до инструмента, который в
устоявшемся состоянии сцены сам снимает полную опись: экранные координаты каждого `RectTransform`
(пиксели + доли экрана), `CanvasScaler`, тексты, лейаут-компоненты, камеры, `DontDestroyOnLoad`.
На выходе — JSON + компактный layout-лист (CSV), по которому затем (фаза 2, отдельный план)
переделывается раскладка рейдового HUD и BattleTest в Unity, а WPF воссоздаёт похожие позиции.
Легаси-код не трогаем; инструмент живёт в `Scripts\Diagnostics` и гейм-код не использует.

## Design decisions (user-confirmed)

- Триггеры — только авто: `auto-settled` (после загрузки сцены: end of frame + ForceUpdateCanvases
  + кадр, заменяет t0), resize-снимок при смене `Screen.width/height`; t5/t15 и F9 остаются как
  резерв. Семантические хуки — позже, публичный `Capture(tag)` задел на будущее.
- Объём — расширенный: экранные координаты, CanvasScaler/канвасы/камеры, uGUI `Text`,
  лейаут-компоненты (LayoutGroup/ContentSizeFitter/LayoutElement/AspectRatioFitter).
- Артефакты — JSON (большой) + компактный layout-лист по UI-узлам рядом.
- Фаза 2 (раскладка HUD, фикс BattleTest, карта для WPF) — отдельным планом по этим данным.

## Steps / status

1. [x] PLAN.md зафиксирован.
2. [x] DTO + ридер: заголовок `ScreenWidth/Height/Aspect` + флаг DDOL; экстракторы CanvasScaler,
   Text (~80 симв.), LayoutGroup/CSF/LE/AspectRatioFitter, Image.fillMethod, Canvas.pixelPerfect,
   Camera.pixelRect/targetTexture; screen rect (px + доли экрана) в `ReadRectTransform`
   (root-canvas ищется с `includeInactive: true` — иначе неактивные панели молча теряли координаты).
3. [x] `SceneSnapshotLayoutWriter.cs` (+ `.meta`) — CSV рядом с JSON, та же фоновая запись.
4. [x] DDOL: псевдо-сцена (`GetSceneByName("DontDestroyOnLoad")`, fallback — обход
   `Resources.FindObjectsOfTypeAll`), отдельные файлы `DontDestroyOnLoad-<триггер>-<stamp>` на каждое
   capture-событие.
5. [x] Триггеры: `auto-settled` вместо t0, resize-триггер, публичный `Capture(tag)`.
6. [x] README дампера: поля/артефакты/триггеры; убрать ограничение «только активная сцена».
7. [x] Верификация (агент): `unity-compile-check.ps1` зелёный (дважды); билд + прогон игрока:
   на каждую сцену — `auto-settled`/`auto-t5s` (JSON+CSV+PNG), DDOL — отдельные JSON+CSV
   (1,4–2 МБ, 152 узла), шапка `Screen*` корректна (1920×1080/1.7778), screen rect сходится
   (проверено вручную: 488.5/1920 = 0.2544); на активных сценах screen rect у 100% UI-узлов,
   у неактивных — после правки includeInactive (перепроверено: DDOL 144/144 UI-строк с rect).
8. [x] Пользователь: снимки сделаны (редактор и билд) — `Dungeon-auto-settled-*` и
   `resize-<WxH>-*` (2658×960, 3088×1440, 2640×1200, 520×325, 677×423 и др.). Данные разобраны
   при подготовке фазы 2-A: `RaidPanel` = (0,0,1,1/3) во всех снимках; арт-полоса панели — фикс
   360 юнитов (на 2.77:1 занимает 52% экрана и режется снизу — корень проблемы раскладки).
9. [x] Коммит на `core/unity7-urp`: `19ea5aa` (7 файлов, +645/−37, hook зелёный); push — по запросу.

## Cleanup (по завершении плана)

- `unity\Build\` — вывод сборки (gitignored), нужен для быстрых прогонов; удаляем по желанию.
- `%LOCALAPPDATA%Low\RedHooks\Darkest Dungeon\SceneDumps\` — десятки МБ дампов; старые чистить.
- Логи сборки/чеков: `%TEMP%\dd-build*.log`, `%TEMP%\unity-build-*.log`, `%TEMP%\unity-compile-*.log` — временные.

## Affected files

- `unity\Assets\Scripts\Diagnostics\SceneSnapshot.cs`, `SceneNodeSnapshot.cs` (при необходимости),
  `SceneSnapshotPropertyReader.cs`, `SceneSnapshotDumper.cs`, новый `SceneSnapshotLayoutWriter.cs`.
- `unity\Assets\Scripts\Diagnostics\README.md`.
- `PLAN.md`.

## Acceptance criteria

- В SceneDumps на каждую загрузку сцены: JSON + CSV + PNG с тегом `auto-settled`; отдельные файлы
  `DontDestroyOnLoad-<триггер>-*` с содержимым `DontDestroyOnLoad` (менеджеры/хосты из стартовых сцен).
- CSV содержит `RaidPanel` со screen rect в пикселях и долях; `CanvasScaler` присутствует в JSON.
- Смена разрешения без F9 → файл `resize-<WxH>`; легаси-сцена не меняется; компиляционный чек зелёный.

## Out of scope (фаза 2, отдельный план)

- Адаптивность рейдового HUD (нижняя панель/верх/центр), рефактор в переиспользуемое.
- Фикс BattleTest (позиционирование юнитов с первого кадра, масштаб ~30–35% высоты кадра,
  аспект-безопасные места).
- Карта долей экрана → XAML для WPF.
