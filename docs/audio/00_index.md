# Аудио-каталог (FMOD)

Навигатор по аудио-данным проекта и записи аудио-событий в контракт `docs\SPEC_GAME_EVENTS_SCHEMA.md`.

## Артефакты

| Артефакт | Что внутри |
|---|---|
| `docs\audio\audio_catalog.json` | Полный машиночитаемый каталог: 2007 уникальных `event:/` путей, банки, флаги `isStream`/`is3D`/`isOneShot`, группы |
| `docs\audio\audio_catalog.md` | Человекочитаемая сводка: counts, банки с числом событий, все music events, группы |
| `tools\extract-fmod-catalog.ps1` | Генератор обоих файлов из Unity-кэша |

## Источник

Каталог строится из `unity\Assets\FMODStudioCache.asset` — это единственный надёжный источник
связи «событие → банк». Бинарные `.strings.bank` не парсятся: их формат версионируется FMOD, а
динамические пути (`event:/char/ally/<class>_<skill>`) собираются в рантайме и записываются в
журнал фактическим значением, а не по каталогу.

Настройки банков (master bank, список загружаемых банков) — `unity\Assets\Resources\FMODStudioSettings.asset`.

## Генерация

```powershell
pwsh tools\extract-fmod-catalog.ps1
```

Опции: `-CachePath`, `-OutputPath`, `-MarkdownPath`, `-LogPath` (по умолчанию
`%TEMP%\extract-fmod-catalog.log`). Скрипт детерминирован (без timestamp) и падает с ненулевым
кодом, если кэш отсутствует или ссылка на банк не разрешается; полное исключение — в логе.

Зависимости: PowerShell 7+, закоммиченный `FMODStudioCache.asset`. FMOD-рантайм не нужен.

## Использование в клиентах

- WPF-реплей читает `AudioPlayed.EventPath` и по каталогу находит банк; для воспроизведения
  нужны нативные `fmod.dll`/`fmodstudio.dll` x64 (`unity\Assets\Plugins\FMOD\Libs\x86_64`) —
  отдельный срез.
- Legacy Unity уже загружает банки через `FMODStudioSettings`; запись аудио идёт из
  `DarkestSoundManager` и прямых вызовов `RuntimeManager.CreateInstance`.

## Актуальность

После изменения FMOD-проекта (новые/удалённые события) перегенерировать каталог тем же
скриптом и закоммитить оба файла вместе. Числа-ориентиры текущей версии: 3908 записей,
2007 уникальных событий, 40 банков, 20 music events.
