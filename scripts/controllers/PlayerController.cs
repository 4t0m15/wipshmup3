using Godot;
using System;

/// <summary>
/// Player Controller - Input handling and player entity management
/// 
/// This controller manages the player's ship in the classic shmup style of
/// Cho Ren Sha 68K, 1942, and TouHou Project. It handles:
///   - Arrow key movement with 8-directional control
///   - Space bar shooting with rate limiting
///   - Collision detection with enemy bullets and enemies
///   - Integration with GameState for lives management
/// 
/// Controls (from shmupbible):
///   - Arrow Keys: 8-directional movement (up, down, left, right, diagonals)
///   - Space (ui_accept): Shoot bullets upward
///   - X Key: Bomb (future feature, mentioned in shmupbible architecture)
/// 
/// Collision System:
///   - CollisionLayer = 1 (PLAYER layer)
///   - CollisionMask = 4 (detects ENEMY_BULLET layer)
///   - Player hurtbox detects both enemy bullets and enemy body collisions
/// 
/// Signal Flow on Hit:
///   PlayerController.PlayerHit -> [listeners]
///   PlayerController -> GameState.LoseLife() -> GameState.LivesChanged
///   -> Main.OnLivesChanged() -> Update HUD
/// 
/// The player starts at screen bottom-center (spawned by Main) and is
/// clamped to screen boundaries to prevent moving off-screen. This follows
/// the classic shmup convention where the player is always visible.
/// </summary>
public partial class PlayerController : Area2D
{
    /// <summary>Movement speed in pixels per second - balance between control and challenge</summary>
    [Export] public float Speed { get; set; } = 300f;
    
    /// <summary>Time in seconds between shots - lower = faster firing rate</summary>
    [Export] public float FireRate { get; set; } = 0.05f;
    
    /// <summary>Bullet prefab scene to instantiate when shooting</summary>
    [Export] public PackedScene BulletScene { get; set; }
    
    /// <summary>Accumulates time since last shot to enforce fire rate limit</summary>
    private float _fireTimer = 0f;
    
    /// <summary>Cached viewport size for screen boundary clamping</summary>
    private Vector2 _screenSize;
    
    /// <summary>Reference to global GameState singleton for lives management</summary>
    private GameState _gameState;
    
    /// <summary>Signal emitted when player is hit - can trigger invulnerability, effects, etc.</summary>
    [Signal] public delegate void PlayerHitEventHandler();

    /// <summary>
    /// Initialize player controller when entering the scene tree
    /// 
    /// Setup Steps:
    /// 1. Cache viewport size for boundary clamping calculations
    /// 2. Connect to GameState singleton autoload for lives management
    /// 3. Configure collision system:
    ///    - Layer 1 (PLAYER) for detection by enemy bullets
    ///    - Mask 4 (ENEMY_BULLET) to detect incoming enemy fire
    /// 4. Connect collision signal for damage detection
    /// 
    /// The player's collision setup is the inverse of enemies:
    ///   - Player on Layer 1, detected by enemy bullets on Mask 1
    ///   - Player detects Mask 4 (enemy bullets) and enemy bodies (Layer 8)
    /// 
    /// This ensures proper collision matrix for the shmup gameplay.
    /// </summary>
    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;
        _gameState = GetNode<GameState>("/root/GameState");
        
        // Set up collision
        CollisionLayer = 1; // Player layer
        CollisionMask = 4; // Enemy bullet layer
        
