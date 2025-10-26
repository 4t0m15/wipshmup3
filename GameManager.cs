using Godot;

public partial class GameManager : Node
{
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public PackedScene EnemyBulletScene { get; set; }
    [Export] public float SpawnRate { get; set; } = 2.0f;
    [Export] public int PlayerHealth { get; set; } = 3;
    [Export] public int Score { get; set; } = 0;
    
    private Timer _spawnTimer;
    private Vector2 _screenSize;
    private Label _scoreLabel;
    private Label _healthLabel;
    private Player _player;
    
    // Cache the SceneTree to avoid using GetTree() after this node leaves the tree
    private SceneTree _tree;
    private bool _isGameOver;
    
    public override void _Ready()
    {
        _screenSize = GetViewport().GetVisibleRect().Size;
        _player = GetNode<Player>("Player");
        _tree = GetTree();
        
        // Create UI
        CreateUI();
        
        // Set up spawn timer
        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = SpawnRate;
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
        _spawnTimer.Autostart = true;
        AddChild(_spawnTimer);
        
        // Add to group for easy access
        AddToGroup("game_manager");
    }
    
    private void CreateUI()
    {
        // Create score label
        _scoreLabel = new Label();
        _scoreLabel.Text = "Score:0";
        _scoreLabel.Position = new Vector2(20,20);
        _scoreLabel.AddThemeFontSizeOverride("font_size",24);
        AddChild(_scoreLabel);
        
        // Create health label
        _healthLabel = new Label();
        _healthLabel.Text = "Health: " + PlayerHealth;
        _healthLabel.Position = new Vector2(20,60);
        _healthLabel.AddThemeFontSizeOverride("font_size",24);
        AddChild(_healthLabel);
    }
    
    private void OnSpawnTimerTimeout()
    {
        if (EnemyScene == null) return;
        
        // Spawn enemy at random y position, right of screen
        var enemy = EnemyScene.Instantiate<Enemy>();
        AddChild(enemy);
        
        float randomY = GD.Randf() * (_screenSize.Y -100) +50;
        enemy.Initialize(new Vector2(_screenSize.X +50, randomY));
        
        // Set bullet scene for enemy
        if (EnemyBulletScene != null)
        {
            enemy.BulletScene = EnemyBulletScene;
        }
    }
    
    public void AddScore(int points)
    {
        Score += points;
        _scoreLabel.Text = "Score: " + Score;
    }
    
    public void PlayerTakeDamage(int damage)
    {
        if (_isGameOver) return;
        
        PlayerHealth -= damage;
        _healthLabel.Text = "Health: " + PlayerHealth;
        
        if (PlayerHealth <=0)
        {
            GameOver();
        }
    }
    
    private void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        // Stop spawning
        _spawnTimer?.Stop();
        
        // Show game over message
        var gameOverLabel = new Label();
        gameOverLabel.Text = "GAME OVER\nFinal Score: " + Score;
        gameOverLabel.Position = new Vector2(_screenSize.X /2 -150, _screenSize.Y /2 -50);
        gameOverLabel.AddThemeFontSizeOverride("font_size",32);
        AddChild(gameOverLabel);
        
        // Restart after 3 seconds using cached tree
        var tree = _tree ?? GetTree();
        if (tree == null)
        {
            GD.PushWarning("[GameManager] SceneTree is null; cannot restart scene.");
            return;
        }

        var timer = tree.CreateTimer(3.0);
        timer.Timeout += () => tree.ReloadCurrentScene();
    }
}