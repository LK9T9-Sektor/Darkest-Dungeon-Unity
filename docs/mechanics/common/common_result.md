# common_result.md — Result/Result<T>: инвариант «без исключений в ядре»

> Домен: `common` (ядро `Core.Common`). Статус: **реализовано**.

## 1. Назначение и когда работает

Функциональный тип ошибок ядра: вместо исключений для контроля потока/бизнес-ошибок используется
`Result`/`Result<T>`. Инвариант: **ядро не бросает исключения для бизнес-ошибок** (см. `AGENTS.md`).

## 2. Модель данных

- `Result` (`Core.Common/Result.cs:7`) — `Success()` (`:30`), `Failure(errorMessage)` (`:36`),
  `IsSuccess`, `ErrorMessage`.
- `Result<T>` (`Core.Common/ResultOfT.cs:7`) — `Value`, `IsSuccess`, `ErrorMessage`,
  `Success(value)` (`:37`), `Failure` (`:43`).

## 3. Порядок срабатывания (трассировка)

- Методы ядра возвращают `Result`/`Result<T>` для операций, которые могут не выполниться
  (валидация, бизнес-ошибки).
- `IsSuccess` проверяется в caller'ах; `Value` доступен только при успехе.

## 4. Очередь и обновления

- Нет очередей — простой возврат значения.

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Успех | `Result.Success()` | без сообщения |
| Ошибка | `Result.Failure(msg)` | с сообщением |
| `Result<T>` | `ResultOfT` | `Value` при `IsSuccess` |

## 6. Нюансы и подводные камни

- **Не путать с исключениями для программистских ошибок** (аргументы, состояния) — они допустимы;
  `Result` — для ожидаемых бизнес-исходов.
- `Result<T>.Value` при `!IsSuccess` — контракт нарушен; caller обязан проверять `IsSuccess` первым.

## 7. Взаимодействия

- Используется доменами (`Save`, `Campaign`, ...) и клиентами.
- `IProportionValue`/`ISingleProportion` — смежные примитивы (`common_proportion.md`).

## 8. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Common/Result.cs`, `ResultOfT.cs`