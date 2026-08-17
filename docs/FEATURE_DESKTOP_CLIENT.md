# FEATURE_DESKTOP_CLIENT.md — Десктоп-клиент WPF

## Цель

Второй тонкий потребитель чистого ядра и сети — WPF-клиент (proof of concept): дуэль 1v1 с
переиспользованием логики из `src\Core` и транспортов Steam/Photon. Доказывает шов «логика без ссылок
на движок/UI»: один и тот же бой в Unity и в WPF; транспорт переиспользуем в другом клиенте.

## Состав

- **Расположение:** в DD-монорепо, `clients\wpf\` (net8.0, ссылки на `src\Core` + `src\Lan`/`src\Networking`).
- **Мини-слой дуэли** в `src\Core\Combat` (2×4 юнита, атака/лечение/статус/ход/победа) — затравка Фазы 3.
- **Сеть:** `SteamTransport` (host/join по session id, уже работает); `PhotonTransport` — после Фазы 5.
- **UI:** XAML по паттерну session/snapshot — лобби (nickname, host/join, ROOM_ID) + бой (юниты, HP,
  скилл+цель, лог).
- **Этап A (без Spine):** плейсхолдеры — доказывает ядро+сеть+UI с нулевым риском.
- **Этап B (опционально):** Spine-контрол отдельным `UserControl` — spine-csharp 2.x (под ассеты DD,
  runtime 2.3) + SkiaSharp (фолбэк MonoGame.WpfCore); демо на sample-скелете 2.x. Аудио в PoC пропускаем.

## Gate

Мини-ядро в `src\Core\Combat` (создаётся в составе PoC) + `src\Lan` (есть). Полноценно — по мере выноса.

## Feature-flag

`desktop_client` (off по умолчанию).

## Статус

idea. Cross-cutting: частично — новый клиент-презентация, переиспользует ядро и сеть.
