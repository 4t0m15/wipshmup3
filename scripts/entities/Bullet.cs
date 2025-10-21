using Godot;
using System;

/// <summary>
/// Bullet Entity - Projectile system for both player and enemy bullets
/// 
/// This unified bullet class handles projectiles from both player and enemies,
/// using collision layers to determine friend/foe interactions. Inspired by
/// classic shmup bullet patterns from TouHou Project and Cho Ren Sha 68K.
/// 
/// Collision System Architecture:
///   - Player Bullets: Layer 2 (PLAYER_BULLET) -> Mask 8 (ENEMY)
///   - Enemy Bullets: Layer 4 (ENEMY_BULLET) -> Mask 1 (PLAYER)
/// 
/// This separation ensures:
///   - Player bullets only hit enemies
///   - Enemy bullets only hit the player
///   - Bullets don't collide with each other
/// 
/// Lifecycle:
///   1. Instantiated by Player or Enemy
///   2. Moves in specified direction at constant speed
///   3. Destroyed on collision or when leaving screen bounds
/// 
/// Performance Note: Bullets are frequently spawned/destroyed, so this class
/// is kept lightweight. Off-screen culling prevents memory leaks.
/// </summary>
public partial class Bullet : Area2D
{
    /// <summary>Bullet velocity in pixels per second - default 400 for player, varies for enemies</summary>
    [Export] public float Speed { get; set; } = 400f;
    
    /// <summary>Normalized direction vector - Up for player bullets, varies for enemy patterns</summary>
    [Export] public Vector2 Direction { get; set; } = Vector2.Up;
    
    /// <summary>Determines collision behavior: true = player bullet, false = enemy bullet</summary>
    [Export] public bool IsPlayerBullet { get; set; } = true;
    
    /// <summary>Damage value dealt on hit - used by target to reduce health</summary>
    [Export] public int Damage { get; set; } = 1;
    
    /// <summary>Cached viewport size for efficient off-screen detection</summary>
    private Vector2 _screenSize;
    
    /// <summary>
    /// Initialize bullet when entering the scene tree
    /// 
    /// Configuration Steps:
    /// 1. Cache viewport size for off-screen boundary checks
    /// 2. Configure collision layers based on bullet ownership:
    ///    - Player bullets (IsPlayerBullet = true):
    ///      * CollisionLayer = 2 (PLAYER_BULLET layer)
    ///      * CollisionMask = 8 (detect ENEMY layer)
    ///      * Added to "player_bullet" group for identification
    ///    - Enemy bullets (IsPlayerBullet = false):
    ///      * CollisionLayer = 4 (ENEMY_BULLET layer)
    ///      * CollisionMask = 1 (detect PLAYER layer)
    ///      * Added to "enemy_bullet" group for identification
    /// 3. Connect collision signal for hit detection
    /// 
    /// The collision layer system prevents bullets from hitting their own team
    /// and allows for proper hit detection in the event-driven architecture.
    /// </summary>
    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;

        // Set collision layers based on bullet type
        if (IsPlayerBullet)
        {
            CollisionLayer = 2; // Player bullet layer
            CollisionMask = 8; // Enemy layer
            AddToGroup("player_bullet");
        }
        else
        {
            CollisionLayer = 4; // Enemy bullet layer
            CollisionMask = 1; // Player layer
            AddToGroup("enemy_bullet");
        }
        
        AreaEntered += OnAreaEntered;
    }
    
    /// <summary>
    /// Update bullet position every frame and handle off-screen culling
    /// 
    /// Movement:
    ///   - Position updated by Direction * Speed * delta for smooth frame-rate independent motion
    ///   - Direction is normalized, so Speed directly controls velocity in pixels/second
    /// 
    /// Off-Screen Culling:
    ///   Bullets are destroyed if they exceed screen bounds by 50px buffer:
    ///   - Top: Y < -50
    ///   - Bottom: Y > screenHeight + 50
    ///   - Left: X < -50
    ///   - Right: X > screenWidth + 50
    /// 
    /// The 50px buffer prevents visible pop-out when bullets leave the screen.
    /// This is critical for performance - without culling, bullets would accumulate
    /// indefinitely and cause memory/performance issues.
    /// </summary>
    public override void _Process(double delta)
    {
        Position += Direction * Speed * (float)delta;
        
        // Remove bullet if off-screen
        if (Position.Y < -50 || Position.Y > _screenSize.Y + 50 ||
            Position.X < -50 || Position.X > _screenSize.X + 50)
        {
            QueueFree();
        }
    }
    
    /// <summary>
    /// Handle collision with other Area2D nodes (Player or Enemy)
    /// 
    /// Collision Logic:
    ///   - Player bullets: Destroy on hitting "enemy" group
    ///   - Enemy bullets: Destroy on hitting "player" group
    /// 
    /// The actual damage/hit logic is handled by the target entity:
    ///   - Enemy.OnAreaEntered() reduces enemy health and awards points
    ///   - PlayerController.OnAreaEntered() reduces player lives
    /// 
    /// This separation of concerns keeps the bullet class simple and focused
    /// solely on projectile behavior, following the single-responsibility principle.
    /// The bullet's job is just to exist, move, and self-destruct on impact.
    /// </summary>
    private void OnAreaEntered(Area2D area)
    {
        if (IsPlayerBullet && area.IsInGroup("enemy"))
        {
            QueueFree();
        }
        else if (!IsPlayerBullet && area.IsInGroup("player"))
        {
            QueueFree();
        }
    }
}

