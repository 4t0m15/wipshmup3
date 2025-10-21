using Godot;
using System;

/// <summary>
/// Enemy Entity - Component-based enemy system with behavior patterns
/// 
/// This enemy implementation follows the Component-Based Architecture described in
/// the shmupbible, where enemies are composed of modular, reusable behaviors rather
/// than monolithic enemy classes. This design supports the various enemy types:
/// 
/// Enemy Types (from shmupbible):
///   - Type 01 "Straight-Shot": Uses StraightMovement + ForwardAttack
///     * Moves straight down
///     * Shoots forward in V-formation groups
///   - Type 02 "Fan": Uses StraightMovement + SpiralAttack
///     * Moves down while spinning
///     * Shoots spiral/fan bullet patterns
///   - Boss "Ultra-Zeppelin": Larger version with faster SpiralAttack
/// 
/// Component Architecture:
///   - MovementBehavior: Defines how enemy moves (straight, sine wave, circle, etc.)
///   - AttackBehavior: Defines how enemy shoots (forward, spiral, aimed, etc.)
/// 
/// Collision System:
///   - CollisionLayer = 8 (ENEMY layer)
///   - CollisionMask = 2 (detects PLAYER_BULLET layer)
///   - Added to "enemy" group for identification
/// 
/// Signal Flow:
///   Enemy.EnemyKilled(points) -> GameState.AddScore(points) -> Main.OnScoreChanged()
/// 
/// This component-based design allows for mixing and matching behaviors to create
/// diverse enemy types without code duplication, following the composition-over-inheritance principle.
/// </summary>
public partial class Enemy : Area2D
{
    /// <summary>Hit points - enemy is destroyed when health reaches 0</summary>
    [Export] public int Health { get; set; } = 1;
    
    /// <summary>Points awarded when this enemy is destroyed - contributes to score and rank system</summary>
    [Export] public int PointValue { get; set; } = 100;
    
    /// <summary>Bullet prefab scene to instantiate when firing - shared between enemy types</summary>
    [Export] public PackedScene BulletScene { get; set; }
    
    /// <summary>Pluggable movement behavior component - determines movement pattern (straight, sine, etc.)</summary>
    private MovementBehavior _movementBehavior;
    
    /// <summary>Pluggable attack behavior component - determines firing pattern (forward, spiral, etc.)</summary>
    private AttackBehavior _attackBehavior;
    
    /// <summary>Cached viewport size for off-screen culling detection</summary>
    private Vector2 _screenSize;

    /// <summary>Signal emitted when enemy is destroyed - carries point value for scoring system</summary>
    [Signal] public delegate void EnemyKilledEventHandler(int points);
    
    /// <summary>
    /// Initialize enemy when entering the scene tree
    /// 
    /// Component Setup Strategy:
    /// 1. Cache viewport size for boundary checks
    /// 2. Configure collision system:
    ///    - Layer 8 (ENEMY) for detection by player bullets
    ///    - Mask 2 (PLAYER_BULLET) to detect incoming fire
    ///    - Join "enemy" group for type identification
    /// 3. Load or create MovementBehavior component:
    ///    - First tries to find existing child node
    ///    - Falls back to creating default StraightMovement if none found
    /// 4. Load or create AttackBehavior component:
    ///    - First tries to find existing child node
    ///    - Falls back to creating default ForwardAttack if none found
    /// 5. Connect collision signal for damage detection
    /// 
    /// This flexible initialization allows enemies to be configured either:
    ///   A) In the scene file with pre-attached behavior nodes (preferred)
    ///   B) Programmatically with default behaviors as fallback
    /// 
    /// The component pattern means we can swap behaviors at runtime or define
    /// enemy variants entirely through scene configuration without code changes.
    /// </summary>
    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;

        // Set collision
        CollisionLayer = 8; // Enemy layer
        CollisionMask = 2; // Player bullet layer
        AddToGroup("enemy");
        
        // Set up behaviors
        _movementBehavior = GetNodeOrNull<MovementBehavior>("MovementBehavior");
        if (_movementBehavior == null)
        {
            _movementBehavior = new StraightMovement { Speed = 100f };
            AddChild(_movementBehavior);
        }
        
        _attackBehavior = GetNodeOrNull<AttackBehavior>("AttackBehavior");
        if (_attackBehavior == null)
        {
            _attackBehavior = new ForwardAttack { FireRate = 2.0f, BulletSpeed = 150f };
            AddChild(_attackBehavior);
        }
        