        AreaEntered += OnAreaEntered;
    }
    
    /// <summary>
    /// Update player every frame - handle input and game state
    /// 
    /// Update Sequence:
    /// 1. Check if game is over - pause all input if true
    /// 2. Handle movement input (arrow keys, 8-directional)
    /// 3. Handle shooting input (space bar with rate limiting)
    /// 
    /// The game-over check prevents player from moving/shooting after death,
    /// which is standard behavior in classic shmups. All input is disabled
    /// until the game is restarted.
    /// </summary>
    public override void _Process(double delta)
    {
        if (_gameState?.IsGameOver ?? true) return;
        
        float dt = (float)delta;
        HandleMovement(dt);
        HandleShooting(dt);
    }
    
    /// <summary>
    /// Process movement input and update player position with screen clamping
    /// 
    /// Movement System:
    /// 1. Poll directional inputs (ui_left, ui_right, ui_up, ui_down)
    /// 2. Build velocity vector from input
    /// 3. Normalize diagonal movement (prevent faster diagonal speed)
    /// 4. Apply speed multiplier for final velocity
    /// 5. Update position with delta time for frame-rate independence
    /// 6. Clamp position to screen boundaries (0 to screen width/height)
    /// 
    /// Input Mapping (Godot defaults):
    ///   - ui_left: Left Arrow
    ///   - ui_right: Right Arrow
    ///   - ui_up: Up Arrow
    ///   - ui_down: Down Arrow
    /// 
    /// The normalization step is crucial: without it, diagonal movement would be
    /// sqrt(2) ≈ 1.41x faster than cardinal directions, giving unfair advantage
    /// to diagonal movement. Normalization ensures constant speed in all directions.
    /// 
    /// Screen clamping prevents the player from moving off-screen, which is
    /// standard in vertical shmups where the playfield is fixed. This differs
    /// from games like R-Type where the screen scrolls.
    /// </summary>
    private void HandleMovement(float delta)
    {
        var velocity = Vector2.Zero;
        
        if (Input.IsActionPressed("ui_right"))
            velocity.X += 1;
        if (Input.IsActionPressed("ui_left"))
            velocity.X -= 1;
        if (Input.IsActionPressed("ui_down"))
            velocity.Y += 1;
        if (Input.IsActionPressed("ui_up"))
            velocity.Y -= 1;
        
        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * Speed;
        }

        Position += velocity * delta;

        // Clamp to screen
        Position = new Vector2(
            Mathf.Clamp(Position.X, 0, _screenSize.X),
            Mathf.Clamp(Position.Y, 0, _screenSize.Y)
        );
    }
    
    /// Process shooting input with rate limiting
    /// 
    /// Shooting System:
    /// 1. Accumulate time since last shot (_fireTimer)
    /// 2. Check if shoot button pressed (ui_accept = Space bar)
    /// 3. Verify fire rate timer has elapsed (prevents spam)
    /// 4. Call Shoot() to spawn bullet
    /// 5. Reset timer for next shot
    /// 
    /// Rate Limiting:
    ///   The FireRate property (default 0.2s = 5 shots/second) prevents
    ///   bullet spam and adds strategic gameplay. Players must manage their
    ///   fire timing rather than holding down the button mindlessly.
    /// 
    /// This is a simple rate limiter - classic shmups often have more complex
    /// systems like:
    ///   - Charge shots (hold to charge, release for powerful shot)
    ///   - Power-up levels that increase fire rate
    ///   - Different weapon types with varying rates
    /// 
    /// The current implementation provides a solid foundation for these features.
    private void HandleShooting(float delta)
    {
        _fireTimer += delta;
        
        if (Input.IsActionPressed("ui_accept") && _fireTimer >= FireRate)
        {
            Shoot();
            _fireTimer = 0f;
        }
    }

    /// Spawn a player bullet traveling upward
    /// 
    /// Bullet Spawning:
    /// 1. Instantiate bullet from BulletScene prefab
    /// 2. Position 20px above player center (spawn from "gun" position)
    /// 3. Add to parent scene (not as child of player)
    /// 4. Bullet defaults: Direction=Up, IsPlayerBullet=true, Speed=400
    /// 
    /// The 20px offset prevents the bullet from immediately colliding with
    /// the player's own hitbox. Adding to parent rather than as child ensures
    /// bullets persist independently and don't inherit player transform.
    /// 

    private void Shoot()
    {
        if (BulletScene == null) return;

        var bullet = BulletScene.Instantiate<Area2D>();
        bullet.Position = Position + new Vector2(0, -20);
        GetParent().AddChild(bullet);
    }
    
    /// <summary>
    /// Handle collision with enemy bullets or enemies - damage and life loss
    /// 
    /// Collision Response:
    /// 1. Verify collision is with enemy bullet or enemy body (group check)
    /// 2. Emit PlayerHit signal for other systems (audio, effects, etc.)
    /// 3. Call GameState.LoseLife() to decrement lives
    /// 4. Print debug message for development
    /// 
    /// Signal Flow on Hit:
    ///   PlayerController.PlayerHit -> [Audio/Visual effect listeners]
    ///   PlayerController -> GameState.LoseLife()
    ///   GameState.LivesChanged(newLives) -> Main.OnLivesChanged()
    ///   -> Update HUD lives display
    ///   
    ///   If lives reach 0:
    ///   GameState.GameOver -> Main.OnGameOver() -> Show "GAME OVER"
    /// 
    /// Missing Features (from shmupbible):
    ///   - Invulnerability frames after hit (common in TouHou/Galaga)
    ///   - Visual feedback (flashing, screen shake)
    ///   - Death animation and respawn sequence
    ///   - Bomb system (X key) for emergency clear
    /// 
    /// Currently, the player takes damage instantly with no grace period.
    /// This is intentional for the prototype but should be enhanced for
    /// final gameplay to match the polished feel of the inspiration games.
    /// </summary>
    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("enemy_bullet") || area.IsInGroup("enemy"))
        {
            EmitSignal(SignalName.PlayerHit);
            _gameState?.LoseLife();
            GD.Print("Player hit!");
        }
    }
}

