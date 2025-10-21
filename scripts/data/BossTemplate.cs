using Godot;
using System;

/// <summary>
/// Boss Template Data - Configuration resource for boss encounters
/// 
/// Similar to EnemyTemplate but specialized for boss battles, which have different
/// characteristics than regular enemies (higher health, complex patterns, stage markers).
/// 
/// Boss: "Ultra-Zeppelin" (from shmupbible)
///   - Description: "A bigger version of the normal Fan Enemy that shoots faster"
///   - Health: 100 (vs regular enemy's 1)
///   - PointValue: 10000 (vs regular enemy's 100)
///   - FireRate: 0.5 (shoots twice as fast)
///   - AttackPattern: "Spiral" (same as Type 02 Fan, but faster/denser)
/// 
/// Boss Design Philosophy (classic shmups):
///   - Bullet Hell: Dense, complex patterns that fill the screen
///   - Phases: Bosses often change attack patterns at health thresholds
///   - Spectacle: Larger sprites, dramatic music, screen effects
///   - Fairness: Despite intensity, patterns are learnable with practice
/// 
/// Signal Flow on Boss Defeat (from shmupbible):
///   Boss.defeated -> StageController.boss_defeated -> HUD Popup
///   Boss -> GameState.AddScore(10000) -> Potentially advance stage
/// 
/// Future Enhancements:
///   - Phase system: Different patterns at 75%, 50%, 25% health
///   - Attack pattern arrays: Rotate through multiple patterns
///   - Weak points: Specific hitboxes for extra damage
///   - Time bonuses: Extra points for fast defeats
/// </summary>
public partial class BossTemplate : Resource
{
    /// <summary>Boss name for dramatic reveals and HUD display (e.g., "Ultra-Zeppelin", "Death Star")</summary>
    [Export] public string BossName { get; set; } = "Ultra-Zeppelin";
    
    /// <summary>Boss health pool - typically 50-200 for epic multi-phase battles (100x stronger than fodder)</summary>
    [Export] public int Health { get; set; } = 100;
    
    /// <summary>Massive point award on defeat - stage completion bonus (10000 = 100x regular enemy)</summary>
    [Export] public int PointValue { get; set; } = 10000;
    
    /// <summary>Boss movement speed - typically slower than regular enemies for predictability (50 vs 100)</summary>
    [Export] public float Speed { get; set; } = 50f;
    
    /// <summary>Very fast firing rate for bullet hell intensity - creates dense patterns (0.5s = twice as fast as regular)</summary>
    [Export] public float FireRate { get; set; } = 0.5f;
    
    /// <summary>Fast bullet speed to increase pressure - harder to dodge (200 vs 150 for regular enemies)</summary>
    [Export] public float BulletSpeed { get; set; } = 200f;
    
    /// <summary>Boss attack pattern identifier - typically complex patterns like "Spiral", "BulletHell", "Aimed"</summary>
    [Export] public string AttackPattern { get; set; } = "Spiral";
}