        AreaEntered += OnAreaEntered;
    }
    
    /// <summary>
    /// Update enemy behavior every frame - movement, attacking, and culling
    /// 
    /// Update Sequence:
    /// 1. Movement Phase:
    ///    - Query MovementBehavior for velocity delta
    ///    - Apply to current position (frame-rate independent)
    ///    - Behavior determines pattern (straight, sine wave, circle, etc.)
    /// 
    /// 2. Attack Phase:
    ///    - Update AttackBehavior's internal timer
    ///    - Check if ready to fire (based on FireRate)
    ///    - If ready, call Shoot() to spawn bullet pattern
    /// 
    /// 3. Culling Phase:
    ///    - Check if enemy moved off-screen (Y > screenHeight + 100)
    ///    - Destroy enemy if beyond visible bounds to prevent memory leak
    ///    - 100px buffer ensures enemies fully exit before destruction
    /// 
    /// This update loop demonstrates the Component-Based Architecture in action:
    /// the Enemy entity delegates behavior to specialized components, keeping
    /// this class focused solely on coordination rather than behavior implementation.
    /// </summary>
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        
        // Update movement
        if (_movementBehavior != null)
        {
            Position += _movementBehavior.CalculateMovement(dt, Position);
        }
        
        // Update attack
        if (_attackBehavior != null)
        {
            _attackBehavior.Update(dt);

            if (_attackBehavior.CanFire())
            {
                Shoot();
            }
        }
        
        // Remove if off-screen
        if (Position.Y > _screenSize.Y + 100)
        {
            QueueFree();
        }
    }
    
    /// <summary>
    /// Execute attack by spawning bullet pattern based on AttackBehavior
    /// 
    /// Shooting Process:
    /// 1. Query AttackBehavior for bullet directions array
    ///    - ForwardAttack returns single direction (down)
    ///    - SpiralAttack returns multiple directions in rotating pattern
    ///    - Custom behaviors can return any pattern (aimed, spread, etc.)
    /// 2. For each direction:
    ///    a. Instantiate bullet from BulletScene prefab
    ///    b. Position at enemy center (inherit enemy's world position)
    ///    c. Configure as enemy bullet (IsPlayerBullet = false)
    ///    d. Set direction and speed from AttackBehavior
    ///    e. Add to scene tree (parent's level, not as child of enemy)
    /// 
    /// Adding bullets to parent rather than as children ensures:
    ///   - Bullets persist if enemy is destroyed
    ///   - Bullets don't inherit enemy's transform
    ///   - Clean separation of entity lifetimes
    /// 
    /// This method showcases how AttackBehavior encapsulates bullet pattern logic,
    /// making it trivial to swap from single-shot to complex patterns like the
    /// Type 02 "Fan" enemy's spiral attack or boss bullet hell patterns.
    /// </summary>
    private void Shoot()
    {
        if (BulletScene == null) return;

        var directions = _attackBehavior.GetBulletDirections(Position);

        foreach (var direction in directions)
        {
            var bullet = BulletScene.Instantiate<Bullet>();
            bullet.Position = Position;
            bullet.Direction = direction;
            bullet.IsPlayerBullet = false;
            bullet.Speed = _attackBehavior.BulletSpeed;
            GetParent().AddChild(bullet);
        }
    }
    
    /// <summary>
    /// Handle collision with player bullets - damage and destruction logic
    /// 
    /// Damage Sequence:
    /// 1. Verify collision is with player bullet (group check)
    /// 2. Reduce health by 1 (damage amount from bullet is stored but not used yet)
    /// 3. Check if health depleted:
    ///    a. Emit EnemyKilled signal with point value
    ///    b. Award points through GameState singleton
    ///    c. Queue enemy for destruction (QueueFree)
    /// 
    /// Signal Flow on Death:
    ///   Enemy.EnemyKilled(100) -> [Any Listeners]
    ///   Enemy -> GameState.AddScore(100) -> GameState.ScoreChanged(newTotal)
    ///   -> Main.OnScoreChanged() -> Update HUD
    /// 
    /// Future Enhancement: The Damage property on bullets is available but not
    /// currently used. Could be implemented as "Health -= bullet.Damage" for
    /// variable damage bullets or power-ups.
    /// 
    /// The dual signaling (EnemyKilled + direct GameState.AddScore) provides
    /// both event notification for other systems (audio, particles, rank) and
    /// immediate score update. This follows the event-driven architecture while
    /// ensuring score is always updated.
    /// </summary>
    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("player_bullet"))
        {
            Health--;
            if (Health <= 0)
            {
                EmitSignal(SignalName.EnemyKilled, PointValue);

                // Award points
                var gameState = GetNodeOrNull<GameState>("/root/GameState");
                gameState?.AddScore(PointValue);

                QueueFree();
            }
        }
    }
}

