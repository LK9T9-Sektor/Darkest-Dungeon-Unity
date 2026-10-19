# Death's door + heart attack в ядре

- **Death's door**: герой при 0 HP входит в death's door (дебаффы `deathsdoor*`, `BarkStress` 6,
  survival-бафф) вместо мгновенной смерти; при повторном ударе — ролл `DeathBlow`-резиста (клэмп
  0..0.87, +0.3 при численном перевесе стороны); хил снимает death's door и вводит в mortality.
- **Heart attack** (стресс 200): на death's door — смерть, иначе — 100% урона + стресс 75% + вход
  в death's door.
- **Монстры**: `death_class.can_die_from_damage` парсится — монстры с `false` не умирают от урона
  (как боссы-анцесторы). Парсинг `deaths_door:` секции героев; ресолв баффов case-insensitive.
