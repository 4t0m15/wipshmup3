using Godot;
using System;

/// <summary>
/// Game State - Global singleton autoload for persistent game state
/// 
/// This is the central state management system from the shmupbible's Event-Driven
/// Architecture. It serves as the single source of truth for all game state that
/// needs to persist across scenes or be accessed globally.
/// 
/// Architecture Position: Autoload System (Global Singleton)
///   - Automatically instantiated at game start via Project Settings > Autoload
///   - Accessible from any script via: GetNode<GameState>("/root/GameState")
///   - Persists across scene changes (unlike scene-specific nodes)
///   - Emits signals when state changes for reactive UI updates
/// 
/// State Managed:
///   - Score: Current player score (for ranking and high scores)
///   - Lives: Remaining player lives (game over when 0)
///   - Bombs: Available smart bombs (future feature from shmupbible)
///   - Game Over: Boolean flag to pause gameplay
///   - Streak System: Consecutive kill tracking (future enhancement)
///   - Rank System: Dynamic difficulty adjustment (future enhancement)
/// 
/// Signal Flow Examples (from shmupbible):
///   Enemy destroyed -> GameState.AddScore() -> ScoreChanged signal
///   -> Main.OnScoreChanged() -> Update HUD
///   
///   Player hit -> GameState.LoseLife() -> LivesChanged signal
///   -> Main.OnLivesChanged() -> Update HUD
///   -> If lives == 0: GameOver signal -> Main.OnGameOver() -> Show game over screen
/// 
/// Why Signals?
///   The signal-based approach decouples game logic from UI updates. GameState
///   doesn't need to know about the HUD - it just emits events when state changes,
///   and interested systems (Main, HUD, etc.) react accordingly. This is the
///   Observer Pattern, fundamental to event-driven architectures.
/// 
/// This autoload singleton pattern is common in Godot games and provides a clean
/// way to manage global state without static classes or singletons with complex
/// lifecycle management.
/// </summary>
public partial class GameState : Node
{
    /// <summary>Current player score - increases from enemy/boss kills, combos, power-ups</summary>
    private int _score = 0;
    
    /// <summary>Remaining player lives - starts at 3, game over when reaches 0</summary>
    private int _lives = 3;
    
    /// <summary>Available smart bombs for emergency screen clear (future feature - X key)</summary>
    private int _bombs = 3;
    
    /// <summary>Game over flag - when true, stops spawning and input processing</summary>
    private bool _isGameOver = false;
    
    // Public properties for read-only access
    /// <summary>Current score value - read-only, use AddScore() to modify</summary>
    public int Score => _score;
    
    /// <summary>Current lives count - read-only, use LoseLife() to modify</summary>
    public int Lives => _lives;
    
    /// <summary>Current bomb count - read-only, use UseBomb() to modify</summary>
    public int Bombs => _bombs;
    
    /// <summary>Whether game is over - read-only, set automatically when lives reach 0</summary>
    public bool IsGameOver => _isGameOver;
    
    // Signals for reactive state updates
    /// <summary>Emitted when score changes - carries new score value for UI updates</summary>
    [Signal] public delegate void ScoreChangedEventHandler(int newScore);
    
    /// <summary>Emitted when lives change - carries new lives count for UI updates</summary>
    [Signal] public delegate void LivesChangedEventHandler(int newLives);
    
    /// <summary>Emitted when bomb count changes - for UI updates (future feature)</summary>
    [Signal] public delegate void BombsChangedEventHandler(int newBombs);
    
    /// <summary>Emitted when game over occurs - triggers game over screen and stops gameplay</summary>
    [Signal] public delegate void GameOverEventHandler();
    
    /// <summary>
    /// Initialize the game state singleton
    /// 
    /// Called automatically by Godot when the autoload is created at game start.
    /// Sets initial values and logs confirmation.
    /// 
    /// Future: Load saved high scores, preferences, unlocks from file
    /// </summary>
    public override void _Ready()
    {
        GD.Print("GameState initialized - Score: 0, Lives: 3, Bombs: 3");
    }
    
