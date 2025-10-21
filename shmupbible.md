## wipshmup2 - a shmup game inspired by Cho Ren Sha 68K, 1942, the TouHou Project series and Galaga/Galaxian.

Credits: Harrison Allen for the base of my own CRT shader which is very heavily modified from his which can be found @ (https://godotshaders.com/shader/crt-with-luminance-preservation/)

Kody Gentry for the base of my own dithering "shader" (my version isn't really a shader more of an effect) which can be found @ (https://github.com/kodygentry/godot-dot-shader)

this so far is not used.

"saavane" for the background music (the music is under the pixabay license (https://pixabay.com/service/license-summary/)) it can be found @ (https://pixabay.com/music/synthwave-retro-waves-139640/)

This song is now removed.

controls: arrow keys to move and space to shoot

## Architecture

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                    GODOT ENGINE                                        │
│                                                                                        │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│  │                              MAIN SCENE (Main.gd)                               │   │
│  │                            Game Loop & Coordination                             │   │
│  │                                                                                 │   │
│  │  ┌────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐       │   │
│  │  │   GameViewport     │  │        HUD          │  │    Post-Processing  │       │   │
│  │  │   (1920*1080)      │  │   (CanvasLayer)     │  │     Pipeline        │       │   │
│  │  │                    │  │                     │  │                     │       │   │
│  │  │  ┌──────────────┐  │  │  ┌───────────────┐  │  │  ┌───────────────┐  │       │   │
│  │  │  │   PLAYER     │  │  │  │  Score/Lives  │  │  │  │               │  │       │   │
│  │  │  │ (Player.gd)  │  │  │  │  FPS Counter  │  │  │  │               │  │       │   │
│  │  │  │              │  │  │  │  Game Over    │  │  │  │               │  │       │   │
│  │  │  │ ┌─────────┐  │  │  │  │  Popups       │  │  │  │               │  │       │   │
│  │  │  │ │Movement │  │  │  │  └───────────────┘  │  │  │               │  │       │   │
│  │  │  │ │Shooting │  │  │  │                     │  │  └───────────────┘  │       │   │
│  │  │  │ │Hit Det. │  │  │  │                     │  │                     │       │   │
│  │  │  │ │Invuln.  │  │  │  │                     │  │                     │       │   │
│  │  │  │ └─────────┘  │  │  │                     │  │                     │       │   │
│  │  │  └──────────────┘  │  │                     │  │                     │       │   │
│  │  │                    │  │                     │  │                     │       │   │
│  │  │  ┌──────────────┐  │  │                     │  │                     │       │   │
│  │  │  │   ENEMIES    │  │  │                     │  │                     │       │   │
│  │  │  │ Container    │  │  │                     │  │                     │       │   │
│  │  │  │              │  │  │                     │  │                     │       │   │
│  │  │  │ ┌─────────┐  │  │  │                     │  │                     │       │   │
│  │  │  │ │ Enemy   │  │  │  │                     │  │                     │       │   │
│  │  │  │ │Instances│  │  │  │                     │  │                     │       │   │
│  │  │  │ │(13 Types)│ │  │  │                     │  │                     │       │   │
│  │  │  │ │+ Bosses │  │  │  │                     │  │                     │       │   │
│  │  │  │ └─────────┘  │  │  │                     │  │                     │       │   │
│  │  │  └──────────────┘  │  │                     │  │                     │       │   │
│  │  │                    │  │                     │  │                     │       │   │
│  │  │  ┌──────────────┐  │  │                     │  │                     │       │   │
│  │  │  │   BULLETS    │  │  │                     │  │                     │       │   │
│  │  │  │ Container    │  │  │                     │  │                     │       │   │
│  │  │  │              │  │  │                     │  │                     │       │   │
│  │  │  │ ┌─────────┐  │  │  │                     │  │                     │       │   │
│  │  │  │ │ Player  │  │  │  │                     │  │                     │       │   │
│  │  │  │ │Bullets  │  │  │  │                     │  │                     │       │   │
│  │  │  │ │& Enemy  │  │  │  │                     │  │                     │       │   │
│  │  │  │ │Bullets  │  │  │  │                     │  │                     │       │   │
│  │  │  │ └─────────┘  │  │  │                     │  │                     │       │   │
│  │  │  └──────────────┘  │  │                     │  │                     │       │   │
│  │  └────────────────────┘  └─────────────────────┘  └─────────────────────┘       │   │
│  └─────────────────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────────────────────┐
│                                 AUTOLOAD SYSTEMS                                      │
│                              (Global Singletons)                                      │
│                                                                                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                        │
│  │  AudioManager   │  │                 │  │                 │                        │
│  │   (Audio.gd)    │  │                 │  │                 │                        │
│  │                 │  │                 │  │                 │                        │
│  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌─────────────┐ │                        │
│  │ │Procedural   │ │  │ │             │ │  │ │             │ │                        │
│  │ │Sound Effects│ │  │ │             │ │  │ │             │ │                        │
│  │ │Generation   │ │  │ │     N/A     │ │  │ │      N/A    │ │                        │
│  │ │             │ │  │ │             │ │  │ │             │ │                        │
│  │ │• Beeps      │ │  │ │             │ │  │ │             │ │                        │
│  │ │• Boops      │ │  │ │             │ │  │ │             │ │                        │
│  │ │• Explosions │ │  │ │             │ │  │ │             │ │                        │
│  │ │• Extends    │ │  │ │             │ │  │ │             │ │                        │
│  │ └─────────────┘ │  │ │             │ │  │ │             │ │                        │
│  └─────────────────┘  │ │             │ │  │ └─────────────┘ │                        │
│                       │ └─────────────┘ │  └─────────────────┘                        │
│                       └─────────────────┘                                             │
└───────────────────────────────────────────────────────────────────────────────────────┘
```

## Data Flow

```

     ┌─────────────┐              ┌──────────────┐              ┌─────────────┐
     │   INPUT     │              │   GAME       │              │   OUTPUT    │
     │             │              │   STATE      │              │             │
     │ ┌─────────┐ │   signals    │              │   updates    │ ┌─────────┐ │
     │ │Movement │ ├──────────────┤ ┌──────────┐ ├──────────────┤ │Visual   │ │
     │ │Keys     │ │              │ │Lives     │ │              │ │Elements │ │
     │ └─────────┘ │              │ │Score     │ │              │ └─────────┘ │
     │             │              │ │Bombs     │ │              │             │
     │ ┌─────────┐ │              │ │Game Over │ │              │ ┌─────────┐ │
     │ │Shooting │ ├──────────────┤ │          │ ├──────────────┤ │Audio    │ │
     │ │         │ │              │ └──────────┘ │              │ │Effects  │ │
     │ └─────────┘ │              │              │              │ └─────────┘ │
     │             │              │ ┌──────────┐ │              │             │
     │ ┌─────────┐ │              │ │Player    │ │              │ ┌─────────┐ │
     │ │Bomb     │ ├──────────────┤ │Position  │ ├──────────────┤ │Shader   │ │
     │ │(X Key)  │ │              │ │Enemy     │ │              │ │Effects  │ │
     │ └─────────┘ │              │ │Positions │ │              │ └─────────┘ │
     │             │              │ │Bullet    │ │              │             │
     │ ┌─────────┐ │              │ │Positions │ │              │             │
     │ │N/A      │ ├──────────────┤ └──────────┘ │              │             │
     │ │         │ │              │              │              │             │
     │ └─────────┘ │              └──────────────┘              │             │
     └─────────────┘                                            └─────────────┘
```

## Collision System

```

                    ┌────────────────────────────────────────┐
                    │            COLLISION GROUPS            │
                    │                                        │
                    │  ┌─────────────┐   ┌─────────────┐     │
                    │  │   PLAYER    │   │   ENEMY     │     │
                    │  │             │   │             │     │
                    │  │ ┌─────────┐ │   │ ┌─────────┐ │     │
                    │  │ │Hurtbox  │ │   │ │Body     │ │     │
                    │  │ │(Area2D) │ │   │ │(Area2D) │ │     │
                    │  │ └─────────┘ │   │ └─────────┘ │     │
                    │  └─────────────┘   └─────────────┘     │
                    │         │                 │            │
                    │         │     COLLISION   │            │
                    │         └─────────────────┘            │
                    │                                        │
                    │  ┌─────────────┐   ┌─────────────┐     │
                    │  │PLAYER_BULLET│   │ENEMY_BULLET │     │
                    │  │             │   │             │     │
                    │  │ ┌─────────┐ │   │ ┌─────────┐ │     │
                    │  │ │Hitbox   │ │   │ │Hitbox   │ │     │
                    │  │ │(Area2D) │ │   │ │(Area2D) │ │     │
                    │  │ └─────────┘ │   │ └─────────┘ │     │
                    │  └─────────────┘   └─────────────┘     │
                    │         │                 │            │
                    │         │   COLLISION     │            │
                    │         └─────────────────┘            │
                    └────────────────────────────────────────┘
```

## Signal Flow Chart

```

    Player.hit ──────────────────────► Main._on_player_hit()
       │                                       │
       ▼                                       ▼
   Lives Decrease                        Audio Effect
                                              │
    Enemy.killed ────────────────────► StageController.enemy_killed
       │                                       │
       ▼                                       ▼
   Points Award ──────────────────────► Main._on_enemy_killed()
       │                                       │
       ▼                                       ▼
   Score Update                           RankManager Update

    Boss.defeated ───────────────────► StageController.boss_defeated
       │                                       │
       ▼                                       ▼
   Stage Progress                         HUD Popup

    Engine.get_frames_per_second() ──────► HUD._process()
                                              │
                                              ▼
                                        FPS Display (000 format)
```

## Enemy Behavior Types

```

    ┌──────────────────────────────────────────────────────────────────────────────────┐
    │                               REGULAR ENEMIES                                    │
    │                                                                                  │
    │  Type 01: Straight-Shot      │ Type 02: Fan            │                         │
    │   (Shoots forward            │     (They spin and      │                         │
    │	                           │      shoot bullets in   │                         │
	│							   │	       a spiral.)    │                         │
	│   Only does that and they    │                         │                         │
	│   come in a v shape.)        │                         │                         │
    │                              │                         │                         │
    │                              │                         │                         │
    │                              │                         │                         │
    │                                                                                  │
    │                                     BOSSES                                       │
    │                                                                                  │
    │  Ultra-Zeplin - Is a bigger version of the normal "Fan Enemy that shoots faster" │
    │                                                                                  │
    └──────────────────────────────────────────────────────────────────────────────────┘
```

---

### Core Infrastructure

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                              EVENT-DRIVEN ARCHITECTURE                                 │
│                                                                                        │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│  │                              AUTOLOAD SYSTEMS                                   │   │
│  │                              (Global Singletons)                                │   │
│  │                                                                                 │   │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │   │
│  │  │   EventBus      │  │   GameState     │  │ EntityFactory   │  │     N/A     │ │   │
│  │  │ (Event System)  │  │ (Game State)    │  │ (Spawn System)  │  │             │ │   │
│  │  │                 │  │                 │  │                 │  │             │ │   │
│  │  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌─────────┐ │ │   │
│  │  │ │Game Events  │ │  │ │Player State │ │  │ │Player Spawn │ │  │ │         │ │ │   │
│  │  │ │Combat Events│ │  │ │             │ │  │ │Enemy Spawn  │ │  │ │         │ │ │   │
│  │  │ │Visual Events│ │  │ │Streak System│ │  │ │Bullet Spawn │ │  │ │         │ │ │   │
│  │  │ │Audio Events │ │  │ │             │ │  │ │             │ │  │ │         │ │ │   │
│  │  │ └─────────────┘ │  │ └─────────────┘ │  │ └─────────────┘ │  │ └─────────┘ │ │   │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────┘ │   │
│  │                                                                                 │   │
│  │                                                                                 │   │
│  └─────────────────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### Component-Based Enemy System

```
┌───────────────────────────────────────────────────────────────────────────────────────┐
│                            ENEMY BEHAVIOR COMPONENTS                                  │
│                                                                                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │MovementBehavior │  │ AttackBehavior  │  │                 │  │                 │   │
│  │ (Base Class)    │  │ (Base Class)    │  │         N/A     │  │         N/A     │   │
│  │                 │  │                 │  │                 │  │                 │   │
│  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌─────────────┐ │   │
│  │ │Speed        │ │  │ │Fire Rate    │ │  │ │             │ │  │ │             │ │   │
│  │ │Direction    │ │  │ │Bullet Speed │ │  │ │             │ │  │ │             │ │   │
│  │ │             │ │  │ │Damage       │ │  │ │             │ │  │ │             │ │   │
│  │ │             │ │  │ │Patterns     │ │  │ │             │ │  │ │             │ │   │
│  │ └─────────────┘ │  │ └─────────────┘ │  │ └─────────────┘ │  │ └─────────────┘ │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                                       │
└───────────────────────────────────────────────────────────────────────────────────────┘
```

### Event Flow Architecture

```
┌───────────────────────────────────────────────────────────────────────────────────────┐
│                              EVENT FLOW SYSTEM                                        │
│                                                                                       │
│  ┌─────────────────┐              ┌─────────────────┐              ┌─────────────┐    │
│  │   INPUT         │              │   EVENTBUS      │              │   SYSTEMS   │    │
│  │                 │              │                 │              │             │    │
│  │ ┌─────────────┐ │   events     │ ┌─────────────┐ │   events     │ ┌─────────┐ │    │
│  │ │Player Input │ ├────────────► │ │Game Events  │ ├─────────────►│ │Combat   │ │    │
│  │ │Movement     │ │              │ │Combat Evts  │ │              │ │Visual   │ │    │
│  │ │Shooting     │ │              │ │Visual Evts  │ │              │ │Audio    │ │    │
│  │ │Bomb         │ │              │ │Audio Evts   │ │              │ │Rank     │ │    │
│  │ └─────────────┘ │              │ └─────────────┘ │              │ └─────────┘ │    │
│  └─────────────────┘              └─────────────────┘              └─────────────┘    │
└───────────────────────────────────────────────────────────────────────────────────────┘
```


### File Structure

```
scripts/
├─ autoload/              # Centralized Singletons
│  ├─ GameState.c#       # Game state management
├─ systems/               # Game Systems
│  ├─ CombatSystem.c#    # Combat logic
├─ controllers/           # Input Handlers
│  └─ PlayerController.c#       # Player input
├─ components/           # Reusable Behaviors
│  └─ behaviors/
│     ├─ MovementBehavior.c#    # Base movement
│     ├─ AttackBehavior.c#      # Base attack
├─ stages/               # Stage Definitions
│  ├─ StageDefinition.c# # Stage data
│  └─ BossEncounter.c#   # Boss encounter data
├─ data/                 # Templates and Definitions
│  ├─ EnemyTemplate.c#   # Enemy template
│  ├─ BossTemplate.c#    # Boss template
└─ [Game Modes]          # Game Mode Classes
   ├─ EndlessMode.c#    # Endless mode
```

*Thanks for reading! :)*
