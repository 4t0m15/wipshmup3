using Godot;
using System;

/// <summary>
/// Enemy Template Data - Configuration resource for enemy types
/// 
/// This Resource class serves as a data-driven template for defining enemy properties
/// without hardcoding values in scripts. It's part of the Entity Factory pattern from
/// the shmupbible architecture, enabling designers to create enemy variants through
/// the Godot editor rather than code changes.
/// 
/// Design Pattern: Template Method / Data-Driven Design
///   - Templates are saved as .tres resource files in the Godot project
///   - EntityFactory (future system) reads these templates to spawn enemies
///   - Same code supports unlimited enemy types through different templates
/// 
/// Enemy Types Defined by Templates:
///   - Type 01 "Straight-Shot": Health=1, Speed=100, MovementPattern="Straight", AttackPattern="Forward"
///   - Type 02 "Fan": Health=1, Speed=100, MovementPattern="Straight", AttackPattern="Spiral"
///   - Boss "Ultra-Zeppelin": Health=100, Speed=50, FireRate=0.5, AttackPattern="Spiral"
/// 
/// Benefits:
///   - Balancing: Designers can tune values without programmer intervention
///   - Iteration: Quick testing of different configurations
///   - Variation: Create enemy variants by duplicating and tweaking templates
///   - Data: Templates can be loaded from JSON/XML for modding support
/// 
/// This separates game data from game logic, a fundamental principle of
/// maintainable game architecture used in professional game development.
/// </summary>
public partial class EnemyTemplate : Resource
{
    /// <summary>Display name for this enemy type (e.g., "Basic Enemy", "Red Fighter", "Tank")</summary>
    [Export] public string EnemyName { get; set; } = "Basic Enemy";
    
    /// <summary>Hit points - how many player bullets needed to destroy (1 for fodder, 3-5 for mini-bosses)</summary>
    [Export] public int Health { get; set; } = 1;
    
    /// <summary>Points awarded on destruction - affects score and potentially rank/difficulty scaling</summary>
    [Export] public int PointValue { get; set; } = 100;
    
    /// <summary>Movement speed in pixels per second - typical range 50-200 (slower = easier to dodge)</summary>
    [Export] public float Speed { get; set; } = 100f;
    
    /// <summary>Time between shots in seconds - lower = more dangerous (typical: 1-3 seconds)</summary>
    [Export] public float FireRate { get; set; } = 1.0f;
    
    /// <summary>Bullet velocity in pixels per second - faster = harder to dodge (typical: 100-300)</summary>
    [Export] public float BulletSpeed { get; set; } = 150f;
    
    /// <summary>Movement behavior identifier: "Straight", "SineWave", "Circle", etc. - used by factory to instantiate correct behavior</summary>
    [Export] public string MovementPattern { get; set; } = "Straight";
    
    /// <summary>Attack behavior identifier: "Forward", "Spiral", "Aimed", etc. - used by factory to instantiate correct behavior</summary>
    [Export] public string AttackPattern { get; set; } = "Forward";
}

