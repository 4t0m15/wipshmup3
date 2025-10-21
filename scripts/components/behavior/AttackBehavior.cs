using Godot;
using System;

/// <summary>
/// Base Attack Behavior Component - Pluggable enemy attack patterns
/// 
/// This is the second core component in the Component-Based Enemy System from the
/// shmupbible architecture. While MovementBehavior controls WHERE enemies move,
/// AttackBehavior controls HOW enemies shoot.
/// 
/// Enemy Types and Their Attack Patterns (from shmupbible):
///   - Type 01 "Straight-Shot": ForwardAttack (shoots straight down in V-formation)
///   - Type 02 "Fan": SpiralAttack (spins and shoots spiraling bullet patterns)
///   - Boss "Ultra-Zeppelin": Fast SpiralAttack (larger, faster spiral patterns)
/// 
/// Architecture Benefits:
///   - Modularity: Mix any attack pattern with any movement pattern
///   - Scalability: Easy to add new patterns (aimed shot, burst fire, laser, etc.)
///   - Balance: Tune difficulty by adjusting FireRate and BulletSpeed
///   - Variety: Create unique enemies by combining components
/// 
/// Usage Pattern:
///   1. Enemy adds AttackBehavior as child node
///   2. Enemy calls Update() each frame to advance timer
///   3. Enemy checks CanFire() to see if ready to shoot
///   4. When ready, Enemy calls GetBulletDirections() for bullet pattern
///   5. Enemy spawns bullets in those directions
/// 
/// This follows the Strategy Pattern, allowing bullet patterns to be
/// swapped without modifying the Enemy class itself.
/// </summary>
public partial class AttackBehavior : Node
{
    /// <summary>Time in seconds between shots - lower = faster firing (0.5 = 2 shots/sec)</summary>
    [Export] public float FireRate { get; set; } = 1.0f;
    
    /// <summary>Speed of spawned bullets in pixels per second</summary>
    [Export] public float BulletSpeed { get; set; } = 150f;
    
    /// <summary>Damage dealt per bullet - currently unused but available for future systems</summary>
    [Export] public int Damage { get; set; } = 1;
    
    /// <summary>Internal timer accumulating time since last shot</summary>
    protected float _fireTimer = 0f;
    
    /// <summary>
    /// Update the attack behavior timer
    /// 
    /// Called by Enemy every frame to advance the internal fire timer.
    /// Subclasses can override to add additional per-frame logic like
    /// rotating the pattern, tracking player position, or charging attacks.
    /// </summary>
    public virtual void Update(float delta)
    {
        _fireTimer += delta;
    }
    
