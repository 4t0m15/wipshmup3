using Godot;
using System;

/// <summary>
/// Main Game Scene Controller - Root coordinator for the entire game
/// 
/// This is the primary game loop coordinator inspired by classic shmups like Cho Ren Sha 68K,
/// 1942, TouHou Project, and Galaga/Galaxian. It manages the game viewport (1920x1080),
/// spawns entities, coordinates between systems, and handles the HUD layer.
/// 
/// Architecture Position: Top-level coordinator in the event-driven architecture
/// - Manages the GameViewport container
/// - Spawns and coordinates Player, Enemies, and Bullets
/// - Interfaces with the GameState autoload singleton for persistent state
/// - Handles UI updates through signal-driven events
/// 
/// Signal Flow:
///   GameState.ScoreChanged -> OnScoreChanged() -> Update HUD
///   GameState.LivesChanged -> OnLivesChanged() -> Update HUD
///   GameState.GameOver -> OnGameOver() -> Display Game Over screen
/// </summary>
public partial class Main : Node2D
{
    /// <summary>Player scene prefab to instantiate - contains PlayerController component</summary>
    [Export] public PackedScene PlayerScene { get; set; }
    
    /// <summary>Enemy scene prefab to instantiate - contains Enemy entity with behavior components</summary>
    [Export] public PackedScene EnemyScene { get; set; }
    
    /// <summary>Time in seconds between enemy spawns - adjustable for difficulty tuning</summary>
    [Export] public float EnemySpawnRate { get; set; } = 2.0f;
    
    /// <summary>Reference to the global GameState singleton autoload for score, lives, and game state</summary>
    private GameState _gameState;
    
    /// <summary>Accumulates time to trigger enemy spawns at regular intervals</summary>
    private float _spawnTimer = 0f;
    
    /// <summary>HUD label displaying current score (top-left corner)</summary>
    private Label _scoreLabel;
    
    /// <summary>HUD label displaying remaining lives (below score)</summary>
    private Label _livesLabel;
    
    /// <summary>Game Over overlay label (centered, hidden until game ends)</summary>
    private Label _gameOverLabel;
    
    /// <summary>
    /// Initialize the game scene when it enters the scene tree
    /// 
    /// Steps:
    /// 1. Connect to the global GameState singleton (autoload system)
    /// 2. Subscribe to state change signals for reactive UI updates
    /// 3. Create and position the HUD CanvasLayer (Score, Lives, Game Over)
    /// 4. Spawn the player at starting position (centered, near bottom)
    /// 
    /// This follows the event-driven architecture pattern where UI updates
    /// are triggered by GameState signals rather than polling.
    /// </summary>
    public override void _Ready()
    {
        _gameState = GetNode<GameState>("/root/GameState");
        _gameState.ScoreChanged += OnScoreChanged;
        _gameState.LivesChanged += OnLivesChanged;
        _gameState.GameOver += OnGameOver;
        
        SetupUI();
        SpawnPlayer();

        GD.Print("Game started!");
    }

    /// <summary>
    /// Create and configure the HUD layer using Godot's CanvasLayer system
    /// 
    /// The HUD is rendered on a separate layer above the game viewport:
    /// - Score Label: Top-left (10, 10) - displays current score
    /// - Lives Label: Below score (10, 40) - displays remaining lives
    /// - Game Over Label: Centered (400, 300) - initially hidden, shown on game over
    /// 
    /// All UI elements are children of a CanvasLayer for proper render ordering
    /// and independence from camera movement (if camera is added later).
    /// </summary>
    private void SetupUI()
    {
        // Create UI layer
        var canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);

        // Score label
        _scoreLabel = new Label();
        _scoreLabel.Position = new Vector2(10, 10);
        _scoreLabel.Text = "Score: 0";
        canvasLayer.AddChild(_scoreLabel);

        // Lives label
        _livesLabel = new Label();
        _livesLabel.Position = new Vector2(10, 40);
        _livesLabel.Text = "Lives: 3";
        canvasLayer.AddChild(_livesLabel);

