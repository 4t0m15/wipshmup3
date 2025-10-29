using Godot;

public partial class Enemy : Area2D
{
	[Export] public float Speed { get; set; } = 200f;
	[Export] public int Health { get; set; } = 3;
	[Export] public int ScoreValue { get; set; } = 100;
	[Export] public float FireRate { get; set; } = 1.5f;

	private Vector2 _screenSize;
	private float _fireTimer = 0f;
	private PackedScene? _bulletScene;
	private ColorRect? _visual;
	private bool _isFlashing = false;
	private float _flashTimer = 0f;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;
		AddToGroup("enemies");
		
		// Set up collision detection
		AreaEntered += OnAreaEntered;
		
		// Get bullet scene from GameManager
		var gameManager = GetTree().GetFirstNodeInGroup("game_manager");
		if (gameManager != null && gameManager.HasMethod("GetEnemyBulletScene"))
		{
			_bulletScene = gameManager.Call("GetEnemyBulletScene").AsGodotObject() as PackedScene;
		}
		
		// Get visual component
		_visual = GetNodeOrNull<ColorRect>("ColorRect");
		
		// Start firing timer
		_fireTimer = FireRate;
	}

	public override void _Process(double delta)
	{
		// Move leftward
		Position += Vector2.Left * Speed * (float)delta;
		
		// Handle flash effect
		if (_isFlashing)
		{
			_flashTimer -= (float)delta;
			if (_flashTimer <= 0f)
			{
				_isFlashing = false;
				if (_visual != null)
					_visual.Color = new Color(0.8f, 0.2f, 0.2f, 1f); // Back to normal red
			}
		}
		
		// Handle shooting
		if (_bulletScene != null)
		{
			_fireTimer -= (float)delta;
			if (_fireTimer <= 0f)
			{
				_fireTimer = FireRate;
				Shoot();
			}
		}
		
		// Clean up when off-screen
		if (Position.X < -100f)
		{
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		// Check if it's a player bullet
		if (area.IsInGroup("player_bullets"))
		{
			// The bullet will handle damage via TakeDamage method
			// We don't need to check health here since TakeDamage handles destruction
		}
	}

	public void TakeDamage(int damage)
	{
		GD.Print($"Enemy: TakeDamage called! Health: {Health} -> {Health - damage}");
		Health -= damage;
		
		// Flash red when hit
		_isFlashing = true;
		_flashTimer = 0.1f;
		if (_visual != null)
			_visual.Color = new Color(1f, 0f, 0f, 1f); // Bright red flash
		
		if (Health <= 0)
		{
			GD.Print("Enemy: Health reached 0, calling Destroy()");
			Destroy();
		}
	}

	private void Destroy()
	{
		GD.Print($"Enemy: Destroy() called! Score value: {ScoreValue}");
		
		// Create explosion effect
		var explosion = new ColorRect();
		explosion.Color = new Color(1f, 0.5f, 0f, 1f); // Orange explosion
		explosion.Size = new Vector2(120f, 120f);
		explosion.Position = GlobalPosition - new Vector2(60f, 60f);
		GetTree().CurrentScene.AddChild(explosion);
		
		// Remove explosion after short time
		var timer = new Godot.Timer();
		timer.WaitTime = 0.2f;
		timer.OneShot = true;
		timer.Timeout += () => explosion.QueueFree();
		explosion.AddChild(timer);
		timer.Start();
		
		// Notify GameManager for score
		var gameManager = GetTree().GetFirstNodeInGroup("game_manager");
		if (gameManager != null && gameManager.HasMethod("OnEnemyDestroyed"))
		{
			GD.Print("Enemy: Calling GameManager.OnEnemyDestroyed");
			gameManager.Call("OnEnemyDestroyed", ScoreValue);
		}
		else
		{
			GD.PrintErr("Enemy: GameManager not found or OnEnemyDestroyed method not available!");
		}
		
		QueueFree();
	}

	private void Shoot()
	{
		if (_bulletScene == null) return;
		
		var bullet = _bulletScene.Instantiate();
		if (bullet is Node2D bulletNode)
		{
			bulletNode.Position = GlobalPosition + new Vector2(-40, 0);
			GetTree().CurrentScene.AddChild(bulletNode);
		}
	}
}
