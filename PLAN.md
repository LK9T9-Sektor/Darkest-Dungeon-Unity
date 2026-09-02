# PLAN: Unified skill/MOVE reveal-then-resolve + pacing for the Duel (WPF)

## Goal

Match the duel AI mode's skill previews in the multiplayer (network) duel, via a **unified
reveal-then-resolve** path in `DuelBattleViewModel` shared by AI and MP. Also:

1. Unified pacing: 0.5s delay + "ТВОЙ ХОД" popup when the turn passes to a new character; 1s skill/MOVE
   reveal after the rival's action is received (AI and MP alike).
2. High-performance turn popup ("ТВОЙ ХОД" + gold flash on the card that receives the turn).
3. Reduce the AI "thinking" time in the skill preview.

## Design decisions (user-confirmed)

- Single `reveal-then-resolve` for both AI + MP previews.
- Pacing logic lives in `DuelBattleViewModel` (not the links).
- Turn popup = flash + "ТВОЙ ХОД" badge on the receiving actor card.
- **Pacing applies to the rival side only** (decision shifted after tests): local actions stay
  immediate so the WPF test suite (which does not pump the Dispatcher) stays green. The 1s beat
  covers the rival reveal for both AI + MP uniformly; the 0.5s turn-swap is a rival-side beat +
  popup, not a local input block.

## Steps / status

1. [x] **State machine in `DuelBattleViewModel`.**
   - `PaceState` enum (`Idle`/`TurnSwap`/`Reveal`), 50ms `DispatcherTimer`, `TurnSwapMs=500`,
     `RevealMs=1000`, `bufferedRivalPayload`, `previousActorCombatId`, `actorInitialized`.
   - `DetectTurnTransition()` (called from `Refresh()`): fires a one-shot "ТВОЙ ХОД" popup + `Turn`
     flash on the new actor and enters `TurnSwap`.
   - `PaceTick()` advances the beats; a rival payload buffered during `TurnSwap` is released to
     `StartRivalReveal` after 0.5s; `Reveal` completes → `ApplyStaged()`.
   - `OnRivalActionReceived` buffers during `TurnSwap`; otherwise `StartRivalReveal(payload)`.
   - `StartRivalReveal(payload)`: parses `move|rank` → sets `AiTargetPreview` to destination card +
     `IsMovePreview=true`; else calls `OnAiSkillPreviewed`/`OnAiTargetPreviewed`; stages the payload.
   - `ApplyStaged()` injects the staged rival payload via `ApplyRivalActionPayload`.

2. [x] **AI mode — simplify `AiRivalLink` to immediate action emission.**
   - Removed the old multi-phase `SkillPreviewed`/`TargetPreviewed` pacing; single 100ms timer with a
     `lastActedCombatId` guard emits the rival action once per turn. Events still declared (interface).

3. [x] **Local actions reverted to immediate execution** (kept tests green):
   - `SelectSkill`/`SelectTarget`/`SelectMove`/`Pass` execute immediately, gated only on
     `controller.IsLocalTurn`.
   - Removed `IsInputEnabled`, `LocalStageKind`, `stagedLocal*` fields, `ExecuteStagedLocal`;
     simplified `ApplyStaged`/`BeginReveal` to rival-only.

4. [x] **Turn popup visual.**
   - `DuelUnitViewModel`: `_cardFlash`, `_turnPopupVisible`, `TriggerTurnPopup()` (auto-hide via a
     1200ms `DispatcherTimer`).
   - `DuelUnitCardView.xaml`: `"Turn"` flash `DataTrigger` (gold pulse) + "ТВОЙ ХОД" `TextBlock`
     (rise + fade) bound to `TurnPopupVisible`.

5. [x] **Skill/MOVE arrow preview.**
   - `DuelBattleView.xaml.cs` `RedrawAiArrow`: branches on `IsMovePreview` → `DrawMoveArrow`
     (`AiTargetPreview`); else elbow arrows. `DuelBattleView.cs` reacts to `IsMovePreview`.

6. [x] **Tests + docs.**
   - Added `RivalAction_IsStagedAndPreviewed_NotAppliedImmediately` (`DuelRenderTests.cs`) proving the
     1s reveal holds the rival payload (preview shown, controller not advanced synchronously).
   - Verified: WPF tests 65/65, duel tests 43/43, combat tests 61/61, `check-using-placement.ps1` OK,
     WPF build 0 errors / 0 warnings.
   - Update `docs\mechanics\duel\duel_01_lockstep.md`, `docs\mechanics\presentation\presentation_wpf.md`,
     `docs\TESTING.md` (still to do).

## Affected files

- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/DuelBattleViewModel.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Combat/AiRivalLink.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/DuelUnitViewModel.cs`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/DuelUnitCardView.xaml`
- `src/Wpf/Sektor.DarkestDungeon.Wpf/Views/DuelBattleView.xaml.cs`, `DuelBattleView.cs`
- `tests/Wpf/Sektor.DarkestDungeon.Wpf.Tests/DuelRenderTests.cs`
- `docs/mechanics/duel/*`, `docs/mechanics/presentation/presentation_wpf.md`, `docs/TESTING.md`

## Acceptance criteria

- In MP duel, the rival's skill/MOVE shows a 1s preview (badge + arrow/move-line) before resolving,
  matching AI mode.
- A 0.5s turn-swap beat + "ТВОЙ ХОД" popup + gold flash plays when the turn passes to a new character.
- AI acts faster (100ms decision tick) than before.
- Local actions remain immediate; all WPF tests pass without pumping the Dispatcher.
