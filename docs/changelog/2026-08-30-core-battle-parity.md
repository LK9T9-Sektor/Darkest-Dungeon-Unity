# Паритет боевых механик с Unity (`BATTLE_PARITY.md` §5)

- **DoT-тик урона**: кровь/яд наносят `CurrentTickDamage` в начале хода цели (как в кампании);
  статусы и раундовые баффы тикают по ходам, а не раз в раунд.
- **Stun**: застанный юнит пропускает ход и получает `STUNRECOVERYBUFF` (+40% к stun-резисту, 2 раунда).
- **Riposte**: цель с рипост-статусом контратакует атакующего своим `riposte_skill` (парсится
  `HeroClassFileParser`); скиллы MAA (Retribution) и другие рипост-скиллы теперь работают.
- **Guard**: парсинг `.guard`/`.swap_source_and_target`/`.clearguarding`/`.clearguarded`;
  атаки по охраняемому юниту перенаправляются на защитника (MAA Defender, HoundMaster Guard Dog).
- **Pull/Push/Shuffle**: эффекты реально двигают юнитов в партии (ранги меняются), уважают
  immobilize; MAA Rampart (push), скиллы с `.move` и shuffle работают как в Unity.
- **Immobilize**: блокирует ручное перемещение (`TryMove`); `.unimmobilize`/`.unstun`/`.untag` парсятся.
- **RemoveConditions**: после каждого скилла снимаются условные баффы с исполнителя и целей
  (правило-баффы больше не висят до конца раунда).
- **Buff-идемпотентность**: повторное применение правил не накладывает стат-бафф дважды
  (`IsApplied`-гейт в `ApplyBuff`/`RevertBuff`, как в Unity).