    /// <summary>
    /// Add points to the player's score
    /// 
    /// Called when:
    ///   - Enemy destroyed (typically 100 points)
    ///   - Boss defeated (typically 10000 points)
    ///   - Power-up collected (future)
    ///   - Stage completed (future)
    /// 
    /// Signal Flow:
    ///   AddScore(100) -> _score += 100 -> EmitSignal(ScoreChanged, 100)
    ///   -> Main.OnScoreChanged(100) -> Update HUD label
    /// 
    /// Future Enhancements:
    ///   - Combo multipliers (2x, 3x, etc. for consecutive kills)
    ///   - Difficulty rank multipliers (higher rank = more points)
    ///   - Extra life awards at score thresholds (10000, 50000, etc.)
    /// </summary>
    public void AddScore(int points)
    {
        _score += points;
        EmitSignal(SignalName.ScoreChanged, _score);
        GD.Print($"Score: {_score} (+{points})");
        
        // Future: Check for extra life milestones
        // if (_score >= 10000 && _score - points < 10000)
        //     AddLife();
    }
    
    /// <summary>
    /// Remove a life from the player
    /// 
    /// Called when:
    ///   - Player hit by enemy bullet
    ///   - Player collides with enemy
    /// 
    /// Process:
    /// 1. Decrement lives counter
    /// 2. Emit LivesChanged signal for UI update
    /// 3. Check if lives reached 0
    /// 4. If dead: Set game over flag and emit GameOver signal
    /// 
    /// Signal Flow:
    ///   LoseLife() -> _lives-- -> EmitSignal(LivesChanged, 2)
    ///   -> Main.OnLivesChanged(2) -> Update HUD
    ///   
    ///   If lives == 0:
    ///   -> _isGameOver = true -> EmitSignal(GameOver)
    ///   -> Main.OnGameOver() -> Show "GAME OVER" label
    ///   -> Main._Process() stops spawning enemies
    /// 
    /// Future Enhancements:
    ///   - Respawn sequence with invulnerability frames
    ///   - Life loss penalty (lose power-ups, reset combo, etc.)
    ///   - Continue system (insert coin to continue)
    /// </summary>
    public void LoseLife()
    {
        _lives--;
        EmitSignal(SignalName.LivesChanged, _lives);
        GD.Print($"Lives: {_lives}");
        
        if (_lives <= 0)
        {
            _isGameOver = true;
            EmitSignal(SignalName.GameOver);
            GD.Print("GAME OVER");
        }
    }
    
    /// <summary>
    /// Add an extra life (from score milestones or power-ups)
    /// 
    /// Called when:
    ///   - Score reaches milestone (10000, 50000, etc.)
    ///   - Collect 1-Up power-up (future)
    ///   - Complete stage with bonus requirements (future)
    /// 
    /// Lives are typically capped at 9 in classic shmups to prevent
    /// infinite accumulation and maintain challenge.
    /// </summary>
    public void AddLife()
    {
        if (_lives < 9) // Cap at 9 lives
        {
            _lives++;
            EmitSignal(SignalName.LivesChanged, _lives);
            GD.Print($"Extra Life! Lives: {_lives}");
        }
    }
    
    /// <summary>
    /// Use a smart bomb to clear the screen (future feature from shmupbible)
    /// 
    /// Called when: Player presses X key (bomb button)
    /// 
    /// Bomb Effect (planned):
    ///   - Destroy all enemies on screen
    ///   - Clear all enemy bullets
    ///   - Brief invulnerability window
    ///   - Dramatic visual/audio effect
    ///   - Award points for destroyed enemies (reduced points)
    /// 
    /// This is the "panic button" in shmups - limited uses per life,
    /// but gets you out of overwhelming bullet patterns. Strategic bomb
    /// usage is a key skill in games like TouHou Project.
    /// </summary>
    public void UseBomb()
    {
        if (_bombs > 0 && !_isGameOver)
        {
            _bombs--;
            EmitSignal(SignalName.BombsChanged, _bombs);
            GD.Print($"BOMB! Remaining: {_bombs}");
            
            // Future: Trigger bomb effect through EventBus
            // - Clear all bullets from screen
            // - Damage all enemies
            // - Spawn bomb explosion effect
        }
    }
    
    /// <summary>
    /// Reset all game state for a new game
    /// 
    /// Called when: Player starts new game after game over
    /// 
    /// Resets:
    ///   - Score to 0
    ///   - Lives to 3
    ///   - Bombs to 3
    ///   - Game over flag to false
    /// 
    /// Emits all change signals to update UI to initial state.
    /// </summary>
    public void ResetGame()
    {
        _score = 0;
        _lives = 3;
        _bombs = 3;
        _isGameOver = false;
        
        EmitSignal(SignalName.ScoreChanged, _score);
        EmitSignal(SignalName.LivesChanged, _lives);
        EmitSignal(SignalName.BombsChanged, _bombs);
        
        GD.Print("Game Reset - Score: 0, Lives: 3, Bombs: 3");
    }
}

