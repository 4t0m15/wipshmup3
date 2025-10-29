using Godot;

public partial class GameManager : Node
{
	[Export] public PackedScene? EnemyScene { get; set; } = null;
	[Export] public PackedScene? EnemyBulletScene { get; set; } = null;
	[Export] public int PlayerHealth { get; set; } = 5; // Easy mode: more health
	[Export] public float SpawnRate { get; set; } = 3.5f; // Easy mode: slower spawn

	private Vector2 _screenSize;
	private float _spawnTimer = 0f;
	private int _currentHealth;
	private bool _gameOver = false;
	private int _currentScore = 0;
	private int _highScore = 0;
	private HUD? _hud;

	public override void _Ready()
	{
		// Ensure we have a valid screen size
		_screenSize = GetViewport().GetVisibleRect().Size;
		if (_screenSize == Vector2.Zero)
		{
			// Fallback to a reasonable default if viewport isn't ready
			_screenSize = new Vector2(1024f, 600f);
		}
		
		_currentHealth = PlayerHealth;
		AddToGroup("game_manager");
		
		// Self-load default scenes if not set (autoload can't set exports via scene)
		if (EnemyScene == null)
			EnemyScene = ResourceLoader.Load<PackedScene>("res://enemy.tscn");
		if (EnemyBulletScene == null)
			EnemyBulletScene = ResourceLoader.Load<PackedScene>("res://enemy_bullet.tscn");
		
		// Find HUD from current scene
		_hud = GetTree().CurrentScene?.GetNodeOrNull<HUD>("HUD");
		
		// Debug: Check if HUD was found
		if (_hud == null)
		{
			GD.PrintErr("GameManager: HUD not found!");
		}
		else
		{
			GD.Print("GameManager: HUD found successfully");
		}
		
		// Start spawning timer
		_spawnTimer = SpawnRate;

		// Initialize HUD with current values
		UpdateHealth();
		UpdateScore();
		HideGameOver();
	}

	public override void _Process(double delta)
	{
		if (_gameOver) return;
		
		// Ensure we have a valid screen size (in case viewport wasn't ready in _Ready)
		if (_screenSize == Vector2.Zero)
		{
			_screenSize = GetViewport().GetVisibleRect().Size;
			if (_screenSize == Vector2.Zero)
				_screenSize = new Vector2(1024f, 600f);
		}
		
		// Handle enemy spawning
		_spawnTimer -= (float)delta;
		if (_spawnTimer <= 0f)
		{
			_spawnTimer = SpawnRate;
			SpawnEnemy();
		}
	}

	private void SpawnEnemy()
	{
		if (EnemyScene == null) return;
		
		var enemy = EnemyScene.Instantiate();
		if (enemy is Node2D enemyNode)
		{
		// Spawn at random Y position at right side of screen
		var randomY = (float)GD.RandRange(50f, _screenSize.Y - 50f);
		enemyNode.Position = new Vector2(_screenSize.X + 50f, randomY);
			GetTree().CurrentScene.AddChild(enemyNode);
		}
	}

	public void OnPlayerHit()
	{
		if (_gameOver) return;
		
		_currentHealth--;
		UpdateHealth();
		
		if (_currentHealth <= 0)
		{
			GameOver();
		}
	}

	public void OnEnemyDestroyed(int scoreValue)
	{
		GD.Print($"GameManager: Enemy destroyed! Adding {scoreValue} points. Current score: {_currentScore}");
		_currentScore += scoreValue;
		UpdateScore();
	}

	private void GameOver()
	{
		_gameOver = true;
		
		// Update high score
		if (_currentScore > _highScore)
		{
			_highScore = _currentScore;
		}
		
		// Show game over message
		ShowGameOver();
		
		// Restart game after short delay
		var timer = new Godot.Timer();
		timer.WaitTime = 2.0f;
		timer.OneShot = true;
		timer.Timeout += RestartGame;
		AddChild(timer);
		timer.Start();
	}

	private void RestartGame()
	{
		// Reset health and score
		_currentHealth = PlayerHealth;
		_currentScore = 0;
		_gameOver = false;
		
		// Update HUD
		UpdateHealth();
		UpdateScore();
		HideGameOver();
		
		// Reset player position to left side
		var player = GetTree().GetFirstNodeInGroup("player");
		if (player is Node2D playerNode)
		{
			playerNode.Position = new Vector2(100f, _screenSize.Y / 2f);
		}
		
		// Clear all enemies and bullets
		ClearAllEntities();
	}

	private void ClearAllEntities()
	{
		// Clear enemies
		var enemies = GetTree().GetNodesInGroup("enemies");
		foreach (var enemy in enemies)
		{
			if (enemy is Node node)
				node.QueueFree();
		}
		
		// Clear bullets
		var playerBullets = GetTree().GetNodesInGroup("player_bullets");
		foreach (var bullet in playerBullets)
		{
			if (bullet is Node node)
				node.QueueFree();
		}
		
		var enemyBullets = GetTree().GetNodesInGroup("enemy_bullets");
		foreach (var bullet in enemyBullets)
		{
			if (bullet is Node node)
				node.QueueFree();
		}
	}

	public PackedScene? GetEnemyBulletScene()
	{
		return EnemyBulletScene;
	}
	
	private void UpdateScore()
	{
		GD.Print($"GameManager: UpdateScore called. Current score: {_currentScore}, High score: {_highScore}");
		if (_hud != null)
		{
			GD.Print("GameManager: Calling HUD.UpdateScore");
			_hud.UpdateScore(_currentScore, _highScore);
		}
		else
		{
			GD.PrintErr("GameManager: HUD is null in UpdateScore!");
		}
	}
	
	private void UpdateHealth()
	{
		if (_hud != null)
		{
			_hud.UpdateHealth(_currentHealth);
		}
	}
	
	private void ShowGameOver()
	{
		if (_hud != null)
		{
			_hud.ShowGameOver(_currentScore, _highScore);
		}
	}
	
	private void HideGameOver()
	{
		if (_hud != null)
		{
			_hud.HideGameOver();
		}
	}
}
