# Spine (WPF): прозрачность атласа и «белые прямоугольники» (vertex colors, SkiaSharp 2.88)

Разбор дефекта Spine-рендера в WPF: почему на прозрачных пикселях атласа появлялись белые
прямоугольники, как воспроизведены и чем закрыты. Краткая версия (gotcha) — в
`presentation_wpf.md` §6.

## Симптом

На демо-экране «SPINE TEST» вокруг частей Crusader видны непрозрачные белые прямоугольники,
совпадающие с границами mesh-регионов атласа (голова, торс, руки, ноги). Скелет, анимация и
кости корректны — «белым» становилась только прозрачная часть текселей атласа внутри
треугольников меша. На UI (WPF-элементы) и на обычных `DrawBitmap`-спрайтах дефект не проявлялся.

## Минимальное воспроизведение

Изолированный тест (SkiaSharp 2.88.8, CPU-рендер): атлас 64×64, крайний тексель `(0,0,0,0)`
(полностью прозрачный), центр `(0,0,0,255)`; один quad с UV, покрывающий оба текселя; шейдер —
`SKShader.CreateBitmap(bitmap, Clamp, Clamp)`.

```csharp
// схема: три варианта draw для одного и того же quad
canvas.DrawBitmap(bitmap, rect);                          // A: DrawBitmap
canvas.DrawVertices(verticesWithoutColors, blend, paint); // B: DrawVertices, colors: null
canvas.DrawVertices(verticesWithColors, blend, paint);    // C: DrawVertices, colors: [white]
```

Пиксели кадра (`SKBitmap.GetPixel`) в области прозрачного текселя:

| Вариант | Прозрачный тексель (0,0,0,0) | Непрозрачный тексель (0,0,0,255) |
|---|---|---|
| A: `DrawBitmap` | `(0,0,0,0)` — верно | `(0,0,0,255)` — верно |
| B: `DrawVertices`, `colors: null` | `(0,0,0,0)` — верно | `(0,0,0,255)` — верно |
| C: `DrawVertices`, `colors: [white]` | `(255,255,255,255)` — **дефект** | `(0,0,0,255)` — верно |

Вывод: дефект появляется только при непустом массиве vertex colors и только на полностью
прозрачных текселях — результат равен цвету вершины (непрозрачный белый). Тот же кадр без
vertex colors корректен, значит атлас, шейдер, UV и бленды не при чём.

## Причина

Дефект пути `drawVertices` с vertex colors в SkiaSharp 2.88.8: при одновременных vertex colors и
растровом (bitmap) шейдере полностью прозрачный premultiplied-тексель выходит из сэмплинга с
неопределённым RGB, и итоговый пиксель принимает цвет вершины вместо прозрачного. В нашем коде
это и было исходной реализацией: рендер передавал в `SKVertices.CreateCopy` массив цветов
вершин `[white]` как «тинт-множитель», и каждый прозрачный пиксель атласа превращался в белый.

Существенно для документирования: воспроизводится **без** участия нашего кода (голый SkiaSharp),
на CPU-растре и не зависит от нашей математики UV/трансформов. Это дефект версии/пути Skia, а не
ошибка геометрии Spine. Альтернативный корректный путь того же кадра — `DrawBitmap` и
`DrawVertices` без colors (см. таблицу).

## Решение

Тинт убран из vertex colors и перенесён на `SKColorFilter`:

- `SpineFrameRenderer.Render` — `SKVertices.CreateCopy(..., colors: null, indices)`
  (`src\Wpf\Sektor.DarkestDungeon.Spine.Wpf\SpineFrameRenderer.cs:75-77`); цвет кадра
  рассчитывается как `skeleton × slot × attachment` (`CreateColor`).
- `SpineFrameRenderer.GetPaint` — `SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate)`
  на paint (`SpineFrameRenderer.cs:198-217`); кэш paint'ов — по ключу (текстура, бленд, цвет),
  `CachedPaint.Dispose` освобождает шейдер и фильтр.
- Формула тинта и мапа блендов (`normal→SrcOver`, `additive→Plus`, `multiply→Modulate`,
  `screen→Screen`) — в `docs\mechanics\presentation\presentation_wpf.md` §6.

Проверка фикса в мини-репро: с фильтром прозрачные тексели остаются `(0,0,0,0)`, чёрный центр —
`(0,0,0,255)`, полупрозрачный тинт (A=128) даёт A=128 — каждый канал ведёт себя как множитель.

## Защита от регресса

Тест `IdleAnimation_KeepsTransparentPixelsTransparent`
(`tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\SpineFrameRendererTests.cs`): рендерит кадр idle
(idle, t=0) в bitmap анимационных bounds (`FrameProjection` по `MeasureAnimation`) и требует, чтобы
доля полностью прозрачных пикселей в этой области лежала в границах **(0.15, 0.9)**:

- без фикса доля прозрачных ≈ 0 (белые прямоугольники закрывают фон) — тест краснеет;
- нижняя граница 0.15 — «фон виден»; верхняя 0.9 — «персонаж отрисован» (защита от пустого кадра).

## Проверка

- Мини-репро (три варианта draw + ColorFilter) — таблица выше; скрипты диагностики:
  `%TEMP%\opencode\spine-repro2.ps1`, `spine-repro3.ps1` (декод в pwsh: сначала
  `NativeLibrary.Load(<bin>\runtimes\win-x64\native\libSkiaSharp.dll)`).
- Приложение: скриншот демо-экрана — Crusader рендерится полностью, белых прямоугольников нет;
  кадры анимации различаются (тест `Render_TwoFrames...`).
- Полный прогон `dotnet test tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests` — зелёный.

## Подводные камни

- **Не возвращать vertex colors в `DrawVertices` с bitmap-шейдером** в SkiaSharp 2.88.x —
  дефект версии. Тинт, альфу и бленды держать на paint (`ColorFilter`/`BlendMode`).
- Тот же дефект объясняет «белые квадраты» как индикатор: если в кадре появились белые
  прямоугольники ровно по регионам атласа — это он, а не UV, не экспорт атласа и не формат PNG.
- При апгрейде SkiaSharp (3.x и выше) имеет смысл перепроверить: возможно, путь исправлен
  апстримом; регресс-тест при этом останется валидным независимо от реализации.
- `DrawBitmap` вместо `DrawVertices` — не альтернатива «на будущее»: теряются деформация меша
  скелета и per-slot бленды; чинилось именно наложение цвета, примитив остался тем же.

## Файлы-источники

- `src\Wpf\Sektor.DarkestDungeon.Spine.Wpf\SpineFrameRenderer.cs` — `Render`, `GetPaint`,
  `CachedPaint`.
- `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\SpineFrameRendererTests.cs` —
  `IdleAnimation_KeepsTransparentPixelsTransparent`, `FrameProjection`.
- `docs\mechanics\presentation\presentation_wpf.md` §6 — краткая версия (gotcha) и ссылка сюда.