    /// <summary>
    /// Check if enough time has elapsed to fire again
    /// 
    /// Returns true when _fireTimer >= FireRate, then resets the timer.
    /// This creates consistent firing intervals regardless of frame rate.
    /// 
    /// Example: With FireRate = 2.0, enemy fires every 2 seconds
    /// </summary>
    public virtual bool CanFire()
    {
        if (_fireTimer >= FireRate)
        {
            _fireTimer = 0f;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get the bullet direction vectors for this attack pattern
    /// 
    /// Parameters:
    ///   - position: Enemy's current position (used for aimed shots, etc.)
    /// 
    /// Returns:
    ///   - Array of normalized direction vectors (one per bullet to spawn)
    /// 
    /// Base implementation fires a single bullet downward.
    /// Subclasses override to create complex patterns (spiral, spread, aimed, etc.)
    /// </summary>
    public virtual Vector2[] GetBulletDirections(Vector2 position)
    {
        return new Vector2[] { Vector2.Down };
    }
}

/// <summary>
/// Forward Attack - Type 01 "Straight-Shot" enemy pattern
/// 
/// The simplest attack: shoots a single bullet straight downward.
/// This is the classic shmup enemy bullet from games like 1942 and Galaga.
/// 
/// Used by:
///   - Type 01 enemies in V-formation
///   - Early game enemies
///   - Default fallback pattern
/// 
/// Gameplay Characteristics:
///   - Predictable: Players can easily dodge by moving left/right
///   - Fair: No surprise bullet angles
///   - Scalable: Difficulty comes from enemy quantity and formation
/// </summary>
public partial class ForwardAttack : AttackBehavior
{
    /// <summary>
    /// Returns single downward direction
    /// 
    /// Creates the classic vertical bullet that requires horizontal dodging.
    /// Position parameter is ignored since direction is always down.
    /// </summary>
    public override Vector2[] GetBulletDirections(Vector2 position)
    {
        return new Vector2[] { Vector2.Down };
    }
}

/// <summary>
/// Spiral Attack - Type 02 "Fan" enemy and Boss "Ultra-Zeppelin" pattern
/// 
/// Creates a rotating radial pattern of bullets, inspired by TouHou Project's
/// signature bullet hell patterns. The pattern continuously rotates, creating
/// a mesmerizing spiral as bullets are fired at intervals.
/// 
/// Pattern Mathematics:
///   - BulletCount bullets arranged in a circle (360° / BulletCount apart)
///   - Rotation increases over time, spinning the entire pattern
///   - Each bullet travels in its assigned direction
/// 
/// Used by:
///   - Type 02 "Fan" enemies (8 bullets, moderate speed)
///   - Boss "Ultra-Zeppelin" (more bullets, faster firing)
/// 
/// Gameplay Characteristics:
///   - Challenging: Requires spatial awareness and timing
///   - Fair: Pattern is visible and consistent, players can learn it
///   - Dramatic: Looks impressive and creates "bullet hell" aesthetic
/// 
/// Tuning Parameters:
///   - BulletCount: More = denser pattern (8-16 typical)
///   - FireRate: Lower = more dangerous (bullets overlap less)
///   - Rotation speed (2.0): Faster = harder to predict safe zones
/// </summary>
public partial class SpiralAttack : AttackBehavior
{
    /// <summary>Number of bullets per shot - forms a complete circle when fired</summary>
    [Export] public int BulletCount { get; set; } = 8;
    
    /// <summary>Current rotation angle in radians - continuously increases to create spiral</summary>
    private float _rotation = 0f;
    
    /// <summary>
    /// Update rotation angle in addition to fire timer
    /// 
    /// The rotation advances at 2 radians per second, causing the pattern
    /// to spin. This makes each shot's bullets appear at different angles,
    /// creating the spiral/fan visual effect as they spread out.
    /// </summary>
    public override void Update(float delta)
    {
        base.Update(delta);
        _rotation += delta * 2f; // Rotate the pattern
    }
    
    /// <summary>
    /// Generate radial bullet directions with current rotation
    /// 
    /// Algorithm:
    ///   1. Calculate angle between bullets: 360° / BulletCount
    ///   2. For each bullet index:
    ///      a. Calculate base angle (evenly distributed around circle)
    ///      b. Add current rotation offset for spiral effect
    ///      c. Convert angle to direction vector using cos/sin
    /// 
    /// Math Tau (τ):
    ///   Mathf.Tau = 2π ≈ 6.28 radians = 360 degrees
    ///   Using Tau makes circular math more intuitive than 2*Pi
    /// 
    /// Example with BulletCount=8:
    ///   - Bullet 0: 0° + rotation
    ///   - Bullet 1: 45° + rotation
    ///   - Bullet 2: 90° + rotation
    ///   - ... and so on in 45° increments
    /// 
    /// As rotation increases over time, all bullets rotate together,
    /// creating the spinning spiral pattern characteristic of Type 02 enemies.
    /// </summary>
    public override Vector2[] GetBulletDirections(Vector2 position)
    {
        var directions = new Vector2[BulletCount];
        float angleStep = Mathf.Tau / BulletCount;
        
        for (int i = 0; i < BulletCount; i++)
        {
            float angle = angleStep * i + _rotation;
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        
        return directions;
    }
}

