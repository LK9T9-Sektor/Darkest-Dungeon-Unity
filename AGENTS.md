# AGENTS.md — Agent Rules

> Single source of truth for AI assistants working on this codebase.

---

## 📁 Repository Structure Invariants

- **Presentation Layer** — The Unity editor environment and asset folders (`unity\` active, `unity-2017\` legacy) contain views, engines, and platform-specific assets. No pure domain logic is allowed here.
- **Source Directory** — Pure C# multi-project source directory targeting strictly `.NET Standard 2.0` with C# language version limited to `7.3`.
- **Test Directory** — Isolated unit and integration tests using standard runners (NUnit/xUnit). Structurally mirrors the architecture of the source directory.
- **External Reference Only** — `src/External` contains vendored upstream source code provided purely as reference/context material. It is **read-only**: never modify, "fix", refactor, or restructure anything inside it. Do not treat it as owned code.

---

## 🛑 STRATEGY: Legacy Coexistence & Compilation

- **Core Isolation** — All core mechanics (combat, turn management, stats, formulas) must live in external plain C# Class Libraries outside the presentation layer. These assemblies must not reference any game engine dependencies.
- **No Raw Sources in Presentation** — Never place raw `.cs` files belonging to the core domain or tests directly inside the presentation layer assets folder.
- **Automated Delivery Target** — Every core project must feature a post-build target that automatically compiles and copies its compiled binaries (`.dll` and `.pdb`) to a single flat internal plugins directory within the presentation layer.
- **Minimal Legacy Diff** — Existing legacy code stays as-is. When a task touches a legacy file, make the smallest change required; no opportunistic cleanup, re-styling, or refactoring of old code. Extract logic into core modules only when the current task actually requires it. Keep commit diffs focused.

---

## 🛠️ Compilation & Workflow Invariants

- **Verification** — Domain projects must not contain references to game engine binaries or editor assemblies. Define local pure C# primitives or interfaces if spatial types are required.
- **No Cross-Pollination** — AI agents must never create raw `.cs` files belonging to the domain layer anywhere inside the presentation layer. All core logic additions happen strictly within the dedicated source directory.
- **Unity Compile Check (for AI agents)** — after changing code in the presentation layer (`Assets\Scripts`) or editor scripts (`Assets\Editor`), agents must verify that the scripts compile in the target Unity editor: run `pwsh tools\compile-check.ps1` (batch-mode import + compilation, parses the log; no player build). A full standalone build is `pwsh tools\build-game.ps1`, launching the built game is `pwsh tools\run-game.ps1` / `pwsh tools\dev-run.ps1`. `compile-check.ps1` needs the project closed in the editor (checks `Temp\UnityLockfile`); use `-Provision` to deliver the Lan transport plugins first when they are absent from `Assets\Plugins\Internal` (they are gitignored). `compile-check.ps1` also runs `tools\check-script-references.ps1`, which fails when scenes/prefabs reference script GUIDs that do not resolve to a committed `.meta`.
- **Never let Unity regenerate `.meta` files** — scene/prefab components bind to scripts by GUID. If a script's `.meta` is deleted, missing, or regenerated (Unity assigns a new guid), every scene/prefab reference breaks silently (components dropped, NullReferenceExceptions, black screens). Preserve metas byte-for-byte on moves/restructures (`git mv`, never delete+recreate); if Unity reports "Imported GUID ... new" for an existing script, restore the original `.meta`. Keep `!**/[Aa]ssets/**/*.meta` in `.gitignore`; every new `.cs` must be committed with its `.meta`. Before committing after a Unity version migration or large restructure, run `pwsh tools\check-script-references.ps1` on `unity` and `unity-2017`.
- **Commit Messages** — commit messages must be in English, start with a capital letter, and end with a period (for example: "Add lobby timeout handling.").
- **Pull Requests & Merges (human gate)** — work happens on per-task branches (`core/<slice>`); a pull request into `main` may be opened once the work is done and auto-checks pass, but **the PR is merged only after the user explicitly approves** (they verify in the game first). Never squash-merge `core/*` into `main` without that approval. `master` is never touched.

---

## Coding Conventions & Invariants (Non-negotiable for New Code)

### I. Architecture & Dependencies

- **Constructor DI** — All dependencies, settings, and mutable states must be injected via constructors. Global singletons, global engine object lookups, hidden mutable statics, and hardcoded instantiations inside business logic are strictly forbidden.
- **Autonomous Labels (OCP)** — Infrastructure layers (logging, storage, transport) must not know consumer categories or define their vocabulary. Component owners define their own identity via local constants.

### II. Domain Modeling & Types

- **No Anemic Domain Model** — Rich domain entities must encapsulate state and mutate only through validating methods with zero public setters.
- **DTO Exception** — Plain Data Transfer Objects used purely for shifting data across boundaries without validation constraints are exempt from the rich model rule.
- **Polymorphism Over Branching** — Using enums or switches for branching or dispatching behavior is forbidden. Use polymorphic patterns instead (Strategies, State classes, data resources).
- **String IDs** — All asset, event, effect, and resource IDs must be strings, validated at content load time.
- **No Tuples or Weak Collections** — Tuple and ValueTuple types are forbidden; use named classes instead. Avoid dictionaries unless strictly required for performance.

### III. Clean Code & Documentation

- **One Public Type Per File** — Every file must contain exactly one public type. The file name must match the type name exactly.
- **Naming (new code only)** — New code follows standard C# conventions: private fields and private constants use `_camelCase` (underscore prefix), local variables use `camelCase`, public members and methods use `PascalCase`. Existing legacy code is exempt and left untouched.
- **No Magic Strings** — Use the `nameof(...)` operator for code identifiers to ensure refactoring resilience. Use strongly-typed named constants at the definition site for external wire, storage, or configuration keys.
- **Mandatory XML Documentation** — All public types and members must have clear `///` XML comments.
- **KISS/YAGNI** — Avoid over-abstracting code; it must be highly readable top-down. Don't add explanatory comments unless explicitly asked.
- **Design Docs & Knowledge** — Place all high-level designs, architecture documentation, and context details in a centralized documentation directory. Keep global rules universal.

### IV. Error Handling & Feature Management

- **No Exceptions in Core** — Core assemblies must never throw exceptions for control flow or business errors. Use `Result` or `Result<T>` functional types for all error propagation.
- **Feature Flags** — Wrap new or changed user-visible behavior behind an enablement flag only if it is a major feature or old behavior is worth preserving. Ask before flagging small changes.

### V. Structural Evolution

- **Module Growth Lifecycle** — Features, entities, and use cases start flat as a single file in a general folder. Once a feature expands beyond one public type, it must be promoted to a standalone top-level module (its own folder and namespace), never hidden inside subfolders. Further growth promotes it to a dedicated assembly.

---

## 📚 Documentation: Required Reading & Maintenance

Before planning or editing, read the relevant documents from `src\docs\`:
- `ARCHITECTURE.md` — architecture, code structure, god-classes, version;
- `KNOWN_ISSUES.md` — architectural debt and known issues (do not make them worse);
- `CHANGELOG.md` — change log by version (current version at the top of the file);
- `EXTRACTION_PLAN.md` — core-extraction plan: `unity\` (active) + `unity-2017\` (legacy) + shared pure C# core in `src\`;
- `TESTING.md` — manual in-game verification checklists; must be kept in sync with behavior changes;
- `INDEX.md` — doc map: which document answers which question (entities, execution, wishlist `FEATURE_*.md`).

Read only what the task relates to. Legacy edits stay minimal; `src\External\` is read-only.

**Maintenance rule:** if a code change affects a documented fact (paths/structure, god-classes, version, public APIs, new modules, dependencies), update the corresponding document in the same commit. If a code change affects game behavior, add/update the relevant section in `TESTING.md` (what to verify) in the same commit. Do not document internals or cosmetics; `CHANGELOG.md` only for user-visible changes.
