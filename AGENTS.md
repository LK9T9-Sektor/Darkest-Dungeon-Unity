# AGENTS.md — Agent Rules

> Single source of truth for AI assistants working on this codebase.

---

## 📁 Repository Structure Invariants

- **Presentation Layer** — The Unity editor environment and asset folder contain views, engines, and platform-specific assets. No pure domain logic is allowed here.
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
- `GAME_ARCHITECTURE.md` — architecture, code structure, god-classes, version;
- `KNOWN_ISSUES.md` — architectural debt and known issues (do not make them worse);
- `CHANGELOG.md` — change log by version (current version at the top of the file).

Read only what the task relates to. Legacy edits stay minimal; `src\External\` is read-only.

**Maintenance rule:** if a code change affects a documented fact (paths/structure, god-classes, version, public APIs, new modules, dependencies), update the corresponding document in the same commit. Do not document internals or cosmetics; `CHANGELOG.md` only for user-visible changes.
