# presentation_unity_battle_view.md — Unity BattleTest: тонкий вид-слой над ядром боя

> Домен: `presentation` (оба Unity-дерева). Статус: **реализовано (Stage-1)** — играбельный визуальный
> бой; полировка анимаций (Stage-2) — позже. Первый реальный Unity-потребитель ядра боя
> (`Core.Combat`/`Core.Duel`) — прототип Фазы 6 (`EXTRACTION_PLAN.md`).

## 1. Назначение и когда работает

Новая сцена `Assets\Scenes\BattleTest.unity` (в `unity\` и `unity-2017\`) — гибкий настраиваемый
бой (герои/монстры по слотам, контроль сторон, seed, torch) на **чистом ядре**. Unity — только вид:
вся логика боя исполняется в `DuelController`. Легаси-движок (`RaidSceneManager`/`BattleGround`) не
используется и не изменяется. Сцена открывается напрямую, без кампании и без `DarkestDungeonManager`.

## 2. Модель данных и слои

- **Конфиг** (`Assets\Scripts\Testing\BattleTest\`): `BattleTestConfig` → `Side1/Side2`
  (`BattleTestSideSpec` → `BattleTestSlotSpec`), `PlayerControlsSide1/2`, `Seed`, `Torch`, `Difficulty`.
- **Драйвер**: `CoreBattleDriver` (MonoBehaviour) — владеет `DuelController`, маппинг
  `ICombatUnit` ↔ `BattleUnitView`, поллинг/диффинг, роутинг ввода/AI.
- **Вид**: `BattleFormationView` (раскладка по `FormationDisplayOrder`), `BattleUnitView` (префаб +
  HP/stress бары + выделение + смерть), `BattlePopupLayer` (мировые попапы), `BattleHud` (экран:
  раунд, аннонс, скиллы/цели/PASS, лог, победитель), `BattleTestConfigPanel` (setup-оверлей).
- **Контент**: `FightContentLoader.Content` (`TextFightContent`, грузит Resources → каталоги ядра).

## 3. Парсинг контента

Контент грузится `FightContentLoader` (heroes/monsters/brains/buffs/quirks/traits/effects через
`GameDataReader`), сцена его не парсит. Визуальные префабы юнитов — `Resources.Load("Prefabs/Heroes|Monsters/<class>")`
(самодостаточные Spine-префабы); легаси-компонент `FormationUnit` на корне префаба удаляется.

## 4. Порядок срабатывания (трассировка)

1. `BattleTestConfigPanel` собирает `BattleTestConfig` → `CoreBattleDriver.Begin(config)`.
2. `Begin`: `new DuelController(content)`; при монстрах на стороне 2 — `StartFight`, иначе
   `StartDuel(isHost:true)`; `Context.TorchAmount = config.Torch` (до `StartBattle`, влияет на сюрприз);
   `StartBattle()`. Затем создаются попап-слой и адаптер событий, инициализируются формации,
   `RefreshView()`.
3. `Update()` драйвера (каждый кадр):
   - `Duel.IsFinished` → однократный баннер победителя.
   - Иначе `actor = Duel.CurrentUnit`; смена `CombatInfo.CombatId` → «новый ход» (сброс
     `_turnInitialized`, пустой аннонс).
   - Актор на **игровой** стороне → `BattleHud.ShowActor(actor)`, ждём ввод.
   - Актор на **AI**-стороне → таймер `AiActionDelay` (0.4 с) → `ActForAi`.
4. Ввод игрока: `Hud.SelectSkill` → `Duel.GetAvailableTargets(actor, skill)` → кнопки целей →
   `CoreBattleDriver.PlayerAct(skillId, targetId)`. Роутинг по `Duel.IsLocalTurn`:
   - local → `ExecuteLocalSkill/Pass/Move`;
   - remote → `ApplyRemoteSkill(DuelPayload.*)` (`StartFight` инвертирует стороны: герои — «remote»).
5. AI: `DecideAiPayload` — монстр с брейн → `Solver.UseMonsterBrain`; герой → `DuelAi.ChooseAction`;
   применяется тем же роутингом.
6. После каждого действия `CompleteAction()`: рендер `Solver.SkillResult` в попапы + `RefreshView()`
   (формации, раунд, лог `Events.Log` по watermark).
7. Статус-попапы приходят асинхронно из `DuelBattleEvents.PopupShown` через `BattleEventsAdapter`
   (паддинг не блокирует ход).

## 5. Очередь и обновления

- Ядро не эмитит события на «ход начался/скилл применён» — вид **поллит и диффит** состояние после
  каждого действия (паттерн WPF `DuelBattleViewModel`): `CurrentUnit`→ход, `RoundNumber`→раунд,
  `IsDead`→смерть, `Rank`→движение, `SkillResult`→урон/крит/хил/мисс, `PopupShown`→статусы, `Log`→лог.
- Мгновенно: после `ExecuteLocal*`/`ApplyRemoteSkill` ядро само завершает ход (`FinishSkillAction`/
  `CompleteTurn`), драйвер лишь перечитывает `CurrentUnit`.

## 6. Проверки и клэмпы

| Проверка | Где |
|---|---|
| Ввод игрока только на своей стороне | `CoreBattleDriver.CanPlayerAct` / `IsPlayerControlled` |
| Валидность скилла/цели | ядро (`ExecuteLocalSkill`/`ApplyRemoteSkill` игнорируют невалидное) |
| Торч | `Mathf.Clamp(Torch, 0, 100)` перед `StartBattle` |
| Смена хода | по `CombatInfo.CombatId` |

## 7. Нюансы и подводные камни

- **Событий в ядре нет** — вид обязан перечитывать состояние; не полагаться на
  `DuelBattleEvents.StateChanged` (ни разу не вызывается).
- **`StartFight` инвертирует local/remote** (`IsHost=false`): герои игрока — «remote», их ввод идёт
  через `ApplyRemoteSkill`. В `StartDuel(isHost:true)` — наоборот. Единый роутинг — по `IsLocalTurn`.
- **Legacy-твины в глобальном неймспейсе**: `CombatSkill`, `MonsterBrainDecision`, `SkillResult`,
  `Team` и т.п. существуют и в `Assets\Scripts`, и в core-DLL; в глобальном неймспейсе они перекрывают
  core-типы из `using`. Код вида **полностью квалифицирует** core-типы (`Sektor.DarkestDungeon.Core....`).
- **Нет EventSystem в сцене** — создаётся рантаймом `RuntimeUiFactory.EnsureEventSystem()` (панель
  вызывает в Awake), чтобы сцена не ссылалась на engine-GUID'ы Unity 6000.
- **Префабы юнитов несут легаси `FormationUnit`** — компонент отключается и уничтожается при
  создании `BattleUnitView`, иначе его `Update()` дёргает `RaidSceneManager.Rules` (NRE).
- **Попапы в мировых единицах**: канвас попапов имеет масштаб 0.01 (текст fontSize 120), но позиции
  выставляются в мировых координатах (`RectTransform.position`), без пересчёта на масштаб канваса.
- Сцена не требует `DarkestDungeonManager.prefab` (контент — `FightContentLoader`, локализация не
  нужна), поэтому открывается напрямую.
- Не изменён ни один легаси-файл (`RaidSceneManager` и др. — read-only).

## 8. Взаимодействия

- Ядро: `DuelController`, `DuelBattleEvents`, `Solver.SkillResult`, `FormationDisplayOrder`,
  `HeroFightUnitSpec`/`MonsterFightUnitSpec`/`DuelHeroPick`, `DuelAi`, `IBattleContext` (см. `combat/*`,
  `duel/*`).
- Контент: `FightContentLoader` (`TextFightContent`).
- Инструменты: `tools\unity-generate-battle-test-scene.ps1` (пересборка сцены),
  `unity-compile-check.ps1` (проверка обоих деревьев).

## 9. Файлы-источники

- `unity{,-2017}/Assets/Scripts/Testing/BattleTest/*.cs` (10 файлов).
- `unity{,-2017}/Assets/Editor/BattleTestSceneBuilder.cs`.
- `unity{,-2017}/Assets/Scenes/BattleTest.unity`.
- `tools/unity-generate-battle-test-scene.ps1`.
- См. также: `docs/TESTING.md` (§ Тест-бой BattleTest), `docs/BATTLE_PARITY.md`, `docs/EXTRACTION_PLAN.md`
  (Фаза 6), `docs/mechanics/duel/duel_05_fight.md`.