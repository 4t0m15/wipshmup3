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
	private CanvasItem? _visual;
	private bool _isFlashing = false;
	private float _flashTimer = 0f;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;
		AddToGroup("enemies");
		
		// Set up collision detection
		// Removed unused AreaEntered handler
		
		// Get bullet scene from GameManager autoload
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			_bulletScene = gameManager.GetEnemyBulletScene();
		}
		
		// Get visual component (prefer Sprite2D, fallback to ColorRect)
		_visual = GetNodeOrNull<CanvasItem>("Sprite2D") ?? GetNodeOrNull<CanvasItem>("ColorRect");
		
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
					_visual.Modulate = new Color(1f, 1f, 1f, 1f); // Back to normal tint
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

	// Removed unused OnAreaEntered handler

	public void TakeDamage(int damage)
	{
		GD.Print($"Enemy: TakeDamage called! Health: {Health} -> {Health - damage}");
		Health -= damage;
		
		// Flash red when hit
		_isFlashing = true;
		_flashTimer = 0.1f;
		if (_visual != null)
			_visual.Modulate = new Color(1f, 0f, 0f, 1f); // Bright red flash
		
		if (Health <= 0)
		{
			GD.Print("Enemy: Health reached 0, calling Destroy()");
			Destroy();
		}
	}

	private void Destroy()
	{
		GD.Print($"Enemy: Destroy() called! Score value: {ScoreValue}");
		
		// Notify GameManager for score
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			GD.Print("Enemy: Calling GameManager.OnEnemyDestroyed");
			gameManager.OnEnemyDestroyed(ScoreValue);
		}
		else
		{
			GD.PrintErr("Enemy: GameManager autoload not found!");
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
