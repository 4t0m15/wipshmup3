# Roadmap

Making the shmupbible and guidelines.md a reality for wipshmup3

Updated: 2025-10-21 00:38 (local)

---

## Purpose
This roadmap translates the design described in:
- shmupbible.md (project root)
- .junie/guidelines.md (detailed architecture and flows)

into a concrete, buildable plan with phases, milestones, deliverables, and acceptance criteria. It maps each guideline section to work items in the existing Godot 4 + C# project (net9.0).

## High‑Level Goals
- Implement the core game loop and architecture described in the documents using Godot 4 C#.
- Realize the event-driven and component-based enemy systems.
- Deliver a minimal but complete vertical slice: player, bullets, two enemy types, scoring, HUD, and one boss.
- Establish repeatable processes for content (stages) and technical systems (autoloads, components, systems).
- Keep documentation living: guidelines.md and shmupbible.md updated alongside the code.

## Definitions and Assumptions
- Engine: Godot 4.x (C#).
- Runtime: .NET 8.0 (as per project files).
- Scenes and nodes follow Godot conventions; code lives under scripts/.
- File extension is .cs (not “.c#”). Any examples below use .cs.

## Traceability: From Document Sections to Implementation
- Architecture (Main Scene, HUD, Post-Processing) → scenes/Main.tscn, scenes/HUD.tscn, scenes/PostFX.tscn + scripts to wire them.
- Autoload Systems → scripts/autoload/EventBus.cs, GameState.cs, EntityFactory.cs, Audio.cs (or AudioManager.cs) as needed.
- Data Flow, Signal Flow → Signals and events (C# events or Godot signals) defined in EventBus and node scripts.
- Collision System → PhysicsLayer/Mask setup in project.godot and per-Node2D areas/bodies.
- Enemy Behavior Types → scripts/components/behavior/MovementBehavior.cs, AttackBehavior.cs, plus concrete behaviors.
- Core Systems → scripts/systems/CombatSystem.cs (hit logic, score, damage), Rank system (optional in later phase).
- Controllers → scripts/controllers/PlayerController.cs.
- Stages and Boss → scripts/stages/StageDefinition.cs, BossEncounter.cs; scenes for enemy/boss.
- Data/Templates → scripts/data/EnemyTemplate.cs, BossTemplate.cs (and JSON/Resources if desired).
- Game Modes → scripts/stages/GameModes/EndlessMode.cs.

## Phased Delivery Plan

### Phase 0 — Project Hygiene and Setup (1–2 days)
Deliverables:
- Confirm Godot 4 + C# build works (dotnet, Editor). Ensure project.godot is aligned.
- Establish collision layers/masks constants (Player, Enemy, PlayerBullet, EnemyBullet).
- Create a Developer Guide section in README.md explaining build/run.
Acceptance Criteria:
- Project opens and runs a blank main scene in editor and export template if used.

### Phase 1 — Autoloads and Event Backbone (2–3 days)
Deliverables:
- scripts/autoload/EventBus.cs: central events for gameplay and UI (PlayerHit, EnemyKilled, BossDefeated, ScoreChanged, LifeChanged, GameOver, BulletSpawned, EnemySpawned, etc.).
- scripts/autoload/GameState.cs: score, lives, bombs, rank (optional placeholder), pause state; emits events on change.
- scripts/autoload/EntityFactory.cs: spawn helpers for player, enemies, bullets; loads scenes/resources.
- scripts/autoload/Audio.cs (AudioManager): SFX methods (PlayShoot, PlayHit, PlayExplosion, Extend).
- Register these autoloads in project.godot.
Acceptance Criteria:
- Events can be published/subscribed from a simple test scene; debug logs confirm flow.

### Phase 2 — Main Scene + HUD (2–3 days)
Deliverables:
- scenes/Main.tscn: viewport root, child containers for Player, Enemies, Bullets, and PostFX.
- scenes/HUD.tscn (CanvasLayer): score, lives, game over message, optional FPS counter.
- scripts/HUD.cs: subscribes to GameState / EventBus to update values.
- Wire Main to GameState start/reset and to EntityFactory for initial player spawn.
Acceptance Criteria:
- Game runs: shows HUD 000 score format; lives displayed; pressing a stub input starts the game.

### Phase 3 — Player and Input (2–3 days)
Deliverables:
- scenes/Player.tscn with Area2D or CharacterBody2D, sprite, Hurtbox Area2D.
- scripts/controllers/PlayerController.cs: movement using arrow keys; shot on Space; optional Bomb on X (stub).
- Player fires bullets (EntityFactory spawns into Bullets container).
- Collision layers for player/hurtbox set according to docs.
Acceptance Criteria:
- Player moves within bounds; shoots forward bullets at a fixed rate; SFX on shoot.

### Phase 4 — Bullet System (Player and Enemy) (2 days)
Deliverables:
- scenes/PlayerBullet.tscn, scenes/EnemyBullet.tscn with Area2D + CollisionShape2D.
- scripts/components/Bullet.cs parametrized by speed, direction, damage, lifetime.
- Collision masks align with Collision System diagram.
Acceptance Criteria:
- Player bullets travel upward and despawn offscreen; Enemy bullets travel per pattern.

### Phase 5 — Enemy Framework (2–4 days)
Deliverables:
- scripts/components/behavior/MovementBehavior.cs (base), AttackBehavior.cs (base).
- Concrete behaviors for first two enemy types:
  - Type 01 Straight: linear movement + forward shot.
  - Type 02 Fan: rotational/spin movement + spiral shot.
- scenes/enemies/EnemyBase.tscn + EnemyType01.tscn, EnemyType02.tscn.
- scripts/data/EnemyTemplate.cs with stats used by scenes.
Acceptance Criteria:
- Spawning each enemy type results in expected movement and fire patterns; killing yields points.

### Phase 6 — Combat System and Collisions (2–3 days)
Deliverables:
- scripts/systems/CombatSystem.cs: hit detection callbacks, applying damage, death, score/life updates via GameState and EventBus.
- Standardized signal names per Signal Flow Chart.
- Basic explosion VFX placeholder and SFX.
Acceptance Criteria:
- Player bullets damage enemies; enemies can damage player; lives decrement and invulnerability frames if desired.

### Phase 7 — Stage and Spawn Control (2–3 days)
Deliverables:
- scripts/stages/StageDefinition.cs: wave timelines or simple coroutine-based spawn sequences.
- Spawner in Main that reads StageDefinition to create waves (V-shape for Type 01, etc.).
- Endless mode scaffold: scripts/stages/GameModes/EndlessMode.cs.
Acceptance Criteria:
- One complete stage loopable or an endless wave proving out systems; score increases; HUD reflects state.

### Phase 8 — Boss Encounter (2–4 days)
Deliverables:
- scripts/stages/BossEncounter.cs and scenes/boss/UltraZeplin.tscn.
- Boss attack patterns (faster fan/spiral); health bar on HUD.
- Emits BossDefeated → Stage progression.
Acceptance Criteria:
- Boss spawns after waves; can be defeated; triggers clear or next loop.

### Phase 9 — Post‑Processing and Visual Polish (2–4 days)
Deliverables:
- CRT shader and optional dithering effect integration (with credits kept in docs).
- Camera shake, simple particles for explosions/hits.
Acceptance Criteria:
- Toggleable post-processing pipeline works and does not impact gameplay performance significantly.

### Phase 10 — Audio and UX Polish (1–2 days)
Deliverables:
- Procedural SFX hooks or sample-based effects; volume controls.
- Game over and extend cues; pause handling.
Acceptance Criteria:
- Audio events correspond to actions reliably; mix levels acceptable.

### Phase 11 — Rank/Scoring Extensions (Optional) (2–3 days)
Deliverables:
- RankManager (within GameState or separate autoload) adjusts difficulty based on performance.
- Bonus systems (chains, extends) as per shmupbible aspirations.
Acceptance Criteria:
- Rank visibly affects enemy fire rate/density; scoring documented.

### Phase 12 — Documentation, QA, and Packaging (1–3 days)
Deliverables:
- Update shmupbible.md with implemented details and any deviations.
- Update .junie/guidelines.md to reflect reality and keep diagrams current.
- Add GIFs/screenshots to README.md showcasing features and controls.
- Create a simple test checklist; optionally automated sanity tests (debug logs, headless runs).
Acceptance Criteria:
- Docs are consistent with code; on-boarding a new contributor takes < 30 minutes.

## Milestones & Timeline (Flexible)
- M0 (Week 0): Phase 0
- M1 (Week 1): Phases 1–2
- M2 (Week 2): Phases 3–4
- M3 (Week 3): Phases 5–6
- M4 (Week 4): Phases 7–8 (Vertical Slice complete)
- M5 (Week 5): Phases 9–10
- M6 (Week 6): Phase 11 (optional), Phase 12

## Acceptance Criteria Summary
- Vertical slice: Player + bullets + two enemy types + scoring + HUD + one boss; runs at target framerate; collisions behave as specified; signals/events verified in logs.
- Documentation synchronized: diagrams and file structure reflect actual code.

## Risks and Mitigations
- Scope creep: Lock vertical slice before adding new enemy types; use backlog.
- Performance: Keep 320x180 viewport upscale to manage fill-rate; profile shaders.
- C# and Godot interop issues: Prefer Godot 4.2+; keep autoloads minimal and test in isolation.

## Backlog (Post‑Roadmap Ideas)
- Replays and input recording.
- Advanced bullet patterns and pattern editor.
- Multiple ships and shot types.
- Online leaderboards.

## Working Agreements
- Branching: feature/xyz → PR → main.
- Definition of Done: code + docs + test pass + demo GIF when UX-visible.
- Logging: prefix debug with [DEBUG_LOG] for testability.

## Implementation Checklist (Traceability)
- Autoloads: EventBus.cs, GameState.cs, EntityFactory.cs, Audio.cs
- Systems: CombatSystem.cs
- Controllers: PlayerController.cs
- Components: MovementBehavior.cs, AttackBehavior.cs, Bullet.cs
- Data/Templates: EnemyTemplate.cs, BossTemplate.cs
- Stages: StageDefinition.cs, BossEncounter.cs; GameModes/EndlessMode.cs
- Scenes: Main.tscn, HUD.tscn, Player.tscn, bullets, two enemies, one boss
- Collisions: Layers/masks set per diagram
- Signals/Events: Player.hit, Enemy.killed, Boss.defeated, Score/Lives updates
- Post‑FX: CRT + dithering (toggleable)
- Docs: Update shmupbible.md and guidelines.md after each milestone

— End of Roadmap —