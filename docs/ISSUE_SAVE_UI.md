# ISSUE: Сейв-экран в Unity 6 — именование, удаление и загрузка сейва

Статус: **open** (требует проверки пользователем). Обнаружено при отладке через
диагностические логи `[DD]` (коммит `d37de4f`).

## Симптомы

1. Новый сейв создаётся в UI, но «продолжить» не работает — сцена не загружается.
2. Клик по заполненному слоту (второй сейв) ничего не делает.
3. Крестик удаления сейва не нажимается.
4. Ввод имени с клавиатуры не мапится: `GetVirtualKey: Could not map char: ...`.

## Диагностика по логам

Цепочка дедлока (видно по `[DD] [SAVE] ...`):

1. `SaveSlot.SaveButtonClick` (пустой слот) → `SaveSelector.SaveNamingStart`.
2. `SaveNamingStart` вызывал `DisableInteraction()` на **всех** слотах, включая
   именуемый → `saveSlotButton.interactable = false`, а `titleInput` (InputField)
   находится **внутри** кнопки Save → ввод блокировался, `onEndEdit` не срабатывал.
3. `SaveNamingCompleted` не вызывался → `selectedSaveSlot` оставался не-`null` →
   `SaveSelector.Update` перехватывал клики мышью в `RefocusInput` → крестик удаления
   и повторный клик по слотам не доходили до кнопок.
4. `GetVirtualKey: Could not map char` — старый Input Manager не мапит вводимые
   символы (кириллица/раскладка) в виртуальные клавиши.

## Что уже исправлено (коммит `b16ea18`)

- `SaveSelector.SaveNamingStart`: `DisableInteraction()` применяется только к **другим**
  слотам, именуемый слот остаётся активным (InputField внутри него работает).
- `SaveSlot.SaveNamingCompleted` при пустом имени: теперь вызывает
  `SaveSelector.SaveNamingCompleted()` — слоты разблокируются (раньше `return` без
  разблокировки).
- `SaveSelector.Update`: Escape во время именования отменяет его (раньше
  обрабатывался только при `selectedSaveSlot == null`).
- `SaveSlot.SaveButtonClick`: добавлен `titleInput.ActivateInputField()` — поле
  гарантированно получает фокус ввода.

Изменения применены в обоих деревьях (`unity\` и `unity-2017\`).
`unity\` — compile-check пройден.

## Доп. фикс: UI не залипает + создание сейва без ввода с клавиатуры

- **Симптом (повтор после `b16ea18`):** клик по пустому слоту (началось именование)
  → слоты отключались, а `SaveSelector.Update` перехватывал каждый клик мыши
  (`RefocusInput`) → клик по заполненному слоту ничего не делал, UI «не отвечал».
  Дополнительно ввод имени не работал на не-US раскладках (`GetVirtualKey: Could
  not map char`), поэтому именование нельзя было завершить.
- **Причина:** перехват кликов в `Update`, отключение остальных слотов в
  `SaveNamingStart`, и зависимость создания сейва от ввода текста.
- **Исправление:**
  - `SaveSelector.Update`: перехват кликов мыши убран — осталась только Escape-отмена
    (`AbortNaming()`). Клик мимо поля ввода отпускает фокус → `onEndEdit`
    срабатывает → слоты разблокируются.
  - `SaveSelector.SaveNamingStart`: остальные слоты больше **не отключаются**;
    при именовании на другом слоте сначала `AbortNaming()`.
  - `SaveSlot.SaveButtonClick`: повторный клик по именуемому слоту «подтверждает»
    именование (создаёт сейв); клик по заполненному слоту сначала сбрасывает
    активное именование (`SaveSelector.SaveNamingCompleted()`), затем загружает.
  - `SaveSlot.SaveNamingCompleted`: пустое имя больше не отменяет — создаётся сейв
    с дефолтным именем `"Campaign N"` (создание не зависит от клавиатуры; Escape —
    единственный способ полностью отменить).
  - `SaveSlot.CancelNaming()` — сброс поля без создания сейва (для `AbortNaming`).

Изменения применены в обоих деревьях (`unity\` и `unity-2017\`).
`unity\` — compile-check пройден.

## Осталось проверить пользователю

- [ ] Клик по пустому слоту — UI не залипает: клик мимо/Enter/повторный клик по
      слоту создаёт сейв (дефолтное имя при пустом вводе).
- [ ] Заполненный слот загружается даже во время именования другого слота.
- [ ] Escape отменяет именование без создания сейва.
- [ ] Создание сейва → «продолжить» → загрузка `LoadingScreen` → следующая сцена.
- [ ] Крестик удаления сейва.
- [ ] Повторный клик по заполненному слоту.

## Связанные ссылки

- `unity\Assets\Scripts\Setup\SaveSystem\SaveSlot.cs`
- `unity\Assets\Scripts\Setup\SaveSystem\SaveSelector.cs`
- `unity\Assets\Scripts\Setup\SaveSystem\SaveLoadManager.cs`
- `unity\Assets\Scripts\Setup\ContentLoading\ScreenLoader.cs`
- `unity\Assets\Scripts\Managers\DarkestDungeonManager.cs` (глобальный перехватчик `[DD]`)