        // Game over label
        _gameOverLabel = new Label();
        _gameOverLabel.Position = new Vector2(400, 300);
        _gameOverLabel.Text = "GAME OVER";
        _gameOverLabel.Visible = false;
        canvasLayer.AddChild(_gameOverLabel);
    }

    /// <summary>
    /// Instantiate and position the player entity in the game world
    /// 
    /// The player is spawned at:
    /// - X: Horizontal center of the viewport (960px for 1920px width)
    /// - Y: Near bottom of screen (100px from bottom edge)
    /// 
    /// This follows the classic shmup convention where the player starts
    /// centered at the bottom of the screen, with enemies coming from above.
    /// </summary>
    private void SpawnPlayer()
    {
        if (PlayerScene == null) return;

        var player = PlayerScene.Instantiate<PlayerController>();
        player.Position = new Vector2(GetViewportRect().Size.X / 2, GetViewportRect().Size.Y - 100);
        AddChild(player);
    }
    
    /// <summary>
    /// Main game loop - processes every frame
    /// 
    /// Responsibilities:
    /// 1. Check if game is over (pause spawning if true)
    /// 2. Accumulate spawn timer delta
    /// 3. Spawn enemies at regular intervals based on EnemySpawnRate
    /// 
    /// The spawn timer is reset each time an enemy is spawned, creating
    /// a consistent spawn rate. Game over stops all spawning automatically.
    /// </summary>
    public override void _Process(double delta)
    {
        if (_gameState?.IsGameOver ?? true) return;
        
        _spawnTimer += (float)delta;

        if (_spawnTimer >= EnemySpawnRate)
        {
            SpawnEnemy();
            _spawnTimer = 0f;
        }
    }
    
    /// <summary>
    /// Spawn a single enemy at a random horizontal position at the top of the screen
    /// 
    /// Spawn Logic:
    /// - X position: Random across full screen width (0 to 1920px)
    /// - Y position: -50px (above visible screen) so enemies "enter" from top
    /// 
    /// The enemy is added as a child of Main, placing it in the same scene tree
    /// level as the player for proper collision detection and rendering order.
    /// Enemy behavior (movement, shooting) is handled by the Enemy class itself
    /// using component-based MovementBehavior and AttackBehavior patterns.
    /// </summary>
    private void SpawnEnemy()
    {
        if (EnemyScene == null) return;

        var enemy = EnemyScene.Instantiate<Enemy>();
        var screenWidth = GetViewportRect().Size.X;
        enemy.Position = new Vector2(GD.Randf() * screenWidth, -50);
        AddChild(enemy);
    }
    
    /// <summary>
    /// Signal handler: Reacts to score changes from GameState
    /// 
    /// This is part of the event-driven architecture - GameState emits ScoreChanged
    /// whenever points are awarded (enemy killed, boss defeated), and this handler
    /// updates the HUD label accordingly. This decouples game logic from UI updates.
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        if (_scoreLabel != null)
            _scoreLabel.Text = $"Score: {newScore}";
    }
    
    /// <summary>
    /// Signal handler: Reacts to life count changes from GameState
    /// 
    /// Triggered when player is hit by enemy bullet or collision. Updates the
    /// lives display on the HUD. When lives reach 0, GameState will emit GameOver.
    /// </summary>
    private void OnLivesChanged(int newLives)
    {
        if (_livesLabel != null)
            _livesLabel.Text = $"Lives: {newLives}";
    }

    /// <summary>
    /// Signal handler: Reacts to game over state from GameState
    /// 
    /// When the player loses all lives, GameState emits this signal and Main
    /// displays the centered "GAME OVER" label. The _Process loop also stops
    /// spawning enemies when IsGameOver is true.
    /// </summary>
    private void OnGameOver()
    {
        if (_gameOverLabel != null)
            _gameOverLabel.Visible = true;
    }
}

